using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public TerritorioClique territorioSelecionado;

    // Só para teste agora:
    public TerritorioClique.Dono jogadorLocal = TerritorioClique.Dono.Jogador1;

    void Awake()
    {
        instance = this;
    }

    public void ClicarTerritorio(TerritorioClique t)
    {
        // Se não é meu território e não tenho nada selecionado, não faz nada
        if (territorioSelecionado == null && t.dono != jogadorLocal)
        {
            return;
        }

        // Se é meu território, seleciona
        if (t.dono == jogadorLocal)
        {
            if (territorioSelecionado != null)
            {
                territorioSelecionado.AtualizarCor();
            }

            territorioSelecionado = t;
            territorioSelecionado.DestacarSelecao();
            Debug.Log("Território selecionado: " + t.name);
            return;
        }

        // Se clicou em território inimigo depois de selecionar um meu
        if (territorioSelecionado != null && t.dono != jogadorLocal)
        {
            Debug.Log("Ataque planejado de " + territorioSelecionado.name + " para " + t.name);

            // Por enquanto, só registra no log
            territorioSelecionado.AtualizarCor();
            territorioSelecionado = null;
        }
    }
}