using UnityEngine;

public class TerritorioFronteiras : MonoBehaviour
{
    // =====================================================
    // VIZINHOS
    // =====================================================

    [SerializeField]
    private TerritorioClique[] vizinhos;

    // =====================================================
    // CONFIGURAÇÃO
    // =====================================================

    public void Configurar(TerritorioClique[] novosVizinhos)
    {
        vizinhos = novosVizinhos;
    }

    // =====================================================
    // CONSULTA DE VIZINHANÇA
    // =====================================================

    public bool EhVizinho(TerritorioClique outro)
    {
        if (outro == null || vizinhos == null)
            return false;

        foreach (TerritorioClique vizinho in vizinhos)
        {
            if (vizinho == outro)
                return true;
        }

        return false;
    }
}
