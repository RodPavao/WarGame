using UnityEngine;

public class HUDPreparacao : MonoBehaviour
{
    private GameManager gm;

    // =====================================================
    // CORES
    // =====================================================

    private readonly Color corPainel =
        new Color(0.025f, 0.025f, 0.032f, 0.99f);

    private readonly Color corVinho =
        new Color(0.49f, 0.12f, 0.22f, 1f);

    private readonly Color corVinhoClaro =
        new Color(0.76f, 0.26f, 0.38f, 1f);

    private readonly Color corDourado =
        new Color(0.96f, 0.72f, 0.18f, 1f);

    private readonly Color corTexto =
        new Color(0.94f, 0.94f, 0.96f, 1f);

    private readonly Color corSecundaria =
        new Color(0.68f, 0.69f, 0.73f, 1f);

    // =====================================================
    // ESTILOS
    // =====================================================

    private GUIStyle painel;

    private GUIStyle tituloJogo;
    private GUIStyle tituloRound;
    private GUIStyle tituloSecao;

    private GUIStyle texto;
    private GUIStyle textoCentral;

    private GUIStyle numeroGrande;
    private GUIStyle numeroDourado;

    private GUIStyle botao;
    private GUIStyle botaoPrincipal;
    private GUIStyle botaoCancelar;

    private GUIStyle linhaAcao;

    // =====================================================
    // RESPONSIVIDADE
    // =====================================================

    private float escalaHUD = 1f;

    private float larguraPainelEsquerdo;
    private float larguraPainelDireito;

    // =====================================================
    // INICIALIZAÇÃO
    // =====================================================

    private void Awake()
    {
        gm =
            GetComponent<GameManager>();

        if (gm == null)
        {
            gm =
                FindAnyObjectByType<GameManager>();
        }

        if (gm == null)
        {
            Debug.LogError(
                "HUD | GameManager não encontrado."
            );
        }
    }

    // =====================================================
    // GUI
    // =====================================================

    private void OnGUI()
    {
        if (gm == null)
            return;

        AtualizarDimensoesResponsivas();

        GarantirEstilos();

        AtualizarEstilosResponsivos();

        DesenharFundoLaterais();

        DesenharPainelEsquerdo();

        DesenharPainelDireito();
    }

    // =====================================================
    // RESPONSIVIDADE
    // =====================================================

    private void AtualizarDimensoesResponsivas()
    {
        float proporcaoEsquerda = 0.10f;
        float proporcaoDireita = 0.10f;

        if (LayoutPartida.instance != null)
        {
            proporcaoEsquerda =
                LayoutPartida.instance.larguraPainelEsquerdo;

            proporcaoDireita =
                LayoutPartida.instance.larguraPainelDireito;
        }

        larguraPainelEsquerdo =
            Screen.width * proporcaoEsquerda;

        larguraPainelDireito =
            Screen.width * proporcaoDireita;

        float menorPainel =
            Mathf.Min(
                larguraPainelEsquerdo,
                larguraPainelDireito
            );

        escalaHUD =
            Mathf.Clamp(
                menorPainel / 170f,
                0.58f,
                1.10f
            );
    }

    // =====================================================
    // ESTILOS
    // =====================================================

    private void GarantirEstilos()
    {
        if (painel != null)
            return;

        painel =
            new GUIStyle(GUI.skin.box);

        painel.normal.background =
            CriarTexturaCor(corPainel);

        tituloJogo =
            new GUIStyle(GUI.skin.label);

        tituloJogo.fontStyle =
            FontStyle.Bold;

        tituloJogo.alignment =
            TextAnchor.MiddleCenter;

        tituloJogo.normal.textColor =
            corVinhoClaro;

        tituloRound =
            new GUIStyle(GUI.skin.label);

        tituloRound.fontStyle =
            FontStyle.Bold;

        tituloRound.alignment =
            TextAnchor.MiddleCenter;

        tituloRound.normal.textColor =
            corDourado;

        tituloSecao =
            new GUIStyle(GUI.skin.label);

        tituloSecao.fontStyle =
            FontStyle.Bold;

        tituloSecao.alignment =
            TextAnchor.MiddleCenter;

        tituloSecao.normal.textColor =
            corVinhoClaro;

        texto =
            new GUIStyle(GUI.skin.label);

        texto.wordWrap = true;

        texto.normal.textColor =
            corTexto;

        textoCentral =
            new GUIStyle(texto);

        textoCentral.alignment =
            TextAnchor.MiddleCenter;

        numeroGrande =
            new GUIStyle(GUI.skin.label);

        numeroGrande.fontStyle =
            FontStyle.Bold;

        numeroGrande.alignment =
            TextAnchor.MiddleCenter;

        numeroGrande.normal.textColor =
            corTexto;

        numeroDourado =
            new GUIStyle(numeroGrande);

        numeroDourado.normal.textColor =
            corDourado;

        botao =
            new GUIStyle(GUI.skin.button);

        botao.alignment =
            TextAnchor.MiddleCenter;

        botao.normal.textColor =
            corTexto;

        botao.hover.textColor =
            Color.white;

        botao.active.textColor =
            Color.white;

        botaoPrincipal =
            new GUIStyle(botao);

        botaoPrincipal.fontStyle =
            FontStyle.Bold;

        botaoPrincipal.normal.background =
            CriarTexturaCor(corVinho);

        botaoPrincipal.hover.background =
            CriarTexturaCor(corVinhoClaro);

        botaoPrincipal.active.background =
            CriarTexturaCor(
                new Color(
                    0.38f,
                    0.08f,
                    0.15f,
                    1f
                )
            );

        botaoCancelar =
            new GUIStyle(botao);

        botaoCancelar.normal.textColor =
            corSecundaria;

        botaoCancelar.hover.textColor =
            corDourado;

        linhaAcao =
            new GUIStyle(GUI.skin.label);

        linhaAcao.wordWrap = true;

        linhaAcao.normal.textColor =
            corTexto;
    }

