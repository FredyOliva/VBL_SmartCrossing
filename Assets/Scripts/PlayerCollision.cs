using UnityEngine;

/// <summary>
/// Detecta colisões do jogador com carros e dispara Game Over
/// </summary>
public class PlayerCollision : MonoBehaviour
{
    [Header("Referências")]
    public TrafficManager trafficManager;

    private void OnCollisionEnter(Collision collision)
    {
        if (trafficManager == null) return;

        // Verifica se colidiu com um carro
        if (collision.gameObject.CompareTag("Car"))
        {
            trafficManager.GameOver("ATROPELADO");
        }
    }
}