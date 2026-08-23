using UnityEngine;

public class TerritorioContador : MonoBehaviour
{
    private ContadorTropas contadorTropas;
    private TerritorioClique territorio;

    private void Awake()
    {
        territorio =
            GetComponent<TerritorioClique>();
    }

    public void Inicializar()
    {
        if (territorio == null)
            territorio =
                GetComponent<TerritorioClique>();

        Transform existente =
            transform.Find("ContadorModerno");

        if (existente != null)
        {
            contadorTropas =
                existente.GetComponent<ContadorTropas>();

            if (contadorTropas != null)
                contadorTropas.Configurar(territorio);

            return;
        }

        CriarContador();
    }

    private void CriarContador()
    {
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

        const float escalaPadraoContador =
            0.9004f;

        obj.transform.localScale =
            new Vector3(
                escalaPadraoContador,
                escalaPadraoContador,
                1f
            );

        contadorTropas =
            obj.AddComponent<ContadorTropas>();

        contadorTropas.Configurar(
            territorio
        );
    }

    public void Atualizar()
    {
        if (contadorTropas != null)
            contadorTropas.Atualizar();
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
}