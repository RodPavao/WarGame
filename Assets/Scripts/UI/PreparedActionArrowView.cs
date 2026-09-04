using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class PreparedActionArrowView : MonoBehaviour
{
    // ============================================================
    // 01. ELEMENTOS VISUAIS DA SETA
    // ============================================================

    private RectTransform headA;
    private RectTransform headB;
    private RectTransform energy;
    private WarDominionUITheme theme;
    private float revealProgress;
    private readonly List<RectTransform> curveSegments = new List<RectTransform>();
    private bool planning;
    private float beamProgress;

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

        energy = CreateLine("EnergyPulse", root, 1f);
        headA = CreateLine("ArrowHeadA", root, 1f);
        headB = CreateLine("ArrowHeadB", root, 1f);

        for (int i = 0; i < 20; i++)
            curveSegments.Add(CreateLine("CurveSegment_" + i, root, theme.ArrowColorOpacity));
    }

    public void SetPlanning(bool value)
    {
        planning = value;
        headA.gameObject.SetActive(!value);
        headB.gameObject.SetActive(!value);
        energy.gameObject.SetActive(value);
    }

    public void SetColorAndAmount(Color playerColor, int amount)
    {
        Color baseColor = playerColor;
        baseColor.a = theme.ArrowColorOpacity;
        headA.GetComponent<Image>().color = playerColor;
        headB.GetComponent<Image>().color = playerColor;
        Color beamColor = Color.Lerp(playerColor, Color.white, 0.52f);
        beamColor.a = 0.92f;
        energy.GetComponent<Image>().color = beamColor;
        foreach (RectTransform segment in curveSegments)
            segment.GetComponent<Image>().color = baseColor;
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
        Vector2 normal = new Vector2(-direction.y, direction.x);
        Vector2 control = (start + end) * 0.5f + normal * Mathf.Min(
            theme.PreparedArrowMaxCurveHeight,
            usableDistance * theme.PreparedArrowCurveFactor);
        int visibleSegments = Mathf.CeilToInt(curveSegments.Count * Mathf.Clamp01(revealProgress));
        Vector2 visibleEnd = start;
        for (int i = 0; i < curveSegments.Count; i++)
        {
            float t0 = i / (float)curveSegments.Count;
            float segmentCoverage = planning ? 0.68f : 1.05f;
            float t1 = (i + segmentCoverage) / curveSegments.Count;
            Vector2 pointA = Bezier(start, control, end, t0);
            Vector2 pointB = Bezier(start, control, end, Mathf.Min(1f, t1));
            int dashStride = Mathf.Max(2, theme.PlanningDashStride);
            bool active = i < visibleSegments &&
                (!planning || i % dashStride != dashStride - 1);
            curveSegments[i].gameObject.SetActive(active);
            if (active) LayoutLine(curveSegments[i], pointA, pointB, theme.PreparedArrowThickness);
            if (i == visibleSegments - 1) visibleEnd = pointB;
        }

        float headLength = theme.PreparedArrowHeadSize * Mathf.Clamp01(revealProgress);
        Vector2 tangent = (end - control).normalized;
        Vector2 backA = visibleEnd - Rotate(tangent, 28f) * headLength;
        Vector2 backB = visibleEnd - Rotate(tangent, -28f) * headLength;
        LayoutLine(headA, visibleEnd, backA, theme.PreparedArrowThickness);
        LayoutLine(headB, visibleEnd, backB, theme.PreparedArrowThickness);

        energy.gameObject.SetActive(planning);
        if (planning)
        {
            float halfCoverage = Mathf.Min(
                0.16f,
                theme.PreparedArrowEnergyLength / Mathf.Max(1f, usableDistance) * 0.5f);
            float centerT = Mathf.Repeat(beamProgress, 1f);
            Vector2 pulseStart = Bezier(start, control, end, Mathf.Clamp01(centerT - halfCoverage));
            Vector2 pulseEnd = Bezier(start, control, end, Mathf.Clamp01(centerT + halfCoverage));
            LayoutLine(
                energy,
                pulseStart,
                pulseEnd,
                theme.PreparedArrowEnergyThickness);
        }
    }

    private void Update()
    {
        if (theme == null)
            return;

        if (planning)
            beamProgress = Mathf.Repeat(
                beamProgress + Time.unscaledDeltaTime * theme.PlanningBeamSpeed,
                1f);

        if (revealProgress >= 1f)
            return;

        revealProgress = Mathf.Min(
            1f,
            revealProgress +
            Time.unscaledDeltaTime / Mathf.Max(0.01f, theme.PreparedArrowRevealDuration));
    }

    private static Vector2 Bezier(Vector2 start, Vector2 control, Vector2 end, float t)
    {
        float inverse = 1f - t;
        return inverse * inverse * start + 2f * inverse * t * control + t * t * end;
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