    private void AtualizarEstilosResponsivos()
    {
        tituloJogo.fontSize =
            TamanhoFonte(20);

        tituloRound.fontSize =
            TamanhoFonte(15);

        tituloSecao.fontSize =
            TamanhoFonte(11);

        texto.fontSize =
            TamanhoFonte(10);

        textoCentral.fontSize =
            texto.fontSize;

        numeroGrande.fontSize =
            TamanhoFonte(21);

        numeroDourado.fontSize =
            numeroGrande.fontSize;

        botao.fontSize =
            TamanhoFonte(10);

        botaoPrincipal.fontSize =
            botao.fontSize;

        botaoCancelar.fontSize =
            botao.fontSize;

        linhaAcao.fontSize =
            TamanhoFonte(10);

        int margem =
            Mathf.Max(
                3,
                Mathf.RoundToInt(
                    7f * escalaHUD
                )
            );

        painel.padding =
            new RectOffset(
                margem,
                margem,
                margem,
                margem
            );
    }

    private int TamanhoFonte(
        int tamanhoBase)
    {
        return Mathf.Max(
            7,
            Mathf.RoundToInt(
                tamanhoBase *
                escalaHUD
            )
        );
    }

    private float Altura(
        float alturaBase)
    {
        return Mathf.Max(
            18f,
            alturaBase *
            escalaHUD
        );
    }

    private float Espaco(
        float espacoBase)
    {
        return Mathf.Max(
            2f,
            espacoBase *
            escalaHUD
        );
    }

    // =====================================================
    // FUNDO
    // =====================================================

    private void DesenharFundoLaterais()
    {
        GUI.Box(
            new Rect(
                0f,
                0f,
                larguraPainelEsquerdo,
                Screen.height
            ),
            GUIContent.none,
            painel
        );

        GUI.Box(
            new Rect(
                Screen.width -
                larguraPainelDireito,
                0f,
                larguraPainelDireito,
                Screen.height
            ),
            GUIContent.none,
            painel
        );
    }

    // =====================================================
    // PAINEL ESQUERDO
    // =====================================================

