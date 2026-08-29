using UnityEngine;

public class SistemaTransferencias : MonoBehaviour
{
    // =====================================================
    // 1. ESTADO E INICIALIZAÇÃO
    // =====================================================

    private FilaTransferencias fila;

    public void Inicializar(FilaTransferencias filaTransferencias)
    {
        fila = filaTransferencias;
    }

    // =====================================================
    // 2. VALIDAÇÃO E REGISTRO
    // =====================================================

    public bool Registrar(
        TerritorioClique territorio,
        TerritorioClique.Dono autor,
        TerritorioClique.Dono destinatario,
        out string motivo)
    {
        if (fila == null)
            return Falhar("Sistema de transferências indisponível.", out motivo);

        if (fila.PossuiPara(autor))
            return Falhar("Limite de 1 transferência atingido.", out motivo);

        if (territorio == null || territorio.dono != autor)
            return Falhar("O território não pertence ao jogador autor.", out motivo);

        if (destinatario == autor)
            return Falhar("Você não pode transferir para si mesmo.", out motivo);

        if (!EquipesJogadores.SaoAliados(autor, destinatario))
            return Falhar("Selecione um aliado.", out motivo);

        bool adicionada = fila.Adicionar(
            new OrdemTransferencia(territorio, autor, destinatario));

        motivo = adicionada
            ? "Transferência preparada."
            : "Não foi possível preparar a transferência.";

        return adicionada;
    }

    private bool Falhar(string mensagem, out string motivo)
    {
        motivo = mensagem;
        Debug.Log("TRANSFERÊNCIA INVÁLIDA: " + mensagem);
        return false;
    }
}
