using UnityEngine;
using UnityEngine.InputSystem;

public class InputPartida : MonoBehaviour
{
    private Camera cameraPrincipal;

    // Contador que está sendo pressionado neste momento.
    private ContadorTropas contadorPressionado;

    private void Awake()
    {
        EncontrarCamera();
    }

    private void Update()
    {
        if (Pointer.current == null)
            return;

        if (cameraPrincipal == null)
        {
            EncontrarCamera();

            if (cameraPrincipal == null)
                return;
        }

        // =================================================
        // SOLTOU O CONTADOR
        // =================================================

        if (contadorPressionado != null &&
            Pointer.current.press.wasReleasedThisFrame)
        {
            contadorPressionado
                .FinalizarPressaoInput();

            contadorPressionado = null;

            return;
        }

        // =================================================
        // NOVO CLIQUE / TOQUE
        // =================================================

        if (!Pointer.current.press.wasPressedThisFrame)
            return;

        Vector2 posicaoTela =
            Pointer.current.position.ReadValue();

        // Não processa clique fora do viewport do mapa.
        if (!cameraPrincipal.pixelRect.Contains(
                posicaoTela))
        {
            return;
        }

        Vector2 posicaoMundo =
            cameraPrincipal.ScreenToWorldPoint(
                posicaoTela
            );

        Collider2D[] atingidos =
            Physics2D.OverlapPointAll(
                posicaoMundo
            );

        // =================================================
        // PRIORIDADE ABSOLUTA: CONTADOR
        // =================================================

        foreach (Collider2D collider in atingidos)
        {
            ContadorTropas contador =
                collider.GetComponent<ContadorTropas>();

            if (contador == null)
            {
                contador =
                    collider.GetComponentInParent<
                        ContadorTropas
                    >();
            }

            if (contador != null)
            {
                contadorPressionado =
                    contador;

                contadorPressionado
                    .IniciarPressaoInput();

                Debug.Log(
                    "INPUT CENTRAL | CONTADOR → " +
                    contador.gameObject.name
                );

                return;
            }
        }

        // =================================================
        // SEGUNDA PRIORIDADE: TERRITÓRIO
        // =================================================

        foreach (Collider2D collider in atingidos)
        {
            TerritorioClique territorio =
                collider.GetComponent<
                    TerritorioClique
                >();

            if (territorio == null)
            {
                territorio =
                    collider.GetComponentInParent<
                        TerritorioClique
                    >();
            }

            if (territorio != null)
            {
                Debug.Log(
                    "INPUT CENTRAL | TERRITÓRIO → " +
                    territorio.name
                );

                territorio.Clicar();

                return;
            }
        }
    }

    private void EncontrarCamera()
    {
        cameraPrincipal =
            Camera.main;

        if (cameraPrincipal == null)
        {
            cameraPrincipal =
                FindAnyObjectByType<Camera>();
        }

        if (cameraPrincipal == null)
        {
            Debug.LogError(
                "INPUT | Main Camera não encontrada."
            );
        }
    }

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.AfterSceneLoad
    )]
    private static void CriarAutomaticamente()
    {
        if (FindAnyObjectByType<InputPartida>() != null)
            return;

        GameObject objeto =
            new GameObject("InputPartida");

        DontDestroyOnLoad(objeto);

        objeto.AddComponent<InputPartida>();
    }
}