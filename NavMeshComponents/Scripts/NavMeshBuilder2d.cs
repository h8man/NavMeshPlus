using System;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

namespace UnityEngine.AI
{
    class NavMeshBuilder2dWrapper
    {
        public Dictionary<Sprite, Mesh> map;
        public Dictionary<uint, Mesh> coliderMap;
        public int defaultArea;
        public int layerMask;
        public int agentID;
        public bool overrideByGrid;
        public GameObject useMeshPrefab;
        public bool compressBounds;
        public Vector3 overrideVector;
        public NavMeshCollectGeometry CollectGeometry;
        public CollectObjects2d CollectObjects;
        public GameObject parent;
        public bool hideEditorLogs;

        public NavMeshBuilder2dWrapper()
        {
            map = new Dictionary<Sprite, Mesh>();
            coliderMap = new Dictionary<uint, Mesh>();
        }

        public Mesh GetMesh(Sprite sprite)
        {
            Mesh mesh;
            if (map.ContainsKey(sprite))
            {
                mesh = map[sprite];
            }
            else
            {
                mesh = new Mesh();
                NavMeshBuilder2d.sprite2mesh(sprite, mesh);
                map.Add(sprite, mesh);
            }
            return mesh;
        }

        internal Mesh GetMesh(Collider2D collider)
        {
#if UNITY_2019_3_OR_NEWER
            Mesh mesh;
            uint hash = collider.GetShapeHash();
            if (coliderMap.ContainsKey(hash))
            {
                mesh = coliderMap[hash];
            }
            else
            {
                mesh = collider.CreateMesh(false, false);
                coliderMap.Add(hash, mesh);
            }
            return mesh;
#else
            throw new InvalidOperationException("PhysicsColliders supported in Unity 2019.3 and higher.");
#endif
        }

        internal IEnumerable<GameObject> GetRoot()
        {
            switch (CollectObjects)
            {
                case CollectObjects2d.Children: return new[] { parent };
                case CollectObjects2d.Volume: 
                case CollectObjects2d.All: 
                default:
                    return new[] { GameObject.FindObjectOfType<Grid>().gameObject };
            }
        }
    }

    class NavMeshBuilder2d
    {
        internal static void CollectSources(List<NavMeshBuildSource> sources, NavMeshBuilder2dWrapper builder)
        {
            var root = builder.GetRoot();
            foreach (var it in root)
            {
                CollectSources(it, sources, builder);
            }
        }

        private static void CollectSources(GameObject root, List<NavMeshBuildSource> sources, NavMeshBuilder2dWrapper builder)
        {
            foreach (var modifier in root.GetComponentsInChildren<NavMeshModifier>())
            {
                if (((0x1 << modifier.gameObject.layer) & builder.layerMask) == 0)
                {
                    continue;
                }
                if (!modifier.AffectsAgentType(builder.agentID))
                {
                    continue;
                }
                int area = builder.defaultArea;
                //if it is walkable
                if (builder.defaultArea != 1 && !modifier.ignoreFromBuild)
                {
                    var tilemap = modifier.GetComponent<Tilemap>();
                    if (tilemap != null)
                    {
                        if (builder.compressBounds)
                        {
                            tilemap.CompressBounds();
                        }

                        if (!builder.hideEditorLogs) Debug.Log($"Walkable Bounds [{tilemap.name}]: {tilemap.localBounds}");
                        var box = BoxBoundSource(NavMeshSurface2d.GetWorldBounds(tilemap.transform.localToWorldMatrix, tilemap.localBounds));
                        box.area = builder.defaultArea;
                        sources.Add(box);
                    }
                }

                if (modifier.overrideArea)
                {
                    area = modifier.area;
                }
                if (!modifier.ignoreFromBuild)
                {
                    if (builder.CollectGeometry == NavMeshCollectGeometry.PhysicsColliders)
                    {
                        CollectSources(sources, modifier, area, builder);
                    }
                    else
                    {
                        var tilemap = modifier.GetComponent<Tilemap>();
                        if (tilemap != null)
                        {
                            CollectTileSources(sources, tilemap, area, builder);
                        }
                        var sprite = modifier.GetComponent<SpriteRenderer>();
                        if (sprite != null)
                        {
                            CollectSources(sources, sprite, area, builder);
                        }
                    }
                }
            }
            if (!builder.hideEditorLogs) Debug.Log("Sources " + sources.Count);
        }

        private static void CollectSources(List<NavMeshBuildSource> sources, SpriteRenderer sprite, int area, NavMeshBuilder2dWrapper builder)
        {
            if (sprite == null)
            {
                return;
            }
            var src = new NavMeshBuildSource();
            src.shape = NavMeshBuildSourceShape.Mesh;
            src.area = area;

            Mesh mesh;
            mesh = builder.GetMesh(sprite.sprite);
            if (mesh == null)
            {
                if (!builder.hideEditorLogs) Debug.Log($"{sprite.name} mesh is null");
                return;
            }
            src.transform = Matrix4x4.TRS(Vector3.Scale(sprite.transform.position, builder.overrideVector), sprite.transform.rotation, sprite.transform.lossyScale);
            src.sourceObject = mesh;
            sources.Add(src);
        }

