using NavMeshPlus.Components;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;

namespace NavMeshPlus.Extensions
{
    [ExecuteAlways]
    [AddComponentMenu("Navigation/NavMesh CollectSources2d", 30)]
    public class CollectSources2d: NavMeshExtension
    {
        [SerializeField]
        bool m_OverrideByGrid;
        public bool overrideByGrid { get { return m_OverrideByGrid; } set { m_OverrideByGrid = value; } }

        [SerializeField]
        GameObject m_UseMeshPrefab;
        public GameObject useMeshPrefab { get { return m_UseMeshPrefab; } set { m_UseMeshPrefab = value; } }

        [SerializeField]
        bool m_CompressBounds;
        public bool compressBounds { get { return m_CompressBounds; } set { m_CompressBounds = value; } }

        [SerializeField]
        Vector3 m_OverrideVector = Vector3.one;
        public Vector3 overrideVector { get { return m_OverrideVector; } set { m_OverrideVector = value; } }

        public override void CalculateWorldBounds(NavMeshSurface surface, List<NavMeshBuildSource> sources, NavMeshBuilderState navNeshState)
        {
            if (surface.collectObjects != CollectObjects.Volume)
            {
                navNeshState.worldBounds.Encapsulate(CalculateGridWorldBounds(surface, navNeshState.worldToLocal, navNeshState.worldBounds));
            }
        }

        private static Bounds CalculateGridWorldBounds(NavMeshSurface surface, Matrix4x4 worldToLocal, Bounds bounds)
        {
            var grid = FindObjectOfType<Grid>();
            var tilemaps = grid?.GetComponentsInChildren<Tilemap>();
            if (tilemaps == null || tilemaps.Length < 1)
            {
                return bounds;
            }
            foreach (var tilemap in tilemaps)
            {
                var lbounds = NavMeshSurface.GetWorldBounds(worldToLocal * tilemap.transform.localToWorldMatrix, tilemap.localBounds);
                bounds.Encapsulate(lbounds);
                if (!surface.hideEditorLogs)
                {
                    Debug.Log($"From Local Bounds [{tilemap.name}]: {tilemap.localBounds}");
                    Debug.Log($"To World Bounds: {bounds}");
                }
            }
            return bounds;
        }

        public override void CollectSources(NavMeshSurface surface, List<NavMeshBuildSource> sources, NavMeshBuilderState navNeshState)
        {
            if (!surface.hideEditorLogs)
            {
                if (!Mathf.Approximately(transform.eulerAngles.x, 270f))
                {
                    Debug.LogWarning("NavMeshSurface is not rotated respectively to (x-90;y0;z0). Apply rotation unless intended.");
                }
                if (Application.isPlaying)
                {
                    if (surface.useGeometry == NavMeshCollectGeometry.PhysicsColliders && Time.frameCount <= 1)
                    {
                        Debug.LogWarning("Use Geometry - Physics Colliders option in NavMeshSurface may cause inaccurate mesh bake if executed before Physics update.");
                    }
                }
            }
            var builder = navNeshState.GetExtraState<NavMeshBuilder2dState>();
            builder.defaultArea = surface.defaultArea;
            builder.layerMask = surface.layerMask;
            builder.agentID = surface.agentTypeID;
            builder.useMeshPrefab = useMeshPrefab;
            builder.overrideByGrid = overrideByGrid;
            builder.compressBounds = compressBounds;
            builder.overrideVector = overrideVector;
            builder.CollectGeometry = surface.useGeometry;
            builder.CollectObjects = (CollectObjects)(int)surface.collectObjects;
            builder.parent = surface.gameObject;
            builder.hideEditorLogs = surface.hideEditorLogs;
            builder.SetRoot(navNeshState.roots);
            NavMeshBuilder2d.CollectSources(sources, builder);
        }
    }
}
