using UnityEngine;
using TMPro;

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
            bordaExistente
                .GetComponent<SpriteRenderer>();

        fundo =
            fundoExistente
                .GetComponent<SpriteRenderer>();

        numero =
            numeroExistente
                .GetComponent<TextMeshPro>();

        // NÃO mexe na posição de nada.
        Atualizar();
    }

    // =====================================================
    // CONFIGURAÇÃO
    // =====================================================

    public void Configurar(
        TerritorioClique territorioAlvo)
    {
        territorio =
            territorioAlvo;

        // NÃO altera transform.localPosition.
        CriarVisual();

        escalaOriginal =
            transform.localScale;

        Atualizar();
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

        // =================================================
        // CONTADOR JÁ EXISTE
        // =================================================

        if (bordaExistente != null &&
            fundoExistente != null &&
            numeroExistente != null)
        {
            borda =
                bordaExistente
                    .GetComponent<SpriteRenderer>();

            fundo =
                fundoExistente
                    .GetComponent<SpriteRenderer>();

            numero =
                numeroExistente
                    .GetComponent<TextMeshPro>();

            // Cria/reutiliza somente a borda externa.
            // Copia exatamente a posição da borda existente.
            GarantirBordaExterna(
                bordaExistente
            );

            GarantirCollider();

            return;
        }

        // =================================================
        // BORDA INTERNA / SKIN
        // =================================================

        GameObject objBorda =
            new GameObject("Borda");

        objBorda.transform.SetParent(
            transform,
            false
        );

        borda =
            objBorda
                .AddComponent<SpriteRenderer>();

        borda.sprite =
            CriarSpriteArredondado();

        borda.sortingOrder =
            20;

        objBorda.transform.localScale =
            new Vector3(
                0.82f,
                0.82f,
                1f
            );

        // Cria a borda preta atrás dela.
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
            objFundo
                .AddComponent<SpriteRenderer>();

        fundo.sprite =
            CriarSpriteArredondado();

        fundo.color =
            new Color(
                0.012f,
                0.012f,
                0.015f,
                1f
            );

        fundo.sortingOrder =
            21;

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
            objNumero
                .AddComponent<TextMeshPro>();

        TMP_FontAsset fonteNumero =
            Resources.Load<TMP_FontAsset>(
                "Fonts & Materials/BebasNeue-Regular SDF"
            );

        if (fonteNumero != null)
        {
            numero.font =
                fonteNumero;
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

        numero.enableAutoSizing =
            false;

        numero.overflowMode =
            TextOverflowModes.Overflow;

        numero.extraPadding =
            true;

        numero.margin =
            Vector4.zero;

        numero.sortingOrder =
            22;

        RectTransform rectNumero =
            objNumero
                .GetComponent<RectTransform>();

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
    // BORDA EXTERNA PRETA
    // =====================================================

    private void GarantirBordaExterna(
        Transform bordaTransform)
    {
        if (bordaTransform == null)
            return;

        SpriteRenderer bordaAtual =
            bordaTransform
                .GetComponent<SpriteRenderer>();

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
            obj =
                existente.gameObject;
        }

        bordaExterna =
            obj.GetComponent<SpriteRenderer>();

        if (bordaExterna == null)
        {
            bordaExterna =
                obj.AddComponent<SpriteRenderer>();
        }

        // Mesma forma da borda interna.
        bordaExterna.sprite =
            bordaAtual.sprite;

        bordaExterna.color =
            Color.black;

        bordaExterna.sortingOrder =
            bordaAtual.sortingOrder - 1;

        // IMPORTANTE:
        // segue a borda.
        // Não força nenhum contador a voltar para 0,0.
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
                gameObject
                    .AddComponent<BoxCollider2D>();
        }

        colliderContador.size =
            new Vector2(
                0.55f,
                0.55f
            );
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
        {
            numero.fontSize =
                4.0f;
        }
        else if (quantidadeDigitos == 2)
        {
            numero.fontSize =
                3.7f;
        }
        else
        {
            numero.fontSize =
                3.1f;
        }

        numero.ForceMeshUpdate();

        CentralizarMalhaNumero();

        // =================================================
        // REGRA FUNDAMENTAL:
        //
        // O contador NÃO escolhe cor.
        // Ele apenas copia a skin do dono do território.
        // =================================================

        borda.color =
            PaletaJogadores
                .ObterCorAtiva(
                    territorio.dono
                );

        if (bordaExterna != null)
        {
            bordaExterna.color =
                Color.black;
        }
    }

    // =====================================================
    // CLIQUE
    // =====================================================

    public void Clicar()
    {
        if (territorio == null)
            return;

        if (GameManager.instance == null)
            return;

        int tropasAntes =
            territorio.Tropas;

        GameManager.instance
            .TentarAdicionarReforco(
                territorio
            );

        if (territorio.Tropas >
            tropasAntes)
        {
            AnimarAdicao();
        }
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

    private System.Collections.IEnumerator
        AnimacaoAdicao()
    {
        float duracao =
            0.16f;

        float tempo =
            0f;

        Vector3 escalaMaior =
            escalaOriginal *
            1.14f;

        while (tempo < duracao)
        {
            tempo +=
                Time.deltaTime;

            float progresso =
                tempo /
                duracao;

            float curva =
                Mathf.Sin(
                    progresso *
                    Mathf.PI
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

        animacaoAtual =
            null;
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

        float minX =
            float.MaxValue;

        float maxX =
            float.MinValue;

        float minY =
            float.MaxValue;

        float maxY =
            float.MinValue;

        bool encontrou =
            false;

        for (int i = 0;
             i < info.characterCount;
             i++)
        {
            TMP_CharacterInfo caractere =
                info.characterInfo[i];

            if (!caractere.isVisible)
                continue;

            encontrou =
                true;

            int indiceVertice =
                caractere.vertexIndex;

            int indiceMaterial =
                caractere.materialReferenceIndex;

            Vector3[] vertices =
                info.meshInfo[
                    indiceMaterial
                ].vertices;

            for (int j = 0;
                 j < 4;
                 j++)
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

            for (int j = 0;
                 j < 4;
                 j++)
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
    // SPRITE ARREDONDADO
    // =====================================================

    private Sprite CriarSpriteArredondado()
    {
        const int tamanho =
            64;

        const float raio =
            19f;

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
                            x -
                            tamanho / 2f
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
                            y -
                            tamanho / 2f
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