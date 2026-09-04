using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum WDMatchParticipantKind
{
    Local,
    Remote,
    Bot
}

[Serializable]
public sealed class WDMatchParticipant
{
    public string PlayerId { get; }
    public string Nickname { get; }
    public int TeamIndex { get; }
    public int SlotIndex { get; }
    public Color ProfileColor { get; }
    public Color MatchColor { get; internal set; }
    public string SkinId { get; }
    public string DeckId { get; }
    public WDMatchParticipantKind Kind { get; }

    public WDMatchParticipant(
        string playerId, string nickname, int teamIndex, int slotIndex,
        Color profileColor, string skinId, string deckId,
        WDMatchParticipantKind kind)
    {
        PlayerId = playerId ?? string.Empty;
        Nickname = nickname ?? string.Empty;
        TeamIndex = teamIndex;
        SlotIndex = slotIndex;
        ProfileColor = profileColor;
        MatchColor = profileColor;
        SkinId = skinId ?? string.Empty;
        DeckId = deckId ?? string.Empty;
        Kind = kind;
    }
}

[Serializable]
public sealed class WDMatchSetup
{
    // ============================================================
    // 01. CONTRATO IMUTÁVEL DA PARTIDA PRONTA
    // ============================================================

    public string ModeId { get; }
    public string SubmodeId { get; }
    public string MapId { get; }
    public string ScenePath { get; }
    public string CardRuleId { get; }
    public int RoundLimit { get; }
    public bool SuddenDeathEnabled { get; }
    public int DeterministicSeed { get; }
    public IReadOnlyList<WDMatchParticipant> Participants { get; }

    public WDMatchSetup(
        string modeId, string submodeId, string mapId, string scenePath,
        string cardRuleId, int roundLimit, bool suddenDeathEnabled,
        int deterministicSeed, IReadOnlyList<WDMatchParticipant> participants)
    {
        ModeId = modeId ?? string.Empty;
        SubmodeId = submodeId ?? string.Empty;
        MapId = mapId ?? string.Empty;
        ScenePath = scenePath ?? string.Empty;
        CardRuleId = cardRuleId ?? string.Empty;
        RoundLimit = roundLimit;
        SuddenDeathEnabled = suddenDeathEnabled;
        DeterministicSeed = deterministicSeed;
        Participants = participants ?? Array.Empty<WDMatchParticipant>();
    }
}

public static class WDMatchColorResolver
{
    // ============================================================
    // 02. CORES EFETIVAS SEM ALTERAR O PERFIL
    // ============================================================

    private const float MinimumColorDistance = 0.18f;

    public static void Resolve(
        IList<WDMatchParticipant> participants,
        IReadOnlyList<Color> preferredAlternatives = null)
    {
        var used = new List<Color>();
        for (int i = 0; i < participants.Count; i++)
        {
            WDMatchParticipant participant = participants[i];
            Color selected = participant.ProfileColor;
            if (IsDistinct(selected, used))
            {
                participant.MatchColor = Opaque(selected);
                used.Add(participant.MatchColor);
                continue;
            }

            selected = FindAlternative(participant.ProfileColor, used, preferredAlternatives, i);
            participant.MatchColor = Opaque(selected);
            used.Add(participant.MatchColor);
        }
    }

    private static Color FindAlternative(
        Color preferred, IReadOnlyList<Color> used,
        IReadOnlyList<Color> alternatives, int participantIndex)
    {
        if (alternatives != null)
            foreach (Color candidate in alternatives)
                if (IsDistinct(candidate, used))
                    return candidate;

        Color.RGBToHSV(preferred, out float hue, out float saturation, out float value);
        saturation = Mathf.Max(0.65f, saturation);
        value = Mathf.Max(0.72f, value);
        for (int step = 1; step <= 12; step++)
        {
            Color candidate = Color.HSVToRGB(
                Mathf.Repeat(hue + (participantIndex + step) * 0.381966f, 1f),
                saturation, value);
            if (IsDistinct(candidate, used))
                return candidate;
        }
        return Color.HSVToRGB(Mathf.Repeat(hue + 0.5f, 1f), 1f, 1f);
    }

    private static bool IsDistinct(Color candidate, IReadOnlyList<Color> used)
    {
        for (int i = 0; i < used.Count; i++)
        {
            Vector3 difference = new Vector3(
                candidate.r - used[i].r,
                candidate.g - used[i].g,
                candidate.b - used[i].b);
            if (difference.sqrMagnitude < MinimumColorDistance * MinimumColorDistance)
                return false;
        }
        return true;
    }

