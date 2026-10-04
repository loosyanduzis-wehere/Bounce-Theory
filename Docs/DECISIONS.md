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
