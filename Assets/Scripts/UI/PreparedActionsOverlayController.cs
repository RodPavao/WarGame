using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class PreparedActionsOverlayController : MonoBehaviour
{
    private sealed class ArrowBinding
    {
        public PreparedActionArrowView View;
        public string OriginId;
        public string DestinationId;
    }

    // ============================================================
    // 01. ESTADO VISUAL DERIVADO DO PRESENTER
    // ============================================================

    private readonly Dictionary<string, ArrowBinding> arrows =
        new Dictionary<string, ArrowBinding>();
    private readonly HashSet<string> activeKeys = new HashSet<string>();
    private MatchUIPresenter presenter;
    private RectTransform overlay;
    private WarDominionUITheme theme;
    private RectTransform originMarker;
    private CanvasGroup originMarkerCanvas;
    private string selectedOriginId = string.Empty;

    // ============================================================
    // 02. INICIALIZAÇÃO E ASSINATURA
    // ============================================================

    public void Configure(
        MatchUIPresenter newPresenter,
        RectTransform newOverlay,
        WarDominionUITheme newTheme)
    {
        if (presenter != null)
            presenter.EstadoAlterado -= OnStateChanged;

        presenter = newPresenter;
        overlay = newOverlay;
        theme = newTheme;
        EnsureOriginMarker();

        if (presenter != null)
        {
            presenter.EstadoAlterado += OnStateChanged;
            presenter.SolicitarSnapshotAtual();
        }
    }

    private void OnDestroy()
    {
        if (presenter != null)
            presenter.EstadoAlterado -= OnStateChanged;
    }

    // ============================================================
    // 03. RECONCILIAÇÃO COM A FILA REAL DE ATAQUES
    // ============================================================

    private void OnStateChanged(MatchUIState state)
    {
        if (state == null ||
            state.Fase != GameManager.FaseTurno.Preparacao)
        {
            ClearAll();
            return;
        }

        activeKeys.Clear();
        foreach (MatchUIActionState action in state.Acoes)
        {
            if (action.TipoEsperado != "ATAQUE")
                continue;

            string key = $"action:{action.Posicao}:{action.OrigemId}:{action.DestinoId}";
            EnsureArrow(
                key, action.OrigemId, action.DestinoId,
                state.JogadorLocal, action.Quantidade);
            activeKeys.Add(key);
        }

        bool hasPreview =
            !string.IsNullOrEmpty(state.OrigemSelecionadaId) &&
            !string.IsNullOrEmpty(state.DestinoSelecionadoId) &&
            state.TipoAcaoEsperado == "ATAQUE";

        if (hasPreview)
        {
            const string previewKey = "preview";
            EnsureArrow(
                previewKey,
                state.OrigemSelecionadaId,
                state.DestinoSelecionadoId,
                state.JogadorLocal,
                state.QuantidadeAcaoSelecionada);
            activeKeys.Add(previewKey);
        }

        RemoveInactiveArrows();
        selectedOriginId = state.OrigemSelecionadaId;
        originMarker.gameObject.SetActive(!string.IsNullOrEmpty(selectedOriginId));
    }

    private void EnsureArrow(
        string key,
        string originId,
        string destinationId,
        TerritorioClique.Dono player,
        int amount)
    {
        if (!arrows.TryGetValue(key, out ArrowBinding binding))
        {
            var arrowObject = new GameObject(
                $"PreparedAttack_{key}", typeof(RectTransform));
            arrowObject.transform.SetParent(overlay, false);
            var view = arrowObject.AddComponent<PreparedActionArrowView>();
            view.Build(theme);
            binding = new ArrowBinding { View = view };
            arrows.Add(key, binding);
        }

        binding.OriginId = originId;
        binding.DestinationId = destinationId;
        binding.View.SetColorAndAmount(
            PaletaJogadores.ObterCorAtiva(player), amount);
    }

    private void RemoveInactiveArrows()
    {
        var removedKeys = new List<string>();
        foreach (KeyValuePair<string, ArrowBinding> pair in arrows)
        {
            if (activeKeys.Contains(pair.Key))
                continue;

            Destroy(pair.Value.View.gameObject);
            removedKeys.Add(pair.Key);
        }

        foreach (string key in removedKeys)
            arrows.Remove(key);
    }

    // ============================================================
    // 04. ACOMPANHAMENTO DE CÂMERA, MAPA E CONTADORES
    // ============================================================

    private void LateUpdate()
    {
        if (overlay == null || theme == null)
            return;

        foreach (ArrowBinding binding in arrows.Values)
        {
            if (TryGetOverlayPosition(binding.OriginId, out Vector2 origin) &&
                TryGetOverlayPosition(binding.DestinationId, out Vector2 destination))
            {
                binding.View.SetGeometry(origin, destination);
            }
            else
            {
                binding.View.gameObject.SetActive(false);
            }
        }

        UpdateOriginMarker();
    }

    private bool TryGetOverlayPosition(string territoryId, out Vector2 position)
    {
        position = default;
        if (!TryGetTerritory(territoryId, out TerritorioClique territory))
            return false;

        ContadorTropas counter = territory.GetComponentInChildren<ContadorTropas>(true);
        Vector3 worldPosition = counter != null
            ? counter.transform.position
            : territory.transform.position;

        Camera worldCamera = Camera.main != null
            ? Camera.main
            : FindAnyObjectByType<Camera>();
        return MapOverlayProjection.TryWorldToOverlay(
            overlay, worldPosition, worldCamera, out position);
    }

    private static bool TryGetTerritory(
        string territoryId,
        out TerritorioClique territory)
    {
        territory = null;
        if (MapaAtivo.Instance != null &&
            MapaAtivo.Instance.TentarObterTerritorio(territoryId, out territory))
        {
            return territory != null;
        }

        foreach (TerritorioClique candidate in MapaAtivo.ObterTerritoriosOuCena())
        {
            if (candidate != null && candidate.idTerritorio == territoryId)
            {
                territory = candidate;
                return true;
            }
        }

        return false;
    }

    // ============================================================
    // 05. FEEDBACK DISCRETO DA ORIGEM/CONTADOR
    // ============================================================

    private void EnsureOriginMarker()
    {
        if (originMarker != null)
            return;

        var markerObject = new GameObject(
            "PreparedOriginMarker", typeof(RectTransform),
            typeof(Image), typeof(CanvasGroup));
        originMarker = markerObject.GetComponent<RectTransform>();
        originMarker.SetParent(overlay, false);
        originMarker.anchorMin = new Vector2(0.5f, 0.5f);
        originMarker.anchorMax = new Vector2(0.5f, 0.5f);
        originMarker.sizeDelta = Vector2.one * theme.PreparedOriginMarkerSize;
        originMarker.localEulerAngles = new Vector3(0f, 0f, 45f);

        Image image = markerObject.GetComponent<Image>();
        image.color = theme.Acento;
        image.raycastTarget = false;
        originMarkerCanvas = markerObject.GetComponent<CanvasGroup>();
        originMarkerCanvas.blocksRaycasts = false;
        originMarkerCanvas.interactable = false;
        markerObject.SetActive(false);
    }

    private void UpdateOriginMarker()
    {
        if (originMarker == null || !originMarker.gameObject.activeSelf)
            return;

        if (!TryGetOverlayPosition(selectedOriginId, out Vector2 position))
        {
            originMarker.gameObject.SetActive(false);
            return;
        }

        float pulse = (Mathf.Sin(Time.unscaledTime * 5f) + 1f) * 0.5f;
        originMarker.anchoredPosition = position;
        originMarker.localScale = Vector3.one * Mathf.Lerp(0.82f, 1.08f, pulse);
        originMarkerCanvas.alpha = Mathf.Lerp(0.10f, 0.25f, pulse);
    }

    private void ClearAll()
    {
        foreach (ArrowBinding binding in arrows.Values)
            if (binding.View != null)
                Destroy(binding.View.gameObject);
        arrows.Clear();
        activeKeys.Clear();
        selectedOriginId = string.Empty;
        if (originMarker != null)
            originMarker.gameObject.SetActive(false);
    }
}
