using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using UnityEditor.Tilemaps;
using NavMeshPlus.Extensions;

//***********************************************************************************
// Contributed by author jl-randazzo github.com/jl-randazzo
//***********************************************************************************
namespace NavMeshPlus.Editors.Extensions
{
    public static class NavTileFactory
    {
        /// <summary>Creates a Tile with defaults based on the Tile preset and a Sprite set</summary>
        /// <param name="sprite">A Sprite to set the Tile with</param>
        /// <returns>A Tile with defaults based on the Tile preset and a Sprite set</returns>
        [CreateTileFromPalette]
        public static TileBase CreateNavTile(Sprite sprite)
        {
            NavTile tile = ObjectFactory.CreateInstance<NavTile>();
            tile.name = sprite.name;
            tile.sprite = sprite;
            tile.color = Color.white;
            tile.overrideArea = false;
            tile.areaID = 0;

            return tile;
        }
    }
}
