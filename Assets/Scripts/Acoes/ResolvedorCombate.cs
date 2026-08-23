using System.Collections.Generic;
using UnityEngine;

public class ResolvedorCombate : MonoBehaviour
{
    // =====================================================
    // RESOLVER FILA
    // =====================================================

    public void Resolver(
        FilaAcoes fila)
    {
        if (fila == null)
            return;

        List<OrdemAtaque> ordens =
            fila.CriarCopiaAtaques();

        Debug.Log(
            "=== INÍCIO DA RESOLUÇÃO ==="
        );

        foreach (OrdemAtaque ordem in ordens)
        {
            ResolverAtaque(ordem);
        }

        fila.Limpar();

        Debug.Log(
            "=== FIM DA RESOLUÇÃO ==="
        );
    }

    // =====================================================
    // ATAQUE
    // =====================================================

    private ResultadoCombate ResolverAtaque(
        OrdemAtaque ordem)
    {
        if (!OrdemAindaValida(ordem))
        {
            Debug.Log(
                "ORDEM CANCELADA: estado do mapa mudou."
            );

            return new ResultadoCombate(
                false,
                false,
                0,
                0,
                0,
                0,
                "Ordem inválida no momento da resolução."
            );
        }

        TerritorioClique origem =
            ordem.Origem;

        TerritorioClique destino =
            ordem.Destino;

        int ataque =
            ordem.Tropas;

        int defesa =
            destino.Tropas;

        int minimoParaConquistar =
            CalcularAtaqueMinimo(defesa);

        // =================================================
        // CONQUISTA
        // =================================================

        if (ataque >= minimoParaConquistar)
        {
            int sobreviventes =
                ataque - defesa;

            origem.RemoverTropas(
                ataque
            );

            destino.dono =
                ordem.Jogador;

            destino.DefinirTropas(
                sobreviventes
            );

            destino.AtualizarCor();

            Debug.Log(
                "CONQUISTA: " +
                origem.name +
                " -> " +
                destino.name +
                " | Ataque: " +
                ataque +
                " | Defesa: " +
                defesa +
                " | Sobreviventes: " +
                sobreviventes
            );

            return new ResultadoCombate(
                true,
                true,
                ataque,
                defesa,
                sobreviventes,
                0,
                "Território conquistado."
            );
        }

        // =================================================
        // ATAQUE INSUFICIENTE
        // =================================================

        int sobreviventesAtaque =
            Mathf.Max(
                1,
                ataque - defesa
            );

        int sobreviventesDefesa =
            Mathf.Max(
                1,
                defesa - ataque
            );

        int perdasAtacante =
            ataque -
            sobreviventesAtaque;

        int tropasOrigemDepois =
            origem.Tropas -
            perdasAtacante;

        tropasOrigemDepois =
            Mathf.Max(
                1,
                tropasOrigemDepois
            );

        origem.DefinirTropas(
            tropasOrigemDepois
        );

        destino.DefinirTropas(
            sobreviventesDefesa
        );

        Debug.Log(
            "ATAQUE REPELIDO: " +
            origem.name +
            " -> " +
            destino.name +
            " | Ataque enviado: " +
            ataque +
            " | Defesa: " +
            defesa +
            " | Origem agora: " +
            origem.Tropas +
            " | Defesa agora: " +
            destino.Tropas
        );

        return new ResultadoCombate(
            true,
            false,
            ataque,
            defesa,
            sobreviventesAtaque,
            sobreviventesDefesa,
            "Ataque insuficiente para conquista."
        );
    }

    // =====================================================
    // REVALIDAÇÃO
    // =====================================================

    private bool OrdemAindaValida(
        OrdemAtaque ordem)
    {
        if (ordem == null)
            return false;

        if (ordem.Origem == null ||
            ordem.Destino == null)
            return false;

        // Origem mudou de dono.
        if (ordem.Origem.dono !=
            ordem.Jogador)
            return false;

        // O destino agora já pertence
        // ao próprio jogador.
        if (ordem.Destino.dono ==
            ordem.Jogador)
            return false;

        // Fronteira deixou de ser válida.
        if (!ordem.Origem.EhVizinho(
                ordem.Destino))
            return false;

        // Tropas foram gastas por ação anterior.
        if (ordem.Tropas >
            ordem.Origem.Tropas - 1)
            return false;

        if (ordem.Tropas < 1)
            return false;

        return true;
    }

    // =====================================================
    // REGRA V1 DE FORÇA
    // =====================================================

    public int CalcularAtaqueMinimo(
        int defesa)
    {
        defesa =
            Mathf.Max(
                1,
                defesa
            );

        float percentual =
            defesa <= 10
                ? 0.30f
                : 0.15f;

        int vantagemPercentual =
            Mathf.CeilToInt(
                defesa *
                percentual
            );

        int vantagemNecessaria =
            Mathf.Max(
                2,
                vantagemPercentual
            );

        return
            defesa +
            vantagemNecessaria;
    }
}