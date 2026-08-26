using UnityEngine;
using TMPro;
using System.Collections;

[SelectionBase]
public class ContadorTropas : MonoBehaviour
{
    private TerritorioClique territorio;

    private SpriteRenderer fundo;
    private SpriteRenderer borda;
    private SpriteRenderer bordaExterna;

    private TextMeshPro numero;

    private Vector3 escalaOriginal;
    private Coroutine animacaoAtual;

    // =====================================================
    // TAMANHO VISUAL GLOBAL
    // =====================================================

    // 0.80 = todos os contadores ficam 20% menores.
    // NÃO altera o tamanho da área clicável.
    private const float escalaVisualGlobal = 0.80f;

    // Área clicável maior que o desenho visual.
    private const float tamanhoCollider = 0.82f;

    // =====================================================
    // CLIQUE LONGO
    // =====================================================

    private bool pressionando = false;
    private bool cliqueLongoExecutado = false;

    private float inicioPressao;

    private const float tempoCliqueLongo = 1f;

    // =====================================================
    // EDITOR
    // =====================================================

    private void OnValidate()
    {
        if (Application.isPlaying)
            return;

        territorio =
            GetComponentInParent<TerritorioClique>();

        Transform bordaExistente =
            transform.Find("Borda");

        Transform fundoExistente =
            transform.Find("Fundo");

        Transform numeroExistente =
            transform.Find("Numero");

        if (territorio == null ||
            bordaExistente == null ||
            fundoExistente == null ||
            numeroExistente == null)
            return;

        borda =
            bordaExistente.GetComponent<SpriteRenderer>();

        fundo =
            fundoExistente.GetComponent<SpriteRenderer>();

        numero =
            numeroExistente.GetComponent<TextMeshPro>();

        AplicarEscalaVisual();

        GarantirBordaExterna(
            bordaExistente
        );

        GarantirCollider();

        Atualizar();
    }

    // =====================================================
    // CONFIGURAÇÃO
    // =====================================================

    public void Configurar(
        TerritorioClique territorioAlvo)
    {
        territorio = territorioAlvo;

        CriarVisual();

        AplicarEscalaVisual();

        escalaOriginal =
            transform.localScale;

        Atualizar();
    }

    // =====================================================
    // UPDATE — HOLD 1 SEGUNDO
    // =====================================================

    private void Update()
    {
        if (!pressionando)
            return;

        if (cliqueLongoExecutado)
            return;

        if (GameManager.instance == null)
            return;

        if (GameManager.instance.faseAtual !=
            GameManager.FaseTurno.Reforco)
            return;

        if (Time.unscaledTime - inicioPressao >=
            tempoCliqueLongo)
        {
            cliqueLongoExecutado = true;

            DespejarTodosReforcos();
        }
    }

    // =====================================================
    // VISUAL
    // =====================================================

