using UnityEditor;
using UnityEngine;
using Bezier.Features;

namespace Bezier.Editor
{
    [CustomEditor(typeof(SplineMeshCreator))]
    public class SplineEditor : UnityEditor.Editor
    {
        private SplineMeshCreator Creator;

        private void OnSceneGUI()
        {
            if (Creator.AutoUpdate && !EditorApplication.isPlaying || EditorApplication.isPaused && Event.current.type == EventType.Repaint)
            {
                Creator.CreateAndSetMesh();
            }
        }

        private void OnEnable()
        {
            Creator = (SplineMeshCreator)target;
        }
    }
}