#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class MapaTeste12Editor
{
    // Ferramenta técnica temporária e genérica de prova multi-mapa.
    private const string TriggerPath = "Temp/GerarMapaTeste12.trigger";
    private const string CenaClassic = "Assets/WarGame.unity";
    private const string PastaMapa = "Assets/Mapas/MapaTeste12";
    private const string PastaArte = PastaMapa + "/Arte";
    private const string PastaMascaras = PastaMapa + "/Mascaras";
    private const string ArtePath = PastaArte + "/MapaTeste12.png";
    private const string CenaTeste = PastaMapa + "/MapaTeste12.unity";
    private const string DefinicaoPath = "Assets/Resources/Mapas/MapaTeste12.asset";
    private const string CatalogoPath = "Assets/Resources/Mapas/CatalogoMapas.asset";
    private const string ClassicPath = "Assets/Resources/Mapas/Classic.asset";
    private const string IdMapa = "mapa_teste_12";

    private static readonly string[] Ids =
    {
        "Alpha1", "Alpha2", "Alpha3", "Alpha4",
        "Beta1", "Beta2", "Beta3", "Beta4",
        "Gamma1", "Gamma2", "Gamma3", "Gamma4"
    };

    private static readonly Vector2[] Posicoes =
    {
        new Vector2(-4.5f, 2.6f), new Vector2(-1.5f, 2.6f), new Vector2(1.5f, 2.6f), new Vector2(4.5f, 2.6f),
        new Vector2(-4.5f, 0f), new Vector2(-1.5f, 0f), new Vector2(1.5f, 0f), new Vector2(4.5f, 0f),
        new Vector2(-4.5f, -2.6f), new Vector2(-1.5f, -2.6f), new Vector2(1.5f, -2.6f), new Vector2(4.5f, -2.6f)
    };

    static MapaTeste12Editor()
    {
        if (File.Exists(Absoluto(TriggerPath)))
            EditorApplication.delayCall += ExecutarGeracaoAgendada;
    }

    public static void GerarOuAtualizarMapaTeste12()
    {
        GarantirPastas();
        GerarArteTecnica();
        DefinicaoMapa definicao = GerarDefinicao();
        AtualizarCatalogo(definicao);
        GerarCena(definicao);
        definicao = AssetDatabase.LoadAssetAtPath<DefinicaoMapa>(DefinicaoPath);
        ValidarMapa("MapaTeste12", definicao, 12, 3, 12);
        Debug.Log("MAPA_TESTE_12 | Gerado com 12 territórios, 3 regiões e 12 conexões.");
    }

    private static void ValidarMapaTeste12()
    {
        ValidarMapa(
            "MapaTeste12",
            AssetDatabase.LoadAssetAtPath<DefinicaoMapa>(DefinicaoPath),
            12,
            3,
            12);
    }

    private static void ValidarAmbos()
    {
        ValidarMapa("Classic", AssetDatabase.LoadAssetAtPath<DefinicaoMapa>(ClassicPath), 42, 6, 83);
        ValidarMapa("MapaTeste12", AssetDatabase.LoadAssetAtPath<DefinicaoMapa>(DefinicaoPath), 12, 3, 12);
    }

    public static void GerarBatch()
    {
        GerarOuAtualizarMapaTeste12();
        Debug.Log("MAPA_TESTE_12_BATCH_OK");
        EditorApplication.Exit(0);
    }

    public static void ValidarAmbosBatch()
    {
        ValidarAmbos();
        Debug.Log("MAPAS_CLASSIC_E_TESTE_12_BATCH_OK");
        EditorApplication.Exit(0);
    }

    private static void ExecutarGeracaoAgendada()
    {
        if (!File.Exists(Absoluto(TriggerPath)))
            return;

        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.delayCall += ExecutarGeracaoAgendada;
            return;
        }

        try
        {
            GerarOuAtualizarMapaTeste12();
            File.Delete(Absoluto(TriggerPath));
            Debug.Log("MAPA_TESTE_12_TRIGGER_OK");
        }
        catch (Exception excecao)
        {
            Debug.LogError("MAPA_TESTE_12_TRIGGER_ERRO | " + excecao);
        }
    }

    private static void GarantirPastas()
    {
        GarantirPasta("Assets", "Mapas");
        GarantirPasta("Assets/Mapas", "MapaTeste12");
        GarantirPasta(PastaMapa, "Arte");
        GarantirPasta(PastaMapa, "Mascaras");
    }

    private static void GarantirPasta(string pai, string nome)
    {
        string caminho = pai + "/" + nome;
        if (!AssetDatabase.IsValidFolder(caminho))
            AssetDatabase.CreateFolder(pai, nome);
    }

    private static void GerarArteTecnica()
    {
        const int largura = 1365;
        const int altura = 1024;
        Texture2D arte = new Texture2D(largura, altura, TextureFormat.RGBA32, false, true);
        Color32[] pixels = new Color32[largura * altura];
        Color32 fundo = new Color32(20, 27, 38, 255);
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = fundo;

        Color32[] coresRegiao =
        {
            new Color32(28, 82, 112, 255),
            new Color32(92, 65, 35, 255),
            new Color32(65, 45, 96, 255)
        };
        Color32[] contornos =
        {
            new Color32(70, 190, 235, 255),
            new Color32(235, 170, 70, 255),
            new Color32(180, 110, 235, 255)
        };

        for (int i = 0; i < Posicoes.Length; i++)
        {
            int centroX = Mathf.RoundToInt(largura * 0.5f + Posicoes[i].x * 100f);
            int centroY = Mathf.RoundToInt(altura * 0.5f + Posicoes[i].y * 100f);
            DesenharRetangulo(pixels, largura, altura, centroX, centroY, 230, 165, coresRegiao[i / 4], contornos[i / 4], 5);
        }

        arte.SetPixels32(pixels);
        arte.Apply(false, false);
        File.WriteAllBytes(Absoluto(ArtePath), arte.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(arte);

        Texture2D mascara = new Texture2D(230, 165, TextureFormat.RGBA32, false, true);
        Color32[] pixelsMascara = new Color32[230 * 165];
        Color32 branco = new Color32(255, 255, 255, 255);
        for (int y = 5; y < 160; y++)
            for (int x = 5; x < 225; x++)
                pixelsMascara[y * 230 + x] = branco;
        mascara.SetPixels32(pixelsMascara);
        mascara.Apply(false, false);
        byte[] mascaraPng = mascara.EncodeToPNG();
        UnityEngine.Object.DestroyImmediate(mascara);

        foreach (string id in Ids)
            File.WriteAllBytes(Absoluto(PastaMascaras + "/" + id + ".png"), mascaraPng);

        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        ConfigurarSprite(ArtePath, 100f, FilterMode.Bilinear);
        foreach (string id in Ids)
            ConfigurarSprite(PastaMascaras + "/" + id + ".png", 100f, FilterMode.Point);
    }

    private static void DesenharRetangulo(
        Color32[] pixels,
        int largura,
        int altura,
        int centroX,
        int centroY,
        int tamanhoX,
        int tamanhoY,
        Color32 preenchimento,
        Color32 contorno,
        int espessura)
    {
        int minX = Mathf.Clamp(centroX - tamanhoX / 2, 0, largura - 1);
        int maxX = Mathf.Clamp(centroX + tamanhoX / 2, 0, largura - 1);
        int minY = Mathf.Clamp(centroY - tamanhoY / 2, 0, altura - 1);
        int maxY = Mathf.Clamp(centroY + tamanhoY / 2, 0, altura - 1);
        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                bool borda = x - minX < espessura || maxX - x < espessura || y - minY < espessura || maxY - y < espessura;
                pixels[y * largura + x] = borda ? contorno : preenchimento;
            }
        }
    }

    private static void ConfigurarSprite(string path, float pixelsPorUnidade, FilterMode filtro)
    {
        TextureImporter importador = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importador == null)
            throw new InvalidOperationException("TextureImporter ausente: " + path);
        importador.textureType = TextureImporterType.Sprite;
        importador.spriteImportMode = SpriteImportMode.Single;
        importador.spritePixelsPerUnit = pixelsPorUnidade;
        importador.alphaIsTransparency = true;
        importador.mipmapEnabled = false;
        importador.textureCompression = TextureImporterCompression.Uncompressed;
        importador.filterMode = filtro;
        importador.SaveAndReimport();
    }

    private static DefinicaoMapa GerarDefinicao()
    {
        List<DefinicaoTerritorioMapa> territorios = new List<DefinicaoTerritorioMapa>();
        for (int i = 0; i < Ids.Length; i++)
        {
            territorios.Add(new DefinicaoTerritorioMapa
            {
                id = Ids[i],
                nomeExibido = Ids[i],
                chaveTraducao = "territory_" + Ids[i].ToLowerInvariant(),
                regiaoId = i < 4 ? "alpha" : i < 8 ? "beta" : "gamma",
                posicaoContador = Vector2.zero,
                possuiPosicaoContador = true
            });
        }

        List<DefinicaoRegiaoMapa> regioes = new List<DefinicaoRegiaoMapa>
        {
            CriarRegiao("alpha", "Alpha", 0, new Color(0.25f, 0.75f, 0.95f), 0),
            CriarRegiao("beta", "Beta", 4, new Color(0.95f, 0.65f, 0.25f), 1),
            CriarRegiao("gamma", "Gamma", 8, new Color(0.7f, 0.4f, 0.95f), 2)
        };

        List<DefinicaoConexaoMapa> conexoes = new List<DefinicaoConexaoMapa>();
        for (int inicio = 0; inicio < 12; inicio += 4)
            for (int i = inicio; i < inicio + 3; i++)
                conexoes.Add(Conexao(Ids[i], Ids[i + 1]));
        conexoes.Add(Conexao("Alpha4", "Beta1"));
        conexoes.Add(Conexao("Beta4", "Gamma1"));
        conexoes.Add(Conexao("Gamma4", "Alpha1"));

        DefinicaoMapa mapa = AssetDatabase.LoadAssetAtPath<DefinicaoMapa>(DefinicaoPath);
        if (mapa == null)
        {
            mapa = ScriptableObject.CreateInstance<DefinicaoMapa>();
            AssetDatabase.CreateAsset(mapa, DefinicaoPath);
        }

        mapa.ConfigurarNoEditor(
            IdMapa,
            "MapaTeste12",
            "Mapa Teste 12",
            "Mapa técnico temporário para validar a arquitetura multi-mapa.",
            "teste_tecnico",
            AssetDatabase.LoadAssetAtPath<Sprite>(ArtePath),
            Vector2.zero,
            5.12f,
            territorios,
            regioes,
            conexoes);
        EditorUtility.SetDirty(mapa);
        AssetDatabase.SaveAssetIfDirty(mapa);
        return mapa;
    }

    private static DefinicaoRegiaoMapa CriarRegiao(string id, string nome, int inicio, Color cor, int ordem)
    {
        DefinicaoRegiaoMapa regiao = new DefinicaoRegiaoMapa
        {
            id = id,
            nomeExibido = nome,
            descricao = "Região técnica " + nome,
            chaveTraducao = "region_" + id,
            bonus = 2,
            ordemExibicao = ordem,
            corDestaque = cor
        };
        for (int i = inicio; i < inicio + 4; i++)
            regiao.territorioIds.Add(Ids[i]);
        return regiao;
    }

    private static DefinicaoConexaoMapa Conexao(string origem, string destino)
    {
        return new DefinicaoConexaoMapa
        {
            origemId = origem,
            destinoId = destino,
            tipo = TipoConexaoMapa.Terrestre,
            bidirecional = true
        };
    }

    private static void AtualizarCatalogo(DefinicaoMapa teste)
    {
        CatalogoMapas catalogo = AssetDatabase.LoadAssetAtPath<CatalogoMapas>(CatalogoPath);
        DefinicaoMapa classic = AssetDatabase.LoadAssetAtPath<DefinicaoMapa>(ClassicPath);
        if (catalogo == null || classic == null)
            throw new InvalidOperationException("Catálogo ou Classic ausente.");
        catalogo.ConfigurarNoEditor(classic, new List<DefinicaoMapa> { classic, teste });
        EditorUtility.SetDirty(catalogo);
        AssetDatabase.SaveAssetIfDirty(catalogo);
    }

    private static void GerarCena(DefinicaoMapa definicao)
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(CenaTeste) != null && !AssetDatabase.DeleteAsset(CenaTeste))
            throw new InvalidOperationException("Não foi possível substituir a cena técnica.");
        if (!AssetDatabase.CopyAsset(CenaClassic, CenaTeste))
            throw new InvalidOperationException("Não foi possível copiar a estrutura da cena Classic.");

        Scene cena = EditorSceneManager.OpenScene(CenaTeste, OpenSceneMode.Single);
        GameObject pai = GameObject.Find("Territorios");
        Transform modelo = pai != null ? pai.transform.Find("Arabia") : null;
        if (pai == null || modelo == null)
            throw new InvalidOperationException("Estrutura Territorios/Arabia ausente na cena base.");

        List<GameObject> novos = new List<GameObject>();
        for (int i = 0; i < Ids.Length; i++)
        {
            GameObject copia = UnityEngine.Object.Instantiate(modelo.gameObject, pai.transform, false);
            copia.name = "__MapaTeste12_" + i;
            novos.Add(copia);
        }

        for (int i = pai.transform.childCount - 1; i >= 0; i--)
        {
            Transform filho = pai.transform.GetChild(i);
            if (!filho.name.StartsWith("__MapaTeste12_", StringComparison.Ordinal))
                UnityEngine.Object.DestroyImmediate(filho.gameObject);
        }

        for (int i = 0; i < novos.Count; i++)
            ConfigurarTerritorio(novos[i], i);

        GameObject visual = GameObject.Find("classic_0");
        if (visual == null)
            throw new InvalidOperationException("Visual base classic_0 ausente.");
        visual.name = "MapaTeste12_Arte";
        visual.transform.position = Vector3.zero;
        visual.transform.localScale = Vector3.one;
        SpriteRenderer visualRenderer = visual.GetComponent<SpriteRenderer>();
        visualRenderer.sprite = definicao.ArteBase;
        visualRenderer.color = Color.white;

        Camera camera = Camera.main;
        if (camera != null)
        {
            camera.transform.position = new Vector3(0f, 0f, camera.transform.position.z);
            camera.orthographicSize = definicao.TamanhoOrtografico;
        }

        EditorSceneManager.MarkSceneDirty(cena);
        if (!EditorSceneManager.SaveScene(cena, CenaTeste))
            throw new InvalidOperationException("Falha ao salvar MapaTeste12.unity.");
    }

    private static void ConfigurarTerritorio(GameObject objeto, int indice)
    {
        string id = Ids[indice];
        objeto.name = id;
        objeto.transform.localPosition = new Vector3(Posicoes[indice].x, Posicoes[indice].y, 0f);
        objeto.transform.localRotation = Quaternion.identity;
        objeto.transform.localScale = Vector3.one;

        SpriteRenderer renderer = objeto.GetComponent<SpriteRenderer>();
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(PastaMascaras + "/" + id + ".png");
        if (renderer == null || sprite == null)
            throw new InvalidOperationException("Renderer/sprite ausente: " + id);
        renderer.sprite = sprite;
        renderer.color = Color.white;
        renderer.sortingOrder = 1;

        TerritorioClique territorio = objeto.GetComponent<TerritorioClique>();
        TerritorioTropas tropas = objeto.GetComponentInChildren<TerritorioTropas>(true);
        TerritorioFronteiras fronteiras = objeto.GetComponent<TerritorioFronteiras>();
        if (territorio == null || fronteiras == null)
            throw new InvalidOperationException("Componentes de gameplay ausentes: " + id);
        if (tropas == null)
            tropas = objeto.AddComponent<TerritorioTropas>();
        territorio.idTerritorio = id;
        territorio.chaveTraducao = "territory_" + id.ToLowerInvariant();
        territorio.continente = indice < 4
            ? TerritorioClique.Continente.AmericaDoNorte
            : indice < 8
                ? TerritorioClique.Continente.Africa
                : TerritorioClique.Continente.Asia;
        territorio.dono = TerritorioClique.Dono.Neutro;
        tropas.Quantidade = 1;

        SerializedObject fronteirasSerializadas = new SerializedObject(fronteiras);
        fronteirasSerializadas.FindProperty("vizinhos").ClearArray();
        fronteirasSerializadas.ApplyModifiedPropertiesWithoutUndo();

        PolygonCollider2D colliderAnterior = objeto.GetComponent<PolygonCollider2D>();
        if (colliderAnterior != null)
            UnityEngine.Object.DestroyImmediate(colliderAnterior);
        PolygonCollider2D collider = objeto.AddComponent<PolygonCollider2D>();
        AplicarPhysicsShape(sprite, collider);

        Transform contador = objeto.transform.Find("ContadorModerno");
        if (contador == null)
            throw new InvalidOperationException("ContadorModerno ausente: " + id);
        contador.localPosition = Vector3.zero;
    }

    private static void AplicarPhysicsShape(Sprite sprite, PolygonCollider2D collider)
    {
        int paths = sprite.GetPhysicsShapeCount();
        if (paths <= 0)
            throw new InvalidOperationException("Physics shape ausente: " + sprite.name);
        collider.pathCount = paths;
        List<Vector2> pontos = new List<Vector2>();
        for (int i = 0; i < paths; i++)
        {
            pontos.Clear();
            sprite.GetPhysicsShape(i, pontos);
            collider.SetPath(i, pontos);
        }
    }

    private static void ValidarMapa(string nome, DefinicaoMapa mapa, int territorios, int regioes, int conexoes)
    {
        if (mapa == null)
            throw new InvalidOperationException(nome + " não encontrado.");
        List<ProblemaValidacaoMapa> problemas = ValidadorDefinicaoMapa.Validar(mapa);
        if (mapa.Territorios.Count != territorios || mapa.Regioes.Count != regioes || mapa.Conexoes.Count != conexoes || problemas.Count != 0)
            throw new InvalidOperationException($"{nome} inválido: {mapa.Territorios.Count}/{mapa.Regioes.Count}/{mapa.Conexoes.Count}, problemas={problemas.Count}.");
        Debug.Log($"VALIDADOR MAPA | {nome} válido: {territorios} territórios, {regioes} regiões, {conexoes} conexões.");
    }

    private static string Absoluto(string relativo)
    {
        string raiz = Directory.GetParent(Application.dataPath).FullName;
        return Path.Combine(raiz, relativo.Replace('/', Path.DirectorySeparatorChar));
    }
}
#endif
