using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NavMeshPlus.Extensions;
using UnityEngine.Tilemaps;
using UnityEngine.AI;

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
    public class NavMeshModifierTilemap : MonoBehaviour
    {
        [System.Serializable]
        public struct TileModifier
        {
            public TileBase tile;
            public bool overrideArea;
            [NavMeshArea] public int area;
        }

        [SerializeField]
        List<TileModifier> m_TileModifiers = new List<TileModifier>();
        public IEnumerable<TileModifier> tileModifiers => m_TileModifiers;

        private Dictionary<TileBase, TileModifier> m_ModifierMap;

        public virtual void PreModifySources()
        {
            m_ModifierMap = tileModifiers.Where(mod => mod.tile != null).ToDictionary(mod => mod.tile);
        }

        public virtual void ModifyBuildSource(Vector3Int coords, Tilemap tilemap, ref NavMeshBuildSource src)
        {
            if (tilemap.GetTile(coords) is TileBase tileBase)
            {
                if (m_ModifierMap.TryGetValue(tileBase, out TileModifier modifier))
                {
                    src.area = modifier.overrideArea ? modifier.area : src.area;
                }
            }
        }

        public virtual void PostModifySources()
        {
            m_ModifierMap = null;
        }
    }
}
