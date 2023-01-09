using System;
using System.Collections.Generic;
using UnityEngine;

namespace NavMeshPlus.Extensions
{
    public class NavMeshBuilderState
    {
        public Matrix4x4 worldToLocal;
        public Bounds worldBounds;
        public IEnumerable<GameObject> roots;
        public T GetExtraState<T>() where T : class, new()
        {
            if (mExtraState == null)
                mExtraState = new Dictionary<Type, System.Object>();
            if (!mExtraState.TryGetValue(typeof(T), out System.Object extra))
                extra = mExtraState[typeof(T)] = new T();
            return extra as T;
        }
        private Dictionary<Type, System.Object> mExtraState;
    }
}