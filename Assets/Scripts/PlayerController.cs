using UnityEngine;

/// <summary>
/// Controla o movimento do jogador e aplica efeitos do clima na velocidade
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movimento")]
    public float baseSpeed = 5f;

    private float speedMultiplier = 1f;

    void Update()
    {
        HandleMovement();
    }

    /// <summary>
    /// Captura input e move o jogador
    /// </summary>
    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveX, 0, moveZ);

        transform.Translate(movement * baseSpeed * speedMultiplier * Time.deltaTime);
    }

    /// <summary>
    /// Ajusta a velocidade do jogador com base no clima
    /// </summary>
    public void SetWeather(string weather)
    {
        switch (weather.ToLower())
        {
            case "sunny":
                speedMultiplier = 1f;
                break;

            case "clouded":
            case "foggy":
                speedMultiplier = 0.8f;
                break;

            case "light rain":
                speedMultiplier = 0.6f;
                break;

            case "heavy rain":
                speedMultiplier = 0.4f;
                break;

            default:
                speedMultiplier = 1f;
                Debug.LogWarning("Clima desconhecido: " + weather);
                break;
        }
    }
}