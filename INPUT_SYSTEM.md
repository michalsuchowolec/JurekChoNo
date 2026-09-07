# Intent Collection & Verification (Input System)

This document describes the layer that sits **in front of** `Intent` (see `DESIGN.md`): turning "a
player clicked things in a panel" into a validated, queued piece of pending unit/card state — a
`PlayerDecision` in the original sketch below, though that class doesn't survive this document's own
conclusions (see "No `PlayerDecision`, no central handler") — in a way the server and client can
negotiate over the network without the client ever having to duplicate game rules.

Most of `DESIGN.md` stays exactly as it is — `Intent.Perform`, `MomentResolver`'s drain loop,
`EffectsTracker`, and the `Stat<T>`/`Modifier<T>` system are untouched. One piece does not survive
this document, though: `PlayerDecision` and `PlayersDecisionsHandler`/`PlayerDecisions` are replaced
by decentralized, event-driven gathering (see "No `PlayerDecision`, no central handler" below) —
there is no `PlayerDecision.Do()` left to call.

---

## Client round-trip this is designed for

```
click unit
  → server: list available UnitActions for this unit right now
click an action icon
  → server: next required input + legal values for it (repeat until none left)
client performs inputs one by one, confirms
  → server: validate full selection, store the pending action on the unit/card itself, return delta
```

The "legal values" offered during discovery are a UX affordance, not the security boundary — the
final build step always re-validates independently of what was offered.

---

## `UnitAction`

A `UnitAction` is a capability class, not a single move — `Move`, `Rotate`, `UseAbility`,
`UseInvocation`. `Unit` holds a `List<UnitAction>` assembled from its template plus whatever
effects add/remove, exactly the same shape as the existing `_collisionBehaviours` list.

```csharp
public abstract class UnitAction
{
    public abstract UnitActionType Type { get; }

    // Coarse gate: is this action offered at all right now?
    public abstract bool IsAvailable(Unit unit, MomentResolver resolver);

    // Discovery: next thing the client needs to choose + legal values, given what's picked so
    // far. Null means the selection is complete and can be submitted.
    public abstract InputSlot? GetNextInputSlot(Unit unit, PartialSelection selection, MomentResolver resolver);

    // Commit: re-validate the *complete* selection and hand back a way to build the eventual
    // Intent, deferred until a moment-scoped resolver actually exists (see IntentFactory below).
    public abstract bool TryBuild(
        Unit unit, Player owner, PartialSelection selection,
        out IntentFactory? factory, out string? error);
}
```

This is also where "verification" becomes one place instead of scattered event handlers: today
`MoveUnitDecision.Do()` privately raises `CanPlanUnitAtionRequested` and computes a `check` value it
never actually branches on (worth fixing regardless — moot, in fact, since `Do()` itself is going
away, see below). `IsAvailable`/`TryBuild` become the single spot every action type's legality is
checked, still using the existing event+`Modifier<bool>` pattern for pluggable effects, just
centralized.

### `InputSlot` and `PartialSelection`

`InputSlot` is not just "which input is next" — it's *which* input and *what values are legal for
it*, returned together specifically so the client never needs a second round trip just to learn a
slot's domain:

```csharp
public abstract class InputSlot
{
    // Echoed back in the client's answer so PartialSelection knows which slot a submitted value
    // belongs to - "destination", "target", "invocation", etc.
    public string Key { get; }
    protected InputSlot(string key) => Key = key;
}

// "Choose one value out of an explicit, enumerable list" - invocations, ability targets, any
// slot whose legal set is small enough to just hand over outright.
public class PickOneInputSlot<T> : InputSlot
{
    public IReadOnlyList<T> Options { get; }
    public PickOneInputSlot(string key, IReadOnlyList<T> options) : base(key) => Options = options;
}

// "Choose a position + facing together" - the compound Move/placement case (see Compound inputs
// below). Both domains are handed over in one response so the client can build whatever
// rotate-then-place or place-then-rotate preview it wants without asking again.
public class PoseInputSlot : InputSlot
{
    public IReadOnlyList<HexPosition> LegalPositions { get; }
    public IReadOnlyList<HexDirection> LegalDirections { get; }
    public PoseInputSlot(string key, IReadOnlyList<HexPosition> positions, IReadOnlyList<HexDirection> directions)
        : base(key) { LegalPositions = positions; LegalDirections = directions; }
}
```

