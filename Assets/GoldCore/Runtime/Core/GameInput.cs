using UnityEngine;

namespace GoldCore
{
    /// <summary>
    /// The one place gameplay reads input from. While <see cref="Blocked"/> is true (the console
    /// or a menu is open) every action reads as not pressed, so nothing fires behind the UI.
    /// Uses the Input Manager names from ProjectSettings; swapping to the Input System later
    /// only changes this file.
    /// </summary>
    public static class GameInput
    {
        static int blockers;

        /// <summary>True while any UI holds input. Use <see cref="PushBlock"/> and <see cref="PopBlock"/>.</summary>
        public static bool Blocked => blockers > 0;

        public static void PushBlock() => blockers++;
        public static void PopBlock() => blockers = Mathf.Max(0, blockers - 1);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetOnPlay() => blockers = 0;

        public static KeyCode UseKey = KeyCode.E;
        public static KeyCode ReloadKey = KeyCode.R;

        public static bool AttackHeld => !Blocked && Input.GetButton("Fire1");
        public static bool AttackPressed => !Blocked && Input.GetButtonDown("Fire1");
        public static bool UsePressed => !Blocked && Input.GetKeyDown(UseKey);
        public static bool ReloadPressed => !Blocked && Input.GetKeyDown(ReloadKey);
        public static bool FlashlightPressed => !Blocked && Input.GetButtonDown("Flashlight");
        public static float WeaponScroll => Blocked ? 0f : Input.GetAxis("Mouse ScrollWheel");
        public static Vector2 Look => Blocked ? Vector2.zero : new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        /// <summary>The console toggle works even while input is blocked.</summary>
        public static bool ConsolePressed => Input.GetButtonDown("Console");
    }
}
