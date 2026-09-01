using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class MatchUIPresenter : MonoBehaviour
{
    // ============================================================
    // 01. ESTADO PUBLICADO E COMANDOS
    // ============================================================

    private GameManager gameManager;
    private MatchUIState estadoAtual;
    private float proximaAtualizacao;

    private const float IntervaloAtualizacao = 0.1f;

    public MatchUIState EstadoAtual => estadoAtual;
    public IMatchUICommands Comandos { get; private set; }
    public event Action<MatchUIState> EstadoAlterado;

    // ============================================================
    // 02. INICIALIZAÇÃO E ATUALIZAÇÃO CENTRALIZADA
    // ============================================================

    private void Awake()
    {
        gameManager = GetComponent<GameManager>();
        Comandos = new MatchUICommands(gameManager);
    }

    private void Start()
    {
        PublicarSeAlterado(true);
    }

    private void Update()
    {
        if (Time.unscaledTime < proximaAtualizacao)
            return;

        proximaAtualizacao = Time.unscaledTime + IntervaloAtualizacao;
        PublicarSeAlterado(false);
    }

    public void SolicitarSnapshotAtual()
    {
        PublicarSeAlterado(true);
    }

    private void PublicarSeAlterado(bool forcar)
    {
        if (gameManager == null)
            return;

        MatchUIState novoEstado = CriarSnapshot();

        if (!forcar && novoEstado.Equals(estadoAtual))
            return;

        estadoAtual = novoEstado;
        EstadoAlterado?.Invoke(estadoAtual);
    }

    // ============================================================
    // 03. CONSTRUÇÃO DO SNAPSHOT SOMENTE DE LEITURA
    // ============================================================

    private MatchUIState CriarSnapshot()
    {
        IReadOnlyList<TerritorioClique> territorios = MapaAtivo.ObterTerritoriosOuCena();
        MatchUIPlayerState[] jogadores = CriarJogadores(territorios);
        MatchUIActionState[] acoes = CriarAcoes();
        MatchUIReinforcementState[] distribuicoes = CriarDistribuicoes();
        MatchUITransferState transferencia = CriarTransferencia();
        MatchUIResultState resultado = gameManager.ResultadoPartidaAtual != null
            ? new MatchUIResultState(gameManager.ResultadoPartidaAtual)
            : null;

        float tempo = Mathf.Max(0f, gameManager.TempoPreparacaoRestante);
        tempo = Mathf.Round(tempo * 10f) / 10f;

        string origemId = ObterId(gameManager.territorioSelecionado);
        string destinoId = ObterId(gameManager.territorioDestinoSelecionado);
        string transferenciaId = ObterId(gameManager.territorioTransferenciaSelecionado);
        string chave = CriarChaveConteudo(
            tempo, jogadores, acoes, distribuicoes, transferencia, resultado,
            origemId, destinoId, transferenciaId);

        return new MatchUIState(
            gameManager.RodadaAtual,
            gameManager.faseAtual,
            gameManager.estadoPreparacao,
            gameManager.modoAcao,
            gameManager.EstadoAtualPartida,
            gameManager.EstadoAtualPartida == GerenciadorRodada.EstadoPartida.MorteSubita,
            gameManager.RoundMorteSubita,
            tempo,
            gameManager.reforcosDisponiveis,
            gameManager.QuantidadeOrdensPreparadas,
            gameManager.LimiteAcoesPorRodada,
            gameManager.TransferenciaPreparada != null,
            gameManager.AcoesEnviadas,
            gameManager.PodeCancelarEnvio,
            gameManager.PodeEditarPreparacao,
            gameManager.jogadorLocal,
            origemId,
            destinoId,
            transferenciaId,
            gameManager.quantidadeAcaoSelecionada,
            gameManager.TipoAcaoSelecionadaEsperado,
            gameManager.feedbackTransferencia,
            jogadores,
            acoes,
            distribuicoes,
            transferencia,
            resultado,
            chave);
    }

    // ============================================================
    // 04. PROJEÇÕES SEM REFERÊNCIAS A GAMEOBJECTS
    // ============================================================

    private MatchUIPlayerState[] CriarJogadores(IReadOnlyList<TerritorioClique> territorios)
    {
        IReadOnlyList<TerritorioClique.Dono> jogadoresAtivos = gameManager.ObterJogadoresTransferencia();
        var resultado = new MatchUIPlayerState[jogadoresAtivos.Count];

        for (int i = 0; i < jogadoresAtivos.Count; i++)
        {
            TerritorioClique.Dono jogador = jogadoresAtivos[i];
            int controlados = 0;

            foreach (TerritorioClique territorio in territorios)
                if (territorio != null && territorio.dono == jogador)
                    controlados++;

            resultado[i] = new MatchUIPlayerState(
                jogador,
                EquipesJogadores.ObterEquipe(jogador),
                controlados,
                jogador == gameManager.jogadorLocal);
        }

        return resultado;
    }

    private MatchUIActionState[] CriarAcoes()
    {
        IReadOnlyList<OrdemTerrestre> ordens = gameManager.OrdensPreparadas;
        if (ordens == null)
            return Array.Empty<MatchUIActionState>();

        var resultado = new MatchUIActionState[ordens.Count];
        for (int i = 0; i < ordens.Count; i++)
        {
            OrdemTerrestre ordem = ordens[i];
            resultado[i] = new MatchUIActionState(
                ordem.PosicaoNaFila,
                ordem.IdOrigem,
                ordem.IdDestino,
                ordem.QuantidadePretendida,
                SistemaAcoesTerrestres.ObterTipoEsperado(ordem.Jogador, ordem.Destino));
        }

        return resultado;
    }

    private MatchUIReinforcementState[] CriarDistribuicoes()
    {
        IReadOnlyList<DistribuicaoReforco> distribuicoes = gameManager.DistribuicoesReforcos;
        var resultado = new MatchUIReinforcementState[distribuicoes.Count];

        for (int i = 0; i < distribuicoes.Count; i++)
        {
            DistribuicaoReforco distribuicao = distribuicoes[i];
            resultado[i] = new MatchUIReinforcementState(
                distribuicao.Id,
                ObterId(distribuicao.Territorio),
                distribuicao.Quantidade);
        }

        return resultado;
    }

    private MatchUITransferState CriarTransferencia()
    {
        OrdemTransferencia transferencia = gameManager.TransferenciaPreparada;
        return transferencia == null
            ? null
            : new MatchUITransferState(
                transferencia.IdTerritorio,
                transferencia.Autor,
                transferencia.Destinatario);
    }

    // ============================================================
    // 05. DETECÇÃO DETERMINÍSTICA DE MUDANÇAS
    // ============================================================

    private string CriarChaveConteudo(
        float tempo,
        MatchUIPlayerState[] jogadores,
        MatchUIActionState[] acoes,
        MatchUIReinforcementState[] distribuicoes,
        MatchUITransferState transferencia,
        MatchUIResultState resultado,
        string origemId,
        string destinoId,
        string transferenciaId)
    {
        var chave = new StringBuilder(512);
        chave.Append(gameManager.RodadaAtual).Append('|')
            .Append((int)gameManager.faseAtual).Append('|')
            .Append((int)gameManager.estadoPreparacao).Append('|')
            .Append((int)gameManager.modoAcao).Append('|')
            .Append((int)gameManager.EstadoAtualPartida).Append('|')
            .Append(gameManager.RoundMorteSubita).Append('|')
            .Append(tempo).Append('|')
            .Append(gameManager.reforcosDisponiveis).Append('|')
            .Append(gameManager.quantidadeAcaoSelecionada).Append('|')
            .Append(origemId).Append('|').Append(destinoId).Append('|')
            .Append(transferenciaId).Append('|').Append(gameManager.feedbackTransferencia);

        foreach (MatchUIPlayerState jogador in jogadores)
            chave.Append("|P:").Append((int)jogador.Jogador).Append(':').Append(jogador.TerritoriosControlados);

        foreach (MatchUIActionState acao in acoes)
            chave.Append("|A:").Append(acao.Posicao).Append(':').Append(acao.OrigemId)
                .Append(':').Append(acao.DestinoId).Append(':').Append(acao.Quantidade);

        foreach (MatchUIReinforcementState distribuicao in distribuicoes)
            chave.Append("|R:").Append(distribuicao.Id).Append(':')
                .Append(distribuicao.TerritorioId).Append(':').Append(distribuicao.Quantidade);

        if (transferencia != null)
            chave.Append("|T:").Append(transferencia.TerritorioId).Append(':')
                .Append((int)transferencia.Destinatario);

        if (resultado != null)
            chave.Append("|F:").Append(resultado.Encerrada).Append(':').Append(resultado.Empate)
                .Append(':').Append((int)resultado.Tipo).Append(':').Append(resultado.Territorios)
                .Append(':').Append(resultado.RoundFinal).Append(':').Append(resultado.HouveMorteSubita);

        return chave.ToString();
    }

    private static string ObterId(TerritorioClique territorio)
    {
        if (territorio == null)
            return string.Empty;

        return string.IsNullOrWhiteSpace(territorio.idTerritorio)
            ? territorio.name
            : territorio.idTerritorio;
    }
}
