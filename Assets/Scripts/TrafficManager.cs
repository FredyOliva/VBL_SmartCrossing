using System.Collections;
using UnityEngine;

/// <summary>
/// Gerencia o fluxo principal do jogo:
/// - Carregamento de dados (JSON)
/// - Aplicação do estado inicial
/// - Execução de predições ao longo do tempo
/// - Controle de HUD, vitória e game over
/// - Ajuste visual do ambiente conforme clima (com transição suave)
/// </summary>
public class TrafficManager : MonoBehaviour
{
    [Header("Referências")]
    public CarSpawner carSpawner;
    public PlayerController playerController;
    public HUDController hud;
    public Transform player;

    [Header("Iluminação")]
    public Light sceneLight;

    [Header("Chuva")]
    public ParticleSystem rain;

    [Header("Configuração de Gameplay")]
    public float winZ = 4f;
    public float extraTimeAfterLastPrediction = 8f;

    // Dados da API
    private TrafficResponse trafficData;

    // Controle de tempo
    private float totalTime;
    private float startTime;

    // Estado do jogo
    private bool gameEnded = false;
    private bool isGameStarted = false;

    // Controle da transição de iluminação
    private Coroutine lightingCoroutine;

    void Start()
    {
        LoadTrafficData();
        ApplyCurrentStatus();

        // Define tempo total do jogo
        if (trafficData != null && trafficData.predicted_status.Count > 0)
        {
            var lastPrediction = trafficData.predicted_status[trafficData.predicted_status.Count - 1];
            totalTime = (lastPrediction.estimated_time / 1000f) + extraTimeAfterLastPrediction;
        }

        StartCoroutine(HandlePredictions());
        StartCoroutine(StartCountdown());
    }

    void Update()
    {
        if (!isGameStarted) return;

        if (trafficData == null || hud == null) return;

        float elapsed = Time.time - startTime;
        float remaining = Mathf.Max(0, totalTime - elapsed);

        hud.UpdateHUD(
            trafficData.current_status.weather,
            remaining
        );

        if (gameEnded) return;

        if (remaining <= 0)
        {
            GameOver("Tempo esgotado");
            return;
        }

        if (player != null && player.position.z >= winZ)
        {
            Win();
        }
    }

    void LoadTrafficData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("traffic");

        if (jsonFile == null)
        {
            Debug.LogError("Arquivo JSON não encontrado!");
            return;
        }

        trafficData = JsonUtility.FromJson<TrafficResponse>(jsonFile.text);

