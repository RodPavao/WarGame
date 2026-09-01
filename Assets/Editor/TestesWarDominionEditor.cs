#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

public static class TestesWarDominionEditor
{
    private const string Raiz = "War Dominion/Testes/";

    [MenuItem(Raiz + "Partida/Iniciar Partida Teste")]
    private static void IniciarPartida() => ComRodada(r => r.TesteEditorIniciarPartida());
    [MenuItem(Raiz + "Partida/Iniciar Partida Teste", true)]
    private static bool ValidarIniciarPartida() => EmPlayCom<GerenciadorRodada>();

    [MenuItem(Raiz + "Partida/Redistribuir Territórios 2x2")]
    private static void RedistribuirTerritorios()
    {
        if (!EditorApplication.isPlaying) return;
        DistribuidorTerritorios distribuidor = UnityEngine.Object.FindAnyObjectByType<DistribuidorTerritorios>();
        if (distribuidor != null) distribuidor.Distribuir();
    }
    [MenuItem(Raiz + "Partida/Redistribuir Territórios 2x2", true)]
    private static bool ValidarRedistribuirTerritorios() => EmPlayCom<DistribuidorTerritorios>();

    [MenuItem(Raiz + "Rodada/Iniciar Próxima Rodada")]
    private static void ProximaRodada() => ComRodada(r => r.TesteEditorIniciarProximaRodada());
    [MenuItem(Raiz + "Rodada/Iniciar Próxima Rodada", true)]
    private static bool ValidarProximaRodada() => EmPlayCom<GerenciadorRodada>();

    [MenuItem(Raiz + "Rodada/Resolver Round Atual")]
    private static void Resolver() => ComRodada(r => r.TesteEditorResolverRoundAtual());
    [MenuItem(Raiz + "Rodada/Resolver Round Atual", true)]
    private static bool ValidarResolver() => EmPlayCom<GerenciadorRodada>();

    [MenuItem(Raiz + "Rodada/Preparar Round 10")]
    private static void Round10() => ComRodada(r => r.TesteEditorPrepararRound10());
    [MenuItem(Raiz + "Rodada/Preparar Round 10", true)]
    private static bool ValidarRound10() => EmPlayCom<GerenciadorRodada>();

    [MenuItem(Raiz + "Rodada/Preparar Round 10 Empatado")]
    private static void Round10Empatado() => ComRodada(r => r.TesteEditorPrepararRound10Empatado());
    [MenuItem(Raiz + "Rodada/Preparar Round 10 Empatado", true)]
    private static bool ValidarRound10Empatado() => EmPlayCom<GerenciadorRodada>();

    [MenuItem(Raiz + "Rodada/Forçar Desempate")]
    private static void Desempate() => ComRodada(r => r.TesteEditorForcarDesempate());
    [MenuItem(Raiz + "Rodada/Forçar Desempate", true)]
    private static bool ValidarDesempate() => EmPlayCom<GerenciadorRodada>();

    [MenuItem(Raiz + "Ações/Enviar Ações")]
    private static void EnviarAcoes() => ComGameManager(g => g.EnviarAcoes());
    [MenuItem(Raiz + "Ações/Enviar Ações", true)]
    private static bool ValidarEnviarAcoes() => EmPlayCom<GameManager>();

    [MenuItem(Raiz + "Jogadores Simulados/Ativar Autoenvio")]
    private static void AtivarAutoenvio() => ComGameManager(g => g.TesteEditorAtivarAutoenvio());
    [MenuItem(Raiz + "Jogadores Simulados/Ativar Autoenvio", true)]
    private static bool ValidarAtivarAutoenvio() => EmPlayCom<GameManager>();

    [MenuItem(Raiz + "Jogadores Simulados/Desativar Autoenvio")]
    private static void DesativarAutoenvio() => ComGameManager(g => g.TesteEditorDesativarAutoenvio());
    [MenuItem(Raiz + "Jogadores Simulados/Desativar Autoenvio", true)]
    private static bool ValidarDesativarAutoenvio() => EmPlayCom<GameManager>();

    private static bool EmPlayCom<T>() where T : UnityEngine.Object =>
        EditorApplication.isPlaying && UnityEngine.Object.FindAnyObjectByType<T>() != null;

    private static void ComRodada(Action<GerenciadorRodada> acao)
    {
        if (!EditorApplication.isPlaying) return;
        GerenciadorRodada rodada = UnityEngine.Object.FindAnyObjectByType<GerenciadorRodada>();
        if (rodada != null) acao(rodada);
    }

    private static void ComGameManager(Action<GameManager> acao)
    {
        if (!EditorApplication.isPlaying) return;
        GameManager manager = GameManager.instance ?? UnityEngine.Object.FindAnyObjectByType<GameManager>();
        if (manager != null) acao(manager);
    }
}
#endif
