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
        public float VertexRadius = 0.1f;
        public Color LineColor;
        public GameObject VertexPrefab;
        public Mesh CylinderMesh;
        [Tooltip("相对相机z轴值的偏移")]
        public int VertexPositionZ = 10;
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
        private PolygonScene _polygonScene;
        private int _scenePolygonNum = 1000;
        private bool _sceneDirty = false;

        [HideInInspector]
        public List<Vector2Int> TargetPoints = new List<Vector2Int>();
        private readonly List<GameObject> _targetPointObjects = new List<GameObject>();
        private bool _setOn = false;
        private GameObject _mousePoint = null;

        [Inject] private readonly IPolygonSceneStorer _polygonSceneStorer;
        [Inject] private readonly IPolygonSceneChecker _polygonSceneChecker;
        [Inject] private readonly IDrawPanelPreference _drawPanelPreference;
        [Inject] private readonly ProjectSetting _projectSetting;
        
        private void InitPolygonScene()
        {
            _polygonScene = new PolygonScene();
            int sceneNum = 2000;
            if (_polygonSceneStorer.PolygonScenes.Count > 0)
            {
                sceneNum += _polygonSceneStorer.PolygonScenes.Count;
            }
            _polygonScene.SceneNum = sceneNum;
            _polygonScene.Polygons = new List<Polygon>();
            _scenePolygonNum = 1000;
        }
        
        #region 编辑场景的接口
        public void IntoSceneEdit()
        {
            SetLineColor(LineColor);
            SetVertexRadius(VertexRadius);
            InitPolygonScene();
            _editorOn = true;
        }

        public void ExitSceneEdit()
        {
            _editorOn = false;
            ClearScene();
        }
        
        private bool _editorOn = false;
        public bool EditorOn
        {
            get => _editorOn;
            set => _editorOn = value;
        }

        public bool SceneDirty
        {
            get
            {
                bool sceneIsEmpty = _polygonScene.Polygons.IsEmpty() && _vertexes.IsEmpty();
                if (sceneIsEmpty)
                {
                    return false;
                }
                return _sceneDirty;
            }
        }

        public bool Undo()
        {
            if (_vertexes.IsEmpty())
            {
                return false;
            }
            GameObject.Destroy(_mouseVertex);
            _vertexes.RemoveAt(_vertexes.Count - 1);
            _vertexPositions.RemoveAt(_vertexPositions.Count - 1);
            _mouseVertex = _vertexes.IsEmpty() ? null : _vertexes[_vertexes.Count - 1];
            if (_edges.Contains(_mouseLine))
            {
                _edges.Remove(_mouseLine);
            }
            GameObject.Destroy(_mouseLine);
            _mouseLine = _edges.IsEmpty() ? null : _edges[_edges.Count - 1];
            return true;
        }
        
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
        #endregion

        #region 编辑起点终的接口
        public void IntoPointEdit()
        {
            SetLineColor(_projectSetting.PolygonLineColor);
            SetVertexRadius(_projectSetting.SEPointRadius);
        }

        public void ExitPointEdit()
        {
            _setOn = false;
            ClearTargetPoints();
        }

        public bool BeReady()
        {
            return TargetPoints.Count == 2;
        }

        public void SetOrReset()
        {
            if (!TargetPoints.IsEmpty())
            {
                ClearTargetPoints();
                _setOn = true;
                return;
            }
            _setOn = true;
            
            if (_mousePoint == null)
            {
                _mousePoint = CreateTargetPointObject(Vector3.zero);
            }
        }
        #endregion

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

        private void ClearScene()
        {
            _vertexPositions.Clear();

            //删除临时顶点
            foreach (GameObject g in _vertexes)
            {
                if (g == null)
                {
                    continue;
                }
                GameObject.Destroy(g);
            }
            _vertexes.Clear();
            _mouseVertex = null;

            //删除临时边
            foreach (GameObject g in _edges)
            {
                if (g == null)
                {
                    continue;
                }
                GameObject.Destroy(g);
            }
            _edges.Clear();
            if (_mouseLine != null)
            {
                GameObject.Destroy(_mouseLine);
                _mouseLine = null;
            }

            //删除已保存的边
            foreach (List<GameObject> edges in _sceneEdges.Values)
            {
                foreach (GameObject g in edges)
                {
                    if (g == null)
                    {
                        continue;
                    }
                    GameObject.Destroy(g);
                }
            }
            _sceneEdges.Clear();

            //删除已保存的顶点
            foreach (List<GameObject> vertexes in _sceneVertexes.Values)
            {
                foreach (GameObject g in vertexes)
                {
                    if (g == null)
                    {
                        continue;
                    }
                    GameObject.Destroy(g);
                }
            }
            _sceneVertexes.Clear();
        }
        
        private void ClearTargetPoints()
        {
            if (_mousePoint != null)
            {
                GameObject.Destroy(_mousePoint);
                _mousePoint = null;
            }

            TargetPoints.Clear();
            for (int i = _targetPointObjects.Count - 1; i >= 0; --i)
            {
                GameObject.Destroy(_targetPointObjects[i]);
            }
            _targetPointObjects.Clear();
        }
        
        private void UpdataMousePoint()
        {
            if (TargetPoints.Count == 2)
            {
                return;
            }
            if (_mousePoint == null)
            {
                _mousePoint = CreateTargetPointObject(GetWorldPos(_mouseScreenPos));
            }
            SetPoint(_mousePoint, GetWorldPos(_mouseScreenPos));
        }

        private void AddTargetPoint(Vector2Int mouseScreenPos)
        {
            if (!_drawPanelPreference.PointAroundIsBgColor(mouseScreenPos))
            {
                return;
            }
            SetPoint(_mousePoint, GetWorldPos(mouseScreenPos));
            TargetPoints.Add(mouseScreenPos);
            _targetPointObjects.Add(_mousePoint);
            _mousePoint = null;
            if (TargetPoints.Count == 2)
            {
                _setOn = false;
            }
        }

        private void SetPoint(GameObject point, Vector3 pos)
        {
            point.transform.position = pos;
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

        private void SetLine(Vector3 pos, GameObject v, GameObject line)
        {
            line.transform.position = GetMiddlePos(pos, v.transform.position);
            float cylinderDistance = 0.5f * Vector3.Distance(v.transform.position, pos);
            line.transform.localScale = new Vector3(line.transform.localScale.x, cylinderDistance, line.transform.localScale.z);
            line.transform.LookAt(v.transform, Vector3.up);
            line.transform.rotation *= Quaternion.Euler(90, 0, 0);
        }

        private void SetLineColor(Color color)
        {
            LineMaterial.SetColor("_Color", color);
        }
        
        private void SetVertexRadius(float radius)
        {
            VertexPrefab.transform.localScale = new Vector3(radius, radius, radius);
        }

        private void Update()
        {
            _mouseScreenPos.x = (int)Input.mousePosition.x;
            _mouseScreenPos.y = (int)Input.mousePosition.y;
            if (_editorOn && _drawPanelPreference.MouseIsInDrawPanel(_mouseScreenPos))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    AddNewVertex(_mouseScreenPos);
                }

                UpdataMouseLine();
            }

            if (_setOn && _drawPanelPreference.MouseIsInDrawPanel(_mouseScreenPos))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    AddTargetPoint(_mouseScreenPos);
                }

                UpdataMousePoint();
            }
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

        private GameObject CreateTargetPointObject(Vector3 pos)
        {
            GameObject point = GameObject.Instantiate(VertexPrefab, pos, Quaternion.identity);
            point.name = $"Point {TargetPoints.Count}";
            point.transform.parent = VertexRoot;
            return point;
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
        #endregion

        //private void OnGUI()
        //{
        //    GUIStyle style = new GUIStyle();
        //    style.fontSize = 24;
        //    GUI.Label(new Rect(10, 10, 150, 80), "MouseInDrawPanel: " + MouseIsInDrawPanel(_mouseScreenPos), style);
        //}
    }

}


