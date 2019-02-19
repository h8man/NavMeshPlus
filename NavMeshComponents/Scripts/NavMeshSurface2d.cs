using System;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace UnityEngine.AI
{
    public enum CollectObjects2d
    {
        All = 0,
        Volume = 1,
        Children = 2,
        Grid = 3,
    }

    [ExecuteAlways]
    [DefaultExecutionOrder(-102)]
    [AddComponentMenu("Navigation/NavMeshSurface2d", 30)]
    [HelpURL("https://github.com/Unity-Technologies/NavMeshComponents#documentation-draft")]
    public class NavMeshSurface2d : MonoBehaviour
    {
        [SerializeField]
        int m_AgentTypeID;
        public int agentTypeID { get { return m_AgentTypeID; } set { m_AgentTypeID = value; } }

        [SerializeField]
        CollectObjects2d m_CollectObjects = CollectObjects2d.All;
        public CollectObjects2d collectObjects { get { return m_CollectObjects; } set { m_CollectObjects = value; } }

        [SerializeField]
        Vector3 m_Size = new Vector3(10.0f, 10.0f, 10.0f);
        public Vector3 size { get { return m_Size; } set { m_Size = value; } }

        [SerializeField]
        Vector3 m_Center = new Vector3(0, 2.0f, 0);
        public Vector3 center { get { return m_Center; } set { m_Center = value; } }

        [SerializeField]
        LayerMask m_LayerMask = ~0;
        public LayerMask layerMask { get { return m_LayerMask; } set { m_LayerMask = value; } }

        [SerializeField]
        NavMeshCollectGeometry m_UseGeometry = NavMeshCollectGeometry.RenderMeshes;
        public NavMeshCollectGeometry useGeometry { get { return m_UseGeometry; } set { m_UseGeometry = value; } }

        [SerializeField]
        GameObject m_UseMeshPrefab;
        public GameObject useMeshpPrefab { get { return m_UseMeshPrefab; } set { m_UseMeshPrefab = value; } }

        [SerializeField]
        int m_DefaultArea;
        public int defaultArea { get { return m_DefaultArea; } set { m_DefaultArea = value; } }

        [SerializeField]
        bool m_IgnoreNavMeshAgent = true;
        public bool ignoreNavMeshAgent { get { return m_IgnoreNavMeshAgent; } set { m_IgnoreNavMeshAgent = value; } }

        [SerializeField]
        bool m_IgnoreNavMeshObstacle = true;
        public bool ignoreNavMeshObstacle { get { return m_IgnoreNavMeshObstacle; } set { m_IgnoreNavMeshObstacle = value; } }

        [SerializeField]
        bool m_OverrideTileSize;
        public bool overrideTileSize { get { return m_OverrideTileSize; } set { m_OverrideTileSize = value; } }
        [SerializeField]
        int m_TileSize = 256;
        public int tileSize { get { return m_TileSize; } set { m_TileSize = value; } }
        [SerializeField]
        bool m_OverrideVoxelSize;
        public bool overrideVoxelSize { get { return m_OverrideVoxelSize; } set { m_OverrideVoxelSize = value; } }
        [SerializeField]
        float m_VoxelSize;
        public float voxelSize { get { return m_VoxelSize; } set { m_VoxelSize = value; } }

        // Currently not supported advanced options
        [SerializeField]
        bool m_BuildHeightMesh;
        public bool buildHeightMesh { get { return m_BuildHeightMesh; } set { m_BuildHeightMesh = value; } }

        // Reference to whole scene navmesh data asset.
        [UnityEngine.Serialization.FormerlySerializedAs("m_BakedNavMeshData")]
        [SerializeField]
        NavMeshData m_NavMeshData;
        public NavMeshData navMeshData { get { return m_NavMeshData; } set { m_NavMeshData = value; } }

        // Do not serialize - runtime only state.
        NavMeshDataInstance m_NavMeshDataInstance;
        Vector3 m_LastPosition = Vector3.zero;
        Quaternion m_LastRotation = Quaternion.identity;

        static readonly List<NavMeshSurface2d> s_NavMeshSurfaces = new List<NavMeshSurface2d>();

        public static List<NavMeshSurface2d> activeSurfaces
        {
            get { return s_NavMeshSurfaces; }
        }

        void OnEnable()
        {
            Register(this);
            AddData();
        }

        void OnDisable()
        {
            RemoveData();
            Unregister(this);
        }

        public void AddData()
        {
#if UNITY_EDITOR
            var isInPreviewScene = EditorSceneManager.IsPreviewSceneObject(this);
            var isPrefab = isInPreviewScene || EditorUtility.IsPersistent(this);
            if (isPrefab)
            {
                //Debug.LogFormat("NavMeshData from {0}.{1} will not be added to the NavMesh world because the gameObject is a prefab.",
                //    gameObject.name, name);
                return;
            }
#endif
            if (m_NavMeshDataInstance.valid)
                return;

            if (m_NavMeshData != null)
            {
                m_NavMeshDataInstance = NavMesh.AddNavMeshData(m_NavMeshData, transform.position, transform.rotation);
                m_NavMeshDataInstance.owner = this;
            }

            m_LastPosition = transform.position;
            m_LastRotation = transform.rotation;
        }

        public void RemoveData()
        {
            m_NavMeshDataInstance.Remove();
            m_NavMeshDataInstance = new NavMeshDataInstance();
        }

        public NavMeshBuildSettings GetBuildSettings()
        {
            var buildSettings = NavMesh.GetSettingsByID(m_AgentTypeID);
            if (buildSettings.agentTypeID == -1)
            {
                Debug.LogWarning("No build settings for agent type ID " + agentTypeID, this);
                buildSettings.agentTypeID = m_AgentTypeID;
            }

            if (overrideTileSize)
            {
                buildSettings.overrideTileSize = true;
                buildSettings.tileSize = tileSize;
            }
            if (overrideVoxelSize)
            {
                buildSettings.overrideVoxelSize = true;
                buildSettings.voxelSize = voxelSize;
            }
            return buildSettings;
        }

        public void BuildNavMesh()
        {
            var sources = CollectSources();

            // Use unscaled bounds - this differs in behaviour from e.g. collider components.
            // But is similar to reflection probe - and since navmesh data has no scaling support - it is the right choice here.
            var sourcesBounds = new Bounds(m_Center, Abs(m_Size));
            if (m_CollectObjects != CollectObjects2d.Volume)
            {
                sourcesBounds = CalculateWorldBounds(sources);
            }

            var data = NavMeshBuilder.BuildNavMeshData(GetBuildSettings(),
                    sources, sourcesBounds, transform.position, transform.rotation);

            if (data != null)
            {
                data.name = gameObject.name;
                RemoveData();
                m_NavMeshData = data;
                if (isActiveAndEnabled)
                    AddData();
            }
        }

        public AsyncOperation UpdateNavMesh(NavMeshData data)
        {
            var sources = CollectSources();

            // Use unscaled bounds - this differs in behaviour from e.g. collider components.
            // But is similar to reflection probe - and since navmesh data has no scaling support - it is the right choice here.
            var sourcesBounds = new Bounds(m_Center, Abs(m_Size));
            if (m_CollectObjects != CollectObjects2d.Volume)
                sourcesBounds = CalculateWorldBounds(sources);

            return NavMeshBuilder.UpdateNavMeshDataAsync(data, GetBuildSettings(), sources, sourcesBounds);
        }

        static void Register(NavMeshSurface2d surface)
        {
#if UNITY_EDITOR
            var isInPreviewScene = EditorSceneManager.IsPreviewSceneObject(surface);
            var isPrefab = isInPreviewScene || EditorUtility.IsPersistent(surface);
            if (isPrefab)
            {
                //Debug.LogFormat("NavMeshData from {0}.{1} will not be added to the NavMesh world because the gameObject is a prefab.",
                //    surface.gameObject.name, surface.name);
                return;
            }
#endif
            if (s_NavMeshSurfaces.Count == 0)
                NavMesh.onPreUpdate += UpdateActive;

            if (!s_NavMeshSurfaces.Contains(surface))
                s_NavMeshSurfaces.Add(surface);
        }

        static void Unregister(NavMeshSurface2d surface)
        {
            s_NavMeshSurfaces.Remove(surface);

            if (s_NavMeshSurfaces.Count == 0)
                NavMesh.onPreUpdate -= UpdateActive;
        }

        static void UpdateActive()
        {
            for (var i = 0; i < s_NavMeshSurfaces.Count; ++i)
                s_NavMeshSurfaces[i].UpdateDataIfTransformChanged();
        }

        void AppendModifierVolumes(ref List<NavMeshBuildSource> sources)
        {
#if UNITY_EDITOR
            var myStage = StageUtility.GetStageHandle(gameObject);
            if (!myStage.IsValid())
                return;
#endif
            // Modifiers
            List<NavMeshModifierVolume> modifiers;
            if (m_CollectObjects == CollectObjects2d.Children)
            {
                modifiers = new List<NavMeshModifierVolume>(GetComponentsInChildren<NavMeshModifierVolume>());
                modifiers.RemoveAll(x => !x.isActiveAndEnabled);
            }
            else
            {
                modifiers = NavMeshModifierVolume.activeModifiers;
            }

            foreach (var m in modifiers)
            {
                if ((m_LayerMask & (1 << m.gameObject.layer)) == 0)
                    continue;
                if (!m.AffectsAgentType(m_AgentTypeID))
                    continue;
#if UNITY_EDITOR
                if (!myStage.Contains(m.gameObject))
                    continue;
#endif
                var mcenter = m.transform.TransformPoint(m.center);
                var scale = m.transform.lossyScale;
                var msize = new Vector3(m.size.x * Mathf.Abs(scale.x), m.size.y * Mathf.Abs(scale.y), m.size.z * Mathf.Abs(scale.z));

                var src = new NavMeshBuildSource();
                src.shape = NavMeshBuildSourceShape.ModifierBox;
                src.transform = Matrix4x4.TRS(mcenter, m.transform.rotation, Vector3.one);
                src.size = msize;
                src.area = m.area;
                sources.Add(src);
            }
        }

        List<NavMeshBuildSource> CollectSources()
        {
            var sources = new List<NavMeshBuildSource>();
            var markups = new List<NavMeshBuildMarkup>();

            List<NavMeshModifier> modifiers;
            if (m_CollectObjects == CollectObjects2d.Children)
            {
                modifiers = new List<NavMeshModifier>(GetComponentsInChildren<NavMeshModifier>());
                modifiers.RemoveAll(x => !x.isActiveAndEnabled);
            }
            else
            {
                modifiers = NavMeshModifier.activeModifiers;
            }

            foreach (var m in modifiers)
            {
                if ((m_LayerMask & (1 << m.gameObject.layer)) == 0)
                    continue;
                if (!m.AffectsAgentType(m_AgentTypeID))
                    continue;
                var markup = new NavMeshBuildMarkup();
                markup.root = m.transform;
                markup.overrideArea = m.overrideArea;
                markup.area = m.area;
                markup.ignoreFromBuild = m.ignoreFromBuild;
                markups.Add(markup);
            }

#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                if (m_CollectObjects == CollectObjects2d.All)
                {
                    UnityEditor.AI.NavMeshBuilder.CollectSourcesInStage(
                        null, m_LayerMask, m_UseGeometry, m_DefaultArea, markups, gameObject.scene, sources);
                }
                else if (m_CollectObjects == CollectObjects2d.Children)
                {
                    UnityEditor.AI.NavMeshBuilder.CollectSourcesInStage(
                        transform, m_LayerMask, m_UseGeometry, m_DefaultArea, markups, gameObject.scene, sources);
                }
                else if (m_CollectObjects == CollectObjects2d.Volume)
                {
                    Matrix4x4 localToWorld = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                    var worldBounds = GetWorldBounds(localToWorld, new Bounds(m_Center, m_Size));

                    UnityEditor.AI.NavMeshBuilder.CollectSourcesInStage(
                        worldBounds, m_LayerMask, m_UseGeometry, m_DefaultArea, markups, gameObject.scene, sources);
                }
                if (m_CollectObjects == CollectObjects2d.Grid)
                {
                    UnityEditor.AI.NavMeshBuilder.CollectSourcesInStage(
                        transform, m_LayerMask, m_UseGeometry, m_DefaultArea, markups, gameObject.scene, sources);
                    CollectGridSources(sources);
                }
            }
            else
#endif
            {
                if (m_CollectObjects == CollectObjects2d.All)
                {
                    NavMeshBuilder.CollectSources(null, m_LayerMask, m_UseGeometry, m_DefaultArea, markups, sources);
                }
                else if (m_CollectObjects == CollectObjects2d.Children)
                {
                    NavMeshBuilder.CollectSources(transform, m_LayerMask, m_UseGeometry, m_DefaultArea, markups, sources);
                }
                else if (m_CollectObjects == CollectObjects2d.Volume)
                {
                    Matrix4x4 localToWorld = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                    var worldBounds = GetWorldBounds(localToWorld, new Bounds(m_Center, m_Size));
                    NavMeshBuilder.CollectSources(worldBounds, m_LayerMask, m_UseGeometry, m_DefaultArea, markups, sources);
                }
                if (m_CollectObjects == CollectObjects2d.Grid)
                {
                    NavMeshBuilder.CollectSources(transform, m_LayerMask, m_UseGeometry, m_DefaultArea, markups, sources);
                    CollectGridSources(sources);
                }
            }
            if (m_IgnoreNavMeshAgent)
                sources.RemoveAll((x) => (x.component != null && x.component.gameObject.GetComponent<NavMeshAgent>() != null));

            if (m_IgnoreNavMeshObstacle)
                sources.RemoveAll((x) => (x.component != null && x.component.gameObject.GetComponent<NavMeshObstacle>() != null));

            AppendModifierVolumes(ref sources);

            return sources;
        }

        private void CollectGridSources(List<NavMeshBuildSource> sources)
        {
            var grid = FindObjectOfType<Grid>();
            bool flag = true;
            foreach (var tilemap in grid.GetComponentsInChildren<Tilemap>())
            {
                if ( ((0x1 << tilemap.gameObject.layer) & m_LayerMask) == 0)
                {
                    continue;
                }
                if (flag)
                {
                    Debug.Log($"Walkable Bounds [{tilemap.name}]: {tilemap.localBounds}");
                    sources.Add(BoxBoundSource(tilemap.localBounds));
                    flag = false;
                }
                int area = m_DefaultArea;
                var modifier = tilemap.GetComponent<NavMeshModifier>();
                if (modifier != null && modifier.overrideArea)
                {
                    area = modifier.area;
                }
                if (modifier != null && !modifier.ignoreFromBuild)
                {
                    CollectTileSources(sources, tilemap, area);
                }
                else
                {
                    var collider = tilemap.GetComponent<TilemapCollider2D>();
                    if (collider != null &&
                        (modifier == null || (modifier != null && !modifier.ignoreFromBuild)))
                    {
                        CollectTileSources(sources, tilemap, area);
                    }
                }
            }

            Debug.Log("Sources " + sources.Count);
        }

        private void CollectTileSources(List<NavMeshBuildSource> sources, Tilemap tilemap, int area)
        {
            var bound = tilemap.cellBounds;
            var vec3int = new Vector3Int(0, 0, 0);
            var size = new Vector3(tilemap.layoutGrid.cellSize.x, tilemap.layoutGrid.cellSize.y, tilemap.layoutGrid.cellSize.y);
            Mesh mesh = null;
            Quaternion rot = default;
            if (m_UseMeshPrefab != null)
            {
                mesh = m_UseMeshPrefab.GetComponent<MeshFilter>().sharedMesh;
                size = m_UseMeshPrefab.transform.localScale;
                rot = m_UseMeshPrefab.transform.rotation;
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
                    if (mesh != null)
                    {
                        var src = new NavMeshBuildSource();
                        src.transform = Matrix4x4.TRS(tilemap.GetCellCenterWorld(vec3int), rot, size);
                        src.shape = NavMeshBuildSourceShape.Mesh;
                        src.sourceObject = m_UseMeshPrefab.GetComponent<MeshFilter>().sharedMesh;
                        src.area = area;
                        sources.Add(src);
                    }
                    else
                    { 
                        var src = new NavMeshBuildSource();
                        src.transform = Matrix4x4.Translate(tilemap.GetCellCenterWorld(vec3int));
                        src.shape = NavMeshBuildSourceShape.Box;
                        src.size = size;
                        src.area = area;
                        sources.Add(src);
                    }
                }
            }
        }

        static Vector3 Abs(Vector3 v)
        {
            return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        }

        static Bounds GetWorldBounds(Matrix4x4 mat, Bounds bounds)
        {
            var absAxisX = Abs(mat.MultiplyVector(Vector3.right));
            var absAxisY = Abs(mat.MultiplyVector(Vector3.up));
            var absAxisZ = Abs(mat.MultiplyVector(Vector3.forward));
            var worldPosition = mat.MultiplyPoint(bounds.center);
            var worldSize = absAxisX * bounds.size.x + absAxisY * bounds.size.y + absAxisZ * bounds.size.z;
            return new Bounds(worldPosition, worldSize);
        }

        Bounds CalculateWorldBounds(List<NavMeshBuildSource> sources)
        {
            // Use the unscaled matrix for the NavMeshSurface
            Matrix4x4 worldToLocal = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            worldToLocal = worldToLocal.inverse;

            if (m_CollectObjects == CollectObjects2d.Grid)
            {
                var grid = FindObjectOfType<Grid>();
                var tilemap = grid.GetComponentInChildren<Tilemap>();
                if (tilemap == null)
                {
                    throw new NullReferenceException("Add at least one tilemap");
                }
                //Debug.Log($"From Local Bounds [{tilemap.name}]: {tilemap.localBounds}");
                var bounds = GetWorldBounds(worldToLocal , tilemap.localBounds);
                bounds.Expand(0.1f);
                //Debug.Log($"To World Bounds: {bounds}");
                return bounds;
            }

            var result = new Bounds();
            foreach (var src in sources)
            {
                switch (src.shape)
                {
                    case NavMeshBuildSourceShape.Mesh:
                    {
                        var m = src.sourceObject as Mesh;
                        result.Encapsulate(GetWorldBounds(worldToLocal * src.transform, m.bounds));
                        break;
                    }
                    case NavMeshBuildSourceShape.Terrain:
                    {
                        // Terrain pivot is lower/left corner - shift bounds accordingly
                        var t = src.sourceObject as TerrainData;
                        result.Encapsulate(GetWorldBounds(worldToLocal * src.transform, new Bounds(0.5f * t.size, t.size)));
                        break;
                    }
                    case NavMeshBuildSourceShape.Box:
                    case NavMeshBuildSourceShape.Sphere:
                    case NavMeshBuildSourceShape.Capsule:
                    case NavMeshBuildSourceShape.ModifierBox:
                        result.Encapsulate(GetWorldBounds(worldToLocal * src.transform, new Bounds(Vector3.zero, src.size)));
                        break;
                }
            }
            // Inflate the bounds a bit to avoid clipping co-planar sources
            result.Expand(0.1f);
            return result;
        }

        bool HasTransformChanged()
        {
            if (m_LastPosition != transform.position) return true;
            if (m_LastRotation != transform.rotation) return true;
            return false;
        }

        void UpdateDataIfTransformChanged()
        {
            if (HasTransformChanged())
            {
                RemoveData();
                AddData();
            }
        }

        private NavMeshBuildSource BoxBoundSource(Bounds localBounds)
        {
            var src = new NavMeshBuildSource();
            src.transform = Matrix4x4.Translate(localBounds.center);
            src.shape = NavMeshBuildSourceShape.Box;
            src.size = localBounds.size;
            src.area = 0;
            return src;
        }

