using System;
using System.Collections.Generic;
using System.Linq;

public enum WDPreMatchState
{
    Idle,
    Searching,
    OpponentFound,
    Cancelled
}

public sealed class WDPreMatchFlowController
{
    // ============================================================
    // 01. ESTADO LOCAL E RELÓGIO CRESCENTE DA FILA
    // ============================================================

    public WDPreMatchState State { get; private set; } = WDPreMatchState.Idle;
    public float WaitSeconds { get; private set; }
    public bool CanCancel => State == WDPreMatchState.Searching;

    public void BeginSearch()
    {
        State = WDPreMatchState.Searching;
        WaitSeconds = 0f;
    }

    public void Tick(float unscaledDeltaTime)
    {
        if (State == WDPreMatchState.Searching && unscaledDeltaTime > 0f)
            WaitSeconds += unscaledDeltaTime;
    }

    // ============================================================
    // 02. ENCONTRO E CANCELAMENTO MUTUAMENTE EXCLUSIVOS
    // ============================================================

    public bool MarkOpponentFound()
    {
        if (State != WDPreMatchState.Searching)
            return false;

        State = WDPreMatchState.OpponentFound;
        return true;
    }

    public bool Cancel()
    {
        if (!CanCancel)
            return false;

        State = WDPreMatchState.Cancelled;
        return true;
    }
}

public sealed class WDMapVoteCandidate
{
    public string MapId { get; }
    public float Weight { get; }

    public WDMapVoteCandidate(string mapId, float weight)
    {
        MapId = mapId;
        Weight = weight;
    }
}

public sealed class WDMapVoteResult
{
    public string WinningMapId { get; }
    public IReadOnlyDictionary<string, int> VoteCounts { get; }
    public IReadOnlyList<string> TiedMapIds { get; }

    public WDMapVoteResult(
        string winningMapId, IReadOnlyDictionary<string, int> voteCounts,
        IReadOnlyList<string> tiedMapIds)
    {
        WinningMapId = winningMapId;
        VoteCounts = voteCounts;
        TiedMapIds = tiedMapIds;
    }
}

public sealed class WDMapVoteController
{
    // ============================================================
    // 01. ESTADO PURO DA VOTAÇÃO
    // ============================================================

    public const int CandidateLimit = 2;
    private readonly Random random;
    private readonly List<WDMapVoteCandidate> candidates = new();
    private readonly Dictionary<string, string> votesByPlayer = new(StringComparer.Ordinal);

    public IReadOnlyList<WDMapVoteCandidate> Candidates => candidates;
    public IReadOnlyDictionary<string, string> VotesByPlayer => votesByPlayer;

    public WDMapVoteController(int? seed = null)
    {
        random = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    // ============================================================
    // 02. SELEÇÃO PONDERADA SEM REPOSIÇÃO
    // ============================================================

    public IReadOnlyList<WDMapVoteCandidate> SelectCandidates(
        IEnumerable<WDMapModeRule> eligibleMaps)
    {
        candidates.Clear();
        votesByPlayer.Clear();

        if (eligibleMaps == null)
            return candidates;

        List<WDMapVoteCandidate> pool = eligibleMaps
            .Where(rule => rule != null &&
                !string.IsNullOrWhiteSpace(rule.MapId) &&
                rule.VotingWeight > 0f)
            .GroupBy(rule => rule.MapId, StringComparer.Ordinal)
            .Select(group => new WDMapVoteCandidate(
                group.Key, group.Max(rule => rule.VotingWeight)))
            .ToList();

        while (pool.Count > 0 && candidates.Count < CandidateLimit)
        {
            int selectedIndex = SelectWeightedIndex(pool);
            candidates.Add(pool[selectedIndex]);
            pool.RemoveAt(selectedIndex);
        }

        return candidates;
    }

    private int SelectWeightedIndex(IReadOnlyList<WDMapVoteCandidate> pool)
    {
        double total = 0d;
        for (int i = 0; i < pool.Count; i++)
            total += pool[i].Weight;

        double target = random.NextDouble() * total;
        for (int i = 0; i < pool.Count; i++)
        {
            target -= pool[i].Weight;
            if (target < 0d)
                return i;
        }

        return pool.Count - 1;
    }

    // ============================================================
    // 03. VOTO ÚNICO, SUBSTITUIÇÃO E ABSTENÇÃO
    // ============================================================

    public bool SubmitVote(string playerId, string mapId)
    {
        if (string.IsNullOrWhiteSpace(playerId) ||
            candidates.All(candidate => candidate.MapId != mapId))
            return false;

        votesByPlayer[playerId] = mapId;
        return true;
    }

    public void Abstain(string playerId)
    {
        if (!string.IsNullOrWhiteSpace(playerId))
            votesByPlayer.Remove(playerId);
    }

    public string GetVote(string playerId)
    {
        return !string.IsNullOrWhiteSpace(playerId) &&
            votesByPlayer.TryGetValue(playerId, out string mapId)
            ? mapId
            : string.Empty;
    }

    // ============================================================
    // 04. CONTAGEM E DESEMPATE RESTRITO AOS EMPATADOS
    // ============================================================

    public WDMapVoteResult Resolve()
    {
        var counts = candidates.ToDictionary(
            candidate => candidate.MapId, _ => 0, StringComparer.Ordinal);

        foreach (string mapId in votesByPlayer.Values)
        {
            if (counts.ContainsKey(mapId))
                counts[mapId]++;
        }

        if (counts.Count == 0)
            return new WDMapVoteResult(string.Empty, counts, Array.Empty<string>());

        int highest = counts.Values.Max();
        string[] tied = counts
            .Where(entry => entry.Value == highest)
            .Select(entry => entry.Key)
            .ToArray();
        string winner = tied[random.Next(tied.Length)];
        return new WDMapVoteResult(winner, counts, tied);
    }
}
