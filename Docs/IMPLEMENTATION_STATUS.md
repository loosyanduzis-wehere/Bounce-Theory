# Bounce Theory — Implementation Status

**Project status:** Active prototype  
**Current branch:** `milestone/defender`  
**Current task:** `BT-DF-01 — Defender Lean + Recovery Foundation`  
**Task status:** `AWAITING PLAYTEST`
**Design source of truth:** `Docs/Game Design.md`  
**Project instructions:** `AGENTS.md`  
**Last updated:** 2026-10-07

> This file is the fast current handoff. Design truth belongs in `Game Design.md`; future work belongs in `ROADMAP.md`; completed history belongs in `COMPLETED_TASKS.md`; architectural rationale belongs in `DECISIONS.md`; unresolved choices belong in `OPEN_QUESTIONS.md`; technical discoveries belong in `IMPLEMENTATION_NOTES.md`.

---

## Current Accepted Foundation

- Core rhythm/contact foundation: COMPLETE.
- Core dribble vocabulary: COMPLETE.
- Stance milestone: PROTOTYPE COMPLETE.
- Dribble-state milestone: PROTOTYPE COMPLETE.
- BT-DS-01 user Play Mode verification accepted on **2026-10-06**.

The defender feature should consume the existing dribble-state context rather than creating a parallel timing or action system.

---

## Active Task — BT-DF-01

**Title:** Defender Lean + Recovery Foundation  
**Status:** AWAITING PLAYTEST

### Goal

Make the placeholder defender visibly and deterministically react to the dribble state so the prototype begins functioning as a basketball duel rather than a stationary dribble sandbox.

This first defender feature implements readable:

- `Centered`
- `LeaningLeft`
- `LeaningRight`
- `Recovering`

states only.

### Unknowns Pass — Resolved For This Task

- Defender state changes must be triggered by gameplay/dribble events, not by waiting for visual animation completion.
- Defender root position remains stationary for this feature; readable reaction comes from body/presentation transforms.
- Dribble source-hand presentation can induce a defender lean toward that side.
- A hand-transfer action that resolves opposite the committed lean creates `Recovering`.
- Offensive timing quality affects defender recovery:
  - Secure offense → longest defender recovery,
  - Recovering offense → medium recovery,
  - Exposed offense → shortest recovery.
- This encodes the Source of Truth principle that poor offensive timing gives the defender better recovery opportunity.
- A hesitation accepted while the defender is already Recovering extends the recovery window rather than replacing it with a new lean. This is the first prototype expression of “hesitation punishes recovery.”
- All numeric reaction distances, tilts, and durations are prototype tuning.
- Exact AI intelligence, reach/steal logic, overcommit, beaten state, and final stance/exposure interaction remain unresolved.

### Prototype State Flow

```text
Centered
  ↓ dribble starts / presented side
LeanLeft or LeanRight
  ↓ transfer resolves away from committed side
Recovering
  ↓ recovery timer
Centered

Recovering
  ↓ hesitation
Recovering for longer
```

### Reaction Rules

#### Lean

When an action begins and the defender is not already Recovering:

- source hand Left → LeaningLeft,
- source hand Right → LeaningRight.

The lean is a readable placeholder reaction to the side being presented.

#### Transfer resolution

When crossover or behind-the-back reaches floor contact and changes ownership:

- if the defender was leaning toward the source side, enter Recovering.

#### Same-hand resolution

Pound/hesitation do not create a transfer-recovery state by themselves.

#### Timing → recovery duration

Prototype mapping:

- Secure → long recovery,
- Recovering → medium recovery,
- Exposed → short recovery.

Exact values are tunable and should be serialized.

#### Hesitation during recovery

If hesitation begins while the defender is Recovering:

- keep Recovering,
- extend remaining recovery by a tunable amount,
- do not introduce Overcommitted yet.

### Scope

- Add a prototype defender controller/state machine.
- Subscribe it to the existing dribble controller events/state.
- Add readable body lean/recovery visuals without moving the defender root.
- Reset defender to Centered on possession restart.
- Expose current defender state and recovery timing for validation/debug.
- Add SceneBuilder upgrade/setup for the existing prototype scene.
- Add targeted deterministic validator.
- Preserve all accepted dribble, stance, possession, rhythm, and restart behavior.

