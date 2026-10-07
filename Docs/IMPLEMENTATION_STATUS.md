# Bounce Theory — Implementation Status

**Project status:** Active prototype  
**Current branch:** `milestone/finishes`  
**Current task:** `BT-FN-01 — Core Finish Timing System`  
**Task status:** `IN PROGRESS`  
**Design source of truth:** `Docs/Game Design.md`  
**Project instructions:** `AGENTS.md`  
**Last updated:** 2026-10-07

> This branch is being developed in parallel with Codex verification of `milestone/defender-interaction`. It was created from the then-current defender interaction implementation. Before final Codex handoff, absorb any material parent-branch fixes rather than overwriting them.

---

## Accepted / Inherited Foundation

- Core rhythm/contact foundation: COMPLETE.
- Core dribble vocabulary: COMPLETE.
- Stance milestone: PROTOTYPE COMPLETE.
- Dribble-state milestone: PROTOTYPE COMPLETE.
- BT-DF-01 lean/recovery: verified and user-accepted.
- BT-DF-02/03 defender interaction implementation is inherited from `milestone/defender-interaction`; its own executable verification may still advance independently.

Finishes must consume the existing rhythm clock, possession lifecycle, and defender state rather than create parallel timing/contest systems.

---

## Active Task — BT-FN-01

**Title:** Core Finish Timing System  
**Status:** IN PROGRESS

### Goal

Add the first complete finish loop:

`create advantage → commit finish → release on shared rhythm target → made/missed possession outcome`

with Shot, Stepback, and Drive all using the same reusable timing architecture.

### Controls

- `Q` — Shot
- `E` — Stepback
- `F` — Drive
- first press commits the selected finish,
- second press of the same key releases/resolves it,
- `R` restarts after a made/missed finish through the existing possession restart path.

### Unknowns Pass — Resolved For This Prototype

- Finishes use a separate `PrototypeFinishController`; do not merge finish state into the dribble enum.
- Finish timing remains anchored to the shared `RhythmClock`.
- A committed finish targets the first whole beat that leaves enough prototype lead time for that finish type.
- Current prototype minimum lead times differ by type but are serialized/tunable.
- Defender state is snapshotted when the finish is committed. The resulting green window does not shrink later if the defender visually recovers during the finish.
- `Beaten` → Wide green window.
- `Recovering` / `Overcommitted` → Medium green window.
- `Centered`, `LeaningLeft`, `LeaningRight`, and `Reaching` → Tight green window.
- The current defender root is stationary, so defender **state** is the first contest proxy. Final spatial contest math remains deferred.
- A finish may start only when the ball is `Controlled`, with no queued dribble or pending plain-Space stance input.
- Once a finish is committed, ordinary dribble input is suppressed until the finish resolves or possession restarts.
- Releasing inside the captured green half-window → `GreenMade`.
- Releasing before/after it → `EarlyMiss` / `LateMiss`.
- Failing to release before the late edge auto-resolves `LateMiss`.
- Made/missed finishes end the current possession through `FinishMade` / `FinishMissed`.
- No score calculation is introduced yet.
- No ball-flight, rim physics, locomotion, shot contest animation, or polished finish animation is introduced yet.

### Prototype Flow

```text
Controlled possession
  ↓ Q / E / F
Finish Timing
  ↓ capture defender state
  ↓ select Tight / Medium / Wide green window
  ↓ target shared whole beat
  ↓ second same-key press

inside window  → GreenMade  → possession Ended
too early      → EarlyMiss  → possession Ended
too late       → LateMiss   → possession Ended
no release     → LateMiss   → possession Ended

R → restart → Idle
```

### Scope

- Add reusable finish types: Shot / Stepback / Drive.
- Add finish phases: Idle / Timing / Resolved.
- Add Tight / Medium / Wide defender-dependent green windows.
- Add GreenMade / EarlyMiss / LateMiss.
- Add shared-rhythm finish target scheduling.
- Add Q/E/F input handling.
- Add narrow dribble-input suppression while finish timing is active.
- Add `FinishMade` / `FinishMissed` possession-end reasons.
- Add temporary timing cue/debug display.
- Add scene upgrade/setup.
- Add one deterministic validator covering the complete first finish loop.
- Preserve defender, dribble, stance, rhythm, and restart architecture.

### Success Criteria

