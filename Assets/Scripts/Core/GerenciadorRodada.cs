using System.Collections.Generic;
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

    private readonly List<TerritorioClique.Dono> prioridadeJogadores =
        new List<TerritorioClique.Dono>();

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

        AtualizarPrioridadeJogadores();
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
    // 4. PRIORIDADE DETERMINÍSTICA ENTRE JOGADORES
    // =====================================================

    public IReadOnlyList<TerritorioClique.Dono> ObterPrioridadeJogadores()
    {
        if (prioridadeJogadores.Count == 0)
            AtualizarPrioridadeJogadores();

        return prioridadeJogadores;
    }

    private void AtualizarPrioridadeJogadores()
    {
        SortedSet<TerritorioClique.Dono> ativos =
            new SortedSet<TerritorioClique.Dono>();

        foreach (TerritorioClique territorio in
                 FindObjectsByType<TerritorioClique>())
        {
            if (territorio.dono != TerritorioClique.Dono.Neutro)
                ativos.Add(territorio.dono);
        }

        prioridadeJogadores.Clear();

        if (ativos.Count == 0)
            return;

        List<TerritorioClique.Dono> baseOrdenada =
            new List<TerritorioClique.Dono>(ativos);

        int deslocamento = (rodadaAtual - 1) % baseOrdenada.Count;

        for (int i = 0; i < baseOrdenada.Count; i++)
        {
            prioridadeJogadores.Add(
                baseOrdenada[(i + deslocamento) % baseOrdenada.Count]);
        }

        Debug.Log(
            "PRIORIDADE ROUND " + rodadaAtual + ": " +
            string.Join(" -> ", prioridadeJogadores));
    }

    // =====================================================
    // 5. CONTROLES DE TESTE NO INSPECTOR
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
