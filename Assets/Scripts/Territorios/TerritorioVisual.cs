using UnityEngine;

public class TerritorioVisual : MonoBehaviour
{
    private SpriteRenderer sr;
    private TerritorioClique territorio;

    private static Material materialSemLuz;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        territorio = GetComponent<TerritorioClique>();

        GarantirMaterialSemLuz();
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
                    Shader.Find("Sprites/Default");
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
            territorio = GetComponent<TerritorioClique>();

        if (sr == null ||
            territorio == null)
            return;

        GarantirMaterialSemLuz();

        sr.color =
            PaletaJogadores.ObterCorAtiva(
                territorio.dono
            );
    }

    public void DestacarContinente(Color cor)
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        if (sr != null)
            sr.color = cor;
    }

    public void RestaurarCor()
    {
        AtualizarCor();
    }

    public void DestacarSelecao()
    {
    }

    public void RemoverDestaqueSelecao()
    {
        AtualizarCor();
    }
}