using System;
using System.Collections.Generic;
using MusicTogether.DancingLine.Core;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace MusicTogether.DancingLine.Basic
{
    public class TapRecorder : SerializedMonoBehaviour
    {
        public BaseLinePool LinePool;
        public List<Tuple<double,IDirection>> TapTimes = new ();

        [Button]
        public void Record()
        {
            foreach (var node in LinePool.lineNodes)
            {
                TapTimes.Add(new Tuple<double, IDirection>(node.BeginTime, node.Direction));
            }
            TapTimes.Sort();
        }

        [Button]
        public void Apply()
        {
            foreach (var time in TapTimes)
            {
                LinePool.AddNode(time.Item1,time.Item2);
            }
        }
    }
}