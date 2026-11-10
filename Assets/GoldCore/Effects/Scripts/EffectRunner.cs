using UnityEngine;

namespace GoldCore
{
    /// <summary>Drives every playing effect from one Update. Added by <see cref="Effects"/>.</summary>
    [AddComponentMenu("")]
    public class EffectRunner : MonoBehaviour
    {
        void Update() => Effects.TickAll(Time.deltaTime);
    }
}
