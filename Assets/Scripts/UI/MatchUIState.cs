using System;
using System.Collections.Generic;

// ============================================================
// 01. SNAPSHOT IMUTÁVEL DA PARTIDA
// ============================================================

public sealed class MatchUIState : IEquatable<MatchUIState>
{
    private readonly string chaveConteudo;

    public int Round { get; }
    public GameManager.FaseTurno Fase { get; }
    public GameManager.EstadoPreparacao EstadoPreparacao { get; }
    public GameManager.ModoAcao ModoAcao { get; }
    public GerenciadorRodada.EstadoPartida EstadoPartida { get; }
    public bool EmMorteSubita { get; }
    public int RoundMorteSubita { get; }
    public float TempoRestante { get; }
    public int ReforcosDisponiveis { get; }
    public int AcoesPreparadas { get; }
    public int LimiteAcoes { get; }
    public bool TransferenciaUsada { get; }
    public bool AcoesEnviadas { get; }
    public bool PodeCancelarEnvio { get; }
    public bool PodeEditarPreparacao { get; }
    public TerritorioClique.Dono JogadorLocal { get; }
    public string OrigemSelecionadaId { get; }
    public string DestinoSelecionadoId { get; }
    public string TerritorioTransferenciaId { get; }
    public int QuantidadeAcaoSelecionada { get; }
    public string TipoAcaoEsperado { get; }
    public string FeedbackTransferencia { get; }
    public IReadOnlyList<MatchUIPlayerState> Jogadores { get; }
    public IReadOnlyList<MatchUIActionState> Acoes { get; }
    public IReadOnlyList<MatchUIReinforcementState> Distribuicoes { get; }
    public MatchUITransferState Transferencia { get; }
    public MatchUIResultState Resultado { get; }

    internal MatchUIState(
        int round,
        GameManager.FaseTurno fase,
        GameManager.EstadoPreparacao estadoPreparacao,
        GameManager.ModoAcao modoAcao,
        GerenciadorRodada.EstadoPartida estadoPartida,
        bool emMorteSubita,
        int roundMorteSubita,
        float tempoRestante,
        int reforcosDisponiveis,
        int acoesPreparadas,
        int limiteAcoes,
        bool transferenciaUsada,
        bool acoesEnviadas,
        bool podeCancelarEnvio,
        bool podeEditarPreparacao,
        TerritorioClique.Dono jogadorLocal,
        string origemSelecionadaId,
        string destinoSelecionadoId,
        string territorioTransferenciaId,
        int quantidadeAcaoSelecionada,
        string tipoAcaoEsperado,
        string feedbackTransferencia,
        MatchUIPlayerState[] jogadores,
        MatchUIActionState[] acoes,
        MatchUIReinforcementState[] distribuicoes,
        MatchUITransferState transferencia,
        MatchUIResultState resultado,
        string chaveConteudo)
    {
        Round = round;
        Fase = fase;
        EstadoPreparacao = estadoPreparacao;
        ModoAcao = modoAcao;
        EstadoPartida = estadoPartida;
        EmMorteSubita = emMorteSubita;
        RoundMorteSubita = roundMorteSubita;
        TempoRestante = tempoRestante;
        ReforcosDisponiveis = reforcosDisponiveis;
        AcoesPreparadas = acoesPreparadas;
        LimiteAcoes = limiteAcoes;
        TransferenciaUsada = transferenciaUsada;
        AcoesEnviadas = acoesEnviadas;
        PodeCancelarEnvio = podeCancelarEnvio;
        PodeEditarPreparacao = podeEditarPreparacao;
        JogadorLocal = jogadorLocal;
        OrigemSelecionadaId = origemSelecionadaId ?? string.Empty;
        DestinoSelecionadoId = destinoSelecionadoId ?? string.Empty;
        TerritorioTransferenciaId = territorioTransferenciaId ?? string.Empty;
        QuantidadeAcaoSelecionada = quantidadeAcaoSelecionada;
        TipoAcaoEsperado = tipoAcaoEsperado ?? string.Empty;
        FeedbackTransferencia = feedbackTransferencia ?? string.Empty;
        Jogadores = Array.AsReadOnly(jogadores ?? Array.Empty<MatchUIPlayerState>());
        Acoes = Array.AsReadOnly(acoes ?? Array.Empty<MatchUIActionState>());
        Distribuicoes = Array.AsReadOnly(distribuicoes ?? Array.Empty<MatchUIReinforcementState>());
        Transferencia = transferencia;
        Resultado = resultado;
        this.chaveConteudo = chaveConteudo ?? string.Empty;
    }

