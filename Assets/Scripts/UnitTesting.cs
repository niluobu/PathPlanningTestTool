using UnityEngine;

namespace Project.Test
{
    internal class UnitTesting : MonoBehaviour
    {
        public Vector2Int From;
        public Vector2Int To;

        private void Update()
        {
            Debug.Log($"{GetClockwiseAngle(From, To)}");
        }

        private float GetClockwiseAngle(Vector2Int from, Vector2Int to)
        {
            float angle = Vector3.SignedAngle(new Vector3(from.x, from.y, 0), new Vector3(to.x, to.y, 0), Vector3.back);
            if (angle < 0)
            {
                angle = 180f - angle;
            }
            return angle;
        }
    }
}
