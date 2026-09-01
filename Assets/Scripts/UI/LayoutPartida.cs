using UnityEngine;

public class LayoutPartida : MonoBehaviour
{
    public static LayoutPartida instance;

    // =====================================================
    // CONFIGURAÇÃO
    // =====================================================

    [Header("Painéis laterais")]

    [Range(0.05f, 0.20f)]
    public float larguraPainelEsquerdo = 0.10f;

    [Range(0.05f, 0.20f)]
    public float larguraPainelDireito = 0.10f;


    [Header("Mapa")]

    [Tooltip("1 = tamanho normal. Acima de 1 aproxima.")]
    [Range(0.90f, 1.10f)]
    public float zoomMapa = 1.00f;


    // =====================================================
    // ESTADO
    // =====================================================

    private Camera cameraPrincipal;

    private Rect rectOriginal;
    private float sizeOriginal;
    private Vector3 posicaoOriginal;
    private Color corOriginal;

    private bool configurado = false;


    // =====================================================
    // INICIALIZAÇÃO
    // =====================================================

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        EncontrarCamera();
    }


    private void Start()
    {
        AplicarLayout();
    }


    // Recalcula se a resolução/tamanho da janela mudar.
    private void OnRectTransformDimensionsChange()
    {
        if (Application.isPlaying && configurado)
        {
            AplicarLayout();
        }
    }


    // =====================================================
    // CÂMERA
    // =====================================================

    private void EncontrarCamera()
    {
        cameraPrincipal = Camera.main;

        if (cameraPrincipal == null)
        {
            cameraPrincipal = FindAnyObjectByType<Camera>();
        }

        if (cameraPrincipal == null)
        {
            Debug.LogError(
                "LAYOUT | Main Camera não encontrada."
            );
        }
    }


    // =====================================================
    // APLICAR LAYOUT
    // =====================================================

    public void AplicarLayout()
    {
        if (cameraPrincipal == null)
        {
            EncontrarCamera();

            if (cameraPrincipal == null)
                return;
        }


        if (!configurado)
        {
            rectOriginal = cameraPrincipal.rect;

            sizeOriginal =
                cameraPrincipal.orthographicSize;

            posicaoOriginal =
                cameraPrincipal.transform.position;

            corOriginal =
                cameraPrincipal.backgroundColor;

            configurado = true;
        }


        float larguraCentral =
            1f
            - larguraPainelEsquerdo
            - larguraPainelDireito;


        larguraCentral =
            Mathf.Clamp(
                larguraCentral,
                0.60f,
                0.90f
            );


        // Área exclusiva do mapa.
        cameraPrincipal.rect =
            new Rect(
                larguraPainelEsquerdo,
                0f,
                larguraCentral,
                1f
            );


        cameraPrincipal.backgroundColor =
            new Color(
                0.005f,
                0.005f,
                0.008f,
                1f
            );


        AjustarMapaPelaAltura();
    }


    // =====================================================
    // MAPA - ALTURA TEM PRIORIDADE
    // =====================================================

    private void AjustarMapaPelaAltura()
    {
        if (MapaAtivo.Instance == null ||
            !MapaAtivo.Instance.TentarObterLimitesVisuais(out Bounds limitesMapa))
        {
            Debug.LogError(
                "LAYOUT | Visual do mapa ativo não encontrado."
            );

            return;
        }


        // =================================================
        // REGRA PRINCIPAL
        //
        // A altura do sprite ocupa toda a altura disponível.
        // Não usamos mais a largura para afastar a câmera.
        // =================================================

        float alturaMapa =
            limitesMapa.size.y;


        float tamanhoCamera =
            alturaMapa / 2f;


        tamanhoCamera /=
            Mathf.Max(
                zoomMapa,
                0.01f
            );


        cameraPrincipal.orthographicSize =
            tamanhoCamera;


        // =================================================
        // 04. CENTRALIZAÇÃO E OFFSET DATA-DRIVEN
        //
        // O centro da arte permanece como comportamento padrão.
        // Cada definição pode ajustar somente a composição visual,
        // sem mover mapa, territórios, colliders ou contadores.
        // =================================================

        Vector3 centroMapa =
            limitesMapa.center;

        Vector2 deslocamentoEnquadramento =
            MapaAtivo.Instance.Definicao != null
                ? MapaAtivo.Instance.Definicao.DeslocamentoEnquadramento
                : Vector2.zero;


        cameraPrincipal.transform.position =
            new Vector3(
                centroMapa.x + deslocamentoEnquadramento.x,
                centroMapa.y + deslocamentoEnquadramento.y,
                posicaoOriginal.z
            );


        Debug.Log(
            "LAYOUT | Mapa preenchendo altura | " +
            "Orthographic Size: " +
            tamanhoCamera.ToString("0.00") +
            " | Offset: " +
            deslocamentoEnquadramento.ToString("F2") +
            " | Painéis: " +
            Mathf.RoundToInt(
                larguraPainelEsquerdo * 100f
            ) +
            "% / " +
            Mathf.RoundToInt(
                larguraPainelDireito * 100f
            ) +
            "%"
        );
    }


    // =====================================================
    // RESTAURAR
    // =====================================================

    public void RestaurarLayout()
    {
        if (!configurado ||
            cameraPrincipal == null)
            return;


        cameraPrincipal.rect =
            rectOriginal;


        cameraPrincipal.orthographicSize =
            sizeOriginal;


        cameraPrincipal.transform.position =
            posicaoOriginal;


        cameraPrincipal.backgroundColor =
            corOriginal;
    }
}
