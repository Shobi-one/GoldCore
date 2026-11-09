using UnityEngine;
using UnityEngine.InputSystem;

namespace GoldCore
{
    /// <summary>
    /// The one place gameplay reads input from, built on the Input System package.
    /// Keyboard and mouse plus gamepad bindings are set up in code, so there's no asset to wire.
    /// Keyboard bindings follow key position, so WASD is ZQSD on AZERTY automatically.
    /// While <see cref="Blocked"/> is true (console or menu open) every action reads as idle.
    /// Rebind through <see cref="Actions"/> and call <see cref="SaveBindings"/>.
    /// </summary>
    public static class GameInput
    {
        const string BindingsKey = "goldcore.bindings";

        /// <summary>Mouse delta is multiplied by this so it matches the old Input Manager's Mouse X/Y axes.</summary>
        public static float MouseScale = 0.1f;
        /// <summary>Gamepad look speed, in the same units as mouse look per second.</summary>
        public static float StickLookSpeed = 15f;

        static InputActionMap map;
        static InputAction move, look, lookStick, jump, crouch, sprint, attack, attack2, reload, use,
            flashlight, console, scroll, nextWeapon, previousWeapon, resetPosition;
        static readonly InputAction[] slots = new InputAction[6];
        static int blockers;

        /// <summary>All gameplay actions, for rebinding UIs.</summary>
        public static InputActionMap Actions
        {
            get
            {
                if (map == null) Build();
                return map;
            }
        }

        /// <summary>True while any UI holds input. Use <see cref="PushBlock"/> and <see cref="PopBlock"/>.</summary>
        public static bool Blocked => blockers > 0;
        public static void PushBlock() => blockers++;
        public static void PopBlock() => blockers = Mathf.Max(0, blockers - 1);

        // Movement
        public static Vector2 Move => Read2(move);
        public static bool JumpHeld => Held(jump);
        public static bool JumpPressed => Pressed(jump);
        public static bool CrouchHeld => Held(crouch);
        public static bool CrouchPressed => Pressed(crouch);
        public static bool SprintHeld => Held(sprint);
        /// <summary>Fragsurf's test bind (Backspace) that puts the player back at the start.</summary>
        public static bool ResetPositionPressed => Pressed(resetPosition);

        /// <summary>Look input this frame, in old Mouse X/Y axis units (x right, y up).</summary>
        public static Vector2 Look
        {
            get
            {
                if (Blocked) return Vector2.zero;
                if (Actions == null) return Vector2.zero;
                Vector2 mouse = look.ReadValue<Vector2>() * MouseScale;
                Vector2 stick = lookStick.ReadValue<Vector2>() * (StickLookSpeed * Time.unscaledDeltaTime);
                return mouse + stick;
            }
        }

        // Combat and interaction
        public static bool AttackHeld => Held(attack);
        public static bool AttackPressed => Pressed(attack);
        public static bool SecondaryAttackHeld => Held(attack2);
        public static bool ReloadPressed => Pressed(reload);
        public static bool UsePressed => Pressed(use);
        public static bool FlashlightPressed => Pressed(flashlight);

        /// <summary>Positive for next weapon, negative for previous, 0 for none.</summary>
        public static float WeaponScroll
        {
            get
            {
                if (Blocked) return 0f;
                float value = Actions != null ? scroll.ReadValue<float>() : 0f;
                if (nextWeapon.WasPressedThisFrame()) value += 1f;
                if (previousWeapon.WasPressedThisFrame()) value -= 1f;
                return value;
            }
        }

        /// <summary>Weapon slot key pressed this frame (0 to 5), or -1.</summary>
        public static int SlotPressed
        {
            get
            {
                if (Blocked || Actions == null) return -1;
                for (int i = 0; i < slots.Length; i++)
                    if (slots[i].WasPressedThisFrame()) return i;
                return -1;
            }
        }

        /// <summary>The console toggle works even while input is blocked.</summary>
        public static bool ConsolePressed => Actions != null && console.WasPressedThisFrame();

        public static void SaveBindings()
        {
            PlayerPrefs.SetString(BindingsKey, Actions.SaveBindingOverridesAsJson());
            PlayerPrefs.Save();
        }

        public static void ResetBindings()
        {
            Actions.RemoveAllBindingOverrides();
            PlayerPrefs.DeleteKey(BindingsKey);
        }

        static bool Held(InputAction action) => !Blocked && Actions != null && action.IsPressed();
        static bool Pressed(InputAction action) => !Blocked && Actions != null && action.WasPressedThisFrame();
        static Vector2 Read2(InputAction action) => Blocked || Actions == null ? Vector2.zero : action.ReadValue<Vector2>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            // Works with Enter Play Mode Options (no domain reload).
            blockers = 0;
            if (map != null)
            {
                map.Disable();
                map.Dispose();
                map = null;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Build()
        {
            if (map != null) return;
            map = new InputActionMap("GoldCore");

            move = map.AddAction("Move", InputActionType.Value, expectedControlLayout: "Vector2");
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
            move.AddBinding("<Gamepad>/leftStick");

            look = map.AddAction("Look", InputActionType.PassThrough, "<Mouse>/delta", expectedControlLayout: "Vector2");
            lookStick = map.AddAction("LookStick", InputActionType.Value, "<Gamepad>/rightStick", expectedControlLayout: "Vector2");

            jump = Button("Jump", "<Keyboard>/space", "<Gamepad>/buttonSouth");
            crouch = Button("Crouch", "<Keyboard>/leftCtrl", "<Gamepad>/buttonEast");
            sprint = Button("Sprint", "<Keyboard>/leftShift", "<Gamepad>/leftStickPress");
            attack = Button("Attack", "<Mouse>/leftButton", "<Gamepad>/rightTrigger");
            attack2 = Button("Attack2", "<Mouse>/rightButton", "<Gamepad>/leftTrigger");
            reload = Button("Reload", "<Keyboard>/r", "<Gamepad>/buttonWest");
            use = Button("Use", "<Keyboard>/e", "<Gamepad>/buttonNorth");
            flashlight = Button("Flashlight", "<Keyboard>/f", "<Gamepad>/dpad/up");
            // Backquote is Source's console key. Bindings follow key position, so on AZERTY it is the key left of 1.
            console = Button("Console", "<Keyboard>/backquote", "<Keyboard>/f1");
            nextWeapon = Button("NextWeapon", "<Gamepad>/rightShoulder", null);
            previousWeapon = Button("PreviousWeapon", "<Gamepad>/leftShoulder", null);
            resetPosition = Button("ResetPosition", "<Keyboard>/backspace", null);
            scroll = map.AddAction("Scroll", InputActionType.PassThrough, "<Mouse>/scroll/y", expectedControlLayout: "Axis");
            for (int i = 0; i < slots.Length; i++)
                slots[i] = Button("Slot" + (i + 1), "<Keyboard>/" + (i + 1), null);

            string saved = PlayerPrefs.GetString(BindingsKey, "");
            if (!string.IsNullOrEmpty(saved)) map.LoadBindingOverridesFromJson(saved);
            map.Enable();
        }

        static InputAction Button(string name, string primary, string secondary)
        {
            InputAction action = map.AddAction(name, InputActionType.Button);
            if (primary != null) action.AddBinding(primary);
            if (secondary != null) action.AddBinding(secondary);
            return action;
        }
    }
}
