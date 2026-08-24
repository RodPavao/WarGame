using UnityEngine;

public class GerenciadorRodada : MonoBehaviour
{
    private GameManager gameManager;

    [Header("Rodada")]
    [SerializeField]
    private int rodadaAtual = 1;

    public int RodadaAtual => rodadaAtual;

    private void Awake()
    {
        gameManager =
            GetComponent<GameManager>();
    }

    // =====================================================
    // INICIAR PARTIDA / RODADA
    // =====================================================

    public void IniciarPartida()
    {
        rodadaAtual = 1;

        PrepararReforcos();
    }

    public void IniciarProximaRodada()
    {
        rodadaAtual++;

        PrepararReforcos();
    }

    // =====================================================
    // REFORÇOS
    // =====================================================

    public void PrepararReforcos()
    {
        if (gameManager == null)
            return;

        gameManager.faseAtual =
            GameManager.FaseTurno.Reforco;

        int reforcos =
            CalcularReforcos(
                gameManager.jogadorLocal
            );

        gameManager.DefinirReforcos(
            reforcos
        );

        Debug.Log(
            "RODADA " +
            rodadaAtual +
            " | Reforços disponíveis: " +
            reforcos
        );
    }

    public int CalcularReforcos(
        TerritorioClique.Dono jogador)
    {
        // Round 1:
        // regra fixa já definida = 8 tropas.
        if (rodadaAtual == 1)
            return 8;

        TerritorioClique[] territorios =
    FindObjectsByType<TerritorioClique>();

        int quantidadeTerritorios = 0;

        foreach (
            TerritorioClique territorio
            in territorios)
        {
            if (territorio.dono == jogador)
            {
                quantidadeTerritorios++;
            }
        }

        // Rounds seguintes:
        // nº de territórios / 2
        // divisão inteira.
        int reforcos =
            quantidadeTerritorios / 2;

        return Mathf.Max(
            1,
            reforcos
        );
    }

    // =====================================================
    // TESTES PELO INSPECTOR
    // =====================================================

    [ContextMenu("Iniciar Partida Teste")]
    private void IniciarPartidaTeste()
    {
        IniciarPartida();
    }

    [ContextMenu("Iniciar Proxima Rodada")]
    private void ProximaRodadaTeste()
    {
        IniciarProximaRodada();
    }
}
