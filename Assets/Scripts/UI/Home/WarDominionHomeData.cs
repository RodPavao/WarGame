using UnityEngine;

[CreateAssetMenu(fileName = "WarDominionHomeMockData", menuName = "War Dominion/UI/Home Mock Data")]
public sealed class WarDominionHomeData : ScriptableObject
{
    // ============================================================
    // 01. IDENTIDADE MOCK CONFIGURÁVEL
    // ============================================================

    [Header("Identidade")]
    [SerializeField] private string nickname = "PLAYER";
    [SerializeField] private string status = "Online";
    [SerializeField] private Color playerColor = new Color(0.12f, 0.75f, 0.82f, 1f);
    [SerializeField, Min(0)] private int notifications = 3;

    [Header("Competitivo")]
    [SerializeField] private string league = "Liga 1";
    [SerializeField, Min(0)] private int trophies;

    [Header("Eventos provisórios")]
    [SerializeField] private string nextEvent = "Operação Atlântico";
    [SerializeField] private string registrationState = "Inscrições abertas";
    [SerializeField] private string liveEvent = "Confronto Relâmpago";
    [SerializeField] private string countdown = "02D 14H";

    public string Nickname => nickname;
    public string Status => status;
    public Color PlayerColor => playerColor;
    public int Notifications => notifications;
    public string League => league;
    public int Trophies => trophies;
    public string NextEvent => nextEvent;
    public string RegistrationState => registrationState;
    public string LiveEvent => liveEvent;
    public string Countdown => countdown;
}
