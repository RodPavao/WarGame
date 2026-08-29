using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // =====================================================
    // 1. SELEÇÃO E JOGADOR LOCAL
    // =====================================================

    public TerritorioClique territorioSelecionado;
    public TerritorioClique territorioDestinoSelecionado;

    public TerritorioClique.Dono jogadorLocal =
        TerritorioClique.Dono.Jogador1;

    // =====================================================
    // 2. SISTEMAS
    // =====================================================

    private FilaAcoes filaAcoes;
    private SistemaAtaque sistemaAtaque;
    private ResolvedorCombate resolvedorCombate;
    private GerenciadorRodada gerenciadorRodada;
    private readonly HistoricoReforcos historicoReforcos =
        new HistoricoReforcos();

    // =====================================================
    // 3. PREPARAÇÃO E MODOS
    // =====================================================

    public enum FaseTurno
    {
        Preparacao,
        Resolucao
    }

    public enum EstadoPreparacao
    {
        Preparando,
        Enviado,
        Resolvendo
    }

    public enum ModoAcao
    {
        Nenhum,
        Atacar,
        Mover,
        Transferir
    }

    public FaseTurno faseAtual =
        FaseTurno.Preparacao;

    public EstadoPreparacao estadoPreparacao =
        EstadoPreparacao.Preparando;

    public ModoAcao modoAcao =
        ModoAcao.Nenhum;

    // =====================================================
    // 4. REFORÇOS
    // =====================================================

    public int reforcosDisponiveis = 0;

    public System.Collections.Generic.IReadOnlyList<DistribuicaoReforco>
        DistribuicoesReforcos =>
            historicoReforcos.Distribuicoes;

    public bool PodeEditarPreparacao =>
        faseAtual == FaseTurno.Preparacao &&
        estadoPreparacao == EstadoPreparacao.Preparando;

    public bool AcoesEnviadas =>
        estadoPreparacao == EstadoPreparacao.Enviado;

    // =====================================================
    // 5. AÇÕES TERRESTRES
    // =====================================================

    [Min(1)]
    public int quantidadeAtaqueSelecionada = 1;

    // =====================================================
    // 6. CRONÔMETRO
    // =====================================================

    [Header("Preparação")]
    [SerializeField]
    private float duracaoPreparacao = 90f;

    private float tempoPreparacaoRestante;

    public float TempoPreparacaoRestante =>
        tempoPreparacaoRestante;

    // =====================================================
    // 7. CONSULTAS PARA O HUD
    // =====================================================

    public int QuantidadeOrdensPreparadas =>
        filaAcoes != null
            ? filaAcoes.QuantidadeAtaques
            : 0;

    public IReadOnlyList<OrdemAtaque>
        OrdensPreparadas =>
            filaAcoes != null
                ? filaAcoes.Ataques
                : null;

    public int RodadaAtual =>
        gerenciadorRodada != null
            ? gerenciadorRodada.RodadaAtual
            : 1;

    // =====================================================
    // 8. INICIALIZAÇÃO
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

        if (GetComponent<HUDPreparacao>() == null)
        {
            gameObject.AddComponent<HUDPreparacao>();
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
    // 9. CICLO DA PREPARAÇÃO
    // =====================================================

    private void Update()
    {
        if (faseAtual != FaseTurno.Preparacao)
            return;

        tempoPreparacaoRestante -=
            Time.deltaTime;

        if (tempoPreparacaoRestante <= 0f)
        {
            tempoPreparacaoRestante = 0f;

            FecharPreparacaoEIniciarResolucao();
        }
    }

    public void IniciarPreparacaoRound()
    {
        faseAtual = FaseTurno.Preparacao;
        estadoPreparacao = EstadoPreparacao.Preparando;

        tempoPreparacaoRestante =
            duracaoPreparacao;

        modoAcao =
            ModoAcao.Atacar;

        quantidadeAtaqueSelecionada = 1;

        CancelarSelecaoAtual();

    }

    // =====================================================
    // 10. DISTRIBUIÇÃO DE REFORÇOS
    // =====================================================

    public void DefinirReforcos(
        int quantidade)
    {
        reforcosDisponiveis =
            Mathf.Max(
                0,
                quantidade
            );

        historicoReforcos.Limpar();
    }

    public bool PodeAdicionarReforcoEm(
        TerritorioClique territorio)
    {
        return
            PodeEditarPreparacao &&
            territorio != null &&
            territorio.dono == jogadorLocal &&
            reforcosDisponiveis > 0;
    }

    public void TentarAdicionarReforco(
        TerritorioClique territorio)
    {
        DistribuirReforcos(territorio, 1);
    }

    public int DistribuirReforcos(
        TerritorioClique territorio,
        int quantidade)
    {
        if (!PodeAdicionarReforcoEm(territorio))
            return 0;

        int quantidadeAplicada =
            Mathf.Clamp(
                quantidade,
                0,
                reforcosDisponiveis
            );

        if (quantidadeAplicada <= 0)
            return 0;

        for (int i = 0; i < quantidadeAplicada; i++)
        {
            territorio.AdicionarTropa();
        }

        reforcosDisponiveis -= quantidadeAplicada;
        historicoReforcos.Registrar(
            territorio,
            quantidadeAplicada
        );

        return quantidadeAplicada;
    }

    public bool DesfazerDistribuicaoReforco(int id)
    {
        if (!PodeEditarPreparacao)
            return false;

        if (!historicoReforcos.TentarObter(
                id,
                out DistribuicaoReforco distribuicao))
            return false;

        if (distribuicao.Territorio == null ||
            !distribuicao.Territorio.RemoverTropas(
                distribuicao.Quantidade))
        {
            return false;
        }

        historicoReforcos.Remover(id);
        reforcosDisponiveis += distribuicao.Quantidade;
        return true;
    }

    // =====================================================
    // 11. SELEÇÃO DE MODO
    // =====================================================

    public void SelecionarModoAtacar()
    {
        if (!PodeEditarPreparacao)
            return;

        modoAcao =
            ModoAcao.Atacar;

        CancelarSelecaoAtual();
    }

    // =====================================================
    // 12. QUANTIDADE DA AÇÃO
    // =====================================================

    public void AumentarQuantidadeAtaque()
    {
        if (!PodeEditarPreparacao)
            return;

        int maximo =
            ObterMaximoDisponivelOrigem();

        if (maximo <= 0)
            return;

        quantidadeAtaqueSelecionada =
            Mathf.Min(
                quantidadeAtaqueSelecionada + 1,
                maximo
            );
    }

    public void DiminuirQuantidadeAtaque()
    {
        if (!PodeEditarPreparacao)
            return;

        quantidadeAtaqueSelecionada =
            Mathf.Max(
                1,
                quantidadeAtaqueSelecionada - 1
            );
    }

    private int ObterMaximoDisponivelOrigem()
    {
        if (territorioSelecionado == null ||
            filaAcoes == null)
        {
            return 999;
        }

        return filaAcoes
            .TropasDisponiveis(
                territorioSelecionado
            );
    }

    // =====================================================
    // 13. SELEÇÃO NO MAPA
    // =====================================================

    public void ClicarTerritorio(
        TerritorioClique territorio)
    {
        if (territorio == null)
            return;

        if (!PodeEditarPreparacao)
            return;

        if (modoAcao !=
            ModoAcao.Atacar)
            return;

        // =================================================
        // ESCOLHER ORIGEM
        // =================================================

        if (territorioSelecionado == null)
        {
            if (territorio.dono != jogadorLocal)
                return;

            int disponiveis =
                filaAcoes
                    .TropasDisponiveis(
                        territorio
                    );

            if (disponiveis <= 0)
                return;

            territorioSelecionado =
                territorio;

            territorioSelecionado
                .DestacarSelecao();

            quantidadeAtaqueSelecionada =
                disponiveis;

            return;
        }

        // =================================================
        // CANCELAR ORIGEM
        // =================================================

        if (territorio ==
            territorioSelecionado)
        {
            CancelarSelecaoAtual();
            return;
        }

        // =================================================
        // TROCAR ORIGEM
        // =================================================

        if (territorio.dono ==
            jogadorLocal)
        {
            territorioSelecionado
                .RemoverDestaqueSelecao();

            if (territorioDestinoSelecionado != null)
            {
                territorioDestinoSelecionado
                    .RemoverDestaqueSelecao();
            }

            territorioSelecionado =
                territorio;

            territorioDestinoSelecionado =
                null;

            territorioSelecionado
                .DestacarSelecao();

            int disponiveis =
                filaAcoes
                    .TropasDisponiveis(
                        territorio
                    );

            quantidadeAtaqueSelecionada =
                Mathf.Max(
                    1,
                    disponiveis
                );

            return;
        }

        // =================================================
        // ESCOLHER DESTINO
        // =================================================

        if (!sistemaAtaque.PodeAtacar(
                territorioSelecionado,
                territorio,
                jogadorLocal))
        {
            return;
        }

        if (territorioDestinoSelecionado != null)
        {
            territorioDestinoSelecionado
                .RemoverDestaqueSelecao();
        }

        territorioDestinoSelecionado =
            territorio;

        territorioDestinoSelecionado
            .DestacarSelecao();

        int disponiveisOrigem =
            filaAcoes
                .TropasDisponiveis(
                    territorioSelecionado
                );

        quantidadeAtaqueSelecionada =
            Mathf.Clamp(
                quantidadeAtaqueSelecionada,
                1,
                disponiveisOrigem
            );

    }

    // =====================================================
    // 14. REGISTRO DE ATAQUE
    // =====================================================

    public void ConfirmarAtaquePreparado()
    {
        if (!PodeEditarPreparacao)
            return;

        if (territorioSelecionado == null ||
            territorioDestinoSelecionado == null)
            return;

        bool registrado =
            sistemaAtaque
                .RegistrarAtaque(
                    territorioSelecionado,
                    territorioDestinoSelecionado,
                    quantidadeAtaqueSelecionada,
                    jogadorLocal
                );

        if (!registrado)
            return;

        CancelarSelecaoAtual();

        quantidadeAtaqueSelecionada = 1;
    }

    // =====================================================
    // 15. CANCELAMENTO DE SELEÇÃO
    // =====================================================

    public void CancelarSelecaoAtual()
    {
        if (territorioSelecionado != null)
        {
            territorioSelecionado
                .RemoverDestaqueSelecao();
        }

        if (territorioDestinoSelecionado != null)
        {
            territorioDestinoSelecionado
                .RemoverDestaqueSelecao();
        }

        territorioSelecionado = null;
        territorioDestinoSelecionado = null;
    }

    // =====================================================
    // 16. EDIÇÃO DE ORDENS
    // =====================================================

    public void CancelarUltimaOrdem()
    {
        if (!PodeEditarPreparacao ||
            filaAcoes == null)
            return;

        filaAcoes.RemoverUltimoAtaque();
    }

    // =====================================================
    // 17. ENVIO LOCAL
    // =====================================================

    public void EnviarAcoes()
    {
        if (!PodeEditarPreparacao)
            return;

        estadoPreparacao =
            EstadoPreparacao.Enviado;

        CancelarSelecaoAtual();
    }

    public void CancelarEnvio()
    {
        if (faseAtual != FaseTurno.Preparacao ||
            estadoPreparacao != EstadoPreparacao.Enviado ||
            tempoPreparacaoRestante <= 0f)
        {
            return;
        }

        estadoPreparacao =
            EstadoPreparacao.Preparando;
    }

    // =====================================================
    // 18. FECHAMENTO DA PREPARAÇÃO
    // =====================================================

    public void FecharPreparacaoEIniciarResolucao()
    {
        if (faseAtual != FaseTurno.Preparacao ||
            estadoPreparacao == EstadoPreparacao.Resolvendo)
        {
            return;
        }

        estadoPreparacao =
            EstadoPreparacao.Resolvendo;

        reforcosDisponiveis = 0;
        historicoReforcos.Limpar();

        CancelarSelecaoAtual();

        ResolverRodadaAgora();
    }

    // =====================================================
    // 19. RESOLUÇÃO
    // =====================================================

    public void ResolverRodadaAgora()
    {
        if (filaAcoes == null ||
            resolvedorCombate == null ||
            gerenciadorRodada == null)
        {
            return;
        }

        faseAtual =
            FaseTurno.Resolucao;

        estadoPreparacao =
            EstadoPreparacao.Resolvendo;

        resolvedorCombate
            .Resolver(
                filaAcoes
            );

        CancelarSelecaoAtual();

        gerenciadorRodada
            .IniciarProximaRodada();
    }
}