Other slot kinds (unit reference, tile-only, card reference) follow the same shape — a small, closed
set of `InputSlot` subclasses, the same pattern `HexPoseModifier`'s concrete subclasses already use
in this codebase, rather than one generic bag with a type-discriminator field to switch on.

`PartialSelection` is the client's answer accumulating across the round trip, keyed by the same
`Key` the offering `InputSlot` used:

```csharp
public class PartialSelection
{
    private readonly Dictionary<string, object> _values = new();
    public void Set(string key, object value) => _values[key] = value;
    public bool Has(string key) => _values.ContainsKey(key);
    public T Get<T>(string key) => (T)_values[key];
}
```

### `IActionInputSource` — shared delegation contract

`UseInvocation` needs to hand control to whichever `InvocationCard` was picked (that card owns its
own targeting — a heal invocation needs a unit target, a movement invocation might need a tile,
etc). Rather than special-casing that inside `UnitAction`, both a unit's built-in ability and a
card's invocation effect implement the same small interface:

```csharp
public interface IActionInputSource
{
    InputSlot? GetNextInputSlot(PartialSelection selection, MomentResolver resolver);
    bool TryBuild(Unit unit, Player owner, PartialSelection selection,
        out IntentFactory? factory, out string? error);
}
```

```csharp
public class UseInvocationUnitAction : UnitAction
{
    public override InputSlot? GetNextInputSlot(Unit unit, PartialSelection selection, MomentResolver resolver)
    {
        if (!selection.Has("invocation"))
            return new PickOneInputSlot<InvocationCard>("invocation", UsableInvocationsFor(unit, resolver)); // from PreparationZone

        return selection.Get<InvocationCard>("invocation").GetNextInputSlot(selection, resolver); // control passed to the card
    }

    public override bool TryBuild(Unit unit, Player owner, PartialSelection selection,
        out IntentFactory? factory, out string? error)
        => selection.Get<InvocationCard>("invocation").TryBuild(unit, owner, selection, out factory, out error);
}
```

`UseAbilityUnitAction` (unit's own active ability, once it exists) can be the same thin wrapper
around an `IActionInputSource` owned by the unit instead of by a card. Neither the client nor the
discovery loop needs to know or care whether it's talking to a built-in ability or a prepared
invocation — same `GetNextInputSlot`/submit contract either way.

`IActionInputSource` itself is deliberately the narrow, two-method shape — a *delegation target*,
used wherever something needs to gather its own inputs on behalf of whoever's currently holding the
selection. `UnitAction` is that same shape plus `Type`/`IsAvailable`, because it's also a *discovery
root* — one of the top-level choices offered when a player clicks a unit. Keep that distinction: an
`InvocationCard` has two *separate* input-gathering flows that happen to share the narrow shape —
playing it (trivial, see Cards below) and using it later from a unit's queue (the rich targeting
above) — they are different decisions and must stay different objects/methods, not one
`IActionInputSource` implementation trying to mean two things.

### No `PlayerDecision`, no central handler — gathering is event-driven

`Intent` construction can't happen at commit time: `MoveIntent`'s constructor binds to a specific
`MovementResolver` instance, and `MomentResolver` builds a fresh one for every single moment
(`_movementResolver = new MovementResolver(...)`), thrown away the instant `Drain()` finishes.
Commit happens during a decision-*collecting* phase, before that moment's resolver exists — so
*something* has to stay portable data until the right moment actually arrives.

