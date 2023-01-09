using UnityEngine;
using UnityEngine.AI;

namespace NavMeshPlus.Extensions
{
    public class AgentRotate2d: MonoBehaviour
    {
        private AgentOverride2d override2D;
        private void Start()
        {
            override2D = GetComponent<AgentOverride2d>();
            override2D.agentOverride = new RotateAgentInstantly(override2D.Agent, override2D);
        }

    }
}
