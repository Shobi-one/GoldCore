# GoldCore

A Unity framework for building and prototyping first-person games, modeled on the Source Engine.
Each module lives in its own folder under `Runtime/` and talks to the others through small shared
types in `Core/`, so you can drop in the parts you need.

See the [framework plan](https://claude.ai/artifact/R5AVBXxVVHqD5kF9Gun8je) for where this is going.

## Modules

| Folder | What it does |
| --- | --- |
| `Runtime/Core` | `DamageInfo`, `DamageType` flags, `IDamageable` with `Damage.Apply`, and `GameInput`, the one place gameplay reads input (Input System, keyboard, mouse and gamepad, rebindable) |
| `Runtime/Health` | `Health` (health, armor with the Half-Life 2 armor ratio, god mode, events) and `HealthManager`, the player's health plus HUD numbers |
| `Runtime/Surfaces` | `SurfaceProperty` assets (decal, effect, impact and footstep sounds), `SurfaceTag` for colliders, `Impacts`, `Footsteps` |
| `Runtime/Weapons` | `GunSystem` (hitscan, shotgun, burst, projectile), `MeleeWeapon`, `RocketLauncher` and `Rocket` with blast damage, `WeaponScroller`, sway |
| `Runtime/Items` | Health, armor and ammo pickups that only the player can take |
| `Runtime/Entities` | `Button` for now. Source-style triggers and entity I/O come next |
| `Runtime/Physics` | Carry and throw props with the use key |
| `Runtime/NPC` | `EnemyAI` (melee) and `EnforcerAI` (ranged), both using `Health` |
| `Runtime/Console` | Developer console: `help`, `map`, `maps`, `impulse 101`, `god`, `kill`, `sv_autobunnyhop`, `clear`, `quit`, history with the arrow keys |
| `Effects` | Particle effects as editable presets (explosion, muzzle flash, impacts, blood, fire, smoke and more), pooled, with a light flash, camera shake and sound. See its README |
| `RetroFX` | PS1, N64, PS2 and Modern looks. See its README |
| `ThirdParty/Fragsurf` | Source-style movement (bhop, air strafing, ladders, water) by Ole Ellefsen Farner, MIT licensed, with GoldCore changes |

## Setup

GoldCore targets Unity 6 with URP and the Input System package.

1. Convert the project with **Window > Rendering > Render Pipeline Converter** (Built-in to URP).
2. Run **Tools > GoldCore > Set Up URP**. It adds RetroFX to the URP renderers and stacks the Player's
   weapon camera as a URP overlay camera.

## Controls

| Action | Keyboard and mouse | Gamepad |
| --- | --- | --- |
| Move | WASD or arrows | Left stick |
| Look | Mouse | Right stick |
| Jump | Space | A / Cross |
| Crouch | Left Ctrl | B / Circle |
| Sprint | Left Shift | Left stick press |
| Attack | Left mouse | Right trigger |
| Reload | R | X / Square |
| Use | E | Y / Triangle |
| Flashlight | F | D-pad up |
| Weapons | Scroll, 1 to 6 | Shoulder buttons |
| Console | Key left of 1 (` on US layouts), F1 | |

Keys follow position, so on AZERTY the movement keys are ZQSD. Rebind through `GameInput.Actions`
and save with `GameInput.SaveBindings()`.

## Making something damageable

Add a `Health` component. Weapons, rockets and NPC attacks find it on the collider they hit or any
of its parents. To react, hook up `onDamaged`, `onDeath` or `onChanged` in the Inspector.

## Surfaces

Create a Surface Property (Assets > Create > GoldCore > Surface Property), give it a decal, an impact
effect and sounds, and put a `SurfaceTag` on the object. Surfaces without a tag still get the weapon's
own bullet hole and impact effect.
