using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Work
{
    [Serializable]
    public class PolygonSceneConfig
    {
        public List<PolygonScene> PolygonScenes;
    }

    [Serializable]
    public class PolygonScene
    {
        public int SceneNum;
        public List<PolygonData> Polygons;
    }

    [Serializable]
    public class Polygon
    {
        public int PolygonNum;
        public List<Vertex> Vertexes;
    }

    [Serializable]
    public class Vertex
    {
        public int VertexNum;
        public Vector2Int Point;
    }
}


