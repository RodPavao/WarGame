using UnityEngine;

public class TerritorioClique : MonoBehaviour
{
    private SpriteRenderer sr;

    public enum Dono
    {
        Neutro,
        Jogador1,
        Jogador2,
        Jogador3,
        Jogador4
    }

    public Dono dono = Dono.Neutro;

    public TerritorioClique[] vizinhos;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        AtualizarCor();
    }

    void OnMouseDown()
    {
        GameManager.instance.ClicarTerritorio(this);
    }

    public void AtualizarCor()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();

        switch (dono)
        {
            case Dono.Jogador1:
                sr.color = Color.red;
                break;
            case Dono.Jogador2:
                sr.color = Color.blue;
                break;
            case Dono.Jogador3:
                sr.color = Color.green;
                break;
            case Dono.Jogador4:
                sr.color = Color.magenta;
                break;
            default:
                sr.color = new Color(1f, 1f, 1f, 0.35f);
                break;
        }
    }

    public void DestacarSelecao()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        sr.color = Color.yellow;
    }
}