Rather than solving that by keeping a `PlayerDecision` object around and having some central handler
walk a list of them calling `.Realize(resolver)`, the moment-scoped resolver only needs to reach the
right pending data at the right time — and `MomentResolver` already has a way to reach arbitrary
interested parties without knowing who they are: the same request/mutable-list idiom used everywhere
else in this codebase (`HealthModifiersRequested`, `AttackModifiersRequested`,
`CanPlanUnitAtionRequested` — publish an event, let subscribers append to a list on it, read the list
back after `Publish` returns), just pointed in the other direction. Instead of a `Unit` publishing
and effects subscribing, `MomentResolver` publishes and units (and cards) subscribe:

```csharp
public class IntentsGatheringRequested
{
    public int? Initiative { get; }         // set for MovementResolution, null for PlayResolution
    public MomentResolver Resolver { get; }  // this moment's live resolver - Events, MovementResolver, etc.
    public List<Intent> Intents { get; } = new();
    public IntentsGatheringRequested(int? initiative, MomentResolver resolver)
    { Initiative = initiative; Resolver = resolver; }
}
```

`MomentResolver` publishes this once per moment, replacing what `RealizeDecisions(decisions)` used
to do:

```csharp
private void GatherIntents(int? initiative)
{
    var request = new IntentsGatheringRequested(initiative, this);
    _events.Publish(request);
    foreach (var intent in request.Intents) _intentsResolver.QueueIntent(intent);
}
```

`Unit` subscribes once, for its whole lifetime (constructor — same as where it would subscribe to
anything else), and holds its own pending data directly instead of registering it into an external
dictionary keyed by unit:

```csharp
public delegate Intent? IntentFactory(MomentResolver resolver);

// on Unit:
private readonly IntentFactory?[] _pendingActions = new IntentFactory?[MovementInitiativeCount];

private void OnIntentsGathering(IntentsGatheringRequested request)
{
    if (request.Initiative is not int initiative) return; // not a unit-turn gathering
    var factory = _pendingActions[initiative - 1];
    if (factory is null) return;
    var intent = factory(request.Resolver);
    if (intent is not null) request.Intents.Add(intent);
}
```

`Card`/`PreparationZone` subscribes the same way for `Initiative is null` (Play-resolution
gathering).

This is the point where the last two turns' simplifications land together:

- **`PlayerDecision` is gone**, not just narrowed. Its only remaining job —
  `Realize(resolver) -> Intent` — is now the body of the `IntentFactory` closure that
  `UnitAction.TryBuild`/`CardAction.TryBuild` captures at commit time. There's no `Owner`/
  `IsCancelled` to track either: ownership is implicit (it's *this* unit's own array slot), and
  cancellation is just clearing that slot.
- **No central handler owns decision data.** The slot-assignment logic in the next section moves
  onto `Unit` itself (`unit.NextOpenInitiative()`, reading its own array) — this was always
  self-contained per-unit data, per the passives bucket-2 note above; it never needed to live in an
  external dictionary. Undo is `unit.ClearPendingAction(initiative)` — no stack, no
  `DequeueDecision`, no scoping question (Open Question 1 below is moot: it was always this unit's
  own last action to clear).
