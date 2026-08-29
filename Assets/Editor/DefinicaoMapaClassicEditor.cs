using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class DefinicaoMapaClassicEditor
{
    // ============================================================
    // 1. MIGRAÇÃO E VALIDAÇÃO DO CLASSIC
    // ============================================================

    private const string Cena = "Assets/WarGame.unity";
    private const string PastaResources = "Assets/Resources";
    private const string PastaMapas = "Assets/Resources/Mapas";
    private const string CaminhoMapa = PastaMapas + "/Classic.asset";
    private const string CaminhoCatalogo = PastaMapas + "/CatalogoMapas.asset";

    [MenuItem("WarGame/Mapas/Gerar ou Atualizar Classic da Cena %#g")]
    public static void GerarOuAtualizarClassic()
    {
        if (SceneManager.GetActiveScene().path != Cena)
            EditorSceneManager.OpenScene(Cena, OpenSceneMode.Single);

        GarantirPastas();
        TerritorioClique[] territoriosCena = UnityEngine.Object.FindObjectsByType<TerritorioClique>();
        Array.Sort(territoriosCena, (a, b) => string.CompareOrdinal(a.idTerritorio, b.idTerritorio));

        Dictionary<TerritorioClique.Continente, DefinicaoRegiaoMapa> regioesPorLegado = CriarRegioes();
        DefinicaoMapa mapaExistente = AssetDatabase.LoadAssetAtPath<DefinicaoMapa>(CaminhoMapa);
        Dictionary<string, DefinicaoTerritorioMapa> dadosExistentes = new Dictionary<string, DefinicaoTerritorioMapa>(StringComparer.Ordinal);
        if (mapaExistente != null)
        {
            foreach (DefinicaoTerritorioMapa dados in mapaExistente.Territorios)
                if (dados != null && !string.IsNullOrWhiteSpace(dados.id))
                    dadosExistentes[dados.id] = dados;
        }

        List<DefinicaoTerritorioMapa> territorios = new List<DefinicaoTerritorioMapa>();
        foreach (TerritorioClique territorio in territoriosCena)
        {
            DefinicaoRegiaoMapa regiao = regioesPorLegado[territorio.continente];
            Transform contador = territorio.transform.Find("ContadorModerno");
            DefinicaoTerritorioMapa dados = new DefinicaoTerritorioMapa
            {
                id = territorio.idTerritorio,
                nomeExibido = territorio.gameObject.name,
                chaveTraducao = territorio.chaveTraducao,
                regiaoId = regiao.id,
                posicaoContador = contador != null ? (Vector2)contador.localPosition : Vector2.zero
            };
            if (dadosExistentes.TryGetValue(dados.id, out DefinicaoTerritorioMapa anterior))
            {
                dados.posicaoNomeManual = anterior.posicaoNomeManual;
                dados.possuiPosicaoNomeManual = anterior.possuiPosicaoNomeManual;
                dados.tamanhoFonteNome = anterior.tamanhoFonteNome;
            }
            territorios.Add(dados);
            regiao.territorioIds.Add(dados.id);
        }

        List<DefinicaoConexaoMapa> conexoes = CriarConexoes(territoriosCena);
        GameObject visual = GameObject.Find("classic_0");
        SpriteRenderer sprite = visual != null ? visual.GetComponent<SpriteRenderer>() : null;
        Camera camera = Camera.main != null ? Camera.main : UnityEngine.Object.FindAnyObjectByType<Camera>();

        DefinicaoMapa mapa = mapaExistente;
        if (mapa == null)
        {
            mapa = ScriptableObject.CreateInstance<DefinicaoMapa>();
            AssetDatabase.CreateAsset(mapa, CaminhoMapa);
        }

        mapa.ConfigurarNoEditor(
            "classic",
            "classic",
            "Classic",
            "Mapa clássico migrado da cena WarGame sem alteração de layout.",
            "classic",
            sprite != null ? sprite.sprite : null,
            camera != null ? (Vector2)camera.transform.position : Vector2.zero,
            camera != null ? camera.orthographicSize : 5f,
            territorios,
            new List<DefinicaoRegiaoMapa>(regioesPorLegado.Values),
            conexoes);
        EditorUtility.SetDirty(mapa);

        CatalogoMapas catalogo = AssetDatabase.LoadAssetAtPath<CatalogoMapas>(CaminhoCatalogo);
        if (catalogo == null)
        {
            catalogo = ScriptableObject.CreateInstance<CatalogoMapas>();
            AssetDatabase.CreateAsset(catalogo, CaminhoCatalogo);
        }
        catalogo.ConfigurarNoEditor(mapa, new List<DefinicaoMapa> { mapa });
        EditorUtility.SetDirty(catalogo);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Validar(mapa);
        Debug.Log($"MAPA CLASSIC | Gerado com {territorios.Count} territórios, {regioesPorLegado.Count} regiões e {conexoes.Count} conexões.");
    }

    [MenuItem("WarGame/Mapas/Validar Classic")]
    public static void ValidarClassic()
    {
        Validar(AssetDatabase.LoadAssetAtPath<DefinicaoMapa>(CaminhoMapa));
    }

    [MenuItem("WarGame/Mapas/Teste Editor/Preparar Controle de Regiao")]
    public static void PrepararControleDeRegiao()
    {
        if (!EditorApplication.isPlaying || MapaAtivo.Instance == null || GameManager.instance == null)
        {
            Debug.LogError("TESTE REGIÃO | Execute em Play Mode com a partida carregada.");
            return;
        }

        DefinicaoMapa mapa = MapaAtivo.Instance.Definicao;
        if (mapa == null || mapa.Regioes.Count == 0)
        {
            Debug.LogError("TESTE REGIÃO | O mapa ativo não possui regiões.");
            return;
        }

        DefinicaoRegiaoMapa regiao = mapa.Regioes[0];
        TerritorioClique.Dono jogador = GameManager.instance.jogadorLocal;
        foreach (string id in regiao.territorioIds)
            if (MapaAtivo.Instance.TentarObterTerritorio(id, out TerritorioClique territorio))
                territorio.DefinirDono(jogador);

        GerenciadorRodada rodadas = UnityEngine.Object.FindAnyObjectByType<GerenciadorRodada>();
        if (rodadas == null)
        {
            Debug.LogError("TESTE REGIÃO | GerenciadorRodada não encontrado.");
            return;
        }

        int total = rodadas.CalcularReforcosRegulares(jogador, out int baseTerritorial, out int bonusRegioes);
        Debug.Log($"TESTE REGIÃO | {regiao.nomeExibido} controlada por {jogador} | Base territorial {baseTerritorial} + bônus de regiões {bonusRegioes} = total {total}.");
    }

    public static void GerarClassicBatch()
    {
        GerarOuAtualizarClassic();
    }

    private static void Validar(DefinicaoMapa mapa)
    {
        List<ProblemaValidacaoMapa> problemas = ValidadorDefinicaoMapa.Validar(mapa);
        foreach (ProblemaValidacaoMapa problema in problemas)
        {
            if (problema.Severidade == SeveridadeProblemaMapa.Erro)
                Debug.LogError("VALIDADOR MAPA | " + problema);
            else
                Debug.LogWarning("VALIDADOR MAPA | " + problema);
        }
        if (problemas.Count == 0)
            Debug.Log("VALIDADOR MAPA | Classic válido, sem problemas.");
    }

    private static Dictionary<TerritorioClique.Continente, DefinicaoRegiaoMapa> CriarRegioes()
    {
        return new Dictionary<TerritorioClique.Continente, DefinicaoRegiaoMapa>
        {
            [TerritorioClique.Continente.AmericaDoNorte] = Regiao("america_norte", "América do Norte", "continent_north_america", 5, 0, new Color(0.25f, 0.65f, 1f)),
            [TerritorioClique.Continente.AmericaDoSul] = Regiao("america_sul", "América do Sul", "continent_south_america", 2, 1, new Color(0.3f, 0.85f, 0.35f)),
            [TerritorioClique.Continente.Africa] = Regiao("africa", "África", "continent_africa", 3, 2, new Color(1f, 0.65f, 0.2f)),
            [TerritorioClique.Continente.Europa] = Regiao("europa", "Europa", "continent_europe", 5, 3, new Color(0.65f, 0.4f, 1f)),
            [TerritorioClique.Continente.Asia] = Regiao("asia", "Ásia", "continent_asia", 7, 4, new Color(1f, 0.35f, 0.35f)),
            [TerritorioClique.Continente.Oceania] = Regiao("oceania", "Oceania", "continent_oceania", 2, 5, new Color(0.2f, 0.85f, 0.85f))
        };
    }

    private static DefinicaoRegiaoMapa Regiao(string id, string nome, string traducao, int bonus, int ordem, Color cor)
    {
        return new DefinicaoRegiaoMapa { id = id, nomeExibido = nome, descricao = nome, chaveTraducao = traducao, bonus = bonus, ordemExibicao = ordem, corDestaque = cor };
    }

    private static List<DefinicaoConexaoMapa> CriarConexoes(TerritorioClique[] territorios)
    {
        List<DefinicaoConexaoMapa> resultado = new List<DefinicaoConexaoMapa>();
        HashSet<string> pares = new HashSet<string>(StringComparer.Ordinal);
        foreach (TerritorioClique origem in territorios)
        {
            TerritorioFronteiras fronteiras = origem.GetComponent<TerritorioFronteiras>();
            if (fronteiras == null || fronteiras.VizinhosLegados == null)
                continue;
            foreach (TerritorioClique destino in fronteiras.VizinhosLegados)
            {
                if (destino == null)
                    continue;
                string primeiro = origem.idTerritorio;
                string segundo = destino.idTerritorio;
                if (string.CompareOrdinal(primeiro, segundo) > 0)
                    (primeiro, segundo) = (segundo, primeiro);
                if (!pares.Add(primeiro + "|" + segundo))
                    continue;
                resultado.Add(new DefinicaoConexaoMapa { origemId = primeiro, destinoId = segundo, tipo = TipoConexaoMapa.Terrestre, bidirecional = true });
            }
        }
        return resultado;
    }

    private static void GarantirPastas()
    {
        if (!AssetDatabase.IsValidFolder(PastaResources))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder(PastaMapas))
            AssetDatabase.CreateFolder(PastaResources, "Mapas");
    }
}
