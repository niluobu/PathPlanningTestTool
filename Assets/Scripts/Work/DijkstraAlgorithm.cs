using System.Collections.Generic;

namespace Project.Work
{
    public interface IDijkstraAlgorithm
    {
        List<Path> PathPlanning(float[,] adjacentM, int startIndex, int endIndex);
    }

    public class Path //表示起点到场景中的一个顶点的路径信息
    {
        public int VertexIndex; //表示这个路径最后一个顶点的编号
        public int PreIndex; //路径倒数第二个顶点的编号
        public float Length; //路径长度
    }

    public class DijkstraAlgorithm : IDijkstraAlgorithm
    {
        private readonly List<Path> _tempPaths = new List<Path>();
        private readonly List<Path> _shortestPaths = new List<Path>();

        public List<Path> PathPlanning(float[,] adjacentM, int startIndex, int endIndex)
        {
            InitPaths(adjacentM, startIndex);

            int count = 0;
            int n = adjacentM.GetLength(0);
            while (count < n)
            {
                SelectMinLengthPath(out Path minPath);
                if (minPath.VertexIndex == endIndex)
                {
                    _shortestPaths.Add(minPath);
                    break;
                }
                _tempPaths.Remove(minPath);
                _shortestPaths.Add(minPath);
                UpdateTempPath(adjacentM, minPath);
                ++count;
            }

            return _shortestPaths;
        }

        private void UpdateTempPath(float[,] adjacentM, Path minPath)
        {
            foreach (var path in _tempPaths)
            {
                float newLength = minPath.Length + adjacentM[minPath.VertexIndex, path.VertexIndex];
                if (newLength < path.Length)
                {
                    path.Length = newLength;
                    path.PreIndex = minPath.VertexIndex;
                }
            }
        }

        private void SelectMinLengthPath(out Path minPath)
        {
            float minLength = _tempPaths[0].Length;
            minPath = _tempPaths[0];
            foreach (var path in _tempPaths)
            {
                if (path.Length == float.MaxValue)
                {
                    continue;
                }

                if (path.Length < minLength)
                {
                    minPath = path;
                    minLength = path.Length;
                }
            }
        }

        private void InitPaths(float[,] adjacentM, int startIndex)
        {
            _tempPaths.Clear();
            _shortestPaths.Clear();
            int n = adjacentM.GetLength(0);
            for (int i = 0; i < n; ++i)
            {
                if (i == startIndex)
                {
                    continue;
                }
                _tempPaths.Add(new Path()
                {
                    Length = adjacentM[startIndex, i],
                    PreIndex = startIndex,
                    VertexIndex = i
                });
            }
        }
    }
}