    // ============================================================
    // 02. COMPARAÇÃO PARA PUBLICAÇÃO DE MUDANÇAS
    // ============================================================

    public bool Equals(MatchUIState other) =>
        other != null && chaveConteudo == other.chaveConteudo;

    public override bool Equals(object obj) => Equals(obj as MatchUIState);

    public override int GetHashCode() => chaveConteudo.GetHashCode();
}

// ============================================================
// 03. DADOS SIMPLES EXPOSTOS À APRESENTAÇÃO
// ============================================================

public readonly struct MatchUIPlayerState
{
    public TerritorioClique.Dono Jogador { get; }
    public EquipesJogadores.Equipe Equipe { get; }
    public int TerritoriosControlados { get; }
    public bool EhJogadorLocal { get; }

    public MatchUIPlayerState(
        TerritorioClique.Dono jogador,
        EquipesJogadores.Equipe equipe,
        int territoriosControlados,
        bool ehJogadorLocal)
    {
        Jogador = jogador;
        Equipe = equipe;
        TerritoriosControlados = territoriosControlados;
        EhJogadorLocal = ehJogadorLocal;
    }
}

public readonly struct MatchUIActionState
{
    public int Posicao { get; }
    public string OrigemId { get; }
    public string DestinoId { get; }
    public int Quantidade { get; }
    public string TipoEsperado { get; }

    public MatchUIActionState(int posicao, string origemId, string destinoId, int quantidade, string tipoEsperado)
    {
        Posicao = posicao;
        OrigemId = origemId ?? string.Empty;
        DestinoId = destinoId ?? string.Empty;
        Quantidade = quantidade;
        TipoEsperado = tipoEsperado ?? string.Empty;
    }
}

public readonly struct MatchUIReinforcementState
{
    public int Id { get; }
    public string TerritorioId { get; }
    public int Quantidade { get; }

    public MatchUIReinforcementState(int id, string territorioId, int quantidade)
    {
        Id = id;
        TerritorioId = territorioId ?? string.Empty;
        Quantidade = quantidade;
    }
}

public sealed class MatchUITransferState
{
    public string TerritorioId { get; }
    public TerritorioClique.Dono Autor { get; }
    public TerritorioClique.Dono Destinatario { get; }

    public MatchUITransferState(string territorioId, TerritorioClique.Dono autor, TerritorioClique.Dono destinatario)
    {
        TerritorioId = territorioId ?? string.Empty;
        Autor = autor;
        Destinatario = destinatario;
    }
}

public sealed class MatchUIResultState
{
    public bool Encerrada { get; }
    public bool Empate { get; }
    public ResultadoPartida.TipoVencedor Tipo { get; }
    public TerritorioClique.Dono JogadorVencedor { get; }
    public EquipesJogadores.Equipe EquipeVencedora { get; }
    public int Territorios { get; }
    public int RoundFinal { get; }
    public bool HouveMorteSubita { get; }

    public MatchUIResultState(ResultadoPartida resultado)
    {
        Encerrada = resultado.Encerrada;
        Empate = resultado.Empate;
        Tipo = resultado.Tipo;
        JogadorVencedor = resultado.JogadorVencedor;
        EquipeVencedora = resultado.EquipeVencedora;
        Territorios = resultado.QuantidadeTerritorios;
        RoundFinal = resultado.RoundFinal;
        HouveMorteSubita = resultado.HouveMorteSubita;
    }
}
