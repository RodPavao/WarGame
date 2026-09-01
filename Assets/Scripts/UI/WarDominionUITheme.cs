using TMPro;
using UnityEngine;

[CreateAssetMenu(
    fileName = "WarDominionUITheme",
    menuName = "War Dominion/UI/Theme")]
public sealed class WarDominionUITheme : ScriptableObject
{
    // ============================================================
    // 01. TIPOGRAFIA CENTRALIZADA
    // ============================================================

    [Header("Tipografia")]
    [SerializeField] private TMP_FontAsset fonteDisplay;
    [SerializeField] private TMP_FontAsset fonteInterface;
    [SerializeField, Min(8f)] private float tamanhoTitulo = 26f;
    [SerializeField, Min(8f)] private float tamanhoEstado = 20f;
    [SerializeField, Min(8f)] private float tamanhoCronometro = 26f;

    // ============================================================
    // 02. PALETA E SUPERFÍCIES
    // ============================================================

    [Header("Paleta")]
    [SerializeField] private Color superficiePrincipal =
        new Color(0.035f, 0.055f, 0.075f, 0.94f);
    [SerializeField] private Color superficieSecundaria =
        new Color(0.075f, 0.105f, 0.13f, 0.82f);
    [SerializeField] private Color textoPrincipal =
        new Color(0.92f, 0.95f, 0.96f, 1f);
    [SerializeField] private Color textoSecundario =
        new Color(0.64f, 0.72f, 0.76f, 1f);
    [SerializeField] private Color acento =
        new Color(0.17f, 0.72f, 0.78f, 1f);
    [SerializeField] private Color moldura =
        new Color(0.31f, 0.39f, 0.43f, 0.72f);

    // ============================================================
    // 03. DIMENSÕES E ESPAÇAMENTO
    // ============================================================

    [Header("Layout")]
    [SerializeField, Min(48f)] private float alturaTopBar = 72f;
    [SerializeField, Min(640f)] private float larguraTopBar = 1320f;
    [SerializeField, Min(0f)] private float margemSuperior = 16f;
    [SerializeField, Min(0f)] private float margemLateral = 24f;

    public TMP_FontAsset FonteDisplay => fonteDisplay;
    public TMP_FontAsset FonteInterface => fonteInterface != null ? fonteInterface : fonteDisplay;
    public float TamanhoTitulo => tamanhoTitulo;
    public float TamanhoEstado => tamanhoEstado;
    public float TamanhoCronometro => tamanhoCronometro;
    public Color SuperficiePrincipal => superficiePrincipal;
    public Color SuperficieSecundaria => superficieSecundaria;
    public Color TextoPrincipal => textoPrincipal;
    public Color TextoSecundario => textoSecundario;
    public Color Acento => acento;
    public Color Moldura => moldura;
    public float AlturaTopBar => alturaTopBar;
    public float LarguraTopBar => larguraTopBar;
    public float MargemSuperior => margemSuperior;
    public float MargemLateral => margemLateral;
}
