using System.Collections;
using System.Collections.Generic;
using AD.Utility;
using UnityEngine;

namespace AD.Experimental.Runtime
{
    public class BehaviourCore : MonoBehaviour
    {
        #region Timeline Supporting

        public string MatchBehaviour;
        public BehaviourTree MatchBehaviourTree;

#if UNITY_EDITOR
        public BehaviourClip CurrentR;
#endif

        #region Mono

        #endregion

        public void StartBehaviourByInitTime(string key,float initTime)
        {
            if(MatchBehaviourTree.TryGetValue(new(key),out var result))
            {
                MatchBehaviour = key;
                result.UpdateBeviour(this, initTime);
            }
        }

        public void StartBehaviour(string key)
        {
            if (MatchBehaviourTree.TryGetValue(new(key), out var result))
            {
                MatchBehaviour = key;
                result.UpdateBeviour(this);
            }
        }

        public void StartBehaviourByTimeLine(string message)
        {
            var strs = message.Split(',');
            StartBehaviourByInitTime(strs[0], float.Parse(strs[1]));
        }

        #endregion

        #region Animation Supporting



        #endregion

    }
}
