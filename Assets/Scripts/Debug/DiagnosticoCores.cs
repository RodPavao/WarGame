using System.Collections;
using System.Linq;
using UnityEngine;

public class DiagnosticoCores : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Criar()
    {
        GameObject obj =
            new GameObject("DIAGNOSTICO_CORES");

        DontDestroyOnLoad(obj);

        obj.AddComponent<DiagnosticoCores>();
    }

    private IEnumerator Start()
    {
        // Espera toda inicialização e distribuição 2x2.
        yield return null;
        yield return null;
        yield return null;

        Debug.Log("========== DIAGNÓSTICO DE CORES ==========");

        PaletaJogadores paleta =
            FindFirstObjectByType<PaletaJogadores>(
                FindObjectsInactive.Include
            );

        if (paleta == null)
        {
            Debug.LogError(
                "RESULTADO: PaletaJogadores NÃO encontrada."
            );

            yield break;
        }

        Debug.Log(
            "PALETA ENCONTRADA | Objeto: " +
            paleta.gameObject.name +
            " | Ativo: " +
            paleta.gameObject.activeInHierarchy
        );

        Debug.Log(
            "SKINS | J1=" + paleta.skinJogador1 +
            " J2=" + paleta.skinJogador2 +
            " J3=" + paleta.skinJogador3 +
            " J4=" + paleta.skinJogador4
        );

        TerritorioClique[] territorios =
            FindObjectsByType<TerritorioClique>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None
            );

        TerritorioClique.Dono[] jogadores =
        {
            TerritorioClique.Dono.Jogador1,
            TerritorioClique.Dono.Jogador2,
            TerritorioClique.Dono.Jogador3,
            TerritorioClique.Dono.Jogador4
        };

        foreach (TerritorioClique.Dono jogador in jogadores)
        {
            TerritorioClique territorio =
                territorios.FirstOrDefault(
                    t => t.dono == jogador
                );

            if (territorio == null)
            {
                Debug.LogError(
                    jogador +
                    " não possui território encontrado."
                );

                continue;
            }

            SpriteRenderer sr =
                territorio.GetComponent<SpriteRenderer>();

            Color corEsperada =
                paleta.ObterCor(jogador);

            string material =
                sr != null &&
                sr.sharedMaterial != null
                    ? sr.sharedMaterial.name
                    : "SEM MATERIAL";

            string shader =
                sr != null &&
                sr.sharedMaterial != null &&
                sr.sharedMaterial.shader != null
                    ? sr.sharedMaterial.shader.name
                    : "SEM SHADER";

            Debug.Log(
                jogador +
                " | território=" + territorio.name +
                " | ESPERADA=" +
                ColorUtility.ToHtmlStringRGBA(corEsperada) +
                " | SPRITE=" +
                (
                    sr != null
                        ? ColorUtility.ToHtmlStringRGBA(sr.color)
                        : "SEM SPRITERENDERER"
                ) +
                " | MATERIAL=" + material +
                " | SHADER=" + shader
            );
        }

        Debug.Log("========== FIM DIAGNÓSTICO ==========");
    }
}