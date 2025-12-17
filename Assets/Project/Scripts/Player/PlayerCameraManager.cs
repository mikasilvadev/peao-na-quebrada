using UnityEngine;
using Unity.Cinemachine;
using Game.Player.Movement;
using UnityEngine.InputSystem;
using MyInputManager = Game.Player.Movement.PlayerInputManager;

namespace Game.Player.View
{
    public class PlayerCameraManager : MonoBehaviour
    {
        [Header("Referências")]
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private GameObject playerMeshObject;

        [Header("Câmeras")]
        [SerializeField] private CinemachineCamera tpcCamera;
        [SerializeField] private CinemachineCamera fpcCamera;

        [Header("Zoom TPC (Multiplicadores de Escala)")]
        // 1.0 = Mantém sua configuração original do Inspector.
        // 1.5 = Aumenta 50% (Mais longe).
        // 0.6 = Diminui (Mais perto).
        [SerializeField] private float[] zoomScales = new float[] { 1.5f, 1.0f, 0.6f };

        // Variáveis para guardar a configuração original de CADA anel
        // Usamos Cinemachine3OrbitRig.Orbit pois é o tipo correto no Unity 6
        private Cinemachine3OrbitRig.Orbit originalTop;
        private Cinemachine3OrbitRig.Orbit originalCenter;
        private Cinemachine3OrbitRig.Orbit originalBottom;
        private bool hasOriginals = false;

        // Estado interno
        private int currentStageIndex = 0; 
        private Renderer[] playerRenderers;
        private CinemachineOrbitalFollow tpcOrbital;

        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (playerMeshObject != null)
                playerRenderers = playerMeshObject.GetComponentsInChildren<Renderer>();

            if (tpcCamera != null)
            {
                tpcOrbital = tpcCamera.GetComponent<CinemachineOrbitalFollow>();
                
                // --- CORREÇÃO DO ERRO DE INDEXAÇÃO ---
                // Em vez de usar array [0], acessamos as propriedades .Top, .Center, .Bottom
                if (tpcOrbital != null)
                {
                    // Acessa a struct de configurações
                    var orbits = tpcOrbital.Orbits;
                    
                    originalTop = orbits.Top;
                    originalCenter = orbits.Center;
                    originalBottom = orbits.Bottom;
                    hasOriginals = true;
                }
            }

            // Começa no estágio 1 (Médio/Original)
            currentStageIndex = 1;
            ApplyCameraMode();
        }

        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.vKey.wasPressedThisFrame)
            {
                CycleCameraMode();
            }
        }

        private void CycleCameraMode()
        {
            currentStageIndex++;
            if (currentStageIndex > 3) currentStageIndex = 0;
            ApplyCameraMode();
        }

        private void ApplyCameraMode()
        {
            bool isFPC = (currentStageIndex == 3);

            if (isFPC)
            {
                // --- MODO FPC ---
                fpcCamera.Priority = 20;
                tpcCamera.Priority = 10;
                if (playerMovement != null) playerMovement.RotateBodyWithCamera = true;
                SetShadowsOnly(true);
            }
            else
            {
                // --- MODO TPC ---
                fpcCamera.Priority = 10;
                tpcCamera.Priority = 20;
                if (playerMovement != null) playerMovement.RotateBodyWithCamera = false;
                SetShadowsOnly(false);

                // --- APLICA O ZOOM PROPORCIONAL ---
                if (tpcOrbital != null && hasOriginals && currentStageIndex < zoomScales.Length)
                {
                    float scale = zoomScales[currentStageIndex];
                    
                    // Copia a struct atual para modificar
                    var currentOrbits = tpcOrbital.Orbits;

                    // Aplica a escala baseada nos originais salvos no Start
                    // Acessando pelos nomes corretos (.Top, .Center, .Bottom)
                    currentOrbits.Top.Radius = originalTop.Radius * scale;
                    currentOrbits.Center.Radius = originalCenter.Radius * scale;
                    currentOrbits.Bottom.Radius = originalBottom.Radius * scale;

                    // Devolve a struct modificada para o componente
                    tpcOrbital.Orbits = currentOrbits;
                }
            }
        }

        private void SetShadowsOnly(bool shadowsOnly)
        {
            if (playerRenderers == null) return;
            foreach (var r in playerRenderers)
            {
                r.shadowCastingMode = shadowsOnly 
                    ? UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly 
                    : UnityEngine.Rendering.ShadowCastingMode.On;
            }
        }
    }
}