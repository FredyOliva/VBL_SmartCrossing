using UnityEngine;

/// <summary>
/// Responsável por gerar os carros nas lanes
/// com base na densidade de veículos e velocidade da API
/// </summary>
public class CarSpawner : MonoBehaviour
{
    [Header("Referências")]
    public GameObject carPrefab;
    public LaneData[] lanes;

    [Header("Configuração de Tráfego")]
    [Range(0.1f, 1f)]
    public float vehicleDensity = 0.5f;

    public float baseSpeed = 10f;
    public float averageSpeedFromAPI = 50f;

    private float spawnTimer = 0f;

    void Update()
    {
        if (carPrefab == null || lanes.Length == 0) return;

        // Garante que a densidade está dentro do limite
        vehicleDensity = Mathf.Clamp(vehicleDensity, 0.1f, 1f);

        float spawnInterval = 1f / vehicleDensity;

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            SpawnCar();
            spawnTimer = 0f;
        }
    }

    /// <summary>
    /// Instancia um carro em uma lane aleatória
    /// </summary>
    void SpawnCar()
    {
        LaneData lane = lanes[Random.Range(0, lanes.Length)];

        float laneZ = lane.laneTransform.position.z;
        int direction = lane.direction;

        // Ajusta altura baseado no tamanho do prefab
        float y = carPrefab.transform.localScale.y / 2f;

        // Define lado de spawn baseado na direção
        float spawnX = direction == 1 ? -25f : 25f;

        GameObject car = Instantiate(
            carPrefab,
            new Vector3(spawnX, y, laneZ),
            Quaternion.identity
        );

        // Velocidade baseada na API
        float speed = (averageSpeedFromAPI / 100f) * baseSpeed;

        // Aplica comportamento no carro
        CarController controller = car.GetComponent<CarController>();
        if (controller != null)
        {
            controller.speed = speed;
            controller.direction = direction;
        }
    }

    /// <summary>
    /// Representa uma lane com posição e direção
    /// </summary>
    [System.Serializable]
    public class LaneData
    {
        public Transform laneTransform;
        public int direction; // 1 = esquerda → direita | -1 = direita → esquerda
    }
}