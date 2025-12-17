using UnityEngine;
using UnityEngine.InputSystem;

public class MotorcycleController : MonoBehaviour
{
    [Header("Motor")]
    public float motorTorque = 1500f;
    public float freioTorque = 3000f;
    public float steerAngle = 30f;

    [Header("Referências")]
    public WheelCollider rodaFrente;
    public WheelCollider rodaTras;
    
    private bool ativo = false;
    private PlayerControls controls;
    
    // Variáveis para guardar os valores do input
    private Vector2 driveInput;
    private bool isBraking;

    void Awake()
    {
        controls = new PlayerControls();

        // Conecta os inputs (Lê os valores do mapa 'Vehicle' que criamos no Passo 3)
        // Se você não criou o mapa 'Vehicle', pode usar 'Player.Move' temporariamente
        // Mas recomendo fortemente criar o mapa separado.
        
        // Lê o Vector2 (WASD) sempre que mudar
        controls.Vehicle.Drive.performed += ctx => driveInput = ctx.ReadValue<Vector2>();
        controls.Vehicle.Drive.canceled += ctx => driveInput = Vector2.zero;

        // Lê o Freio (Espaço)
        controls.Vehicle.Brake.performed += ctx => isBraking = true;
        controls.Vehicle.Brake.canceled += ctx => isBraking = false;
    }

    // Ativa/Desativa inputs junto com o script
    void OnEnable() { controls.Vehicle.Enable(); }
    void OnDisable() { controls.Vehicle.Disable(); }

    public void SetAtivo(bool estado) 
    { 
        ativo = estado; 
        
        if (!ativo)
        {
            // Zera tudo ao sair
            rodaTras.motorTorque = 0;
            rodaFrente.steerAngle = 0;
            rodaTras.brakeTorque = 100f; // Freio de estacionamento
            
            // Importante: Desativar os inputs do veículo para não conflitar com o player a pé
            controls.Vehicle.Disable();
        }
        else
        {
            // Ativa os inputs do veículo
            controls.Vehicle.Enable();
        }
    }

    void FixedUpdate()
    {
        if (!ativo) return;

        // --- LÓGICA DE CONTROLE ---

        // Aceleração (W/S -> Eixo Y do Vector2)
        // driveInput.y vai de -1 a 1
        rodaTras.motorTorque = driveInput.y * motorTorque;

        // Curva (A/D -> Eixo X do Vector2)
        rodaFrente.steerAngle = driveInput.x * steerAngle;

        // Freio
        float forcaAtual = isBraking ? freioTorque : 0f;
        
        // Opcional: Se estiver acelerando para trás, não freia a frente (Ré)
        if (driveInput.y < 0) forcaAtual = 0;

        rodaFrente.brakeTorque = forcaAtual;
        rodaTras.brakeTorque = forcaAtual;
    }
}