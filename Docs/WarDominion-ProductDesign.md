# War Dominion — Documento Mestre de Produto e Design

## 1. Propósito e autoridade do documento

Este documento é a fonte persistente de verdade para as decisões de produto, direção visual, arquitetura de interface e comportamento de apresentação do War Dominion.

Ele existe para:

- registrar decisões aprovadas sem depender do histórico de conversas;
- distinguir claramente o que já é definitivo do que ainda é provisório, pendente ou futuro;
- orientar implementação, validação e revisão visual;
- impedir que uma prova funcional seja confundida com uma solução visual aprovada;
- evitar regras contraditórias ou obsoletas durante a evolução do projeto.

Regra central:

> **Funcional no Play != visual aprovado.**

Uma funcionalidade operar corretamente no Play Mode valida seu fluxo técnico e sua integração. Isso não aprova automaticamente composição, identidade visual, tipografia, animação, materiais, ícones ou acabamento.

## 2. Sistema de status

Toda decisão relevante deste documento deve ser interpretada por um dos quatro estados abaixo.

### [DEFINITIVO]

Decisão de produto, regra funcional ou direção estrutural aprovada. Só deve ser alterada por nova decisão explícita e registrada.

### [PROVISÓRIO]

Implementação utilizável para validar arquitetura, fluxo ou integração, mas ainda não aprovada como solução final — especialmente no aspecto visual.

### [PENDENTE]

Tema reconhecido que exige definição adicional antes de implementação definitiva. Não deve ser preenchido por suposição.

### [FUTURO]

Evolução planejada fora do escopo imediato. Serve para preservar intenção sem antecipar implementação.

## 3. Metodologia de implementação da interface

### [DEFINITIVO] Passo 1 — implementação global

O primeiro passe estabelece o produto de ponta a ponta:

- telas;
- layout geral;
- navegação;
- fluxos;
- funções;
- integrações;
- estados e comandos necessários.

O objetivo é obter uma experiência funcional, coerente e verificável, sem prolongar prematuramente o polimento de cada elemento.

### [DEFINITIVO] Passo 2 — polimento premium

O segundo passe transforma a fundação funcional na apresentação final:

- arte definitiva;
- molduras e materiais;
- tipografia;
- iconografia;
- glow e iluminação;
- animações e transições;
- partículas;
- áudio de interface;
- responsividade;
- consistência global.

Nenhum elemento do primeiro passe deve ser considerado visualmente definitivo apenas porque está funcional.

## 4. Identidade visual do War Dominion

### [DEFINITIVO] Relação entre Home e HUD de partida

A Home e o HUD de partida pertencem à mesma identidade, mas têm responsabilidades visuais diferentes.

- A Home pode ser mais rica, expressiva e orientada à identidade do jogador.
- O HUD de partida deve ser mais contido, contextual e subordinado ao mapa.
- A Home não deve ser tratada como uma simples variação do HUD.
- O HUD não deve transportar permanentemente toda a densidade informacional da Home.

### [DEFINITIVO] Prioridade do mapa durante a partida

O mapa é o protagonista da tela de partida.

- A interface não deve cobrir áreas importantes do mapa sem necessidade.
- Informações principais devem ocupar bordas e zonas de baixa interferência.
- Informações secundárias devem surgir sob demanda em painéis contextuais, expansões ou overlays.
- Controles devem ser reconhecíveis, compactos e preferencialmente iconográficos.

### [DEFINITIVO] Linguagem de componentes

Evitar aparência padrão do Unity, retângulos simples sem identidade e botões definidos apenas por texto. Componentes devem ser reconhecíveis por forma, iconografia, material, estado e comportamento.

### [DEFINITIVO] Glow como assinatura

O glow é uma assinatura visual central, mas deve ser controlado e significativo.

- Na Preparação, sua intensidade deve ser menor e associada a intenção, seleção e energia contida.
- Na Resolução, pode ganhar intensidade para comunicar execução, impacto e consequência.
- Glow constante e indiscriminado reduz hierarquia e não deve substituir composição ou legibilidade.

### [DEFINITIVO] Materiais e atmosfera permitidos

A linguagem material pode combinar:

