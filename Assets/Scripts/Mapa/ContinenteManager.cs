using UnityEngine;

// Fachada de compatibilidade para a UI legada. Regras e bônus vêm do mapa ativo.
public class ContinenteManager : MonoBehaviour
{
    // ============================================================
    // 1. FACHADA LEGADA DE REGIÕES
    // ============================================================

    public static ContinenteManager instance;

    private void Awake() => instance = this;

    public int ObterBonus(TerritorioClique.Continente continente)
    {
        return ObterRegiaoLegada(continente)?.bonus ?? 0;
    }

    public string ObterChaveTraducao(TerritorioClique.Continente continente)
    {
        return ObterRegiaoLegada(continente)?.chaveTraducao ?? string.Empty;
    }

    public bool JogadorControlaContinente(TerritorioClique.Dono jogador, TerritorioClique.Continente continente)
    {
        DefinicaoRegiaoMapa regiao = ObterRegiaoLegada(continente);
        return MapaAtivo.Instance != null && MapaAtivo.Instance.JogadorControlaRegiao(jogador, regiao);
    }

    public int CalcularBonusContinentes(TerritorioClique.Dono jogador)
    {
        return MapaAtivo.Instance != null ? MapaAtivo.Instance.CalcularBonusRegioes(jogador) : 0;
    }

    public void MostrarContinente(TerritorioClique.Continente continente)
    {
        DefinicaoRegiaoMapa regiao = ObterRegiaoLegada(continente);
        if (regiao == null)
            return;

        foreach (TerritorioClique territorio in MapaAtivo.ObterTerritoriosOuCena())
        {
            DefinicaoTerritorioMapa dados = MapaAtivo.Instance?.ObterDefinicaoTerritorio(territorio.idTerritorio);
            territorio.DestacarContinente(dados != null && dados.regiaoId == regiao.id
                ? regiao.corDestaque
                : new Color(0.25f, 0.25f, 0.25f, 1f));
        }

        Debug.Log($"{regiao.nomeExibido} | Bônus: +{regiao.bonus}");
    }

    // ============================================================
    // 2. DESTAQUE E TRADUÇÃO PARA A DEFINIÇÃO ATIVA
    // ============================================================

    public void OcultarDestaque()
    {
        foreach (TerritorioClique territorio in MapaAtivo.ObterTerritoriosOuCena())
            territorio.RestaurarCor();
    }

    private static DefinicaoRegiaoMapa ObterRegiaoLegada(TerritorioClique.Continente continente)
    {
        if (MapaAtivo.Instance == null || MapaAtivo.Instance.Definicao == null)
            return null;

        foreach (TerritorioClique territorio in MapaAtivo.ObterTerritoriosOuCena())
        {
            if (territorio.continente != continente)
                continue;
            DefinicaoTerritorioMapa dados = MapaAtivo.Instance.ObterDefinicaoTerritorio(territorio.idTerritorio);
            if (dados != null)
                return MapaAtivo.Instance.ObterRegiao(dados.regiaoId);
        }

        return null;
    }
}
