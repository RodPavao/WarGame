using System.Collections.Generic;
using UnityEngine;

public class ResolvedorTransferencias : MonoBehaviour
{
    // =====================================================
    // 1. RESOLUÇÃO ORDENADA
    // =====================================================

    public List<ResultadoTransferencia> Resolver(
        FilaTransferencias fila,
        IReadOnlyList<TerritorioClique.Dono> prioridadeJogadores)
    {
        List<ResultadoTransferencia> resultados =
            new List<ResultadoTransferencia>();

        if (fila == null)
            return resultados;

        List<OrdemTransferencia> pendentes = fila.CriarCopia();

        if (prioridadeJogadores != null)
        {
            foreach (TerritorioClique.Dono jogador in prioridadeJogadores)
                ResolverDoJogador(jogador, pendentes, resultados);
        }

        pendentes.Sort((a, b) => a.Autor.CompareTo(b.Autor));

        foreach (OrdemTransferencia ordem in pendentes)
            resultados.Add(ResolverOrdem(ordem));

        fila.Limpar();
        return resultados;
    }

    private void ResolverDoJogador(
        TerritorioClique.Dono jogador,
        List<OrdemTransferencia> pendentes,
        List<ResultadoTransferencia> resultados)
    {
        OrdemTransferencia ordem = pendentes.Find(item => item.Autor == jogador);

        if (ordem == null)
            return;

        resultados.Add(ResolverOrdem(ordem));
        pendentes.Remove(ordem);
    }

    // =====================================================
    // 2. REVALIDAÇÃO AUTORITATIVA
    // =====================================================

    private ResultadoTransferencia ResolverOrdem(OrdemTransferencia ordem)
    {
        if (ordem == null || ordem.Territorio == null)
            return Cancelar(ordem, "Território indisponível.");

        if (ordem.Territorio.dono != ordem.Autor)
            return Cancelar(ordem, "O autor não possui mais o território.");

        if (!EquipesJogadores.SaoAliados(ordem.Autor, ordem.Destinatario))
            return Cancelar(ordem, "O destinatário não é mais aliado.");

        int tropasAntes = ordem.Territorio.Tropas;
        TerritorioClique.Dono donoAntes = ordem.Territorio.dono;
        ordem.Territorio.DefinirDono(ordem.Destinatario);

        Debug.Log(
            "TRANSFERÊNCIA EXECUTADA: " + ordem.IdTerritorio + " | " +
            ordem.Autor + " -> " + ordem.Destinatario + " | Tropas: " +
            tropasAntes);

        return new ResultadoTransferencia(
            true,
            ordem,
            tropasAntes,
            "Transferência concluída.",
            tropasAntes,
            ordem.Territorio.Tropas,
            donoAntes,
            ordem.Territorio.dono);
    }

    private ResultadoTransferencia Cancelar(
        OrdemTransferencia ordem,
        string motivo)
    {
        Debug.Log("TRANSFERÊNCIA CANCELADA: " + motivo);
        return new ResultadoTransferencia(false, ordem, 0, motivo);
    }
}
