using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CatalogoMapas", menuName = "WarGame/Catalogo de Mapas")]
public sealed class CatalogoMapas : ScriptableObject
{
    // ============================================================
    // 1. CATÁLOGO E MAPA PADRÃO
    // ============================================================

    [SerializeField] private DefinicaoMapa mapaPadrao;
    [SerializeField] private List<DefinicaoMapa> mapas = new List<DefinicaoMapa>();

    public DefinicaoMapa MapaPadrao => mapaPadrao;
    public IReadOnlyList<DefinicaoMapa> Mapas => mapas;

#if UNITY_EDITOR
    public void ConfigurarNoEditor(DefinicaoMapa novoMapaPadrao, List<DefinicaoMapa> novosMapas)
    {
        mapaPadrao = novoMapaPadrao;
        mapas = novosMapas;
    }
#endif
}
