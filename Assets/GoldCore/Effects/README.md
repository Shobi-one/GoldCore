# GoldCore Effects

Particle effects described as data. An **Effect** preset is a list of layers (flash, fireball, smoke,
sparks, debris), plus an optional light flash, camera shake and sound. Every value is a field in the
Inspector, so changing an effect never means opening a particle system.

The particle sprites (soft dot, hard dot, ring, smoke, spark, square) are drawn by the shader, so the
presets need no textures. Pick **Texture** as the shape to use your own.

## Presets

| Preset | Use |
| --- | --- |
| `Explosion` | Rockets and barrels. Flash, fireball, smoke, bouncing sparks and debris, light, shake |
| `MuzzleFlash` | Every gun. Follows the barrel and takes its layer, so the weapon camera draws it |
| `BulletImpact` | Default hit on untagged surfaces: dust puff, sparks and chips |
| `Sparks` | Metal surfaces, broken cables |
| `Blood` | Hits on anything with `Health` |
| `Dust` | Landings, falling debris, concrete |
| `Smoke` | Looping. Vents, wreckage |
| `Fire` | Looping. Flames, embers and smoke |
| `WaterSplash` | Water surfaces |

To make a variant, duplicate a preset (Ctrl+D) and change it. **Scale** resizes the whole effect, so
a grenade can be the explosion at 0.6.

## Tuning

Select a preset and press **Preview**. It plays at the center of the Scene view, in edit mode too,
and restarts as you change values. Tick **Repeat** to keep it looping. In Play mode, edited presets
rebuild the next time they play.

Useful fields on a layer:

- **Pixelate** draws each particle as a small pixel grid, which matches RetroFX's PS1 look.
- **Intensity** above 1 makes additive layers glow, and bloom picks them up.
- **Stretch** turns particles into streaks along their velocity, for sparks and tracers.
- **Gravity** below 0 makes smoke and flames rise. **Drag** slows particles down.
- **Bounce** collides particles with the world. It costs more, so keep it to sparks and debris.
- Hemisphere and Cone emitters point along the effect's forward axis, which is the surface normal
  for impacts.

## Using effects

- **Drag and drop:** add an **Effect Player** (GameObject > GoldCore > Effect Player, or Add Component)
  and pick a preset. It plays on enable, and `Play()` and `Stop()` can be called from any UnityEvent.
- **Weapons:** `GunSystem` has Muzzle Effect, Impact Effect and Blood Effect. `Rocket` has Explosion Effect.
- **Surfaces:** a Surface Property's Impact Preset plays on every bullet or melee hit on that surface.
- **Code:**

  ```csharp
  Effects.Play(explosion, hit.point);                 // at a point
  Effects.Play(sparks, hit);                          // out of a surface
  var fire = Effects.Play(fireLoop, pos, rot, transform); // follow a transform
  fire.Stop();                                        // looping effects fade out
  ```

Effects are pooled per preset, and **Max Instances** caps how many copies play at once (the oldest
is reused), so automatic weapons don't pile up particles.

## Camera shake

Presets with **Shake** above 0 shake every camera in range, less with distance. The main camera gets a
`CameraShake` the first time something shakes; add one yourself to change its strength or recovery.
`GunSystem`'s Cam Shake Magnitude shakes the camera on every shot.
