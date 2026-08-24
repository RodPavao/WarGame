public static class EquipesJogadores
{
    public enum Equipe
    {
        Nenhuma,
        Vanguard,
        Sentinel
    }

    public static Equipe ObterEquipe(
        TerritorioClique.Dono jogador)
    {
        switch (jogador)
        {
            case TerritorioClique.Dono.Jogador1:
            case TerritorioClique.Dono.Jogador2:
                return Equipe.Vanguard;

            case TerritorioClique.Dono.Jogador3:
            case TerritorioClique.Dono.Jogador4:
                return Equipe.Sentinel;

            default:
                return Equipe.Nenhuma;
        }
    }

    public static bool SaoAliados(
        TerritorioClique.Dono jogadorA,
        TerritorioClique.Dono jogadorB)
    {
        Equipe a = ObterEquipe(jogadorA);
        Equipe b = ObterEquipe(jogadorB);

        return
            a != Equipe.Nenhuma &&
            a == b;
    }
}