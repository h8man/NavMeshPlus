using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NavMeshPlus.Extensions;
using UnityEngine.Tilemaps;

//***********************************************************************************
// Contributed by author jl-randazzo github.com/jl-randazzo
//***********************************************************************************
namespace NavMeshPlus.Components
{
    [AddComponentMenu("Navigation/Navigation Modifier Tilemap", 33)]
    [HelpURL("https://github.com/Unity-Technologies/NavMeshComponents#documentation-draft")]
    [RequireComponent(typeof(Tilemap))]
    [RequireComponent(typeof(NavMeshModifier))]
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    public class NavMeshModifierTilemap : MonoBehaviour
    {
        [System.Serializable]
        public struct TileModifier
        {
            public TileBase tile;
            public bool overrideArea;
            [NavMeshArea] public int area;
        }

        private class MatchingTileComparator : IEqualityComparer<TileModifier>
        {
            public static readonly IEqualityComparer<TileModifier> Instance = new MatchingTileComparator();
            public bool Equals(TileModifier a, TileModifier b) => a.tile == b.tile;
            public int GetHashCode(TileModifier tileModifier) => tileModifier.GetHashCode();
        }

        [SerializeField]
        List<TileModifier> m_TileModifiers = new List<TileModifier>();

        private Dictionary<TileBase, TileModifier> m_ModifierMap;

        public Dictionary<TileBase, TileModifier> GetModifierMap() => m_TileModifiers.Where(mod => mod.tile != null).Distinct(MatchingTileComparator.Instance).ToDictionary(mod => mod.tile);

        void OnEnable()
        {
            CacheModifiers();
        }

        public void CacheModifiers()
        {
            m_ModifierMap = GetModifierMap();
        }

#if UNITY_EDITOR
        public bool HasDuplicateTileModifiers()
        {
            return m_TileModifiers.Count != m_TileModifiers.Distinct(MatchingTileComparator.Instance).Count();
        }
#endif // UNITY_EDITOR

        public virtual bool TryGetTileModifier(Vector3Int coords, Tilemap tilemap, out TileModifier modifier)
        {
            if (tilemap.GetTile(coords) is TileBase tileBase)
            {
                return m_ModifierMap.TryGetValue(tileBase, out modifier);
            }
            modifier = new TileModifier();
            return false;
        }
    }
}
