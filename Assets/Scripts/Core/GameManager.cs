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
    private ResolutionSequenceController sequenciaResolucaoVisualAtiva;
    private List<ResolutionVisualEvent> eventosReforcoPendentes =
        new List<ResolutionVisualEvent>();
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

    public int LimiteAcoesPorRodada =>
        FilaAcoes.MaximoAcoesPorRodada;

    public bool PodeCancelarEnvio =>
        faseAtual == FaseTurno.Preparacao &&
        estadoPreparacao == EstadoPreparacao.Enviado &&
        tempoPreparacaoRestante > 0f;

#if UNITY_EDITOR
    // =====================================================
    // 8. TESTE LOCAL DE PARTICIPANTES SIMULADOS
    // =====================================================

    private bool autoenviarJogadoresSimuladosEditor;

    [ContextMenu("Teste Editor/Ativar Autoenvio de Jogadores Simulados")]
    private void AtivarAutoenvioJogadoresSimuladosEditor()
    {
        autoenviarJogadoresSimuladosEditor = true;
        Debug.Log("TESTE LOCAL | Autoenvio dos jogadores simulados ATIVADO.");
    }

    [ContextMenu("Teste Editor/Desativar Autoenvio de Jogadores Simulados")]
    private void DesativarAutoenvioJogadoresSimuladosEditor()
    {
        autoenviarJogadoresSimuladosEditor = false;
        Debug.Log("TESTE LOCAL | Autoenvio dos jogadores simulados DESATIVADO.");
    }

    public void TesteEditorAtivarAutoenvio() => AtivarAutoenvioJogadoresSimuladosEditor();
    public void TesteEditorDesativarAutoenvio() => DesativarAutoenvioJogadoresSimuladosEditor();
#endif

    // =====================================================
    // 9. INICIALIZAÇÃO
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

        MatchUIPresenter matchUIPresenter =
            GetComponent<MatchUIPresenter>();

        if (matchUIPresenter == null)
        {
            matchUIPresenter =
                gameObject.AddComponent<MatchUIPresenter>();
        }

        UICompositionRoot uiCompositionRoot =
            GetComponent<UICompositionRoot>();

        if (uiCompositionRoot == null)
        {
            uiCompositionRoot =
                gameObject.AddComponent<UICompositionRoot>();
        }

        uiCompositionRoot.Inicializar(matchUIPresenter);

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
    // 10. CICLO DA PREPARAÇÃO
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

#if UNITY_EDITOR
        if (autoenviarJogadoresSimuladosEditor)
        {
            Debug.Log(
                "TESTE LOCAL | Jogadores simulados considerados enviados; " +
                "seguindo pelo fechamento real da preparação.");
            FecharPreparacaoEIniciarResolucao();
        }
