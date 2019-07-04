using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Bezier.Features
{
    [RequireComponent(typeof(BezierSpline))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class SplineMeshCreator : MonoBehaviour
    {
        [Range(0.01f, 1.5f)]
        public float Spacing = 1;
        public float MeshWidth = 1;
        public float Tiling = 1;
        
        public bool CreateMeshAtStart = false;
        public bool AutoUpdateMaterialTiling = true;
        private Material MeshMaterialCache = null;
        public Material MeshMaterial = null;

        public string SortingLayerName;

#if UNITY_EDITOR
        [Tooltip("AutoUpdate works only in editor while paused or not playing. It works in build. (proven by Trail of Relics.")]
        public bool AutoUpdate; //Enable SplineEditor OnSceneGUI also...
#endif
        public ShadowCastingMode ShadowCastingMode = ShadowCastingMode.Off;

        private BezierSpline Spline;
        private const int MagicNumber = 50;
        private MeshRenderer MeshRenderer;
        private List<Vector3> Points;

        private void Awake()
        {
            Spline = GetComponent<BezierSpline>();
            verts = new List<Vector3>(Mathf.FloorToInt(MagicNumber / Spacing));
            uvs = new List<Vector2>(verts.Capacity);
            tris = new List<int>(verts.Capacity * 3);
        }

        private void Start()
        {
            if(CreateMeshAtStart)
                CreateAndSetMesh();
        }

        private void Update()
        {
            //if (Mes)
        }

        /// <summary>
        /// Creates the mesh and counts tiling for the material.
        /// </summary>
        /// <param name="startCurveIndex">Curve where the mesh starts.</param>
        /// <param name="endCurveIndex">Curve where to end mesh.</param>
        /// <param name="meshFilter">If NULL, SplineMeshCreator will assign the mesh to the MeshFilter attached to this gameObject.</param>
        public void CreateAndSetMesh(int startCurveIndex = 0, int endCurveIndex = -1, MeshFilter meshFilter = null) //Todo expand by making it available to set mesh anywhere
        {
            if (Spline == null)
                Spline = GetComponent<BezierSpline>();
            
            if (endCurveIndex == -1)
                endCurveIndex = Spline.CurveCount;
            else if (endCurveIndex < startCurveIndex)
                throw new Exception("EndCurveIndex cannot be smaller than StartCurveIndex!");

            Points = Spline.CalculateEvenlySpacedPoints(Spacing, 1.0f, startCurveIndex, endCurveIndex);

            if (meshFilter == null)
                meshFilter = GetComponent<MeshFilter>();

            if (meshFilter.sharedMesh != null)
            {
                meshFilter.sharedMesh.Clear();
                meshFilter.sharedMesh = CreateMeshFromSpline(Points, Spline.Loop, meshFilter.sharedMesh);
            }
            else
                meshFilter.sharedMesh = CreateMeshFromSpline(Points, Spline.Loop);

            MeshRenderer = GetComponent<MeshRenderer>();
            
            // Mesh Renderer Stuff
            if (AutoUpdateMaterialTiling && MeshRenderer.sharedMaterial != null)
                SetMaterialTiling();

            SetMeshMaterial();
            SetShadowCastingMode();
            SetMeshRendererSortingLayer();
        }

        private void SetMeshRendererSortingLayer()
        {
            if (MeshRenderer.sortingLayerName == SortingLayerName)
                return;

            MeshRenderer.sortingLayerName = SortingLayerName;
        }

        private void SetMeshMaterial()
        {
            if (MeshMaterial == null || MeshMaterial == MeshMaterialCache)
                return;

            MeshRenderer.sharedMaterial = Instantiate(MeshMaterial);
            MeshMaterialCache = MeshMaterial;
        }

        private void SetMaterialTiling()
        {
            int textureRepeat = Mathf.RoundToInt(Tiling * Points.Count * Spacing * 0.05f);
            MeshRenderer.sharedMaterial.mainTextureScale = new Vector2(1, textureRepeat);
        }

        private void SetShadowCastingMode()
        {
            MeshRenderer.shadowCastingMode = ShadowCastingMode;
        }        

        //Cached collections
        private List<Vector3> verts = new List<Vector3>();
        private List<Vector2> uvs = new List<Vector2>();
        private List<int> tris = new List<int>();
        
        public Mesh CreateMeshFromSpline(List<Vector3> points, bool loop, Mesh meshToUpdate = null)
        {
            int numVerts = points.Count * 2;
            verts.Clear();
            if (verts.Capacity < numVerts)
                verts = new List<Vector3>(numVerts);

            uvs.Clear();
            if (uvs.Capacity < verts.Capacity)
                uvs = new List<Vector2>(verts.Capacity);

            int numTris = (2 * points.Count - 1) + ((loop) ? 2 : 0);
            tris.Clear();
            if (tris.Capacity < numTris * 3)
                tris = new List<int>(numTris * 3);
            
            int vertIndex = 0;

            for (int i = 0; i < points.Count; ++i)
            {
                Vector3 forward = Vector3.zero;
                if (i < points.Count - 1 || loop)
                {
                    forward += points[(i + 1) % points.Count] - points[i];
                }
                if (i > 0 || loop)
                {
                    forward += points[i] - points[(i - 1 + points.Count) % points.Count];
                }
                forward.Normalize();

                float percentCompleted = i / (float)(points.Count - 1);
                float v = 1 - Mathf.Abs(2 * percentCompleted - 1);
                
                uvs.Add(new Vector2(0, v));
                uvs.Add(new Vector2(1, v));

                // Vector Magicks in 3D, Vector3.Cross(yourforwardvector3, referenceDirection)
                Vector3 right = Vector3.Cross(forward, Vector3.forward);
                Vector3 left = -right;
                //left = new Vector3(-forward.y, forward.x); // If you want to roll without Vector3.Cross
                
                verts.Add(points[i] + left * MeshWidth * 0.5f);
                verts.Add(points[i] - left * MeshWidth * 0.5f);

                if (i < points.Count - 1 || loop)
                {
                    tris.Add(vertIndex);
                    tris.Add((vertIndex + 2) % numVerts);
                    tris.Add(vertIndex + 1);

                    tris.Add(vertIndex + 1);
                    tris.Add((vertIndex + 2) % numVerts);
                    tris.Add((vertIndex + 3) % numVerts);
                }

                vertIndex += 2;
            }

            if (meshToUpdate != null)
            {
                meshToUpdate.SetVertices(verts);
                meshToUpdate.SetTriangles(tris, 0);
                meshToUpdate.SetUVs(0, uvs);
                return meshToUpdate;
            }

            Mesh mesh = new Mesh();
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetUVs(0, uvs);
            return mesh;
        }
    }
}