using System.Collections.Generic;
using UnityEngine;

public class TerritorioFronteiras : MonoBehaviour
{
    // =====================================================
    // 1. VIZINHOS SERIALIZADOS LEGADOS
    // =====================================================

    [SerializeField]
    private TerritorioClique[] vizinhos;

    public IReadOnlyList<TerritorioClique> VizinhosLegados => vizinhos;

    // =====================================================
    // 2. CONFIGURAÇÃO
    // =====================================================

    public void Configurar(TerritorioClique[] novosVizinhos)
    {
        vizinhos = novosVizinhos;
    }

    // =====================================================
    // 3. CONSULTA DATA-DRIVEN COM FALLBACK
    // =====================================================

    public bool EhVizinho(TerritorioClique outro)
    {
        if (outro == null)
            return false;

        if (MapaAtivo.Instance != null && MapaAtivo.Instance.Definicao != null)
        {
            return MapaAtivo.Instance.SaoConectados(
                GetComponent<TerritorioClique>().idTerritorio,
                outro.idTerritorio,
                TipoConexaoMapa.Terrestre);
        }

        if (vizinhos == null)
            return false;

        foreach (TerritorioClique vizinho in vizinhos)
        {
            if (vizinho == outro)
                return true;
        }

        return false;
    }
}