    private void DesenharPainelEsquerdo()
    {
        float margem =
            Mathf.Max(
                3f,
                5f * escalaHUD
            );

        GUILayout.BeginArea(
            new Rect(
                margem,
                margem,
                larguraPainelEsquerdo -
                margem * 2f,
                Screen.height -
                margem * 2f
            )
        );

        GUILayout.Label(
            "WarGame",
            tituloJogo,
            GUILayout.Height(Altura(28))
        );

        GUILayout.Label(
            "ROUND " + gm.RodadaAtual,
            tituloRound,
            GUILayout.Height(Altura(22))
        );

        GUILayout.Space(Espaco(6));

        DesenharSeparador(corVinho);

        GUILayout.Space(Espaco(7));

        GUILayout.Label(
            "FASE",
            tituloSecao,
            GUILayout.Height(Altura(16))
        );

        GUILayout.Label(
            NomeFase(),
            textoCentral,
            GUILayout.Height(Altura(18))
        );

        GUILayout.Space(Espaco(6));

        // =================================================
        // TEMPO
        // =================================================

        if (gm.faseAtual !=
            GameManager.FaseTurno.Resolucao)
        {
            GUILayout.Label(
                "TEMPO",
                tituloSecao,
                GUILayout.Height(Altura(16))
            );

            int segundos =
                Mathf.Max(
                    0,
                    Mathf.CeilToInt(
                        gm.TempoPreparacaoRestante
                    )
                );

            int minutos =
                segundos / 60;

            int resto =
                segundos % 60;

            GUILayout.Label(
                minutos.ToString("00") +
                ":" +
                resto.ToString("00"),
                numeroDourado,
                GUILayout.Height(Altura(31))
            );
        }

        // =================================================
        // REFORÇOS
        // =================================================

        if (gm.faseAtual ==
            GameManager.FaseTurno.Reforco)
        {
            GUILayout.Space(Espaco(5));

            DesenharSeparador(
                new Color(
                    1f,
                    1f,
                    1f,
                    0.10f
                )
            );

            GUILayout.Space(Espaco(6));

            GUILayout.Label(
                "REFORÇOS",
                tituloSecao,
                GUILayout.Height(Altura(16))
            );

            GUILayout.Label(
                gm.reforcosDisponiveis.ToString(),
                numeroDourado,
                GUILayout.Height(Altura(30))
            );

            GUILayout.Label(
                "Clique: +1\n" +
                "Segure 1 s: todos",
                textoCentral,
                GUILayout.Height(Altura(34))
            );

            // =============================================
            // TERRITÓRIO REFORÇADO SELECIONADO
            // =============================================

            if (gm.TerritorioReforcadoSelecionado != null &&
                gm.ReforcosNoTerritorioSelecionado > 0)
            {
                GUILayout.Space(Espaco(7));

                GUILayout.Label(
                    gm.TerritorioReforcadoSelecionado.name,
                    tituloSecao,
                    GUILayout.Height(Altura(18))
                );

                GUILayout.Label(
                    "+" +
                    gm.ReforcosNoTerritorioSelecionado +
                    " nesta rodada",
                    textoCentral,
                    GUILayout.Height(Altura(20))
                );

                GUILayout.Space(Espaco(4));

                if (GUILayout.Button(
                        "DESFAZER REFORÇO",
                        botaoCancelar,
                        GUILayout.Height(
                            Altura(31)
                        )))
                {
                    gm.DesfazerReforcosTerritorioSelecionado();
                }
            }

            // =============================================
            // CONFIRMAR DISTRIBUIÇÃO
            // =============================================

            if (gm.PodeConfirmarReforcos)
            {
                GUILayout.Space(Espaco(8));

                if (GUILayout.Button(
                        "CONFIRMAR REFORÇOS",
                        botaoPrincipal,
                        GUILayout.Height(
                            Altura(35)
                        )))
                {
                    gm.ConfirmarReforcos();
                }
            }
        }

        // =================================================
        // ATAQUE
        // =================================================

        if (gm.faseAtual ==
            GameManager.FaseTurno.Ataque)
        {
            GUILayout.Space(Espaco(5));

            DesenharSeparador(
                new Color(
                    1f,
                    1f,
                    1f,
                    0.10f
                )
            );

            GUILayout.Space(Espaco(6));

            GUILayout.Label(
                "TROPAS",
                tituloSecao,
                GUILayout.Height(Altura(17))
            );

            GUILayout.BeginHorizontal();

            if (GUILayout.Button(
                    "-",
                    botao,
                    GUILayout.Width(
                        Mathf.Max(
                            24,
                            28 * escalaHUD
                        )
                    ),
                    GUILayout.Height(
                        Altura(29)
                    )))
            {
                gm.DiminuirQuantidadeAtaque();
            }

            GUILayout.Label(
                gm.quantidadeAtaqueSelecionada.ToString(),
                numeroDourado,
                GUILayout.Height(Altura(29))
            );

            if (GUILayout.Button(
                    "+",
                    botao,
                    GUILayout.Width(
                        Mathf.Max(
                            24,
                            28 * escalaHUD
                        )
                    ),
                    GUILayout.Height(
                        Altura(29)
                    )))
            {
                gm.AumentarQuantidadeAtaque();
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(Espaco(5));

            string origem =
                gm.territorioSelecionado != null
                    ? gm.territorioSelecionado.name
                    : "—";

            string destino =
                gm.territorioDestinoSelecionado != null
                    ? gm.territorioDestinoSelecionado.name
                    : "—";

            GUILayout.Label(
                "ORIGEM",
                tituloSecao,
                GUILayout.Height(Altura(16))
            );

            GUILayout.Label(
                origem,
                textoCentral,
                GUILayout.Height(Altura(20))
            );

            GUILayout.Label(
                "DESTINO",
                tituloSecao,
                GUILayout.Height(Altura(16))
            );

            GUILayout.Label(
                destino,
                textoCentral,
                GUILayout.Height(Altura(20))
            );

            GUILayout.Space(Espaco(5));

            if (GUILayout.Button(
                    "CONFIRMAR",
                    botaoPrincipal,
                    GUILayout.Height(Altura(33))
                ))
            {
                gm.ConfirmarAtaquePreparado();
            }

            GUILayout.Space(Espaco(4));

            if (GUILayout.Button(
                    "CANCELAR",
                    botaoCancelar,
                    GUILayout.Height(Altura(29))
                ))
            {
                gm.CancelarSelecaoAtual();
            }
        }

        GUILayout.EndArea();
    }

    // =====================================================
    // PAINEL DIREITO
    // =====================================================

    private void DesenharPainelDireito()
    {
        float margem =
            Mathf.Max(
                3f,
                5f * escalaHUD
            );

        float inicioX =
            Screen.width -
            larguraPainelDireito;

        GUILayout.BeginArea(
            new Rect(
                inicioX + margem,
                margem,
                larguraPainelDireito -
                margem * 2f,
                Screen.height -
                margem * 2f
            )
        );

        GUILayout.Label(
            "AÇÕES",
            tituloJogo,
            GUILayout.Height(Altura(28))
        );

        GUILayout.Label(
            "PREPARADAS",
            tituloRound,
            GUILayout.Height(Altura(19))
        );

        GUILayout.Label(
            gm.QuantidadeOrdensPreparadas +
            "/3",
            numeroDourado,
            GUILayout.Height(Altura(24))
        );

        GUILayout.Space(Espaco(5));

        DesenharSeparador(corVinho);

        GUILayout.Space(Espaco(7));

        var ordens =
            gm.OrdensPreparadas;

        if (ordens == null ||
            ordens.Count == 0)
        {
            GUILayout.Label(
                "Nenhuma ação preparada.",
                textoCentral,
                GUILayout.Height(Altura(34))
            );
        }
        else
        {
            for (int i = 0;
                 i < ordens.Count;
                 i++)
            {
                OrdemAtaque ordem =
                    ordens[i];

                GUILayout.Label(
                    (i + 1) +
                    ". " +
                    ordem.Origem.name +
                    "\n→ " +
                    ordem.Destino.name +
                    "\n" +
                    ordem.Tropas +
                    " tropa(s)",
                    linhaAcao,
                    GUILayout.Height(Altura(48))
                );

                GUILayout.Space(Espaco(5));
            }
        }

        GUILayout.FlexibleSpace();

        if (gm.faseAtual ==
            GameManager.FaseTurno.Ataque)
        {
            GUI.enabled = false;

            GUILayout.Button(
                "TRANSFERIR",
                botao,
                GUILayout.Height(Altura(28))
            );

            GUI.enabled = true;

            GUILayout.Space(Espaco(4));

            if (GUILayout.Button(
                    "DESFAZER",
                    botaoCancelar,
                    GUILayout.Height(Altura(30))
                ))
            {
                gm.CancelarUltimaOrdem();
            }

            GUILayout.Space(Espaco(4));

            if (GUILayout.Button(
                    "ENVIAR",
                    botaoPrincipal,
                    GUILayout.Height(Altura(36))
                ))
            {
                gm.EnviarAcoes();
            }
        }

        GUILayout.EndArea();
    }

    // =====================================================
    // UTILIDADES
    // =====================================================

    private void DesenharSeparador(
        Color cor)
    {
        Rect linha =
            GUILayoutUtility.GetRect(
                1f,
                Mathf.Max(
                    1f,
                    2f * escalaHUD
                ),
                GUILayout.ExpandWidth(true)
            );

        GUI.DrawTexture(
            linha,
            Texture2D.whiteTexture,
            ScaleMode.StretchToFill,
            true,
            0f,
            cor,
            0f,
            0f
        );
    }

    private string NomeFase()
    {
        switch (gm.faseAtual)
        {
            case GameManager.FaseTurno.Reforco:
                return "REFORÇOS";

            case GameManager.FaseTurno.Ataque:
                return "PREPARAÇÃO";

            case GameManager.FaseTurno.Resolucao:
                return "RESOLUÇÃO";
        }

        return gm.faseAtual.ToString();
    }

    private Texture2D CriarTexturaCor(
        Color cor)
    {
        Texture2D textura =
            new Texture2D(
                1,
                1
            );

        textura.SetPixel(
            0,
            0,
            cor
        );

        textura.Apply();

        return textura;
    }
}