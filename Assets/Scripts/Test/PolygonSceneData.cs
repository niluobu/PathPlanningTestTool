using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Work
{
    [Serializable]
    public class SceneDataList
    {
        public List<PolygonSceneData> PolygonSceneDatas;
    }

    [Serializable]
    public class PolygonSceneData
    {
        public int SceneNum;
        public List<PolygonData> PolygonDatas;
    }

    [Serializable]
    public class PolygonData
    {
        public int PolygonNum;
        public List<Vector2Int> Vertexes;
    }
}