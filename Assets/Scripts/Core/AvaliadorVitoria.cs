using System.Collections.Generic;
using UnityEngine;

public class AvaliadorVitoria : MonoBehaviour
{
    // =====================================================
    // 1. IDENTIDADE GENERALIZADA DO LADO COMPETITIVO
    // =====================================================

    private struct LadoCompetitivo
    {
        public ResultadoPartida.TipoVencedor Tipo;
        public int Id;

        public LadoCompetitivo(ResultadoPartida.TipoVencedor tipo, int id)
        {
            Tipo = tipo;
            Id = id;
        }
    }

    // =====================================================
    // 2. CONTAGEM E AVALIAÇÃO
    // =====================================================

    public ResultadoPartida Avaliar(
        IReadOnlyCollection<TerritorioClique.Dono> participantes,
        int round,
        bool morteSubita)
    {
        Dictionary<LadoCompetitivo, int> contagem =
            new Dictionary<LadoCompetitivo, int>();

        if (participantes != null)
        {
            foreach (TerritorioClique.Dono jogador in participantes)
                GarantirLado(contagem, ObterLado(jogador));
        }

        foreach (TerritorioClique territorio in
                 MapaAtivo.ObterTerritoriosOuCena())
        {
            if (territorio.dono == TerritorioClique.Dono.Neutro)
                continue;

            LadoCompetitivo lado = ObterLado(territorio.dono);
            GarantirLado(contagem, lado);
            contagem[lado]++;
        }

        int maiorQuantidade = -1;
        int quantidadeLideres = 0;
        LadoCompetitivo vencedor = default;

        foreach (KeyValuePair<LadoCompetitivo, int> item in contagem)
        {
            if (item.Value > maiorQuantidade)
            {
                maiorQuantidade = item.Value;
                quantidadeLideres = 1;
                vencedor = item.Key;
            }
            else if (item.Value == maiorQuantidade)
            {
                quantidadeLideres++;
            }
        }

        if (quantidadeLideres != 1)
        {
            return ResultadoPartida.CriarEmpate(
                round,
                Mathf.Max(0, maiorQuantidade),
                morteSubita);
        }

        if (vencedor.Tipo == ResultadoPartida.TipoVencedor.Equipe)
        {
            return ResultadoPartida.CriarVitoriaEquipe(
                (EquipesJogadores.Equipe)vencedor.Id,
                maiorQuantidade,
                round,
                morteSubita);
        }

        return ResultadoPartida.CriarVitoriaJogador(
            (TerritorioClique.Dono)vencedor.Id,
            maiorQuantidade,
            round,
            morteSubita);
    }

    // =====================================================
    // 3. AGRUPAMENTO EQUIPE/INDIVIDUAL
    // =====================================================

    private static LadoCompetitivo ObterLado(TerritorioClique.Dono jogador)
    {
        EquipesJogadores.Equipe equipe =
            EquipesJogadores.ObterEquipe(jogador);

        return equipe != EquipesJogadores.Equipe.Nenhuma
            ? new LadoCompetitivo(
                ResultadoPartida.TipoVencedor.Equipe,
                (int)equipe)
            : new LadoCompetitivo(
                ResultadoPartida.TipoVencedor.Jogador,
                (int)jogador);
    }

    private static void GarantirLado(
        Dictionary<LadoCompetitivo, int> contagem,
        LadoCompetitivo lado)
    {
        if (!contagem.ContainsKey(lado))
            contagem.Add(lado, 0);
    }
}
