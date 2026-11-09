# RetroFX

GoldCore's own retro look, replacing the paid PSXEffects asset. It is built for the built-in
render pipeline, which is what GoldCore uses.

## Setup

1. Add **RetroCamera** to a camera (the Player prefab's main camera already has it).
2. Drag a preset from `Assets/GoldCore/RetroFX/Presets` into **Preset**.
3. For wobbling vertices and warped textures, switch materials to **GoldCore/RetroLit**:
   select them and use **Tools > GoldCore > Convert Selected Materials to RetroLit**.
   The image effects (resolution, color, dithering, fog, blur, bloom, scanlines) work on every material.

On an overlay camera, such as the Player's weapon camera, add RetroCamera with **Post Process** off,
or leave it off entirely.

## Presets

| Preset | What it does |
| --- | --- |
| PS1 | 240p with hard pixels, 15-bit color with full dithering, warm shadows, vertex snapping, affine texture warping, point sampled textures |
| N64 | 240p scaled up smoothly, 15-bit color with light dithering, soft blur, distance fog, no wobble |
| PS2 | 448p, mild color banding and dithering, light bloom, slight softness, stable geometry |
| Modern | Off. The camera renders normally and RetroLit materials draw without snapping or warping |

Each setting is independent, so a preset is just a mix. Make your own with
**Assets > Create > GoldCore > Retro Preset**, or duplicate one of the four.

Switch at runtime with `GetComponent<RetroCamera>().SetPreset(preset)`, or press the cycle key
(F8 on the Player camera) to step through **Cycle Presets**.

## Settings

- **Vertical Resolution**: height the image is rendered at. 0 keeps the screen resolution.
- **Pixelated Upscale**: hard pixels when scaling up; off gives the soft, filtered look.
- **Color Bits**: bits per channel (5 = 15-bit color, 8 = off).
- **Dither**: Bayer 4x4 ordered dithering in the low resolution pixels.
- **Warm Shadows**, **Saturation**: simple grading.
- **Blur**, **Bloom**, **Bloom Threshold**: softness and glow.
- **Fog**: distance fog from the depth buffer, so it covers the sky at the far plane too.
- **Scanlines**: darkens every other low resolution row.
- **Vertex Snap**, **Affine Textures**, **Point Sample Textures**: RetroLit materials only.
  A material's **Vertex Snap Override** replaces the preset's value for that material.
