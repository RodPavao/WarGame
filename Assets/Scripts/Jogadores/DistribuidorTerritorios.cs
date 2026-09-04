using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistribuidorTerritorios : MonoBehaviour
{
    [Header("Distribuição automática da partida")]
    [SerializeField]
    private bool distribuirAoIniciar = true;
    private bool distribuicaoInicialExecutada;

    private IEnumerator Start()
    {
        // Todos os TerritorioClique precisam
        // terminar Start() primeiro.
        yield return null;

        if (distribuirAoIniciar)
            GarantirDistribuicaoInicial();
    }

    public void GarantirDistribuicaoInicial()
    {
        if (!distribuicaoInicialExecutada)
            Distribuir();
    }

    [ContextMenu("Distribuir Territórios da Partida")]
    public void Distribuir()
    {
        List<TerritorioClique> lista =
            new List<TerritorioClique>(
                MapaAtivo.ObterTerritoriosOuCena()
            );

        List<TerritorioClique.Dono> jogadores = ObterParticipantesAtivos();
        if (jogadores.Count == 0)
        {
            Debug.LogError("DISTRIBUIÇÃO | Nenhum participante ativo válido.");
            return;
        }

        Embaralhar(lista);
        for (int i = 0;
             i < lista.Count;
             i++)
        {
            TerritorioClique territorio =
                lista[i];

            TerritorioClique.Dono jogador =
                jogadores[
                    i % jogadores.Count
                ];

            territorio.DefinirDono(
                jogador
            );

            territorio.DefinirTropas(
                1
            );
        }

        distribuicaoInicialExecutada = true;
        WDMatchSetup setup = WDMatchSetupContext.Current;
        Debug.Log(
            $"Distribuição concluída | Participantes ativos: {jogadores.Count}" +
            (setup != null ? $" | Modo: {setup.ModeId}" : " | Legado sem Match Setup"));
    }

    // =====================================================
    // 2. PARTICIPANTES AUTORITATIVOS DO MATCH SETUP
    // =====================================================

    private static List<TerritorioClique.Dono> ObterParticipantesAtivos()
    {
        var jogadores = new List<TerritorioClique.Dono>();
        WDMatchSetup setup = WDMatchSetupContext.Current;
        if (setup != null)
        {
            foreach (WDMatchParticipant participant in setup.Participants)
            {
                int ownerValue = participant.SlotIndex + 1;
                if (System.Enum.IsDefined(typeof(TerritorioClique.Dono), ownerValue))
                    jogadores.Add((TerritorioClique.Dono)ownerValue);
            }
            return jogadores;
        }

        jogadores.Add(TerritorioClique.Dono.Jogador1);
        jogadores.Add(TerritorioClique.Dono.Jogador2);
        jogadores.Add(TerritorioClique.Dono.Jogador3);
        jogadores.Add(TerritorioClique.Dono.Jogador4);
        return jogadores;
    }

    // =====================================================
    // 3. EMBARALHAMENTO PRESERVADO
    // =====================================================

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
