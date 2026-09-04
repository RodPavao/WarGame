using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        PreparedActionArrowView route = root.gameObject.AddComponent<PreparedActionArrowView>();
        route.Build(theme);
        route.SetColorAndAmount(playerColor, 0);
        route.SetPlanning(false);
        GameObject attacker = CreateCounterCopy(origin, visualEvent.OriginBefore, visualEvent.Player);
        route.SetGeometry(originPosition, destinationPosition);
        Vector3 originWorld = GetCounterWorldPosition(origin);
        Vector3 destinationWorld = GetCounterWorldPosition(destination);
        Vector3 controlWorld = CreateWorldControl(originWorld, destinationWorld);
        attacker.transform.position = originWorld;
        origin.DestacarReforco();
        float elapsed = 0f;
        while (elapsed < theme.AttackTravelDuration)
        {
            elapsed += context.PlaybackDeltaTime;
            float progress = Mathf.Clamp01(elapsed / theme.AttackTravelDuration);
            float smooth = progress * progress * (3f - 2f * progress);
            attacker.transform.position = Bezier(
                originWorld, controlWorld, destinationWorld, smooth);
            yield return null;
        }

        Destroy(attacker);
        ApplyFinalVisualState(visualEvent);
        destination.DestacarReforco();
        yield return context.Wait(theme.AttackImpactDuration);
        yield return context.Wait(theme.AttackResultDuration);

        Destroy(root.gameObject);
    }

    // ============================================================
    // 04. APLICAÇÃO DO RESULTADO AUTORITATIVO
    // ============================================================

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

    private static GameObject CreateCounterCopy(
        TerritorioClique origin,
        int amount,
        TerritorioClique.Dono owner)
    {
        ContadorTropas source = origin.GetComponentInChildren<ContadorTropas>(true);
        if (source == null)
            return new GameObject("AnimatedCounterCopy");

        GameObject copy = Instantiate(source.gameObject);
        copy.name = "AnimatedCounterCopy";
        copy.transform.SetParent(null, true);
        foreach (Collider2D collider in copy.GetComponentsInChildren<Collider2D>(true))
            collider.enabled = false;
        foreach (Renderer renderer in copy.GetComponentsInChildren<Renderer>(true))
            renderer.sortingOrder += 100;
        ContadorTropas controller = copy.GetComponent<ContadorTropas>();
        controller.AtualizarVisual(amount, owner);
        controller.enabled = false;
        return copy;
    }

    private static Vector3 GetCounterWorldPosition(TerritorioClique territory)
    {
        ContadorTropas counter = territory.GetComponentInChildren<ContadorTropas>(true);
        return counter != null ? counter.transform.position : territory.transform.position;
    }

    private Vector3 CreateWorldControl(Vector3 origin, Vector3 destination)
    {
        Vector3 delta = destination - origin;
        Vector3 normal = new Vector3(-delta.y, delta.x, 0f).normalized;
        return (origin + destination) * 0.5f + normal * Mathf.Min(
            theme.AttackMaxCurveHeight,
            delta.magnitude * theme.AttackCurveFactor);
    }

    private static Vector3 Bezier(Vector3 start, Vector3 control, Vector3 end, float t)
    {
        float inverse = 1f - t;
        return inverse * inverse * start + 2f * inverse * t * control + t * t * end;
    }
}
