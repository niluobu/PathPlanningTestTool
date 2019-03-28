using ModestTree;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

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
        public int ApproximateDis = 20;
        public Transform LineRoot;
        public Transform VertexRoot;

        private Rect _drawRect;
        private readonly List<Vector2Int> _vertexPositions = new List<Vector2Int>();
        private readonly List<GameObject> _vertexes = new List<GameObject>();
        private readonly List<GameObject> _edges = new List<GameObject>();
        private Vector2Int _mouseScreenPos = new Vector2Int();
        private GameObject _mouseLine = null;
        private GameObject _mouseVertex = null;

        private readonly Dictionary<int, List<GameObject>> _sceneVertexes = new Dictionary<int, List<GameObject>>();
        private readonly Dictionary<int, List<GameObject>> _sceneEdges = new Dictionary<int, List<GameObject>>();
        private readonly PolygonScene _polygonScene = new PolygonScene();
        private int _scenePolygonNum = 1000;
        private bool _sceneDirty = false;

        [Inject] private readonly IPolygonSceneStorer _polygonSceneStorer;
        [Inject] private readonly IPolygonSceneChecker _polygonSceneChecker;

        private bool _editorOn = false;
        public bool EditorOn
        {
            get => _editorOn;
            set => _editorOn = value;
        }

        public bool SceneDirty => _sceneDirty;

        public bool Undo()
        {
            if (_vertexes.IsEmpty())
            {
                return false;
            }
            GameObject.Destroy(_mouseVertex);
            _vertexes.RemoveAt(_vertexes.Count - 1);
            _mouseVertex = _vertexes.IsEmpty() ? null : _vertexes[_vertexes.Count - 1];
            if (_edges.Contains(_mouseLine))
            {
                _edges.Remove(_mouseLine);
            }
            GameObject.Destroy(_mouseLine);
            _mouseLine = _edges.IsEmpty() ? null : _edges[_edges.Count - 1];
            return true;
        }

        #region SaveScene
        public bool SaveScene()
        {
            bool succeed = SceneValidityCheck();
            if (succeed)
            {
                _polygonSceneStorer.SavePolygonScene(_polygonScene);
                _sceneDirty = false;
            }
            return succeed;
        }

        private bool SceneValidityCheck()
        {
            List<int> invalidPolygonNums = _polygonSceneChecker.CheckPolygonScene(_polygonScene);
            ClearInvalidPolygon(invalidPolygonNums);
            return invalidPolygonNums.IsEmpty();
        }

        private void ClearInvalidPolygon(List<int> invalidPolygonNums)
        {
            foreach (var invalidPolygonNum in invalidPolygonNums)
            {
                Polygon invalidPolygon = _polygonScene.Polygons.Find(x => x.PolygonNum == invalidPolygonNum);
                if (invalidPolygon != null)
                {
                    _polygonScene.Polygons.Remove(invalidPolygon);
                }

                if (_sceneVertexes.ContainsKey(invalidPolygonNum))
                {
                    foreach (var point in _sceneVertexes[invalidPolygonNum])
                    {
                        GameObject.Destroy(point);
                    }

                    _sceneVertexes.Remove(invalidPolygonNum);
                }

                if (_sceneEdges.ContainsKey(invalidPolygonNum))
                {
                    foreach (var line in _sceneEdges[invalidPolygonNum])
                    {
                        GameObject.Destroy(line);
                    }

                    _sceneEdges.Remove(invalidPolygonNum);
                }
            }
        }
        #endregion

        private void Start()
        {
            SetTextureRect();
            SetLineColor(LineColor);
            InitPolygonScene();
        }

        private void InitPolygonScene()
        {
            int sceneNum = 2000;
            if (_polygonSceneStorer.PolygonScenes.Count > 0)
            {
                sceneNum += _polygonSceneStorer.PolygonScenes.Count;
            }
            _polygonScene.SceneNum = sceneNum;
            _polygonScene.Polygons = new List<Polygon>();
        }

        private void Update()
        {
            _mouseScreenPos.x = (int)Input.mousePosition.x;
            _mouseScreenPos.y = (int)Input.mousePosition.y;
            if (_editorOn && MouseIsInDrawPanel(_mouseScreenPos))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    AddNewVertex(_mouseScreenPos);
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

            if (_vertexPositions.Count > 2 && IsClose(_mouseScreenPos, _vertexPositions[0]))
            {
                SetLine(_vertexes[0].transform.position, _mouseVertex, _mouseLine);
                return;
            }
            SetLine(GetWorldPos(_mouseScreenPos), _mouseVertex, _mouseLine);
        }

        private void AddNewVertex(Vector2Int mouseScreenPos)
        {
            //将要新添加的顶点与第一个顶点相近，自动连接这两点，结束当前多边形的绘制
            if (_vertexPositions.Count > 2 && IsClose(mouseScreenPos, _vertexPositions[0]))
            {
                AddLine(_vertexes[_vertexes.Count - 1].transform.position, _vertexes[0]);
                EndCurrentPolygonDrawing();
                return;
            }

            AddVertex(mouseScreenPos);
            if (_vertexes.Count > 1)
            {
                AddLine(_vertexes[_vertexes.Count - 2].transform.position, _mouseVertex);
            }
        }

        private void AddVertex(Vector2Int mouseScreenPos)
        {
            GameObject vertex = CreateVertexObject(GetWorldPos(mouseScreenPos));
            _vertexes.Add(vertex);
            _vertexPositions.Add(mouseScreenPos);
            _mouseVertex = vertex;
            _sceneDirty = true;
        }

        private void AddLine(Vector3 pos, GameObject v)
        {   //设置线的位置，添加到边的队列，生成新的线作为连接鼠标和新的顶点的线
            SetLine(pos, v, _mouseLine);
            _edges.Add(_mouseLine);
            _mouseLine = null;
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

        #region EndCurrentPolygonDrawing
        private void EndCurrentPolygonDrawing()
        {
            CreatePolygon(out Polygon polygon);
            _polygonScene.Polygons.Add(polygon);
            _vertexPositions.Clear();

            _sceneVertexes.Add(polygon.PolygonNum, new List<GameObject>(_vertexes));
            _vertexes.Clear();

            _sceneEdges.Add(polygon.PolygonNum, new List<GameObject>(_edges));
            _edges.Clear();
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
            for (int i = 0; i < _vertexPositions.Count; ++i)
            {
                polygon.Vertexes.Add(new Vertex()
                {
                    Position = _vertexPositions[i],
                    VertexNum = i
                });
            }
        }
        #endregion

        #region CreateGameObject
        private GameObject CreateLineObject()
        {
            GameObject line = new GameObject
            {
                name = $"Polygon {_scenePolygonNum} line #{_edges.Count}"
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
            vertex.name = $"Polygon {_scenePolygonNum} vertex #{_vertexes.Count}";
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


