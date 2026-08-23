using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class GeradorTerritorios : EditorWindow
{
    private Texture2D fonte;
    private Texture2D texturaLeitura;
    private Texture2D overlay;

    private bool[] selecionado;
    private Color32[] pixels;

    private string nomeTerritorio = "NovoTerritorio";
private int tolerancia = 32;
private int tamanhoBorracha = 10;

private float zoom = 1f;
private Vector2 scrollPos = Vector2.zero;

// Lista de territórios já gerados
private List<string> nomesTerritorios = new List<string>();
private int indiceTerritorio = 0;

// Histórico para DESFAZER
private Stack<bool[]> historico = new Stack<bool[]>();

    private enum Ferramenta
    {
        Selecionar,
        Adicionar,
        Borracha
    }

    private Ferramenta ferramentaAtual = Ferramenta.Selecionar;

    private const string PastaSaida =
        "Assets/TerritoriosDefinitivos";

    // =========================================================
    // ABRIR EXTRATOR
    // =========================================================

    [MenuItem("WarGame/Extrator Visual de Territorios")]
    public static void Abrir()
    {
        GetWindow<GeradorTerritorios>(
            "Extrator de Territorios"
        );
    }

    // =========================================================
    // INTERFACE
    // =========================================================

    private void OnGUI()
    {
        GUILayout.Label(
            "WarGame - Extrator de Territorios",
            EditorStyles.boldLabel
        );

        fonte = (Texture2D)EditorGUILayout.ObjectField(
            "Mapa fonte",
            fonte,
            typeof(Texture2D),
            false
        );

        if (nomesTerritorios.Count == 0)
{
    AtualizarListaTerritorios();
}

if (nomesTerritorios.Count > 0)
{
    int novoIndice =
        EditorGUILayout.Popup(
            "Territorio",
            indiceTerritorio,
            nomesTerritorios.ToArray()
        );

    if (novoIndice != indiceTerritorio)
    {
        indiceTerritorio = novoIndice;
        nomeTerritorio = nomesTerritorios[indiceTerritorio];

        // Se o mapa já estiver carregado,
        // carrega o território automaticamente.
        if (texturaLeitura != null)
        {
            CarregarTerritorioExistente();
        }
    }
}
else
{
    nomeTerritorio =
        EditorGUILayout.TextField(
            "Nome tecnico",
            nomeTerritorio
        );
}

        tolerancia =
            EditorGUILayout.IntSlider(
                "Tolerancia",
                tolerancia,
                0,
                100
            );

        GUILayout.Space(5);

        if (GUILayout.Button(
            "Carregar / Recarregar mapa"))
        {
            CarregarFonte();
        }

        if (texturaLeitura == null)
            return;

        GUILayout.Space(8);

        GUILayout.Label(
            "Ferramenta",
            EditorStyles.boldLabel
        );

        ferramentaAtual =
            (Ferramenta)GUILayout.Toolbar(
                (int)ferramentaAtual,
                new string[]
                {
                    "Selecionar",
                    "Adicionar",
                    "BORRACHA"
                }
            );

        if (ferramentaAtual ==
            Ferramenta.Borracha)
        {
            tamanhoBorracha =
    EditorGUILayout.IntSlider(
        "Tamanho da borracha",
        tamanhoBorracha,
        1,
        30
    );

zoom =
    EditorGUILayout.Slider(
        "Zoom",
        zoom,
        1f,
        10f
    );
        }

        GUILayout.Space(5);

        EditorGUILayout.HelpBox(
            "SELECIONAR: clique em uma região.\n" +
            "ADICIONAR: soma outra região/ilha.\n" +
            "BORRACHA: clique ou arraste para apagar sujeiras.\n" +
            "SHIFT + clique também adiciona regiões.",
            MessageType.Info
        );

        GUILayout.BeginHorizontal();

if (GUILayout.Button(
    "Carregar territorio existente"))
{
    CarregarTerritorioExistente();
}

GUI.enabled = historico.Count > 0;

if (GUILayout.Button("DESFAZER"))
{
    Desfazer();
}

GUI.enabled = true;

if (GUILayout.Button(
    "Limpar selecao"))
{
    LimparSelecao();
}

GUILayout.EndHorizontal();

        GUILayout.Space(5);

        float larguraDisponivel =
    position.width - 20f;

float alturaDisponivel =
    Mathf.Max(
        250f,
        position.height - 380f
    );

Rect areaVisivel =
    GUILayoutUtility.GetRect(
        larguraDisponivel,
        alturaDisponivel
    );

float larguraImagem =
    larguraDisponivel * zoom;

float alturaImagem =
    larguraImagem *
    texturaLeitura.height /
    texturaLeitura.width;

Rect areaConteudo =
    new Rect(
        0,
        0,
        larguraImagem,
        alturaImagem
    );

scrollPos =
    GUI.BeginScrollView(
        areaVisivel,
        scrollPos,
        areaConteudo
    );

Rect imagemRect =
    new Rect(
        0,
        0,
        larguraImagem,
        alturaImagem
    );

GUI.DrawTexture(
    imagemRect,
    texturaLeitura,
    ScaleMode.StretchToFill,
    false
);

if (overlay != null)
{
    GUI.DrawTexture(
        imagemRect,
        overlay,
        ScaleMode.StretchToFill,
        true
    );
}

ProcessarMouse(imagemRect);

GUI.EndScrollView();

        GUILayout.Space(8);

        GUI.backgroundColor =
            new Color(0.45f, 1f, 0.45f);

        if (GUILayout.Button(
            "GERAR / SOBRESCREVER TERRITORIO",
            GUILayout.Height(35)))
        {
            GerarTerritorio();
        }

        GUI.backgroundColor = Color.white;
    }

    // =========================================================
    // CARREGAR MAPA
    // =========================================================

    private void CarregarFonte()
    {
        if (fonte == null)
        {
            EditorUtility.DisplayDialog(
                "WarGame",
                "Selecione o classic3 no campo Mapa fonte.",
                "OK"
            );

            return;
        }

        string caminhoAsset =
            AssetDatabase.GetAssetPath(fonte);

        byte[] bytes =
            File.ReadAllBytes(caminhoAsset);

        if (texturaLeitura != null)
            DestroyImmediate(texturaLeitura);

        texturaLeitura =
            new Texture2D(
                2,
                2,
                TextureFormat.RGBA32,
                false
            );

        texturaLeitura.LoadImage(bytes);

        pixels =
            texturaLeitura.GetPixels32();

        selecionado =
            new bool[
                texturaLeitura.width *
                texturaLeitura.height
            ];

        CriarOverlay();

        Repaint();
    }

    // =========================================================
    // CARREGAR TERRITÓRIO JÁ GERADO
    // =========================================================

    private void CarregarTerritorioExistente()
    {
        if (texturaLeitura == null)
            return;

            historico.Clear();

        string nomeSeguro =
            nomeTerritorio.Replace(" ", "");

        string caminho =
            PastaSaida +
            "/" +
            nomeSeguro +
            ".png";

        if (!File.Exists(caminho))
        {
            EditorUtility.DisplayDialog(
                "WarGame",
                "Não encontrei:\n" + caminho,
                "OK"
            );

            return;
        }

        byte[] bytes =
            File.ReadAllBytes(caminho);

        Texture2D mascara =
            new Texture2D(
                2,
                2,
                TextureFormat.RGBA32,
                false
            );

        mascara.LoadImage(bytes);

        if (mascara.width !=
                texturaLeitura.width ||
            mascara.height !=
                texturaLeitura.height)
        {
            DestroyImmediate(mascara);

            EditorUtility.DisplayDialog(
                "WarGame",
                "O território não tem o mesmo tamanho do mapa.",
                "OK"
            );

            return;
        }

        Color32[] mascaraPixels =
            mascara.GetPixels32();

        selecionado =
            new bool[mascaraPixels.Length];

        for (int i = 0;
             i < mascaraPixels.Length;
             i++)
        {
            selecionado[i] =
                mascaraPixels[i].a > 10;
        }

        DestroyImmediate(mascara);

        AtualizarOverlay();
        Repaint();

        Debug.Log(
            "Território carregado para edição: " +
            nomeSeguro
        );
    }

    // =========================================================
    // DIMENSÃO DA IMAGEM
    // =========================================================

    private Rect CalcularRectImagem(Rect area)
    {
        float proporcaoImagem =
            (float)texturaLeitura.width /
            texturaLeitura.height;

        float proporcaoArea =
            area.width /
            area.height;

        if (proporcaoImagem >
            proporcaoArea)
        {
            float altura =
                area.width /
                proporcaoImagem;

            return new Rect(
                area.x,
                area.y +
                (area.height - altura) / 2f,
                area.width,
                altura
            );
        }

        float largura =
            area.height *
            proporcaoImagem;

        return new Rect(
            area.x +
            (area.width - largura) / 2f,
            area.y,
            largura,
            area.height
        );
    }

    // =========================================================
    // MOUSE
    // =========================================================

    private void ProcessarMouse(Rect imagemRect)
    {
        Event e = Event.current;

        bool clique =
            e.type == EventType.MouseDown &&
            e.button == 0;

        bool arraste =
            e.type == EventType.MouseDrag &&
            e.button == 0;

        if (!clique && !arraste)
            return;

        if (!imagemRect.Contains(
            e.mousePosition))
            return;

        Vector2Int pixel =
            ConverterMouseParaPixel(
                e.mousePosition,
                imagemRect
            );

        // BORRACHA
        if (ferramentaAtual ==
    Ferramenta.Borracha)
{
    // Registra apenas no início do traço.
    if (clique)
    {
        RegistrarEstadoParaDesfazer();
    }

    AplicarBorracha(
        pixel.x,
        pixel.y
    );

            AtualizarOverlay();

            e.Use();
            Repaint();

            return;
        }

        // Seleção só acontece no clique,
        // não no arraste.
        if (!clique)
            return;

        RegistrarEstadoParaDesfazer();    

        bool adicionar =
            ferramentaAtual ==
            Ferramenta.Adicionar ||
            e.shift;

        if (!adicionar)
        {
            selecionado =
                new bool[
                    texturaLeitura.width *
                    texturaLeitura.height
                ];
        }

        FloodFill(
            pixel.x,
            pixel.y
        );

        AtualizarOverlay();

        e.Use();
        Repaint();
    }

    private Vector2Int ConverterMouseParaPixel(
        Vector2 mouse,
        Rect imagemRect)
    {
        float normalX =
            (mouse.x - imagemRect.x) /
            imagemRect.width;

        float normalY =
            1f -
            (
                (mouse.y - imagemRect.y) /
                imagemRect.height
            );

        int x =
            Mathf.Clamp(
                Mathf.FloorToInt(
                    normalX *
                    texturaLeitura.width
                ),
                0,
                texturaLeitura.width - 1
            );

        int y =
            Mathf.Clamp(
                Mathf.FloorToInt(
                    normalY *
                    texturaLeitura.height
                ),
                0,
                texturaLeitura.height - 1
            );

        return new Vector2Int(x, y);
    }

    // =========================================================
    // BORRACHA
    // =========================================================

    private void AplicarBorracha(
    int centroX,
    int centroY)
{
    int largura =
        texturaLeitura.width;

    int altura =
        texturaLeitura.height;

    int raio =
        tamanhoBorracha;

    int raioQuadrado =
        raio * raio;

    for (int y =
            centroY - raio;
         y <= centroY + raio;
         y++)
    {
        if (y < 0 || y >= altura)
            continue;

        for (int x =
                centroX - raio;
             x <= centroX + raio;
             x++)
        {
            if (x < 0 ||
                x >= largura)
                continue;

            int dx =
                x - centroX;

            int dy =
                y - centroY;

            if (
                dx * dx +
                dy * dy >
                raioQuadrado
            )
                continue;

            int indice =
                y * largura + x;

            // Em vez de apagar a máscara,
            // ADICIONA o pixel ao território.
            selecionado[indice] = true;
        }
    }
}

    // =========================================================
    // FLOOD FILL
    // =========================================================

    private void FloodFill(
        int inicioX,
        int inicioY)
    {
        int largura =
            texturaLeitura.width;

        int altura =
            texturaLeitura.height;

        int inicio =
            inicioY *
            largura +
            inicioX;

        Color32 corInicial =
            pixels[inicio];

        bool[] visitado =
            new bool[pixels.Length];

        Queue<int> fila =
            new Queue<int>();

        fila.Enqueue(inicio);

        visitado[inicio] = true;

        while (fila.Count > 0)
        {
            int indice =
                fila.Dequeue();

            if (!CorParecida(
                pixels[indice],
                corInicial))
                continue;

            selecionado[indice] =
                true;

            int x =
                indice % largura;

            int y =
                indice / largura;

            AdicionarFila(
                x + 1, y,
                largura, altura,
                visitado, fila
            );

            AdicionarFila(
                x - 1, y,
                largura, altura,
                visitado, fila
            );

            AdicionarFila(
                x, y + 1,
                largura, altura,
                visitado, fila
            );

            AdicionarFila(
                x, y - 1,
                largura, altura,
                visitado, fila
            );
        }
    }

    private void AdicionarFila(
        int x,
        int y,
        int largura,
        int altura,
        bool[] visitado,
        Queue<int> fila)
    {
        if (x < 0 ||
            x >= largura ||
            y < 0 ||
            y >= altura)
            return;

        int indice =
            y * largura + x;

        if (visitado[indice])
            return;

        visitado[indice] =
            true;

        fila.Enqueue(indice);
    }

    private bool CorParecida(
        Color32 a,
        Color32 b)
    {
        return
            Mathf.Abs(a.r - b.r)
                <= tolerancia &&
            Mathf.Abs(a.g - b.g)
                <= tolerancia &&
            Mathf.Abs(a.b - b.b)
                <= tolerancia &&
            Mathf.Abs(a.a - b.a)
                <= tolerancia;
    }

    // =========================================================
    // OVERLAY VERDE
    // =========================================================

    private void CriarOverlay()
    {
        if (overlay != null)
            DestroyImmediate(overlay);

        overlay =
            new Texture2D(
                texturaLeitura.width,
                texturaLeitura.height,
                TextureFormat.RGBA32,
                false
            );

        overlay.filterMode =
            FilterMode.Point;

        AtualizarOverlay();
    }

    private void AtualizarOverlay()
    {
        if (overlay == null ||
            selecionado == null)
            return;

        Color32[] cores =
            new Color32[
                selecionado.Length
            ];

        Color32 destaque =
            new Color32(
                0,
                255,
                80,
                120
            );

        Color32 transparente =
            new Color32(
                0,
                0,
                0,
                0
            );

        for (int i = 0;
             i < selecionado.Length;
             i++)
        {
            cores[i] =
                selecionado[i]
                ? destaque
                : transparente;
        }

        overlay.SetPixels32(cores);
        overlay.Apply();
    }

    private void LimparSelecao()
    {

        RegistrarEstadoParaDesfazer();

        if (texturaLeitura == null)
            return;

        selecionado =
            new bool[
                texturaLeitura.width *
                texturaLeitura.height
            ];

        AtualizarOverlay();
        Repaint();
    }

    // =========================================================
    // GERAR PNG
    // =========================================================

    private void GerarTerritorio()
    {
        if (texturaLeitura == null ||
            selecionado == null)
            return;

        if (string.IsNullOrWhiteSpace(
            nomeTerritorio))
        {
            EditorUtility.DisplayDialog(
                "WarGame",
                "Digite o nome técnico do território.",
                "OK"
            );

            return;
        }

        bool existeSelecao =
            false;

        Color32[] resultado =
            new Color32[
                selecionado.Length
            ];

        for (int i = 0;
             i < selecionado.Length;
             i++)
        {
            if (selecionado[i])
            {
                resultado[i] =
                    new Color32(
                        255,
                        255,
                        255,
                        255
                    );

                existeSelecao = true;
            }
            else
            {
                resultado[i] =
                    new Color32(
                        0,
                        0,
                        0,
                        0
                    );
            }
        }

        if (!existeSelecao)
        {
            EditorUtility.DisplayDialog(
                "WarGame",
                "Nenhuma região selecionada.",
                "OK"
            );

            return;
        }

        if (!Directory.Exists(
            PastaSaida))
        {
            Directory.CreateDirectory(
                PastaSaida
            );
        }

        Texture2D saida =
            new Texture2D(
                texturaLeitura.width,
                texturaLeitura.height,
                TextureFormat.RGBA32,
                false
            );

        saida.SetPixels32(resultado);
        saida.Apply();

        string nomeSeguro =
            nomeTerritorio.Replace(
                " ",
                ""
            );

        string caminho =
            PastaSaida +
            "/" +
            nomeSeguro +
            ".png";

        File.WriteAllBytes(
            caminho,
            saida.EncodeToPNG()
        );

        DestroyImmediate(saida);

        AssetDatabase.Refresh();

        TextureImporter importer =
            AssetImporter.GetAtPath(
                caminho
            ) as TextureImporter;

        if (importer != null)
        {
            importer.textureType =
                TextureImporterType.Sprite;

            importer.spriteImportMode =
                SpriteImportMode.Single;

            importer.spritePixelsPerUnit =
                100;

            importer.alphaIsTransparency =
                true;

            importer.mipmapEnabled =
                false;

            importer.filterMode =
                FilterMode.Bilinear;

            importer.textureCompression =
                TextureImporterCompression
                    .Uncompressed;

            importer.SaveAndReimport();
        }

        Debug.Log(
            "Território salvo/atualizado: " +
            nomeSeguro
        );

        AtualizarListaTerritorios();

    }

    // =========================================================
    // MONTAGEM AUTOMÁTICA
    // =========================================================

    [MenuItem(
        "WarGame/Montar Territorios Definitivos"
    )]
    public static void MontarTerritoriosDefinitivos()
    {
        const string pastaSprites =
            "Assets/TerritoriosDefinitivos";

        GameObject pai =
            GameObject.Find(
                "Territorios"
            );

        if (pai == null)
        {
            Debug.LogError(
                "Objeto Territorios não encontrado."
            );

            return;
        }

        Transform modelo =
            pai.transform.Find(
                "Arabia"
            );

        if (modelo == null)
        {
            Debug.LogError(
                "Arabia não encontrada para servir de referência."
            );

            return;
        }

        Vector3 posicaoPadrao =
            modelo.localPosition;

        Vector3 escalaPadrao =
            modelo.localScale;

        Quaternion rotacaoPadrao =
            modelo.localRotation;

        string[] guids =
            AssetDatabase.FindAssets(
                "t:Sprite",
                new[]
                {
                    pastaSprites
                }
            );

        int criados = 0;
        int atualizados = 0;

        foreach (string guid in guids)
        {
            string caminho =
                AssetDatabase
                    .GUIDToAssetPath(
                        guid
                    );

            Sprite sprite =
                AssetDatabase
                    .LoadAssetAtPath<Sprite>(
                        caminho
                    );

            if (sprite == null)
                continue;

            string nome =
                sprite.name;

            Transform existente =
                pai.transform.Find(
                    nome
                );

            GameObject objeto;

            if (existente != null)
            {
                objeto =
                    existente.gameObject;

                atualizados++;
            }
            else
            {
                objeto =
                    new GameObject(nome);

                objeto.transform.SetParent(
                    pai.transform,
                    false
                );

                criados++;
            }

            objeto.transform.localPosition =
                posicaoPadrao;

            objeto.transform.localRotation =
                rotacaoPadrao;

            objeto.transform.localScale =
                escalaPadrao;

            SpriteRenderer sr =
                objeto.GetComponent<
                    SpriteRenderer>();

            if (sr == null)
            {
                sr =
                    objeto.AddComponent<
                        SpriteRenderer>();
            }

            sr.sprite = sprite;
            sr.sortingLayerName =
                "Default";
            sr.sortingOrder = 1;
            sr.color = Color.white;

            TerritorioClique territorio =
                objeto.GetComponent<
                    TerritorioClique>();

            if (territorio == null)
            {
                territorio =
                    objeto.AddComponent<
                        TerritorioClique>();
            }

            territorio.dono =
                TerritorioClique
                    .Dono
                    .Neutro;

            territorio.tropas = 1;

            territorio.idTerritorio = nome;
            territorio.chaveTraducao = "territory_" + ConverterParaChave(nome);
            territorio.continente = ObterContinente(nome);

            if (territorio.vizinhos == null)
            {
                territorio.vizinhos =
                    new TerritorioClique[0];
            }

            // Recria o collider para refletir
            // o PNG atualizado.
            PolygonCollider2D antigo =
                objeto.GetComponent<
                    PolygonCollider2D>();

            if (antigo != null)
            {
                DestroyImmediate(antigo);
            }

            PolygonCollider2D collider =
                objeto.AddComponent<
                    PolygonCollider2D>();

            collider.isTrigger = false;

            EditorUtility.SetDirty(objeto);
        }

        UnityEditor.SceneManagement
            .EditorSceneManager
            .MarkSceneDirty(
                UnityEngine.SceneManagement
                    .SceneManager
                    .GetActiveScene()
            );

        Debug.Log(
            "MONTAGEM CONCLUÍDA | Criados: " +
            criados +
            " | Atualizados: " +
            atualizados
        );
    }

// =========================================================
// GERAR CONTADORES PERMANENTES
// =========================================================

[MenuItem("WarGame/Gerar Contadores Permanentes")]
public static void GerarContadoresPermanentes()
{
    GameObject pai =
        GameObject.Find("Territorios");

    if (pai == null)
    {
        Debug.LogError(
            "Objeto Territorios não encontrado."
        );

        return;
    }

    int criados = 0;
    int preservados = 0;
    int erros = 0;

    foreach (Transform filho in pai.transform)
    {
        TerritorioClique territorio =
            filho.GetComponent<TerritorioClique>();

        if (territorio == null)
            continue;

        // Se já existir, NÃO altera posição nem escala.
        // Assim seus ajustes manuais ficam preservados.
        Transform existente =
            filho.Find("ContadorModerno");

        if (existente != null)
        {
            preservados++;
            continue;
        }

        PolygonCollider2D collider =
            filho.GetComponent<PolygonCollider2D>();

        if (collider == null)
        {
            Debug.LogWarning(
                "Sem PolygonCollider2D: " +
                filho.name
            );

            erros++;
            continue;
        }

        Vector2 centroEscolhido =
            Vector2.zero;

        float maiorArea =
            0f;

        for (int i = 0;
             i < collider.pathCount;
             i++)
        {
            Vector2[] pontos =
                collider.GetPath(i);

            float area =
                Mathf.Abs(
                    CalcularAreaContador(pontos)
                );

            if (area > maiorArea)
            {
                maiorArea = area;

                centroEscolhido =
                    CalcularCentroContador(
                        pontos
                    );
            }
        }

        GameObject contadorObj =
            new GameObject(
                "ContadorModerno"
            );

        Undo.RegisterCreatedObjectUndo(
            contadorObj,
            "Criar Contador Moderno"
        );

        contadorObj.transform.SetParent(
            filho,
            false
        );

        contadorObj.transform.localPosition =
            new Vector3(
                centroEscolhido.x,
                centroEscolhido.y,
                0f
            );

        float escala =
            Mathf.Sqrt(
                maiorArea / 0.65f
            );

        escala =
            Mathf.Clamp(
                escala,
                0.68f,
                1f
            );

        contadorObj.transform.localScale =
            new Vector3(
                escala,
                escala,
                1f
            );

        ContadorTropas contador =
            contadorObj.AddComponent<
                ContadorTropas>();

        contador.Configurar(
            territorio
        );

        EditorUtility.SetDirty(
            contadorObj
        );

        criados++;
    }

    UnityEditor.SceneManagement
        .EditorSceneManager
        .MarkSceneDirty(
            UnityEngine.SceneManagement
                .SceneManager
                .GetActiveScene()
        );

    Debug.Log(
        "CONTADORES PERMANENTES | Criados: " +
        criados +
        " | Preservados: " +
        preservados +
        " | Erros: " +
        erros
    );
}


// =========================================================
// CÁLCULO DOS CONTADORES
// =========================================================

private static float CalcularAreaContador(
    Vector2[] pontos)
{
    if (pontos == null ||
        pontos.Length < 3)
        return 0f;

    float area = 0f;

    for (int i = 0;
         i < pontos.Length;
         i++)
    {
        Vector2 atual =
            pontos[i];

        Vector2 proximo =
            pontos[
                (i + 1) %
                pontos.Length
            ];

        area +=
            atual.x * proximo.y -
            proximo.x * atual.y;
    }

    return area * 0.5f;
}


private static Vector2 CalcularCentroContador(
    Vector2[] pontos)
{
    if (pontos == null ||
        pontos.Length == 0)
        return Vector2.zero;

    Vector2 soma =
        Vector2.zero;

    foreach (Vector2 ponto in pontos)
    {
        soma += ponto;
    }

    return soma /
        pontos.Length;
}

    private static string ConverterParaChave(string nome)
{
    System.Text.StringBuilder resultado =
        new System.Text.StringBuilder();

    for (int i = 0; i < nome.Length; i++)
    {
        char atual = nome[i];

        if (i > 0 &&
            char.IsUpper(atual) &&
            char.IsLower(nome[i - 1]))
        {
            resultado.Append("_");
        }

        resultado.Append(
            char.ToLowerInvariant(atual)
        );
    }

    return resultado.ToString();
}

private static TerritorioClique.Continente ObterContinente(string nome)
{
    switch (nome)
    {
        // NORTH AMERICA
        case "Alaska":
        case "Yukon":
        case "Alberta":
        case "Ontario":
        case "Quebec":
        case "Greenland":
        case "WestUSA":
        case "EastUSA":
        case "Mexico":
            return TerritorioClique.Continente.AmericaDoNorte;

        // SOUTH AMERICA
        case "Venezuela":
        case "Peru":
        case "Brazil":
        case "Argentina":
            return TerritorioClique.Continente.AmericaDoSul;

        // AFRICA
        case "Algeria":
        case "Egypt":
        case "Ethiopia":
        case "Congo":
        case "SouthAfrica":
        case "Madagascar":
            return TerritorioClique.Continente.Africa;

        // EUROPE
        case "Iceland":
        case "Scandinavia":
        case "UnitedKingdom":
        case "Germany":
        case "Spain":
        case "Italy":
        case "Russia":
            return TerritorioClique.Continente.Europa;

        // ASIA
        case "Ural":
        case "Siberia":
        case "Yakutsk":
        case "Kamchatka":
        case "Irkutsk":
        case "Mongolia":
        case "Japan":
        case "Afghanistan":
        case "China":
        case "Arabia":
        case "India":
        case "Indochina":
            return TerritorioClique.Continente.Asia;

        // OCEANIA
        case "Indonesia":
        case "NewGuinea":
        case "WestAustralia":
        case "EastAustralia":
            return TerritorioClique.Continente.Oceania;

        default:
            Debug.LogWarning(
                "Continente não definido para: " + nome
            );

            return TerritorioClique.Continente.AmericaDoNorte;
    }
}

// =========================================================
// CONFIGURAR VIZINHOS
// =========================================================

[MenuItem("WarGame/Configurar Vizinhos")]
public static void ConfigurarVizinhos()
{
    GameObject pai = GameObject.Find("Territorios");

    if (pai == null)
    {
        Debug.LogError("Objeto Territorios não encontrado.");
        return;
    }

    Dictionary<string, string[]> tabela =
        new Dictionary<string, string[]>
    {
        // AMÉRICA DO NORTE
        { "Alaska", new[] { "Yukon", "Alberta", "Kamchatka" } },
        { "Yukon", new[] { "Alaska", "Alberta", "Ontario", "Greenland" } },
        { "Alberta", new[] { "Alaska", "Yukon", "Ontario", "WestUSA" } },
        { "Ontario", new[] { "Yukon", "Alberta", "Greenland", "Quebec", "WestUSA", "EastUSA" } },
        { "Quebec", new[] { "Ontario", "Greenland", "EastUSA" } },
        { "Greenland", new[] { "Yukon", "Ontario", "Quebec", "Iceland" } },
        { "WestUSA", new[] { "Alberta", "Ontario", "EastUSA", "Mexico" } },
        { "EastUSA", new[] { "Ontario", "Quebec", "WestUSA", "Mexico" } },
        { "Mexico", new[] { "WestUSA", "EastUSA", "Venezuela" } },

        // AMÉRICA DO SUL
        { "Venezuela", new[] { "Mexico", "Brazil", "Peru" } },
        { "Brazil", new[] { "Venezuela", "Peru", "Argentina", "Algeria" } },
        { "Peru", new[] { "Venezuela", "Brazil", "Argentina" } },
        { "Argentina", new[] { "Brazil", "Peru" } },

        // EUROPA
        { "Iceland", new[] { "Greenland", "UnitedKingdom", "Scandinavia" } },
        { "UnitedKingdom", new[] { "Iceland", "Scandinavia", "Germany", "Spain" } },
        { "Scandinavia", new[] { "Iceland", "UnitedKingdom", "Germany", "Russia" } },
        { "Germany", new[] { "UnitedKingdom", "Scandinavia", "Russia", "Italy", "Spain" } },
        { "Spain", new[] { "UnitedKingdom", "Germany", "Italy", "Algeria" } },
        { "Italy", new[] { "Spain", "Germany", "Russia", "Arabia", "Egypt", "Algeria" } },
        { "Russia", new[] { "Scandinavia", "Germany", "Italy", "Ural", "Afghanistan", "Arabia" } },

        // ÁFRICA
        { "Algeria", new[] { "Brazil", "Spain", "Italy", "Egypt", "Ethiopia", "Congo" } },
        { "Egypt", new[] { "Algeria", "Italy", "Arabia", "Ethiopia" } },
        { "Ethiopia", new[] { "Egypt", "Algeria", "Congo", "SouthAfrica", "Madagascar", "Arabia" } },
        { "Congo", new[] { "Algeria", "Ethiopia", "SouthAfrica" } },
        { "SouthAfrica", new[] { "Congo", "Ethiopia", "Madagascar" } },
        { "Madagascar", new[] { "Ethiopia", "SouthAfrica" } },

        // ÁSIA
        { "Ural", new[] { "Russia", "Siberia", "China", "Afghanistan" } },
        { "Siberia", new[] { "Ural", "Yakutsk", "Irkutsk", "Mongolia", "China" } },
        { "Yakutsk", new[] { "Siberia", "Irkutsk", "Kamchatka" } },
        { "Kamchatka", new[] { "Yakutsk", "Irkutsk", "Mongolia", "Japan", "Alaska" } },
        { "Irkutsk", new[] { "Siberia", "Yakutsk", "Kamchatka", "Mongolia" } },
        { "Mongolia", new[] { "Siberia", "Irkutsk", "Kamchatka", "Japan", "China" } },
        { "Japan", new[] { "Kamchatka", "Mongolia" } },
        { "Afghanistan", new[] { "Russia", "Ural", "China", "India", "Arabia" } },
        { "China", new[] { "Ural", "Siberia", "Mongolia", "Indochina", "India", "Afghanistan" } },
        { "Arabia", new[] { "Italy", "Russia", "Afghanistan", "India", "Ethiopia", "Egypt" } },
        { "India", new[] { "Arabia", "Afghanistan", "China", "Indochina" } },
        { "Indochina", new[] { "India", "China", "Indonesia" } },

        // OCEANIA
        { "Indonesia", new[] { "Indochina", "NewGuinea", "WestAustralia" } },
        { "NewGuinea", new[] { "Indonesia", "WestAustralia", "EastAustralia" } },
        { "WestAustralia", new[] { "Indonesia", "NewGuinea", "EastAustralia" } },
        { "EastAustralia", new[] { "NewGuinea", "WestAustralia" } }
    };

    int configurados = 0;
    int erros = 0;

    foreach (var item in tabela)
    {
        Transform origem = pai.transform.Find(item.Key);

        if (origem == null)
        {
            Debug.LogError("Território não encontrado: " + item.Key);
            erros++;
            continue;
        }

        TerritorioClique territorio =
            origem.GetComponent<TerritorioClique>();

        List<TerritorioClique> lista =
            new List<TerritorioClique>();

        foreach (string nomeVizinho in item.Value)
        {
            Transform destino =
                pai.transform.Find(nomeVizinho);

            if (destino == null)
            {
                Debug.LogError(
                    item.Key +
                    " -> vizinho não encontrado: " +
                    nomeVizinho
                );

                erros++;
                continue;
            }

            TerritorioClique vizinho =
                destino.GetComponent<TerritorioClique>();

            if (vizinho != null)
                lista.Add(vizinho);
        }

        territorio.vizinhos = lista.ToArray();

        EditorUtility.SetDirty(territorio);

        configurados++;
    }

    UnityEditor.SceneManagement.EditorSceneManager
        .MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager
                .GetActiveScene()
        );

    Debug.Log(
        "VIZINHOS CONFIGURADOS | Territorios: " +
        configurados +
        " | Erros: " +
        erros
    );
}

