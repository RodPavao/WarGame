using UnityEngine;

public class PaletaJogadores : MonoBehaviour
{
    public static PaletaJogadores instance;

    [Header("10 cores disponíveis para skins")]
    public Color[] coresDisponiveis = new Color[10];

    [Header("Cor escolhida por jogador")]
    [Range(0, 9)]
    public int skinJogador1 = 9;

    [Range(0, 9)]
    public int skinJogador2 = 1;

    [Range(0, 9)]
    public int skinJogador3 = 2;

    [Range(0, 9)]
    public int skinJogador4 = 4;

    private void Awake()
    {
        instance = this;

        GarantirPaletaPadrao();
    }

    private void GarantirPaletaPadrao()
    {
        if (coresDisponiveis == null ||
            coresDisponiveis.Length != 10)
        {
            coresDisponiveis =
                new Color[10];
        }

        // Só preenche slots vazios.
        // Não altera cores escolhidas manualmente no Inspector.

        if (coresDisponiveis[0] == default)
            coresDisponiveis[0] =
                Hex("#55BCEB"); // azul celeste

        if (coresDisponiveis[1] == default)
            coresDisponiveis[1] =
                Hex("#E6A0B8"); // rosa fraco

        if (coresDisponiveis[2] == default)
            coresDisponiveis[2] =
                Hex("#D65353"); // vermelho fraco

        if (coresDisponiveis[3] == default)
            coresDisponiveis[3] =
                Hex("#80868B"); // cinza

        if (coresDisponiveis[4] == default)
            coresDisponiveis[4] =
                Hex("#596B3D"); // verde musgo

        if (coresDisponiveis[5] == default)
            coresDisponiveis[5] =
                Hex("#E8D477"); // amarelo manteiga

        if (coresDisponiveis[6] == default)
            coresDisponiveis[6] =
                Hex("#B96A4B"); // terracota

        if (coresDisponiveis[7] == default)
            coresDisponiveis[7] =
                Hex("#65442F"); // marrom escuro

        if (coresDisponiveis[8] == default)
            coresDisponiveis[8] =
                Hex("#72C98A"); // verde claro

        if (coresDisponiveis[9] == default)
            coresDisponiveis[9] =
                Hex("#7E3045"); // vinho
    }

    private Color Hex(string codigo)
    {
        if (ColorUtility.TryParseHtmlString(
                codigo,
                out Color cor))
        {
            return cor;
        }

        return Color.white;
    }

    public Color ObterCor(
        TerritorioClique.Dono dono)
    {
        int indice;

        switch (dono)
        {
            case TerritorioClique.Dono.Jogador1:
                indice = skinJogador1;
                break;

            case TerritorioClique.Dono.Jogador2:
                indice = skinJogador2;
                break;

            case TerritorioClique.Dono.Jogador3:
                indice = skinJogador3;
                break;

            case TerritorioClique.Dono.Jogador4:
                indice = skinJogador4;
                break;

            default:
                return Color.white;
        }

        indice =
            Mathf.Clamp(
                indice,
                0,
                coresDisponiveis.Length - 1
            );

        Color cor =
    coresDisponiveis[indice];

cor.a = 1f;

return cor;

    }

    public static Color ObterCorAtiva(
        TerritorioClique.Dono dono)
    {
        if (WDMatchSetupContext.TryGetParticipant(dono, out WDMatchParticipant participant))
            return participant.MatchColor;

        if (instance == null)
        {
            instance =
                FindAnyObjectByType<PaletaJogadores>();
        }

        if (instance == null)
            return Color.white;

        return instance.ObterCor(dono);
    }
}
