using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class WarDominionMatchHUD : MonoBehaviour
{
    // ============================================================
    // 01. REFERÊNCIAS E ESTADO PUBLICADO
    // ============================================================

    private WarDominionUITheme theme;
    private IMatchUICommands commands;
    private MatchUIState state;
    private TextMeshProUGUI roundText;
    private TextMeshProUGUI phaseText;
    private TextMeshProUGUI timerText;
    private TextMeshProUGUI reinforcementText;
    private TextMeshProUGUI selectionText;
    private TextMeshProUGUI feedbackText;
    private RectTransform playersContent;
    private RectTransform actionsContent;
    private WDUIPremiumButton actionModeButton;
    private WDUIPremiumButton transferModeButton;
    private WDUIPremiumButton reinforceButton;
    private WDUIPremiumButton confirmButton;
    private WDUIPremiumButton undoButton;
    private WDUIPremiumButton sendButton;

    // ============================================================
    // 02. COMPOSIÇÃO PERIFÉRICA RESPONSIVA
    // ============================================================

    public void Build(WarDominionUITheme newTheme, IMatchUICommands newCommands)
    {
        theme = newTheme;
        commands = newCommands;

        RectTransform identity = CreatePanel(
            "MatchIdentity", new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(24f, -24f), new Vector2(292f, 104f));
        roundText = CreateText(identity, "Round", theme.TypeTitle, theme.TextoPrincipal,
            new Vector2(16f, -12f), new Vector2(250f, 34f));
        phaseText = CreateText(identity, "Phase", theme.TypeSection, theme.Acento,
            new Vector2(16f, -51f), new Vector2(250f, 28f));

        RectTransform timer = CreatePanel(
            "CriticalTimer", Vector2.one, Vector2.one,
            new Vector2(-24f, -24f), new Vector2(210f, 104f));
        CreateText(timer, "TimerLabel", theme.TypeMicro, theme.TextoSecundario,
            new Vector2(14f, -10f), new Vector2(175f, 22f)).text = "TEMPO RESTANTE";
        timerText = CreateText(timer, "Timer", theme.TypeNumber, theme.TextoPrincipal,
            new Vector2(14f, -36f), new Vector2(175f, 48f));

        RectTransform players = CreatePanel(
            "Players", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(24f, 52f), new Vector2(252f, 330f));
        CreateText(players, "Title", theme.TypeSection, theme.TextoSecundario,
            new Vector2(14f, -12f), new Vector2(220f, 24f)).text = "COMANDO / EQUIPES";
        playersContent = WDUIFactory.Rect("Content", players);
        SetStretch(playersContent, 12f, 12f, 44f, 12f);
        VerticalLayoutGroup playerLayout = playersContent.gameObject.AddComponent<VerticalLayoutGroup>();
        playerLayout.spacing = theme.SpacingSm;
        playerLayout.childControlHeight = true;
        playerLayout.childForceExpandHeight = false;

        RectTransform bottom = CreatePanel(
            "PreparationActions", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 22f), new Vector2(940f, 188f));
        reinforcementText = CreateText(bottom, "Reinforcements", theme.TypeSection,
            theme.TextoPrincipal, new Vector2(18f, -12f), new Vector2(230f, 28f));
        selectionText = CreateText(bottom, "Selection", theme.TypeBody,
            theme.TextoSecundario, new Vector2(250f, -13f), new Vector2(660f, 28f));
        BuildButtons(bottom);
        actionsContent = WDUIFactory.Rect("PreparedActions", bottom);
        actionsContent.anchorMin = new Vector2(0f, 0f);
        actionsContent.anchorMax = new Vector2(1f, 0f);
        actionsContent.pivot = new Vector2(0.5f, 0f);
        actionsContent.anchoredPosition = new Vector2(0f, 10f);
        actionsContent.sizeDelta = new Vector2(-30f, 43f);
        HorizontalLayoutGroup actionLayout = actionsContent.gameObject.AddComponent<HorizontalLayoutGroup>();
        actionLayout.spacing = theme.SpacingSm;
        actionLayout.childControlWidth = true;
        actionLayout.childForceExpandWidth = true;

        RectTransform feedback = CreatePanel(
            "ContextFeedback", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -24f), new Vector2(520f, 48f));
        feedbackText = CreateText(feedback, "Message", theme.TypeBody,
            theme.TextoPrincipal, new Vector2(12f, -7f), new Vector2(496f, 32f));
        feedbackText.alignment = TextAlignmentOptions.Center;
    }

    private void BuildButtons(RectTransform parent)
    {
        RectTransform row = WDUIFactory.Rect("CommandRow", parent);
        row.anchorMin = new Vector2(0f, 1f);
        row.anchorMax = new Vector2(1f, 1f);
        row.pivot = new Vector2(0.5f, 1f);
        row.anchoredPosition = new Vector2(0f, -48f);
        row.sizeDelta = new Vector2(-30f, theme.ButtonHeight);
        HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = theme.SpacingSm;
        layout.childControlWidth = true;
        layout.childForceExpandWidth = true;

        actionModeButton = CreateButton(row, "AÇÃO", () => commands?.SelecionarModoAcao());
        transferModeButton = CreateButton(row, "TRANSFERIR", () => commands?.SelecionarModoTransferencia());
        reinforceButton = CreateButton(row, "+ REFORÇO", AddReinforcement);
        confirmButton = CreateButton(row, "CONFIRMAR", () => commands?.ConfirmarAcao());
        undoButton = CreateButton(row, "DESFAZER", () => commands?.DesfazerUltimaAcao());
        sendButton = CreateButton(row, "ENVIAR", ToggleSend);
    }

    // ============================================================
    // 03. APRESENTAÇÃO DO MATCHUISTATE
    // ============================================================

    public void Present(MatchUIState newState)
    {
        if (newState == null || theme == null) return;
        state = newState;
        roundText.text = state.EmMorteSubita
            ? $"MORTE SÚBITA {state.RoundMorteSubita}"
            : $"ROUND {state.Round:00}";
        phaseText.text = state.Fase.ToString().ToUpperInvariant();
        int seconds = Mathf.CeilToInt(state.TempoRestante);
        timerText.text = $"{seconds / 60:00}:{seconds % 60:00}";
        timerText.color = seconds <= 10 ? theme.Danger :
            seconds <= 30 ? theme.Warning : theme.TextoPrincipal;
        reinforcementText.text = $"REFORÇOS  {state.ReforcosDisponiveis:00}";
        selectionText.text = BuildSelectionText(state);
        PresentFeedback(state.FeedbackAtual);
        PresentButtons();
        RebuildPlayers();
        RebuildActions();
    }

    private void PresentButtons()
    {
        actionModeButton.SetSelected(state.EmModoAcaoTerrestre);
        transferModeButton.SetSelected(state.EmModoTransferencia);
        actionModeButton.SetInteractable(state.PodeEditarPreparacao);
        transferModeButton.SetInteractable(state.PodeEditarPreparacao && !state.TransferenciaUsada);
        reinforceButton.SetInteractable(
            state.PodeAdicionarReforco && !string.IsNullOrEmpty(state.OrigemSelecionadaId));
        confirmButton.SetInteractable(state.PodeConfirmarAcao);
        undoButton.SetInteractable(state.PodeDesfazerAcao);
        sendButton.SetInteractable(state.AcoesEnviadas
            ? state.PodeCancelarEnvio : state.PodeEnviarPreparacao);
        sendButton.SetLabel(state.AcoesEnviadas ? "CANCELAR ENVIO" : "ENVIAR AÇÕES");
        sendButton.SetSelected(state.AcoesEnviadas);
    }

    // ============================================================
    // 04. JOGADORES, EQUIPES E AÇÕES PREPARADAS
    // ============================================================

    private void RebuildPlayers()
    {
        ClearChildren(playersContent);
        foreach (MatchUIPlayerState player in state.Jogadores)
        {
            RectTransform row = WDUIFactory.Rect("Player_" + player.Jogador, playersContent);
            LayoutElement element = row.gameObject.AddComponent<LayoutElement>();
            element.preferredHeight = 48f;
            Image background = row.gameObject.AddComponent<Image>();
            Color rowColor = theme.BackgroundPrimary;
            rowColor.a = player.EhJogadorLocal ? 0.96f : 0.72f;
            background.color = rowColor;
            background.raycastTarget = false;
            Image marker = WDUIFactory.Rect("Color", row).gameObject.AddComponent<Image>();
            marker.color = player.Cor;
            marker.raycastTarget = false;
            RectTransform markerRect = marker.rectTransform;
            markerRect.anchorMin = new Vector2(0f, 0f);
            markerRect.anchorMax = new Vector2(0f, 1f);
            markerRect.sizeDelta = new Vector2(5f, 0f);
            markerRect.anchoredPosition = new Vector2(2.5f, 0f);
            TextMeshProUGUI name = CreateText(row, "Name", theme.TypeBody,
                theme.TextoPrincipal, new Vector2(14f, -5f), new Vector2(125f, 22f));
            name.text = player.Nome + (player.EhJogadorLocal ? "  • VOCÊ" : string.Empty);
            TextMeshProUGUI meta = CreateText(row, "Meta", theme.TypeMicro,
                theme.TextoSecundario, new Vector2(14f, -25f), new Vector2(190f, 18f));
            meta.text = $"{player.Equipe}  |  {player.TerritoriosControlados} TERR.";
            if (state.EmModoTransferencia &&
                !string.IsNullOrEmpty(state.TerritorioTransferenciaId) &&
                player.Jogador != state.JogadorLocal)
            {
                Button target = row.gameObject.AddComponent<Button>();
                target.transition = Selectable.Transition.None;
                background.raycastTarget = true;
                TerritorioClique.Dono recipient = player.Jogador;
                target.onClick.AddListener(() => commands?.SelecionarDestinatarioTransferencia(recipient));
            }
        }
    }

    private void RebuildActions()
    {
        ClearChildren(actionsContent);
        foreach (MatchUIActionState action in state.Acoes)
        {
            RectTransform item = WDUIFactory.Rect("Action_" + action.Posicao, actionsContent);
            Image image = item.gameObject.AddComponent<Image>();
            image.color = theme.BackgroundPrimary;
            TextMeshProUGUI text = CreateText(item, "Summary", theme.TypeMicro,
                theme.TextoPrincipal, new Vector2(8f, -4f), new Vector2(250f, 32f));
            text.text = $"{action.TipoEsperado}  {action.OrigemId} → {action.DestinoId}  x{action.Quantidade}";
            Button button = item.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            int position = action.Posicao;
            button.onClick.AddListener(() => commands?.RemoverAcao(position));
        }
        if (state.Acoes.Count == 0)
        {
            TextMeshProUGUI empty = WDUIFactory.Text(
                "NoActions", actionsContent, theme, theme.TypeMicro,
                theme.TextoSecundario, TextAlignmentOptions.Center);
            empty.text = "NENHUMA AÇÃO PREPARADA";
        }
    }

    // ============================================================
    // 05. COMANDOS E FEEDBACK SEM REGRA DE GAMEPLAY
    // ============================================================

    private void AddReinforcement()
    {
        if (state != null && !string.IsNullOrEmpty(state.OrigemSelecionadaId))
            commands?.AdicionarReforco(state.OrigemSelecionadaId, 1);
    }

    private void ToggleSend()
    {
        if (state == null) return;
        if (state.AcoesEnviadas) commands?.CancelarEnvio();
        else commands?.EnviarAcoes();
    }

    private void PresentFeedback(MatchUIFeedbackState feedback)
    {
        feedbackText.text = feedback.Mensagem;
        feedbackText.color = feedback.Tipo switch
        {
            MatchUIFeedbackKind.Sucesso => theme.Success,
            MatchUIFeedbackKind.Aviso => theme.Warning,
            MatchUIFeedbackKind.Erro => theme.Danger,
            _ => theme.TextoPrincipal
        };
        feedbackText.transform.parent.gameObject.SetActive(
            !string.IsNullOrWhiteSpace(feedback.Mensagem));
    }

    private static string BuildSelectionText(MatchUIState current)
    {
        if (!string.IsNullOrEmpty(current.DestinoSelecionadoId))
            return $"{current.TipoAcaoEsperado}: {current.OrigemSelecionadaId} → " +
                $"{current.DestinoSelecionadoId}  x{current.QuantidadeAcaoSelecionada}";
        if (!string.IsNullOrEmpty(current.OrigemSelecionadaId))
            return $"ORIGEM: {current.OrigemSelecionadaId}";
        if (!string.IsNullOrEmpty(current.TerritorioTransferenciaId))
            return $"TRANSFERIR CONTROLE: {current.TerritorioTransferenciaId}";
        return "SELECIONE UM TERRITÓRIO NO MAPA";
    }

    // ============================================================
    // 06. UTILITÁRIOS DE LAYOUT
    // ============================================================

    private RectTransform CreatePanel(
        string name, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
    {
        RectTransform rect = WDUIFactory.Rect(name, transform);
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.gameObject.AddComponent<WDUIPanel>().Build(theme, true);
        return rect;
    }

    private TextMeshProUGUI CreateText(
        Transform parent, string name, float size, Color color,
        Vector2 position, Vector2 dimensions)
    {
        TextMeshProUGUI text = WDUIFactory.Text(name, parent, theme, size, color);
        RectTransform rect = text.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = dimensions;
        return text;
    }

    private WDUIPremiumButton CreateButton(Transform parent, string label, UnityEngine.Events.UnityAction action)
    {
        RectTransform rect = WDUIFactory.Rect(label, parent);
        WDUIPremiumButton button = rect.gameObject.AddComponent<WDUIPremiumButton>();
        button.Build(theme, label, action);
        return button;
    }

    private static void SetStretch(RectTransform rect, float left, float right, float top, float bottom)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }

    private static void ClearChildren(Transform parent)
    {
        for (int index = parent.childCount - 1; index >= 0; index--)
            Destroy(parent.GetChild(index).gameObject);
    }
}