- **Cross-entity contention moves onto the contended entity.** `InvocationCard` tracks its own
  per-initiative claim directly (was the old handler's `_invocationsUsed`); a second unit's `TryBuild`
  attempt on an already-claimed slot just fails at commit time, same as any other legality check.
  `_tilesUsed` — already dead code — either gets deleted, or if a real requirement turns up later,
  becomes an on-demand query over `Board.Units` rather than a maintained shadow copy.
- **What's left of `PlayersDecisionsHandler`**, if anything, is a stateless request router: look up
  the unit/card by id, call its `UnitAction`/`CardAction`, store the returned `factory` into that
  unit's own array on success. It holds no data of its own. This lands closer to `DESIGN.md`'s
  aspirational `DecisionGateway` than today's `PlayersDecisionsHandler` — and goes one step further
  than that sketch too: `DecisionGateway` still centralizes storage in
  `MatchState.QueuedPlayerActions`; here, entities hold their own pending data directly, so there's
  nothing left to centralize even as a queue.

---

## Compound inputs vs. sequential inputs

Not every extra thing the client asks the player for needs its own round trip. Take an Agile move:
right-click cycles a facing preview at the destination tile, left-click confirms tile+facing
together. Position and facing here are components of *one* `HexPose`-valued `InputSlot`, not two
sequential slots — the server can hand back the full domain (legal tiles × legal directions) in a
single response, and everything after that (cycling the preview as many times as the player likes,
in whatever order) is free, local client interaction over data it already has. Only the one final
confirmed value gets submitted and validated.

A slot only needs to be genuinely sequential — its own discovery round trip — when the *next* slot's
domain actually depends on the concrete value chosen for the current one (which invocation → that
invocation's targeting, which is a different domain per invocation). Components that are just
jointly explorable in one UI gesture are one slot, not a chain.

---

## Cards

Playing a card is the same discovery-root shape as `UnitAction`, just rooted at "click/drag a card
in hand" instead of "click a unit" — call it `CardAction`. Unlike `Unit`, a card only supports one
action today (play it), so it doesn't need a `List<CardAction>` wrapper yet; if a card ever needs
multiple distinct actions (play vs. discard-for-effect, say), the same list pattern used on `Unit`
can be reused then.

- **UnitCard**: `CardAction.GetNextInputSlot` returns one Pose slot — position constrained to the
  summoning-area `HexArea`, same compound tile+facing pattern as the Agile case above. If a future
  unit has an enter-effect, the chain continues exactly like `UseInvocationUnitAction` does — delegate
  into the effect's own `IActionInputSource` after placement resolves. No new mechanism.
- **InvocationCard, play step**: zero input slots, submits immediately — the decision is just "move
  this card into the owner's `PreparationZone`." The rich targeting designed above is a *separate*
  `IActionInputSource`, only reached later via `UseInvocationUnitAction` once the card is sitting in
  the preparation zone (see the note at the end of the previous section).
- **EmanationCard**: needs a Pose and/or a unit target, and which one(s) varies per specific
  emanation. Don't model that as a subclass per combination (`PoseEmanationCard`,
  `UnitTargetEmanationCard`, `BothEmanationCard`, ...) — same combinatorial trap as passives (see
  below). Drive it from a declarative list of required inputs on the template, interpreted by one
  `EmanationCard` class, same "data over subclass explosion" principle.

### Cost is checked at commit, not at discovery

`CardAction.IsAvailable` (whether a card even shows as interactable in hand) does not look at mana
cost — every card stays draggable and fully walkable through its input chain regardless of current
affordability. Cost is checked once, as the last step inside `TryBuildDecision`, alongside whatever
other legality re-checks already happen there. No new mechanism — just where in the existing
discover/commit split it lives, chosen specifically so the UI never has to grey out or block a card
the player just wants to explore.

### Client presentation is entirely out of scope for the server

`InputSlot`/`PartialSelection` describe *what* is needed and *what values are legal* — never *how*
the player supplies it. Drag-from-hand vs. click-then-click, right-click-cycling a rotation preview,
dropping an invocation in the middle of the screen instead of onto a tile — all of it is front-end
interaction design layered on the same "fetch domain once → explore it freely client-side → submit
one final value" flow. None of it needs the server to know or care.

---

## Passives: composition, not new `UnitAction` subclasses

Ephemeral, Agile, jump-2 range, random-direction movement, etc. are not new actions — they change
how an existing action behaves or how actions combine. Three buckets, matching where variation
already lives in the codebase:

1. **Changes to a single action's mechanics** (jump-2 range, random-direction move) → a strategy
   object *inside* the action, not a subclass of it. E.g. `MoveUnitAction` holds an `IMovementRule`
   (`AdjacentTileRule` default, `JumpTwoRule`, `RandomDirectionRule` — the latter returns zero input
   slots, there's nothing to ask the player). Same `MoveUnitAction` class throughout.
2. **Changes to how actions combine across initiatives** (Agile: rotate-or-invocation *and* move in
   the same initiative; Ephemeral: all movement collapses into one initiative) → not an input-schema
   concern at all — enforced at `Unit`'s own initiative-assignment bookkeeping (see below), not
   inside `UnitAction`.
3. **Plain numeric/bool stat changes** (speed, `CanExhaust`) → already covered by the existing
   `Stat<T>`/`Modifier<T>` system, nothing new needed.

This keeps the number of `UnitAction` subclasses bounded (`Move`, `Rotate`, `UseAbility`,
`UseInvocation`) regardless of how many passives get added later.

---

## Initiative slot assignment is server-authoritative, not client input

`initiative` is never a field the client sets. `MoveUnitDecision`'s constructor currently takes an
explicit `int initiative` — that needs to change so `TryBuild` pulls it from the unit itself instead:

```csharp
int? initiative = unit.NextOpenInitiative(); // first null slot, scanning from Speed down to 1
```

`Unit.NextOpenInitiative()` scans its own `_pendingActions` array top-down
(`initiative = min(unit.Speed, MovementInitiativeCount) .. 1`) for the first empty slot. `null` means
the unit has no open slots left (already fully committed, or out of eligible initiatives) —
`UnitAction.IsAvailable` folds this in as a gate.

Because slots always fill contiguously from the top, there is no field anywhere the client could
set to jump straight to initiative 1 before initiative 2 exists — the "can't act out of order"
constraint is structural, not a UI convention the client has to be trusted to respect.

### Wait

Wait (`WaitUnitAction` → an `IntentFactory` that just returns `null`) exists so a player can
**deliberately reserve and skip an earlier slot** in order to have the unit act later — e.g. a
speed-3 unit that should only really act at initiative 1, after seeing what faster commitments
already resolved:

```
Wait (confirm, no inputs)  → fills initiative 3
Wait (confirm, no inputs)  → fills initiative 2
Move → choose tile         → fills initiative 1
```

It's a real (if trivial, empty-payload) request rather than a purely client-local cursor advance,
specifically so the server stays the single source of truth for "what's the next slot" — safe across
refresh/reconnect/multiple tabs, and undoable for free via `unit.ClearPendingAction(initiative)`. The
alternative (client tracks skip-count locally, only sends it embedded in the final real action) was
considered and rejected: it works until a refresh or a second client desyncs the count from what the
server actually has queued.

**Caveat:** a player does **not** have to explicitly Wait through every remaining slot up to
`unit.Speed`. If a unit is left with slots un-queued when the player moves on or hits ready, those
trailing slots simply stay `null` and default to doing nothing at resolution — this already falls
out of the existing `Drain()` behavior (`GetMovementPhaseDecisions` only collects non-null entries)
and needs no extra code. Explicit `WaitUnitAction` submissions are only needed to skip a slot that
comes *before* a later slot the player actually wants to use — never to pad out the tail end of a
unit's turn.

---

## Open questions

1. ~~**Undo scope.**~~ Moot — undo is now `unit.ClearPendingAction(initiative)`, a per-unit
   operation on the unit's own array. There's no shared stack left to scope.
2. **`UseAbility`'s `IActionInputSource`.** Sketched above as symmetric with `UseInvocation`, but no
   unit template defines a built-in active ability yet — confirm the shape once one exists.
2a. **Server-initiated decisions.** Today every decision-gathering flow starts from a client request
   ("what can I do with this unit/card") — there are no global decisions yet, only active card plays
   and unit actions. A future passive like "at the start of turn, choose..." has no click to hang off
   of, so the server would need to *push* "you have a pending decision" instead of only answering
   requests. `IActionInputSource` itself doesn't need to change for this — it's already free-standing,
   not owned by `UnitAction`/`CardAction`, so a triggered effect can implement it the same way. What's
   missing is the notification/transport side, and it's not worth designing until an actual passive
   needs it.
3. ~~**Known bug to fix while this lands:** `MoveUnitDecision.Do()` computes `check` from
   `CanPlanUnitAtionRequested` and never uses it to gate the return value.~~ Moot — `Do()`, and
   `MoveUnitDecision` itself, are being removed outright (see "No `PlayerDecision`, no central
   handler" above), so the dangling `check` goes with them rather than needing a standalone fix.
4. **Agile/Ephemeral exact bookkeeping.** Two separate sub-problems, both still open:

   4a. **Slot sharing.** Bucket 2 above says this belongs in `Unit`'s own slot-assignment logic, but
   the precise rule (does Agile let a second action reuse the *same* initiative slot, or grant an
   extra one?) isn't pinned down yet — needs a decision before `NextOpenInitiative` can account for
   it.

   4b. **Resolution ordering within the shared slot.** When Agile pairs rotation with an action
   that depends on facing (e.g. a directional invocation), can the ability observe the new facing
   within the same moment?

   `MomentResolver.Drain()` is a fixed point, not a single pass — `EffectsTracker`, `IntentsResolver`,
   `ZoneChangeResolver`, and `MovementResolver` each only "wake up" on their own stage, at least one
   loop iteration after something is queued for them. Two intents realized in the *same*
   `RealizeIntents()` call are both still working off pre-moment state regardless of what order they
   run in — neither's queued effect/zone-change/movement has had its own stage run yet. **This is by
   design, not a bug**, and no amount of intent ordering fixes it: it's the mechanism that keeps
   same-moment intents from seeing each other's not-yet-real results at all.

   That also rules out a transient Effect that fakes `unit.Pose` for facing-dependent readers:
   `EffectsTracker.RealizeEffects()` runs *before* `IntentsResolver.RealizeIntents()` each iteration,
   so an effect queued by the rotate intent isn't `Enable()`d until the *next* iteration's
   `RealizeEffects()` — one beat too late for a sibling ability intent in the same pass. Same failure
   as reading `unit.Pose` directly, just via a different subsystem.

   For the ability to see the new facing in the same moment, the rotation has to become real without
   going through any queue — nothing deferred to its own later stage (effects, zone changes,
   movement) can be visible same-pass, no matter how intents are ordered.

   **Resolved: pre-bake at build time.** `UnitAction.TryBuild` runs at commit time, and its captured
   `IntentFactory` runs when the gathering event fires — both *before* `Drain()`'s loop starts. Pure
   rotation has no positional offset — unlike a real move, which can be blocked by collision and
   isn't settled until `MovementResolver` runs, nothing in the moment can contest a rotation, so its
   outcome is fully known from build-time data alone. So a paired rotate+ability action's factory:
   - queues the real rotate/move intent through the normal deferred pipeline, unchanged — no
     exception to "state only mutates through its own stage";
   - also constructs the ability/invocation's intent with its target/effect area already computed
     against the post-rotation pose, baked in as a plain value (e.g. a `FixedPoseEvaluator`-style
     evaluator) instead of the ability reading `unit.Pose` dynamically at `Perform()` time.

   Both intents still land in the same `RealizeIntents()` pass, but order between them no longer
   matters — the ability's intent was never waiting on the rotation intent's result to begin with.

   This requires the ability's `TryBuild` to have the rotation delta in hand when it runs, so Agile's
   rotate+ability pairing wants to be one compound action (or at least built together, sharing data)
   — not two independent actions that happen to land in the same slot. That's a real input toward 4a:
   it nudges toward one slot, one compound action, rather than two.

   Caveat, not a blocker: exact only as long as nothing else in the same moment can intercept or
   alter the rotation before its own intent resolves — true today (nothing can currently block a
   queued rotation). If a future effect ever could (e.g. a "rooted" keyword), the pre-bake step needs
   to run through whatever eligibility check the real rotation intent applies, or the two can diverge.

   Rejected alternatives:
   - **Immediate rotation** (synchronous `unit.Pose.Direction` mutation inside `Perform()`, bypassing
     the deferred pipeline) — works, but carves a special-case exception into the one system
     (`MovementResolver`'s planned-move ledger) that's supposed to uniformly own pose mutation.
   - **Sequenced sub-moments** (stage the pairing across two passes) — avoids any exception, but means
     "same initiative" stops meaning "same moment" for compound actions, and needs its own staging
     bookkeeping that pre-baking doesn't.
