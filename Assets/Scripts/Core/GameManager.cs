using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("DEBUG")]
    public bool modoTesteCliquesTodosTerritorios = false;

    public static GameManager instance;

    // =====================================================
    // SELEÇÃO ATUAL
    // =====================================================

    public TerritorioClique territorioSelecionado;
    public TerritorioClique territorioDestinoSelecionado;

    public TerritorioClique.Dono jogadorLocal =
        TerritorioClique.Dono.Jogador1;

    // =====================================================
    // SISTEMAS
    // =====================================================

    private FilaAcoes filaAcoes;
    private SistemaAtaque sistemaAtaque;
    private ResolvedorCombate resolvedorCombate;
    private GerenciadorRodada gerenciadorRodada;

    // =====================================================
    // FASES
    // =====================================================

    public enum FaseTurno
    {
        Reforco,
        Ataque,
        Resolucao
    }

    public enum ModoAcao
    {
        Nenhum,
        Atacar,
        Mover,
        Transferir
    }

    public FaseTurno faseAtual =
        FaseTurno.Reforco;

    public ModoAcao modoAcao =
        ModoAcao.Nenhum;

    // =====================================================
    // REFORÇOS
    // =====================================================

    public int reforcosDisponiveis = 0;

    // Guarda SOMENTE reforços adicionados na rodada atual.
    private readonly Dictionary<TerritorioClique, int>
        reforcosAplicadosNaRodada =
            new Dictionary<TerritorioClique, int>();

    // Território escolhido para possível desfazer reforço.
    private TerritorioClique territorioReforcadoSelecionado;

    public TerritorioClique TerritorioReforcadoSelecionado =>
        territorioReforcadoSelecionado;

    public int ReforcosNoTerritorioSelecionado
    {
        get
        {
            if (territorioReforcadoSelecionado == null)
                return 0;

            if (reforcosAplicadosNaRodada.TryGetValue(
                    territorioReforcadoSelecionado,
                    out int quantidade))
            {
                return quantidade;
            }

            return 0;
        }
    }

    public bool PodeConfirmarReforcos =>
        faseAtual == FaseTurno.Reforco &&
        reforcosDisponiveis == 0;

    // =====================================================
    // ATAQUE
    // =====================================================

    [Min(1)]
    public int quantidadeAtaqueSelecionada = 1;

    // =====================================================
    // CRONÔMETRO
    // =====================================================

    [Header("Preparação")]
    [SerializeField]
    private float duracaoPreparacao = 90f;

    private float tempoPreparacaoRestante;

    private bool acoesEnviadas;

    public float TempoPreparacaoRestante =>
        tempoPreparacaoRestante;

    // =====================================================
    // HUD
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
    // CRONÔMETRO
    // =====================================================

    private void Update()
    {
        if (faseAtual ==
            FaseTurno.Resolucao)
            return;

        if (acoesEnviadas)
            return;

        tempoPreparacaoRestante -=
            Time.deltaTime;

        if (tempoPreparacaoRestante <= 0f)
        {
            tempoPreparacaoRestante = 0f;

            // Se o tempo acabar durante reforços,
            // confirma automaticamente apenas se todos
            // os reforços já tiverem sido distribuídos.
            if (faseAtual == FaseTurno.Reforco)
            {
                if (reforcosDisponiveis == 0)
                {
                    ConfirmarReforcos();
                }

                return;
            }

            EnviarAcoes();
        }
    }

    public void IniciarCronometroRound()
    {
        tempoPreparacaoRestante =
            duracaoPreparacao;

        acoesEnviadas = false;

        modoAcao =
            ModoAcao.Atacar;

        quantidadeAtaqueSelecionada = 1;

        CancelarSelecaoAtual();

        Debug.Log(
            "ROUND INICIADO | Cronômetro: " +
            duracaoPreparacao +
            " segundos."
        );
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

        // Nova fase de reforço = novo histórico.
        reforcosAplicadosNaRodada.Clear();

        territorioReforcadoSelecionado = null;
    }

    public void TentarAdicionarReforco(
        TerritorioClique territorio)
    {
        if (territorio == null)
            return;

        // =================================================
        // DEBUG TEMPORÁRIO
        // =================================================

        if (modoTesteCliquesTodosTerritorios)
        {
            territorio.AdicionarTropa();

            Debug.Log(
                "TESTE CLIQUE OK | " +
                territorio.name +
                " | Tropas: " +
                territorio.Tropas
            );

            return;
        }

        // =================================================
        // REGRA NORMAL
        // =================================================

        if (faseAtual !=
            FaseTurno.Reforco)
            return;

        if (territorio.dono != jogadorLocal)
            return;

        if (reforcosDisponiveis <= 0)
        {
            // Mesmo sem reforços disponíveis,
            // tocar em um território já reforçado
            // permite selecioná-lo para desfazer.
            SelecionarTerritorioReforcado(
                territorio
            );

            return;
        }

        territorio.AdicionarTropa();

        reforcosDisponiveis--;

        if (!reforcosAplicadosNaRodada.ContainsKey(
                territorio))
        {
            reforcosAplicadosNaRodada.Add(
                territorio,
                0
            );
        }

        reforcosAplicadosNaRodada[territorio]++;

        territorioReforcadoSelecionado =
            territorio;

        Debug.Log(
            "Tropa adicionada em " +
            territorio.name +
            " | Tropas: " +
            territorio.Tropas +
            " | Adicionadas nesta rodada: " +
            reforcosAplicadosNaRodada[territorio] +
            " | Reforços restantes: " +
            reforcosDisponiveis
        );

        // IMPORTANTE:
        // NÃO libera ataque automaticamente quando chega a 0.
        // O jogador confirma pelo HUD.
    }

    public void SelecionarTerritorioReforcado(
        TerritorioClique territorio)
    {
        if (faseAtual != FaseTurno.Reforco)
            return;

        if (territorio == null)
            return;

        if (territorio.dono != jogadorLocal)
            return;

        if (!reforcosAplicadosNaRodada.ContainsKey(
                territorio))
        {
            territorioReforcadoSelecionado = null;
            return;
        }

        territorioReforcadoSelecionado =
            territorio;

        Debug.Log(
            "REFORÇO SELECIONADO | " +
            territorio.name +
            " | Reforços aplicados: " +
            reforcosAplicadosNaRodada[territorio]
        );
    }

    public void DesfazerReforcosTerritorioSelecionado()
    {
        if (faseAtual != FaseTurno.Reforco)
            return;

        if (territorioReforcadoSelecionado == null)
            return;

        if (!reforcosAplicadosNaRodada.TryGetValue(
                territorioReforcadoSelecionado,
                out int quantidade))
        {
            return;
        }

        if (quantidade <= 0)
            return;

        // TerritorioClique atual não possui, pelo que temos aqui,
        // método público para remover várias tropas.
        // Usamos RemoverTropa() repetidamente.
        for (int i = 0; i < quantidade; i++)
        {
            territorioReforcadoSelecionado
                .RemoverTropa();
        }

        reforcosDisponiveis +=
            quantidade;

        string nomeTerritorio =
            territorioReforcadoSelecionado.name;

        reforcosAplicadosNaRodada.Remove(
            territorioReforcadoSelecionado
        );

        territorioReforcadoSelecionado = null;

        Debug.Log(
            "REFORÇO DESFEITO | " +
            nomeTerritorio +
            " | " +
            quantidade +
            " tropa(s) devolvida(s) | " +
            "Reforços disponíveis: " +
            reforcosDisponiveis
        );
    }

    public void ConfirmarReforcos()
    {
        if (faseAtual != FaseTurno.Reforco)
            return;

        if (reforcosDisponiveis > 0)
        {
            Debug.Log(
                "Ainda existem " +
                reforcosDisponiveis +
                " reforço(s) para distribuir."
            );

            return;
        }

        territorioReforcadoSelecionado = null;

        reforcosAplicadosNaRodada.Clear();

        LiberarAcoes();
    }

    private void LiberarAcoes()
    {
        faseAtual =
            FaseTurno.Ataque;

        modoAcao =
            ModoAcao.Atacar;

        quantidadeAtaqueSelecionada = 1;

        CancelarSelecaoAtual();

        Debug.Log(
            "REFORÇOS CONFIRMADOS | " +
            "Ações liberadas | Tempo restante: " +
            Mathf.CeilToInt(
                tempoPreparacaoRestante
            ) +
            " s."
        );
    }

    // =====================================================
    // MODOS
    // =====================================================

    public void SelecionarModoAtacar()
    {
        if (faseAtual !=
            FaseTurno.Ataque)
            return;

        modoAcao =
            ModoAcao.Atacar;

        CancelarSelecaoAtual();
    }

    // =====================================================
    // QUANTIDADE
    // =====================================================

    public void AumentarQuantidadeAtaque()
    {
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
    // CLIQUE NO MAPA
    // =====================================================

    public void ClicarTerritorio(
        TerritorioClique territorio)
    {
        if (territorio == null)
            return;

        if (faseAtual ==
            FaseTurno.Resolucao)
            return;

        if (faseAtual ==
            FaseTurno.Reforco)
        {
            SelecionarTerritorioReforcado(
                territorio
            );

            return;
        }

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

            Debug.Log(
                "ORIGEM SELECIONADA: " +
                territorio.name +
                " | Tropas disponíveis: " +
                disponiveis
            );

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

            Debug.Log(
                "NOVA ORIGEM: " +
                territorio.name +
                " | Tropas disponíveis: " +
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

        Debug.Log(
            "DESTINO SELECIONADO: " +
            territorio.name +
            " | Defesa: " +
            territorio.Tropas +
            " | Ataque selecionado: " +
            quantidadeAtaqueSelecionada
        );
    }

    // =====================================================
    // CONFIRMAR ATAQUE
    // =====================================================

    public void ConfirmarAtaquePreparado()
    {
        if (faseAtual !=
            FaseTurno.Ataque)
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

        Debug.Log(
            "ATAQUE CONFIRMADO | " +
            territorioSelecionado.name +
            " -> " +
            territorioDestinoSelecionado.name +
            " | Tropas: " +
            quantidadeAtaqueSelecionada +
            " | Ordem " +
            QuantidadeOrdensPreparadas +
            "/3"
        );

        CancelarSelecaoAtual();

        quantidadeAtaqueSelecionada = 1;
    }

    // =====================================================
    // CANCELAR SELEÇÃO
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
    // CANCELAR ÚLTIMA ORDEM
    // =====================================================

    public void CancelarUltimaOrdem()
    {
        if (filaAcoes == null)
            return;

        filaAcoes.RemoverUltimoAtaque();
    }

    // =====================================================
    // ENVIAR AÇÕES
    // =====================================================

    public void EnviarAcoes()
    {
        if (faseAtual ==
            FaseTurno.Resolucao)
            return;

        if (acoesEnviadas)
            return;

        acoesEnviadas = true;

        CancelarSelecaoAtual();

        Debug.Log(
            "AÇÕES ENVIADAS | " +
            QuantidadeOrdensPreparadas +
            " ordem(ns)."
        );

        ResolverRodadaAgora();
    }

    // =====================================================
    // RESOLUÇÃO
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

        resolvedorCombate
            .Resolver(
                filaAcoes
            );

        CancelarSelecaoAtual();

        gerenciadorRodada
            .IniciarProximaRodada();
    }
}