- aço e ferro;
- metal desgastado;
- latão e bronze envelhecido;
- ferrugem controlada;
- madeira;
- verde militar e musgo;
- elementos industriais e militares históricos;
- referências vintage de Segunda Guerra Mundial;
- dieselpunk leve.

A combinação desejada é materialidade histórica/militar com iluminação, glow, animação e apresentação modernas.

### [DEFINITIVO] Referências externas

KARDS e WarTime são referências de direção, atmosfera e qualidade. Não autorizam copiar propriedade intelectual, reproduzir composições literalmente ou transformar o War Dominion em derivação visual de outra marca.

## 5. Biblioteca de assets visuais

### [DEFINITIVO] Regra de uso

Os packs externos são matéria-prima, não identidade pronta. Seus elementos podem ser combinados, recortados, recoloridos, convertidos, animados, receber glow, material ou servir de base para componentes próprios.

Nenhum pack externo deve, isoladamente, definir a identidade do War Dominion.

### [DEFINITIVO] MetalBaja

Fonte potencial para estruturas metálicas, chapas, bordas, recortes, desgaste e bases de molduras. Deve ser adaptado à linguagem do projeto e não aplicado como skin genérica em toda a interface.

### [DEFINITIVO] Strategic Warfare UI Starter Pack

Fonte de estudo e matéria-prima para organização militar/estratégica, painéis, controles e elementos de HUD. Não deve ser incorporado de forma literal nem substituir o design system próprio.

### [DEFINITIVO] HYPER

Fonte potencial para efeitos modernos de energia, brilho, transições, feedback e movimento. Seu uso deve ser moderado e integrado à materialidade histórica/militar.

### [DEFINITIVO] Game Icon Pack

Base possível para iconografia funcional. Ícones devem ser selecionados, normalizados e adaptados para manter espessura, escala, contraste e linguagem consistentes.

### [DEFINITIVO] Mechanized Magic

Fonte potencial para efeitos mecânicos, energia, partículas e acentos visuais. Deve apoiar eventos relevantes, não criar excesso de ruído ou deslocar a identidade para fantasia desconectada do produto.

## 6. Home

### [DEFINITIVO] Princípio geral

A Home deve ser mais rica que o HUD de partida e organizar a vida competitiva, social e de progressão do jogador. O botão **PLAY** é o protagonista da composição.

### [DEFINITIVO] Área esquerda

Deve reunir:

- identidade do jogador;
- avatar;
- nickname;
- skin/cor;
- status;
- seletor rápido de conta;
- notificações com badges;
- Missões;
- Ranking;
- Eventos/Torneios.

### [DEFINITIVO] Área central

Deve destacar:

- liga competitiva;
- troféus e progressão;
- PLAY;
- Cards;
- Clan.

### [DEFINITIVO] Área direita

Deve organizar:

- Profile com destaque;
- Friends abaixo de Profile;
- Chat e Settings em escala menor;
- Store e VIP com importância intermediária;
- Discord na área inferior.

### [DEFINITIVO] Perfil e personalização

Profile centraliza a personalização do jogador. Alterações de identidade, apresentação, avatar, skin ou cor devem ser encontradas de forma coerente nessa área, sem dispersar configurações equivalentes por múltiplas telas.

### [DEFINITIVO] Eventos

Eventos e torneios devem ter presença identificável e permitir descoberta de conteúdo competitivo, regras, disponibilidade e progressão associada quando essas definições existirem.

### [DEFINITIVO] Densidade de informação

Informações secundárias devem aparecer em popups, painéis contextuais, janelas ou overlays. A Home não deve permanecer permanentemente congestionada para provar que uma função existe.

### [DEFINITIVO] Composição e escala dos cards

Os cards da Home não devem ocupar ou preencher toda a superfície disponível da tela. A referência estrutural e comportamental é WarTime, sem reprodução literal de sua identidade visual.

A composição final deve favorecer:

- cards menores;
- elementos gráficos;
- espaçamento entre componentes;
- áreas de fundo visíveis;
- sensação de profundidade e composição, em vez de uma grade preenchida;
- tamanhos diferentes conforme a importância;
- informações secundárias sob demanda.

