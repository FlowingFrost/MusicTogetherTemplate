using System;
using UnityEngine;

namespace MusicTogether.DancingLine
{
    public class Teleport : MonoBehaviour
    {
        public Transform target;

        private void Update()
        {
            transform.position = target.position;
        }
    }
}