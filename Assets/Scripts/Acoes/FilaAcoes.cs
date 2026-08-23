using System.Collections.Generic;
using UnityEngine;

public class FilaAcoes : MonoBehaviour
{
    private readonly List<OrdemAtaque> ataques =
        new List<OrdemAtaque>();

    public int QuantidadeAtaques =>
        ataques.Count;

    public IReadOnlyList<OrdemAtaque> Ataques =>
        ataques;

    public void AdicionarAtaque(
        OrdemAtaque ordem)
    {
        if (ordem == null)
            return;

        ataques.Add(ordem);

        Debug.Log(
            "ORDEM #" +
            ataques.Count +
            " REGISTRADA: " +
            ordem.Origem.name +
            " -> " +
            ordem.Destino.name +
            " | Tropas: " +
            ordem.Tropas
        );
    }

    public int TropasReservadas(
        TerritorioClique origem)
    {
        int total = 0;

        foreach (OrdemAtaque ordem in ataques)
        {
            if (ordem.Origem == origem)
                total += ordem.Tropas;
        }

        return total;
    }

    public int TropasDisponiveis(
        TerritorioClique origem)
    {
        if (origem == null)
            return 0;

        int disponiveis =
            origem.Tropas -
            1 -
            TropasReservadas(origem);

        return Mathf.Max(
            0,
            disponiveis
        );
    }

    public bool RemoverUltimoAtaque()
    {
        if (ataques.Count == 0)
            return false;

        OrdemAtaque removida =
            ataques[ataques.Count - 1];

        ataques.RemoveAt(
            ataques.Count - 1
        );

        Debug.Log(
            "ORDEM CANCELADA: " +
            removida.Origem.name +
            " -> " +
            removida.Destino.name +
            " | Tropas liberadas: " +
            removida.Tropas
        );

        return true;
    }

    public List<OrdemAtaque>
        CriarCopiaAtaques()
    {
        return new List<OrdemAtaque>(
            ataques
        );
    }

    public void Limpar()
    {
        ataques.Clear();
    }
}