    private void CriarVisual()
    {
        Transform bordaExistente =
            transform.Find("Borda");

        Transform fundoExistente =
            transform.Find("Fundo");

        Transform numeroExistente =
            transform.Find("Numero");

        if (bordaExistente != null &&
            fundoExistente != null &&
            numeroExistente != null)
        {
            borda =
                bordaExistente.GetComponent<SpriteRenderer>();

            fundo =
                fundoExistente.GetComponent<SpriteRenderer>();

            numero =
                numeroExistente.GetComponent<TextMeshPro>();

            GarantirBordaExterna(
                bordaExistente
            );

            GarantirCollider();

            return;
        }

        // =================================================
        // BORDA INTERNA
        // =================================================

        GameObject objBorda =
            new GameObject("Borda");

        objBorda.transform.SetParent(
            transform,
            false
        );

        borda =
            objBorda.AddComponent<SpriteRenderer>();

        borda.sprite =
            CriarSpriteArredondado();

        borda.sortingOrder = 20;

        objBorda.transform.localScale =
            new Vector3(
                0.82f,
                0.82f,
                1f
            );

        GarantirBordaExterna(
            objBorda.transform
        );

        // =================================================
        // FUNDO
        // =================================================

        GameObject objFundo =
            new GameObject("Fundo");

        objFundo.transform.SetParent(
            transform,
            false
        );

        fundo =
            objFundo.AddComponent<SpriteRenderer>();

        fundo.sprite =
            CriarSpriteArredondado();

        fundo.color =
            new Color(
                0.012f,
                0.012f,
                0.015f,
                1f
            );

        fundo.sortingOrder = 21;

        objFundo.transform.localScale =
            new Vector3(
                0.68f,
                0.68f,
                1f
            );

        // =================================================
        // NÚMERO
        // =================================================

        GameObject objNumero =
            new GameObject("Numero");

        objNumero.transform.SetParent(
            transform,
            false
        );

        numero =
            objNumero.AddComponent<TextMeshPro>();

        TMP_FontAsset fonteNumero =
            Resources.Load<TMP_FontAsset>(
                "Fonts & Materials/BebasNeue-Regular SDF"
            );

        if (fonteNumero != null)
        {
            numero.font = fonteNumero;
        }
        else
        {
            Debug.LogError(
                "Fonte Bebas Neue SDF não encontrada."
            );
        }

        numero.fontStyle =
            FontStyles.Normal;

        numero.fontWeight =
            FontWeight.Medium;

        numero.color =
            Color.white;

        numero.alignment =
            TextAlignmentOptions.Center;

        numero.verticalAlignment =
            VerticalAlignmentOptions.Middle;

        numero.enableAutoSizing = false;

        numero.overflowMode =
            TextOverflowModes.Overflow;

        numero.extraPadding = true;
        numero.margin = Vector4.zero;
        numero.sortingOrder = 22;

        RectTransform rectNumero =
            objNumero.GetComponent<RectTransform>();

        rectNumero.sizeDelta =
            new Vector2(
                0.82f,
                0.82f
            );

        rectNumero.localPosition =
            Vector3.zero;

        GarantirCollider();
    }

    // =====================================================
    // ESCALA VISUAL GLOBAL
    // =====================================================

    private void AplicarEscalaVisual()
    {
        Transform bordaTransform =
            transform.Find("Borda");

        Transform fundoTransform =
            transform.Find("Fundo");

        Transform numeroTransform =
            transform.Find("Numero");

        if (bordaTransform != null)
        {
            bordaTransform.localScale =
                new Vector3(
                    0.82f * escalaVisualGlobal,
                    0.82f * escalaVisualGlobal,
                    1f
                );
        }

        if (fundoTransform != null)
        {
            fundoTransform.localScale =
                new Vector3(
                    0.68f * escalaVisualGlobal,
                    0.68f * escalaVisualGlobal,
                    1f
                );
        }

        if (numeroTransform != null)
        {
            numeroTransform.localScale =
                new Vector3(
                    escalaVisualGlobal,
                    escalaVisualGlobal,
                    1f
                );
        }

        if (bordaTransform != null)
        {
            GarantirBordaExterna(
                bordaTransform
            );
        }
    }

    // =====================================================
    // BORDA EXTERNA
    // =====================================================

    private void GarantirBordaExterna(
        Transform bordaTransform)
    {
        if (bordaTransform == null)
            return;

        SpriteRenderer bordaAtual =
            bordaTransform.GetComponent<SpriteRenderer>();

        if (bordaAtual == null)
            return;

        Transform existente =
            transform.Find("BordaExterna");

        GameObject obj;

        if (existente == null)
        {
            obj =
                new GameObject(
                    "BordaExterna"
                );

            obj.transform.SetParent(
                transform,
                false
            );
        }
        else
        {
            obj = existente.gameObject;
        }

        bordaExterna =
            obj.GetComponent<SpriteRenderer>();

        if (bordaExterna == null)
        {
            bordaExterna =
                obj.AddComponent<SpriteRenderer>();
        }

        bordaExterna.sprite =
            bordaAtual.sprite;

        bordaExterna.color =
            Color.black;

        bordaExterna.sortingOrder =
            bordaAtual.sortingOrder - 1;

        obj.transform.localPosition =
            bordaTransform.localPosition;

        obj.transform.localRotation =
            bordaTransform.localRotation;

        Vector3 escalaBorda =
            bordaTransform.localScale;

        obj.transform.localScale =
            new Vector3(
                escalaBorda.x * 1.10f,
                escalaBorda.y * 1.10f,
                escalaBorda.z
            );
    }

    // =====================================================
    // COLLIDER
    // =====================================================

