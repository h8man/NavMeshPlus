namespace UnityEngine.AI {

    [AddComponentMenu("Navigation/NavMeshGrid", 60)]
    public class NavMeshGrid : MonoBehaviour {

        [SerializeField]
        [Tooltip("Instead of using Awake to register OnEnable will be used")]
        private bool m_registerOnEnable = default;

        [SerializeField]
        [Tooltip("Instead of using OnDestroy to unregister OnDisable will be used")]
        private bool m_unregisterOnDisable = default;

        private void Awake() {
            if (!m_registerOnEnable)
                NavMeshBuilder2d.RegisterGrid(gameObject);
        }

        private void OnEnable() {
            if (m_registerOnEnable)
                NavMeshBuilder2d.RegisterGrid(gameObject);
        }

        private void OnDisable() {
            if (m_unregisterOnDisable)
                NavMeshBuilder2d.UnregisterGrid(gameObject);
        }

        private void OnDestroy() {
            if (!m_unregisterOnDisable)
                NavMeshBuilder2d.UnregisterGrid(gameObject);
        }

    }
}