        Debug.Log("JSON carregado com sucesso!");
        Debug.Log("Clima atual: " + trafficData.current_status.weather);
    }

    void ApplyCurrentStatus()
    {
        if (trafficData == null) return;

        carSpawner.vehicleDensity = trafficData.current_status.vehicleDensity;
        carSpawner.baseSpeed = 10f;
        carSpawner.averageSpeedFromAPI = trafficData.current_status.averageSpeed;

        playerController.SetWeather(trafficData.current_status.weather);

        ApplyLighting(trafficData.current_status.weather);
        ApplyRain(trafficData.current_status.weather);

        Debug.Log("Dados iniciais aplicados.");
    }

    void ApplyPrediction(Prediction prediction)
    {
        carSpawner.vehicleDensity = prediction.predictions.vehicleDensity;
        carSpawner.averageSpeedFromAPI = prediction.predictions.averageSpeed;

        playerController.SetWeather(prediction.predictions.weather);

        trafficData.current_status.weather = prediction.predictions.weather;

        ApplyLighting(prediction.predictions.weather);
        ApplyRain(trafficData.current_status.weather);

        hud.ShowOverlayMessage("Mudança: " + prediction.predictions.weather);

        Debug.Log($"[{Time.time:F1}s] Novo clima: {prediction.predictions.weather}");
    }

    /// <summary>
    /// Controla a transição de iluminação garantindo apenas uma coroutine ativa
    /// </summary>
    void ApplyLighting(string weather)
    {
        if (sceneLight == null) return;

        if (lightingCoroutine != null)
        {
            StopCoroutine(lightingCoroutine);
        }

        lightingCoroutine = StartCoroutine(LerpLighting(weather));
    }

    /// <summary>
    /// Controla a chuva
    /// </summary>
    void ApplyRain(string weather)
    {
        if (rain == null) return;

        if (weather == "light rain" || weather == "heavy rain")
        {
            if (!rain.isPlaying)
                rain.Play();
        }
        else
        {
            if (rain.isPlaying)
                rain.Stop();
        }
    }

    /// <summary>
    /// Realiza interpolação suave de luz e fog
    /// </summary>
    IEnumerator LerpLighting(string weather)
    {
        float duration = 1.5f;
        float time = 0f;

        float startIntensity = sceneLight.intensity;
        Color startColor = sceneLight.color;

        float startFogDensity = RenderSettings.fogDensity;
        bool startFog = RenderSettings.fog;

        float targetIntensity = startIntensity;
        Color targetColor = startColor;

        bool targetFog = false;
        float targetFogDensity = 0f;

        switch (weather)
        {
            case "sunny":
                targetIntensity = 1.2f;
                targetColor = Color.white;
                targetFog = false;
                break;

            case "clouded":
                targetIntensity = 0.9f;
                targetColor = Color.gray;
                targetFog = false;
                break;

            case "foggy":
                targetIntensity = 0.6f;
                targetColor = Color.gray;
                targetFog = true;
                targetFogDensity = 0.05f;
                break;

            case "light rain":
                targetIntensity = 0.7f;
                targetColor = new Color(0.7f, 0.7f, 0.8f);
                targetFog = false;
                break;

            case "heavy rain":
                targetIntensity = 0.4f;
                targetColor = new Color(0.5f, 0.5f, 0.6f);
                targetFog = true;
                targetFogDensity = 0.08f;
                break;
        }

        // Garante que o fog esteja ativo durante a transição
        if (targetFog || startFog)
        {
            RenderSettings.fog = true;
        }

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            sceneLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, t);
            sceneLight.color = Color.Lerp(startColor, targetColor, t);

            RenderSettings.fogDensity = Mathf.Lerp(startFogDensity, targetFogDensity, t);

            yield return null;
        }

        sceneLight.intensity = targetIntensity;
        sceneLight.color = targetColor;

        RenderSettings.fogDensity = targetFogDensity;
        RenderSettings.fog = targetFog;

        lightingCoroutine = null;
    }

    public void GameOver(string reason)
    {
        if (gameEnded) return;

        gameEnded = true;

        hud.ShowFullMessage("GAME OVER\n" + reason);
        Debug.Log("Game Over: " + reason);

        Time.timeScale = 0f;
    }

    void Win()
    {
        if (gameEnded) return;

        gameEnded = true;

        hud.ShowFullMessage("VITÓRIA!");
        Debug.Log("Vitória!");

        Time.timeScale = 0f;
    }

    IEnumerator HandlePredictions()
    {
        if (trafficData == null) yield break;

        float elapsedTime = 0f;

        foreach (var prediction in trafficData.predicted_status)
        {
            float targetTime = prediction.estimated_time / 1000f;
            float waitTime = targetTime - elapsedTime;

            yield return new WaitForSeconds(waitTime);

            ApplyPrediction(prediction);

            elapsedTime = targetTime;
        }
    }

    IEnumerator StartCountdown()
    {
        Time.timeScale = 0f;

        hud.ShowFullMessage("O JOGO VAI COMEÇAR!");
        yield return new WaitForSecondsRealtime(1f);

        hud.ShowFullMessage("3");
        yield return new WaitForSecondsRealtime(1f);

        hud.ShowFullMessage("2");
        yield return new WaitForSecondsRealtime(1f);

        hud.ShowFullMessage("1");
        yield return new WaitForSecondsRealtime(1f);

        hud.ShowFullMessage("VAI!");
        yield return new WaitForSecondsRealtime(1f);

        Time.timeScale = 1f;

        hud.ShowGameplay();

        startTime = Time.time;
        isGameStarted = true;
    }
}