    private static Color Opaque(Color color)
    {
        color.a = 1f;
        return color;
    }
}

public static class WDMatchSetupContext
{
    // ============================================================
    // 03. PONTE TRANSITÓRIA ENTRE HOME E CENA DA PARTIDA
    // ============================================================

    public static WDMatchSetup Current { get; private set; }
    public static bool HasSetup => Current != null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset() => Current = null;

    public static void Store(WDMatchSetup setup)
    {
        Current = setup ?? throw new ArgumentNullException(nameof(setup));
    }

    public static void Clear() => Current = null;

    public static bool TryGetParticipant(
        TerritorioClique.Dono owner,
        out WDMatchParticipant participant)
    {
        participant = null;
        if (Current == null || owner == TerritorioClique.Dono.Neutro)
            return false;

        int slotIndex = (int)owner - 1;
        foreach (WDMatchParticipant candidate in Current.Participants)
        {
            if (candidate.SlotIndex != slotIndex)
                continue;

            participant = candidate;
            return true;
        }

        return false;
    }

    public static void ExitToHome()
    {
        Clear();
        const string homePath = "Assets/Scenes/Home.unity";
#if UNITY_EDITOR
        UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(
            homePath,
            new LoadSceneParameters(LoadSceneMode.Single));
#else
        SceneManager.LoadScene("Assets/Scenes/Home");
#endif
    }

    public static bool TryLoadCurrentScene()
    {
        if (Current == null || string.IsNullOrWhiteSpace(Current.ScenePath))
            return false;

#if UNITY_EDITOR
        PlayerPrefs.SetString(MapaAtivo.ChaveMapaTesteEditor, Current.MapId);
#endif
#if UNITY_EDITOR
        UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(
            Current.ScenePath,
            new LoadSceneParameters(LoadSceneMode.Single));
#else
        string sceneReference = Current.ScenePath.EndsWith(
            ".unity", StringComparison.OrdinalIgnoreCase)
            ? Current.ScenePath.Substring(0, Current.ScenePath.Length - ".unity".Length)
            : Current.ScenePath;
        SceneManager.LoadScene(sceneReference);
#endif
        return true;
    }
}

public static class WDMatchSetupFactory
{
    // ============================================================
    // 04. COMPOSIÇÃO DATA-DRIVEN DO SETUP E PARTICIPANTES MOCK
    // ============================================================

    public static WDMatchSetup Create(
        WDMatchmakingRequest request, DefinicaoMapa map,
        WarDominionHomeData profile, int selectedDeckIndex,
        int deterministicSeed = 0)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (map == null) throw new ArgumentNullException(nameof(map));
        if (profile == null) throw new ArgumentNullException(nameof(profile));

        int deckIndex = request.MapSelectionPolicy == WDMapSelectionPolicy.Fixed
            ? profile.DefaultDeckIndex
            : selectedDeckIndex;
        WDDeckProfile deck = profile.GetDeck(deckIndex);
        int participantCount = Mathf.Max(1, request.MatchSize);
        var participants = new List<WDMatchParticipant>(participantCount);
        participants.Add(new WDMatchParticipant(
            "local_player", profile.Nickname, 0, 0, profile.PlayerColor,
            profile.SkinId, deck.Id, WDMatchParticipantKind.Local));

        for (int i = 1; i < participantCount; i++)
        {
            int team = request.GroupSize > 1 ? i / request.GroupSize : i;
            participants.Add(new WDMatchParticipant(
                $"remote_{i}", string.Empty, team, i, profile.PlayerColor,
                string.Empty, string.Empty, WDMatchParticipantKind.Remote));
        }
        WDMatchColorResolver.Resolve(participants);

        return new WDMatchSetup(
            request.ModeId, request.SubmodeId, map.MapaId,
            GetMetadata(map, "scenePath"), request.CardRuleId,
            request.RoundLimit, request.SuddenDeathEnabled,
            deterministicSeed, participants);
    }

    private static string GetMetadata(DefinicaoMapa map, string key)
    {
        MetadadoMapa metadata = map.Metadados.FirstOrDefault(item =>
            item != null && string.Equals(item.chave, key, StringComparison.Ordinal));
        return metadata?.valor ?? string.Empty;
    }
}
