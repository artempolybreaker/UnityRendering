using UnityEngine;

namespace DefaultNamespace
{
    public abstract class TransformationBase : MonoBehaviour
    {
        public abstract Matrix4x4 Matrix { get; }

        public Vector3 Apply(Vector3 point)
        {
            return Matrix.MultiplyPoint(point);
        }
    }
}