#if UNITY_EDITOR
        bool UnshareNavMeshAsset()
        {
            // Nothing to unshare
            if (m_NavMeshData == null)
                return false;

            // Prefab parent owns the asset reference
            var isInPreviewScene = EditorSceneManager.IsPreviewSceneObject(this);
            var isPersistentObject = EditorUtility.IsPersistent(this);
            if (isInPreviewScene || isPersistentObject)
                return false;

            // An instance can share asset reference only with its prefab parent
            var prefab = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(this) as NavMeshSurface2d;
            if (prefab != null && prefab.navMeshData == navMeshData)
                return false;

            // Don't allow referencing an asset that's assigned to another surface
            for (var i = 0; i < s_NavMeshSurfaces.Count; ++i)
            {
                var surface = s_NavMeshSurfaces[i];
                if (surface != this && surface.m_NavMeshData == m_NavMeshData)
                    return true;
            }

            // Asset is not referenced by known surfaces
            return false;
        }

        void OnValidate()
        {
            if (UnshareNavMeshAsset())
            {
                Debug.LogWarning("Duplicating NavMeshSurface does not duplicate the referenced navmesh data", this);
                m_NavMeshData = null;
            }

            var settings = NavMesh.GetSettingsByID(m_AgentTypeID);
            if (settings.agentTypeID != -1)
            {
                // When unchecking the override control, revert to automatic value.
                const float kMinVoxelSize = 0.01f;
                if (!m_OverrideVoxelSize)
                    m_VoxelSize = settings.agentRadius / 3.0f;
                if (m_VoxelSize < kMinVoxelSize)
                    m_VoxelSize = kMinVoxelSize;

                // When unchecking the override control, revert to default value.
                const int kMinTileSize = 16;
                const int kMaxTileSize = 1024;
                const int kDefaultTileSize = 256;

                if (!m_OverrideTileSize)
                    m_TileSize = kDefaultTileSize;
                // Make sure tilesize is in sane range.
                if (m_TileSize < kMinTileSize)
                    m_TileSize = kMinTileSize;
                if (m_TileSize > kMaxTileSize)
                    m_TileSize = kMaxTileSize;
            }
        }
#endif
    }
}