PLAY continua sendo o protagonista, mas não deve terminar como um enorme retângulo simples preenchendo o centro.

A hierarquia de escala desejada é:

- Perfil e Amigos compactos, porém importantes;
- Chat e Configurações pequenos;
- Loja e VIP médios;
- Missões e Ranking compactos;
- Eventos/Torneios com destaque variável e contextual;
- Cards e Clan menores e mais gráficos que na implementação provisória atual.

A Home da Passada 1 valida arquitetura e fluxo. Seu dimensionamento visual atual não é aprovação do tamanho definitivo dos cards.

### [PENDENTE] Definições da Home

Permanecem pendentes:

- economia e moedas;
- detalhes de VIP;
- modelo da Store;
- detalhes de monetização;
- catálogo de missões;
- limites e faixas das ligas;
- matchmaking.

## 7. Ligas e monetização

### [DEFINITIVO] Estrutura competitiva

- Existem 10 ligas antes da liga Veteran.
- Veteran sucede as dez ligas e admite progressão prática de troféus sem limite fechado definido.
- O sistema deve evitar que veteranos dominem iniciantes por pareamento inadequado.
- O produto não deve ser pay-to-win.

### [FUTURO] [PENDENTE] Modelos econômicos

Podem ser avaliados futuramente, sem decisão detalhada atual:

- VIP/premium;
- cosméticos;
- progressão justa;
- anúncios não intrusivos;
- outras formas de receita compatíveis com competição justa.

Esses modelos não devem conceder vantagem competitiva comprável.

## 8. HUD de partida

### [DEFINITIVO] Estrutura desejada

- O mapa mantém prioridade visual.
- Não existe TopBar permanente como solução final.
- Informações essenciais ficam nas bordas.
- Informações secundárias aparecem sob demanda.
- Controles devem ser compactos, gráficos e iconográficos.
- Jogadores e times podem expandir informações quando necessário.
- Regiões e bônus devem ser apresentados de forma contextual.
- O chat deve existir como painel, evitando ocupar permanentemente espaço central.

### [PROVISÓRIO] Implementação atual

Permanecem funcionais, reutilizáveis e ainda não aprovados visualmente como resultado final:

- `WarDominionMatchHUD`;
- `WDUIPremiumComponents`;
- `PreparedActionArrowView`;
- presenters visuais de resolução;
- aparência de molduras;
- aparência de botões;
- tipografia;
- contadores;
- setas;
- feedbacks visuais.

A infraestrutura funcional desses elementos pode ser preservada enquanto a apresentação é substituída ou refinada no passe premium.

## 9. Preparação e Resolução

### [DEFINITIVO] Separação semântica

- **Preparação** comunica intenção, escolha e planejamento.
- **Resolução** comunica execução, impacto e consequência.

As duas fases não devem compartilhar a mesma intensidade visual nem produzir ambiguidade sobre quando o resultado lógico já ocorreu.

### [DEFINITIVO] Preparação

Durante a Preparação, a interface deve permitir compreender e revisar ações antes do envio, incluindo seleção, distribuição, remoção, transferência e indicação visual das intenções preparadas conforme os contratos funcionais disponíveis.

### [DEFINITIVO] Ataque preparado

O diamante/pulso de feixe anteriormente experimentado foi rejeitado como direção final.

Para evolução visual:

- o contador de origem pode reagir ou pulsar;
- a seta deve evoluir para uma forma curva e premium;
- a visualização deve considerar a cor e a energia do jogador;
- o efeito deve comunicar intenção, não impacto já consumado.

### [DEFINITIVO] Resolução

A Resolução deve ser mais intensa e pode empregar, quando adequado:

- trajetória e energia;
- impacto e flash;
- partículas;
- fumaça;
- fogo quando fizer sentido;
- transição visual de controle;
- reação do contador;
- dissipação e encerramento legíveis.

### [DEFINITIVO] Autoridade lógica

A camada visual nunca decide gameplay. `GameManager` e os resultados lógicos são a autoridade. Presenters e sequências exibem estados e consequências já determinados, podendo usar estado visual temporário sem substituir o estado autoritativo.

## 10. Transferência territorial

