using System.Collections;
using UnityEngine;

namespace Bezier.Features
{
    [ExecuteInEditMode]
    public class SplineWalker : MonoBehaviour
    {
        public BezierSpline Spline;
        //public SplineWalkerSpeed Speed;
        public float Duration;
        public bool LookForward;
        [Tooltip("Change this to change the orientation of the walker. If using 'LookForward'")]
        //public Vector3 WorldUp = Vector3.up;
        public SplineWalkerMode Mode;
        public bool CurrentlyWalking { get; private set; }

        private bool ExecuteInEditMode = false; //not working atm
        public bool WalkOnStart = false;

        private float Progress;
        //private float Acceleration = 0;
        private bool GoingForward = true;
        private Coroutine Coroutine;

        public void StartWalking()
        {            
            Coroutine = StartCoroutine(Walker());
            CurrentlyWalking = true;
        }

        public void StopWalking()
        {
            if (Coroutine == null)
                return;

            StopCoroutine(Coroutine);
            CurrentlyWalking = false;
        }

        private void Start()
        {
            if (WalkOnStart == true)
                StartWalking();
        }

        private void Update()
        {
            if(ExecuteInEditMode && !CurrentlyWalking && Application.isEditor)
                StartWalking();
            else if (!ExecuteInEditMode && !Application.isPlaying)
                StopWalking();
        }

        IEnumerator Walker()
        {
            while (true)
            {
                if (GoingForward)
                {
                    Progress += Time.deltaTime / Duration;
                    if (Progress > 1f)
                        if (Mode == SplineWalkerMode.Once)
                        {
                            Progress = 1f;
                            StopWalking();
                        }
                        else if (Mode == SplineWalkerMode.Loop)
                            Progress -= 1f;
                        else
                        {
                            Progress = 2f - Progress;
                            GoingForward = false;
                        }
                }
                else
                {
                    Progress -= Time.deltaTime / Duration;
                    if (Progress < 0f)
                    {
                        Progress = -Progress;
                        GoingForward = true;
                    }
                }

                Vector3 position = Spline.GetPoint(Progress);
                transform.localPosition = position;

                if (LookForward)
                    transform.LookAt(position + Spline.GetDirection(Progress)/*, WorldUp*/);

                yield return null;
            }
        }
    }
}