    private void GarantirCollider()
    {
        BoxCollider2D colliderContador =
            GetComponent<BoxCollider2D>();

        if (colliderContador == null)
        {
            colliderContador =
                gameObject.AddComponent<BoxCollider2D>();
        }

        colliderContador.size =
            new Vector2(
                tamanhoCollider,
                tamanhoCollider
            );

        colliderContador.offset =
            Vector2.zero;
    }

    // =====================================================
    // ATUALIZAÇÃO
    // =====================================================

    public void Atualizar()
    {
        if (territorio == null ||
            numero == null ||
            borda == null)
            return;

        numero.text =
            territorio.Tropas.ToString();

        int quantidadeDigitos =
            numero.text.Length;

        if (quantidadeDigitos == 1)
            numero.fontSize = 4.0f;
        else if (quantidadeDigitos == 2)
            numero.fontSize = 3.7f;
        else
            numero.fontSize = 3.1f;

        numero.ForceMeshUpdate();

        CentralizarMalhaNumero();

        borda.color =
            PaletaJogadores.ObterCorAtiva(
                territorio.dono
            );

        if (bordaExterna != null)
        {
            bordaExterna.color =
                Color.black;
        }
    }

    // =====================================================
    // INPUT CENTRALIZADO
    // =====================================================
    //
    // IMPORTANTE:
    // OnMouseDown / OnMouseUp / OnMouseExit foram removidos.
    //
    // Agora TODO clique/touch é detectado pelo InputPartida
    // e encaminhado diretamente para estes métodos.
    // =====================================================

    public void IniciarPressaoInput()
    {
        if (territorio == null ||
            GameManager.instance == null)
            return;

        pressionando = true;

        cliqueLongoExecutado = false;

        inicioPressao =
            Time.unscaledTime;
    }

    public void FinalizarPressaoInput()
    {
        if (!pressionando)
            return;

        pressionando = false;

        if (territorio == null ||
            GameManager.instance == null)
            return;

        // Hold de 1 segundo já executou.
        // Soltar não pode executar clique curto também.
        if (cliqueLongoExecutado)
        {
            cliqueLongoExecutado = false;
            return;
        }

        ExecutarCliqueNormal();
    }

    public void CancelarPressaoInput()
    {
        pressionando = false;

        cliqueLongoExecutado = false;
    }

    // =====================================================
    // CLIQUE NORMAL
    // =====================================================

    private void ExecutarCliqueNormal()
    {
        GameManager gm =
            GameManager.instance;

        if (gm == null ||
            territorio == null)
            return;

        // =================================================
        // REFORÇO
        // =================================================

        if (gm.faseAtual ==
            GameManager.FaseTurno.Reforco)
        {
            int tropasAntes =
                territorio.Tropas;

            gm.TentarAdicionarReforco(
                territorio
            );

            if (territorio.Tropas >
                tropasAntes)
            {
                AnimarAdicao();
            }

            return;
        }

        // =================================================
        // PREPARAÇÃO / ATAQUE
        // =================================================

        if (gm.faseAtual ==
            GameManager.FaseTurno.Ataque)
        {
            gm.ClicarTerritorio(
                territorio
            );
        }
    }

    // Mantido por compatibilidade com possíveis
    // componentes/scripts antigos.
    public void Clicar()
    {
        ExecutarCliqueNormal();
    }

    // =====================================================
    // HOLD 1 SEGUNDO
    // =====================================================

    private void DespejarTodosReforcos()
    {
        GameManager gm =
            GameManager.instance;

        if (gm == null ||
            territorio == null)
            return;

        if (gm.faseAtual !=
            GameManager.FaseTurno.Reforco)
            return;

        // Durante o modo de diagnóstico do GameManager,
        // a própria regra de TentarAdicionarReforco()
        // decide se qualquer território pode receber tropas.
        //
        // Fora do diagnóstico, mantém a regra normal.
        if (!gm.modoTesteCliquesTodosTerritorios &&
            territorio.dono != gm.jogadorLocal)
        {
            return;
        }

        int quantidade =
            gm.reforcosDisponiveis;

        if (quantidade <= 0)
            return;

        for (int i = 0;
             i < quantidade;
             i++)
        {
            gm.TentarAdicionarReforco(
                territorio
            );
        }

        Atualizar();

        AnimarAdicao();

        Debug.Log(
            "REFORÇO RÁPIDO | " +
            territorio.name +
            " recebeu " +
            quantidade +
            " tropa(s)."
        );
    }

    // =====================================================
    // ANIMAÇÃO
    // =====================================================