### [DEFINITIVO] Regra funcional

Transferência é a entrega completa do controle de um território a um aliado, preservando as tropas existentes nesse território. Não é movimentação de tropas.

No modelo 2v2 atual há apenas um aliado elegível, portanto não é necessário perguntar o destinatário: a seleção direta do território é suficiente. A arquitetura deve continuar extensível para outros formatos.

### [DEFINITIVO] Regra visual

Uma transferência não deve mostrar:

- tropas viajando;
- rota origem-destino;
- quantidade em trânsito;
- explosão;
- impacto ofensivo;
- linguagem de combate.

Ela deve comunicar passagem amigável de controle no próprio território:

- estado/proprietário anterior;
- destaque de transição;
- novo proprietário e nova cor no momento correto;
- tropas preservadas conforme o resultado lógico;
- reação discreta do contador.

## 11. Mapas oficiais e vizinhanças

### [DEFINITIVO] Mapas oficiais

Os mapas oficiais atuais são:

- Classic;
- Pearl Harbor;
- Batalha do Riachuelo;
- Peloponeso;
- Dark World.

`MapaTeste12` foi removido e não integra mais o conjunto de mapas do produto.

### [DEFINITIVO] Regra de adjacência

- Territórios com fronteira terrestre compartilhada podem ser vizinhos.
- Territórios sem fronteira terrestre compartilhada exigem ponte, linha, caminho ou conector explícito definido pelo mapa.
- Toda conexão deve ser bidirecional.

## 12. Arte oficial, gabaritos e geometria

### [DEFINITIVO] Autoridades por camada

- A arte oficial é a camada visual que permanece visível.
- O gabarito branco 1:1 é a autoridade geométrica dos territórios.
- Máscaras e `PolygonCollider2D` sustentam interação e permanecem invisíveis no jogo final.
- Referências coloridas antigas servem somente como apoio semântico; não devem ser usadas para reconstruir geometria.

### [DEFINITIVO] Remaster visual sem mudança geométrica

Quando o trabalho for exclusivamente visual, devem ser preservados:

- resolução;
- registro/alinhamento;
- posições;
- proporções;
- fronteiras;
- geometria territorial.

Uma atualização visual não deve exigir máscaras ou colliders novos se a geometria oficial não mudou.

## 13. Remaster futuro de mapas

### [FUTURO] Classic

Um remaster mais profundo do Classic pode incluir nova arte, nova interpretação dos continentes e mudanças de fronteiras ou composição. Isso não bloqueia a evolução atual do produto.

Se as fronteiras mudarem, trata-se de nova geometria. Nesse caso:

- não reutilizar cegamente máscaras antigas;
- criar gabarito correspondente;
- gerar novas máscaras e colliders;
- revisar adjacências;
- revisar contadores;
- executar validação completa do mapa.

### [FUTURO] Dark World

O Dark World poderá seguir uma de duas direções, a decidir futuramente:

- **Opção A:** remaster preservando a geometria existente;
- **Opção B:** reconstrução profunda ou nova versão, com geometria e validações próprias.

Nenhuma dessas opções deve ser iniciada apenas por estar registrada neste documento.

## 14. Tabela de decisões

### Decisões definitivas

