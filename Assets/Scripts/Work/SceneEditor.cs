using System.Collections.Generic;
using UnityEngine;

namespace Project.Work
{
    public class SceneEditor : MonoBehaviour
    {
        public Material LineMaterial;
        public float LineWide = 0.05f;
        public Color LineColor;
        public GameObject VertexPrefab;
        public Mesh CylinderMesh;
        public int VertexPositionZ = 0;
        public RectTransform DrawPanelRectTransform;
        public int ApproximateDis = 10;
        public Transform LineRoot;
        public Transform VertexRoot;
        public bool EditorOn = false;

        private Rect _drawRect;
        private readonly List<GameObject> _points = new List<GameObject>();
        private readonly List<GameObject> _lines = new List<GameObject>();
        private Vector2Int _mouseScreenPos;
        private int _currentPolygonNum = 1000;

        private void Start()
        {
            SetTextureRect();
            Debug.Log(GetMouseWorldPos(new Vector2Int((int)Input.mousePosition.x, (int)Input.mousePosition.y)));
        }

        private void Update()
        {
            if (EditorOn)
            {
                _mouseScreenPos = new Vector2Int((int)Input.mousePosition.x, (int)Input.mousePosition.y);

            }
        }

        private Vector3 GetMouseWorldPos(Vector2Int mouseScreenPos)
        {
            return Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, VertexPositionZ));
        }

        private void DrawLine()
        {

        }

        //private void DrawConnectingLines()
        //{
        //    if (mainPoint && points.Length > 0)
        //    {
        //        // Loop through each point to connect to the mainPoint
        //        foreach (GameObject point in points)
        //        {
        //            Vector3 mainPointPos = mainPoint.transform.position;
        //            Vector3 pointPos = point.transform.position;

        //            GL.Begin(GL.LINES);
        //            lineMat.SetPass(0);
        //            GL.Color(new Color(lineMat.color.r, lineMat.color.g, lineMat.color.b, lineMat.color.a));
        //            GL.Vertex3(mainPointPos.x, mainPointPos.y, mainPointPos.z);
        //            GL.Vertex3(pointPos.x, pointPos.y, pointPos.z);
        //            GL.End();
        //        }
        //    }
        //}

        private void SetLine(Vector3 pos, GameObject v, GameObject line)
        {
            line.transform.position = pos;
            float cylinderDistance = 0.5f * Vector3.Distance(v.transform.position, pos);
            line.transform.localScale = new Vector3(line.transform.localScale.x, cylinderDistance, line.transform.localScale.z);
            line.transform.LookAt(v.transform, Vector3.up);
            line.transform.rotation *= Quaternion.Euler(90, 0, 0);
        }

        private void SetLine(GameObject va, GameObject vb, GameObject line)
        {
            line.transform.position = va.transform.position;
            float cylinderDistance = 0.5f * Vector3.Distance(va.transform.position, vb.transform.position);
            line.transform.localScale = new Vector3(line.transform.localScale.x, cylinderDistance, line.transform.localScale.z);
            line.transform.LookAt(vb.transform, Vector3.up);
            line.transform.rotation *= Quaternion.Euler(90, 0, 0);
        }

        private GameObject CreateLineObject()
        {
            GameObject line = new GameObject();
            line.name = $"Polygon {_currentPolygonNum} line #{_lines.Count}";
            line.transform.parent = LineRoot;
            GameObject ringOffsetCylinderMeshObject = new GameObject();
            ringOffsetCylinderMeshObject.transform.parent = line.transform;
            ringOffsetCylinderMeshObject.transform.localPosition = new Vector3(0f, 1f, 0f);
            ringOffsetCylinderMeshObject.transform.localScale = new Vector3(LineWide, 1f, LineWide);
            MeshFilter ringMesh = ringOffsetCylinderMeshObject.AddComponent<MeshFilter>();
            ringMesh.mesh = CylinderMesh;
            MeshRenderer ringRenderer = ringOffsetCylinderMeshObject.AddComponent<MeshRenderer>();
            ringRenderer.material = LineMaterial;
            return line;
        }

        private GameObject CreateVertexObject(Vector3 pos)
        {
            GameObject vertex = GameObject.Instantiate(VertexPrefab, pos, Quaternion.identity);
            vertex.name = $"Polygon {_currentPolygonNum} vertex #{_points.Count}";
            vertex.transform.parent = VertexRoot;
            return vertex;
        }

        #region 点的位置和距离判断等方法
        private bool IsClose(Vector2Int pos1, Vector2Int pos2)
        {
            return Vector2Int.Distance(pos1, pos2) < ApproximateDis;
        }

        private Vector2Int ConvertPosition(Vector2Int pos)
        {
            Vector2Int cpos = new Vector2Int
            {
                x = pos.x - (int)_drawRect.xMin,
                y = pos.y - (int)_drawRect.yMin
            };
            return cpos;
        }

        private bool OutTexture(Vector2Int mousePos)
        {
            if (mousePos.x < _drawRect.xMin || mousePos.x > _drawRect.xMax ||
                mousePos.y < _drawRect.yMin || mousePos.y > _drawRect.yMax)
            {
                return true;
            }

            return false;
        }

        private void SetTextureRect()
        {
            Vector3[] fourCornersArray = new Vector3[4];
            DrawPanelRectTransform.GetWorldCorners(fourCornersArray);
            fourCornersArray[0] = Camera.main.WorldToScreenPoint(fourCornersArray[0]);
            fourCornersArray[2] = Camera.main.WorldToScreenPoint(fourCornersArray[2]);
            _drawRect = new Rect();
            _drawRect.min = new Vector2(fourCornersArray[0].x, fourCornersArray[0].y);
            _drawRect.max = new Vector2(fourCornersArray[2].x, fourCornersArray[2].y);
        }
        #endregion
    }

}


