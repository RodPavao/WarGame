using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public TerritorioClique territorioSelecionado;

    public TerritorioClique.Dono jogadorLocal =
        TerritorioClique.Dono.Jogador1;

public enum FaseTurno
{
    Reforco,
    Ataque
}

public FaseTurno faseAtual = FaseTurno.Reforco;

public int reforcosDisponiveis = 8;

    void Awake()
    {
        instance = this;
    }

public void TentarAdicionarReforco(TerritorioClique t)
{
    if (faseAtual != FaseTurno.Reforco)
        return;

    if (t == null)
        return;

    if (t.dono != jogadorLocal)
    {
        Debug.Log(
            "Reforço impossível: território não pertence ao jogador."
        );

        return;
    }

    if (reforcosDisponiveis <= 0)
    {
        Debug.Log(
            "Não há mais tropas disponíveis para distribuir."
        );

        return;
    }

    t.AdicionarTropa();

    reforcosDisponiveis--;

    Debug.Log(
        "Tropa adicionada em " +
        t.name +
        " | Tropas no território: " +
        t.tropas +
        " | Reforços restantes: " +
        reforcosDisponiveis
    );
}

    public void ClicarTerritorio(TerritorioClique t)
    {
        if (faseAtual == FaseTurno.Reforco)
{
    TentarAdicionarReforco(t);
    return;
}
        // Nenhum território selecionado
        if (territorioSelecionado == null)
        {
            if (t.dono != jogadorLocal)
            {
                Debug.Log("Este território não pertence ao jogador.");
                return;
            }

            territorioSelecionado = t;
            territorioSelecionado.DestacarSelecao();

            Debug.Log(
                "Território selecionado: " +
                t.name +
                " | Tropas: " +
                t.tropas
            );

            return;
        }

        // Clicou novamente no território selecionado
        if (t == territorioSelecionado)
        {
            territorioSelecionado.AtualizarCor();
            territorioSelecionado = null;

            Debug.Log("Seleção cancelada.");
            return;
        }

        // Selecionou outro território próprio
        if (t.dono == jogadorLocal)
        {
            territorioSelecionado.AtualizarCor();

            territorioSelecionado = t;
            territorioSelecionado.DestacarSelecao();

            Debug.Log(
                "Novo território selecionado: " +
                t.name +
                " | Tropas: " +
                t.tropas
            );

            return;
        }

        // Tentativa de ataque a território não vizinho
        if (!territorioSelecionado.EhVizinho(t))
        {
            Debug.Log(
                "Ataque impossível: " +
                t.name +
                " não é vizinho de " +
                territorioSelecionado.name
            );

            return;
        }

        // Precisa deixar pelo menos uma tropa no território
        if (territorioSelecionado.tropas <= 1)
        {
            Debug.Log(
                "Ataque impossível: " +
                territorioSelecionado.name +
                " possui apenas " +
                territorioSelecionado.tropas +
                " tropa."
            );

            return;
        }

        Debug.Log(
            "ATAQUE VÁLIDO: " +
            territorioSelecionado.name +
            " -> " +
            t.name
        );

        territorioSelecionado.AtualizarCor();
        territorioSelecionado = null;
    }
}