| DECISÃO | STATUS | OBSERVAÇÃO |
|---|---|---|
| Separar implementação funcional de aprovação visual | [DEFINITIVO] | Funcional no Play não significa visual aprovado. |
| Trabalhar a UI em dois passes | [DEFINITIVO] | Fundação global primeiro; polimento premium depois. |
| Diferenciar Home e HUD de partida | [DEFINITIVO] | Home é mais rica; HUD preserva protagonismo do mapa. |
| Remover TopBar permanente do HUD final | [DEFINITIVO] | Informações essenciais ficam nas bordas e as secundárias aparecem sob demanda. |
| Usar glow de forma controlada | [DEFINITIVO] | Menor na Preparação e maior na Resolução. |
| Usar packs externos como matéria-prima | [DEFINITIVO] | Nenhum pack define isoladamente a identidade. |
| Tornar PLAY protagonista da Home | [DEFINITIVO] | Progressão competitiva e recursos sociais se organizam ao redor desse foco. |
| Preservar fundo visível e composição não preenchida na Home | [DEFINITIVO] | Cards variam de escala conforme importância; PLAY não deve se tornar um retângulo simples gigantesco. |
| Adotar 10 ligas seguidas de Veteran | [DEFINITIVO] | Veteran tem progressão prática sem limite fechado. |
| Rejeitar pay-to-win | [DEFINITIVO] | Monetização não concede vantagem competitiva comprável. |
| Separar intenção de execução | [DEFINITIVO] | Preparação comunica plano; Resolução comunica consequência. |
| Manter o resultado lógico como autoridade | [DEFINITIVO] | A apresentação visual não decide gameplay. |
| Tratar transferência como passagem de controle | [DEFINITIVO] | Território inteiro passa ao aliado e preserva tropas. |
| Manter cinco mapas oficiais | [DEFINITIVO] | Classic, Pearl Harbor, Riachuelo, Peloponeso e Dark World. |
| Exigir conectores explícitos sem fronteira terrestre | [DEFINITIVO] | Conexões são bidirecionais. |
| Separar autoridade visual e geométrica dos mapas | [DEFINITIVO] | Arte oficial é visível; gabarito 1:1 governa geometria. |

### Implementações provisórias

| DECISÃO | STATUS | OBSERVAÇÃO |
|---|---|---|
| HUD atual da partida | [PROVISÓRIO] | A infraestrutura funciona, mas o design visual final não está aprovado. |
| Componentes premium atuais | [PROVISÓRIO] | `WDUIPremiumComponents` é fundação, não acabamento definitivo. |
| Setas de ações preparadas | [PROVISÓRIO] | `PreparedActionArrowView` deverá receber direção visual premium. |
| Presenters da Resolução | [PROVISÓRIO] | Contratos e sequência são reutilizáveis; aparência ainda evoluirá. |
| Molduras, botões, tipografia, contadores e feedbacks atuais | [PROVISÓRIO] | Devem ser avaliados no passe de polimento premium. |
| Dimensionamento dos cards da Home na Passada 1 | [PROVISÓRIO] | Valida arquitetura e fluxo, não a composição visual definitiva. |

### Decisões pendentes

| DECISÃO | STATUS | OBSERVAÇÃO |
|---|---|---|
| Economia e moedas | [PENDENTE] | Não definida. |
| Benefícios e limites de VIP | [PENDENTE] | Não definidos. |
| Modelo da Store | [PENDENTE] | Não definido. |
| Detalhes de monetização | [PENDENTE] | Devem respeitar a regra contra pay-to-win. |
| Catálogo de missões | [PENDENTE] | Não definido. |
| Limites e faixas das ligas | [PENDENTE] | A estrutura geral existe; os thresholds ainda não. |
| Regras de matchmaking | [PENDENTE] | Devem impedir confronto inadequado entre veteranos e iniciantes. |

### Evoluções futuras

| DECISÃO | STATUS | OBSERVAÇÃO |
|---|---|---|
| VIP/premium, cosméticos e progressão justa | [FUTURO] | Modelos a detalhar sem vantagem competitiva comprável. |
| Anúncios não intrusivos | [FUTURO] | Possibilidade, não implementação autorizada. |
| Outras receitas compatíveis | [FUTURO] | Exigem definição de produto. |
| Remaster profundo do Classic | [FUTURO] | Pode exigir nova geometria se fronteiras mudarem. |
| Remaster ou reconstrução do Dark World | [FUTURO] | Direção A ou B ainda será escolhida. |

## 15. Manutenção deste documento

### [DEFINITIVO] Regra de atualização

Toda decisão estrutural de produto ou design deve atualizar este documento no mesmo ciclo de trabalho em que for aprovada.

Ao atualizar:

- alterar o status quando uma pendência se tornar decisão;
- registrar substituições de direção, sem manter regras contraditórias como se ambas fossem atuais;
- preservar contexto suficiente para distinguir infraestrutura funcional de aparência aprovada;
- manter tabelas e seções coerentes entre si;
- não inventar detalhes para preencher itens ainda pendentes;
- tratar a versão corrente do documento como referência ativa para novas fases.
