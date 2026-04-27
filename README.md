# Board Game Logic Sketch

A .NET 8 project for developing turn-based hex board game logic with deterministic replay support, designed for later integration with Unity.

## Project Structure

```
├── Game.sln                 # Solution file
├── src/
│   ├── Game.Core/           # Pure C# game logic (no Unity dependencies)
│   │   ├── Actions.cs       # Game action types
│   │   ├── Events.cs        # Game events and StepResult
│   │   ├── GameEngine.cs    # Core engine: GetLegalActions, Apply, ProjectToView
│   │   ├── Hex.cs           # Axial hex coordinates and utilities
│   │   ├── Ids.cs           # PlayerId, EntityId value types
│   │   ├── Rng.cs           # Deterministic RNG for replay support
│   │   └── State.cs         # GameState (authoritative) and PlayerView (redacted)
│   └── Game.Sim/            # Console simulator for testing/debugging
│       └── Program.cs       # CLI runner with seed/steps/dump options
└── tests/
    └── Game.Tests/          # xUnit tests
        ├── HexTests.cs      # Hex coordinate tests
        └── RngTests.cs      # Deterministic RNG tests
```

## Architecture

### Separation of Concerns

- **Game.Core**: Contains all game rules and logic. No UI, no Unity. This is what you'll import into Unity later.
- **Game.Sim**: Console app for quick testing and deterministic replay verification.
- **Game.Tests**: Automated tests for rules, determinism, and invariants.

### Key Design Patterns

1. **Hidden Information**: `GameState` is the authoritative "god view"; `PlayerView` is the redacted view for each player.
2. **Deterministic Replay**: All randomness goes through `IRng`. Same seed = same game.
3. **Action/Event Model**: `GetLegalActions` → `Apply` → `StepResult` with public/private events.

## Quick Start

### Build

```bash
dotnet build
```

### Run Tests

```bash
dotnet test
```

### Run Simulator

```bash
# Basic run (1 step, seed 0, 2 players)
dotnet run --project src/Game.Sim

# Custom parameters
dotnet run --project src/Game.Sim -- --seed 12345 --steps 10 --players 3

# Dump final state as JSON
dotnet run --project src/Game.Sim -- --seed 42 --steps 5 --dump

# Help
dotnet run --project src/Game.Sim -- --help
```

## Adding Game Logic

1. Define your entities and their state in `State.cs`
2. Add action types in `Actions.cs` (extend `GameAction`)
3. Add event types in `Events.cs` (extend `GameEvent`)
4. Implement rules in `GameEngine.cs`:
   - `GetLegalActions`: What can each player do?
   - `Apply`: Process actions and emit events
   - `ProjectToView`: Hide secrets from each player's view

## Unity Integration (Future)

When ready to add Unity:

1. Create a Unity project
2. Import `Game.Core.dll` (or reference the project)
3. Create a Unity-side renderer that:
   - Calls `GameEngine.ProjectToView()` to get visible state
   - Calls `GameEngine.GetLegalActions()` for UI hints
   - Sends `GameAction`s to `GameEngine.Apply()`
4. **Never** let Unity code make rule decisions—always delegate to `Game.Core`

## Testing Strategy

- **Determinism**: Same seed + same actions = same final state
- **Invariants**: Piece counts, turn order, no impossible states
- **No-leak**: `PlayerView` never contains hidden information
- **Snapshots**: JSON dumps for regression testing
