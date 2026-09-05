using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

public sealed class WDInitialTerritorySelectionSessionTests
{
    // ============================================================
    // 01. DUAS ETAPAS E UMA ATRIBUIÇÃO POR JOGADOR
    // ============================================================

    [Test]
    public void SessionHasExactlyTwoStages()
    {
        var session = Session(1);
        Resolve(session, "t1", "t2");
        Assert.That(session.IsComplete, Is.False);
        Resolve(session, "t3", "t4");
        Assert.That(session.IsComplete, Is.True);
    }

    [Test]
    public void EachStageAssignsOneTerritoryPerPlayer()
    {
        WDInitialTerritoryStageResult result = Resolve(Session(2), "t1", "t2");
        Assert.That(result.Assignments, Has.Count.EqualTo(2));
        Assert.That(result.Assignments.Select(item => item.PlayerId),
            Is.EquivalentTo(new[] { "p1", "p2" }));
    }

    [Test]
    public void FinalResultHasTwoTerritoriesPerPlayer()
    {
        var session = Session(3);
        Resolve(session, "t1", "t2");
        Resolve(session, "t3", "t4");
        Assert.That(session.GetAssignments("p1"), Has.Count.EqualTo(2));
        Assert.That(session.GetAssignments("p2"), Has.Count.EqualTo(2));
    }

    [Test]
    public void SecondStageRejectsAlreadyAssignedTerritory()
    {
        var session = Session(4);
        Resolve(session, "t1", "t2");
        Assert.That(session.SubmitIntent("p1", "t1"), Is.False);
    }

    // ============================================================
    // 02. CONFLITO, PRIORIDADE E FALLBACK
    // ============================================================

    [Test]
    public void SameChoiceGoesToFirstPlayerInRoundOrder()
    {
        WDInitialTerritoryStageResult result = Resolve(Session(5), "t1", "t1");
        Assert.That(result.HadConflict, Is.True);
        Assert.That(result.Assignments[0].PlayerId, Is.EqualTo("p1"));
        Assert.That(result.Assignments[0].TerritoryId, Is.EqualTo("t1"));
    }

    [Test]
    public void ConflictLoserReceivesFallback()
    {
        WDInitialTerritoryStageResult result = Resolve(Session(6), "t1", "t1");
        Assert.That(result.Assignments[1].PlayerId, Is.EqualTo("p2"));
        Assert.That(result.Assignments[1].WasFallback, Is.True);
        Assert.That(result.Assignments[1].TerritoryId, Is.Not.EqualTo("t1"));
    }

    [Test]
    public void FallbackNeverUsesOccupiedTerritory()
    {
        var session = Session(7);
        Resolve(session, "t1", "t1");
        Assert.That(session.IsAvailable("t1"), Is.False);
        Assert.That(session.GetAssignments("p2"), Has.None.EqualTo("t1"));
    }

    [Test]
    public void SameSeedProducesSameFallback()
    {
        string first = Resolve(Session(77), "t1", "t1").Assignments[1].TerritoryId;
        string second = Resolve(Session(77), "t1", "t1").Assignments[1].TerritoryId;
        Assert.That(first, Is.EqualTo(second));
    }

    [Test]
    public void PriorityComesFromProvidedRoundOrder()
    {
        var session = new WDInitialTerritorySelectionSession(
            8, new[] { "p2", "p1" }, Territories());
        session.SubmitIntent("p1", "t1");
        session.SubmitIntent("p2", "t1");
        Assert.That(session.ResolveStage().Assignments[0].PlayerId, Is.EqualTo("p2"));
    }

    // ============================================================
    // 03. DISPONIBILIDADE E NÃO DUPLICAÇÃO
    // ============================================================

    [Test]
    public void AssignedTerritoriesLeaveAllOthersAvailable()
    {
        var session = Session(9);
        Resolve(session, "t1", "t2");
        Assert.That(session.IsAvailable("t1"), Is.False);
        Assert.That(session.IsAvailable("t2"), Is.False);
        Assert.That(session.IsAvailable("t3"), Is.True);
    }

    [Test]
    public void StageCannotResolveBeforeBothIntentionsExist()
    {
        var session = Session(10);
        session.SubmitIntent("p1", "t1");
        Assert.That(session.ResolveStage(), Is.Null);
        Assert.That(session.Stage, Is.EqualTo(1));
    }

    [Test]
    public void CompletedSessionRejectsNewIntentions()
    {
        var session = Session(11);
        Resolve(session, "t1", "t2");
        Resolve(session, "t3", "t4");
        Assert.That(session.SubmitIntent("p1", "t5"), Is.False);
    }

    private static WDInitialTerritorySelectionSession Session(int seed) =>
        new(seed, new[] { "p1", "p2" }, Territories());

    private static IEnumerable<string> Territories() =>
        Enumerable.Range(1, 12).Select(index => $"t{index}");

    private static WDInitialTerritoryStageResult Resolve(
        WDInitialTerritorySelectionSession session, string first, string second)
    {
        Assert.That(session.SubmitIntent("p1", first), Is.True);
        Assert.That(session.SubmitIntent("p2", second), Is.True);
        return session.ResolveStage();
    }
}
