using LiteDebugTool;
using UnityEngine;

namespace LiteGameFrame.StateMachine
{
    public class EmptyState : IState
    {
        public string Description => "Empty State, It's just used to check";
        private IStateNode stateNode;
        private bool completed;
        public void Enter(IStateNode stateNode)
        {
            this.stateNode = stateNode;
            Debug.Log("Entering Empty State");
        }

        public void Update()
        {
            Debug.Log("Updating Empty State");
            if (stateNode != null)
            {
                completed = true;
                stateNode.OnComplete();
            }
            else
                Debug.LogWarning("StateNode is null");
        }

        public void Exit()
        {
            if (completed)
            {
                Debug.Log("Exiting Empty State");
            }
            else
            {
                Debug.LogWarning("Unexpected Exit");
            }
            stateNode = null;
        }
    }
}