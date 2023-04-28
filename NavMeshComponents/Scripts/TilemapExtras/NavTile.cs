using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

//***********************************************************************************
// Contributed by author jl-randazzo github.com/jl-randazzo
//***********************************************************************************
namespace NavMeshPlus.Extensions
{
    [System.Serializable]
    public class NavTile : Tile
    {
        public bool overrideArea;
        [NavMeshArea] public int areaID;
    }
}
