using System.Collections.Generic;
using UnityEngine;

public class ResolvedorCombate : MonoBehaviour
{
    public void Resolver(FilaAcoes fila)
    {
        if (fila == null)
            return;

        List<OrdemAtaque> ordens =
            fila.CriarCopiaAtaques();

        Debug.Log("=== INÍCIO DA RESOLUÇÃO ===");

        foreach (OrdemAtaque ordem in ordens)
        {
            ResolverAtaque(ordem);
        }

        fila.Limpar();

        Debug.Log("=== FIM DA RESOLUÇÃO ===");
    }

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

        // ================================================
        // CONQUISTA
        // ================================================

        if (ataque >= minimoParaConquistar)
        {
            int sobreviventes =
                ataque - defesa;

            origem.RemoverTropas(
                ataque
            );

            destino.DefinirDono(
                ordem.Jogador
            );

            destino.DefinirTropas(
                sobreviventes
            );

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

        // ================================================
        // ATAQUE REPELIDO
        // ================================================

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

    private bool OrdemAindaValida(
        OrdemAtaque ordem)
    {
        if (ordem == null)
            return false;

        if (ordem.Origem == null ||
            ordem.Destino == null)
            return false;

        if (ordem.Origem.dono !=
            ordem.Jogador)
            return false;

        // Não pode atacar território que,
        // durante a resolução, virou aliado.
        if (EquipesJogadores.SaoAliados(
                ordem.Jogador,
                ordem.Destino.dono))
            return false;

        if (!ordem.Origem.EhVizinho(
                ordem.Destino))
            return false;

        if (ordem.Tropas >
            ordem.Origem.Tropas - 1)
            return false;

        if (ordem.Tropas < 1)
            return false;

        return true;
    }

    // =====================================================
    // REGRA DE FORÇA
    // =====================================================

    public int CalcularAtaqueMinimo(
        int defesa)
    {
        defesa =
            Mathf.Max(
                1,
                defesa
            );

        // Até defesa 10:
        // basta vantagem de +1.
        //
        // 1 defesa -> 2 ataque
        // 2 defesa -> 3 ataque
        // ...
        // 10 defesa -> 11 ataque
        if (defesa <= 10)
        {
            return defesa + 1;
        }

        // Defesa 11+:
        // vantagem de 15%,
        // com mínimo de +2.
        int vantagemPercentual =
            Mathf.CeilToInt(
                defesa * 0.15f
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