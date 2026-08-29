using System;

[Serializable]
public class ResultadoPartida
{
    // =====================================================
    // 1. TIPO E IDENTIDADE DO RESULTADO
    // =====================================================

    public enum TipoVencedor
    {
        Nenhum,
        Jogador,
        Equipe
    }

    public bool Encerrada { get; private set; }
    public bool Empate { get; private set; }
    public TipoVencedor Tipo { get; private set; }
    public TerritorioClique.Dono JogadorVencedor { get; private set; }
    public EquipesJogadores.Equipe EquipeVencedora { get; private set; }
    public int QuantidadeTerritorios { get; private set; }
    public int RoundFinal { get; private set; }
    public bool HouveMorteSubita { get; private set; }

    // =====================================================
    // 2. FÁBRICAS DE RESULTADO
    // =====================================================

    public static ResultadoPartida CriarEmpate(
        int round,
        int quantidade,
        bool morteSubita)
    {
        return new ResultadoPartida
        {
            Encerrada = false,
            Empate = true,
            Tipo = TipoVencedor.Nenhum,
            QuantidadeTerritorios = quantidade,
            RoundFinal = round,
            HouveMorteSubita = morteSubita
        };
    }

    public static ResultadoPartida CriarVitoriaEquipe(
        EquipesJogadores.Equipe equipe,
        int quantidade,
        int round,
        bool morteSubita)
    {
        return new ResultadoPartida
        {
            Encerrada = true,
            Tipo = TipoVencedor.Equipe,
            EquipeVencedora = equipe,
            QuantidadeTerritorios = quantidade,
            RoundFinal = round,
            HouveMorteSubita = morteSubita
        };
    }

    public static ResultadoPartida CriarVitoriaJogador(
        TerritorioClique.Dono jogador,
        int quantidade,
        int round,
        bool morteSubita)
    {
        return new ResultadoPartida
        {
            Encerrada = true,
            Tipo = TipoVencedor.Jogador,
            JogadorVencedor = jogador,
            QuantidadeTerritorios = quantidade,
            RoundFinal = round,
            HouveMorteSubita = morteSubita
        };
    }
}
