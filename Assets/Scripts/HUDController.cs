using TMPro;
using UnityEngine;

/// <summary>
/// Gerencia a exibição do HUD:
/// - Informações de gameplay (clima e tempo)
/// - Mensagens do sistema (countdown, vitória, game over)
/// - Mensagens de evento (mudanças durante o jogo)
/// </summary>
public class HUDController : MonoBehaviour
{
    [Header("Referências")]
    public TextMeshProUGUI hudText;

    // Estado atual do HUD
    private string currentWeather = "";
    private float currentTime = 0f;
    private string message = "";

    // Controle de exibição
    private bool showOnlyMessage = false;

    /// <summary>
    /// Atualiza os dados principais exibidos no HUD (clima e tempo)
    /// </summary>
    public void UpdateHUD(string weather, float timeRemaining)
    {
        currentWeather = weather;
        currentTime = timeRemaining;

        Refresh();
    }

    /// <summary>
    /// Exibe apenas uma mensagem (usado para countdown, vitória e game over)
    /// </summary>
    public void ShowFullMessage(string msg)
    {
        message = msg;
        showOnlyMessage = true;

        Refresh();
    }

    /// <summary>
    /// Exibe mensagem junto com o HUD (ex: mudança de clima)
    /// </summary>
    public void ShowOverlayMessage(string msg)
    {
        message = msg;
        showOnlyMessage = false;

        Refresh();
    }

    /// <summary>
    /// Retorna o HUD ao modo normal de gameplay
    /// </summary>
    public void ShowGameplay()
    {
        showOnlyMessage = false;
        message = "";

        Refresh();
    }

    /// <summary>
    /// Atualiza o texto exibido no HUD
    /// </summary>
    private void Refresh()
    {
        if (hudText == null) return;

        if (showOnlyMessage)
        {
            // Apenas mensagem (sem clima/tempo)
            hudText.text = message;
        }
        else
        {
            // HUD completo + mensagem
            hudText.text =
                $"Clima: {currentWeather}\n" +
                $"Tempo restante: {currentTime:F1}s\n\n" +
                message;
        }
    }
}