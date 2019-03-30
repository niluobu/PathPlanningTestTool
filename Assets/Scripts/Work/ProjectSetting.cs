using System;
using UnityEngine;

namespace Project.Work
{
    [CreateAssetMenu(fileName = "ProjectSetting", menuName = "ScriptableObjects/ProjectSetting", order = 1)]
    [Serializable]
    public class ProjectSetting : ScriptableObject
    {
        [Header("-------SceneDraw--------")]
        public float SEPointRadius = 0.2f;
        [Range(10, 30)]
        public int SEPointCheckRadius = 15;
        [Range(1, 3)]
        public int PolygonLineWide = 2;
        public Color PolygonLineColor;
        [Range(1, 3)]
        public int VisibleGLineWide = 1;
        public Color VisibleGLineColor;
        [Range(1, 3)]
        public int ResultPathLineWide = 3;
        public Color ResultPathLineColor;
        public Color FillColor;
        public Color TextureBgColor;
    }
}


