using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace UnityEngine.AI
{
    class NavMeshBuilder2dState
    {
        public Dictionary<Sprite, Mesh> map;
        public Dictionary<uint, Mesh> coliderMap;
        public Action<Object, NavMeshBuildSource> lookupCallback;
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
        
        protected IEnumerable<GameObject> _root;
        public IEnumerable<GameObject> Root => _root ?? GetRoot();

        public NavMeshBuilder2dState()
        {
            map = new Dictionary<Sprite, Mesh>();
            coliderMap = new Dictionary<uint, Mesh>();
            _root = null;
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

        public Mesh GetMesh(Collider2D collider)
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
        public void SetRoot(IEnumerable<GameObject> root)
        {
            _root = root;
        }
        public IEnumerable<GameObject> GetRoot()
        {
            switch (CollectObjects)
            {
                case CollectObjects2d.Children: return new[] { parent };
                case CollectObjects2d.Volume: 
                case CollectObjects2d.All:
                default:
                {
                    var list = new List<GameObject>();
                    var testlist = new List<GameObject>();
                    for (int i = 0; i < SceneManager.sceneCount; ++i)
                    {
                        var s = SceneManager.GetSceneAt(i);
                        s.GetRootGameObjects(list);
                        testlist.AddRange(list);
                    }
                    return testlist;
                }
            }
        }
    }

    class NavMeshBuilder2d
    {
        internal static void CollectListSources(List<NavMeshBuildSource> sources, List<Vector2Int> slist, int area, NavMeshBuilder2dWrapper builder, Vector3 offset)
        {
            var size = new Vector3(1, 0.5f, 0);
            Mesh sharedMesh = null;
            Quaternion rot = default;

            var src = new NavMeshBuildSource();
            src.shape = NavMeshBuildSourceShape.Mesh;
            src.area = area;

            Vector3Int tpos = new Vector3Int(0, 0, 0);


            if (builder.useMeshPrefab != null)
            {
                sharedMesh = builder.useMeshPrefab.GetComponent<MeshFilter>().sharedMesh;
                //Debug.Log("Mesh.name is " + sharedMesh.name);

                size = builder.useMeshPrefab.transform.localScale;
                //Debug.Log("Mesh.size s " + size);

                rot = builder.useMeshPrefab.transform.rotation;
            }
            var new_pos = new Vector3(0, 0, 0);
            //Quaternion myrot = Quaternion.identity;
            Quaternion myrot = Quaternion.Euler(0, 0, 35.0f);

            Matrix4x4 myt = Matrix4x4.TRS(new_pos, myrot, Vector3.one);

            for (int x = 0; x < slist.Count; x++)
            {
                /* this does a cell to world but we dont want to reference the tilemap */
                /* so are assuming the cell width and height and implementing here for speed */
                //new_pos = t.CellToWorld(new_cell);
                new_pos.x = ((slist[x].x - slist[x].y) * 0.5F) + offset.x;
                new_pos.y = (((slist[x].x + slist[x].y) * 0.25f) + 0.25f) + offset.y;


                if (builder.useMeshPrefab != null || (builder.overrideByGrid && builder.useMeshPrefab != null))
                {
                    // Debug.Log("Shared mesh. Override:" + builder.overrideVector + " rot:" + rot + " size:" + size);
                    src.transform = Matrix4x4.TRS(Vector3.Scale(new_pos, builder.overrideVector), rot, size);

                    src.sourceObject = sharedMesh;
                    sources.Add(src);
                }
                else //default to box
                {
                    var boxsrc = new NavMeshBuildSource();

                    myt.SetTRS(new_pos, myrot, Vector3.one);

                    //Debug.Log(myt);
                    //Matrix4x4 ct =  base_transform.Tr;
                    // boxsrc.transform = Matrix4x4.TRS(Vector3.Scale(new_pos, builder.overrideVector), tilemap.transform.rotation, tilemap.transform.lossyScale) * tilemap.orientationMatrix * tilemap.GetTransformMatrix(vec3int);
                    //boxsrc.transform = Matrix4x4.TRS(Vector3.Scale(new_pos, builder.overrideVector), rot, size);
                    //boxsrc.transform = Matrix4x4.TRS(new_pos, rot, size);
                    boxsrc.transform = myt;

                    boxsrc.shape = NavMeshBuildSourceShape.Box;
                    boxsrc.size = size;
                    boxsrc.area = area;
                    //Debug.Log("Size is " + size);
                    sources.Add(boxsrc);
                }
            }
        }

        public static void CollectSources(List<NavMeshBuildSource> sources, NavMeshBuilder2dState builder)
        {
            foreach (var it in builder.Root)
            {
                if(!it.activeSelf){continue;}
                CollectSources(it, sources, builder);
            }
            if (!builder.hideEditorLogs) Debug.Log("Sources " + sources.Count);
        }

        public static void CollectSources(GameObject root, List<NavMeshBuildSource> sources, NavMeshBuilder2dState builder)
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
                    AddDefaultWalkableTilemap(sources, builder, modifier);
                }

                if (modifier.overrideArea)
                {
                    area = modifier.area;
                }
                if (!modifier.ignoreFromBuild)
                {
                    CollectSources(sources, builder, modifier, area);
                }
            }
        }

        public static void CollectSources(List<NavMeshBuildSource> sources, NavMeshBuilder2dState builder, NavMeshModifier modifier, int area)
        {
            if (builder.CollectGeometry == NavMeshCollectGeometry.PhysicsColliders)
            {
                var collider = modifier.GetComponent<Collider2D>();
                if (collider != null)
                {
                    CollectSources(sources, collider, area, builder);
                }
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

        private static void AddDefaultWalkableTilemap(List<NavMeshBuildSource> sources, NavMeshBuilder2dState builder, NavMeshModifier modifier)
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

        public static void CollectSources(List<NavMeshBuildSource> sources, SpriteRenderer spriteRenderer, int area, NavMeshBuilder2dState builder)
        {
            if (spriteRenderer == null)
            {
                return;
            }
            Mesh mesh;
            mesh = builder.GetMesh(spriteRenderer.sprite);
            if (mesh == null)
            {
                if (!builder.hideEditorLogs) Debug.Log($"{spriteRenderer.name} mesh is null");
                return;
            }
            var src = new NavMeshBuildSource();
            src.shape = NavMeshBuildSourceShape.Mesh;
            src.component = spriteRenderer;
            src.area = area;
            src.transform = Matrix4x4.TRS(Vector3.Scale(spriteRenderer.transform.position, builder.overrideVector), spriteRenderer.transform.rotation, spriteRenderer.transform.lossyScale);
            src.sourceObject = mesh;
            sources.Add(src);

            builder.lookupCallback?.Invoke(spriteRenderer.gameObject, src);
        }

        public static void CollectSources(List<NavMeshBuildSource> sources, Collider2D collider, int area, NavMeshBuilder2dState builder)
        { 
            if (collider.usedByComposite)
            {
                collider = collider.GetComponent<CompositeCollider2D>();
            }

            Mesh mesh;
            mesh = builder.GetMesh(collider);
            if (mesh == null)
            {
                if (!builder.hideEditorLogs) Debug.Log($"{collider.name} mesh is null");
                return;
            }

            var src = new NavMeshBuildSource();
            src.shape = NavMeshBuildSourceShape.Mesh;
            src.area = area;
            src.component = collider;
            src.sourceObject = mesh;
            if (collider.attachedRigidbody)
            {
                src.transform = Matrix4x4.TRS(Vector3.Scale(collider.attachedRigidbody.transform.position, builder.overrideVector), collider.attachedRigidbody.transform.rotation, Vector3.one);
            }
            else
            {
                src.transform = Matrix4x4.identity;
            }

            sources.Add(src);

            builder.lookupCallback?.Invoke(collider.gameObject, src);
        }

        public static void CollectTileSources(List<NavMeshBuildSource> sources, Tilemap tilemap, int area, NavMeshBuilder2dState builder)
        {
            var bound = tilemap.cellBounds;

            var vec3int = new Vector3Int(0, 0, 0);

            var size = new Vector3(tilemap.layoutGrid.cellSize.x, tilemap.layoutGrid.cellSize.y, 0);
            Mesh sharedMesh = null;
            Quaternion rot = default;

            var src = new NavMeshBuildSource();
            src.area = area;

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

                    CollectTile(tilemap, builder, vec3int, size, sharedMesh, rot, ref src);
                    sources.Add(src);

                    builder.lookupCallback?.Invoke(tilemap.GetInstantiatedObject(vec3int), src);
                }
            }
        }

        private static void CollectTile(Tilemap tilemap, NavMeshBuilder2dState builder, Vector3Int vec3int, Vector3 size, Mesh sharedMesh, Quaternion rot, ref NavMeshBuildSource src)
        {
            if (!builder.overrideByGrid && tilemap.GetColliderType(vec3int) == Tile.ColliderType.Sprite)
            {
                var sprite = tilemap.GetSprite(vec3int);
                if (sprite != null)
                {
                    Mesh mesh = builder.GetMesh(sprite);
                    src.component = tilemap;
                    src.transform = GetCellTransformMatrix(tilemap, builder.overrideVector, vec3int);
                    src.shape = NavMeshBuildSourceShape.Mesh;
                    src.sourceObject = mesh;
                }
            }
            else if (builder.useMeshPrefab != null || (builder.overrideByGrid && builder.useMeshPrefab != null))
            {
                src.transform = Matrix4x4.TRS(Vector3.Scale(tilemap.GetCellCenterWorld(vec3int), builder.overrideVector), rot, size);
                src.shape = NavMeshBuildSourceShape.Mesh;
                src.sourceObject = sharedMesh;
            }
            else //default to box
            {
                src.transform = GetCellTransformMatrix(tilemap, builder.overrideVector, vec3int);
                src.shape = NavMeshBuildSourceShape.Box;
                src.size = size;
            }
        }

        public static Matrix4x4 GetCellTransformMatrix(Tilemap tilemap, Vector3 scale, Vector3Int vec3int)
        {
            return Matrix4x4.TRS(Vector3.Scale(tilemap.GetCellCenterWorld(vec3int), scale) - tilemap.layoutGrid.cellGap, tilemap.transform.rotation, tilemap.transform.lossyScale) * tilemap.orientationMatrix * tilemap.GetTransformMatrix(vec3int);
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
