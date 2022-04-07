using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace NavMeshComponents.Extensions
{
    [ExecuteAlways]
    [AddComponentMenu("Navigation/NavMeshCollectRootSources2d", 30)]
    public class NavMeshCollectRootSources2d: NavMeshExtension
    {
        [SerializeField]
        private List<GameObject> _rootSources;

        public List<GameObject> RooySources { get => _rootSources; set => _rootSources = value; }

        protected override void Awake()
        {
            Order = -100;
            base.Awake();
        }

        public override void CollectSources(NavMeshSurface surface, List<NavMeshBuildSource> sources, NavMeshBuilderState navNeshState)
        {
            navNeshState.roots = _rootSources;
        }
    }
}
