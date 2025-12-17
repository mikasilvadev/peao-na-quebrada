using UnityEngine;

public class MotoPhysics : MonoBehaviour
{
    [Header("Configurações de Queda")]
    public float anguloQueda = 60f; // Passou disso, caiu
    public float distanciaChassi = 0.8f; // Ajuste no Gizmo Vermelho
    public LayerMask layerChao;

    [Header("Estabilidade (Arcade)")]
    public float forcaEstabilidade = 50f; // Quanto a moto tenta ficar em pé sozinha

    [Header("Referências")]
    public Rigidbody rb;
    public WheelCollider[] rodas;
    public Collider[] protetoresLaterais; // Arraste suas esferas laterais aqui
    public MotorcycleController controllerScript; // Arraste o script aqui no Inspector

    private bool estaTombada = false;
    private float[] suspensaoOriginal;
    private bool temPiloto = false;

    void Start()
    {
        // Salva suspensão original
        suspensaoOriginal = new float[rodas.Length];
        for (int i = 0; i < rodas.Length; i++) suspensaoOriginal[i] = rodas[i].suspensionDistance;
    }

    void FixedUpdate()
    {
        // 1. Detecta Estado
        float angulo = Vector3.Angle(transform.up, Vector3.up);
        bool noChao = Physics.Raycast(transform.position, Vector3.down, distanciaChassi, layerChao);

        // 2. Lógica de Queda
        if (angulo > anguloQueda && noChao)
        {
            if (!estaTombada) AtivarModoTombado();
        }
        else
        {
            // Recupera se levantar
            if (estaTombada && angulo < (anguloQueda - 10)) RestaurarModoVeiculo();
        }

        // 3. Estabilidade Artificial (Só aplica se tiver piloto OU se a moto estiver quase parando)
        if (!estaTombada && temPiloto)
        {
            AplicarEstabilidade();
        }
    }

    void AtivarModoTombado()
    {
        estaTombada = true;

        // 1. MATA O CONTROLE (O Player não pode mais acelerar)
        if (controllerScript) controllerScript.enabled = false;

        // 2. Corta suspensão e Trava Freio
        foreach (var r in rodas)
        {
            r.suspensionDistance = 0f;
            r.motorTorque = 0f;            // Zera força do motor
            r.brakeTorque = float.MaxValue; // Trava roda (Freio de mão infinito)
        }

        // 3. Vira um peso morto
        rb.linearDamping = 1.0f; // Aumentei um pouco pra parar mais rápido
        rb.angularDamping = 3f;
    }

    void RestaurarModoVeiculo()
    {
        estaTombada = false;

        // 1. DEVOLVE O CONTROLE (Se tiver piloto montado)
        if (controllerScript && temPiloto) controllerScript.enabled = true;

        // 2. Restaura rodas
        for (int i = 0; i < rodas.Length; i++)
        {
            rodas[i].suspensionDistance = suspensaoOriginal[i];
            rodas[i].brakeTorque = 0f;
        }

        rb.linearDamping = 0.05f;
        rb.angularDamping = 0.05f;
    }

    void AplicarEstabilidade()
    {
        // Um pequeno empurrão para manter a moto em pé (opcional, estilo GTA)
        // Se quiser realismo total (cair sozinha), deixe forcaEstabilidade = 0
        Vector3 predictedUp = Quaternion.AngleAxis(rb.angularVelocity.magnitude * Mathf.Rad2Deg * forcaEstabilidade / forcaEstabilidade, rb.angularVelocity) * transform.up;
        Vector3 torqueVector = Vector3.Cross(predictedUp, Vector3.up);
        rb.AddTorque(torqueVector * forcaEstabilidade * forcaEstabilidade);
    }

    public void SetPiloto(bool ativo)
    {
        temPiloto = ativo;
        // Se saiu da moto, destrava a rotação pra ela poder cair
        rb.constraints = RigidbodyConstraints.None;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * distanciaChassi);
    }
}