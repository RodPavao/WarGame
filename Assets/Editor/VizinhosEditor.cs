using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class VizinhosEditor
{
    // =========================================================
    // CONFIGURAR VIZINHOS
    // =========================================================

    [MenuItem("WarGame/Configurar Vizinhos")]
    public static void ConfigurarVizinhos()
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
            Transform origem =
                pai.transform.Find(item.Key);

            if (origem == null)
            {
                Debug.LogError(
                    "Território não encontrado: " +
                    item.Key
                );

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

            TerritorioFronteiras fronteiras =
                territorio.GetComponent<TerritorioFronteiras>();

            if (fronteiras == null)
            {
                fronteiras =
                    territorio.gameObject
                        .AddComponent<TerritorioFronteiras>();
            }

            fronteiras.Configurar(
                lista.ToArray()
            );

            EditorUtility.SetDirty(
                fronteiras
            );

            configurados++;
        }

        UnityEditor.SceneManagement
            .EditorSceneManager
            .MarkSceneDirty(
                UnityEngine.SceneManagement
                    .SceneManager
                    .GetActiveScene()
            );

        Debug.Log(
            "VIZINHOS CONFIGURADOS | Territorios: " +
            configurados +
            " | Erros: " +
            erros
        );
    }
}