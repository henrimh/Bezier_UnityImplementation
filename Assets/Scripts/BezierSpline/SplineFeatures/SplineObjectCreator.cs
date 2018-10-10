using UnityEngine;
using System.Collections.Generic;

namespace Bezier.Features
{

    [RequireComponent(typeof(BezierSpline))]
    public class SplineObjectCreator : MonoBehaviour // todo make this to an editor script 
    {
        public float Spacing = .1f;
        public float Resolution = 1f;

        private List<Vector3> points;
        private List<GameObject> gos;

        private void Start()
        {
            gos = new List<GameObject>();
            points = GetComponent<BezierSpline>().CalculateEvenlySpacedPoints(Spacing, Resolution);
            foreach (Vector3 point in points)
            {
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.transform.position = point;
                go.transform.localScale = Vector3.one * Resolution * 0.1f;
                gos.Add(go);
            }
        }

        private void Update()
        {
            List<Vector3> newPoints = GetComponent<BezierSpline>().CalculateEvenlySpacedPoints(Spacing, Resolution);
            if (points.Count != newPoints.Count)
            {
                points = newPoints;

                foreach (var go in gos)
                {
                    Destroy(go);
                }

                foreach (Vector3 point in points)
                {
                    GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    go.transform.position = point;
                    go.transform.localScale = Vector3.one * Resolution * 0.1f;
                    gos.Add(go);
                }
            }
        }
    }
}