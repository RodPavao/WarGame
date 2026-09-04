using System;
using System.Collections.Generic;
using UnityEngine;

public enum WDContentAvailability
{
    Enabled,
    ComingSoon,
    Disabled
}

public enum WDMatchFlowDestination
{
    Matchmaking,
    SubmodeSelection,
    TeamFormation
}

[Serializable]
public sealed class WDMapModeRule
{
    [SerializeField] private string mapId;
    [SerializeField, Min(0f)] private float votingWeight = 1f;

    public string MapId => mapId;
    public float VotingWeight => votingWeight;
}

[Serializable]
public sealed class WDMatchSubmodeDefinition
{
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [SerializeField, TextArea] private string description;
    [SerializeField] private WDContentAvailability availability;
    [SerializeField] private WDMatchFlowDestination destination;
    [SerializeField] private string cardRuleId;
    [SerializeField] private List<WDMapModeRule> eligibleMaps = new();

    public string Id => id;
    public string DisplayName => displayName;
    public string Description => description;
    public WDContentAvailability Availability => availability;
    public WDMatchFlowDestination Destination => destination;
    public string CardRuleId => cardRuleId;
    public IReadOnlyList<WDMapModeRule> EligibleMaps => eligibleMaps;
}

[Serializable]
public sealed class WDMatchModeDefinition
{
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [SerializeField, TextArea] private string description;
    [SerializeField] private WDContentAvailability availability;
    [SerializeField] private WDMatchFlowDestination destination;
    [SerializeField, Min(1)] private int groupSize = 1;
    [SerializeField, Min(0)] private int matchSize;
    [SerializeField] private bool botsAllowed = true;
    [SerializeField] private List<WDMapModeRule> eligibleMaps = new();
    [SerializeField] private List<WDMatchSubmodeDefinition> submodes = new();

    public string Id => id;
    public string DisplayName => displayName;
    public string Description => description;
    public WDContentAvailability Availability => availability;
    public WDMatchFlowDestination Destination => destination;
    public int GroupSize => groupSize;
    public int MatchSize => matchSize;
    public bool BotsAllowed => botsAllowed;
    public IReadOnlyList<WDMapModeRule> EligibleMaps => eligibleMaps;
    public IReadOnlyList<WDMatchSubmodeDefinition> Submodes => submodes;
}

public sealed class WDMatchmakingRequest
{
    // ============================================================
    // 01. CONTRATO IMUTÁVEL PARA INTEGRAÇÃO FUTURA
    // ============================================================

    public string ModeId { get; }
    public string SubmodeId { get; }
    public int GroupSize { get; }
    public int MatchSize { get; }
    public string TeamFormation { get; }
    public string CardRuleId { get; }
    public bool BotsAllowed { get; }
    public IReadOnlyList<WDMapModeRule> EligibleMaps { get; }

    public WDMatchmakingRequest(
        WDMatchModeDefinition mode, WDMatchSubmodeDefinition submode,
        string teamFormation)
    {
        ModeId = mode?.Id ?? string.Empty;
        SubmodeId = submode?.Id ?? string.Empty;
        GroupSize = mode?.GroupSize ?? 1;
        MatchSize = mode?.MatchSize ?? 2;
        TeamFormation = teamFormation ?? string.Empty;
        CardRuleId = submode?.CardRuleId ?? "standard";
        BotsAllowed = mode?.BotsAllowed ?? false;
        EligibleMaps = submode != null && submode.EligibleMaps.Count > 0
            ? submode.EligibleMaps
            : mode?.EligibleMaps ?? Array.Empty<WDMapModeRule>();
    }
}

[CreateAssetMenu(
    fileName = "WarDominionMatchFlowConfig",
    menuName = "War Dominion/UI/Match Flow Config")]
public sealed class WarDominionMatchFlowConfig : ScriptableObject
{
    // ============================================================
    // 02. CATÁLOGO ADMINISTRÁVEL DE MODOS E SUBMODOS
    // ============================================================

    [SerializeField] private List<WDMatchModeDefinition> modes = new();

    public IReadOnlyList<WDMatchModeDefinition> Modes => modes;
}
