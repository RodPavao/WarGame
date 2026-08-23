using UnityEngine;
using TMPro;

[SelectionBase]
public class ContadorTropas : MonoBehaviour
{
    private TerritorioClique territorio;

    private SpriteRenderer fundo;
    private SpriteRenderer borda;
    private TextMeshPro numero;

    public void Configurar(TerritorioClique territorioAlvo)
    {
        territorio = territorioAlvo;

        CriarVisual();
        Atualizar();
    }

    private void CriarVisual()
{
    // Se o visual já existe, reutiliza em vez de duplicar.
    Transform bordaExistente = transform.Find("Borda");
    Transform fundoExistente = transform.Find("Fundo");
    Transform numeroExistente = transform.Find("Numero");

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

        return;
    }

        // =====================================================
        // BORDA
        // =====================================================

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


        // =====================================================
        // FUNDO
        // =====================================================

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

        // Preto profundo
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


        // =====================================================
        // NÚMERO
        // =====================================================

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

        numero.enableAutoSizing =
            false;

        numero.overflowMode =
            TextOverflowModes.Overflow;

        numero.extraPadding =
            true;

        numero.margin =
            Vector4.zero;

        numero.sortingOrder = 22;

        RectTransform rectNumero =
            objNumero.GetComponent<RectTransform>();

        rectNumero.sizeDelta =
            new Vector2(
                0.82f,
                0.82f
            );

        // O objeto SEMPRE fica exatamente no centro.
        rectNumero.localPosition =
            Vector3.zero;
    }


    // =========================================================
    // ATUALIZAR CONTADOR
    // =========================================================

    public void Atualizar()
    {
        if (territorio == null ||
            numero == null)
            return;

        numero.text =
            territorio.tropas.ToString();

        int quantidadeDigitos =
            numero.text.Length;

        if (quantidadeDigitos == 1)
{
    numero.fontSize = 4.0f;
}
else if (quantidadeDigitos == 2)
{
    numero.fontSize = 3.7f;
}
else
{
    numero.fontSize = 3.1f;
}

        numero.ForceMeshUpdate();

        CentralizarMalhaNumero();

        borda.color =
            ObterCorJogador(
                territorio.dono
            );
    }


    // =========================================================
    // CENTRALIZAÇÃO GEOMÉTRICA REAL
    // =========================================================

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

        // Mede somente os pixels/vértices
        // realmente usados pelos algarismos.
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

        // Move somente a geometria das letras.
        // O RectTransform continua parado no centro.
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


    // =========================================================
    // COR DO DONO
    // =========================================================

    private Color ObterCorJogador(
        TerritorioClique.Dono dono)
    {
        switch (dono)
        {
            case TerritorioClique.Dono.Jogador1:
                return new Color(
                    1f,
                    0.22f,
                    0.15f
                );

            case TerritorioClique.Dono.Jogador2:
                return new Color(
                    0.15f,
                    0.45f,
                    1f
                );

            case TerritorioClique.Dono.Jogador3:
                return new Color(
                    0.15f,
                    0.85f,
                    0.35f
                );

            case TerritorioClique.Dono.Jogador4:
                return new Color(
                    0.8f,
                    0.25f,
                    1f
                );

            default:
                return new Color(
                    0.55f,
                    0.58f,
                    0.62f
                );
        }
    }


    // =========================================================
    // SPRITE DO CONTADOR
    // =========================================================

    private Sprite CriarSpriteArredondado()
    {
        const int tamanho = 64;

        // Quadrado com cantos arredondados,
        // sem ficar circular.
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