using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class RoundAnnouncementView : MonoBehaviour
{
    // ============================================================
    // 01. REFERÊNCIAS VISUAIS E CONFIGURAÇÃO
    // ============================================================

    private RectTransform painel;
    private RectTransform linhaBase;
    private RectTransform segmentoEnergia;
    private CanvasGroup canvasGroup;
    private TextMeshProUGUI textoRound;
    private TextMeshProUGUI textoFase;
    private WarDominionUITheme tema;
    private Coroutine animacaoAtual;

    public bool IsShowing => animacaoAtual != null;
    public event Action Completed;

    // ============================================================
    // 02. CONSTRUÇÃO DO ANÚNCIO TEMPORÁRIO
    // ============================================================

    public void Construir(WarDominionUITheme novoTema)
    {
        tema = novoTema;

        RectTransform raiz = (RectTransform)transform;
        raiz.anchorMin = Vector2.zero;
        raiz.anchorMax = Vector2.one;
        raiz.offsetMin = Vector2.zero;
        raiz.offsetMax = Vector2.zero;

        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        painel = CriarRect("Painel", raiz);
        painel.anchorMin = new Vector2(0.5f, 0.5f);
        painel.anchorMax = new Vector2(0.5f, 0.5f);
        painel.pivot = new Vector2(0.5f, 0.5f);
        painel.sizeDelta = new Vector2(tema.LarguraAnuncioRound, tema.AlturaAnuncioRound);
        painel.anchoredPosition = new Vector2(0f, tema.PosicaoVerticalAnuncioRound);

        Image superficie = painel.gameObject.AddComponent<Image>();
        superficie.color = tema.SuperficieAnuncio;
        superficie.raycastTarget = false;

        textoRound = CriarTexto(
            "Round", painel, new Vector2(0f, 0.47f), Vector2.one,
            TextAlignmentOptions.Center, tema.FonteDisplay,
            tema.TamanhoRoundAnuncio, tema.TextoPrincipal);

        textoFase = CriarTexto(
            "Fase", painel, Vector2.zero, new Vector2(1f, 0.48f),
            TextAlignmentOptions.Top, tema.FonteInterface,
            tema.TamanhoFaseAnuncio, tema.TextoSecundario);

        linhaBase = CriarRect("LinhaTecnica", painel);
        linhaBase.anchorMin = new Vector2(0.5f, 0f);
        linhaBase.anchorMax = new Vector2(0.5f, 0f);
        linhaBase.pivot = new Vector2(0.5f, 0.5f);
        linhaBase.sizeDelta = new Vector2(
            tema.LarguraLinhaAnuncio,
            tema.EspessuraLinhaAnuncio);
        linhaBase.anchoredPosition = new Vector2(0f, tema.MargemLinhaAnuncio);

        Image linha = linhaBase.gameObject.AddComponent<Image>();
        linha.color = tema.CorLinhaAnuncio;
        linha.raycastTarget = false;

        segmentoEnergia = CriarRect("SegmentoEnergia", linhaBase);
        segmentoEnergia.anchorMin = new Vector2(0.5f, 0.5f);
        segmentoEnergia.anchorMax = new Vector2(0.5f, 0.5f);
        segmentoEnergia.pivot = new Vector2(0.5f, 0.5f);
        segmentoEnergia.sizeDelta = new Vector2(
            tema.LarguraSegmentoAnuncio,
            tema.EspessuraSegmentoAnuncio);

        Image segmento = segmentoEnergia.gameObject.AddComponent<Image>();
        segmento.color = tema.Acento;
        segmento.raycastTarget = false;

        gameObject.SetActive(false);
    }

    // ============================================================
    // 03. API REUTILIZÁVEL DE APRESENTAÇÃO
    // ============================================================

    public void ExibirResolucao(MatchUIState state)
    {
        if (state == null || tema == null)
            return;

        textoRound.text = $"ROUND {Mathf.Max(1, state.Round):00}";
        textoFase.text = "RESOLUÇÃO";

        if (animacaoAtual != null)
            StopCoroutine(animacaoAtual);

        gameObject.SetActive(true);
        animacaoAtual = StartCoroutine(Animar());
    }

    public void ExibirInicioRound(int round)
    {
        if (tema == null)
            return;
        textoRound.text = $"ROUND {Mathf.Max(1, round):00}";
        textoFase.text = "PREPARAÇÃO";
        if (animacaoAtual != null)
            StopCoroutine(animacaoAtual);
        gameObject.SetActive(true);
        animacaoAtual = StartCoroutine(Animar());
    }

    // ============================================================
    // 04. ANIMAÇÃO PROCEDURAL DE ENTRADA, PAUSA E SAÍDA
    // ============================================================

    private IEnumerator Animar()
    {
        Vector2 posicaoBase = new Vector2(0f, tema.PosicaoVerticalAnuncioRound);
        Vector2 posicaoEntrada = posicaoBase + new Vector2(-tema.DeslocamentoAnuncio, 0f);
        Vector2 posicaoSaida = posicaoBase + new Vector2(tema.DeslocamentoAnuncio, 0f);

        canvasGroup.alpha = 0f;
        painel.anchoredPosition = posicaoEntrada;
        painel.localScale = Vector3.one * tema.EscalaInicialAnuncio;
        linhaBase.localScale = new Vector3(0f, 1f, 1f);

        yield return AnimarTrecho(
            tema.DuracaoEntradaAnuncio,
            progresso =>
            {
                float suave = SuavizarSaida(progresso);
                canvasGroup.alpha = suave;
                painel.anchoredPosition = Vector2.LerpUnclamped(posicaoEntrada, posicaoBase, suave);
                painel.localScale = Vector3.one * Mathf.LerpUnclamped(
                    tema.EscalaInicialAnuncio, 1f, suave);
                linhaBase.localScale = new Vector3(suave, 1f, 1f);
                AtualizarSegmento(progresso);
            });

        yield return AnimarTrecho(
            tema.DuracaoVisivelAnuncio,
            AtualizarSegmento);

        yield return AnimarTrecho(
            tema.DuracaoSaidaAnuncio,
            progresso =>
            {
                float suave = SuavizarEntrada(progresso);
                canvasGroup.alpha = 1f - suave;
                painel.anchoredPosition = Vector2.LerpUnclamped(posicaoBase, posicaoSaida, suave);
                painel.localScale = Vector3.one * Mathf.LerpUnclamped(
                    1f, tema.EscalaFinalAnuncio, suave);
                AtualizarSegmento(progresso);
            });

        canvasGroup.alpha = 0f;
        animacaoAtual = null;
        Completed?.Invoke();
        gameObject.SetActive(false);
    }

    private IEnumerator AnimarTrecho(float duracao, System.Action<float> atualizar)
    {
        float decorrido = 0f;
        float duracaoSegura = Mathf.Max(0.01f, duracao);

        while (decorrido < duracaoSegura)
        {
            decorrido += Time.unscaledDeltaTime;
            atualizar(Mathf.Clamp01(decorrido / duracaoSegura));
            yield return null;
        }

        atualizar(1f);
    }

    private void AtualizarSegmento(float progresso)
    {
        float percurso = tema.LarguraLinhaAnuncio + tema.LarguraSegmentoAnuncio;
        float x = Mathf.Lerp(-percurso * 0.5f, percurso * 0.5f, Mathf.Repeat(progresso, 1f));
        segmentoEnergia.anchoredPosition = new Vector2(x, 0f);
    }

    private static float SuavizarSaida(float valor) =>
        1f - Mathf.Pow(1f - valor, 3f);

    private static float SuavizarEntrada(float valor) =>
        valor * valor * valor;

    // ============================================================
    // 05. FÁBRICA LOCAL SEM CAPTURA DE RAYCAST
    // ============================================================

    private static RectTransform CriarRect(string nome, Transform pai)
    {
        var objeto = new GameObject(nome, typeof(RectTransform));
        RectTransform rect = objeto.GetComponent<RectTransform>();
        rect.SetParent(pai, false);
        return rect;
    }

    private static TextMeshProUGUI CriarTexto(
        string nome,
        Transform pai,
        Vector2 ancoraMin,
        Vector2 ancoraMax,
        TextAlignmentOptions alinhamento,
        TMP_FontAsset fonte,
        float tamanho,
        Color cor)
    {
        RectTransform rect = CriarRect(nome, pai);
        rect.anchorMin = ancoraMin;
        rect.anchorMax = ancoraMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TextMeshProUGUI texto = rect.gameObject.AddComponent<TextMeshProUGUI>();
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
}
