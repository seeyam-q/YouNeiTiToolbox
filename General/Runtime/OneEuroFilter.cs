using System.Collections.Generic;
#if BURST_PRESENT
using Unity.Burst;
#else
using System.Runtime.CompilerServices;
#endif
using Unity.Mathematics;
using UnityEngine;

namespace FortySevenE
{
public class OneEuroFilterVector3
    {
        Vector3 m_LastRawValue;
        public Vector3 LastRawValue => m_LastRawValue;
        Vector3 m_LastFilteredValue;
        public Vector3 LastFilteredValue => m_LastFilteredValue;
        readonly float m_MinCutoff;
        readonly float m_Beta;

        /// <summary>
        /// Initializes a new instance of the <see cref="OneEuroFilterVector3"/> with specified cutoff and beta values.
        /// </summary>
        /// <param name="initialRawValue">The initial raw value for the filter.</param>
        /// <param name="minCutoff">The minimum cutoff value for the filter. Default is 0.1f.</param>
        /// <param name="beta">The beta value for the filter. Default is 0.02f.</param>
        /// <remarks>
        /// Filter parameters:
        /// <list type="bullet">
        /// <item>
        /// <term><paramref name="minCutoff"/></term>
        /// <description>
        /// Controls the amount of smoothing at low speeds. A smaller value will introduce
        /// more smoothing and potential lag, helping to reduce low-frequency jitter. A larger value
        /// may feel more responsive but can let through more jitter. It's advised to start with a
        /// value around 0.1 for masking jitter in movements of about 1 cm.
        /// </description>
        /// </item>
        /// <item>
        /// <term><paramref name="beta"/></term>
        /// <description>
        /// Determines the filter's adjustment to speed changes. A smaller value provides consistent
        /// smoothing, while a larger one introduces more aggressive adjustments for speed changes, offering
        /// responsive filtering at high speeds. A starting value of 0.02 is recommended, but fine-tuning
        /// might be necessary based on specific use cases.
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <seealso cref="Initialize"/>
        public OneEuroFilterVector3(Vector3 initialRawValue, float minCutoff = 0.1f, float beta = 0.02f)
        {
            m_LastRawValue = initialRawValue;
            m_LastFilteredValue = initialRawValue;
            m_MinCutoff = minCutoff;
            m_Beta = beta;
        }

        /// <summary>
        /// Resets the initial raw value. Useful to recover from tracking loss.
        /// </summary>
        /// <param name="initialRawValue">Raw value to reset filtering basis to.</param>
        public void Initialize(Vector3 initialRawValue)
        {
            m_LastRawValue = initialRawValue;
            m_LastFilteredValue = initialRawValue;
        }

        /// <summary>
        /// Filters the given <see cref="Vector3"/> rawValue using the internal minCutoff and beta parameters.
        /// </summary>
        /// <param name="rawValue">The raw <see cref="Vector3"/> value to be filtered.</param>
        /// <param name="deltaTime">The time since the last filter update.</param>
        /// <returns>The filtered <see cref="Vector3"/> value.</returns>
        public Vector3 Filter(Vector3 rawValue, float deltaTime)
        {
            return Filter(rawValue, deltaTime, m_MinCutoff, m_Beta);
        }

        /// <summary>
        /// Filters the given <see cref="Vector3"/> rawValue using provided minCutoff and beta parameters.
        /// This method computes the speed of change in the signal and dynamically adjusts the amount of smoothing
        /// based on the speed and the provided minCutoff and beta values.
        /// </summary>
        /// <param name="rawValue">The raw <see cref="Vector3"/> value to be filtered.</param>
        /// <param name="deltaTime">The time since the last filter update.</param>
        /// <param name="minCutoff">The minimum cutoff value for the filter. Influences the amount of smoothing at low speeds.</param>
        /// <param name="beta">Determines the filter's adjustment to speed changes, influencing its responsiveness.</param>
        /// <returns>The filtered <see cref="Vector3"/> value.</returns>
        public Vector3 Filter(Vector3 rawValue, float deltaTime, float minCutoff, float beta)
        {
            // Calculate speed as a Vector3
            Vector3 speed = (rawValue - m_LastRawValue) / deltaTime;

            // Compute cutoffs for x, y, and z
            Vector3 cutoffs = new Vector3(minCutoff, minCutoff, minCutoff);
            Vector3 betaValues = new Vector3(beta, beta, beta);

            // Incorporate speed into the cutoffs
            Vector3 combinedCutoffs = cutoffs + Vector3.Scale(betaValues, speed);

            // Compute alpha for x, y, and z
            FastSafeDivide(Vector3.one, Vector3.one + combinedCutoffs, out Vector3 alpha);

            Vector3 rawFiltered = Vector3.Scale(alpha, rawValue);
            Vector3 lastFiltered = Vector3.Scale(Vector3.one - alpha, m_LastFilteredValue);

            // Calculate the final filtered value
            Vector3 filteredValue = rawFiltered + lastFiltered;

            m_LastRawValue = rawValue;
            m_LastFilteredValue = filteredValue;

            return filteredValue;
        }
        
        /// <summary>
        /// Performs a safe division of two Vector3s. If the difference between any corresponding pair of components in the vectors exceeds a specified tolerance, the division is carried out for that component. 
        /// </summary>
        /// <param name="a">The dividend Vector3.</param>
        /// <param name="b">The divisor Vector3.</param>
        /// <param name="result">The resulting Vector3 after division. If the difference between the corresponding components of the dividend and divisor is less than the tolerance, the respective component in the result vector remains zero.</param>
        /// <param name="tolerance">The tolerance for the component-wise division operation. Defaults to 0.000001f.</param>
#if BURST_PRESENT
        [BurstCompile]
#endif
        private void FastSafeDivide(in Vector3 a, in Vector3 b, out Vector3 result, float tolerance = 0.000001f)
        {
          FastSafeDivide(a, b, out float3 float3Result, tolerance);
          result = float3Result;
        }
        
        /// <summary>
        /// Performs a safe division of two float3 vectors. If the difference between any corresponding pair of components in the vectors exceeds a specified tolerance, the division is carried out for that component. 
        /// </summary>
        /// <param name="a">The dividend float3 vector.</param>
        /// <param name="b">The divisor float3 vector.</param>
        /// <param name="result">The resulting float3 vector after division. If the difference between the corresponding components of the dividend and divisor is less than the tolerance, the respective component in the result vector remains zero.</param>
        /// <param name="tolerance">The tolerance for the component-wise division operation. Defaults to 0.000001f.</param>
#if BURST_PRESENT
        [BurstCompile]
#endif
        private void FastSafeDivide(in float3 a, in float3 b, out float3 result, float tolerance = 0.000001f)
        {
          result = new float3();
          if (math.abs(a.x - b.x) > tolerance)
            result.x = a.x / b.x;
          if (math.abs(a.y - b.y) > tolerance)
            result.y = a.y / b.y;
          if (math.abs(a.z - b.z) > tolerance)
            result.z = a.z / b.z;
        }
    }
}