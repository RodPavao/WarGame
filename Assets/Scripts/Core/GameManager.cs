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
    private SistemaAcoesTerrestres sistemaAcoesTerrestres;
    private ResolvedorCombate resolvedorCombate;
    private ResolvedorAcoesTerrestres resolvedorAcoesTerrestres;
    private FilaTransferencias filaTransferencias;
    private SistemaTransferencias sistemaTransferencias;
    private ResolvedorTransferencias resolvedorTransferencias;
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
        AcaoTerrestre,
        Transferir
    }

    public FaseTurno faseAtual =
        FaseTurno.Preparacao;

    public EstadoPreparacao estadoPreparacao =
        EstadoPreparacao.Preparando;

    public ModoAcao modoAcao =
        ModoAcao.Nenhum;

    public TerritorioClique territorioTransferenciaSelecionado;
    public string feedbackTransferencia = string.Empty;

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

    public bool EmModoTransferencia =>
        PodeEditarPreparacao &&
        modoAcao == ModoAcao.Transferir;

    // =====================================================
    // 5. AÇÕES TERRESTRES
    // =====================================================

    [Min(1)]
    public int quantidadeAcaoSelecionada = 1;

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
            ? filaAcoes.QuantidadePara(jogadorLocal)
            : 0;

    public IReadOnlyList<OrdemTerrestre>
        OrdensPreparadas =>
            filaAcoes != null
                ? ObterOrdensDoJogadorLocal()
                : null;

    public string TipoAcaoSelecionadaEsperado =>
        SistemaAcoesTerrestres.ObterTipoEsperado(
            jogadorLocal,
            territorioDestinoSelecionado);

    public bool LimiteAcoesAtingido =>
        filaAcoes != null &&
        filaAcoes.EstaCheiaPara(jogadorLocal);

    public int RodadaAtual =>
        gerenciadorRodada != null
            ? gerenciadorRodada.RodadaAtual
            : 1;

    public GerenciadorRodada.EstadoPartida EstadoAtualPartida =>
        gerenciadorRodada != null
            ? gerenciadorRodada.EstadoAtual
            : GerenciadorRodada.EstadoPartida.EmPreparacao;

    public ResultadoPartida ResultadoPartidaAtual =>
        gerenciadorRodada != null
            ? gerenciadorRodada.ResultadoAtual
            : null;

    public bool PartidaEncerrada =>
        gerenciadorRodada != null && gerenciadorRodada.PartidaEncerrada;

    public int RoundMorteSubita =>
        gerenciadorRodada != null
            ? gerenciadorRodada.RoundMorteSubita
            : 0;

    public OrdemTransferencia TransferenciaPreparada =>
        filaTransferencias != null
            ? filaTransferencias.ObterPara(jogadorLocal)
            : null;

    public bool LimiteTransferenciaAtingido =>
        TransferenciaPreparada != null;

    public bool PossuiPreparacaoParaEnviar =>
        QuantidadeOrdensPreparadas > 0 ||
        TransferenciaPreparada != null;

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

        sistemaAcoesTerrestres =
            GetComponent<SistemaAcoesTerrestres>();

        if (sistemaAcoesTerrestres == null)
        {
            sistemaAcoesTerrestres =
                gameObject.AddComponent<SistemaAcoesTerrestres>();
        }

        resolvedorCombate =
            GetComponent<ResolvedorCombate>();

        if (resolvedorCombate == null)
        {
            resolvedorCombate =
                gameObject.AddComponent<ResolvedorCombate>();
        }

        resolvedorAcoesTerrestres =
            GetComponent<ResolvedorAcoesTerrestres>();

        if (resolvedorAcoesTerrestres == null)
        {
            resolvedorAcoesTerrestres =
                gameObject.AddComponent<ResolvedorAcoesTerrestres>();
        }

        filaTransferencias = GetComponent<FilaTransferencias>();

        if (filaTransferencias == null)
            filaTransferencias = gameObject.AddComponent<FilaTransferencias>();

        sistemaTransferencias = GetComponent<SistemaTransferencias>();

        if (sistemaTransferencias == null)
            sistemaTransferencias = gameObject.AddComponent<SistemaTransferencias>();

        resolvedorTransferencias = GetComponent<ResolvedorTransferencias>();

        if (resolvedorTransferencias == null)
            resolvedorTransferencias = gameObject.AddComponent<ResolvedorTransferencias>();

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

        sistemaAcoesTerrestres.Inicializar(filaAcoes);
        resolvedorAcoesTerrestres.Inicializar(resolvedorCombate);
        sistemaTransferencias.Inicializar(filaTransferencias);
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
            ModoAcao.AcaoTerrestre;

        quantidadeAcaoSelecionada = 1;

        territorioTransferenciaSelecionado = null;
        feedbackTransferencia = string.Empty;

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

    public void SelecionarModoAcaoTerrestre()
    {
        if (!PodeEditarPreparacao)
            return;

        modoAcao =
            ModoAcao.AcaoTerrestre;

        CancelarSelecaoTransferencia();
        CancelarSelecaoAtual();
    }

    public void SelecionarModoTransferencia()
    {
        if (!PodeEditarPreparacao)
            return;

        if (LimiteTransferenciaAtingido)
        {
            feedbackTransferencia = "Limite de 1 transferência atingido.";
            return;
        }

        modoAcao = ModoAcao.Transferir;
        feedbackTransferencia =
            "Selecione um território seu e depois seu aliado.";

        CancelarSelecaoAtual();
    }

    // =====================================================
    // 12. QUANTIDADE DA AÇÃO
    // =====================================================

    public void AumentarQuantidadeAcao()
    {
        if (!PodeEditarPreparacao)
            return;

        int maximo =
            ObterMaximoDisponivelOrigem();

        if (maximo <= 0)
            return;

        quantidadeAcaoSelecionada =
            Mathf.Min(
                quantidadeAcaoSelecionada + 1,
                maximo
            );
    }

    public void DiminuirQuantidadeAcao()
    {
        if (!PodeEditarPreparacao)
            return;

        quantidadeAcaoSelecionada =
            Mathf.Max(
                1,
                quantidadeAcaoSelecionada - 1
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
                territorioSelecionado,
                jogadorLocal
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

        if (modoAcao == ModoAcao.Transferir)
        {
            ClicarTerritorioTransferencia(territorio);
            return;
        }

        if (modoAcao !=
            ModoAcao.AcaoTerrestre)
            return;

        if (LimiteAcoesAtingido)
        {
            CancelarSelecaoAtual();
            return;
        }

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
                        territorio,
                        jogadorLocal
                    );

            if (disponiveis <= 0)
                return;

            territorioSelecionado =
                territorio;

            territorioSelecionado
                .DestacarSelecao();

            quantidadeAcaoSelecionada =
                disponiveis;

            AtualizarHighlightsPreparacao();

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
        // TROCAR ORIGEM (antes de existir destino)
        // =================================================

        if (territorio.dono == jogadorLocal &&
            territorioDestinoSelecionado == null &&
            !territorioSelecionado.EhVizinho(territorio))
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
                        territorio,
                        jogadorLocal
                    );

            if (disponiveis <= 0)
            {
                CancelarSelecaoAtual();
                return;
            }

            quantidadeAcaoSelecionada =
                Mathf.Max(
                    1,
                    disponiveis
                );

            AtualizarHighlightsPreparacao();

            return;
        }

        // =================================================
        // ESCOLHER DESTINO
        // =================================================

        if (!sistemaAcoesTerrestres.PodePreparar(
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
                    territorioSelecionado,
                    jogadorLocal
                );

        quantidadeAcaoSelecionada =
            Mathf.Clamp(
                quantidadeAcaoSelecionada,
                1,
                disponiveisOrigem
            );

        AtualizarHighlightsPreparacao();

    }

    // =====================================================
    // 14. REGISTRO DE AÇÃO TERRESTRE
    // =====================================================

    public void ConfirmarAcaoPreparada()
    {
        if (!PodeEditarPreparacao)
            return;

        if (territorioSelecionado == null ||
            territorioDestinoSelecionado == null)
            return;

        bool registrado =
            sistemaAcoesTerrestres
                .Registrar(
                    territorioSelecionado,
                    territorioDestinoSelecionado,
                    quantidadeAcaoSelecionada,
                    jogadorLocal
                );

        if (!registrado)
            return;

        CancelarSelecaoAtual();

        quantidadeAcaoSelecionada = 1;
    }

    // =====================================================
    // 15. CANCELAMENTO DE SELEÇÃO
    // =====================================================

    public void CancelarSelecaoAtual()
    {
        territorioSelecionado = null;
        territorioDestinoSelecionado = null;

        AtualizarHighlightsPreparacao();
    }

    // =====================================================
    // 16. EDIÇÃO DE ORDENS
    // =====================================================

    public void CancelarUltimaOrdem()
    {
        if (!PodeEditarPreparacao ||
            filaAcoes == null)
            return;

        if (filaAcoes.RemoverUltima(jogadorLocal))
            AtualizarHighlightsPreparacao();
    }

    public bool CancelarOrdem(OrdemTerrestre ordem)
    {
        if (!PodeEditarPreparacao || filaAcoes == null ||
            ordem == null || ordem.Jogador != jogadorLocal)
            return false;

        bool removida = filaAcoes.Remover(ordem);

        if (removida)
            AtualizarHighlightsPreparacao();

        return removida;
    }

    // =====================================================
    // 17. PREPARAÇÃO DA TRANSFERÊNCIA
    // =====================================================

    private void ClicarTerritorioTransferencia(TerritorioClique territorio)
    {
        if (LimiteTransferenciaAtingido)
        {
            feedbackTransferencia = "Limite de 1 transferência atingido.";
            CancelarSelecaoTransferencia();
            return;
        }

        if (territorio.dono != jogadorLocal)
        {
            feedbackTransferencia = "Selecione um território que pertence a você.";
            territorioTransferenciaSelecionado = null;
            AtualizarHighlightsPreparacao();
            return;
        }

        territorioTransferenciaSelecionado = territorio;
        feedbackTransferencia = "Agora selecione o slot do seu aliado.";
        AtualizarHighlightsPreparacao();
    }

    public void TentarPrepararTransferenciaPara(
        TerritorioClique.Dono jogador)
    {
        if (!EmModoTransferencia || sistemaTransferencias == null)
            return;

        if (territorioTransferenciaSelecionado == null)
        {
            feedbackTransferencia = "Selecione primeiro um território seu.";
            return;
        }

        bool registrada = sistemaTransferencias.Registrar(
            territorioTransferenciaSelecionado,
            jogadorLocal,
            jogador,
            out string motivo);

        feedbackTransferencia = motivo;

        if (!registrada)
            return;

        territorioTransferenciaSelecionado = null;
        modoAcao = ModoAcao.AcaoTerrestre;
        AtualizarHighlightsPreparacao();
    }

    public IReadOnlyList<TerritorioClique.Dono> ObterJogadoresTransferencia()
    {
        return gerenciadorRodada != null
            ? gerenciadorRodada.ObterJogadoresNoTabuleiro()
            : new List<TerritorioClique.Dono>();
    }

    public void RemoverTransferencia()
    {
        if (!PodeEditarPreparacao || filaTransferencias == null)
            return;

        if (!filaTransferencias.RemoverPara(jogadorLocal))
            return;

        feedbackTransferencia = "Transferência removida.";
        AtualizarHighlightsPreparacao();
    }

    public void CancelarSelecaoTransferencia()
    {
        territorioTransferenciaSelecionado = null;
        AtualizarHighlightsPreparacao();
    }

    // =====================================================
    // 18. ENVIO LOCAL
    // =====================================================

    public void EnviarAcoes()
    {
        if (!PodeEditarPreparacao)
            return;

        estadoPreparacao =
            EstadoPreparacao.Enviado;

        CancelarSelecaoTransferencia();
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
    // 19. FECHAMENTO DA PREPARAÇÃO
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

        CancelarSelecaoTransferencia();
        CancelarSelecaoAtual();

        ResolverRodadaAgora();
    }

    // =====================================================
    // 20. RESOLUÇÃO
    // =====================================================

    public void ResolverRodadaAgora()
    {
        if (filaAcoes == null ||
            resolvedorAcoesTerrestres == null ||
            filaTransferencias == null ||
            resolvedorTransferencias == null ||
            gerenciadorRodada == null)
        {
            return;
        }

        faseAtual =
            FaseTurno.Resolucao;

        estadoPreparacao =
            EstadoPreparacao.Resolvendo;

        gerenciadorRodada.NotificarInicioResolucao();

        resolvedorTransferencias.Resolver(
            filaTransferencias,
            gerenciadorRodada.ObterPrioridadeJogadores());

        resolvedorAcoesTerrestres.Resolver(
            filaAcoes,
            gerenciadorRodada.ObterPrioridadeJogadores());

        CancelarSelecaoAtual();

        gerenciadorRodada.ConcluirResolucao();
    }

    private IReadOnlyList<OrdemTerrestre> ObterOrdensDoJogadorLocal()
    {
        List<OrdemTerrestre> locais = new List<OrdemTerrestre>();

        foreach (OrdemTerrestre ordem in filaAcoes.Ordens)
        {
            if (ordem.Jogador == jogadorLocal)
                locais.Add(ordem);
        }

        locais.Sort((a, b) =>
            a.PosicaoNaFila.CompareTo(b.PosicaoNaFila));

        return locais;
    }

    // =====================================================
    // 21. HIGHLIGHTS DERIVADOS DAS FILAS
    // =====================================================

    private void AtualizarHighlightsPreparacao()
    {
        TerritorioClique[] territorios =
            FindObjectsByType<TerritorioClique>();

        foreach (TerritorioClique territorio in territorios)
        {
            if (DeveManterHighlight(territorio))
                territorio.DestacarSelecao();
            else
                territorio.RemoverDestaqueSelecao();
        }
    }

    private bool DeveManterHighlight(TerritorioClique territorio)
    {
        if (territorio == null)
            return false;

        bool preparacaoAtiva =
            faseAtual == FaseTurno.Preparacao &&
            estadoPreparacao != EstadoPreparacao.Resolvendo;

        if (!preparacaoAtiva)
            return false;

        if (territorio == territorioSelecionado ||
            territorio == territorioDestinoSelecionado ||
            territorio == territorioTransferenciaSelecionado)
            return true;

        if (filaAcoes == null)
            return false;

        foreach (OrdemTerrestre ordem in filaAcoes.Ordens)
        {
            if (ordem.Origem == territorio ||
                ordem.Destino == territorio)
                return true;
        }

        if (filaTransferencias != null)
        {
            foreach (OrdemTransferencia ordem in filaTransferencias.Ordens)
            {
                if (ordem.Territorio == territorio)
                    return true;
            }
        }

        return false;
    }

#if UNITY_EDITOR
    // =====================================================
    // 22. ISOLAMENTO DOS CENÁRIOS DE TESTE DO EDITOR
    // =====================================================

    public void LimparPreparacaoParaTesteEditor()
    {
        filaAcoes?.Limpar();
        filaTransferencias?.Limpar();
        territorioSelecionado = null;
        territorioDestinoSelecionado = null;
        territorioTransferenciaSelecionado = null;
        AtualizarHighlightsPreparacao();
    }
#endif
}