### Success Criteria

- [ ] Defender starts Centered.
- [ ] Left-source action produces LeaningLeft.
- [ ] Right-source action produces LeaningRight.
- [ ] Defender root position remains unchanged.
- [ ] Crossover/BTB resolving away from a committed lean enters Recovering.
- [ ] Secure offensive control produces longer recovery than Recovering control.
- [ ] Recovering control produces longer recovery than Exposed control.
- [ ] Hesitation during Recovering extends recovery.
- [ ] Recovery returns to Centered.
- [ ] Possession restart immediately resets defender to Centered.
- [ ] Existing dribble-state, stance, timing, hand ownership, and restart behavior remain intact.
- [ ] No Rigidbody, CharacterController, navigation, reach, steal, overcommit, beaten, scoring, or finish logic is introduced.

### Out of Scope

Do not implement:

- Reaching,
- steals,
- Overcommitted,
- Beaten,
- defender locomotion/pathfinding,
- shot contest,
- finish windows,
- scoring,
- final defender AI,
- final animation,
- final stance/exposure rules.

### Verification Plan

Automated / executable:

- [x] Unity editor project compiles.
- [x] Scene upgrade attaches/configures the defender controller.
- [x] Targeted BT-DF-01 validator covers Centered/LeanLeft/LeanRight/Recovering.
- [x] Validator proves root position remains fixed.
- [x] Validator proves ordered recovery durations by offensive control quality.
- [x] Validator proves hesitation extends an active recovery.
- [x] Validator proves restart returns Centered.
- [x] Representative BT-DS-01 / stance / rhythm/contact regressions pass.
- [x] Final diff contains no unrelated changes.

User/manual after Codex:

- [ ] Defender lean is obvious enough to read.
- [ ] Transfer → recovery reads coherently.
- [ ] Hesitation during recovery visibly feels like it freezes/punishes recovery.
- [ ] Existing dribbling still feels responsive.
- [ ] Restart resets both ball state and defender.

Do not mark COMPLETE until user Play Mode verification is accepted.


---

## Upcoming Defender Features — Review Only

These are the intended next coherent chunks after BT-DF-01. They are **not** part of the current implementation/review scope unless explicitly promoted later.

### BT-DF-02 — Defender Reach + Steal Opportunity

Purpose:

- add a readable `Reaching` state,
- use existing ball-control quality as the first steal-opportunity input,
- `Exposed` → strongest steal opportunity,
- `Recovering` → smaller steal opportunity,
- `Secure` → largely protected,
- keep Overcommitted / Beaten / scoring / finishes out of scope.

### BT-DF-03 — Defender Overcommit + Beaten State

Purpose:

- add `Overcommitted` after a failed/bad reach or strong offensive counter,
- transition to `Beaten` when the offense successfully exploits that mistake,
- preserve rhythm/game-state authority rather than animation completion,
- keep finish execution and scoring in later milestones.

Intended progression:

`Lean → Recover → Reach → Overcommit → Beaten`

Codex may flag architectural conflicts or hidden coupling these planned chunks would create while reviewing BT-DF-01, but should **not implement them during BT-DF-01 review**.

---

## GitHub Implementation Handoff

**ChatGPT first implementation:** COMPLETE FOR HANDOFF  
**Executable verification:** PASSED 2026-10-07
**Task status:** AWAITING PLAYTEST

Implemented:

- `PrototypeDefenderController.cs` + Unity metadata,
- Centered / LeaningLeft / LeaningRight / Recovering state machine,
- source-hand lean reactions,
- transfer-at-floor-contact recovery,
- Secure / Recovering / Exposed ordered recovery timing,
- hesitation recovery extension,
- possession-restart reset event,
- defender visual pivot scene upgrade,
- targeted BT-DF-01 validator.

### Codex verification result

Codex inspected the actual diff, ran the scene upgrade, compiled the Unity editor project, ran the targeted BT-DF-01 validator and complete BT-DS-01 regression validator, repaired hesitation recovery so it responds at accepted command time instead of delayed visual execution, and reran verification successfully.

The next action is user Play Mode verification.

Do not mark COMPLETE without user acceptance.
