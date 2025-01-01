using NavMeshPlus.Components;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;

namespace NavMeshPlus.Extensions
{
    [ExecuteAlways]
    [AddComponentMenu("Navigation/Navigation CacheTilemapSources2d", 30)]
    public class CollectTilemapSourcesCache2d : NavMeshExtension
    {
        [SerializeField] private Tilemap _tilemap;
        [SerializeField] private NavMeshModifier _modifier;
        [SerializeField] private NavMeshModifierTilemap _modifierTilemap;

        private List<NavMeshBuildSource> _sources;
        private Dictionary<Vector3Int, int> _lookup;
        private Dictionary<TileBase, NavMeshModifierTilemap.TileModifier> _modifierMap;

        protected override void Awake()
        {
            _modifier ??= _tilemap.GetComponent<NavMeshModifier>();
            _modifierTilemap ??= _tilemap.GetComponent<NavMeshModifierTilemap>();
            _modifierMap = _modifierTilemap.GetModifierMap();
            Order = -1000;
            base.Awake();
        }

#if UNITY_EDITOR || UNITY_2022_2_OR_NEWER
        private void OnTilemapTileChanged(Tilemap tilemap, Tilemap.SyncTile[] syncTiles)
        {
            if (tilemap == _tilemap)
            {
                foreach (Tilemap.SyncTile syncTile in syncTiles)
                {
                    Vector3Int position = syncTile.position;
                    if (syncTile.tile != null && _modifierMap.TryGetValue(syncTile.tile, out NavMeshModifierTilemap.TileModifier tileModifier))
                    {
                        int i = _lookup[position];
                        NavMeshBuildSource source = _sources[i];
                        source.area = tileModifier.area;
                        _sources[i] = source;
                    }
                    else if (_modifier.overrideArea)
                    {
                        int i = _lookup[position];
                        NavMeshBuildSource source = _sources[i];
                        source.area = _modifier.area;
                        _sources[i] = source;
                    }
                }
            }
        }
#endif


        public AsyncOperation UpdateNavMesh(NavMeshData data)
        {
            return NavMeshBuilder.UpdateNavMeshDataAsync(data, NavMeshSurfaceOwner.GetBuildSettings(), _sources, data.sourceBounds);
        }

        public AsyncOperation UpdateNavMesh()
        {
            return UpdateNavMesh(NavMeshSurfaceOwner.navMeshData);
        }

        public override void PostCollectSources(NavMeshSurface surface, List<NavMeshBuildSource> sources, NavMeshBuilderState navNeshState)
        {
            _sources = sources;
            if (_lookup == null)
            {
                _lookup = new Dictionary<Vector3Int, int>();
                for (int i = 0; i < _sources.Count; i++)
                {
                    NavMeshBuildSource source = _sources[i];
                    Vector3Int position = _tilemap.WorldToCell(source.transform.GetPosition());
                    _lookup[position] = i;
                }
            }
            #if UNITY_EDITOR || UNITY_2022_2_OR_NEWER
            Tilemap.tilemapTileChanged -= OnTilemapTileChanged;
            Tilemap.tilemapTileChanged += OnTilemapTileChanged;
            #endif
        }

        protected override void OnDestroy()
        {
            #if UNITY_EDITOR || UNITY_2022_2_OR_NEWER
            Tilemap.tilemapTileChanged -= OnTilemapTileChanged;
            #endif
            base.OnDestroy();
        }
    }
}
