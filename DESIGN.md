# Game Logic Architecture

This document describes a proposed split of responsibilities between the major components.

---

## Core Principle: State vs. Logic

**`MatchState`** owns all data that must survive a save/load or a network sync. State classes may have small methods that describe their own behavior — a `Unit` can know how to apply damage to itself, a `Health` struct can know what clamped heal means. What state classes must not do is coordinate with other systems or drive the turn forward.

**`MatchEngine`** owns coordination. It reads and writes `MatchState` but holds no persistent data of its own. If you restarted the engine from a saved `MatchState`, the game should continue identically.

---

## MatchState (data + self-contained behavior)

```
MatchState
├── Board
├── Units[]
├── Players[]
├── TurnContext          ← current Turn, Phase, Initiative, Moment
├── LiveEffects[]        ← effects currently active (persistent, affect stats)
└── QueuedPlayerActions  ← submitted player decisions waiting for resolution
```

Notice what is **not** in `MatchState`: `IntentQueue`, `ZoneChangeQueue`, `SummonQueue`, `MovementQueue`. Those are transient — they exist only within a single resolution pass and are managed inside `MomentResolver` as local state. They do not need to survive a save/load.

`LiveEffects` **is** in state because ongoing effects (auras, DOTs, shields) persist across resolution passes and affect stat queries at any time. A `QueuedEffect` that hasn't been enabled yet does not need to be in state either — the moment it is realized, it becomes a `LiveEffect`.

`QueuedPlayerActions` **is** in state because it bridges the decision phase and the resolution phase: players submit during one phase, resolution consumes during the next.

---

## MatchEngine (coordination)

```
MatchEngine
├── TurnFlowController    ← "what phase are we in and what happens next?"
├── DecisionGateway       ← "collect decisions from all players simultaneously"
└── MomentResolver        ← "run everything until stable"
    └── subsystems: EffectsTracker, IntentsResolver, ZoneChangeResolver,
                   SummoningResolver, MovementResolver, StatsEvaluator
```

### TurnFlowController (replaces GameStateManager)

Drives the turn as a **state machine**. The current phase is stored in `MatchState.TurnContext` so it is serializable.

```
Upkeep              → automatic, run MomentResolver
PlayDecisions       → pause until DecisionGateway reports all players ready
PlayResolution      → automatic: translate QueuedPlayerActions into Intents, run MomentResolver
MovementDecisions   → pause until DecisionGateway reports all players ready
MovementResolution  → automatic: translate QueuedPlayerActions into Intents, run MomentResolver
EndOfTurn           → automatic, advance turn counter
```

`TurnFlowController.Tick()` advances one step. It returns one of two outcomes:
- `AutomaticResult` — a phase ran and completed, call Tick() again
- `AwaitingDecisions` — waiting on player input, do not Tick() until `DecisionGateway` signals ready

### DecisionGateway (replaces PlayerDecisionsHandler)

Collects decisions from **all players simultaneously**. When a decision phase begins, `TurnFlowController` tells `DecisionGateway` what kind of decisions are needed. Players submit independently. Once all required players have submitted, `DecisionGateway` writes their choices to `MatchState.QueuedPlayerActions` and signals ready.

It does not know the phase sequence — that belongs to `TurnFlowController`. It only knows how to accept submissions and when the set is complete.

### MomentResolver

Runs the fixed-point loop until no subsystem has work to do.

```csharp
while (true) {
    if (effectsTracker.RealizeEffects(state))        continue;
    if (intentsResolver.RealizeIntents(intents))     continue;
    if (zoneChangeResolver.ProcessZoneChanges(...))  continue;
    if (summoningResolver.ProcessSummons(...))       continue;
    if (movementResolver.ProcessMovement(...))       continue;
    break;
}
```

The ordering is deliberate: effects run first because enabling them may cause immediate stat changes that other systems react to; intents run next because they may cause zone changes or summons; movement is last.

`MomentResolver` owns the transient queues (`IntentQueue`, `ZoneChangeQueue`, etc.) as local working memory for the duration of a single resolution. They are not passed in from outside. At the start of a resolution pass, `IntentQueue` is populated from `QueuedPlayerActions` (translated by `TurnFlowController` before handing off), then grows and shrinks as subsystems run.

---

## Effects vs. Intents vs. PlayerActions

Three distinct concepts, each with a different lifetime:

**`PlayerAction`** — persists in `MatchState` between phases. A player's choice (e.g. "play card X targeting unit Y", "move to hex Z"). Lives from submission through the start of resolution, then is consumed.

**`Intent`** — lives only within a single resolution pass. Represents a concrete game event that is about to happen ("deal 3 damage to unit X", "summon unit at position Y"). Created from `PlayerAction`s at the start of resolution, and also by Effects and other Intents during the fixed-point loop. Consumed when processed.

**`Effect`** — persists in `MatchState.LiveEffects` across resolution passes. Represents an ongoing modifier (aura, DOT, shield). Enabled during resolution; remains active until explicitly disabled. Hooks into `StatsEvaluator` to modify stat queries.

Common flow:
```
PlayerAction (PlayCard)
  → Intent (SummonUnit, ApplyDamageEffect)
    → new LiveEffect (DamageEffect enabled)
      → StatsEvaluator now returns reduced health for that unit
```

---

## What Doesn't Belong Where

- **State classes can have self-contained methods** (`Health.ApplyDamage()`, `Unit.IsAlive()`). They must not coordinate between systems or call into the engine.
- **Subsystems must not hold persistent data.** If a subsystem needs to remember something between resolution passes, that memory belongs in `MatchState`.
- **`TurnFlowController` must not know specific game rules** (e.g. "what does upkeep do to DOT units"). That rule lives inside a specific Intent or Effect. `TurnFlowController` only knows phase order and when to hand off to `MomentResolver`.
- **`DecisionGateway` must not know phase order.** It just knows how to accept submissions and when the set is complete.
- **Intents must not persist in `MatchState`** — if you're tempted to save an Intent to state, it should probably be a `PlayerAction` or a `LiveEffect` instead.

---

## Open Questions

1. **Effect removal triggers**: when an Effect is disabled, should it be able to enqueue new Intents (e.g. "when this shield breaks, deal 2 damage")? If yes, `Disable()` needs access to the active `IntentQueue`, which means it must be called from within `MomentResolver` and never directly.

2. **`StatsEvaluator` event model**: WIP.
