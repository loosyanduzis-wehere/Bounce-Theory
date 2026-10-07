# Bounce Theory — Implementation Status

**Project status:** Active prototype  
**Current branch:** `milestone/dribble-state`  
**Current task:** `BT-DS-01 — Complete Prototype Dribble-State Milestone`  
**Task status:** `AWAITING PLAYTEST` — Codex executable verification passed; user Play Mode verification is next
**Design source of truth:** `Docs/Game Design.md`  
**Project instructions:** `AGENTS.md`  
**Last updated:** 2026-10-06

> This file is the fast current handoff. It contains current implementation state, the active task, and the next verification gate.
> Design truth belongs in `Game Design.md`; future work belongs in `ROADMAP.md`; completed history belongs in `COMPLETED_TASKS.md`; architectural rationale belongs in `DECISIONS.md`; unresolved choices belong in `OPEN_QUESTIONS.md`; technical discoveries belong in `IMPLEMENTATION_NOTES.md`.

---

## 1. Current Working Systems

### Prototype foundation

- Stationary behind-the-player basketball prototype scene.
- Offensive player, defender placeholder, basketball, hoop, court, lighting, and camera foundation.
- No conventional free-locomotion foundation in the active Bounce Theory prototype.

### Rhythm / contact architecture

- Shared DSP-time rhythm clock and global pulse.
- Core pipeline:
  `keypress → DSP timestamp → rhythm judgment → action decision → visual execution`.
- Gameplay input is not gated by animation completion.
- Floor contact targets remain aligned to the shared rhythm grid.
- Prototype rhythmic interval vocabulary includes `0.5`, `0.75`, `1.0`, `1.5`, and `2.0` beats.
- Ball motion phases:
  - `Controlled`
  - `Descending`
  - `FloorContact`
  - `Returning`
- Pending input can be accepted before prior visual motion finishes and can execute when physically feasible.

### Core dribble vocabulary

Implemented and previously accepted:

- Pound
- Crossover
- Hesitation
- Behind-the-back

Hand ownership and transfer behavior remain part of the shared rhythm/contact path.

### Stance milestone

**Status:** PROTOTYPE COMPLETE

- Low / Medium / High.
- Medium default.
- Plain Space flow: `Medium → Low → Medium → High → Medium...`
- Space + Crossover → Low.
- Space + Pound → Medium.
- Space + Hesitation → High.
- Space + Behind-the-back remains unassigned.
- Pound, crossover, hesitation, and behind-the-back capture stance at action acceptance.
- Final stance tuning/animation remains provisional.

---

## 2. Active Task — BT-DS-01

**Title:** Complete Prototype Dribble-State Milestone  
**Status:** AWAITING PLAYTEST

### Goal

Complete the Phase-5 prototype state layer so action decisions can represent:

`hand + ball phase + stance + timing + previous move + follow-up input`

without introducing fixed combos, defender behavior, final violation rules, scoring, or animation-gated input.

### Implemented Through GitHub

#### Possession lifecycle

- `Active` / `Ended`.
- Prototype possession-end reason.
- R-key restart.
- Temporary restart overlay/button.
- Restart resets starting hand, Medium stance, pending actions, pending Space state, rhythm history, and sequence context.
- Waiting by itself does **not** end possession.
- Attempting a continuation after the finite supported rhythm window has expired transitions the prototype possession to `Ended`.

#### Ball-control quality

Timing maps to:

- Perfect / Good → `Secure`
- Early / Late → `Recovering`
- Broken Rhythm → `Exposed`

These states exist for future defender/exposure logic; no steal behavior is implemented yet.

#### Resolved sequence context

At floor contact the controller preserves:

- resolved action,
- resolved stance,
- original timing judgment,
- resulting hand,
- resolved control quality,
- sequence action count.

#### Follow-up relation

Accepted actions classify as:

- `FirstAction`
- `Repeat`
- `SameHandVariation`
- `Transfer`
- `CounterTransfer`

Queued actions preserve the relation captured at input acceptance.

### Architecture Decisions For This Task

- Use orthogonal state dimensions rather than one giant dribble-state enum.
- Preserve resolved action context separately from newer pending input.
- Keep current legality grounded in possession state, active hand, ball phase, reachability, one pending slot, stance profile, and rhythm judgment.
- Do not invent a canned combo chart merely to make previous-action context matter.
- No-input time alone is not possession failure.

### Relevant Files

- `Assets/BounceTheory/Scripts/PoundDribbleController.cs`
- `Assets/BounceTheory/Scripts/RhythmClock.cs`
- `Assets/BounceTheory/Editor/BounceTheorySceneBuilder.cs`
- `Docs/DECISIONS.md`
- `Docs/OPEN_QUESTIONS.md`
- `Docs/IMPLEMENTATION_NOTES.md`

---

## 3. Verification Gate

### ChatGPT/GitHub

**State:** COMPLETE FOR HANDOFF

The full documented BT-DS-01 implementation is committed on `milestone/dribble-state`.

ChatGPT performed repository/diff review but did **not** claim Unity compilation or executable validation.

### Codex — executable verification complete

**State:** PASSED 2026-10-06

Codex inspected the feature diff and surrounding code, compiled the Unity project, ran the complete BT-DS-01 validator and representative regressions, repaired queued-action authority and expired-target reporting issues, and reran verification successfully.

Do **not** mark BT-DS-01 complete from automated verification alone.

### Verified validator coverage

- Active → Ended → Restart → Active.
- Waiting alone does not end possession.
- Expired continuation attempt ends the prototype possession.
- Ended possession blocks normal gameplay input.
- Restart clears rhythm and sequence context.
- Fresh first action works after restart.
- All five follow-up relation classes.
- Queued relation capture.
- Pending input does not overwrite resolved context.
- Secure / Recovering / Exposed timing mapping.
- Existing stance/dribble/rhythm/contact regressions.

---

## 4. Manual Playtest After Codex

Required before BT-DS-01 can become COMPLETE:

- Normal quick chains still feel responsive.
- Debug state follows the actual sequence being performed.
- Waiting does not end possession by itself.
- An expired continuation attempt produces the restart overlay instead of a dead/stuck controller state.
- R restarts cleanly.
- Restart button restarts cleanly.
- Stance behavior remains intact.

---

## 5. Explicitly Out of Scope

Do not fold these into BT-DS-01:

- fixed/canned combo sequences,
- full move-to-move legality tables,
- final stance-dependent follow-up advantages,
- automatic gather from inactivity,
- travel or double-dribble enforcement,
- loose-ball physics,
- defender reaction/steal/reach/lean/recovery,
- scoring,
- finishes,
- polished menus/UI,
- final animation,
- unrelated refactors.

These belong to later tasks after BT-DS-01 verification and playtest.
