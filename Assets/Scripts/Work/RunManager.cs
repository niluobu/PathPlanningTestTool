using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

namespace Project.Work
{
    public interface IRunManager
    {
        void DrawScene(PolygonScene scene);
        void ClearDrawPanel();
        void StartTest(Vector2Int startPoint, Vector2Int endPoint);
        void GetTestParameter(out int vertexNum, out int edgeNum, out int CVGTime, out int SPTime);
        IObservable<string> TestStageHintAsObservable { get; }
        IObservable<Unit> TestEndAsObservable { get; }
    }

    public class RunManager : IRunManager
    {
        [Inject] private readonly IDrawPanelPreference _drawPanelPreference;
        [Inject] private readonly IGlUtil _glUtil;
        [Inject] private readonly ProjectSetting _projectSetting;

        private readonly Subject<string> _hintSubject = new Subject<string>();
        private readonly Subject<Unit> _endSubject = new Subject<Unit>();

        private PolygonScene _scene;

        public IObservable<string> TestStageHintAsObservable => _hintSubject;

        public IObservable<Unit> TestEndAsObservable => _endSubject;

        public void DrawScene(PolygonScene scene)
        {
            _scene = scene;
            DrawPolygon();
            _drawPanelPreference.Apply();
        }

        public void ClearDrawPanel()
        {
            _drawPanelPreference.SetTextureBgColor();
        }

        public void StartTest(Vector2Int startPoint, Vector2Int endPoint)
        {

        }

        public void GetTestParameter(out int vertexNum, out int edgeNum, out int CVGTime, out int SPTime)
        {
            vertexNum = 0;
            foreach (var polygon in _scene.Polygons)
            {
                vertexNum += polygon.Vertexes.Count;
            }
            edgeNum = 0;
            CVGTime = 0;
            SPTime = 0;
        }

        private void DrawPolygon()
        {
            _scene.Polygons.Sort((a, b) => a.GetMinX() - b.GetMinX());
            List<int> insidePolygonNums = new List<int>();
            foreach (Polygon polygon in _scene.Polygons)
            {
                //for (int i = 0; i < polygon.Vertexes.Count; ++i)
                //{
                //    DrawLine(polygon.Vertexes[i].Position, polygon.Vertexes[(i + 1) % polygon.Vertexes.Count].Position,
                //        PolygonLineColor, PolygonLineWide);
                //}
                if (!_glUtil.FillPolygon(polygon, _projectSetting.FillColor, _drawPanelPreference.Texture,
                    _drawPanelPreference.ConvertToDrawPanelPos))
                {
                    insidePolygonNums.Add(polygon.PolygonNum);
                }
            }

            foreach (var insidePolygonNum in insidePolygonNums)
            {
                Polygon insidePolygon = _scene.Polygons.Find(x => x.PolygonNum == insidePolygonNum);
                if (insidePolygon != null)
                {
                    _scene.Polygons.Remove(insidePolygon);
                }
            }
        }

        //private void DrawLine(Vector2Int startPoint, Vector2Int endPoint, Color lineColor, int lineWide)
        //{
        //    startPoint = _drawPanelPreference.ConvertToDrawPanelPos(startPoint);
        //    endPoint = _drawPanelPreference.ConvertToDrawPanelPos(endPoint);
        //    _glUtil.DrawLine(startPoint, endPoint, lineColor, _drawPanelPreference.Texture);
        //    for (int i = 1; i < lineWide; ++i)
        //    {
        //        if (Mathf.Abs(startPoint.x - endPoint.x) < Mathf.Abs(startPoint.y - endPoint.y) || startPoint.x == endPoint.x)
        //        {
        //            _glUtil.DrawLine(new Vector2Int(startPoint.x + i, startPoint.y), new Vector2Int(endPoint.x + i, endPoint.y),
        //                lineColor, _drawPanelPreference.Texture);
        //            _glUtil.DrawLine(new Vector2Int(startPoint.x - i, startPoint.y), new Vector2Int(endPoint.x - i, endPoint.y),
        //                lineColor, _drawPanelPreference.Texture);
        //        }
        //        else
        //        {
        //            _glUtil.DrawLine(new Vector2Int(startPoint.x, startPoint.y + i), new Vector2Int(endPoint.x, endPoint.y + i),
        //                lineColor, _drawPanelPreference.Texture);
        //            _glUtil.DrawLine(new Vector2Int(startPoint.x, startPoint.y - i), new Vector2Int(endPoint.x, endPoint.y - i),
        //                lineColor, _drawPanelPreference.Texture);
        //        }
        //    }
        //}

    }
}
