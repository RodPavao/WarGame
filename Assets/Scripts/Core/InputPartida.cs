using UnityEngine;
using UnityEngine.InputSystem;

public class InputPartida : MonoBehaviour
{
    // =====================================================
    // 1. REFERÊNCIAS E ESTADO
    // =====================================================

    private Camera cameraPrincipal;

    // Contador que está sendo pressionado neste momento.
    private ContadorTropas contadorPressionado;

    // =====================================================
    // 2. INICIALIZAÇÃO
    // =====================================================

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
        // 3. SOLTOU O CONTADOR
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
        // 4. NOVO CLIQUE / TOQUE
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
        // 5. PRIORIDADE CONTEXTUAL: MODO TRANSFERÊNCIA
        // =================================================

        if (GameManager.instance != null &&
            GameManager.instance.EmModoTransferencia &&
            TentarSelecionarTerritorioTransferencia(atingidos))
        {
            return;
        }

        // =================================================
        // 6. PRIORIDADE NORMAL: CONTADOR
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

                return;
            }
        }

        // =================================================
        // 7. SEGUNDA PRIORIDADE: TERRITÓRIO
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
                territorio.Clicar();

                return;
            }
        }
    }

    // =====================================================
    // 8. SELEÇÃO CONTEXTUAL DE TRANSFERÊNCIA
    // =====================================================

    private bool TentarSelecionarTerritorioTransferencia(
        Collider2D[] atingidos)
    {
        foreach (Collider2D collider in atingidos)
        {
            TerritorioClique territorio =
                collider.GetComponent<TerritorioClique>();

            if (territorio == null)
                territorio = collider.GetComponentInParent<TerritorioClique>();

            if (territorio == null)
            {
                ContadorTropas contador =
                    collider.GetComponent<ContadorTropas>();

                if (contador == null)
                    contador = collider.GetComponentInParent<ContadorTropas>();

                if (contador != null)
                    territorio = contador.Territorio;
            }

            if (territorio == null)
                continue;

            territorio.Clicar();
            return true;
        }

        return false;
    }

    // =====================================================
    // 9. CÂMERA
    // =====================================================

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

    // =====================================================
    // 10. CRIAÇÃO AUTOMÁTICA
    // =====================================================

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
