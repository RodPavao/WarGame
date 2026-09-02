// ============================================================
// 01. CONTRATO DE COMANDOS DA INTERFACE
// ============================================================

public interface IMatchUICommands
{
    void SelecionarModoAcao();
    void SelecionarModoTransferencia();
    bool SelecionarTerritorio(string territorioId);
    int AdicionarReforco(string territorioId, int quantidade);
    void AumentarQuantidadeAcao();
    void DiminuirQuantidadeAcao();
    void ConfirmarAcao();
    void CancelarSelecao();
    void DesfazerUltimaAcao();
    bool RemoverAcao(int posicaoNaFila);
    bool RemoverDistribuicaoReforco(int distribuicaoId);
    void SelecionarDestinatarioTransferencia(TerritorioClique.Dono jogador);
    void RemoverTransferencia();
    void CancelarSelecaoTransferencia();
    void EnviarAcoes();
    void CancelarEnvio();
}

// ============================================================
// 02. FACHADA SEM LÓGICA DE GAMEPLAY
// ============================================================

public sealed class MatchUICommands : IMatchUICommands
{
    private readonly GameManager gameManager;

    public MatchUICommands(GameManager gameManager)
    {
        this.gameManager = gameManager;
    }

    public void SelecionarModoAcao() => gameManager?.SelecionarModoAcaoTerrestre();
    public void SelecionarModoTransferencia() => gameManager?.SelecionarModoTransferencia();

    public bool SelecionarTerritorio(string territorioId)
    {
        if (!TentarObterTerritorio(territorioId, out TerritorioClique territorio))
            return false;
        gameManager.ClicarTerritorio(territorio);
        return true;
    }

    public int AdicionarReforco(string territorioId, int quantidade)
    {
        return TentarObterTerritorio(territorioId, out TerritorioClique territorio)
            ? gameManager.DistribuirReforcos(territorio, quantidade)
            : 0;
    }
    public void AumentarQuantidadeAcao() => gameManager?.AumentarQuantidadeAcao();
    public void DiminuirQuantidadeAcao() => gameManager?.DiminuirQuantidadeAcao();
    public void ConfirmarAcao() => gameManager?.ConfirmarAcaoPreparada();
    public void CancelarSelecao() => gameManager?.CancelarSelecaoAtual();
    public void DesfazerUltimaAcao() => gameManager?.CancelarUltimaOrdem();

    public bool RemoverAcao(int posicaoNaFila)
    {
        if (gameManager?.OrdensPreparadas == null)
            return false;

        foreach (OrdemTerrestre ordem in gameManager.OrdensPreparadas)
        {
            if (ordem.PosicaoNaFila == posicaoNaFila)
                return gameManager.CancelarOrdem(ordem);
        }

        return false;
    }

    public bool RemoverDistribuicaoReforco(int distribuicaoId) =>
        gameManager != null && gameManager.DesfazerDistribuicaoReforco(distribuicaoId);

    public void SelecionarDestinatarioTransferencia(TerritorioClique.Dono jogador) =>
        gameManager?.TentarPrepararTransferenciaPara(jogador);

    public void RemoverTransferencia() => gameManager?.RemoverTransferencia();
    public void CancelarSelecaoTransferencia() =>
        gameManager?.CancelarSelecaoTransferencia();
    public void EnviarAcoes() => gameManager?.EnviarAcoes();
    public void CancelarEnvio() => gameManager?.CancelarEnvio();

    // ============================================================
    // 03. RESOLUÇÃO GENÉRICA POR ID, SEM REFERÊNCIAS DE VIEW
    // ============================================================

    private bool TentarObterTerritorio(
        string territorioId,
        out TerritorioClique territorio)
    {
        territorio = null;
        if (gameManager == null || string.IsNullOrWhiteSpace(territorioId))
            return false;

        if (MapaAtivo.Instance != null &&
            MapaAtivo.Instance.TentarObterTerritorio(territorioId, out territorio))
        {
            return territorio != null;
        }

        foreach (TerritorioClique candidato in MapaAtivo.ObterTerritoriosOuCena())
        {
            if (candidato != null && candidato.idTerritorio == territorioId)
            {
                territorio = candidato;
                return true;
            }
        }

        return false;
    }
}
