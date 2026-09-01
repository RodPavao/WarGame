using UnityEngine;

public class TerritorioClique : MonoBehaviour
{
    private TerritorioVisual territorioVisual;
    private TerritorioContador territorioContador;
    private TerritorioTropas territorioTropas;
    private TerritorioFronteiras territorioFronteiras;

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
    // INICIALIZAÇÃO
    // =====================================================

    private void Start()
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

        territorioContador =
            GetComponent<TerritorioContador>();

        if (territorioContador == null)
        {
            territorioContador =
                gameObject.AddComponent<TerritorioContador>();
        }

        territorioTropas =
            GetComponent<TerritorioTropas>();

        if (territorioTropas == null)
        {
            territorioTropas =
                gameObject.AddComponent<TerritorioTropas>();
        }

        territorioTropas.Inicializar();

        territorioContador.Inicializar();

        AtualizarIdentidadeVisual();
    }

    // =====================================================
    // DONO
    // =====================================================

    public void DefinirDono(
        Dono novoDono)
    {
        dono = novoDono;

        AtualizarIdentidadeVisual();
    }

    public void AtualizarIdentidadeVisual()
    {
        if (territorioVisual != null)
        {
            territorioVisual.AtualizarCor();
        }

        if (territorioContador != null)
        {
            territorioContador.Atualizar();
        }
    }

    // =====================================================
    // CLIQUE
    // =====================================================

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

    // =====================================================
    // TROPAS
    // =====================================================

    public void AdicionarTropa()
    {
        if (territorioTropas == null)
            return;

        territorioTropas.Adicionar();

        territorioContador?.Atualizar();
    }

    public void RemoverTropa()
    {
        if (territorioTropas == null)
            return;

        territorioTropas.Remover();

        territorioContador?.Atualizar();
    }

    public bool RemoverTropas(
        int quantidade)
    {
        if (territorioTropas == null)
            return false;

        bool resultado =
            territorioTropas
                .RemoverQuantidade(
                    quantidade
                );

        territorioContador?.Atualizar();

        return resultado;
    }

    public void DefinirTropas(
        int quantidade)
    {
        if (territorioTropas == null)
            return;

        territorioTropas
            .DefinirQuantidade(
                quantidade
            );

        territorioContador?.Atualizar();
    }

    // =====================================================
    // VIZINHANÇA
    // =====================================================

    public bool EhVizinho(
        TerritorioClique outro)
    {
        if (territorioFronteiras == null)
            return false;

        return
            territorioFronteiras
                .EhVizinho(outro);
    }

    // =====================================================
    // VISUAL
    // =====================================================

    public void AtualizarCor()
    {
        AtualizarIdentidadeVisual();
    }

    public void AplicarEstadoVisualResolucao(int tropasVisuais, Dono donoVisual)
    {
        if (territorioVisual == null)
            territorioVisual = GetComponent<TerritorioVisual>();
        territorioVisual?.AplicarDonoVisual(donoVisual);

        ContadorTropas contador = GetComponentInChildren<ContadorTropas>(true);
        contador?.AtualizarVisual(tropasVisuais, donoVisual);
    }

    public void RestaurarEstadoVisualLogico()
    {
        AtualizarIdentidadeVisual();
    }

    public void DestacarContinente(
        Color cor)
    {
        if (territorioVisual != null)
        {
            territorioVisual
                .DestacarContinente(cor);
        }
    }

    public void RestaurarCor()
    {
        if (territorioVisual != null)
        {
            territorioVisual.RestaurarCor();
        }
    }

    public void DestacarSelecao()
    {
        if (territorioVisual != null)
        {
            territorioVisual
                .DestacarSelecao();
        }
    }

    public void RemoverDestaqueSelecao()
    {
        if (territorioVisual != null)
        {
            territorioVisual
                .RemoverDestaqueSelecao();
        }
    }
}
