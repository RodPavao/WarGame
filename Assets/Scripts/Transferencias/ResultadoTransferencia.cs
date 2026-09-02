public class ResultadoTransferencia
{
    // =====================================================
    // 1. RESULTADO IMUTÁVEL
    // =====================================================

    public bool Executada { get; private set; }
    public TerritorioClique.Dono Autor { get; private set; }
    public TerritorioClique.Dono Destinatario { get; private set; }
    public string IdTerritorio { get; private set; }
    public int TropasPreservadas { get; private set; }
    public int TropasAntes { get; private set; }
    public int TropasDepois { get; private set; }
    public TerritorioClique.Dono DonoAntes { get; private set; }
    public TerritorioClique.Dono DonoDepois { get; private set; }
    public string Motivo { get; private set; }

    public ResultadoTransferencia(
        bool executada,
        OrdemTransferencia ordem,
        int tropasPreservadas,
        string motivo,
        int tropasAntes = 0,
        int tropasDepois = 0,
        TerritorioClique.Dono donoAntes = TerritorioClique.Dono.Neutro,
        TerritorioClique.Dono donoDepois = TerritorioClique.Dono.Neutro)
    {
        Executada = executada;
        Autor = ordem != null ? ordem.Autor : TerritorioClique.Dono.Neutro;
        Destinatario = ordem != null
            ? ordem.Destinatario
            : TerritorioClique.Dono.Neutro;
        IdTerritorio = ordem != null ? ordem.IdTerritorio : string.Empty;
        TropasPreservadas = tropasPreservadas;
        TropasAntes = tropasAntes;
        TropasDepois = tropasDepois;
        DonoAntes = donoAntes;
        DonoDepois = donoDepois;
        Motivo = motivo;
    }
}
