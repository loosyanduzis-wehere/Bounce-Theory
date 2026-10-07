# Bounce Theory — Decisions

This file records important architecture/design decisions that future developers or agents might otherwise accidentally undo.

Routine implementation choices do not need entries here.

---

## Decision 001 — Gameplay State Is Authoritative Over Animation Completion

**Status:** Active

### Decision

Gameplay input is accepted and judged according to rhythm, ball state, game state, hand, stance, and physical legality.

Animation completion is not the authority that decides whether the next input may exist.

Conceptual pipeline:

`keypress -> DSP timestamp -> rhythm judgment -> action decision -> visual execution`

### Why

Bounce Theory depends on musical timing. A fixed animation lock would make visually long motions prevent valid rhythmic follow-ups and would turn animation duration into hidden gameplay timing.

Visual motion may therefore overlap a future accepted input.

---

## Decision 002 — Basketball Actions Share One Global Pulse

**Status:** Active

### Decision

Dribble actions share one underlying musical pulse instead of each action owning an independent BPM.

Actions may occupy different rhythmic intervals on the shared pulse, including prototype values such as:

- `0.5`
- `0.75`
- `1.0`
- `1.5`
- `2.0` beats

Exact move-to-interval mappings remain tunable.

### Why

The game should feel like the player is composing basketball percussion inside one musical framework, not switching between unrelated timing systems for each move.

---

## Decision 003 — Bounce Theory Is Not a Conventional Free-Locomotion Basketball Prototype

**Status:** Active

### Decision

The restarted Bounce Theory project is rhythm-first and does not use conventional free player locomotion as its core control foundation.

The older conventional-movement prototype should remain a separate reference/foundation for another basketball project.

### Why

Bounce Theory's core interaction is timed hand/dribble language, defender manipulation, and rhythm. Conventional navigation would move the prototype toward a different game before the core rhythm interaction is proven.

---

## Decision 004 — Stance Uses Fast Three-State Controls Instead of a Menu

**Status:** Active

### Decision

Bounce Theory uses three working stance states: **Low**, **Medium**, and **High**, with Medium as the neutral/home stance.

A plain **Space** press follows this quick stance flow:

`Medium → Low → Medium → High → Medium → Low → ...`

Space also acts as a stance modifier while the associated dribble action still occurs:

- **Space + Crossover → Low**
- **Space + Pound → Medium**
- **Space + Hesitation → High**

`Space + Behind-the-back` has no dedicated stance destination yet and should remain unassigned until a gameplay purpose is designed.

### Why

Bounce Theory's dribble language moves too quickly for a stance-selection menu to feel natural. The stance controls should remain playable inside the rhythm. Returning through Medium keeps the plain-Space flow readable, while modified dribble inputs give direct stance access without creating a separate menu or pausing basketball action.


---

## Decision 005 — Prototype Possession Lifecycle Is Separate From Final Failure Rules

**Status:** Active

### Decision

Bounce Theory will introduce a minimal prototype possession lifecycle before final menus or complete basketball failure rules exist:

`Active → Ended → Restart → Active`

A restart path is development infrastructure and should remain available independently of the final rules that eventually end a possession.

For the current prototype, when the player attempts another dribble after the existing rhythmic continuation vocabulary no longer offers a reachable future contact target, the possession may transition to **Ended** instead of leaving the controller in a permanently unreachable/dead continuation state.

This is a prototype resolution rule, not a locked final gather/mishandle/travel rule.

### Why

The current rhythm planner intentionally supports a finite interval vocabulary. After the latest supported continuation target has passed, repeatedly asking the same planner for another continuation can produce an unreachable target and leave playtesting in a state where the player cannot meaningfully continue.

Separating the possession lifecycle from the final basketball-specific reason allows the prototype to recover cleanly now while leaving future systems free to reinterpret the same boundary as a gather, mishandle, steal, violation, finish, or another outcome.

A lack of input by itself is **not** automatically possession failure because hesitation and intentional pauses remain valid design concepts.


---

## Decision 006 — Dribble State Uses Orthogonal Context Dimensions

**Status:** Active

### Decision

Do not collapse the entire dribble state machine into one giant state enum.

Represent the prototype through separate, composable dimensions:

- possession lifecycle,
- ball motion phase,
- hand ownership,
- stance,
- timing judgment / ball-control quality,
- previous resolved action context,
- accepted follow-up relation,
- pending action context.

Follow-up relations describe sequence context without creating a fixed combo system.

### Why

Bounce Theory actions are rhythmically independent but physically contextual. A single monolithic enum would create a combinatorial state explosion and would encourage animation-shaped state definitions.

Keeping the dimensions separate lets the action-decision layer ask the relevant questions—hand, phase, stance, timing, previous move, possession, and follow-up—while preserving the existing rhythm-first input architecture.


---

## Decision 007 — Defender Reactions Consume Gameplay State, Not Animation Completion

**Status:** Active

### Decision

Prototype defender reactions should subscribe to authoritative dribble/gameplay state and events rather than infer legality or timing from visual animation completion.

The first defender feature reads the accepted dribble action, source hand, transfer resolution, and offensive control quality to select readable lean/recovery behavior.

### Why

Bounce Theory is rhythm-first. If defender state depended on animation completion, defender timing would quietly reintroduce visual-duration authority into the gameplay loop.

Using the existing dribble-state layer keeps defender behavior aligned with the same DSP/rhythm/action-decision architecture as offense.


---

## Decision 008 — Prototype Steal Pressure Is Deterministic

**Status:** Active for prototype

### Decision

The first defender steal-pressure loop uses deterministic gameplay-state rules instead of random steal percentages.

- Secure ball control is Protected.
- Recovering ball control is Contested.
- Exposed ball control is Vulnerable.
- Vulnerable pressure resolves as a prototype steal.
- Contested pressure can be escaped by a valid follow-up already accepted before floor contact; the failed reach creates Overcommitted.
- A clean non-pound counter can then create Beaten.

### Why

The prototype needs to prove that rhythm quality and contextual follow-ups create readable basketball consequences before adding AI probability or difficulty tuning.

Deterministic rules make cause/effect legible, testable, and compatible with the rhythm-authoritative input architecture. Final steal probability/intelligence may change later.


---

## Decision 009 — Finish Timing Uses Shared Rhythm With Captured Defender Advantage

**Status:** Active for prototype

### Decision

The first finish system uses a two-stage timing event on the existing shared rhythm clock:

1. commit a finish,
2. release the same finish input on its scheduled rhythm target.

The finish target snaps to the shared/global pulse rather than using an unrelated timer.

Defender state is captured when the finish is committed and determines the green-window tier for that attempt:

- Beaten → Wide,
- Recovering / Overcommitted → Medium,
- Centered / Leaning / Reaching → Tight.

The captured window remains authoritative for that attempt even if the defender's placeholder visual state changes before release.

### Why

The Source of Truth says the hard basketball work happens before the finish: the player creates advantage, then the finish timing cashes it out.

Capturing defender advantage at finish commitment preserves that cause/effect relationship. Keeping the target on the shared rhythm clock prevents finishes from becoming a separate timing minigame disconnected from Bounce Theory's musical foundation.
