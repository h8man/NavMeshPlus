using System;
using System.Collections.Generic;
using UnityEngine;

namespace NavMeshComponents.Extensions
{
    public class NavMeshBuilderState
    {
        public Matrix4x4 worldToLocal;
        public Bounds result;
        public IEnumerable<GameObject> roots;
        
    }
}