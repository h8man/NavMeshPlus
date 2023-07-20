using UnityEngine;

namespace NavMeshPlus.Extensions
{
    class AgentRotateSmooth2d: MonoBehaviour
    {
        public float angularSpeed;
        private AgentOverride2d override2D;

        private void Start()
        {
            override2D = GetComponent<AgentOverride2d>();
            override2D.agentOverride = new RotateAgentSmoothly(override2D.Agent, override2D, angularSpeed);
        }
    }
}
