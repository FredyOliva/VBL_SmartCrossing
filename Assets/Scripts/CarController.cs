using UnityEngine;

/// <summary>
/// Responsável pelo movimento do carro e sua remoção ao sair da tela
/// </summary>
public class CarController : MonoBehaviour
{
    [Header("Movimento")]
    public float speed = 5f;
    public int direction = 1; // 1 = esquerda → direita | -1 = direita → esquerda

    [Header("Limites")]
    public float destroyLimitX = 15f;

    void Update()
    {
        Move();
        CheckBounds();
    }

    /// <summary>
    /// Move o carro na direção definida
    /// </summary>
    void Move()
    {
        transform.Translate(Vector3.right * speed * direction * Time.deltaTime);
    }

    /// <summary>
    /// Destroi o carro ao sair da área visível
    /// </summary>
    void CheckBounds()
    {
        if (direction == 1 && transform.position.x > destroyLimitX)
        {
            Destroy(gameObject);
        }
        else if (direction == -1 && transform.position.x < -destroyLimitX)
        {
            Destroy(gameObject);
        }
    }
}