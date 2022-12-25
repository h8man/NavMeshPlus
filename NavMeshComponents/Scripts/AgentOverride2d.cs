using UnityEngine;
using UnityEngine.AI;

namespace NavMeshComponents.Extensions
{
    public interface IAgentOverride
    {
        void UpdateAgent();
    }

    public class AgentDefaultOverride : IAgentOverride
    {
        public void UpdateAgent()
        {
        }
    }
    public class AgentOverride2d: MonoBehaviour
    {
        public NavMeshAgent Agent { get; private set; }
        public IAgentOverride agentOverride { get; set; }
        private void Awake()
        {
            Agent = GetComponent<NavMeshAgent>();
        }
        private void Start()
        {
            Agent.updateRotation = false;
            Agent.updateUpAxis = false;
        }

        private void Update()
        {
            agentOverride?.UpdateAgent();
        }
    }
}
