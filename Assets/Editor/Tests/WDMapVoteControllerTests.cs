using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

public sealed class WDMapVoteControllerTests
{
    // ============================================================
    // 01. CANDIDATOS E ELEGIBILIDADE CONFIGURADA
    // ============================================================

    [Test]
    public void SelectCandidates_ReturnsTwoWithoutDuplicates()
    {
        var vote = new WDMapVoteController(10);
        IReadOnlyList<WDMapVoteCandidate> candidates = vote.SelectCandidates(new[]
        {
            Rule("a", 1f), Rule("a", 4f), Rule("b", 1f),
            Rule("c", 1f), Rule("d", 1f)
        });

        Assert.That(candidates, Has.Count.EqualTo(2));
        Assert.That(candidates.Select(item => item.MapId).Distinct(), Has.Count.EqualTo(2));
    }

    [Test]
    public void SelectCandidates_UsesOnlyRulesProvidedAsEligible()
    {
        var vote = new WDMapVoteController(20);
        IReadOnlyList<WDMapVoteCandidate> candidates = vote.SelectCandidates(new[]
        {
            Rule("eligible_a", 1f), Rule("eligible_b", 1f)
        });

        Assert.That(candidates.Select(item => item.MapId),
            Is.SubsetOf(new[] { "eligible_a", "eligible_b" }));
    }

    [Test]
    public void SelectCandidates_IgnoresInvalidWeights()
    {
        var vote = new WDMapVoteController(30);
        IReadOnlyList<WDMapVoteCandidate> candidates = vote.SelectCandidates(new[]
        {
            Rule("zero", 0f), Rule("negative", -2f), Rule("valid", 2f)
        });

        Assert.That(candidates.Select(item => item.MapId), Is.EqualTo(new[] { "valid" }));
    }

    [Test]
    public void SelectCandidates_WithOnlyOneEligibleMap_DoesNotThrow()
    {
        var vote = new WDMapVoteController(40);
        IReadOnlyList<WDMapVoteCandidate> candidates = null;

        Assert.DoesNotThrow(() => candidates = vote.SelectCandidates(new[]
        {
            Rule("a", 1f)
        }));
        Assert.That(candidates, Has.Count.EqualTo(1));
    }

    // ============================================================
    // 02. VOTO ÚNICO, ABSTENÇÃO E RESULTADO
    // ============================================================

    [Test]
    public void SubmitVote_ReplacesPreviousVoteFromSamePlayer()
    {
        WDMapVoteController vote = CreateTwoCandidateVote();

        Assert.That(vote.SubmitVote("player", "a"), Is.True);
        Assert.That(vote.SubmitVote("player", "b"), Is.True);

        Assert.That(vote.GetVote("player"), Is.EqualTo("b"));
        Assert.That(vote.VotesByPlayer, Has.Count.EqualTo(1));
    }

    [Test]
    public void Abstain_RemovesVoteWithoutCreatingAutomaticVote()
    {
        WDMapVoteController vote = CreateTwoCandidateVote();
        vote.SubmitVote("player", "a");
        vote.Abstain("player");

        WDMapVoteResult result = vote.Resolve();

        Assert.That(vote.VotesByPlayer, Is.Empty);
        Assert.That(result.VoteCounts.Values, Is.All.EqualTo(0));
    }

    [Test]
    public void Resolve_SelectsMapWithMostVotes()
    {
        WDMapVoteController vote = CreateTwoCandidateVote();
        vote.SubmitVote("p1", "a");
        vote.SubmitVote("p2", "a");
        vote.SubmitVote("p3", "b");

        Assert.That(vote.Resolve().WinningMapId, Is.EqualTo("a"));
    }

    [Test]
    public void Resolve_TieWinnerBelongsOnlyToTiedMaps()
    {
        WDMapVoteController vote = CreateTwoCandidateVote();
        vote.SubmitVote("p1", "a");
        vote.SubmitVote("p2", "b");

        WDMapVoteResult result = vote.Resolve();

        Assert.That(result.TiedMapIds, Is.EquivalentTo(new[] { "a", "b" }));
        Assert.That(result.WinningMapId == "a" || result.WinningMapId == "b", Is.True);
    }

    private static WDMapVoteController CreateTwoCandidateVote()
    {
        var vote = new WDMapVoteController(50);
        vote.SelectCandidates(new[] { Rule("a", 1f), Rule("b", 1f) });
        return vote;
    }

    private static WDMapModeRule Rule(string mapId, float weight) => new(mapId, weight);

    // ============================================================
    // 03. MATCHMAKING LOCAL E REGRA FIXA DO FFA
    // ============================================================

    [Test]
    public void PreMatch_CancelIsAllowedOnlyWhileSearching()
    {
        var flow = new WDPreMatchFlowController();
        flow.BeginSearch();

        Assert.That(flow.CanCancel, Is.True);
        Assert.That(flow.Cancel(), Is.True);
        Assert.That(flow.State, Is.EqualTo(WDPreMatchState.Cancelled));
    }

    [Test]
    public void PreMatch_OpponentFoundBlocksCancellation()
    {
        var flow = new WDPreMatchFlowController();
        flow.BeginSearch();
        flow.Tick(2.5f);

        Assert.That(flow.MarkOpponentFound(), Is.True);
        Assert.That(flow.CanCancel, Is.False);
        Assert.That(flow.Cancel(), Is.False);
        Assert.That(flow.WaitSeconds, Is.EqualTo(2.5f));
    }

    [Test]
    public void Ffa_UsesClassicAsFixedMapWithoutVoting()
    {
        WarDominionMatchFlowConfig config =
            UnityEngine.Resources.Load<WarDominionMatchFlowConfig>(
                "UI/Home/WarDominionMatchFlowConfig");
        WDMatchModeDefinition ffa = config.Modes.Single(mode => mode.Id == "ffa");

        Assert.That(ffa.MapSelectionPolicy, Is.EqualTo(WDMapSelectionPolicy.Fixed));
        Assert.That(ffa.FixedMapId, Is.EqualTo("classic"));
    }
}
