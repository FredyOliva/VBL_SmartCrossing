using System;
using System.Collections.Generic;

/// <summary>
/// Representa a resposta completa da API de tráfego
/// </summary>
[Serializable]
public class TrafficResponse
{
    public Status current_status;
    public List<Prediction> predicted_status;
}

/// <summary>
/// Representa o estado atual ou previsto do tráfego
/// </summary>
[Serializable]
public class Status
{
    public float vehicleDensity;
    public float averageSpeed;
    public string weather;
}

/// <summary>
/// Representa uma predição futura baseada em tempo
/// </summary>
[Serializable]
public class Prediction
{
    /// <summary>
    /// Tempo estimado em milissegundos para aplicação da predição
    /// </summary>
    public int estimated_time;

    /// <summary>
    /// Dados previstos para esse momento
    /// </summary>
    public Status predictions;
}