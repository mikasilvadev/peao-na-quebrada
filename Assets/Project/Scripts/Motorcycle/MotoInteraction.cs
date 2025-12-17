using UnityEngine;
using UnityEngine.InputSystem;
using Game.Player.Movement;

public class MotoInteraction : MonoBehaviour
{
    [Header("Configuração")]
    public Transform seatPoint; 
    public Transform exitPoint; 
    
    // Não precisamos mais arrastar o player manualmente
    private GameObject currentPlayer; 

    [Header("Scripts da Moto")]
    public MotoPhysics physicsScript;
    public MotorcycleController controllerScript;

    private bool montado = false;
    private Transform playerOriginalParent;
    private PlayerControls controls;

    // Cache dos componentes para restaurar depois
    private PlayerMovement playerMove;
    
    // CORREÇÃO AQUI: Especificamos o caminho completo para não confundir com o da Unity
    private Game.Player.Movement.PlayerInputManager playerInput;
    
    private Rigidbody playerRB;
    private CapsuleCollider playerCol;

    void Awake()
    {
        controls = new PlayerControls();
        controls.Player.Interact.started += ctx => TentarInteragir();
    }

    void OnEnable() { controls.Player.Enable(); }
    void OnDisable() { controls.Player.Disable(); }

    void OnTriggerEnter(Collider other)
    {
        if (montado) return; 

        if (other.CompareTag("Player"))
        {
            currentPlayer = other.gameObject;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (montado) return;

        if (other.CompareTag("Player") && other.gameObject == currentPlayer)
        {
            currentPlayer = null;
        }
    }

    void TentarInteragir()
    {
        if (montado)
        {
            Desmontar();
        }
        else if (currentPlayer != null)
        {
            Montar(currentPlayer);
        }
    }

    void Montar(GameObject playerRef)
    {
        // 1. Coletar Componentes
        playerMove = playerRef.GetComponent<PlayerMovement>();
        
        // CORREÇÃO AQUI TAMBÉM: Usamos o caminho completo no GetComponent
        playerInput = playerRef.GetComponent<Game.Player.Movement.PlayerInputManager>();
        
        playerRB = playerRef.GetComponent<Rigidbody>();
        playerCol = playerRef.GetComponent<CapsuleCollider>();

        montado = true;

        // 2. DESLIGAR TUDO NO PLAYER (Cérebro e Física)
        if (playerMove) playerMove.enabled = false;
        if (playerInput) playerInput.enabled = false;
        
        // Rigidbody: Vira Kinematic para não brigar com a moto
        if (playerRB)
        {
            playerRB.isKinematic = true; 
            playerRB.detectCollisions = false; 
            // Zera qualquer velocidade residual
            playerRB.linearVelocity = Vector3.zero;
            playerRB.angularVelocity = Vector3.zero;
            playerRB.interpolation = RigidbodyInterpolation.None; 
        }

        if (playerCol) playerCol.enabled = false;

        // 3. GRUDAR NA MOTO (Parenteamento)
        playerOriginalParent = playerRef.transform.parent;
        playerRef.transform.SetParent(seatPoint);

        // 4. POSICIONAR PERFEITAMENTE
        playerRef.transform.localPosition = Vector3.zero;
        playerRef.transform.localRotation = Quaternion.identity;

        // 5. ATIVAR CONTROLE DA MOTO
        if (physicsScript) physicsScript.SetPiloto(true);
        if (controllerScript) controllerScript.SetAtivo(true);

        Debug.Log("Player Montado (Modo Real)");
    }

    void Desmontar()
    {
        if (currentPlayer == null) return;

        montado = false;

        // 1. DESLIGAR CONTROLE DA MOTO
        if (physicsScript) physicsScript.SetPiloto(false);
        if (controllerScript) controllerScript.SetAtivo(false);

        // 2. SOLTAR O PLAYER
        currentPlayer.transform.SetParent(playerOriginalParent);

        // 3. POSICIONAR NO PONTO DE SAÍDA
        if (exitPoint != null)
        {
            currentPlayer.transform.position = exitPoint.position;
            currentPlayer.transform.rotation = exitPoint.rotation;
        }
        else
        {
            currentPlayer.transform.position = transform.position + (-transform.right * 1.5f);
            currentPlayer.transform.rotation = Quaternion.LookRotation(transform.forward);
        }

        // 4. RESTAURAR PLAYER
        if (playerRB)
        {
            playerRB.isKinematic = false;
            playerRB.detectCollisions = true;
            playerRB.interpolation = RigidbodyInterpolation.Interpolate; 
        }
        
        if (playerCol) playerCol.enabled = true;
        if (playerMove) playerMove.enabled = true;
        if (playerInput) playerInput.enabled = true;

        currentPlayer = null; 
        Debug.Log("Player Desmontou");
    }
}