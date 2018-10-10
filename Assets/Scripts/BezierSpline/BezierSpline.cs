using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bezier
{
    public class BezierSpline : MonoBehaviour, IBezierSpline
    {
        [SerializeField]
        public List<Vector3> points;

        [SerializeField]
        private List<BezierControlPointMode> modes;

        [SerializeField]
        private bool LoopField;

        [SerializeField]
        private bool AutoSetControlPointsField;

        [SerializeField]
        private float AutoSetStrengthField;

        public bool Loop
        {
            get { return LoopField; }
            set
            {
                if (Loop != value)
                {
                    LoopField = value;

                    if (value)
                    {

                        points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);
                        points.Add(points[0] * 2 - points[1]);
                        //modes[modes.Count - 1] = modes[0];
                        SetControlPoint(0, points[0]);

                        if (AutoSetControlPoints)
                        {
                            modes.Add(BezierControlPointMode.Aligned);
                            AutoSetAnchorControlPoints(0);
                            AutoSetAnchorControlPoints(points.Count - 3);
                        }
                        else
                        {
                            modes.Add(BezierControlPointMode.Free);
                        }

                    }
                    else
                    {
                        points.RemoveRange(points.Count - 2, 2);

                        if (AutoSetControlPoints)
                            AutoSetStartAndEndControlPoints();
                    }
                }
            }
        }

        public bool AutoSetControlPoints
        {
            get { return AutoSetControlPointsField; }
            set
            {
                if (AutoSetControlPointsField != value)
                {
                    AutoSetControlPointsField = value;
                    if (AutoSetControlPointsField)
                    {
                        AutoSetAllControlPoints();
                    }
                }
            }
        }

        public float AutoSetStrength
        {
            get { return AutoSetStrengthField; }
            set
            {
                AutoSetStrengthField = value;
                AutoSetAllControlPoints();
            }
        }

        public int PointCount
        {
            get { return points.Count; }
        }

        private List<Vector3> EvenlySpacedPoints;

        // Methods
        public void Reset()
        {
            points = new List<Vector3>
            {
                new Vector3(1f, 0f, 0f),
                //new Vector3(2f, 0f, 0f),
                //new Vector3(3f, 0f, 0f),
                //new Vector3(4f, 0f, 0f),
            };

            modes = new List<BezierControlPointMode>
            {
                BezierControlPointMode.Free,
                //BezierControlPointMode.Free
            };
        }

        /// <summary>
        /// Get point on spline.
        /// </summary>
        /// <param name="t"> time </param>
        public Vector3 GetPoint(float t)
        {
            int i;
            if (t >= 1f)
            {
                t = 1f;
                i = points.Count - 4;
            }
            else
            {
                t = Mathf.Clamp01(t) * CurveCount;
                i = (int)t;
                t -= i;
                i *= 3;
            }

            return transform.TransformPoint(Bezier.GetPoint(points[i], points[i + 1], points[i + 2], points[LoopIndex(i + 3)], t));
        }

        public Vector3 GetVelocity(float t)
        {
            int i;
            if (t >= 1f)
            {
                t = 1f;
                i = points.Count - 4;
            }
            else
            {
                t = Mathf.Clamp01(t) * CurveCount;
                i = (int)t;
                t -= i;
                i *= 3;
            }
            return transform.TransformPoint(Bezier.GetFirstDerivative(points[i], points[i + 1], points[i + 2],
                       points[i + 3], t)) -
                   transform.position;
        }

        public Vector3 GetDirection(float t)
        {
            return GetVelocity(t).normalized;
        }

        public void AddPoint()
        {
            Vector3 point = points[points.Count - 1];

            point.x += 2f;
            points.Add(point);
            point.x += 2f;
            points.Add(point);
            point.x += 2f;
            points.Add(point);

            if (AutoSetControlPointsField)
            {
                modes.Add(BezierControlPointMode.Aligned);
                AutoSetAllAffectedControlPoints(points.Count - 1);
            }
            else
            {
                modes.Add(modes[modes.Count - 1]);
                EnforceMode(points.Count - 4);
            }

            if (LoopField)
            {
                AutoSetAllControlPoints();
                //points[points.Count - 1] = points[0];
                //modes[modes.Count - 1] = modes[0];
                //EnforceMode(0);
            }
        }

        public void AddPoint(Vector3 newAnchorPos)
        {
            Vector3 point = points[points.Count - 1];

            point = newAnchorPos + Vector3.right * 2;
            points.Add(point);
            //point = newAnchorPos + Vector3.right * 2;
            points.Add(point);

            point = newAnchorPos;
            points.Add(point);

            if (AutoSetControlPointsField)
            {
                modes.Add(BezierControlPointMode.Aligned);
                AutoSetAllControlPoints();
            }
            else
            {
                modes.Add(modes[modes.Count - 1]);
                EnforceMode(points.Count - 4);
            }

            if (LoopField)
            {
                AutoSetAllControlPoints();
            }
        }

        public int CurveCount
        {
            get
            {
                int value = (points.Count - 1) / 3;
                if (Loop)
                    value += 1;
                return value;
            }
        }

        // Control Point Methods
        public int ControlPointCount
        {
            get { return points.Count; }
        }

        /// <summary>
        /// Get any control point on spline.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Vector3 GetControlPoint(int index)
        {
            return points[index];
        }

        public void SetControlPoint(int index, Vector3 point)
        {
            if (index % 3 == 0)
            {
                Vector3 delta = point - points[index];
                if (LoopField)
                {
                    if (index == 0)
                    {
                        points[1] += delta;
                        points[points.Count - 2] += delta;
                        points[points.Count - 1] = point;
                    }
                    else if (index == points.Count - 1)
                    {
                        points[0] = point;
                        points[1] += delta;
                        points[index - 1] += delta;
                    }
                    else
                    {
                        points[index - 1] += delta;
                        points[index + 1] += delta;
                    }
                }
                else
                {
                    if (index > 0)
                        points[index - 1] += delta;

                    if (index + 1 < points.Count)
                        points[index + 1] += delta;
                }

            }
            points[index] = point;
            EnforceMode(index);

            if (AutoSetControlPoints && index % 3 == 0)
                AutoSetAllAffectedControlPoints(index);
        }

        // Mode control methods
        public BezierControlPointMode GetControlPointMode(int index)
        {
            return modes[(index + 1) / 3];
        }

        public void SetControlPointMode(int index, BezierControlPointMode mode)
        {
            int modeIndex = GetModeIndex(index);
            modes[modeIndex] = mode;
            if (LoopField)
            {
                if (modeIndex == 0)
                    modes[modes.Count - 1] = mode;
                else if (modeIndex == modes.Count - 1)
                {
                    modes[0] = mode;
                }
            }

            EnforceMode(index);
        }

        private void EnforceMode(int index)
        {
            int modeIndex = GetModeIndex(index);
            BezierControlPointMode mode = modes[modeIndex];
            if (mode == BezierControlPointMode.Free || !LoopField && (modeIndex == 0 || modeIndex == modes.Count - 1))
                return;

            int middleIndex = modeIndex * 3;
            int fixedIndex, enforcedIndex;
            if (index <= middleIndex)
            {
                fixedIndex = middleIndex - 1;

                if (fixedIndex < 0)
                    fixedIndex = points.Count - 2;

                enforcedIndex = middleIndex + 1;

                if (enforcedIndex >= points.Count)
                    enforcedIndex = 1;
            }
            else
            {
                fixedIndex = middleIndex + 1;

                if (fixedIndex >= points.Count)
                    fixedIndex = 1;

                enforcedIndex = middleIndex - 1;

                if (enforcedIndex < 0)
                    enforcedIndex = points.Count - 2;
            }

            Vector3 middle = points[middleIndex];
            Vector3 enforcedTangent = middle - points[fixedIndex];
            if (mode == BezierControlPointMode.Aligned)
            {
                enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, points[enforcedIndex]);
            }
            points[enforcedIndex] = middle + enforcedTangent;
        }

        private int GetModeIndex(int index)
        {
            return (index + 1) / 3;
        }

        // Own Methods

        public void DeleteAnchorPoint(int index)
        {
            if (points.Count <= 1)
                return;

            modes.RemoveAt((index + 1) / 3); // at mode index

            if (index == 0)
            {
                if (LoopField)
                {
                    points[points.Count - 1] = points[2];
                }
                points.RemoveRange(0, 3);
            }
            else if (index == points.Count - 1 && !LoopField)
                points.RemoveRange(index - 2, 3);
            else
                points.RemoveRange(index - 1, 3);

            if (AutoSetControlPoints)
                AutoSetAllAffectedControlPoints(index);
        }

        public void AddCurveInBetween(int index)
        {
            Vector3 point = points[index];
            int modeIndex = GetModeIndex(index);

            point.x += 2f;
            points.Insert(index + 2, point);
            point.x += 2f;
            points.Insert(index + 3, point);
            point.x += 2f;
            points.Insert(index + 4, point);

            if (AutoSetControlPoints)
            {
                modes.Insert(modeIndex, BezierControlPointMode.Aligned);
                AutoSetAllAffectedControlPoints(index + 3);
            }
            else
            {
                modes.Insert(modeIndex, modes[modeIndex]);
                AutoSetAnchorControlPoints(index + 3);
                EnforceMode(index + 2);
            }

        }

        public Vector3[] GetControlPointsInCurve(int i) // return points from 0 to 3 & 4 to 7 and so on...
        {
            return new Vector3[]
            {
                points[i*3], points[i*3 + 1], points[i*3 + 2], points[LoopIndex(i*3 + 3)]
            };
        }

        // Cached collections
        List<Vector3> controlPoints = new List<Vector3>();
        List<float> curveLengths = new List<float>();
        public List<Vector3> CalculateEvenlySpacedPoints(float spacing, float resolution = 1, int startCurve = 0, int endCurve = -1)
        {
            if (endCurve == -1)
                endCurve = CurveCount;
            if (endCurve < startCurve)
                throw new Exception("endCurve value can't be smaller than startCurve");
            if (startCurve > CurveCount)
                throw new Exception("startCurve is bigger than curvecount");

            int numControlPoints = (endCurve - startCurve) * 3 + 1;
            if (controlPoints.Capacity < numControlPoints)
                controlPoints = new List<Vector3>(numControlPoints);
            for (int i = 0; i < numControlPoints; ++i)
            {
                controlPoints.Add(GetControlPoint(i + startCurve * 3));
            }

            float estimatedSplineLength = 0;
            int numCurves = endCurve - startCurve;
            if (curveLengths.Capacity < numCurves)
                curveLengths = new List<float>(numCurves);

            // Calculate estimatedSplinelength
            for (int i = 0; i < controlPoints.Count - 1; i += 3)
            {
                Vector3 p0 = controlPoints[i],
                    p1 = controlPoints[i + 1],
                    p2 = controlPoints[i + 2],
                    p3 = controlPoints[i + 3];

                float chord = Vector3.Distance(p0, p3);
                float controlNetLength = Vector3.Distance(p0, p1) + Vector3.Distance(p1, p2) + Vector3.Distance(p2, p3);
                float estimatedCurveLength = (chord + controlNetLength) / 2f;

                curveLengths.Add(estimatedCurveLength);
                estimatedSplineLength += estimatedCurveLength;
            }

            #region EvenlySpacedPoints declaration

            if (EvenlySpacedPoints == null || EvenlySpacedPoints.Capacity < Mathf.RoundToInt(estimatedSplineLength / spacing))
                EvenlySpacedPoints = new List<Vector3>(Mathf.RoundToInt(estimatedSplineLength / spacing));

            EvenlySpacedPoints.Clear();            
            EvenlySpacedPoints.Add(points[startCurve * 3]);
            Vector3 previousPoint = points[startCurve * 3];

            #endregion
            
            float distanceSinceLastEvenPoint = 0;            
            for (int i = 0; i < curveLengths.Count; ++i)
            {
                int j = i * 3;
                int divisions = Mathf.CeilToInt(curveLengths[i] * resolution * 10);
                float t = 0;

                while (t <= 1)
                {
                    t += 1f / divisions;
                    Vector3 pointOnCurve = Bezier.GetPoint(controlPoints[j], controlPoints[j+1], controlPoints[j+2], controlPoints[j+3], t);
                    distanceSinceLastEvenPoint += Vector3.Distance(previousPoint, pointOnCurve);

                    Vector3 normalizedDir = (previousPoint - pointOnCurve).normalized;
                    while (distanceSinceLastEvenPoint >= spacing)
                    {
                        float overshootDistance = distanceSinceLastEvenPoint - spacing;
                        Vector3 newEvenlySpacedPoint = pointOnCurve + normalizedDir * overshootDistance;
                        EvenlySpacedPoints.Add(newEvenlySpacedPoint);
                        distanceSinceLastEvenPoint = overshootDistance;
                        previousPoint = newEvenlySpacedPoint;
                    }

                    previousPoint = pointOnCurve;
                }
            }
            
            curveLengths.Clear();
            controlPoints.Clear();

            return EvenlySpacedPoints;
        }

        public void AutoSetAllControlPoints()
        {
            for (int i = 0; i < modes.Count; ++i)
            {
                modes[i] = BezierControlPointMode.Aligned;
            }

            for (int i = 0; i < points.Count; i += 3)
            {
                AutoSetAnchorControlPoints(i);
            }
            AutoSetStartAndEndControlPoints();
        }

        private int LoopIndex(int i)
        {
            return (i + points.Count) % points.Count;
        }

        // Auto Setting points
        private void AutoSetAnchorControlPoints(int index)
        {
            Vector3 anchorPos = points[index];
            Vector3 dir = Vector3.zero;
            float[] neighbourDistances = new float[2];

            if (index - 3 >= 0 || Loop)
            {
                Vector3 offset = points[LoopIndex(index - 3)] - anchorPos;
                dir += offset.normalized;
                neighbourDistances[0] = offset.magnitude;
            }
            if (index + 3 >= 0 || Loop)
            {
                Vector3 offset = points[LoopIndex(index + 3)] - anchorPos;
                dir -= offset.normalized;
                neighbourDistances[1] = -offset.magnitude;
            }

            dir.Normalize();

            for (int i = 0; i < 2; i++)
            {
                int controlIndex = index + i * 2 - 1;
                if (controlIndex >= 0 && controlIndex < points.Count || Loop)
                {
                    points[LoopIndex(controlIndex)] = anchorPos + dir * neighbourDistances[i] * AutoSetStrengthField;
                }
            }

            Array.Clear(neighbourDistances, 0, neighbourDistances.Length);
        }

        private void AutoSetStartAndEndControlPoints()
        {
            if (!Loop && points.Count > 1)
            {
                points[1] = (points[0] + points[2]) * 0.5f;
                points[points.Count - 2] = (points[points.Count - 1] + points[points.Count - 3]) * 0.5f;
            }
        }

        private void AutoSetAllAffectedControlPoints(int index)
        {
            for (int i = index - 3; i <= index + 3; i += 3)
            {
                if (i >= 0 && i < points.Count || Loop)
                {
                    AutoSetAnchorControlPoints(LoopIndex(i));
                }
            }
            AutoSetStartAndEndControlPoints();
        }
    }
}