using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class TopBarView : MonoBehaviour
{
    // ============================================================
    // 01. REFERÊNCIAS DA VIEW
    // ============================================================

    private TextMeshProUGUI titulo;
    private TextMeshProUGUI estado;
    private TextMeshProUGUI cronometro;

    // ============================================================
    // 02. CONSTRUÇÃO VISUAL
    // ============================================================

    public void Construir(WarDominionUITheme tema)
    {
        RectTransform raiz = (RectTransform)transform;
        raiz.anchorMin = new Vector2(0.5f, 1f);
        raiz.anchorMax = new Vector2(0.5f, 1f);
        raiz.pivot = new Vector2(0.5f, 1f);
        raiz.anchoredPosition = new Vector2(0f, -tema.MargemSuperior);
        raiz.sizeDelta = new Vector2(tema.LarguraTopBar, tema.AlturaTopBar);

        Image fundo = gameObject.AddComponent<Image>();
        fundo.color = tema.SuperficiePrincipal;
        fundo.raycastTarget = false;

        CriarFaixa("MolduraSuperior", raiz, tema.Moldura, 1f, true);
        CriarFaixa("AcentoInferior", raiz, tema.Acento, 2f, false);
        CriarSeparador("SeparadorEsquerdo", raiz, 0.31f, tema.Moldura);
        CriarSeparador("SeparadorDireito", raiz, 0.72f, tema.Moldura);

        titulo = CriarTexto(
            "Titulo", raiz, new Vector2(0f, 0f), new Vector2(0.31f, 1f),
            "WAR DOMINION", TextAlignmentOptions.MidlineLeft,
            tema.FonteDisplay, tema.TamanhoTitulo, tema.TextoPrincipal);
        titulo.margin = new Vector4(tema.MargemLateral, 0f, 8f, 0f);

        estado = CriarTexto(
            "EstadoPartida", raiz, new Vector2(0.31f, 0f), new Vector2(0.72f, 1f),
            "ROUND 01  •  PREPARAÇÃO", TextAlignmentOptions.Center,
            tema.FonteInterface, tema.TamanhoEstado, tema.TextoSecundario);

        cronometro = CriarTexto(
            "Cronometro", raiz, new Vector2(0.72f, 0f), Vector2.one,
            "01:30", TextAlignmentOptions.MidlineRight,
            tema.FonteDisplay, tema.TamanhoCronometro, tema.TextoPrincipal);
        cronometro.margin = new Vector4(8f, 0f, tema.MargemLateral, 0f);
    }

    // ============================================================
    // 03. APRESENTAÇÃO DO MATCHUISTATE
    // ============================================================

    public void Apresentar(MatchUIState state)
    {
        if (state == null || estado == null || cronometro == null)
            return;

        string round = state.EmMorteSubita
            ? $"MORTE SÚBITA {Mathf.Max(1, state.RoundMorteSubita):00}"
            : $"ROUND {Mathf.Max(1, state.Round):00}";

        estado.text = $"{round}  •  {FormatarFase(state)}";
        cronometro.text = FormatarTempo(state.TempoRestante);
    }

    private static string FormatarFase(MatchUIState state)
    {
        if (state.Resultado != null && state.Resultado.Encerrada)
            return "ENCERRADA";

        if (state.Fase == GameManager.FaseTurno.Resolucao)
            return "RESOLUÇÃO";

        switch (state.EstadoPreparacao)
        {
            case GameManager.EstadoPreparacao.Enviado:
                return "ENVIADO";
            case GameManager.EstadoPreparacao.Resolvendo:
                return "RESOLVENDO";
            default:
                return "PREPARAÇÃO";
        }
    }

    private static string FormatarTempo(float segundos)
    {
        int total = Mathf.Max(0, Mathf.CeilToInt(segundos));
        return $"{total / 60:00}:{total % 60:00}";
    }

    // ============================================================
    // 04. FÁBRICA LOCAL DE ELEMENTOS DECORATIVOS
    // ============================================================

    private static TextMeshProUGUI CriarTexto(
        string nome,
        Transform pai,
        Vector2 ancoraMin,
        Vector2 ancoraMax,
        string valor,
        TextAlignmentOptions alinhamento,
        TMP_FontAsset fonte,
        float tamanho,
        Color cor)
    {
        var objeto = new GameObject(nome, typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform rect = objeto.GetComponent<RectTransform>();
        rect.SetParent(pai, false);
        rect.anchorMin = ancoraMin;
        rect.anchorMax = ancoraMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TextMeshProUGUI texto = objeto.GetComponent<TextMeshProUGUI>();
        texto.text = valor;
        texto.font = fonte;
        texto.fontSize = tamanho;
        texto.fontStyle = FontStyles.Bold;
        texto.color = cor;
        texto.alignment = alinhamento;
        texto.textWrappingMode = TextWrappingModes.NoWrap;
        texto.overflowMode = TextOverflowModes.Ellipsis;
        texto.raycastTarget = false;
        return texto;
    }

    private static void CriarFaixa(
        string nome,
        Transform pai,
        Color cor,
        float altura,
        bool superior)
    {
        var objeto = new GameObject(nome, typeof(RectTransform), typeof(Image));
        RectTransform rect = objeto.GetComponent<RectTransform>();
        rect.SetParent(pai, false);
        rect.anchorMin = superior ? new Vector2(0f, 1f) : Vector2.zero;
        rect.anchorMax = superior ? Vector2.one : new Vector2(1f, 0f);
        rect.pivot = superior ? new Vector2(0.5f, 1f) : new Vector2(0.5f, 0f);
        rect.sizeDelta = new Vector2(0f, altura);
        rect.anchoredPosition = Vector2.zero;

        Image imagem = objeto.GetComponent<Image>();
        imagem.color = cor;
        imagem.raycastTarget = false;
    }

    private static void CriarSeparador(string nome, Transform pai, float ancoraX, Color cor)
    {
        var objeto = new GameObject(nome, typeof(RectTransform), typeof(Image));
        RectTransform rect = objeto.GetComponent<RectTransform>();
        rect.SetParent(pai, false);
        rect.anchorMin = new Vector2(ancoraX, 0.18f);
        rect.anchorMax = new Vector2(ancoraX, 0.82f);
        rect.sizeDelta = new Vector2(1f, 0f);
        rect.anchoredPosition = Vector2.zero;

        Image imagem = objeto.GetComponent<Image>();
        imagem.color = cor;
        imagem.raycastTarget = false;
    }
}
