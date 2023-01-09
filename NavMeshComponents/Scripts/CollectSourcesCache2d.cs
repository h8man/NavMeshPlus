using NavMeshPlus.Components;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace NavMeshPlus.Extensions
{
    [ExecuteAlways]
    [AddComponentMenu("Navigation/NavMesh CacheSources2d", 30)]
    public class CollectSourcesCache2d : NavMeshExtension
    {
        List<NavMeshBuildSource> _sources;
        Dictionary<UnityEngine.Object, NavMeshBuildSource> _lookup;
        private Bounds _sourcesBounds;
        public bool IsDirty { get; protected set; }

        private NavMeshBuilder2dState _state;

        public int SourcesCount => _sources.Count;
        public int CahcheCount => _lookup.Count;

        public List<NavMeshBuildSource> Cache { get => _sources; }

        protected override void Awake()
        {
            _lookup = new Dictionary<UnityEngine.Object, NavMeshBuildSource>();
            _sources = new List<NavMeshBuildSource>();
            IsDirty = false;
            Order = -1000;
            _sourcesBounds = new Bounds();
            base.Awake();
        }

        public bool AddSource(GameObject gameObject, NavMeshBuildSource source)
        {
            var res = _lookup.ContainsKey(gameObject);
            if (res)
            {
                return UpdateSource(gameObject);
            }
            _sources.Add(source);
            _lookup.Add(gameObject, source);
            IsDirty = true;
            return true;
        }
        public bool UpdateSource(GameObject gameObject)
        {
            var res = _lookup.ContainsKey(gameObject);
            if(res)
            {
                IsDirty = true;
                var source = _lookup[gameObject];
                var idx = _sources.IndexOf(source);
                if (idx >= 0)
                {
                    source.transform = Matrix4x4.TRS(gameObject.transform.position, gameObject.transform.rotation, gameObject.transform.lossyScale);
                    _sources[idx] = source;
                    _lookup[gameObject] = source;
                }
            }
            return res;
        }

        public bool RemoveSource(GameObject gameObject)
        {
            var res = _lookup.ContainsKey(gameObject);
            if (res)
            {
                IsDirty = true;
                var source = _lookup[gameObject];
                _lookup.Remove(gameObject);
                _sources.Remove(source);
            }
            return res;
        }

        public AsyncOperation UpdateNavMesh(NavMeshData data)
        {
            IsDirty = false;
            return NavMeshBuilder.UpdateNavMeshDataAsync(data, NavMeshSurfaceOwner.GetBuildSettings(), _sources, _sourcesBounds);
        }
        public AsyncOperation UpdateNavMesh()
        {
            return UpdateNavMesh(NavMeshSurfaceOwner.navMeshData);
        }
        public override void CollectSources(NavMeshSurface surface, List<NavMeshBuildSource> sources, NavMeshBuilderState navMeshState)
        {
            _lookup.Clear();
            IsDirty = false;
            _state = navMeshState.GetExtraState<NavMeshBuilder2dState>();
            _state.lookupCallback = LookupCallback;
        }

        private void LookupCallback(UnityEngine.Object component, NavMeshBuildSource source)
        {
            if (component == null)
            {
                return;
            }
            _lookup.Add(component, source);
        }

        public override void PostCollectSources(NavMeshSurface surface, List<NavMeshBuildSource> sources, NavMeshBuilderState navNeshState)
        {
            _sourcesBounds = navNeshState.worldBounds;
            _sources = sources;
        }
    }
}
