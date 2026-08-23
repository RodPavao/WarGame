using UnityEngine;

public class TerritorioClique : MonoBehaviour
{
    private SpriteRenderer sr;

    // =====================================================
    // IDENTIFICAÇÃO
    // =====================================================

    public string idTerritorio;

    public string chaveTraducao;

    public enum Continente
    {
        AmericaDoNorte,
        AmericaDoSul,
        Africa,
        Europa,
        Asia,
        Oceania
    }

    public Continente continente;

    // =====================================================
    // JOGADOR
    // =====================================================

    public enum Dono
    {
        Neutro,
        Jogador1,
        Jogador2,
        Jogador3,
        Jogador4
    }

    public Dono dono = Dono.Neutro;

    // =====================================================
    // EXÉRCITO
    // =====================================================

    public int tropas = 1;

    // =====================================================
    // FRONTEIRAS
    // =====================================================

    public TerritorioClique[] vizinhos;

    void Start()
{
    sr = GetComponent<SpriteRenderer>();

    AtualizarCor();

    CriarContadorModerno();
}

    void OnMouseDown()
    {
        GameManager.instance.ClicarTerritorio(this);
    }

private void CriarContadorModerno()
{
    Transform existente =
        transform.Find("ContadorModerno");

    if (existente != null)
        return;

    PolygonCollider2D collider =
        GetComponent<PolygonCollider2D>();

    if (collider == null)
        return;

    GameObject obj =
        new GameObject("ContadorModerno");

    obj.transform.SetParent(
        transform,
        false
    );

    // -------------------------------------------------
    // Encontra a maior área contínua do território.
    // Isso evita colocar o contador no mar em
    // arquipélagos como Japão e Indonésia.
    // -------------------------------------------------

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
                CalcularAreaPoligono(pontos)
            );

        if (area > maiorArea)
        {
            maiorArea = area;

            centroEscolhido =
                CalcularCentroPoligono(pontos);
        }
    }

    obj.transform.localPosition =
        new Vector3(
            centroEscolhido.x,
            centroEscolhido.y,
            0f
        );

    // -------------------------------------------------
    // Escala automática conforme a área do território.
    // -------------------------------------------------

    float escala =
        Mathf.Sqrt(
            maiorArea / 0.65f
        );

    escala =
        Mathf.Clamp(
            escala,
            0.68f,
            1.00f
        );

    obj.transform.localScale =
        new Vector3(
            escala,
            escala,
            1f
        );

    ContadorTropas contador =
        obj.AddComponent<ContadorTropas>();

    contador.Configurar(this);
}

private float CalcularAreaPoligono(
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

private Vector2 CalcularCentroPoligono(
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

public bool EhVizinho(TerritorioClique outro)
{
    if (outro == null || vizinhos == null)
        return false;

    foreach (TerritorioClique vizinho in vizinhos)
    {
        if (vizinho == outro)
            return true;
    }

    return false;
}

    public void AtualizarCor()
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        switch (dono)
        {
            case Dono.Jogador1:
                sr.color = Color.red;
                break;

            case Dono.Jogador2:
                sr.color = Color.blue;
                break;

            case Dono.Jogador3:
                sr.color = Color.green;
                break;

            case Dono.Jogador4:
                sr.color = Color.magenta;
                break;

            default:
                sr.color = Color.white;
                break;
        }
    }

public void DestacarContinente(Color cor)
{
    if (sr == null)
        sr = GetComponent<SpriteRenderer>();

    sr.color = cor;
}

public void RestaurarCor()
{
    AtualizarCor();
}
    public void DestacarSelecao()
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        sr.color = Color.yellow;
    }
}