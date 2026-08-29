using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistribuidorTerritorios : MonoBehaviour
{
    [Header("Distribuição automática 2x2")]
    [SerializeField]
    private bool distribuirAoIniciar = true;

    private IEnumerator Start()
    {
        // Todos os TerritorioClique precisam
        // terminar Start() primeiro.
        yield return null;

        if (distribuirAoIniciar)
        {
            Distribuir();
        }
    }

    [ContextMenu("Distribuir Territorios 2x2")]
    public void Distribuir()
    {
        List<TerritorioClique> lista =
            new List<TerritorioClique>(
                MapaAtivo.ObterTerritoriosOuCena()
            );

        Embaralhar(lista);

        TerritorioClique.Dono[] jogadores =
        {
            TerritorioClique.Dono.Jogador1,
            TerritorioClique.Dono.Jogador2,
            TerritorioClique.Dono.Jogador3,
            TerritorioClique.Dono.Jogador4
        };

        for (int i = 0;
             i < lista.Count;
             i++)
        {
            TerritorioClique territorio =
                lista[i];

            TerritorioClique.Dono jogador =
                jogadores[
                    i % jogadores.Length
                ];

            territorio.DefinirDono(
                jogador
            );

            territorio.DefinirTropas(
                1
            );
        }

        Debug.Log(
            "Distribuição 2x2 concluída | " +
            "J1+J2 = Vanguard | " +
            "J3+J4 = Sentinel"
        );
    }

    private void Embaralhar(
        List<TerritorioClique> lista)
    {
        for (int i = lista.Count - 1;
             i > 0;
             i--)
        {
            int j =
                Random.Range(
                    0,
                    i + 1
                );

            TerritorioClique temp =
                lista[i];

            lista[i] =
                lista[j];

            lista[j] =
                temp;
        }
    }
}