- [ ] Q commits Shot.
- [ ] E commits Stepback.
- [ ] F commits Drive.
- [ ] Finishes start only from Controlled with no pending dribble/Space input.
- [ ] Committed finish suppresses ordinary dribble acceptance.
- [ ] Finish target lands on the shared rhythm grid.
- [ ] Beaten maps to Wide.
- [ ] Recovering / Overcommitted map to Medium.
- [ ] Centered / Leaning / Reaching map to Tight.
- [ ] Wide > Medium > Tight numerically.
- [ ] Window tier is captured at commit time.
- [ ] On-target release produces GreenMade / FinishMade.
- [ ] Early release produces EarlyMiss / FinishMissed.
- [ ] Late release produces LateMiss / FinishMissed.
- [ ] No release auto-resolves LateMiss.
- [ ] Restart returns finish state to Idle and restores dribble input.
- [ ] Existing offense, defender, and camera roots remain stationary.
- [ ] No Rigidbody / CharacterController / free locomotion is introduced.
- [ ] Defender and dribble regressions remain intact.

### Out of Scope

Do not add:

- possession scoring,
- shot percentage/random make rolls,
- spatial rim/ball simulation,
- defender locomotion or contest animation,
- final finish animations,
- dunk variants,
- spectacular/unlocked finish progression,
- stance-specific finish math,
- difficulty-specific finish windows,
- final foul/violation/gather semantics,
- mid-bounce finish queueing.

### Verification Plan

Automated / executable after parent defender branch synchronization:

- [ ] synchronize/absorb latest `milestone/defender-interaction` material fixes,
- [ ] run `Bounce Theory/Upgrade Prototype Scene To BT-FN-01 Core Finishes`,
- [ ] compile Unity editor project,
- [ ] run `Bounce Theory/Validate BT-FN-01 Core Finishes`,
- [ ] run BT-DF-02/03 validator,
- [ ] run BT-DF-01 validator,
- [ ] run BT-DS-01 validator,
- [ ] run representative stance/rhythm/contact regressions,
- [ ] inspect final diff/status for unrelated changes.

Manual after Codex:

- [ ] Q/E/F control loop is understandable.
- [ ] timing cue is readable enough to test.
- [ ] Tight / Medium / Wide windows feel meaningfully different.
- [ ] creating a Beaten defender meaningfully improves the finish opportunity.
- [ ] finish commit does not make dribbling feel broken or sticky.
- [ ] made/missed → restart flow is coherent.

Do not mark COMPLETE until user Play Mode verification is accepted.

---

## GitHub Implementation Handoff

**ChatGPT first implementation:** COMPLETE FOR STATIC HANDOFF  
**Unity compile / executable validation:** NOT YET CLAIMED  
**Task status remains:** IN PROGRESS

Implemented:

- `PrototypeFinishController.cs` + Unity metadata,
- Shot / Stepback / Drive finish types,
- Q / E / F two-stage commit/release controls,
- shared-whole-beat finish target scheduling,
- defender-state snapshot → Tight / Medium / Wide green window,
- GreenMade / EarlyMiss / LateMiss resolution,
- automatic late miss,
- finish gameplay-input suppression,
- `FinishMade` / `FinishMissed` possession outcomes,
- timing cue/debug HUD,
- scene upgrade,
- full BT-FN-01 validator.

The repo remains the context package. Codex should inspect actual code/diff rather than trust this summary.


### Next Codex pass

After the current defender-interaction review is settled, Codex should work on `milestone/finishes` as one coherent feature review.

Codex should:

1. synchronize `milestone/finishes`,
2. compare it against the latest accepted/verified `milestone/defender-interaction`,
3. absorb any material parent-branch fixes without discarding finish work,
4. run `Bounce Theory/Upgrade Prototype Scene To BT-FN-01 Core Finishes`,
5. compile the Unity editor project,
6. run `Bounce Theory/Validate BT-FN-01 Core Finishes`,
7. rerun BT-DF-02/03, BT-DF-01, BT-DS-01, and representative stance/rhythm/contact regressions,
8. fix clear material in-scope issues,
9. rerun verification and inspect final status/diff.

If executable verification succeeds, change BT-FN-01 to `AWAITING PLAYTEST`, commit, push, and stop.

Do not mark COMPLETE without user Play Mode acceptance.
