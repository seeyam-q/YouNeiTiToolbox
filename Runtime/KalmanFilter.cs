using System.Collections.Generic;
using UnityEngine;

namespace FortySevenE
{
    /// <summary>A Kalman filter implementation for <c>float</c> values.</summary>
    public class KalmanFilterFloat
    {
        public float LastEstimatedResult => x;
        public float RawMeasurement => _rawMeasurement;

        //-----------------------------------------------------------------------------------------
        // Constants:
        //-----------------------------------------------------------------------------------------

        public const float DEFAULT_STEP = 0.000001f;
        public const float DEFAULT_COVARIANCE = 0.01f;

        public const float DEFAULT_P = 1;

        //-----------------------------------------------------------------------------------------
        // Private Fields:
        //-----------------------------------------------------------------------------------------

        private float step;
        private float covariance;
        private float p = DEFAULT_P;
        private float x;
        private float k;

        private float _rawMeasurement;

        //-----------------------------------------------------------------------------------------
        // Constructors:
        //-----------------------------------------------------------------------------------------

        // N.B. passing in DEFAULT_Q is necessary, even though we have the same value (as an optional parameter), because this
        // defines a parameterless constructor, allowing us to be new()'d in generics contexts.
        public KalmanFilterFloat() : this(DEFAULT_STEP) { }

        public KalmanFilterFloat(float aQ = DEFAULT_STEP, float aR = DEFAULT_COVARIANCE)
        {
            step = aQ;
            covariance = aR;
        }

        //-----------------------------------------------------------------------------------------
        // Public Methods:
        //-----------------------------------------------------------------------------------------

        public float Update(float measurement, float? newStep = null, float? newCovariance = null)
        {
            _rawMeasurement = measurement;

            // update values if supplied.
            if (newStep != null && step != newStep)
            {
                step = (float)newStep;
            }
            if (newCovariance != null && covariance != newCovariance)
            {
                covariance = (float)newCovariance;
            }

            // update measurement.
            {
                k = (p + step) / (p + step + covariance);
                p = covariance * (p + step) / (covariance + p + step);
            }

            // filter result back into calculation.
            float result = x + (_rawMeasurement - x) * k;
            x = result;
            return result;
        }

        public float Update(List<float> measurements, bool areMeasurementsNewestFirst = false, float? newStep = null, float? newCovaraince = null)
        {

            float result = 0;
            int i = (areMeasurementsNewestFirst) ? measurements.Count - 1 : 0;

            while (i < measurements.Count && i >= 0)
            {

                // decrement or increment the counter.
                if (areMeasurementsNewestFirst)
                {
                    --i;
                }
                else
                {
                    ++i;
                }

                result = Update(measurements[i], newStep, newCovaraince);
            }

            return result;
        }

        public void Reset()
        {
            p = 1;
            x = 0;
            k = 0;
        }
    }

    /// <summary>A Kalman filter implementation for <c>Vector3</c> values.</summary>
    public class KalmanFilterVector3
    {
        public Vector3 LastEstimatedResult => x;

        //-----------------------------------------------------------------------------------------
        // Constants:
        //-----------------------------------------------------------------------------------------

        public const float DEFAULT_STEP = 0.000001f;
        public const float DEFAULT_COVARIANCE = 0.01f;

        public const float DEFAULT_P = 1;

        //-----------------------------------------------------------------------------------------
        // Private Fields:
        //-----------------------------------------------------------------------------------------

        private float step;
        private float covariance;
        private float p = DEFAULT_P;
        private Vector3 x;
        private float k;

        //-----------------------------------------------------------------------------------------
        // Constructors:
        //-----------------------------------------------------------------------------------------

        // N.B. passing in DEFAULT_Q is necessary, even though we have the same value (as an optional parameter), because this
        // defines a parameterless constructor, allowing us to be new()'d in generics contexts.
        public KalmanFilterVector3() : this(DEFAULT_STEP)
        {
        }

        public KalmanFilterVector3(float aQ = DEFAULT_STEP, float aR = DEFAULT_COVARIANCE)
        {
            step = aQ;
            covariance = aR;
        }

        //-----------------------------------------------------------------------------------------
        // Public Methods:
        //-----------------------------------------------------------------------------------------

        public Vector3 Update(Vector3 measurement, float? newStep = null, float? newCovariance = null)
        {

            // update values if supplied.
            if (newStep != null && step != newStep)
            {
                step = (float)newStep;
            }

            if (newCovariance != null && covariance != newCovariance)
            {
                covariance = (float)newCovariance;
            }

            // update measurement.
            {
                k = (p + step) / (p + step + covariance);
                p = covariance * (p + step) / (covariance + p + step);
            }

            // filter result back into calculation.
            Vector3 result = x + (measurement - x) * k;
            x = result;
            return result;
        }

        public Vector3 Update(List<Vector3> measurements, bool areMeasurementsNewestFirst = false, float? newStep = null,
            float? newCovariance = null)
        {

            Vector3 result = Vector3.zero;
            int i = (areMeasurementsNewestFirst) ? measurements.Count - 1 : 0;

            while (i < measurements.Count && i >= 0)
            {

                // decrement or increment the counter.
                if (areMeasurementsNewestFirst)
                {
                    --i;
                }
                else
                {
                    ++i;
                }

                result = Update(measurements[i], newStep, newCovariance);
            }

            return result;
        }

        public void Reset()
        {
            p = 1;
            x = Vector3.zero;
            k = 0;
        }
    }
}