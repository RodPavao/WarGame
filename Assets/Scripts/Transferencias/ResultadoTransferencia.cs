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
    public string Motivo { get; private set; }

    public ResultadoTransferencia(
        bool executada,
        OrdemTransferencia ordem,
        int tropasPreservadas,
        string motivo)
    {
        Executada = executada;
        Autor = ordem != null ? ordem.Autor : TerritorioClique.Dono.Neutro;
        Destinatario = ordem != null
            ? ordem.Destinatario
            : TerritorioClique.Dono.Neutro;
        IdTerritorio = ordem != null ? ordem.IdTerritorio : string.Empty;
        TropasPreservadas = tropasPreservadas;
        Motivo = motivo;
    }
}
