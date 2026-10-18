using NavMeshPlus.Components;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;

namespace NavMeshPlus.Extensions
{
    /// <summary>
    /// Defines the world bounds of the NavMesh (and an optional walkable floor) directly on the
    /// NavMeshSurface, so no extra GameObject with a NavMeshModifier is needed just to hold the bounds.
    /// Bounds are in world space (XY is the 2D plane, Z is the thickness of the floor box).
    /// </summary>
    [ExecuteAlways]
    [AddComponentMenu("Navigation/Navigation WorldBounds2d", 31)]
    public class WorldBounds2d : NavMeshExtension
    {
        // Same value NavMeshBuilder2d uses to decide whether a default floor is wanted (1 == Not Walkable).
        const int NotWalkableArea = 1;

        [SerializeField]
        Vector3 m_Center = Vector3.zero;
        public Vector3 center { get { return m_Center; } set { m_Center = value; } }

        [SerializeField]
        Vector3 m_Size = new Vector3(100f, 100f, 1f);
        public Vector3 size { get { return m_Size; } set { m_Size = value; } }

        [SerializeField, Tooltip("Adds a walkable floor box covering the bounds (ignored when Default Area is Not Walkable, e.g. mazes).")]
        bool m_AddWalkableFloor = true;
        public bool addWalkableFloor { get { return m_AddWalkableFloor; } set { m_AddWalkableFloor = value; } }

        public Bounds WorldBounds
        {
            get { return new Bounds(m_Center, new Vector3(Mathf.Abs(m_Size.x), Mathf.Abs(m_Size.y), Mathf.Max(Mathf.Abs(m_Size.z), 0.01f))); }
            set { m_Center = value.center; m_Size = value.size; }
        }

        public override void CalculateWorldBounds(NavMeshSurface surface, List<NavMeshBuildSource> sources, NavMeshBuilderState navNeshState)
        {
            // In Volume mode the surface's own Center/Size is the bake volume, same rule as CollectSources2d.
            if (surface.collectObjects == CollectObjects.Volume)
            {
                return;
            }
            navNeshState.worldBounds.Encapsulate(NavMeshSurface.GetWorldBounds(navNeshState.worldToLocal, WorldBounds));
        }

        public override void CollectSources(NavMeshSurface surface, List<NavMeshBuildSource> sources, NavMeshBuilderState navNeshState)
        {
            if (!m_AddWalkableFloor || surface.defaultArea == NotWalkableArea)
            {
                return;
            }
            var b = WorldBounds;
            sources.Add(new NavMeshBuildSource
            {
                shape = NavMeshBuildSourceShape.Box,
                transform = Matrix4x4.Translate(b.center),
                size = b.size,
                area = surface.defaultArea
            });
        }

        /// <summary>Sets the bounds so they enclose every Tilemap in the loaded scenes.</summary>
        [ContextMenu("Fit To Tilemaps")]
        public void FitToTilemaps()
        {
#if UNITY_2022_2_OR_NEWER
            var tilemaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
#else
            var tilemaps = FindObjectsOfType<Tilemap>();
#endif
            var found = false;
            var total = new Bounds();
            foreach (var tilemap in tilemaps)
            {
                if (tilemap.cellBounds.size == Vector3Int.zero)
                {
                    continue; // empty tilemap
                }
                var b = NavMeshSurface.GetWorldBounds(tilemap.transform.localToWorldMatrix, tilemap.localBounds);
                if (!found) { total = b; found = true; }
                else { total.Encapsulate(b); }
            }
            if (!found)
            {
                return;
            }
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(this, "Fit To Tilemaps");
#endif
            m_Center = total.center;
            m_Size = new Vector3(total.size.x, total.size.y, Mathf.Max(total.size.z, 1f));
        }

        void OnDrawGizmosSelected()
        {
            var b = WorldBounds;
            Gizmos.color = new Color(0.25f, 0.9f, 0.45f, 0.9f);
            Gizmos.DrawWireCube(b.center, b.size);
        }
    }
}
