using System;
using UnityEngine;

[Serializable]
public class OrdemTransferencia
{
    // =====================================================
    // 1. DADOS SERIALIZÁVEIS
    // =====================================================

    [SerializeField] private TerritorioClique.Dono autor;
    [SerializeField] private TerritorioClique.Dono destinatario;
    [SerializeField] private string idTerritorio;

    public TerritorioClique.Dono Autor => autor;
    public TerritorioClique.Dono Destinatario => destinatario;
    public string IdTerritorio => idTerritorio;

    // =====================================================
    // 2. REFERÊNCIA DE RUNTIME
    // =====================================================

    [NonSerialized] private TerritorioClique territorio;

    public TerritorioClique Territorio => territorio;

    public OrdemTransferencia(
        TerritorioClique territorio,
        TerritorioClique.Dono autor,
        TerritorioClique.Dono destinatario)
    {
        this.territorio = territorio;
        this.autor = autor;
        this.destinatario = destinatario;
        idTerritorio = territorio != null &&
            !string.IsNullOrWhiteSpace(territorio.idTerritorio)
                ? territorio.idTerritorio
                : territorio != null ? territorio.name : string.Empty;
    }
}
