using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public TerritorioClique territorioSelecionado;

    public TerritorioClique.Dono jogadorLocal =
        TerritorioClique.Dono.Jogador1;

    private FilaAcoes filaAcoes;
    private SistemaAtaque sistemaAtaque;
    private ResolvedorCombate resolvedorCombate;
    private GerenciadorRodada gerenciadorRodada;

    public enum FaseTurno
    {
        Reforco,
        Ataque,
        Resolucao
    }

    public FaseTurno faseAtual =
        FaseTurno.Reforco;

    public int reforcosDisponiveis = 0;

    // =====================================================
    // QUANTIDADE DA PRÓXIMA ORDEM
    // =====================================================

    [Min(1)]
    public int quantidadeAtaqueSelecionada = 1;

    public int QuantidadeOrdensPreparadas =>
        filaAcoes != null
            ? filaAcoes.QuantidadeAtaques
            : 0;

    // =====================================================
    // INICIALIZAÇÃO
    // =====================================================

    private void Awake()
    {
        instance = this;

        filaAcoes =
            GetComponent<FilaAcoes>();

        if (filaAcoes == null)
        {
            filaAcoes =
                gameObject.AddComponent<FilaAcoes>();
        }

        sistemaAtaque =
            GetComponent<SistemaAtaque>();

        if (sistemaAtaque == null)
        {
            sistemaAtaque =
                gameObject.AddComponent<SistemaAtaque>();
        }

        resolvedorCombate =
            GetComponent<ResolvedorCombate>();

        if (resolvedorCombate == null)
        {
            resolvedorCombate =
                gameObject.AddComponent<ResolvedorCombate>();
        }

        gerenciadorRodada =
            GetComponent<GerenciadorRodada>();

        if (gerenciadorRodada == null)
        {
            gerenciadorRodada =
                gameObject.AddComponent<GerenciadorRodada>();
        }

        sistemaAtaque.Inicializar(
            filaAcoes
        );
    }

    private void Start()
    {
        gerenciadorRodada
            .IniciarPartida();
    }

    // =====================================================
    // REFORÇOS
    // =====================================================

    public void DefinirReforcos(
        int quantidade)
    {
        reforcosDisponiveis =
            Mathf.Max(
                0,
                quantidade
            );
    }

    public void TentarAdicionarReforco(
        TerritorioClique t)
    {
        if (faseAtual !=
            FaseTurno.Reforco)
            return;

        if (t == null)
            return;

        if (t.dono != jogadorLocal)
            return;

        if (reforcosDisponiveis <= 0)
            return;

        t.AdicionarTropa();

        reforcosDisponiveis--;

        Debug.Log(
            "Tropa adicionada em " +
            t.name +
            " | Tropas: " +
            t.Tropas +
            " | Reforços restantes: " +
            reforcosDisponiveis
        );

        // Acabaram os reforços:
        // inicia automaticamente preparação de ataques.
        if (reforcosDisponiveis == 0)
        {
            faseAtual =
                FaseTurno.Ataque;

            Debug.Log(
                "REFORÇOS ENCERRADOS | " +
                "Fase de preparação iniciada."
            );
        }
    }

    // =====================================================
    // QUANTIDADE DO ATAQUE
    // =====================================================

    public void DefinirQuantidadeAtaque(
        int quantidade)
    {
        quantidadeAtaqueSelecionada =
            Mathf.Max(
                1,
                quantidade
            );
    }

    public void AumentarQuantidadeAtaque()
    {
        quantidadeAtaqueSelecionada++;
    }

    public void DiminuirQuantidadeAtaque()
    {
        quantidadeAtaqueSelecionada =
            Mathf.Max(
                1,
                quantidadeAtaqueSelecionada - 1
            );
    }

    // =====================================================
    // TERRITÓRIOS
    // =====================================================

    public void ClicarTerritorio(
        TerritorioClique t)
    {
        if (t == null)
            return;

        if (faseAtual ==
            FaseTurno.Resolucao)
            return;

        if (territorioSelecionado == null)
        {
            if (t.dono != jogadorLocal)
                return;

            territorioSelecionado = t;

            Debug.Log(
                "Origem selecionada: " +
                t.name +
                " | Tropas: " +
                t.Tropas
            );

            return;
        }

        if (t == territorioSelecionado)
        {
            territorioSelecionado = null;

            Debug.Log(
                "Seleção cancelada."
            );

            return;
        }

        if (t.dono == jogadorLocal)
        {
            territorioSelecionado = t;

            Debug.Log(
                "Nova origem: " +
                t.name +
                " | Tropas: " +
                t.Tropas
            );

            return;
        }

        if (faseAtual !=
            FaseTurno.Ataque)
            return;

        bool registrado =
            sistemaAtaque.RegistrarAtaque(
                territorioSelecionado,
                t,
                quantidadeAtaqueSelecionada,
                jogadorLocal
            );

        if (registrado)
        {
            Debug.Log(
                "Ataque preparado | Ordem " +
                QuantidadeOrdensPreparadas +
                " | " +
                territorioSelecionado.name +
                " -> " +
                t.name +
                " | Tropas: " +
                quantidadeAtaqueSelecionada
            );
        }

        territorioSelecionado = null;
    }

    // =====================================================
    // CANCELAR ORDEM
    // =====================================================

    [ContextMenu("Cancelar Ultima Ordem")]
    public void CancelarUltimaOrdem()
    {
        if (filaAcoes == null)
            return;

        filaAcoes.RemoverUltimoAtaque();
    }

    // =====================================================
    // RESOLUÇÃO
    // =====================================================

    [ContextMenu("Resolver Rodada Agora")]
    public void ResolverRodadaAgora()
    {
        if (filaAcoes == null ||
            resolvedorCombate == null ||
            gerenciadorRodada == null)
            return;

        faseAtual =
            FaseTurno.Resolucao;

        resolvedorCombate.Resolver(
            filaAcoes
        );

        territorioSelecionado = null;

        // Acabou a resolução:
        // próxima rodada automaticamente.
        gerenciadorRodada
            .IniciarProximaRodada();
    }
}