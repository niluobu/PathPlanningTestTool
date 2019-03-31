using UnityEngine;

namespace Project.Test
{
    internal class UnitTesting : MonoBehaviour
    {
        public Vector2Int S;
        public Vector2Int A;
        public Vector2Int B;
        public Vector2Int RayDir;

        private void Update()
        {
            Debug.Log($"{PartitionEdgeByRay(S, RayDir, A, B)}");
        }

        private float GetClockwiseAngle(Vector2Int from, Vector2Int to)
        {
            float angle = Vector3.SignedAngle(new Vector3(from.x, from.y, 0), new Vector3(to.x, to.y, 0), Vector3.back);
            if (angle < 0)
            {
                angle = 360f + angle;
            }
            return angle;
        }

        private int PartitionEdgeByRay(Vector2Int s, Vector2Int rayDir, Vector2Int a, Vector2Int b)
        {
            Vector2Int se1 = a - s;
            Vector2Int se2 = b - s;
            float angle1 = GetClockwiseAngle(rayDir, se1);
            float angle2 = GetClockwiseAngle(rayDir, se2);
            if (angle1 > 180f && angle2 > 180f)
            {
                return -1;
            }

            if (angle1 < 180f && angle2 < 180f)
            {
                return 1;
            }

            if ((angle1 < 90f && angle2 > 180f) || (angle2 < 90f && angle1 > 180f))
            {
                return 0;
            }

            return 1;
        }
    }
}
