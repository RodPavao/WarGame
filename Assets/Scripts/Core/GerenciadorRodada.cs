using UnityEngine;

public class GerenciadorRodada : MonoBehaviour
{
    // =====================================================
    // 1. REFERÊNCIAS E ESTADO
    // =====================================================

    private GameManager gameManager;

    [Header("Rodada")]
    [SerializeField]
    private int rodadaAtual = 1;

    public int RodadaAtual =>
        rodadaAtual;

    private void Awake()
    {
        gameManager =
            GetComponent<GameManager>();
    }

    // =====================================================
    // 2. CICLO DE RODADAS
    // =====================================================

    public void IniciarPartida()
    {
        rodadaAtual = 1;

        IniciarRodada();
    }

    public void IniciarProximaRodada()
    {
        rodadaAtual++;

        IniciarRodada();
    }

    private void IniciarRodada()
    {
        if (gameManager == null)
            return;

        // A preparação é uma única janela para reforços e ações.
        gameManager
            .IniciarPreparacaoRound();

        // Depois prepara os reforços.
        PrepararReforcos();
    }

    public void PrepararReforcos()
    {
        if (gameManager == null)
            return;

        int reforcos =
            CalcularReforcos(
                gameManager.jogadorLocal
            );

        gameManager.DefinirReforcos(
            reforcos
        );
    }

    // =====================================================
    // 3. CÁLCULO DE REFORÇOS
    // =====================================================

    public int CalcularReforcos(
        TerritorioClique.Dono jogador)
    {
        if (rodadaAtual == 1)
            return 8;

        TerritorioClique[] territorios =
            FindObjectsByType<TerritorioClique>();

        int quantidadeTerritorios = 0;

        foreach (
            TerritorioClique territorio
            in territorios)
        {
            if (territorio.dono ==
                jogador)
            {
                quantidadeTerritorios++;
            }
        }

        int reforcos =
            quantidadeTerritorios / 2;

        return Mathf.Max(
            1,
            reforcos
        );
    }

    // =====================================================
    // 4. CONTROLES DE TESTE NO INSPECTOR
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
