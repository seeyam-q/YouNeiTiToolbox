using System;
using UnityEngine;

namespace FortySevenE
{
    [Serializable]
    public struct TransformTRS
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public static TransformTRS GetTransformTRS(Transform transform, Space space = Space.Self)
        {
            var trs = new TransformTRS();
            switch (space)
            {
                case Space.Self:
                    trs.position = transform.localPosition;
                    trs.rotation = transform.localRotation;
                    trs.scale = transform.localScale;
                    break;
                case Space.World:
                    trs.position = transform.position;
                    trs.rotation = transform.rotation;
                    trs.scale = transform.localScale;
                    break;
            }

            return trs;
        }

        public void ApplyToTransform(Transform transform, Space space = Space.Self)
        {
            switch (space)
            {
                case Space.Self:
                    transform.localPosition = position;
                    transform.localRotation = rotation;
                    transform.localScale = scale;
                    break;
                case Space.World:
                    transform.position = position;
                    transform.rotation = rotation;
                    transform.localScale = scale;
                    break;
            }
        }
    }   
}