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

        if (destino.dono == jogador)
        {
            Debug.Log(
                "Ataque impossível: destino já pertence ao jogador."
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
        if (!PodeAtacar(
                origem,
                destino,
                jogador))
            return false;

        if (filaAcoes == null)
        {
            Debug.LogError(
                "FilaAcoes não inicializada."
            );

            return false;
        }

        int disponiveis =
            filaAcoes.TropasDisponiveis(
                origem
            );

        if (quantidade < 1)
        {
            Debug.Log(
                "Ataque impossível: quantidade mínima = 1."
            );

            return false;
        }

        if (quantidade > disponiveis)
        {
            Debug.Log(
                "Ataque impossível: " +
                origem.name +
                " possui apenas " +
                disponiveis +
                " tropas ainda disponíveis para novas ordens."
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

        filaAcoes.AdicionarAtaque(
            ordem
        );

        return true;
    }
}