    private void AnimarAdicao()
    {
        if (animacaoAtual != null)
        {
            StopCoroutine(
                animacaoAtual
            );
        }

        animacaoAtual =
            StartCoroutine(
                AnimacaoAdicao()
            );
    }

    private IEnumerator AnimacaoAdicao()
    {
        float duracao = 0.16f;
        float tempo = 0f;

        Vector3 escalaMaior =
            escalaOriginal * 1.14f;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;

            float progresso =
                tempo / duracao;

            float curva =
                Mathf.Sin(
                    progresso * Mathf.PI
                );

            transform.localScale =
                Vector3.Lerp(
                    escalaOriginal,
                    escalaMaior,
                    curva
                );

            yield return null;
        }

        transform.localScale =
            escalaOriginal;

        animacaoAtual = null;
    }

    // =====================================================
    // CENTRALIZAÇÃO DO NÚMERO
    // =====================================================

    private void CentralizarMalhaNumero()
    {
        TMP_TextInfo info =
            numero.textInfo;

        if (info == null ||
            info.characterCount == 0)
            return;

        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        bool encontrou = false;

        for (int i = 0;
             i < info.characterCount;
             i++)
        {
            TMP_CharacterInfo caractere =
                info.characterInfo[i];

            if (!caractere.isVisible)
                continue;

            encontrou = true;

            int indiceVertice =
                caractere.vertexIndex;

            int indiceMaterial =
                caractere.materialReferenceIndex;

            Vector3[] vertices =
                info.meshInfo[
                    indiceMaterial
                ].vertices;

            for (int j = 0; j < 4; j++)
            {
                Vector3 vertice =
                    vertices[
                        indiceVertice + j
                    ];

                minX =
                    Mathf.Min(
                        minX,
                        vertice.x
                    );

                maxX =
                    Mathf.Max(
                        maxX,
                        vertice.x
                    );

                minY =
                    Mathf.Min(
                        minY,
                        vertice.y
                    );

                maxY =
                    Mathf.Max(
                        maxY,
                        vertice.y
                    );
            }
        }

        if (!encontrou)
            return;

        Vector3 centro =
            new Vector3(
                (minX + maxX) / 2f,
                (minY + maxY) / 2f,
                0f
            );

        for (int i = 0;
             i < info.characterCount;
             i++)
        {
            TMP_CharacterInfo caractere =
                info.characterInfo[i];

            if (!caractere.isVisible)
                continue;

            int indiceVertice =
                caractere.vertexIndex;

            int indiceMaterial =
                caractere.materialReferenceIndex;

            Vector3[] vertices =
                info.meshInfo[
                    indiceMaterial
                ].vertices;

            for (int j = 0; j < 4; j++)
            {
                vertices[
                    indiceVertice + j
                ] -= centro;
            }
        }

        numero.UpdateVertexData(
            TMP_VertexDataUpdateFlags.Vertices
        );
    }

    // =====================================================
    // SPRITE DO CONTADOR
    // =====================================================

    private Sprite CriarSpriteArredondado()
    {
        const int tamanho = 64;
        const float raio = 19f;

        Texture2D textura =
            new Texture2D(
                tamanho,
                tamanho
            );

        textura.filterMode =
            FilterMode.Bilinear;

        Color transparente =
            new Color(
                0f,
                0f,
                0f,
                0f
            );

        for (int y = 0;
             y < tamanho;
             y++)
        {
            for (int x = 0;
                 x < tamanho;
                 x++)
            {
                float px =
                    Mathf.Max(
                        Mathf.Abs(
                            x - tamanho / 2f
                        ) -
                        (
                            tamanho / 2f -
                            raio
                        ),
                        0f
                    );

                float py =
                    Mathf.Max(
                        Mathf.Abs(
                            y - tamanho / 2f
                        ) -
                        (
                            tamanho / 2f -
                            raio
                        ),
                        0f
                    );

                bool dentro =
                    px * px +
                    py * py
                    <= raio * raio;

                textura.SetPixel(
                    x,
                    y,
                    dentro
                        ? Color.white
                        : transparente
                );
            }
        }

        textura.Apply();

        return Sprite.Create(
            textura,
            new Rect(
                0,
                0,
                tamanho,
                tamanho
            ),
            new Vector2(
                0.5f,
                0.5f
            ),
            100f
        );
    }
}