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
        public Space SetSpace { get; private set; }

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

            trs.SetSpace = space;

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
                    //https://discussions.unity.com/t/reading-and-setting-an-objects-global-scale-with-transform-functions/143857/3
                    transform.localScale = Vector3.one;
                    transform.localScale = new Vector3 (scale.x/transform.lossyScale.x, scale.y/transform.lossyScale.y, scale.z/transform.lossyScale.z);
                    break;
            }
        }
    }   
}