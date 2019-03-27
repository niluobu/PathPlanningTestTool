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
        [Tooltip("相对相机z轴值的偏移")]
        public int VertexPositionZ = 10;
        public RectTransform DrawPanelRectTransform;
        public int ApproximateDis = 10;
        public Transform LineRoot;
        public Transform VertexRoot;

        private Rect _drawRect;
        private readonly List<Vector2Int> _vertexPositions = new List<Vector2Int>();
        private readonly List<GameObject> _points = new List<GameObject>();
        private readonly List<GameObject> _lines = new List<GameObject>();
        private Vector2Int _mouseScreenPos = new Vector2Int();
        private GameObject _mouseLine = null;
        private GameObject _mouseVertex = null;

        private Dictionary<int, List<GameObject>> _scenePoints = new Dictionary<int, List<GameObject>>();
        private Dictionary<int, List<GameObject>> _sceneLines = new Dictionary<int, List<GameObject>>();
        private List<Polygon> _polygons = new List<Polygon>();
        private int _scenePolygonNum = 1000;
        private int _sceneVertexNum = 0;

        private bool _editorOn = false;
        public bool EditorOn
        {
            get => _editorOn;
            set => _editorOn = value;
        }

        private void Start()
        {
            SetTextureRect();
            SetLineColor(LineColor);
        }

        private void Update()
        {
            _mouseScreenPos.x = (int)Input.mousePosition.x;
            _mouseScreenPos.y = (int)Input.mousePosition.y;
            if (_editorOn && MouseIsInDrawPanel(_mouseScreenPos))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    DrawVertex(_mouseScreenPos);
                }

                UpdataMouseLine();
            }
        }

        private void UpdataMouseLine()
        {
            if (_mouseVertex == null)
            {
                return;
            }

            if (_mouseLine == null)
            {
                _mouseLine = CreateLineObject();
            }
            SetLine(GetWorldPos(_mouseScreenPos), _mouseVertex, _mouseLine);
        }

        private void DrawVertex(Vector2Int screenPos)
        {
            //将要新添加的顶点与第一个顶点相近，自动连接这两点，结束当前多边形的绘制
            if (_vertexPositions.Count > 2 && IsClose(screenPos, _vertexPositions[0]))
            {
                SetLine(_points[_points.Count - 1].transform.position, _points[0], _mouseLine);
                EndCurrentPolygonDrawing();
                return;
            }

            GameObject vertex = CreateVertexObject(GetWorldPos(screenPos));
            _points.Add(vertex);
            _vertexPositions.Add(screenPos);
            _mouseVertex = vertex;
            if (_points.Count > 1)  //将之前的线确定下来，生成新的线作为连接鼠标和新的顶点的线
            {
                SetLine(_points[_points.Count - 2].transform.position, _mouseVertex, _mouseLine);
                _lines.Add(_mouseLine);
                _mouseLine = CreateLineObject();
            }
        }

        private void EndCurrentPolygonDrawing()
        {
            CreatePolygon(out Polygon polygon);
            _polygons.Add(polygon);
            _vertexPositions.Clear();
            _scenePoints.Add(polygon.PolygonNum, new List<GameObject>(_points));
            _points.Clear();
            _sceneLines.Add(polygon.PolygonNum, new List<GameObject>(_lines));
            _lines.Clear();
            _mouseLine = null;
            _mouseVertex = null;
        }

        private void CreatePolygon(out Polygon polygon)
        {
            polygon = new Polygon()
            {
                PolygonNum = _scenePolygonNum++,
                Vertexes = new List<Vertex>()
            };
            foreach (var pos in _vertexPositions)
            {
                polygon.Vertexes.Add(new Vertex()
                {
                    Position = pos,
                    VertexNum = _sceneVertexNum++
                });
            }
        }

        private void SetLineColor(Color color)
        {
            LineMaterial.SetColor("_Color", color);
        }

        private void SetLine(Vector3 pos, GameObject v, GameObject line)
        {
            line.transform.position = GetMiddlePos(pos, v.transform.position);
            float cylinderDistance = 0.5f * Vector3.Distance(v.transform.position, pos);
            line.transform.localScale = new Vector3(line.transform.localScale.x, cylinderDistance, line.transform.localScale.z);
            line.transform.LookAt(v.transform, Vector3.up);
            line.transform.rotation *= Quaternion.Euler(90, 0, 0);
        }

        #region CreateGameObject
        private GameObject CreateLineObject()
        {
            GameObject line = new GameObject
            {
                name = $"Polygon {_scenePolygonNum} line #{_lines.Count}"
            };
            line.transform.parent = LineRoot;
            line.transform.localPosition = new Vector3(0f, 1f, 0f);
            line.transform.localScale = new Vector3(LineWide, 0f, LineWide);
            MeshFilter ringMesh = line.AddComponent<MeshFilter>();
            ringMesh.mesh = CylinderMesh;
            MeshRenderer ringRenderer = line.AddComponent<MeshRenderer>();
            ringRenderer.material = LineMaterial;
            return line;
        }

        private GameObject CreateVertexObject(Vector3 pos)
        {
            GameObject vertex = GameObject.Instantiate(VertexPrefab, pos, Quaternion.identity);
            vertex.name = $"Polygon {_scenePolygonNum} vertex #{_points.Count}";
            vertex.transform.parent = VertexRoot;
            return vertex;
        }
        #endregion

        #region about Coordinate conversion
        private Vector3 GetWorldPos(Vector2Int screenPos)
        {
            return Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, VertexPositionZ));
        }

        private Vector3 GetMiddlePos(Vector3 a, Vector3 b)
        {
            return new Vector3(Mathf.Lerp(a.x, b.x, 0.5f), Mathf.Lerp(a.y, b.y, 0.5f), Mathf.Lerp(a.z, b.z, 0.5f));
        }

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

        private bool MouseIsInDrawPanel(Vector2Int mousePos)
        {
            if (mousePos.x < _drawRect.xMin || mousePos.x > _drawRect.xMax ||
                mousePos.y < _drawRect.yMin || mousePos.y > _drawRect.yMax)
            {
                return false;
            }

            return true;
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

        //private void OnGUI()
        //{
        //    GUIStyle style = new GUIStyle();
        //    style.fontSize = 24;
        //    GUI.Label(new Rect(10, 10, 150, 80), "MouseInDrawPanel: " + MouseIsInDrawPanel(_mouseScreenPos), style);
        //}
    }

}


