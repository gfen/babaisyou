using UnityEngine;

namespace Gfen.Game.Utility
{
    public static class TransformExtension
    {
        public static void Reset(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
    }
}
