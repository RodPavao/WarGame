using UnityEngine;
using UnityEngine.InputSystem;

public class InputPartida : MonoBehaviour
{
    private Camera cameraPrincipal;

    private void Awake()
{
    cameraPrincipal = Camera.main;

    if (cameraPrincipal == null)
    {
        cameraPrincipal =
            FindAnyObjectByType<Camera>();
    }

    if (cameraPrincipal == null)
    {
        Debug.LogError(
            "Nenhuma câmera encontrada na Scene."
        );
    }
}

    private void Update()
    {
        if (Pointer.current == null)
            return;

        if (!Pointer.current.press.wasPressedThisFrame)
            return;

        if (cameraPrincipal == null)
{
    cameraPrincipal = Camera.main;

    if (cameraPrincipal == null)
    {
        cameraPrincipal =
            FindAnyObjectByType<Camera>();
    }

    if (cameraPrincipal == null)
        return;
}

        Vector2 posicaoTela =
            Pointer.current.position.ReadValue();

        Vector2 posicaoMundo =
            cameraPrincipal.ScreenToWorldPoint(
                posicaoTela
            );

        Collider2D[] atingidos =
            Physics2D.OverlapPointAll(
                posicaoMundo
            );

        // PRIORIDADE 1: CONTADOR
        foreach (Collider2D collider in atingidos)
        {
            ContadorTropas contador =
                collider.GetComponent<ContadorTropas>();

            if (contador != null)
            {
                contador.Clicar();
                return;
            }
        }

        // PRIORIDADE 2: TERRITÓRIO
        foreach (Collider2D collider in atingidos)
        {
            TerritorioClique territorio =
                collider.GetComponent<TerritorioClique>();

            if (territorio != null)
            {
                territorio.Clicar();
                return;
            }
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