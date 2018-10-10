using UnityEngine;

namespace Bezier
{
    public interface IBezierSpline
    {
        void AddPoint();
        void AddPoint(Vector3 pos);
    }
}