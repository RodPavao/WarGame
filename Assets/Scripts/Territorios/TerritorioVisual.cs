using UnityEngine;

public class TerritorioVisual : MonoBehaviour
{
    private SpriteRenderer sr;
    private TerritorioClique territorio;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        territorio = GetComponent<TerritorioClique>();
    }

    public void AtualizarCor()
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        if (territorio == null)
            territorio = GetComponent<TerritorioClique>();

        if (territorio == null)
            return;

        switch (territorio.dono)
        {
            case TerritorioClique.Dono.Jogador1:
                sr.color = Color.red;
                break;

            case TerritorioClique.Dono.Jogador2:
                sr.color = Color.blue;
                break;

            case TerritorioClique.Dono.Jogador3:
                sr.color = Color.green;
                break;

            case TerritorioClique.Dono.Jogador4:
                sr.color = Color.magenta;
                break;

            default:
                sr.color = Color.white;
                break;
        }
    }

    public void DestacarContinente(Color cor)
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        sr.color = cor;
    }

    public void RestaurarCor()
    {
        AtualizarCor();
    }

    public void DestacarSelecao()
    {
        // Seleção visual desativada por enquanto.
        // O território mantém sempre a cor/skin do dono.
    }

    public void RemoverDestaqueSelecao()
    {
        AtualizarCor();
    }
}