using System.Collections.Generic;
using UnityEngine;

public class FilaTransferencias : MonoBehaviour
{
    // =====================================================
    // 1. ESTADO E CONSULTAS
    // =====================================================

    private readonly List<OrdemTransferencia> ordens =
        new List<OrdemTransferencia>();

    public IReadOnlyList<OrdemTransferencia> Ordens => ordens;

    public OrdemTransferencia ObterPara(TerritorioClique.Dono jogador)
    {
        return ordens.Find(ordem => ordem.Autor == jogador);
    }

    public bool PossuiPara(TerritorioClique.Dono jogador) =>
        ObterPara(jogador) != null;

    // =====================================================
    // 2. REGISTRO E REMOÇÃO
    // =====================================================

    public bool Adicionar(OrdemTransferencia ordem)
    {
        if (ordem == null || PossuiPara(ordem.Autor))
            return false;

        ordens.Add(ordem);
        return true;
    }

    public bool RemoverPara(TerritorioClique.Dono jogador)
    {
        OrdemTransferencia ordem = ObterPara(jogador);

        if (ordem == null)
            return false;

        ordens.Remove(ordem);
        return true;
    }

    public List<OrdemTransferencia> CriarCopia() =>
        new List<OrdemTransferencia>(ordens);

    public void Limpar()
    {
        ordens.Clear();
    }
}
