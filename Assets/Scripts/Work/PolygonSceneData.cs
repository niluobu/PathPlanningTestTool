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
        public List<Polygon> Polygons;
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
        public Vector2Int Position;
    }

    public static class PolygonExtension
    {
        public static int GetMinX(this Polygon polygon)
        {
            int minx = Int32.MaxValue;
            foreach (var vertex in polygon.Vertexes)
            {
                if (vertex.Position.x < minx)
                {
                    minx = vertex.Position.x;
                }
            }
            return minx;
        }
    }
}


