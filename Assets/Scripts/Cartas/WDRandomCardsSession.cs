using System;
using System.Collections.Generic;
using System.Linq;

public sealed class WDRandomCardsRoundResult
{
    public int Round { get; }
    public IReadOnlyList<string> DrawnCards { get; }
    public IReadOnlyDictionary<string, bool> DeliveredByPlayer { get; }

    public WDRandomCardsRoundResult(
        int round,
        IReadOnlyList<string> drawnCards,
        IReadOnlyDictionary<string, bool> deliveredByPlayer)
    {
        Round = round;
        DrawnCards = drawnCards;
        DeliveredByPlayer = deliveredByPlayer;
    }
}

public sealed class WDRandomCardsSession
{
    // ============================================================
    // 01. ESTADO DA SEQUÊNCIA COMUM E MÃOS INDIVIDUAIS
    // ============================================================

    public const int HandCapacity = 8;
    public const int InitialCardCount = 4;

    private readonly Random random;
    private readonly List<WDCardPoolEntry> cards;
    private readonly Dictionary<string, string[]> hands = new(StringComparer.Ordinal);
    private readonly HashSet<string> drawnCardIds = new(StringComparer.Ordinal);
    private readonly Dictionary<int, WDRandomCardsRoundResult> processedRounds = new();

    public WDRandomCardsSession(
        int seed,
        IReadOnlyList<WDCardPoolEntry> configuredCards,
        IEnumerable<string> playerIds)
    {
        random = new Random(seed);
        cards = configuredCards?.Where(IsValid).ToList() ?? new List<WDCardPoolEntry>();
        foreach (string playerId in playerIds ?? Array.Empty<string>())
            if (!string.IsNullOrWhiteSpace(playerId) && !hands.ContainsKey(playerId))
                hands.Add(playerId, new string[HandCapacity]);
    }

    public IReadOnlyList<string> GetHand(string playerId)
    {
        if (!hands.TryGetValue(playerId ?? string.Empty, out string[] hand))
            return Array.Empty<string>();
        return Array.AsReadOnly((string[])hand.Clone());
    }

    // ============================================================
    // 02. ENTREGA IDEMPOTENTE POR ROUND
    // ============================================================

    public WDRandomCardsRoundResult ProcessRound(int round)
    {
        round = Math.Max(1, round);
        if (processedRounds.TryGetValue(round, out WDRandomCardsRoundResult previous))
            return previous;

        List<string> drawn = round == 1
            ? DrawUnique(cards.Where(card => card.Basic), InitialCardCount)
            : DrawUnique(cards.Where(card => card.RandomCardsEligible), 1);
        var delivered = new Dictionary<string, bool>(StringComparer.Ordinal);

        foreach (KeyValuePair<string, string[]> player in hands)
        {
            bool receivedAll = true;
            foreach (string cardId in drawn)
                if (!TryDeliver(player.Value, cardId))
                    receivedAll = false;
            delivered[player.Key] = receivedAll && drawn.Count > 0;
        }

        var result = new WDRandomCardsRoundResult(round, drawn.AsReadOnly(), delivered);
        processedRounds.Add(round, result);
        return result;
    }

    public bool Consume(string playerId, int slotIndex)
    {
        if (!hands.TryGetValue(playerId ?? string.Empty, out string[] hand) ||
            slotIndex < 0 || slotIndex >= hand.Length || string.IsNullOrEmpty(hand[slotIndex]))
            return false;

        hand[slotIndex] = string.Empty;
        return true;
    }

    // ============================================================
    // 03. SORTEIO SEM REPETIÇÃO E CAPACIDADE INDIVIDUAL
    // ============================================================

    private List<string> DrawUnique(IEnumerable<WDCardPoolEntry> source, int count)
    {
        List<string> pool = source
            .Select(card => card.CardId)
            .Where(id => !drawnCardIds.Contains(id))
            .Distinct(StringComparer.Ordinal)
            .ToList();
        var result = new List<string>(Math.Min(count, pool.Count));
        while (result.Count < count && pool.Count > 0)
        {
            int index = random.Next(pool.Count);
            string cardId = pool[index];
            pool.RemoveAt(index);
            drawnCardIds.Add(cardId);
            result.Add(cardId);
        }
        return result;
    }

    private static bool TryDeliver(string[] hand, string cardId)
    {
        for (int i = 0; i < hand.Length; i++)
        {
            if (!string.IsNullOrEmpty(hand[i]))
                continue;
            hand[i] = cardId;
            return true;
        }
        return false;
    }

    private static bool IsValid(WDCardPoolEntry card) =>
        card != null && !string.IsNullOrWhiteSpace(card.CardId);
}
