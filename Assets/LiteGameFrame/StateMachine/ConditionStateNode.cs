using System.Collections.Generic;
using UnityEngine;

namespace LiteGameFrame.StateMachine
{
    public class ConditionStateNode : BaseStateNode
    {
        public string LaunchCondition { get; private set; }
        private List<string> completedPredecessorIds = new List<string>();
    }
}