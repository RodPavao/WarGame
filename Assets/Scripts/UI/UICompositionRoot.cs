using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(GameManager))]
public sealed class UICompositionRoot : MonoBehaviour
{
    // ============================================================
    // 01. COMPOSIÇÃO E CICLO DE VIDA
    // ============================================================

    private const string CaminhoTema = "UI/WarDominionUITheme";
    private MatchUIPresenter presenter;
    private RectTransform safeArea;
    private TopBarView topBar;
    private Rect ultimaAreaSegura;
    private Vector2Int ultimaResolucao;

    private void Awake()
    {
        GarantirEstrutura();
        Inicializar(GetComponent<MatchUIPresenter>());
    }

    private void OnDestroy()
    {
        DesconectarPresenter();
    }

    public void Inicializar(MatchUIPresenter novoPresenter)
    {
        GarantirEstrutura();

        if (presenter == novoPresenter)
        {
            presenter?.SolicitarSnapshotAtual();
            return;
        }

        DesconectarPresenter();
        presenter = novoPresenter;

        if (presenter == null)
            return;

        presenter.EstadoAlterado += AoEstadoAlterado;
        presenter.SolicitarSnapshotAtual();
    }

    private void DesconectarPresenter()
    {
        if (presenter != null)
            presenter.EstadoAlterado -= AoEstadoAlterado;

        presenter = null;
    }

    // ============================================================
    // 02. CANVAS E HIERARQUIA REUTILIZÁVEL
    // ============================================================

    private void GarantirEstrutura()
    {
        if (topBar != null)
            return;

        WarDominionUITheme tema = Resources.Load<WarDominionUITheme>(CaminhoTema);
        if (tema == null)
        {
            Debug.LogError($"Tema da UI não encontrado em Resources/{CaminhoTema}.", this);
            return;
        }

        Transform existente = transform.Find("UIRoot");
        GameObject raiz = existente != null
            ? existente.gameObject
            : new GameObject(
                "UIRoot",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));

        RectTransform raizRect = raiz.GetComponent<RectTransform>();
        raizRect.SetParent(transform, false);
        raizRect.localScale = Vector3.one;

        Canvas canvas = raiz.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = raiz.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        safeArea = CriarCamada("SafeArea", raizRect);
        RectTransform topBarRect = CriarCamada("TopBar", safeArea);
        CriarCamada("LeftDock", safeArea);
        CriarCamada("RightDock", safeArea);
        CriarCamada("BottomActionBar", safeArea);
        CriarCamada("MapOverlayLayer", raizRect);
        CriarCamada("NotificationLayer", raizRect);
        CriarCamada("ModalLayer", raizRect);

        topBar = topBarRect.gameObject.AddComponent<TopBarView>();
        topBar.Construir(tema);
        AtualizarSafeArea(true);
    }

    private static RectTransform CriarCamada(string nome, Transform pai)
    {
        Transform existente = pai.Find(nome);
        RectTransform rect;

        if (existente != null)
        {
            rect = existente as RectTransform;
        }
        else
        {
            var objeto = new GameObject(nome, typeof(RectTransform));
            rect = objeto.GetComponent<RectTransform>();
            rect.SetParent(pai, false);
        }

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return rect;
    }

    // ============================================================
    // 03. SAFE AREA E RESPONSIVIDADE BÁSICA
    // ============================================================

    private void Update()
    {
        AtualizarSafeArea(false);
    }

    private void AtualizarSafeArea(bool forcar)
    {
        if (safeArea == null || Screen.width <= 0 || Screen.height <= 0)
            return;

        Rect area = Screen.safeArea;
        var resolucao = new Vector2Int(Screen.width, Screen.height);

        if (!forcar && area == ultimaAreaSegura && resolucao == ultimaResolucao)
            return;

        ultimaAreaSegura = area;
        ultimaResolucao = resolucao;
        safeArea.anchorMin = new Vector2(area.xMin / Screen.width, area.yMin / Screen.height);
        safeArea.anchorMax = new Vector2(area.xMax / Screen.width, area.yMax / Screen.height);
        safeArea.offsetMin = Vector2.zero;
        safeArea.offsetMax = Vector2.zero;
    }

    // ============================================================
    // 04. LIGAÇÃO DA VIEW AO ESTADO
    // ============================================================

    private void AoEstadoAlterado(MatchUIState state)
    {
        topBar?.Apresentar(state);
    }
}
