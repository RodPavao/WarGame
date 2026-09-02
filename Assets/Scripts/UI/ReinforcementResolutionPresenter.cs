using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class ReinforcementResolutionPresenter : MonoBehaviour,
    IResolutionVisualPresenter
{
    // ============================================================
    // 01. CONFIGURAÇÃO E SUPORTE
    // ============================================================

    private WarDominionUITheme theme;

    public void Configure(WarDominionUITheme newTheme) => theme = newTheme;

    public bool Supports(ResolutionVisualEventType type) =>
        type == ResolutionVisualEventType.Reinforcement;

    // ============================================================
    // 02. CHEGADA AMIGÁVEL E ATUALIZAÇÃO DO CONTADOR
    // ============================================================

    public IEnumerator Present(
        ResolutionVisualEvent visualEvent,
        ResolutionVisualPresentationContext context)
    {
        Debug.Log(
            $"[ReinforcementResolution][DEBUG] Presenter de reforço acionado: " +
            $"{visualEvent}");
        if (theme == null || visualEvent == null || context == null ||
            !ResolutionVisualMapUtility.TryGetTerritory(
                visualEvent.DestinationTerritoryId, out TerritorioClique territory) ||
            !ResolutionVisualMapUtility.TryGetPosition(territory, context, out Vector2 position))
        {
            Debug.LogWarning(
                $"[ResolutionVisual] Reforço inválido ou território ausente: {visualEvent}");
            yield break;
        }

        Color color = context.GetPlayerColor(visualEvent.Player);
        RectTransform root = ResolutionVisualMapUtility.CreateRoot(
            "ReinforcementResolution", context.Overlay);
        Image pulse = ResolutionVisualMapUtility.CreateImage(
            "FriendlyPulse", root, color, theme.ReinforcementPulseOpacity);
        RectTransform pulseRect = (RectTransform)pulse.transform;
        pulseRect.anchoredPosition = position;
        pulseRect.sizeDelta = Vector2.one * theme.ReinforcementPulseSize;
        pulseRect.localEulerAngles = new Vector3(0f, 0f, 45f);

        TextMeshProUGUI indicator = ResolutionVisualMapUtility.CreateText(
            "Amount", root, theme.FonteInterface,
            theme.ReinforcementTextSize, color);
        indicator.text = $"+{visualEvent.Amount}";
        indicator.rectTransform.anchoredPosition =
            position + Vector2.up * theme.ReinforcementIndicatorOffset;

        yield return AnimateEntry(indicator, pulseRect, context);
        territory.AplicarEstadoVisualResolucao(
            visualEvent.DestinationAfter, visualEvent.NewOwner);
        yield return AnimateExit(indicator, pulse, context);
        Destroy(root.gameObject);
    }

    // ============================================================
    // 03. ESCALA E FADE CONTROLADOS PELO PLAYBACK
    // ============================================================

    private IEnumerator AnimateEntry(
        TextMeshProUGUI indicator,
        RectTransform pulse,
        ResolutionVisualPresentationContext context)
    {
        float elapsed = 0f;
        while (elapsed < theme.ReinforcementEntryDuration)
        {
            elapsed += context.PlaybackDeltaTime;
            float progress = Mathf.Clamp01(elapsed / theme.ReinforcementEntryDuration);
            indicator.alpha = progress;
            indicator.transform.localScale = Vector3.one * Mathf.Lerp(
                theme.ReinforcementInitialScale, 1f, progress);
            pulse.localScale = Vector3.one * Mathf.Lerp(0.65f, 1.1f, progress);
            yield return null;
        }

        yield return context.Wait(theme.ReinforcementReactionDuration);
    }

    private IEnumerator AnimateExit(
        TextMeshProUGUI indicator,
        Image pulse,
        ResolutionVisualPresentationContext context)
    {
        float elapsed = 0f;
        while (elapsed < theme.ReinforcementExitDuration)
        {
            elapsed += context.PlaybackDeltaTime;
            float progress = Mathf.Clamp01(elapsed / theme.ReinforcementExitDuration);
            indicator.alpha = 1f - progress;
            Color pulseColor = pulse.color;
            pulseColor.a = (1f - progress) * theme.ReinforcementPulseOpacity;
            pulse.color = pulseColor;
            yield return null;
        }
    }
}
