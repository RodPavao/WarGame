using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public TerritorioClique territorioSelecionado;

    public TerritorioClique.Dono jogadorLocal =
        TerritorioClique.Dono.Jogador1;

    void Awake()
    {
        instance = this;
    }

    public void ClicarTerritorio(TerritorioClique t)
    {
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