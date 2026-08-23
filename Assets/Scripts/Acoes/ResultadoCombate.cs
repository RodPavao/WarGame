public class ResultadoCombate
{
    public bool Valido { get; private set; }
    public bool Conquistou { get; private set; }

    public int TropasAtacantesAntes { get; private set; }
    public int TropasDefensorasAntes { get; private set; }

    public int SobreviventesAtacante { get; private set; }
    public int SobreviventesDefensor { get; private set; }

    public string Motivo { get; private set; }

    public ResultadoCombate(
        bool valido,
        bool conquistou,
        int tropasAtacantesAntes,
        int tropasDefensorasAntes,
        int sobreviventesAtacante,
        int sobreviventesDefensor,
        string motivo)
    {
        Valido = valido;
        Conquistou = conquistou;

        TropasAtacantesAntes =
            tropasAtacantesAntes;

        TropasDefensorasAntes =
            tropasDefensorasAntes;

        SobreviventesAtacante =
            sobreviventesAtacante;

        SobreviventesDefensor =
            sobreviventesDefensor;

        Motivo = motivo;
    }
}