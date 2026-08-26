using UnityEngine;

public class TerritorioVisual : MonoBehaviour
{
    private SpriteRenderer sr;
    private TerritorioClique territorio;

    private static Material materialSemLuz;

    // ==============================================
    // SELEÇÃO VISUAL
    // ==============================================

    private bool selecionado = false;

    [Header("Seleção")]
    [SerializeField]
    private float velocidadePulso = 1.5f;

    [SerializeField]
    [Range(0.05f, 0.45f)]
    private float intensidadePulso = 0.30f;

    private Color corBase;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        territorio = GetComponent<TerritorioClique>();

        GarantirMaterialSemLuz();
    }

    private void Update()
    {
        if (!selecionado ||
            sr == null)
            return;

        // Pulso muito lento:
        // 0 -> 1 -> 0.
        float pulso =
            (
                Mathf.Sin(
                    Time.time *
                    velocidadePulso
                ) +
                1f
            ) * 0.5f;

        // Clareia suavemente a própria skin.
        Color corClara =
            Color.Lerp(
                corBase,
                Color.white,
                intensidadePulso
            );

        sr.color =
            Color.Lerp(
                corBase,
                corClara,
                pulso
            );
    }

    private void GarantirMaterialSemLuz()
    {
        if (sr == null)
            return;

        if (materialSemLuz == null)
        {
            Shader shader =
                Shader.Find(
                    "Universal Render Pipeline/2D/Sprite-Unlit-Default"
                );

            if (shader == null)
            {
                shader =
                    Shader.Find(
                        "Sprites/Default"
                    );
            }

            if (shader != null)
            {
                materialSemLuz =
                    new Material(shader);
            }
        }

        if (materialSemLuz != null)
        {
            sr.sharedMaterial =
                materialSemLuz;
        }
    }

    public void AtualizarCor()
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        if (territorio == null)
            territorio =
                GetComponent<TerritorioClique>();

        if (sr == null ||
            territorio == null)
            return;

        GarantirMaterialSemLuz();

        corBase =
            PaletaJogadores.ObterCorAtiva(
                territorio.dono
            );

        corBase.a = 1f;

        if (!selecionado)
        {
            sr.color = corBase;
        }
    }

    public void DestacarContinente(
        Color cor)
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        if (sr != null)
            sr.color = cor;
    }

    public void RestaurarCor()
    {
        selecionado = false;

        AtualizarCor();
    }

    public void DestacarSelecao()
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        if (territorio == null)
            territorio =
                GetComponent<TerritorioClique>();

        if (sr == null ||
            territorio == null)
            return;

        corBase =
            PaletaJogadores.ObterCorAtiva(
                territorio.dono
            );

        corBase.a = 1f;

        selecionado = true;
    }

    public void RemoverDestaqueSelecao()
    {
        selecionado = false;

        AtualizarCor();
    }
}