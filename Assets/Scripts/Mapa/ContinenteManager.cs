using UnityEngine;

public class ContinenteManager : MonoBehaviour
{
    public static ContinenteManager instance;

    void Awake()
    {
        instance = this;
    }

    public int ObterBonus(TerritorioClique.Continente continente)
    {
        switch (continente)
        {
            case TerritorioClique.Continente.AmericaDoNorte:
                return 5;

            case TerritorioClique.Continente.AmericaDoSul:
                return 2;

            case TerritorioClique.Continente.Africa:
                return 3;

            case TerritorioClique.Continente.Europa:
                return 5;

            case TerritorioClique.Continente.Asia:
                return 7;

            case TerritorioClique.Continente.Oceania:
                return 2;

            default:
                return 0;
        }
    }

    public string ObterChaveTraducao(
        TerritorioClique.Continente continente)
    {
        switch (continente)
        {
            case TerritorioClique.Continente.AmericaDoNorte:
                return "continent_north_america";

            case TerritorioClique.Continente.AmericaDoSul:
                return "continent_south_america";

            case TerritorioClique.Continente.Africa:
                return "continent_africa";

            case TerritorioClique.Continente.Europa:
                return "continent_europe";

            case TerritorioClique.Continente.Asia:
                return "continent_asia";

            case TerritorioClique.Continente.Oceania:
                return "continent_oceania";

            default:
                return "";
        }
    }

// =====================================================
// DOMÍNIO E BÔNUS DE CONTINENTES
// =====================================================

public bool JogadorControlaContinente(
    TerritorioClique.Dono jogador,
    TerritorioClique.Continente continente)
{
    GameObject pai = GameObject.Find("Territorios");

    if (pai == null)
        return false;

    TerritorioClique[] territorios =
        pai.GetComponentsInChildren<TerritorioClique>(false);

    bool encontrouTerritorioDoContinente = false;

    foreach (TerritorioClique territorio in territorios)
    {
        if (territorio.continente != continente)
            continue;

        encontrouTerritorioDoContinente = true;

        if (territorio.dono != jogador)
            return false;
    }

    return encontrouTerritorioDoContinente;
}

public int CalcularBonusContinentes(
    TerritorioClique.Dono jogador)
{
    int bonusTotal = 0;

    foreach (
        TerritorioClique.Continente continente
        in System.Enum.GetValues(
            typeof(TerritorioClique.Continente)
        )
    )
    {
        if (JogadorControlaContinente(
            jogador,
            continente))
        {
            bonusTotal += ObterBonus(continente);
        }
    }

    return bonusTotal;
}

    public void MostrarContinente(
        TerritorioClique.Continente continente)
    {
        TerritorioClique[] territorios =
    GameObject.Find("Territorios")
        .GetComponentsInChildren<TerritorioClique>(false);

        Color corDestaque =
            ObterCorContinente(continente);

        foreach (TerritorioClique territorio in territorios)
        {
            if (territorio.continente == continente)
            {
                territorio.DestacarContinente(
                    corDestaque
                );
            }
            else
            {
                // Deixa os outros discretos
                territorio.DestacarContinente(
                    new Color(
                        0.25f,
                        0.25f,
                        0.25f,
                        1f
                    )
                );
            }
        }

        Debug.Log(
            continente +
            " | Bônus: +" +
            ObterBonus(continente)
        );
    }

    public void OcultarDestaque()
    {
        TerritorioClique[] territorios =
    GameObject.Find("Territorios")
        .GetComponentsInChildren<TerritorioClique>(false);

        foreach (TerritorioClique territorio in territorios)
        {
            territorio.RestaurarCor();
        }
    }

    private Color ObterCorContinente(
        TerritorioClique.Continente continente)
    {
        switch (continente)
        {
            case TerritorioClique.Continente.AmericaDoNorte:
                return new Color(0.25f, 0.65f, 1f);

            case TerritorioClique.Continente.AmericaDoSul:
                return new Color(0.3f, 0.85f, 0.35f);

            case TerritorioClique.Continente.Africa:
                return new Color(1f, 0.65f, 0.2f);

            case TerritorioClique.Continente.Europa:
                return new Color(0.65f, 0.4f, 1f);

            case TerritorioClique.Continente.Asia:
                return new Color(1f, 0.35f, 0.35f);

            case TerritorioClique.Continente.Oceania:
                return new Color(0.2f, 0.85f, 0.85f);

            default:
                return Color.white;
        }
    }
}