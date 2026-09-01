// ============================================================
// 01. CONTRATO DE COMANDOS DA INTERFACE
// ============================================================

public interface IMatchUICommands
{
    void SelecionarModoAcao();
    void SelecionarModoTransferencia();
    void AumentarQuantidadeAcao();
    void DiminuirQuantidadeAcao();
    void ConfirmarAcao();
    void CancelarSelecao();
    void DesfazerUltimaAcao();
    bool RemoverAcao(int posicaoNaFila);
    bool RemoverDistribuicaoReforco(int distribuicaoId);
    void SelecionarDestinatarioTransferencia(TerritorioClique.Dono jogador);
    void RemoverTransferencia();
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
    public void EnviarAcoes() => gameManager?.EnviarAcoes();
    public void CancelarEnvio() => gameManager?.CancelarEnvio();
}