#endif
    }

    public void CancelarEnvio()
    {
        if (!PodeCancelarEnvio)
            return;

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

        eventosReforcoPendentes = CriarEventosVisuaisReforco(
            historicoReforcos.Distribuicoes);
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
        GetComponent<MatchUIPresenter>()?.SolicitarSnapshotAtual();

        List<ResultadoTransferencia> resultadosTransferencias =
            resolvedorTransferencias.Resolver(
            filaTransferencias,
            gerenciadorRodada.ObterPrioridadeJogadores());

        List<ResultadoAcaoTerrestre> resultadosTerrestres =
            resolvedorAcoesTerrestres.Resolver(
            filaAcoes,
            gerenciadorRodada.ObterPrioridadeJogadores());

        CancelarSelecaoAtual();

        var eventosVisuais = new List<ResolutionVisualEvent>();
        eventosVisuais.AddRange(eventosReforcoPendentes);
        eventosVisuais.AddRange(
            CriarEventosVisuaisTransferencia(resultadosTransferencias));
        eventosVisuais.AddRange(CriarEventosVisuaisAtaque(resultadosTerrestres));
        eventosReforcoPendentes.Clear();

        ResolutionSequenceController controller =
            FindAnyObjectByType<ResolutionSequenceController>();
        if (controller != null)
        {
            sequenciaResolucaoVisualAtiva = controller;
            controller.SequenceCompleted -= ConcluirResolucaoAposVisual;
            controller.SequenceCompleted += ConcluirResolucaoAposVisual;
            controller.PlayAfterAnnouncement(eventosVisuais);
            return;
        }

        gerenciadorRodada.ConcluirResolucao();
    }

    private static List<ResolutionVisualEvent> CriarEventosVisuaisAtaque(
        IReadOnlyList<ResultadoAcaoTerrestre> resultados)
    {
        var eventos = new List<ResolutionVisualEvent>();
        if (resultados == null)
            return eventos;

        foreach (ResultadoAcaoTerrestre resultado in resultados)
        {
            if (resultado == null || !resultado.Executada ||
                resultado.Tipo != ResultadoAcaoTerrestre.TipoResultado.Ataque ||
                resultado.Ordem == null || resultado.Combate == null)
            {
                continue;
            }

            eventos.Add(ResolutionVisualEvent.Attack(
                resultado.Ordem.Jogador,
                resultado.Ordem.IdOrigem,
                resultado.Ordem.IdDestino,
                resultado.QuantidadeEfetiva,
                resultado.TropasOrigemAntes,
                resultado.TropasOrigemDepois,
                resultado.TropasDestinoAntes,
                resultado.TropasDestinoDepois,
                resultado.Combate.Conquistou,
                resultado.DonoDestinoAntes,
                resultado.DonoDestinoDepois));
        }

        return eventos;
    }

    // =====================================================
    // 21. SNAPSHOTS VISUAIS DE REFORÇO E TRANSFERÊNCIA
    // =====================================================

    private static List<ResolutionVisualEvent> CriarEventosVisuaisReforco(
        IReadOnlyList<DistribuicaoReforco> distribuicoes)
    {
        var eventos = new List<ResolutionVisualEvent>();
        if (distribuicoes == null)
            return eventos;

        var totais = new Dictionary<TerritorioClique, int>();
        foreach (DistribuicaoReforco distribuicao in distribuicoes)
        {
            if (distribuicao?.Territorio == null || distribuicao.Quantidade <= 0)
                continue;
            totais.TryGetValue(distribuicao.Territorio, out int total);
            totais[distribuicao.Territorio] = total + distribuicao.Quantidade;
        }

        var valores = new Dictionary<TerritorioClique, int>();
        foreach (KeyValuePair<TerritorioClique, int> item in totais)
            valores[item.Key] = Mathf.Max(0, item.Key.Tropas - item.Value);

        foreach (DistribuicaoReforco distribuicao in distribuicoes)
        {
            TerritorioClique territorio = distribuicao?.Territorio;
            if (territorio == null || distribuicao.Quantidade <= 0)
                continue;

            int antes = valores[territorio];
            int depois = antes + distribuicao.Quantidade;
            valores[territorio] = depois;
            eventos.Add(ResolutionVisualEvent.Reinforcement(
                territorio.dono,
                territorio.idTerritorio,
                distribuicao.Quantidade,
                antes,
                depois));
        }

        return eventos;
    }

    private static List<ResolutionVisualEvent> CriarEventosVisuaisTransferencia(
        IReadOnlyList<ResultadoTransferencia> resultados)
    {
        var eventos = new List<ResolutionVisualEvent>();
        if (resultados == null)
            return eventos;

        foreach (ResultadoTransferencia resultado in resultados)
        {
            if (resultado == null || !resultado.Executada)
                continue;

            eventos.Add(ResolutionVisualEvent.TerritoryHandoff(
                resultado.DonoAntes,
                resultado.DonoDepois,
                resultado.IdTerritorio,
                resultado.TropasAntes,
                resultado.TropasDepois));
        }

        return eventos;
    }

    private void ConcluirResolucaoAposVisual()
    {
        if (sequenciaResolucaoVisualAtiva != null)
            sequenciaResolucaoVisualAtiva.SequenceCompleted -= ConcluirResolucaoAposVisual;
        sequenciaResolucaoVisualAtiva = null;
        gerenciadorRodada?.ConcluirResolucao();
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
        foreach (TerritorioClique territorio in MapaAtivo.ObterTerritoriosOuCena())
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

    private bool resolucaoVisualTesteEditor;

    public bool PodeEntrarResolucaoTesteEditor =>
        Application.isPlaying && !PartidaEncerrada &&
        faseAtual == FaseTurno.Preparacao && gerenciadorRodada != null;

    public bool PodeAvancarResolucaoTesteEditor =>
        Application.isPlaying && !PartidaEncerrada &&
        resolucaoVisualTesteEditor && faseAtual == FaseTurno.Resolucao &&
        gerenciadorRodada != null;

    // =====================================================
    // 23. RESOLUÇÃO VISUAL MANUAL, SEM EXECUTAR ORDENS
    // =====================================================

    public void TesteEditorEntrarEmResolucao()
    {
        if (!PodeEntrarResolucaoTesteEditor)
            return;

        // Publica a preparação antes da transição mesmo entre dois polls da UI.
        MatchUIPresenter presenter = GetComponent<MatchUIPresenter>();
        presenter?.SolicitarSnapshotAtual();
        resolucaoVisualTesteEditor = true;
        faseAtual = FaseTurno.Resolucao;
        estadoPreparacao = EstadoPreparacao.Resolvendo;
        gerenciadorRodada.NotificarInicioResolucao();
        CancelarSelecaoTransferencia();
        CancelarSelecaoAtual();
        presenter?.SolicitarSnapshotAtual();
        // Update já interrompe o cronômetro fora de Preparacao.
        // Não chama ResolverRodadaAgora nem ConcluirResolucao.
    }

    public void TesteEditorAvancarParaProximoRound()
    {
        if (!PodeAvancarResolucaoTesteEditor)
            return;

        resolucaoVisualTesteEditor = false;
        // Ordens não executadas neste cenário não podem vazar para outro round.
        LimparPreparacaoParaTesteEditor();
        gerenciadorRodada.TesteEditorIniciarProximaRodada();
        GetComponent<MatchUIPresenter>()?.SolicitarSnapshotAtual();
    }

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
