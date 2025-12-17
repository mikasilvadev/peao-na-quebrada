using UnityEngine;
using Game.Network;

namespace Game.Player.Movement
{
    [RequireComponent(typeof(PlayerInputManager))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Configurações")]
        [SerializeField] private MovementSettings settings;

        [Header("Referências Visuais")]
        [SerializeField] private Transform meshObject;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private CapsuleCollider playerCollider;
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float groundCheckRadius = 0.4f;

        public bool RotateBodyWithCamera { get; set; } = false;

        public Rigidbody rb { get; private set; }
        public PlayerInputManager InputManager { get; private set; }
        public MovementSettings Settings => settings;
        public FrameInput CurrentInput { get; private set; }
        public PlayerMovementState CurrentState { get; private set; }
        public float MoveMagnitude { get; private set; }
        public bool isGrounded { get; private set; }

        private PlayerBaseState currentStateLogic;
        public readonly PlayerIdleState IdleState = new PlayerIdleState();
        public readonly PlayerWalkingState WalkingState = new PlayerWalkingState();
        public readonly PlayerRunningState RunningState = new PlayerRunningState();
        public readonly PlayerInAirState InAirState = new PlayerInAirState();
        public readonly PlayerCrouchState CrouchState = new PlayerCrouchState();

        private float lastJumpTime;
        private float defaultColliderHeight;
        private Vector3 defaultColliderCenter;
        private Vector3 defaultMeshScale;
        private Vector3 defaultMeshPos; // Nova variável para guardar posição original

        void OnEnable()
        {
            // Se o RB sumiu (foi destruído na moto), tenta pegar o novo
            if (rb == null) rb = GetComponent<Rigidbody>();
        }

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            InputManager = GetComponent<PlayerInputManager>();

            if (playerCollider == null) playerCollider = GetComponent<CapsuleCollider>();
            
            // Salva valores originais
            defaultColliderHeight = playerCollider.height;
            defaultColliderCenter = playerCollider.center;
            
            if (meshObject != null)
            {
                defaultMeshScale = meshObject.localScale;
                defaultMeshPos = meshObject.localPosition;
            }

            ChangeState(IdleState);
        }

        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            CurrentInput = GatherInput();