        private static void CollectSources(List<NavMeshBuildSource> sources, NavMeshModifier modifier, int area, NavMeshBuilder2dWrapper builder)
        {
            var collider = modifier.GetComponent<Collider2D>();
            if (collider == null)
            {
                return;
            }

            if (collider.usedByComposite)
            {
                collider = collider.GetComponent<CompositeCollider2D>();
            }

            var src = new NavMeshBuildSource();
            src.shape = NavMeshBuildSourceShape.Mesh;
            src.area = area;

            Mesh mesh;
            mesh = builder.GetMesh(collider);
            if (mesh == null)
            {
                if (!builder.hideEditorLogs) Debug.Log($"{collider.name} mesh is null");
                return;
            }
            if (collider.attachedRigidbody)
            {
                src.transform = Matrix4x4.TRS(Vector3.Scale(collider.transform.position, builder.overrideVector), collider.transform.rotation, Vector3.one);
            }
            else
            {
                src.transform = Matrix4x4.identity;
            }
            src.sourceObject = mesh;
            sources.Add(src);
        }

        static private void CollectTileSources(List<NavMeshBuildSource> sources, Tilemap tilemap, int area, NavMeshBuilder2dWrapper builder)
        {
            var bound = tilemap.cellBounds;

            var vec3int = new Vector3Int(0, 0, 0);

            var size = new Vector3(tilemap.layoutGrid.cellSize.x, tilemap.layoutGrid.cellSize.y, 0);
            Mesh sharedMesh = null;
            Quaternion rot = default;

            var src = new NavMeshBuildSource();
            src.shape = NavMeshBuildSourceShape.Mesh;
            src.area = area;

            Mesh mesh;

            if (builder.useMeshPrefab != null)
            {
                sharedMesh = builder.useMeshPrefab.GetComponent<MeshFilter>().sharedMesh;
                size = builder.useMeshPrefab.transform.localScale;
                rot = builder.useMeshPrefab.transform.rotation;
            }
            for (int i = bound.xMin; i < bound.xMax; i++)
            {
                for (int j = bound.yMin; j < bound.yMax; j++)
                {
                    vec3int.x = i;
                    vec3int.y = j;
                    if (!tilemap.HasTile(vec3int))
                    {
                        continue;
                    }

                    if (!builder.overrideByGrid && tilemap.GetColliderType(vec3int) == Tile.ColliderType.Sprite)
                    {
                        var sprite = tilemap.GetSprite(vec3int);
                        if (sprite != null)
                        {
                            mesh = builder.GetMesh(sprite);
                            src.transform = Matrix4x4.TRS(Vector3.Scale(tilemap.GetCellCenterWorld(vec3int), builder.overrideVector) - tilemap.layoutGrid.cellGap, tilemap.transform.rotation, tilemap.transform.lossyScale) * tilemap.orientationMatrix * tilemap.GetTransformMatrix(vec3int);
                            src.sourceObject = mesh;
                            sources.Add(src);
                        }
                    }
                    else if (builder.useMeshPrefab != null || (builder.overrideByGrid && builder.useMeshPrefab != null))
                    {
                        src.transform = Matrix4x4.TRS(Vector3.Scale(tilemap.GetCellCenterWorld(vec3int), builder.overrideVector), rot, size);
                        src.sourceObject = sharedMesh;
                        sources.Add(src);
                    }
                    else //default to box
                    {
                        var boxsrc = new NavMeshBuildSource();
                        boxsrc.transform = Matrix4x4.TRS(Vector3.Scale(tilemap.GetCellCenterWorld(vec3int), builder.overrideVector) - tilemap.layoutGrid.cellGap, tilemap.transform.rotation, tilemap.transform.lossyScale) * tilemap.orientationMatrix * tilemap.GetTransformMatrix(vec3int);
                        boxsrc.shape = NavMeshBuildSourceShape.Box;
                        boxsrc.size = size;
                        boxsrc.area = area;
                        sources.Add(boxsrc);
                    }
                }
            }
        }

        internal static void sprite2mesh(Sprite sprite, Mesh mesh)
        {
            Vector3[] vert = new Vector3[sprite.vertices.Length];
            for (int i = 0; i < sprite.vertices.Length; i++)
            {
                vert[i] = new Vector3(sprite.vertices[i].x, sprite.vertices[i].y, 0);
            }
            mesh.vertices = vert;
            mesh.uv = sprite.uv;
            int[] tri = new int[sprite.triangles.Length];
            for (int i = 0; i < sprite.triangles.Length; i++)
            {
                tri[i] = sprite.triangles[i];
            }
            mesh.triangles = tri;
        }

        static private NavMeshBuildSource BoxBoundSource(Bounds localBounds)
        {
            var src = new NavMeshBuildSource();
            src.transform = Matrix4x4.Translate(localBounds.center);
            src.shape = NavMeshBuildSourceShape.Box;
            src.size = localBounds.size;
            src.area = 0;
            return src;
        }
    }
}
