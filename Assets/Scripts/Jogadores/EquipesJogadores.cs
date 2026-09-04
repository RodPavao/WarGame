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
        WDMatchSetup setup = WDMatchSetupContext.Current;
        if (setup != null)
        {
            int slot = (int)jogador - 1;
            WDMatchParticipant participante = null;
            int membrosNaEquipe = 0;

            foreach (WDMatchParticipant candidato in setup.Participants)
                if (candidato.SlotIndex == slot)
                    participante = candidato;

            if (participante == null)
                return Equipe.Nenhuma;

            foreach (WDMatchParticipant candidato in setup.Participants)
                if (candidato.TeamIndex == participante.TeamIndex)
                    membrosNaEquipe++;

            if (membrosNaEquipe < 2)
                return Equipe.Nenhuma;

            return participante.TeamIndex == 0
                ? Equipe.Vanguard
                : participante.TeamIndex == 1
                    ? Equipe.Sentinel
                    : Equipe.Nenhuma;
        }

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
