using System.Collections.Generic;

// =====================================================
// 1. OPERAÇÃO DE DISTRIBUIÇÃO
// =====================================================

public sealed class DistribuicaoReforco
{
    public int Id { get; }
    public TerritorioClique Territorio { get; }
    public int Quantidade { get; }

    public DistribuicaoReforco(
        int id,
        TerritorioClique territorio,
        int quantidade)
    {
        Id = id;
        Territorio = territorio;
        Quantidade = quantidade;
    }
}

// =====================================================
// 2. HISTÓRICO DA PREPARAÇÃO
// =====================================================

public sealed class HistoricoReforcos
{
    private readonly List<DistribuicaoReforco> distribuicoes =
        new List<DistribuicaoReforco>();

    private int proximoId = 1;

    public IReadOnlyList<DistribuicaoReforco> Distribuicoes =>
        distribuicoes;

    public DistribuicaoReforco Registrar(
        TerritorioClique territorio,
        int quantidade)
    {
        if (territorio == null || quantidade <= 0)
            return null;

        if (distribuicoes.Count > 0)
        {
            int ultimoIndice = distribuicoes.Count - 1;
            DistribuicaoReforco ultima =
                distribuicoes[ultimoIndice];

            if (ultima.Territorio == territorio)
            {
                DistribuicaoReforco agregada =
                    new DistribuicaoReforco(
                        ultima.Id,
                        territorio,
                        ultima.Quantidade + quantidade
                    );

                distribuicoes[ultimoIndice] = agregada;
                return agregada;
            }
        }

        DistribuicaoReforco distribuicao =
            new DistribuicaoReforco(
                proximoId++,
                territorio,
                quantidade
            );

        distribuicoes.Add(distribuicao);

        return distribuicao;
    }

    public bool TentarObter(
        int id,
        out DistribuicaoReforco distribuicao)
    {
        for (int i = 0; i < distribuicoes.Count; i++)
        {
            if (distribuicoes[i].Id != id)
                continue;

            distribuicao = distribuicoes[i];
            return true;
        }

        distribuicao = null;
        return false;
    }

    public bool Remover(int id)
    {
        for (int i = 0; i < distribuicoes.Count; i++)
        {
            if (distribuicoes[i].Id != id)
                continue;

            distribuicoes.RemoveAt(i);
            return true;
        }

        return false;
    }

    public void Limpar()
    {
        distribuicoes.Clear();
        proximoId = 1;
    }
}
