using UnityEngine;

public class TerritorioClique : MonoBehaviour
{

    // Tudo isso pertence à classe
    private TerritorioVisual territorioVisual;
    private TerritorioContador territorioContador;
    private TerritorioTropas territorioTropas;
    
    // =====================================================
    // IDENTIFICAÇÃO
    // =====================================================

    public string idTerritorio;

    public string chaveTraducao;

    public enum Continente
    {
        AmericaDoNorte,
        AmericaDoSul,
        Africa,
        Europa,
        Asia,
        Oceania
    }

    public Continente continente;

    // =====================================================
    // JOGADOR
    // =====================================================

    public enum Dono
{
    Neutro,
    Jogador1,
    Jogador2,
    Jogador3,
    Jogador4,
    Jogador5,
    Jogador6
}

    public Dono dono = Dono.Neutro;

    // =====================================================
    // EXÉRCITO
    // =====================================================

    public int Tropas
{
    get
    {
        if (territorioTropas == null)
            return 1;

        return territorioTropas.Quantidade;
    }
}


    // =====================================================
    // FRONTEIRAS
    // =====================================================

    private TerritorioFronteiras territorioFronteiras;

    void Start()
{
    territorioVisual =
    GetComponent<TerritorioVisual>();

if (territorioVisual == null)
{
    territorioVisual =
        gameObject.AddComponent<TerritorioVisual>();
}

territorioFronteiras =
    GetComponent<TerritorioFronteiras>();

if (territorioFronteiras == null)
{
    Debug.LogError(
        "TerritorioFronteiras ausente em " +
        name
    );
}

territorioVisual.AtualizarCor();

territorioContador =
    GetComponent<TerritorioContador>();

if (territorioContador == null)
{
    territorioContador =
        gameObject.AddComponent<TerritorioContador>();
}

territorioContador.Inicializar();

territorioTropas =
    GetComponent<TerritorioTropas>();

if (territorioTropas == null)
{
    territorioTropas =
        gameObject.AddComponent<TerritorioTropas>();
}

territorioTropas.Inicializar();

}

public void Clicar()
{
    if (GameManager.instance == null)
    {
        Debug.LogError(
            "GameManager não encontrado na Scene."
        );

        return;
    }

    GameManager.instance
        .ClicarTerritorio(this);
}


public void AdicionarTropa()
{
    if (territorioTropas == null)
        return;

    territorioTropas.Adicionar();

    territorioContador.Atualizar();
}

public void RemoverTropa()
{
    if (territorioTropas == null)
        return;

    territorioTropas.Remover();

    territorioContador.Atualizar();
}

public bool RemoverTropas(int quantidade)
{
    if (territorioTropas == null)
        return false;

    return territorioTropas
        .RemoverQuantidade(quantidade);
}

public void DefinirTropas(int quantidade)
{
    if (territorioTropas == null)
        return;

    territorioTropas
        .DefinirQuantidade(quantidade);
}

public bool EhVizinho(TerritorioClique outro)
{
    if (territorioFronteiras == null)
        return false;

    return territorioFronteiras.EhVizinho(outro);
}

public void AtualizarCor()
{
    if (territorioVisual != null)
        territorioVisual.AtualizarCor();
}

public void DestacarContinente(Color cor)
{
    if (territorioVisual != null)
        territorioVisual.DestacarContinente(cor);
}

public void RestaurarCor()
{
    if (territorioVisual != null)
        territorioVisual.RestaurarCor();
}

public void DestacarSelecao()
{
    if (territorioVisual != null)
        territorioVisual.DestacarSelecao();
} // Essa chave fecha o MÉTODO

} // Essa chave fecha a CLASSE