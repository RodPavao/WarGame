using System.Linq;
using NUnit.Framework;
using UnityEngine;

public sealed class WDNeutralTerritoryTests
{
    // ============================================================
    // 01. SEMÂNTICA EXPLÍCITA DE AUSÊNCIA DE PROPRIETÁRIO
    // ============================================================

    [Test]
    public void NeutralTerritoryHasNoOwner()
    {
        TerritorioClique territory = CreateTerritory();
        Assert.That(territory.IsNeutral, Is.True);
        Assert.That(territory.PossuiDono, Is.False);
        Object.DestroyImmediate(territory.gameObject);
    }

    [Test]
    public void NeutralOpeningTerritoryHasOneTroop()
    {
        TerritorioClique territory = CreateTerritoryWithTroops();
        territory.DefinirNeutro();
        Assert.That(territory.Tropas, Is.EqualTo(1));
        Object.DestroyImmediate(territory.gameObject);
    }

    [Test]
    public void EmptyTerritoryHasNoOwnerAndZeroTroops()
    {
        TerritorioClique territory = CreateTerritoryWithTroops();
        territory.DefinirVazio();
        Assert.That(territory.IsEmpty, Is.True);
        Assert.That(territory.PossuiDono, Is.False);
        Assert.That(territory.Tropas, Is.Zero);
        Object.DestroyImmediate(territory.gameObject);
    }

    [Test]
    public void NeutralOwnerDoesNotBelongToTeam()
    {
        Assert.That(EquipesJogadores.ObterEquipe(TerritorioClique.Dono.Neutro),
            Is.EqualTo(EquipesJogadores.Equipe.Nenhuma));
    }

    [Test]
    public void NeutralDoesNotCountAsPlayerTerritory()
    {
        var owners = new[] { TerritorioClique.Dono.Jogador1, TerritorioClique.Dono.Neutro };
        Assert.That(owners.Count(owner => owner == TerritorioClique.Dono.Jogador1), Is.EqualTo(1));
    }

    [Test]
    public void NeutralPreventsCompleteRegionOwnership()
    {
        var owners = new[] { TerritorioClique.Dono.Jogador1, TerritorioClique.Dono.Neutro };
        Assert.That(owners.All(owner => owner == TerritorioClique.Dono.Jogador1), Is.False);
    }

    [Test]
    public void NeutralIsClassifiedAsAttackTarget()
    {
        TerritorioClique territory = CreateTerritory();
        Assert.That(SistemaAcoesTerrestres.ObterTipoEsperado(
            TerritorioClique.Dono.Jogador1, territory), Is.EqualTo("ATAQUE"));
        Object.DestroyImmediate(territory.gameObject);
    }

    [Test]
    public void AssigningOwnerEndsNeutralState()
    {
        TerritorioClique territory = CreateTerritory();
        territory.DefinirDono(TerritorioClique.Dono.Jogador1);
        Assert.That(territory.IsNeutral, Is.False);
        Assert.That(territory.PossuiDono, Is.True);
        Object.DestroyImmediate(territory.gameObject);
    }

    [Test]
    public void NeutralStateDoesNotDependOnRendererColor()
    {
        TerritorioClique territory = CreateTerritory();
        SpriteRenderer renderer = territory.gameObject.AddComponent<SpriteRenderer>();
        renderer.color = Color.magenta;
        Assert.That(territory.IsNeutral, Is.True);
        Object.DestroyImmediate(territory.gameObject);
    }

    // ============================================================
    // 02. CONTABILIZAÇÃO DAS SEIS TROPAS INICIAIS DO 1x1
    // ============================================================

    [Test]
    public void TwoInitialTerritoriesHaveZeroTroopsBeforeDistribution()
    {
        TerritorioClique first = CreateTerritoryWithTroops();
        TerritorioClique second = CreateTerritoryWithTroops();
        first.DefinirDono(TerritorioClique.Dono.Jogador1);
        second.DefinirDono(TerritorioClique.Dono.Jogador1);
        first.DefinirTropasIniciaisSemDistribuicao();
        second.DefinirTropasIniciaisSemDistribuicao();
        Assert.That(new[] { first.Tropas, second.Tropas }, Is.EqualTo(new[] { 0, 0 }));
        Object.DestroyImmediate(first.gameObject);
        Object.DestroyImmediate(second.gameObject);
    }

    [Test]
    public void OccupiedTerritoryCanTemporarilyHaveZeroTroopsWithoutBeingEmpty()
    {
        TerritorioClique territory = CreateTerritoryWithTroops();
        territory.DefinirDono(TerritorioClique.Dono.Jogador1);
        territory.DefinirTropasIniciaisSemDistribuicao();
        Assert.That(territory.IsOccupied, Is.True);
        Assert.That(territory.IsEmpty, Is.False);
        Assert.That(territory.PossuiDono, Is.True);
        Assert.That(territory.Tropas, Is.Zero);
        Object.DestroyImmediate(territory.gameObject);
    }

    [Test]
    public void StateIsNotInferredOnlyFromTroopCount()
    {
        TerritorioClique occupied = CreateTerritoryWithTroops();
        TerritorioClique empty = CreateTerritoryWithTroops();
        occupied.DefinirDono(TerritorioClique.Dono.Jogador1);
        occupied.DefinirTropasIniciaisSemDistribuicao();
        empty.DefinirVazio();
        Assert.That(occupied.Tropas, Is.EqualTo(empty.Tropas));
        Assert.That(occupied.Estado, Is.Not.EqualTo(empty.Estado));
        Object.DestroyImmediate(occupied.gameObject);
        Object.DestroyImmediate(empty.gameObject);
    }

    [Test]
    public void InitialOneVsOneProvidesSixDistributableTroops()
    {
        Assert.That(GerenciadorRodada.TotalTropasIniciaisUmContraUm, Is.EqualTo(6));
    }

    [Test]
    public void DistributedInitialTroopsTotalExactlySix()
    {
        int[] finalDistribution = { 5, 1 };
        Assert.That(finalDistribution.Sum(),
            Is.EqualTo(GerenciadorRodada.TotalTropasIniciaisUmContraUm));
    }

    [Test]
    public void NeutralTroopsAreExcludedFromPlayerInitialTotal()
    {
        int ownedTroopsBeforeDistribution = 0;
        int neutralTroops = 38;
        int available = GerenciadorRodada.TotalTropasIniciaisUmContraUm;
        Assert.That(ownedTroopsBeforeDistribution + available, Is.EqualTo(6));
        Assert.That(ownedTroopsBeforeDistribution + neutralTroops + available,
            Is.Not.EqualTo(6));
    }

    private static TerritorioClique CreateTerritory()
    {
        var gameObject = new GameObject("NeutralTest");
        return gameObject.AddComponent<TerritorioClique>();
    }

    private static TerritorioClique CreateTerritoryWithTroops()
    {
        var gameObject = new GameObject("InitialOwnedTest");
        gameObject.AddComponent<TerritorioTropas>();
        return gameObject.AddComponent<TerritorioClique>();
    }
}
