using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Modified from https://github.com/keijiro/Klak
namespace FortySevenE
{
    public class SmoothFollow : MonoBehaviour
    {
        #region Editable attributes

        public enum Interpolator { Exponential, Spring, DampedSpring }

        public Transform target = null;
        public Interpolator interpolator = Interpolator.DampedSpring;
        [Header("Position")]
        public bool followPosX = true, followPosY= true, followPosZ = true;
        [Range(0, 20)] public float positionSpeed = 2;
        [Header("Rotation")]
        public bool followRotX = true, followRotY = true, followRotZ = true;
        [Range(0, 20)] public float rotationSpeed = 2;

        #endregion

        #region Public method

        public void Snap()
        {
            if (positionSpeed > 0) transform.position = target.position;
            if (rotationSpeed > 0) transform.rotation = target.rotation;
        }

        #endregion

        #region Private members

        Vector3 _vp;
        Vector4 _vr;

        #endregion

        #region MonoBehaviour implementation

        protected void Update()
        {
            if (target == null)
            {
                target = Camera.main?.transform;
            }

            if (target == null)
            {
                return;
            }
            
            var dt = UnityEngine.Time.deltaTime;

            if (positionSpeed > 0)
            {
                var p = transform.position;
                var pt = target.position;
                var sp = positionSpeed;

                if (interpolator == Interpolator.Exponential)
                {
                    p = Vector3.Lerp(pt, p, Mathf.Exp(sp * -dt));
                }
                else if (interpolator == Interpolator.Spring)
                {
                    _vp *= Mathf.Exp((1 + sp * 0.5f) * -dt);
                    _vp += (pt - p) * (sp * 0.1f);
                    p += _vp * dt;
                }
                else // interpolator == Interpolator.DampedSpring
                {
                    var n1 = _vp - (p - pt) * (sp * sp * dt);
                    var n2 = 1 + sp * dt;
                    _vp = n1 / (n2 * n2);
                    p += _vp * dt;
                }

                if (!followPosX)
                {
                    p.x = transform.position.x;
                }

                if (!followPosY)
                {
                    p.y = transform.position.y;
                }

                if (!followPosZ)
                {
                    p.z = transform.position.z;
                }

                transform.position = p;
            }
            else
            {
                var p = target.position;
                if (!followPosX)
                {
                    p.x = transform.position.x;
                }

                if (!followPosY)
                {
                    p.y = transform.position.y;
                }

                if (!followPosZ)
                {
                    p.z = transform.position.z;
                }

                transform.position = p;
            }

            if (rotationSpeed > 0)
            {
                Vector4 r = new Vector4(transform.rotation.x,
                    transform.rotation.y,
                    transform.rotation.z,
                    transform.rotation.w);
                Vector4 rt = new Vector4(target.rotation.x,
                    target.rotation.y,
                    target.rotation.z,
                    target.rotation.w);
                var sp = rotationSpeed;

                if (Vector4.Dot(r, rt) < 0) rt = -rt;

                if (interpolator == Interpolator.Exponential)
                {
                    r = Vector4.Lerp(rt, r, Mathf.Exp(sp * -dt));
                }
                else if (interpolator == Interpolator.Spring)
                {
                    _vr *= Mathf.Exp((1 + sp * 0.5f) * -dt);
                    _vr += (rt - r) * (sp * 0.1f);
                    r += _vr * dt;
                }
                else // interpolator == Interpolator.DampedSpring
                {
                    var n1 = _vr - (r - rt) * (sp * sp * dt);
                    var n2 = 1 + sp * dt;
                    _vr = n1 / (n2 * n2);
                    r += _vr * dt;
                }
                
                var result = (Quaternion.Normalize(new Quaternion(r.x, r.y, r.z, r.w))).eulerAngles;
                if (!followRotX)
                {
                    result.x = transform.eulerAngles.x;
                }

                if (!followRotY)
                {
                    result.y = transform.eulerAngles.y;
                }

                if (!followRotZ)
                {
                    result.z = transform.eulerAngles.z;
                }

                transform.eulerAngles = result;
            }
            else
            {
                var result = target.eulerAngles;
                if (!followRotX)
                {
                    result.x = transform.eulerAngles.x;
                }

                if (!followRotY)
                {
                    result.y = transform.eulerAngles.y;
                }

                if (!followRotZ)
                {
                    result.z = transform.eulerAngles.z;
                }
                transform.eulerAngles = result;
            }
        }

        #endregion
    }
}