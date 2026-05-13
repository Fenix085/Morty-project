# Puzzle1 UI Arrows Fix

## Problem

`puzzle1` already had working keyboard movement through `PuzzlePlayerMovement` and `WASD`. The UI arrows were meant to trigger the same movement code, but clicks could do nothing because the UI bridge could resolve the wrong player object.

The scene has a `LevelGenerator` that spawns the actual puzzle player at runtime. There is also a `Player` object/reference in the scene used as the generator's `playerPrefab`. A generic lookup like `FindObjectOfType<PuzzlePlayerMovement>()` can find the prefab-source/player object instead of the runtime-spawned player that the puzzle logic tracks.

`puzzle1` also has a scene-specific setup issue: `LevelGenerator.playerPrefab` points at an active scene `Player` instance instead of directly at the prefab asset. That means the generator clones the scene object and the original can remain active too. This creates two active `PuzzlePlayerMovement` components.

## What Changed

### `Assets/Scripts/PuzzlePlayerMovemant.cs`

- Added public methods:
  - `MoveUp()`
  - `MoveDown()`
  - `MoveLeft()`
  - `MoveRight()`
- `WASD` now calls those public methods.
- Those public methods still call the existing `TryMove(...)`, so this reuses the existing movement, wall blocking, box pushing, and teleport behavior.
- Added a `Keyboard.current == null` guard so mobile/no-keyboard sessions do not throw before UI input can work.

### `Assets/Scripts/LevelGenerator1.cs`

- Added:

```csharp
public PuzzlePlayerMovement PlayerMovement => playerMovement;
```

- This exposes the actual player movement component that `LevelGenerator` found after spawning the runtime player.
- Added public UI-callable methods:
  - `MoveUp()`
  - `MoveDown()`
  - `MoveLeft()`
  - `MoveRight()`
- The arrow buttons now call these `LevelGenerator` methods directly. This avoids adding a separate bridge component to `UIArrows`.
- When spawning the player, the generator now:
  - names the spawned player `PuzzlePlayer`
  - assigns `currentLevelGenerator` on the spawned `PuzzlePlayerMovement`
  - disables the original scene `playerPrefab` object if the assigned prefab is actually a scene object

### `Assets/Scenes/puzzle1.unity`

- `UIArrows` no longer needs a custom movement script component.
- The four arrow buttons call:
  - `LevelGenerator.MoveUp()`
  - `LevelGenerator.MoveDown()`
  - `LevelGenerator.MoveLeft()`
  - `LevelGenerator.MoveRight()`

## Expected Runtime Path

1. `LevelGenerator.Start()` generates the level and spawns the real puzzle player.
2. If the configured `playerPrefab` is a scene object, the original template object is disabled.
3. `LevelGenerator.FindPlayer()` stores that spawned player's `PuzzlePlayerMovement`.
4. A UI arrow button click calls `LevelGenerator.Move...()`.
5. `LevelGenerator` forwards the call to the spawned player's `PuzzlePlayerMovement`.
6. The spawned player's `Move...()` method calls the existing `TryMove(...)`.

## If It Breaks Again

Check these first:

- `UIArrows` does not have a missing script component.
- Each arrow button `On Click` points to the `LevelGenerator` object.
- The methods selected in `On Click` are from `LevelGenerator`: `MoveUp`, `MoveDown`, `MoveLeft`, and `MoveRight`.
- Unity has no missing script warnings on `UIArrows`.