// =========================================================
// HISTÓRICO / DESFAZER
// =========================================================

private void RegistrarEstadoParaDesfazer()
{
    if (selecionado == null)
        return;

    bool[] copia =
        new bool[selecionado.Length];

    System.Array.Copy(
        selecionado,
        copia,
        selecionado.Length
    );

    historico.Push(copia);

    // Evita histórico infinito
    while (historico.Count > 50)
    {
        Stack<bool[]> temporario =
            new Stack<bool[]>(historico);

        temporario.Pop();

        historico =
            new Stack<bool[]>(temporario);
    }
}

private void Desfazer()
{
    if (historico.Count == 0)
        return;

    selecionado =
        historico.Pop();

    AtualizarOverlay();

    Repaint();

    Debug.Log("Última alteração desfeita.");
}


// =========================================================
// LISTA AUTOMÁTICA DOS TERRITÓRIOS
// =========================================================

private void AtualizarListaTerritorios()
{
    string nomeAtual = nomeTerritorio;

    nomesTerritorios.Clear();

    string[] guids =
        AssetDatabase.FindAssets(
            "t:Sprite",
            new[]
            {
                PastaSaida
            }
        );

    foreach (string guid in guids)
    {
        string caminho =
            AssetDatabase.GUIDToAssetPath(guid);

        string nome =
            Path.GetFileNameWithoutExtension(
                caminho
            );

        if (!nomesTerritorios.Contains(nome))
        {
            nomesTerritorios.Add(nome);
        }
    }

    nomesTerritorios.Sort();

    if (nomesTerritorios.Count == 0)
        return;

    int encontrado =
        nomesTerritorios.IndexOf(nomeAtual);

    indiceTerritorio =
        encontrado >= 0
        ? encontrado
        : 0;

    nomeTerritorio =
        nomesTerritorios[indiceTerritorio];
}
    private void OnDestroy()
    {
        if (texturaLeitura != null)
            DestroyImmediate(
                texturaLeitura
            );

        if (overlay != null)
            DestroyImmediate(
                overlay
            );
    }
}