using System.Collections.Generic;
using UnityEngine.Tilemaps;

namespace UnityEngine.AI
{
    class NavMeshBuilder2dWrapper
    {
        public Dictionary<Sprite, Mesh> map;
        public int defaultArea;
        public int layerMask;
        public bool overrideByGrid;
        public GameObject useMeshPrefab;
        public bool compressBounds;
        public Vector3 overrideVector;

        public NavMeshBuilder2dWrapper()
        {
            map = new Dictionary<Sprite, Mesh>();
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
    }
    class NavMeshBuilder2d
    {
        internal static void CollectGridSources(List<NavMeshBuildSource> sources, int defaultArea, int layerMask, bool overrideByGrid, GameObject useMeshPrefab, bool compressBounds, Vector3 overrideVector)
        {
            var builder = new NavMeshBuilder2dWrapper();
            builder.defaultArea = defaultArea;
            builder.layerMask = layerMask;
            builder.useMeshPrefab = useMeshPrefab;
            builder.overrideByGrid = overrideByGrid;
            builder.compressBounds = compressBounds;
            builder.overrideVector = overrideVector;
           var grid = GameObject.FindObjectOfType<Grid>();
            foreach (var tilemap in grid.GetComponentsInChildren<Tilemap>())
            {
                if (((0x1 << tilemap.gameObject.layer) & layerMask) == 0)
                {
                    continue;
                }
                int area = defaultArea;
                var modifier = tilemap.GetComponent<NavMeshModifier>();
                //if it is walkable
                if (defaultArea != 1 && (modifier == null || (modifier != null && !modifier.ignoreFromBuild)))
                {
                    if (compressBounds)
                    {
                        tilemap.CompressBounds();
                    }

                    Debug.Log($"Walkable Bounds [{tilemap.name}]: {tilemap.localBounds}");
                    var box = BoxBoundSource(NavMeshSurface2d.GetWorldBounds(tilemap.transform.localToWorldMatrix, tilemap.localBounds));
                    box.area = defaultArea;
                    sources.Add(box);
                }

                if (modifier != null && modifier.overrideArea)
                {
                    area = modifier.area;
                }
                if (modifier != null && !modifier.ignoreFromBuild)
                {
                    CollectTileSources(sources, tilemap, area, builder);
                }
            }
            Debug.Log("Sources " + sources.Count);
        }

        static private void CollectTileSources(List<NavMeshBuildSource> sources, Tilemap tilemap, int area, NavMeshBuilder2dWrapper builder)
        {
            var bound = tilemap.cellBounds;

            var vec3int = new Vector3Int(0, 0, 0);

            var size = new Vector3(tilemap.layoutGrid.cellSize.x, tilemap.layoutGrid.cellSize.y, tilemap.layoutGrid.cellSize.y);
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
                        mesh = builder.GetMesh(tilemap.GetSprite(vec3int));
                        src.transform = Matrix4x4.Translate(Vector3.Scale(tilemap.GetCellCenterWorld(vec3int),builder.overrideVector)) * tilemap.GetTransformMatrix(vec3int);
                        src.sourceObject = mesh;
                        sources.Add(src);
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
                        boxsrc.transform = Matrix4x4.Translate(Vector3.Scale(tilemap.GetCellCenterWorld(vec3int), builder.overrideVector));
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
