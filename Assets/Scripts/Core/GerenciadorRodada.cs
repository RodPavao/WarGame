using UnityEngine;

public class GerenciadorRodada : MonoBehaviour
{
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

        // PRIMEIRO começa o relógio.
        gameManager
            .IniciarCronometroRound();

        // Depois prepara os reforços.
        PrepararReforcos();
    }

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
            reforcos +
            " | Cronômetro já iniciado."
        );
    }

    public int CalcularReforcos(
        TerritorioClique.Dono jogador)
    {
        if (rodadaAtual == 1)
            return 8;

        TerritorioClique[] territorios =
            FindObjectsByType<TerritorioClique>(
                FindObjectsSortMode.None
            );

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