            if (Time.time < lastJumpTime + 0.2f)
            {
                isGrounded = false;
            }
            else
            {
                isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);
            }

            currentStateLogic.UpdateState(this);
        }

        private FrameInput GatherInput()
        {
            MoveMagnitude = InputManager.MoveInput.magnitude;
            return new FrameInput
            {
                MoveDirection = InputManager.MoveInput,
                LookDirection = InputManager.LookInput,
                RunHeld = InputManager.IsRunning,
                CrouchHeld = InputManager.IsCrouching,
                JumpDown = InputManager.JumpRequest
            };
        }

        void FixedUpdate()
        {
            currentStateLogic.FixedUpdateState(this);
            HandleGravity();
            ApplyFriction(); // Nova função para parar deslizamento
        }

        private void HandleGravity()
        {
            if (isGrounded && CurrentState != PlayerMovementState.InAir)
            {
                rb.AddForce(Vector3.down * settings.groundStickForce, ForceMode.Acceleration);
                // Limita velocidade vertical negativa para não atravessar chão
                if (rb.linearVelocity.y < -2f)
                {
                    // Não zera totalmente para manter contato, mas limita
                }
            }
            else
            {
                Vector3 gravity = rb.linearVelocity.y < 0 
                    ? Vector3.up * Physics.gravity.y * (settings.fallGravityScale - 1f)
                    : Vector3.up * Physics.gravity.y * (settings.gravityScale - 1f);
                rb.AddForce(gravity, ForceMode.Acceleration);
            }
        }

        // --- SOLUÇÃO DO DESLIZAMENTO ---
        private void ApplyFriction()
        {
            // Se estiver no chão e não houver input de movimento (ou input muito fraco)
            if (isGrounded && CurrentInput.MoveDirection.sqrMagnitude < 0.01f)
            {
                // Aumenta drasticamente o drag para parar na hora
                // Mas apenas nos eixos X e Z para não afetar a gravidade (Y)
                Vector3 vel = rb.linearVelocity;
                vel.x *= 0.8f; // Reduz 20% da velocidade por frame (freio rápido)
                vel.z *= 0.8f;
                rb.linearVelocity = vel;
            }
        }

        public void ChangeState(PlayerBaseState newState)
        {
            if (currentStateLogic != null) currentStateLogic.ExitState(this);
            currentStateLogic = newState;
            CurrentState = newState.StateEnum;
            newState.EnterState(this);
        }

        // --- SISTEMA DE AGACHAMENTO CORRIGIDO (SEM FLUTUAR) ---
        public void SetCrouchState(bool isCrouching)
        {
            if (isCrouching)
            {
                // 1. Altura reduzida
                float targetHeight = defaultColliderHeight * settings.crouchHeightRatio;
                playerCollider.height = targetHeight;
                
                // 2. Cálculo preciso do centro para manter os pés no chão
                // A diferença de altura dividida por 2 é quanto o centro encolheu pra cima e pra baixo.
                // Precisamos baixar o centro exatamente essa metade para o pé continuar no zero.
                float heightDifference = defaultColliderHeight - targetHeight;
                float newCenterY = defaultColliderCenter.y - (heightDifference * 0.5f);
                
                playerCollider.center = new Vector3(defaultColliderCenter.x, newCenterY, defaultColliderCenter.z);

                // 3. Ajuste Visual
                if (meshObject != null)
                {
                    Vector3 newScale = defaultMeshScale;
                    newScale.y = defaultMeshScale.y * settings.crouchHeightRatio;
                    meshObject.localScale = newScale;

                    // IMPORTANTE: Se o pivô da mesh for no centro, ela também sobe.
                    // Precisamos baixá-la visualmente também.
                    // Se o pivô for no pé, isso não é necessário (mas mal não faz se for 0).
                    Vector3 newPos = defaultMeshPos;
                    newPos.y = defaultMeshPos.y - (heightDifference * 0.5f);
                    meshObject.localPosition = newPos;
                }
            }
            else
            {
                // Volta tudo ao original
                playerCollider.height = defaultColliderHeight;
                playerCollider.center = defaultColliderCenter;

                if (meshObject != null)
                {
                    meshObject.localScale = defaultMeshScale;
                    meshObject.localPosition = defaultMeshPos;
                }
            }
        }

        public void HandleMovement()
        {
            Vector2 input = CurrentInput.MoveDirection;
            Vector3 desiredMoveDirection = Vector3.zero;

            if (input.sqrMagnitude > 0.01f)
            {
                Transform camTransform = Camera.main.transform;
                Vector3 camForward = camTransform.forward;
                Vector3 camRight = camTransform.right;
                camForward.y = 0;
                camRight.y = 0;
                desiredMoveDirection = (camForward.normalized * input.y + camRight.normalized * input.x).normalized;
            }

            if (CurrentState == PlayerMovementState.InAir)
            {
                if (input.sqrMagnitude > 0.01f) 
                    rb.AddForce(desiredMoveDirection * settings.walkingSpeed * settings.airControlPercentage, ForceMode.Force);

                Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
                if (flatVel.magnitude > settings.runningSpeed)
                {
                    Vector3 capped = flatVel.normalized * settings.runningSpeed;
                    rb.linearVelocity = new Vector3(capped.x, rb.linearVelocity.y, capped.z);
                }
                return;
            }

            float currentSpeed = 0;
            switch (CurrentState)
            {
                case PlayerMovementState.Running: currentSpeed = Settings.runningSpeed; break;
                case PlayerMovementState.Crouching: currentSpeed = Settings.crouchingSpeed; break;
                case PlayerMovementState.Walking: currentSpeed = Settings.walkingSpeed; break;
            }

            if (isGrounded)
            {
                Vector3 targetVelocity = desiredMoveDirection * currentSpeed;
                
                // Se houver input, aplica velocidade. Se não, o ApplyFriction cuida de frear.
                if (targetVelocity.magnitude > 0.1f)
                {
                    // Mantém a velocidade Y original (gravidade) mas muda X e Z
                    rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
                }
            }
        }

        public void HandleRotation()
        {
            if (RotateBodyWithCamera)
            {
                Transform camTransform = Camera.main.transform;
                Vector3 targetDirection = camTransform.forward;
                targetDirection.y = 0; 
                
                if (targetDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                    rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, settings.rotationSmoothing * 2f * Time.fixedDeltaTime));
                }
                return;
            }

            Vector2 input = CurrentInput.MoveDirection;
            if (input.sqrMagnitude < 0.01f) return;

            Transform camTransformTPC = Camera.main.transform;
            Vector3 camForward = camTransformTPC.forward;
            Vector3 camRight = camTransformTPC.right;
            camForward.y = 0;
            camRight.y = 0;

            Vector3 targetDir = (camForward.normalized * input.y + camRight.normalized * input.x).normalized;

            if (targetDir != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDir);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, settings.rotationSmoothing * Time.fixedDeltaTime));
            }
        }

        public void HandleJump()
        {
            Vector3 currentXZVelocity = rb.linearVelocity;
            currentXZVelocity.y = 0;
            Vector3 finalXZVelocity = Vector3.ClampMagnitude(currentXZVelocity, Settings.runningSpeed);
            
            rb.linearVelocity = new Vector3(finalXZVelocity.x, 0, finalXZVelocity.z);
            rb.AddForce(Vector3.up * settings.jumpForce, ForceMode.Impulse);

            InputManager.ConsumeJumpRequest();
            lastJumpTime = Time.time;
        }
    }
}