public class OrdemAtaque
{
    public TerritorioClique Origem { get; private set; }
    public TerritorioClique Destino { get; private set; }

    public int Tropas { get; private set; }

    public TerritorioClique.Dono Jogador
    {
        get;
        private set;
    }

    public OrdemAtaque(
        TerritorioClique origem,
        TerritorioClique destino,
        int tropas,
        TerritorioClique.Dono jogador)
    {
        Origem = origem;
        Destino = destino;
        Tropas = tropas;
        Jogador = jogador;
    }
}