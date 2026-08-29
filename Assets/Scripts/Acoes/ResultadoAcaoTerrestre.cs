public class ResultadoAcaoTerrestre
{
    // =====================================================
    // 1. CLASSIFICAÇÃO
    // =====================================================

    public enum TipoResultado
    {
        Cancelada,
        Ataque,
        MovimentoProprio,
        MovimentoAliado
    }

    public bool Executada { get; private set; }
    public TipoResultado Tipo { get; private set; }
    public int QuantidadeEfetiva { get; private set; }
    public string Motivo { get; private set; }
    public ResultadoCombate Combate { get; private set; }

    // =====================================================
    // 2. CONSTRUÇÃO DO RESULTADO
    // =====================================================

    public ResultadoAcaoTerrestre(
        bool executada,
        TipoResultado tipo,
        int quantidadeEfetiva,
        string motivo,
        ResultadoCombate combate = null)
    {
        Executada = executada;
        Tipo = tipo;
        QuantidadeEfetiva = quantidadeEfetiva;
        Motivo = motivo;
        Combate = combate;
    }
}
