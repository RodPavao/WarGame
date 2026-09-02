using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class AttackResolutionPresenter : MonoBehaviour,
    IResolutionVisualPresenter
{
    // ============================================================
    // 01. CONFIGURAÇÃO E ESTADO VISUAL TEMPORÁRIO
    // ============================================================

    private WarDominionUITheme theme;

    public void Configure(WarDominionUITheme newTheme)
    {
        theme = newTheme;
    }

    public bool Supports(ResolutionVisualEventType type) =>
        type == ResolutionVisualEventType.Attack;

    // ============================================================
    // 02. COREOGRAFIA DO ATAQUE JÁ CALCULADO
    // ============================================================

    public IEnumerator Present(
        ResolutionVisualEvent visualEvent,
        ResolutionVisualPresentationContext context)
    {
        Debug.Log(
            $"[AttackResolution][DEBUG] Presenter de ataque acionado para evento: " +
            $"{visualEvent}");
        if (theme == null || visualEvent == null || context == null ||
            !ResolutionVisualMapUtility.TryGetTerritory(
                visualEvent.OriginTerritoryId, out TerritorioClique origin) ||
            !ResolutionVisualMapUtility.TryGetTerritory(
                visualEvent.DestinationTerritoryId, out TerritorioClique destination) ||
            !ResolutionVisualMapUtility.TryGetPosition(
                origin, context, out Vector2 originPosition) ||
            !ResolutionVisualMapUtility.TryGetPosition(
                destination, context, out Vector2 destinationPosition))
        {
            yield break;
        }

        Color playerColor = context.GetPlayerColor(visualEvent.Player);
        RectTransform root = ResolutionVisualMapUtility.CreateRoot(
            "AttackResolution", context.Overlay);
        Image route = ResolutionVisualMapUtility.CreateImage(
            "Route", root, playerColor, 0.28f);
        Image originPulse = ResolutionVisualMapUtility.CreateImage(
            "OriginPulse", root, playerColor, 0.35f);
        Image destinationPulse = ResolutionVisualMapUtility.CreateImage(
            "DestinationPulse", root, playerColor, 0.28f);
        RectTransform attacker = CreateAttacker(root, playerColor, visualEvent.Amount);
        Image impact = ResolutionVisualMapUtility.CreateImage(
            "Impact", root, playerColor, 0f);
        TextMeshProUGUI result = CreateResult(root, playerColor);

        ResolutionVisualMapUtility.LayoutLine(
            (RectTransform)route.transform, originPosition, destinationPosition,
            theme.AttackRouteThickness);
        ConfigurePulse((RectTransform)originPulse.transform, originPosition);
        ConfigurePulse((RectTransform)destinationPulse.transform, destinationPosition);
        attacker.anchoredPosition = originPosition;
        RectTransform impactRect = (RectTransform)impact.transform;
        impactRect.anchoredPosition = destinationPosition;
        impactRect.sizeDelta = Vector2.one * theme.AttackPulseSize;
        result.rectTransform.anchoredPosition =
            destinationPosition + Vector2.up * theme.AttackResultOffset;

        float elapsed = 0f;
        while (elapsed < theme.AttackTravelDuration)
        {
            elapsed += context.PlaybackDeltaTime;
            float progress = Mathf.Clamp01(elapsed / theme.AttackTravelDuration);
            float smooth = progress * progress * (3f - 2f * progress);
            attacker.anchoredPosition = Vector2.Lerp(originPosition, destinationPosition, smooth);
            float pulse = (Mathf.Sin(progress * Mathf.PI * 4f) + 1f) * 0.5f;
            originPulse.transform.localScale = Vector3.one * Mathf.Lerp(0.85f, 1.12f, pulse);
            yield return null;
        }

        attacker.gameObject.SetActive(false);
        yield return AnimateImpact(impact, context);
        ApplyFinalVisualState(visualEvent);

        result.text = visualEvent.Conquered
            ? $"CONQUISTA  •  {visualEvent.DestinationAfter}"
            : $"DEFESA  •  {visualEvent.DestinationAfter}";
        result.alpha = 1f;
        yield return context.Wait(theme.AttackResultDuration);

        Destroy(root.gameObject);
    }

    // ============================================================
    // 04. IMPACTO E RESULTADO PROVISÓRIOS
    // ============================================================

    private IEnumerator AnimateImpact(
        Image impact,
        ResolutionVisualPresentationContext context)
    {
        RectTransform rect = (RectTransform)impact.transform;
        float elapsed = 0f;
        while (elapsed < theme.AttackImpactDuration)
        {
            elapsed += context.PlaybackDeltaTime;
            float progress = Mathf.Clamp01(elapsed / theme.AttackImpactDuration);
            rect.localScale = Vector3.one * Mathf.Lerp(0.35f, 1.65f, progress);
            Color color = impact.color;
            color.a = Mathf.Sin(progress * Mathf.PI) * theme.AttackImpactIntensity;
            impact.color = color;
            yield return null;
        }
    }

    private static void ApplyFinalVisualState(ResolutionVisualEvent visualEvent)
    {
        if (ResolutionVisualMapUtility.TryGetTerritory(
                visualEvent.OriginTerritoryId, out TerritorioClique origin))
            origin.AplicarEstadoVisualResolucao(
                visualEvent.OriginAfter, visualEvent.Player);
        if (ResolutionVisualMapUtility.TryGetTerritory(
                visualEvent.DestinationTerritoryId, out TerritorioClique destination))
            destination.AplicarEstadoVisualResolucao(
                visualEvent.DestinationAfter, visualEvent.NewOwner);
    }

    // ============================================================
    // 05. ELEMENTOS ESPECÍFICOS DO ATAQUE
    // ============================================================

    private RectTransform CreateAttacker(
        Transform parent,
        Color color,
        int amount)
    {
        Image image = ResolutionVisualMapUtility.CreateImage(
            "Attacker", parent, color, 0.95f);
        RectTransform rect = (RectTransform)image.transform;
        rect.sizeDelta = Vector2.one * theme.AttackIndicatorSize;
        rect.localEulerAngles = new Vector3(0f, 0f, 45f);

        var textObject = new GameObject("Amount", typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.SetParent(rect, false);
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = textRect.offsetMax = Vector2.zero;
        textRect.localEulerAngles = new Vector3(0f, 0f, -45f);
        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.font = theme.FonteInterface;
        text.fontSize = theme.AttackIndicatorTextSize;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.text = amount.ToString();
        text.raycastTarget = false;
        return rect;
    }

    private TextMeshProUGUI CreateResult(Transform parent, Color color)
    {
        TextMeshProUGUI text = ResolutionVisualMapUtility.CreateText(
            "Result", parent, theme.FonteInterface,
            theme.AttackResultTextSize, color);
        RectTransform rect = text.rectTransform;
        rect.sizeDelta = new Vector2(260f, 38f);
        text.alpha = 0f;
        return text;
    }

    private void ConfigurePulse(RectTransform rect, Vector2 position)
    {
        rect.anchoredPosition = position;
        rect.sizeDelta = Vector2.one * theme.AttackPulseSize;
        rect.localEulerAngles = new Vector3(0f, 0f, 45f);
    }
}
