using UnityEngine;

namespace GoldCore
{
    /// <summary>
    /// Says which <see cref="SurfaceProperty"/> a collider is made of. It covers its child colliders too,
    /// so one tag on a building's root is enough.
    /// </summary>
    [DisallowMultipleComponent]
    public class SurfaceTag : MonoBehaviour
    {
        public SurfaceProperty surface;

        /// <summary>The surface for a collider, or null when nothing is tagged.</summary>
        public static SurfaceProperty Find(Collider collider)
        {
            if (collider == null) return null;
            var tag = collider.GetComponentInParent<SurfaceTag>();
            return tag != null ? tag.surface : null;
        }
    }
}
