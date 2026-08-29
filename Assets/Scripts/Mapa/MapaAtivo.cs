using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class MapaAtivo : MonoBehaviour
{
    // ============================================================
    // 1. CICLO DE VIDA E ESTADO DO MAPA ATIVO
    // ============================================================

    private const string CaminhoCatalogo = "Mapas/CatalogoMapas";
    private static MapaAtivo instance;

    [SerializeField] private DefinicaoMapa definicao;

    private readonly List<TerritorioClique> territoriosRuntime = new List<TerritorioClique>();
    private readonly Dictionary<string, TerritorioClique> territorioPorId = new Dictionary<string, TerritorioClique>(StringComparer.Ordinal);
    private readonly Dictionary<string, DefinicaoRegiaoMapa> regiaoPorId = new Dictionary<string, DefinicaoRegiaoMapa>(StringComparer.Ordinal);
    private SpriteRenderer visualMapa;

    public static MapaAtivo Instance => instance;
    public DefinicaoMapa Definicao => definicao;
    public IReadOnlyList<TerritorioClique> TerritoriosRuntime => territoriosRuntime;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CriarServico()
    {
        if (instance != null)
            return;

        GameObject objeto = new GameObject("MapaAtivo");
        DontDestroyOnLoad(objeto);
        instance = objeto.AddComponent<MapaAtivo>();

        CatalogoMapas catalogo = Resources.Load<CatalogoMapas>(CaminhoCatalogo);
        if (catalogo == null || catalogo.MapaPadrao == null)
        {
            Debug.LogError($"Catálogo de mapas ausente ou sem mapa padrão em Resources/{CaminhoCatalogo}.");
            return;
        }

        instance.definicao = catalogo.MapaPadrao;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        AtualizarReferenciasDaCena();
    }

    public void AtualizarReferenciasDaCena()
    {
        territoriosRuntime.Clear();
        territorioPorId.Clear();
        regiaoPorId.Clear();
        visualMapa = null;

        if (definicao == null)
            return;

        foreach (DefinicaoRegiaoMapa regiao in definicao.Regioes)
        {
            if (regiao != null && !string.IsNullOrWhiteSpace(regiao.id))
                regiaoPorId[regiao.id] = regiao;
        }

        TerritorioClique[] encontrados = FindObjectsByType<TerritorioClique>();
        foreach (TerritorioClique territorio in encontrados)
        {
            if (territorio == null || string.IsNullOrWhiteSpace(territorio.idTerritorio))
                continue;
            territorioPorId[territorio.idTerritorio] = territorio;
        }

        foreach (DefinicaoTerritorioMapa dados in definicao.Territorios)
        {
            if (dados != null && territorioPorId.TryGetValue(dados.id, out TerritorioClique territorio))
                territoriosRuntime.Add(territorio);
        }

        SpriteRenderer[] visuais = FindObjectsByType<SpriteRenderer>();
        foreach (SpriteRenderer candidato in visuais)
        {
            if (candidato.sprite == definicao.ArteBase)
            {
                visualMapa = candidato;
                break;
            }
        }

        if (territoriosRuntime.Count != definicao.Territorios.Count)
            Debug.LogError($"Mapa {definicao.MapaId}: esperados {definicao.Territorios.Count} territórios, encontrados {territoriosRuntime.Count} na cena.");
    }

    // ============================================================
    // 2. CONSULTAS UNIVERSAIS DE TERRITÓRIOS E CONEXÕES
    // ============================================================

    public static IReadOnlyList<TerritorioClique> ObterTerritoriosOuCena()
    {
        if (instance != null)
        {
            if (instance.territoriosRuntime.Count == 0)
                instance.AtualizarReferenciasDaCena();
            if (instance.territoriosRuntime.Count > 0)
                return instance.territoriosRuntime;
        }

        return FindObjectsByType<TerritorioClique>();
    }

    public bool TentarObterTerritorio(string id, out TerritorioClique territorio)
    {
        return territorioPorId.TryGetValue(id, out territorio);
    }

    public DefinicaoTerritorioMapa ObterDefinicaoTerritorio(string id)
    {
        if (definicao == null)
            return null;
        foreach (DefinicaoTerritorioMapa territorio in definicao.Territorios)
            if (territorio != null && territorio.id == id)
                return territorio;
        return null;
    }

    public DefinicaoRegiaoMapa ObterRegiao(string regiaoId)
    {
        regiaoPorId.TryGetValue(regiaoId, out DefinicaoRegiaoMapa regiao);
        return regiao;
    }

    public bool SaoConectados(string origemId, string destinoId, TipoConexaoMapa tipo)
    {
        if (definicao == null)
            return false;
        foreach (DefinicaoConexaoMapa conexao in definicao.Conexoes)
        {
            if (conexao == null || conexao.tipo != tipo)
                continue;
            if (conexao.origemId == origemId && conexao.destinoId == destinoId)
                return true;
            if (conexao.bidirecional && conexao.origemId == destinoId && conexao.destinoId == origemId)
                return true;
        }
        return false;
    }

    public bool JogadorControlaRegiao(TerritorioClique.Dono jogador, DefinicaoRegiaoMapa regiao)
    {
        if (regiao == null || regiao.territorioIds == null || regiao.territorioIds.Count == 0)
            return false;
        foreach (string territorioId in regiao.territorioIds)
            if (!territorioPorId.TryGetValue(territorioId, out TerritorioClique territorio) || territorio.dono != jogador)
                return false;
        return true;
    }

    // ============================================================
    // 3. REGIÕES, BÔNUS E ENQUADRAMENTO
    // ============================================================

    public int CalcularBonusRegioes(TerritorioClique.Dono jogador)
    {
        int total = 0;
        if (definicao == null)
            return total;
        foreach (DefinicaoRegiaoMapa regiao in definicao.Regioes)
            if (JogadorControlaRegiao(jogador, regiao))
                total += regiao.bonus;
        return total;
    }

    public bool TentarObterLimitesVisuais(out Bounds limites)
    {
        if (visualMapa == null)
            AtualizarReferenciasDaCena();

        if (visualMapa != null)
        {
            limites = visualMapa.bounds;
            return true;
        }
        limites = default;
        return false;
    }
}
