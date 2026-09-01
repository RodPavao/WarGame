using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class PreparedActionArrowView : MonoBehaviour
{
    // ============================================================
    // 01. ELEMENTOS VISUAIS DA SETA
    // ============================================================

    private RectTransform shaft;
    private RectTransform headA;
    private RectTransform headB;
    private RectTransform energy;
    private TextMeshProUGUI amountText;
    private WarDominionUITheme theme;
    private float revealProgress;

    // ============================================================
    // 02. CONSTRUÇÃO NATIVA SEM RAYCAST
    // ============================================================

    public void Build(WarDominionUITheme newTheme)
    {
        theme = newTheme;
        RectTransform root = (RectTransform)transform;
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.offsetMin = Vector2.zero;
        root.offsetMax = Vector2.zero;

        shaft = CreateLine("Shaft", root, theme.ArrowColorOpacity);
        energy = CreateLine("EnergyPulse", root, 1f);
        headA = CreateLine("ArrowHeadA", root, 1f);
        headB = CreateLine("ArrowHeadB", root, 1f);

        var amountObject = new GameObject(
            "Amount", typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform amountRect = amountObject.GetComponent<RectTransform>();
        amountRect.SetParent(root, false);
        amountRect.sizeDelta = new Vector2(68f, 30f);

        amountText = amountObject.GetComponent<TextMeshProUGUI>();
        amountText.font = theme.FonteInterface;
        amountText.fontSize = theme.PreparedArrowAmountSize;
        amountText.fontStyle = FontStyles.Bold;
        amountText.alignment = TextAlignmentOptions.Center;
        amountText.raycastTarget = false;
        amountText.textWrappingMode = TextWrappingModes.NoWrap;
    }

    public void SetColorAndAmount(Color playerColor, int amount)
    {
        Color baseColor = playerColor;
        baseColor.a = theme.ArrowColorOpacity;
        shaft.GetComponent<Image>().color = baseColor;
        headA.GetComponent<Image>().color = playerColor;
        headB.GetComponent<Image>().color = playerColor;
        energy.GetComponent<Image>().color = playerColor;
        amountText.color = playerColor;
        amountText.text = amount > 0 ? amount.ToString() : string.Empty;
    }

    // ============================================================
    // 03. GEOMETRIA ADAPTATIVA ORIGEM → DESTINO
    // ============================================================

    public void SetGeometry(Vector2 origin, Vector2 destination)
    {
        Vector2 delta = destination - origin;
        float distance = delta.magnitude;
        if (distance < 2f)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        Vector2 direction = delta / distance;
        Vector2 start = origin + direction * theme.PreparedArrowEndpointInset;
        Vector2 end = destination - direction * theme.PreparedArrowEndpointInset;
        float usableDistance = Mathf.Max(1f, Vector2.Distance(start, end));
        float visibleDistance = usableDistance * Mathf.Clamp01(revealProgress);
        Vector2 visibleEnd = start + direction * visibleDistance;

        LayoutLine(shaft, start, visibleEnd, theme.PreparedArrowThickness);

        float headLength = theme.PreparedArrowHeadSize * Mathf.Clamp01(revealProgress);
        Vector2 backA = visibleEnd - Rotate(direction, 28f) * headLength;
        Vector2 backB = visibleEnd - Rotate(direction, -28f) * headLength;
        LayoutLine(headA, visibleEnd, backA, theme.PreparedArrowThickness);
        LayoutLine(headB, visibleEnd, backB, theme.PreparedArrowThickness);

        bool revealing = revealProgress < 1f;
        energy.gameObject.SetActive(revealing);
        if (revealing)
        {
            Vector2 pulseCenter = Vector2.Lerp(start, visibleEnd, revealProgress);
            float pulseLength = Mathf.Min(theme.PreparedArrowEnergyLength, visibleDistance);
            LayoutLine(
                energy,
                pulseCenter - direction * pulseLength * 0.5f,
                pulseCenter + direction * pulseLength * 0.5f,
                theme.PreparedArrowEnergyThickness);
        }

        RectTransform amountRect = (RectTransform)amountText.transform;
        amountRect.anchoredPosition = Vector2.Lerp(start, end, 0.32f) +
            new Vector2(-direction.y, direction.x) * theme.PreparedArrowAmountOffset;
        amountRect.localEulerAngles = Vector3.zero;
    }

    private void Update()
    {
        if (theme == null || revealProgress >= 1f)
            return;

        revealProgress = Mathf.Min(
            1f,
            revealProgress +
            Time.unscaledDeltaTime / Mathf.Max(0.01f, theme.PreparedArrowRevealDuration));
    }

    // ============================================================
    // 04. UTILITÁRIOS DE LINHA
    // ============================================================

    private static RectTransform CreateLine(
        string name,
        Transform parent,
        float opacity)
    {
        var lineObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        RectTransform rect = lineObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        Image image = lineObject.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, opacity);
        image.raycastTarget = false;
        return rect;
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

    private static Vector2 Rotate(Vector2 vector, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float cosine = Mathf.Cos(radians);
        float sine = Mathf.Sin(radians);
        return new Vector2(
            vector.x * cosine - vector.y * sine,
            vector.x * sine + vector.y * cosine);
    }
}
