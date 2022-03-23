using System;
using System.Collections.Generic;
using UnityEngine;

namespace NavMeshComponents.Extensions
{
    public class NavMeshBuilderState
    {
        internal Matrix4x4 worldToLocal;
        internal Bounds result;
    }
}