using UnityEditor;
using UnityEngine;

namespace Bezier.Editor
{
    [CustomEditor(typeof(BezierSpline))]
    public class BezierSplineInspector : UnityEditor.Editor
    {
        private const int stepsPerCurve = 10;
        private const float directionScale = 0.05f;
        private const float handleSize = 0.05f;
        private const float pickSize = 0.07f;

        private static Color[] modeColors =
        {
            Color.white,
            Color.yellow,
            Color.cyan
        };

        private int selectedIndex = -1;

        private BezierSpline spline;
        private Transform handleTransform;
        private Quaternion handleRotation;
        
        private void OnSceneGUI()
        {            
            spline = target as BezierSpline;
            handleTransform = spline.transform;
            handleRotation = Tools.pivotRotation == PivotRotation.Local
                ? handleTransform.rotation
                : Quaternion.identity;

            Draw();
        }

        public override void OnInspectorGUI()
        {         
            spline = target as BezierSpline;

            EditorGUI.BeginChangeCheck();
            bool loop = EditorGUILayout.Toggle("Loop", spline.Loop);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(spline, "Toggle Loop");
                EditorUtility.SetDirty(spline);
                spline.Loop = loop;
            }

            EditorGUI.BeginChangeCheck();
            bool autoSetControlPoints = EditorGUILayout.Toggle("Auto Set Control Points", spline.AutoSetControlPoints);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(spline, "Toggle Auto Set Control Points");
                EditorUtility.SetDirty(spline);
                spline.AutoSetControlPoints = autoSetControlPoints;
            }

            if (autoSetControlPoints)
            {
                EditorGUI.BeginChangeCheck();
                float autoSetStrength = EditorGUILayout.FloatField("Strength", spline.AutoSetStrength);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(spline, "Change AutoSet Strength");
                    EditorUtility.SetDirty(spline);
                    spline.AutoSetStrength = autoSetStrength;
                }
            }

            if (selectedIndex >= 0 && selectedIndex < spline.ControlPointCount)
            {
                DrawSelectedPointInspector();
            }

            if (GUILayout.Button("Add Point"))
            {
                Undo.RecordObject(spline, "Add Point");
                spline.AddPoint();
                EditorUtility.SetDirty(spline);
            }

            if (selectedIndex % 3 == 0 && selectedIndex > 0)
            {
                if (GUILayout.Button("Add Point In Between Points"))
                {
                    Undo.RecordObject(spline, "Add Curve In Between Points");
                    spline.AddCurveInBetween(selectedIndex);
                    EditorUtility.SetDirty(spline);
                }
                if (GUILayout.Button("Remove Point"))
                {
                    Undo.RecordObject(spline, "Remove Point");
                    spline.DeleteAnchorPoint(selectedIndex);
                    EditorUtility.SetDirty(spline);
                }
            }
        }
        
        private void ShowDirections()
        {
            Handles.color = Color.green;
            Vector3 point = spline.GetPoint(0f);
            Handles.DrawLine(point, point + spline.GetDirection(0f) * directionScale);
            int steps = stepsPerCurve * spline.CurveCount;
            for (int i = 1; i <= steps; ++i)
            {
                point = spline.GetPoint(i / (float) steps);
                Handles.DrawLine(point, point + spline.GetDirection(i / (float) steps) * directionScale);
            }
        }

        private void DrawSelectedPointInspector()
        {
            GUILayout.Label("Selected Point");
            EditorGUI.BeginChangeCheck();
            Vector3 point = EditorGUILayout.Vector3Field("Position", spline.GetControlPoint(selectedIndex));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(spline, "Move Point");
                EditorUtility.SetDirty(spline);
                spline.SetControlPoint(selectedIndex, point);
            }
            if (!spline.AutoSetControlPoints)
            {
                EditorGUI.BeginChangeCheck();
                BezierControlPointMode mode = (BezierControlPointMode) EditorGUILayout.EnumPopup("Mode", spline.GetControlPointMode(selectedIndex));
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(spline, "Change Point Mode");
                    spline.SetControlPointMode(selectedIndex, mode);
                    EditorUtility.SetDirty(spline);
                }
            }
        }

        private void Draw()
        {
            //Draw Beziers and points separately

            for (int i = 0; i < spline.CurveCount; i++)
            {
                Vector3[] points = spline.GetControlPointsInCurve(i);

                points[0] += spline.gameObject.transform.position;
                points[1] += spline.gameObject.transform.position;
                points[2] += spline.gameObject.transform.position;
                points[3] += spline.gameObject.transform.position;
                
                Handles.color = Color.gray;
                Handles.DrawLine(points[0], points[1]);
                Handles.DrawLine(points[2], points[3]);

                Handles.DrawBezier(points[0], points[3], points[1], points[2], Color.white, null, 2f);
            }

            //if (spline.Loop)
            //{
            //    Vector3[] points =
            //    {
            //        spline.GetControlPoint(0),
            //        spline.GetControlPoint(spline.PointCount - 1),
            //        spline.GetControlPoint(spline.PointCount - 2),
            //        spline.GetControlPoint(spline.PointCount - 3)
            //    };

            //    Handles.DrawLine(points[0], points[1]);
            //    Handles.DrawLine(points[2], points[3]);

            //    Handles.DrawBezier(points[0], points[3], points[1], points[2], Color.white, null, 2f);
            //}

            for (int i = 0; i < spline.ControlPointCount; i++)
            {
                ShowPoint(i);
            }
            
            // Old Draw()
            //Vector3 p0 = ShowPoint(0);
            //for (int i = 1; i < spline.ControlPointCount; i += 3)
            //{
            //    Vector3 p1 = ShowPoint(i);
            //    Vector3 p2 = ShowPoint(i + 1);
            //    Vector3 p3 = ShowPoint(i + 2);

            //    Handles.color = Color.gray;
            //    Handles.DrawLine(p0, p1);
            //    Handles.DrawLine(p2, p3);

            //    Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);
            //    p0 = p3;
            //}
            //ShowDirections();            
            
        }

        private Vector3 ShowPoint(int index)
        {
            Vector3 point = handleTransform.TransformPoint(spline.GetControlPoint(index));
            float size = HandleUtility.GetHandleSize(point);
            if (index == 0)
                size *= 2f;

            if (index % 3 == 0)
                Handles.color = Color.red;
            else
                Handles.color = modeColors[(int) spline.GetControlPointMode(index)];


            if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap))
            {
                selectedIndex = index;
                Repaint();                
            }
            if (selectedIndex == index)
            {
                EditorGUI.BeginChangeCheck();
                point = Handles.DoPositionHandle(point, handleRotation);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(spline, "Move Point");
                    EditorUtility.SetDirty(spline);
                    spline.SetControlPoint(index, handleTransform.InverseTransformPoint(point));   
                }
            }
            return point;
        }
    }
}