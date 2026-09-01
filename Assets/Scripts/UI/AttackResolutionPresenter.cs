using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class AttackResolutionPresenter : MonoBehaviour,
    IResolutionVisualPresenter,
    IResolutionVisualStateCoordinator
{
    // ============================================================
    // 01. CONFIGURAÇÃO E ESTADO VISUAL TEMPORÁRIO
    // ============================================================

    private readonly HashSet<TerritorioClique> overriddenTerritories =
        new HashSet<TerritorioClique>();
    private WarDominionUITheme theme;

    public void Configure(WarDominionUITheme newTheme)
    {
        theme = newTheme;
    }

    public bool Supports(ResolutionVisualEventType type) =>
        type == ResolutionVisualEventType.Attack;

    // ============================================================
    // 02. SNAPSHOT VISUAL ANTERIOR E RESTAURAÇÃO LÓGICA
    // ============================================================

    public void PrepareVisualState(IReadOnlyList<ResolutionVisualEvent> events)
    {
        CompleteVisualState();
        if (events == null)
            return;

        foreach (ResolutionVisualEvent visualEvent in events)
        {
            if (visualEvent == null || visualEvent.Type != ResolutionVisualEventType.Attack)
                continue;

            ApplyInitialIfFirst(
                visualEvent.OriginTerritoryId,
                visualEvent.OriginBefore,
                visualEvent.Player);
            ApplyInitialIfFirst(
                visualEvent.DestinationTerritoryId,
                visualEvent.DestinationBefore,
                visualEvent.PreviousOwner);
        }
    }

    public void CompleteVisualState()
    {
        foreach (TerritorioClique territory in overriddenTerritories)
            if (territory != null)
                territory.RestaurarEstadoVisualLogico();
        overriddenTerritories.Clear();
    }

    private void ApplyInitialIfFirst(
        string territoryId,
        int troops,
        TerritorioClique.Dono owner)
    {
        if (!TryGetTerritory(territoryId, out TerritorioClique territory) ||
            overriddenTerritories.Contains(territory))
        {
            return;
        }

        overriddenTerritories.Add(territory);
        territory.AplicarEstadoVisualResolucao(troops, owner);
    }

    // ============================================================
    // 03. COREOGRAFIA DO ATAQUE JÁ CALCULADO
    // ============================================================

    public IEnumerator Present(
        ResolutionVisualEvent visualEvent,
        ResolutionVisualPresentationContext context)
    {
        if (theme == null || visualEvent == null || context == null ||
            !TryGetTerritory(visualEvent.OriginTerritoryId, out TerritorioClique origin) ||
            !TryGetTerritory(visualEvent.DestinationTerritoryId, out TerritorioClique destination) ||
            !TryGetPosition(origin, context, out Vector2 originPosition) ||
            !TryGetPosition(destination, context, out Vector2 destinationPosition))
        {
            yield break;
        }

        Color playerColor = context.GetPlayerColor(visualEvent.Player);
        RectTransform root = CreateRoot(context.Overlay);
        Image route = CreateImage("Route", root, playerColor, 0.28f);
        Image originPulse = CreateImage("OriginPulse", root, playerColor, 0.35f);
        Image destinationPulse = CreateImage("DestinationPulse", root, playerColor, 0.28f);
        RectTransform attacker = CreateAttacker(root, playerColor, visualEvent.Amount);
        Image impact = CreateImage("Impact", root, playerColor, 0f);
        TextMeshProUGUI result = CreateResult(root, playerColor);

        LayoutLine((RectTransform)route.transform, originPosition, destinationPosition,
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
        if (TryGetTerritory(visualEvent.OriginTerritoryId, out TerritorioClique origin))
            origin.AplicarEstadoVisualResolucao(
                visualEvent.OriginAfter, visualEvent.Player);
        if (TryGetTerritory(visualEvent.DestinationTerritoryId, out TerritorioClique destination))
            destination.AplicarEstadoVisualResolucao(
                visualEvent.DestinationAfter, visualEvent.NewOwner);
    }

    // ============================================================
    // 05. RESOLUÇÃO GENÉRICA DE TERRITÓRIOS E POSIÇÕES
    // ============================================================

    private static bool TryGetTerritory(string id, out TerritorioClique territory)
    {
        territory = null;
        if (MapaAtivo.Instance != null &&
            MapaAtivo.Instance.TentarObterTerritorio(id, out territory))
        {
            return territory != null;
        }

        foreach (TerritorioClique candidate in MapaAtivo.ObterTerritoriosOuCena())
        {
            if (candidate != null && candidate.idTerritorio == id)
            {
                territory = candidate;
                return true;
            }
        }
        return false;
    }

    private static bool TryGetPosition(
        TerritorioClique territory,
        ResolutionVisualPresentationContext context,
        out Vector2 position)
    {
        ContadorTropas counter = territory.GetComponentInChildren<ContadorTropas>(true);
        Vector3 world = counter != null ? counter.transform.position : territory.transform.position;
        Camera camera = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
        return context.TryWorldToOverlay(world, camera, out position);
    }

    // ============================================================
    // 06. FÁBRICA VISUAL SEM RAYCAST
    // ============================================================

    private static RectTransform CreateRoot(RectTransform parent)
    {
        var gameObject = new GameObject("AttackResolution", typeof(RectTransform));
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return rect;
    }

    private Image CreateImage(
        string name,
        Transform parent,
        Color color,
        float alpha)
    {
        var gameObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        Image image = gameObject.GetComponent<Image>();
        color.a = alpha;
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private RectTransform CreateAttacker(
        Transform parent,
        Color color,
        int amount)
    {
        Image image = CreateImage("Attacker", parent, color, 0.95f);
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
        var gameObject = new GameObject("Result", typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(260f, 38f);
        TextMeshProUGUI text = gameObject.GetComponent<TextMeshProUGUI>();
        text.font = theme.FonteInterface;
        text.fontSize = theme.AttackResultTextSize;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = color;
        text.alpha = 0f;
        text.raycastTarget = false;
        return text;
    }

    private void ConfigurePulse(RectTransform rect, Vector2 position)
    {
        rect.anchoredPosition = position;
        rect.sizeDelta = Vector2.one * theme.AttackPulseSize;
        rect.localEulerAngles = new Vector3(0f, 0f, 45f);
    }

    private static void LayoutLine(
        RectTransform line,
        Vector2 start,
        Vector2 end,
        float thickness)
    {
        Vector2 delta = end - start;
        line.anchoredPosition = (start + end) * 0.5f;
        line.sizeDelta = new Vector2(delta.magnitude, thickness);
        line.localEulerAngles = new Vector3(
            0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
    }
}
