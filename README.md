# 3Dinoz Games — Developer Case Study

A 3D mobile puzzle game built with Unity where players sort colored rings from hanging chains onto matching color sticks, racing against a countdown timer.

---

## Gameplay Overview

- **Chains** hang from the top of the screen, each holding a stack of colored rings.
- Tap a chain to **release the bottom ring** — it automatically flies to the nearest available stick of the same color.
- Each stick has a **capacity** (default: 3 rings). When a stick is fully filled, it **disappears** with a particle effect and the remaining sticks in the column slide up.
- **Win condition:** All chains are emptied.
- **Lose condition:** The countdown timer reaches zero.

---

## Project Structure

```
Assets/
├── Script/
│   ├── Editor/
│   │   └── LevelDataEditor.cs     # Custom Inspector for LevelData
│   ├── ChainController.cs         # Manages one chain: spawns rings, handles release logic
│   ├── ChainClick.cs              # Detects tap/click on a chain collider
│   ├── ChainSceneSetup.cs         # Spawns chain prefabs into the scene at runtime
│   ├── StickController.cs         # Single stick: color, capacity, ring acceptance
│   ├── StickColumnController.cs   # Column of sticks: disappear & slide-up animations
│   ├── StickLevelSpawner.cs       # Spawns stick columns into the scene at runtime
│   ├── RingController.cs          # Single ring: color, idle sway, movement animation
│   ├── GameManager.cs             # Timer, win/lose detection, UI panel animations
│   ├── LevelData.cs               # ScriptableObject holding level configuration
│   ├── ColorMaterialConfig.cs     # ScriptableObject mapping RingColor → Material
│   └── RingColor.cs               # Enum: Red, Blue, Green, Yellow, Orange, Pink
```

---

## Level Editor

Levels are configured through **LevelData ScriptableObjects** with a custom Unity Inspector.

### Creating a Level

1. In the **Project** window, right-click → **Create → Level Data** (or use the existing assets).
2. Select the asset — the custom editor opens in the Inspector.

### Inspector Layout

| Section | Description |
|---|---|
| **Timer** | Level duration in seconds |
| **Color Palette** | Click a color circle to select it as the active paint color |
| **Chains** (max 3) | Each column (C1, C2, C3) = one chain. Each colored cell = one ring, **top → bottom** order. Bottom row = released first. |
| **Stick Columns** (max 5) | Each column (S1, S2, S3…) = one stick column. Each colored cell = one stick. Row 1 = top stick (active first), subsequent rows slide up after each fill. |

### Editing Tips

- **Paint a cell:** Select a color from the palette → click any colored cell to repaint it.
- **Add a ring/stick:** Click `+` below the column.
- **Remove the last ring/stick:** Click `–` below the column.
- **Remove an entire column:** Click the red `X` button.
- **Add a new column:** Click the green `+C` / `+S` button on the right.
- All edits support **Undo** (`Ctrl+Z`).

### Ring Count Rule

> **Total rings in all chains = Total stick capacity across all columns**
>
> `chain_count × rings_per_chain = column_count × sticks_per_column × maxCapacity`
>
> If this doesn't balance, some rings will have nowhere to go and the level cannot be completed.

---

## Scene Setup

The **Game** scene hierarchy has three key GameObjects at the bottom that must be configured before Play:

### GameManager

| Field | Description |
|---|---|
| Timer Text | Assign the `TimerText (TMP)` UI element |
| Level Data | Drag the target **LevelData** asset (e.g. `Level_01`) |
| Win Panel | Assign the `WinPanel` UI GameObject |
| Lose Panel | Assign the `LosePanel` UI GameObject |

### ChainSceneSetup

| Field | Description |
|---|---|
| Chain Root Prefab | Drag the `ChainRoot` prefab |
| Level Data | Same **LevelData** asset as GameManager |
| Horizontal Spacing | Horizontal gap between chains (default: `1.5`) |
| Start Position | World position where the center chain spawns (e.g. `X:0 Y:5 Z:0`) |

### StickLevelSpawner

| Field | Description |
|---|---|
| Stick Prefab | Drag the `Stick Variant` prefab |
| Level Data | Same **LevelData** asset as GameManager |
| Color Config | Drag the `ColorMaterialConfig` ScriptableObject |
| Horizontal Spacing | Gap between stick columns (default: `1`) |
| Stick Spacing | Vertical gap between sticks in a column (default: `1.5`) |
| Disappear Particle Prefab | VFX prefab played when a stick disappears (e.g. `CFXR3 Hit Light B (Air)`) |
| Start Position | World position of the leftmost column center (e.g. `X:0 Y:-1 Z:0`) |

> **Important:** All three GameObjects must reference the **same LevelData asset** so the scene is consistent.

---

## Assets & Tools

### 3D Models
- **Hook:** Custom-made in **Blender** and exported to Unity.

### UI
- **Win Panel** and **Lose Panel** visuals downloaded from the Unity Asset Store / external sources and integrated into the Canvas.

### VFX
- **Stick disappear effect** (`CFXR3 Hit Light B (Air)`) downloaded from the Unity Asset Store and assigned to the `StickLevelSpawner` → `Disappear Particle Prefab` field.

---

## Case Requirements

All requirements specified in the case document have been implemented:

- ✅ Colored rings hanging from chains
- ✅ Tap interaction to release the bottom ring
- ✅ Rings automatically fly to the correct matching stick
- ✅ Sticks fill up and disappear with animation (punch scale → shrink → VFX)
- ✅ Remaining sticks slide up after one disappears
- ✅ Win condition: all chains emptied
- ✅ Lose condition: timer reaches zero
- ✅ Animated Win/Lose UI panels (scale bounce)
- ✅ Data-driven level system via ScriptableObjects
- ✅ Custom Level Editor for fast level design iteration

---

## Development Notes

All gameplay scripts were developed with the assistance of **[Windsurf](https://codeium.com/windsurf)** (Codeium's agentic AI IDE). The AI was used as a pair-programming tool for architecture decisions, script implementation, custom editor UI, and iterative refinements throughout the project.

---

## How to Run

1. Open the project in **Unity 2022.3.62f2**.
2. Open the `Game` scene.
3. Assign a **LevelData** asset to `GameManager`, `ChainSceneSetup`, and `StickLevelSpawner` in the Inspector.
4. Press **Play**.
