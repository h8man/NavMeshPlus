using UnityEngine;
using UnityEngine.Tilemaps;

//***********************************************************************************
// Contributed by author jl-randazzo github.com/jl-randazzo
//***********************************************************************************
namespace NavMeshPlus.Extensions
{
    public class NavTile : Tile
    {
        [SerializeField]
        private NavMeshAreaType _areaType;

        public int area => _areaType.areaID;

    }
}
