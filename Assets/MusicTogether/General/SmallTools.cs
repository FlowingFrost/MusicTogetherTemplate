using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MusicTogether.General
{
    public enum Vector3Direction{Up,Down,Left,Right,Forward,Back}
    public static class Vector3Extensions
    {
        public static Vector3 Round(this Vector3 vector)
        {
            return new Vector3(
                Mathf.Round(vector.x),
                Mathf.Round(vector.y),
                Mathf.Round(vector.z)
            );
        }

        public static Vector3 RoundMax(this Vector3 vector)
        {
            if (vector.x > vector.y && vector.x > vector.z)
                return new Vector3(1, vector.y, vector.z);
            if (vector.y > vector.z)
                return new Vector3(vector.x, 1, vector.z);
            return new Vector3(vector.x, vector.y, 1);
        }

        public static Vector3 Floor(this Vector3 vector)
        {
            return new Vector3(
                Mathf.Floor(vector.x),
                Mathf.Floor(vector.y),
                Mathf.Floor(vector.z)
            );
        }
        public static Vector3 Ceil(this Vector3 vector)
        {
            return new Vector3(
                Mathf.Ceil(vector.x),
                Mathf.Ceil(vector.y),
                Mathf.Ceil(vector.z)
            );
        }

        public static Vector3 ToEulerAngles(this Vector3 vector)
        {
            vector = vector.normalized;
            // 计算俯仰角（Pitch，绕X轴）
            float pitch = -Mathf.Asin(vector.y) * Mathf.Rad2Deg;
            // 计算偏航角（Yaw，绕Y轴）
            float yaw = Mathf.Atan2(vector.x, vector.z) * Mathf.Rad2Deg;
            // 返回欧拉角（Roll默认为0）
            return new Vector3(pitch, yaw, 0f);
        }
    }
}
