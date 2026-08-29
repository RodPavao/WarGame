using System;
using UnityEngine;

[Serializable]
public class OrdemTerrestre
{
    // =====================================================
    // 1. DADOS SERIALIZÁVEIS DA ORDEM
    // =====================================================

    [SerializeField] private TerritorioClique.Dono jogador;
    [SerializeField] private string idOrigem;
    [SerializeField] private string idDestino;
    [SerializeField] private int quantidadePretendida;
    [SerializeField] private int posicaoNaFila;

    public TerritorioClique.Dono Jogador => jogador;
    public string IdOrigem => idOrigem;
    public string IdDestino => idDestino;
    public int QuantidadePretendida => quantidadePretendida;
    public int PosicaoNaFila => posicaoNaFila;

    // =====================================================
    // 2. REFERÊNCIAS DE RUNTIME
    // =====================================================

    [NonSerialized] private TerritorioClique origem;
    [NonSerialized] private TerritorioClique destino;

    public TerritorioClique Origem => origem;
    public TerritorioClique Destino => destino;

    public OrdemTerrestre(
        TerritorioClique origem,
        TerritorioClique destino,
        int quantidadePretendida,
        TerritorioClique.Dono jogador,
        int posicaoNaFila)
    {
        this.origem = origem;
        this.destino = destino;
        this.jogador = jogador;
        this.quantidadePretendida = quantidadePretendida;
        this.posicaoNaFila = posicaoNaFila;

        idOrigem = ObterIdEstavel(origem);
        idDestino = ObterIdEstavel(destino);
    }

    // =====================================================
    // 3. IDENTIFICAÇÃO DETERMINÍSTICA
    // =====================================================

    private static string ObterIdEstavel(
        TerritorioClique territorio)
    {
        if (territorio == null)
            return string.Empty;

        if (!string.IsNullOrWhiteSpace(
                territorio.idTerritorio))
        {
            return territorio.idTerritorio;
        }

        return territorio.name;
    }
}
