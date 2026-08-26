using UnityEngine;

public class SistemaAtaque : MonoBehaviour
{
    private FilaAcoes filaAcoes;

    public void Inicializar(
        FilaAcoes fila)
    {
        filaAcoes = fila;
    }

    public bool PodeAtacar(
        TerritorioClique origem,
        TerritorioClique destino,
        TerritorioClique.Dono jogador)
    {
        if (origem == null ||
            destino == null)
            return false;

        if (origem == destino)
            return false;

        if (origem.dono != jogador)
        {
            Debug.Log(
                "Ataque impossível: origem não pertence ao jogador."
            );

            return false;
        }

        if (EquipesJogadores.SaoAliados(
                jogador,
                destino.dono))
        {
            Debug.Log(
                "Ataque impossível: " +
                destino.name +
                " pertence a um aliado."
            );

            return false;
        }

        if (!origem.EhVizinho(destino))
        {
            Debug.Log(
                "Ataque impossível: " +
                destino.name +
                " não é vizinho de " +
                origem.name
            );

            return false;
        }

        if (origem.Tropas <= 1)
        {
            Debug.Log(
                "Ataque impossível: origem possui apenas 1 tropa."
            );

            return false;
        }

        return true;
    }

    public bool RegistrarAtaque(
        TerritorioClique origem,
        TerritorioClique destino,
        int quantidade,
        TerritorioClique.Dono jogador)
    {
        if (filaAcoes == null)
        {
            Debug.LogError(
                "FilaAcoes não inicializada."
            );

            return false;
        }

        if (filaAcoes.EstaCheia)
        {
            Debug.Log(
                "Máximo de 3 ações terrestres por rodada."
            );

            return false;
        }

        if (!PodeAtacar(
                origem,
                destino,
                jogador))
            return false;

        int disponiveis =
            filaAcoes.TropasDisponiveis(
                origem
            );

        if (quantidade < 1)
        {
            Debug.Log(
                "Quantidade mínima = 1."
            );

            return false;
        }

        if (quantidade > disponiveis)
        {
            Debug.Log(
                origem.name +
                " possui apenas " +
                disponiveis +
                " tropas disponíveis para novas ordens."
            );

            return false;
        }

        OrdemAtaque ordem =
            new OrdemAtaque(
                origem,
                destino,
                quantidade,
                jogador
            );

        return filaAcoes.AdicionarAtaque(
            ordem
        );
    }
}