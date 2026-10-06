# Bounce Theory — Implementation Status

**Status:** Active prototype  
**Completed milestone:** Stance — prototype complete  
**Next planned milestone:** Dribble State / Possession Foundation  
**Current task:** `BT-DS-01 — Possession Lifecycle + Contextual Follow-Up Foundation`  
**Git repository:** Configured  
**Remote:** `https://github.com/loosyanduzis-wehere/Bounce-Theory.git`  
**Baseline commit:** `078def9` — prototype through legacy Chunk 4.75  
**Current branch at handoff:** `milestone/dribble-state`  
**Project path:** `C:\Users\jerry\Bounce Theory v2`  
**Design source of truth:** `Docs/Game Design.md`  
**Project instructions:** `AGENTS.md`  
**Last updated:** 2026-10-06

> This file is the current implementation handoff for Codex.
> It describes what exists now and the one task Codex is allowed to work on next.
> It is **not** the design source of truth and **not** the completed-history log.

---

## 1. Current State

### Prototype Foundation

- Minimal behind-the-player basketball prototype scene exists.
- Offensive player, defender, basketball, hoop, court, lighting, and camera foundation exist.
- The prototype remains stationary; conventional free locomotion is not part of the current Bounce Theory architecture.
- Scene/editor construction tooling exists.

### Shared Rhythm System

- Shared rhythm clock exists.
- Gameplay input receives DSP timestamps.
- Rhythm judgment is based on musical time rather than waiting for visual animation completion.
- Prototype rhythmic intervals can be evaluated against one shared global pulse.
- Small actual contact errors do not shift subsequent grid-aligned rhythmic targets.

### Pound Dribble / Ball State

- Pound dribble is implemented for the active hand.
- Ball motion uses meaningful phases including:
  - `Controlled`
  - `Descending`
  - `FloorContact`
  - `Returning`
- Gameplay authority advances at floor contact rather than waiting for the visual return to finish.
- A pending action may execute during the earliest physically feasible part of `Returning`.

### Input / Judgment Pipeline

Current architecture:

`keypress -> DSP timestamp -> rhythm judgment -> action decision -> visual execution`

Important:

- Valid gameplay input is not rejected solely because a prior bounce animation is still active.
- Keypress time is command time, not impact time.
- Visual motion may overlap future valid rhythmic input.

### Rhythm-Driven Contact Timing

`BT-BC-05 — Rhythm-Driven Floor Contact` is COMPLETE.

Implemented behavior includes:

- Each accepted input creates a `ContactTimingPlan`.
- The plan tracks keypress DSP time, intended interval, target beat, target contact DSP time, projected contact time, and timing judgment.
- Contact targets remain anchored to the global grid.
- The first pound uses a one-beat global-grid target.
- Bounce trajectories can reshape toward the accepted contact target.
- Faster accepted intervals can use compressed motion without waiting for the full prior visual cycle.
- Floor-impact audio fires from measured `FloorContact`.
- `Normal`, `Compressed`, and `Unreachable` motion outcomes exist.
- Physically infeasible targets are explicitly rejected instead of silently moved to another beat.

### Current Prototype Tuning

These are tunable prototype values, not locked design rules:

- BPM: approximately `130`
- Normal hand height: approximately `1.56`
- Floor height: approximately `0.47`
- Reference visual bounce duration: approximately `0.72 s`
- Contact hold: approximately `0.08 s`
- Minimum fast-bounce height: approximately `1.00`
- Maximum descent speed: approximately `8.0 units/second`
- Trajectory compression: approximately `0.65`
- Minimum contact approach: approximately `0.08 s`

---

## 2. Verification State

### BT-BC-05 — Rhythm-Driven Floor Contact

**Status:** COMPLETE  
**Legacy designation:** Chunk 4.75

Automated verification reported:

- explicit target and actual DSP contact timestamps,
- input/contact-time independence,
- impact audio invoked from `FloorContact`,
- normal readable high bounce,
- compressed half-beat motion during the previous return,
- no wait for full visual completion,
- explicit missed-target rejection,
- global-grid stability after small contact errors,
- reference groove and ball-volume hierarchy preserved,
- one basketball controller only,
- stationary player, defender, and camera,
- no `Rigidbody` or `CharacterController`,
- previous prototype regression validators passing,
- no compiler errors or validation exceptions.

Deterministic 240 Hz validation reported approximately:

- normal high-bounce error: `+0.8 ms`
- compressed half-beat error: `+0.9 ms`

User Play Mode verification was accepted on 2026-10-04.

---

## 3. Known Future Systems

These are not implemented merely because they appear in design or roadmap:

- Full crossover behavior
- Complete stance system
- Full dribble branching/state machine
- Defender lean/recovery/reach/overcommit behavior
- Rhythm-based steal opportunities
- Finish timing / green windows
- Possession scoring
- Roguelite progression
- Final animation
- Final audio/percussion system
- Final difficulty timing windows
- Final unresolved-bounce / gather / mishandle rules

Future work must be deliberately promoted into the Current Task before implementation.

---

## 4. High-Value Current Files

- `Assets/BounceTheory/Scripts/RhythmClock.cs`
- `Assets/BounceTheory/Scripts/PoundDribbleController.cs`
- `Assets/BounceTheory/Scripts/ReferenceGroovePlayer.cs`
- `Assets/BounceTheory/Editor/BounceTheorySceneBuilder.cs`
- `Assets/Scenes/BounceTheoryPrototype.unity`

Codex must inspect the actual project before assuming this list is exhaustive.

---

## 5. Current Task — BT-DS-01

**Title:** Possession Lifecycle + Contextual Follow-Up Foundation  
**Status:** IN PROGRESS

### Goal

Create a minimal prototype possession lifecycle and restart path while formalizing the first contextual follow-up state needed for future branching.

The feature should turn the current expired-continuation dead loop into an explicit, recoverable possession end state without prematurely defining final gather, mishandle, steal, travel, or double-dribble rules.

### Unknowns Pass — Resolved For This Task

- The prototype needs an explicit higher-level possession state separate from ball-motion phase.
- Restart behavior should exist now even though final possession-end causes remain open.
- No-input time alone must **not** automatically end a possession because intentional hesitation/silence remains legal design space.
- The current finite rhythm vocabulary tops out at the existing `2.0 beat` continuation target.
- When the player **attempts another dribble** after no reachable target remains in that finite continuation vocabulary, the prototype should end the possession instead of repeatedly returning an unreachable target.
- Previous resolved dribble action should become explicit contextual state for later move branching.
- This task should not invent final move-specific combo legality.

### Desired Prototype Flow

```text
Possession Active
    ↓
dribble / follow-up play
    ↓
attempted continuation after latest reachable rhythm target
    ↓
Possession Ended
    ↓
temporary restart overlay / R
    ↓
clean Active possession
```

### Scope

#### Possession lifecycle

- Add explicit `Active` and `Ended` possession states.
- Record a prototype possession-end reason.
- Provide a public/scoped way for future systems to end a possession without owning restart implementation.
- While Ended, gameplay dribble and stance input must not continue.

#### Restart

- Add **R** as a prototype restart shortcut.
- Add a simple temporary on-screen **Restart** button/overlay when the possession is Ended.
- Restart must restore a clean playable baseline:
  - starting hand,
  - Medium stance,
  - cleared queued/pending action,
  - cleared pending Space modifier state,
  - normal ball/controller state,
  - restarted rhythm clock / cleared ball-event history.
- Restart should not move the player root, defender, or camera.
- This is prototype tooling, not final menu/UI design.

#### Expired continuation handling

- Detect when an attempted dribble continuation no longer has a reachable target inside the currently supported finite rhythmic interval vocabulary.
- Convert that attempted continuation into `PossessionState.Ended` with a clear prototype reason instead of repeatedly leaving the same continuation reference unreachable.
- Do not automatically end merely because time passes with no input.
- Initial input after a clean restart must remain valid even if the player waits before starting.

#### Contextual follow-up state

- Track whether a previous dribble action has resolved.
- Track the most recently resolved dribble action.
- Reset previous-action context on possession restart.
- Expose the contextual state for debug/validation and later branching.
- Preserve existing hand, stance, ball phase, timing judgment, and pending-action semantics.

### Relevant Files / Systems

Likely relevant:

- `Assets/BounceTheory/Scripts/PoundDribbleController.cs`
- `Assets/BounceTheory/Scripts/RhythmClock.cs`
- `Assets/BounceTheory/Editor/BounceTheorySceneBuilder.cs`
- `Docs/DECISIONS.md`
- `Docs/OPEN_QUESTIONS.md`
- `Docs/IMPLEMENTATION_NOTES.md`

### Success Criteria

- [ ] Possession begins Active.
- [ ] Existing valid dribble inputs behave as before while Active.
- [ ] A resolved dribble records its action as previous-action context.
- [ ] Waiting by itself does not end the possession.
- [ ] An attempted continuation after the latest reachable supported rhythm target ends the possession cleanly.
- [ ] The expired continuation does not remain in an endless unreachable retry loop.
- [ ] Ended possession ignores normal dribble/stance input.
- [ ] Pressing R restarts from Active or Ended state.
- [ ] The temporary Restart UI works when Ended.
- [ ] Restart restores starting hand and Medium stance.
- [ ] Restart clears pending dribble and Space-modifier state.
- [ ] Restart resets rhythm-clock ball-event history so a fresh first dribble is valid.
- [ ] Restart clears previous-action context.
- [ ] Player root, defender, and camera remain stationary.
- [ ] Existing stance profiles, dribble ownership, DSP timing, and contact scheduling remain intact for valid in-window play.

### Out of Scope

Do **not** implement during this task:

- final pause/main menu,
- polished UI,
- automatic possession failure from generic inactivity,
- final gather rules,
- travel or double-dribble enforcement,
- mishandle/loose-ball simulation,
- defender steals,
- move-specific legal/illegal combo tables,
- stance-dependent follow-up restrictions,
- scoring,
- finishes,
- unrelated refactors.

### Verification Plan

Automated / executable:

- [ ] Project compiles.
- [ ] Add a targeted validator for possession Active → Ended → Restart → Active.
- [ ] Verify waiting alone does not transition to Ended.
- [ ] Verify a continuation attempt after the maximum supported target ends the possession.
- [ ] Verify restart clears rhythm event history and allows a new first dribble.
- [ ] Verify previous-action context records a resolved move and clears on restart.
- [ ] Verify dribble and stance inputs are blocked while Ended.
- [ ] Run representative stance, pound, crossover, hesitation, behind-the-back, rhythm/contact regressions.
- [ ] Review `git diff`.
- [ ] Confirm unrelated files were not changed.

User/manual:

- [ ] Let a normal dribble return, wait until the continuation window has clearly expired, then attempt another dribble.
- [ ] Confirm a temporary possession-ended/restart overlay appears instead of leaving the prototype unable to continue.
- [ ] Press R and confirm play immediately returns to a clean starting state.
- [ ] Trigger the same flow and click the temporary Restart button.
- [ ] Confirm normal quick follow-ups still work.
- [ ] Confirm stance controls still behave as before.

After ChatGPT implementation is committed/pushed, hand this branch to Codex as a **fresh reviewer/fixer/verifier**. Codex should inspect the actual diff, compile, run the targeted validator/regressions, directly fix clear in-scope material issues, rerun verification, and report anything still requiring manual Play Mode testing.

Do not mark COMPLETE until user/manual verification is accepted.

### Implementation Handoff State

ChatGPT/GitHub first implementation is committed on `milestone/dribble-state`.

Implemented:

- explicit Active / Ended possession state,
- prototype end reason,
- restart API,
- R-key restart,
- temporary Ended overlay with Restart button,
- rhythm-clock query for whether the finite continuation vocabulary still has a reachable future target,
- expired-continuation attempt → Ended instead of repeat-unreachable dead loop,
- previous resolved action tracking,
- targeted `BT-DS-01` Unity validator.

GitHub-side review has been performed, but Unity compilation and executable validation have **not** been claimed by ChatGPT. Keep status `IN PROGRESS` until Codex performs the fresh-context compile/validator/review pass. After successful Codex verification, move to `AWAITING PLAYTEST`.

---

## 6. Current Task Template

```markdown
## 5. Current Task — BT-[SYSTEM]-[NUMBER]

**Title:** [TASK TITLE]
**Status:** PLANNED

### Goal

[ONE CLEAR WORKING OUTCOME.]

### Why This Task Exists

[ONLY THE CONTEXT NEEDED TO IMPLEMENT IT.]

### Scope

- [CHANGE]
- [CHANGE]

### Relevant Files / Systems

- `[PATH]`
- `[PATH]`

### Acceptance Criteria

- [ ] [OBSERVABLE CONDITION]
- [ ] [OBSERVABLE CONDITION]
- [ ] Existing behavior that must remain intact.

### Out of Scope

- [THING NOT TO IMPLEMENT]
- [ADJACENT ROADMAP ITEM]

### Verification Required

Automated:

- [ ] Build/compile passes.
- [ ] Relevant validators/tests pass.
- [ ] `git diff` reviewed.
- [ ] Unrelated files unchanged.

User/manual:

- [ ] [PLAY MODE / VISUAL / AUDIO / FEEL CHECK, OR `None`]
```

---

## 7. Task Procedure

When a Current Task exists:

1. Read `AGENTS.md`.
2. Read this file.
3. Read only relevant Source of Truth, Roadmap, and Decision material.
4. Inspect the relevant existing code.
5. Set task status to `IN PROGRESS`.
6. Implement only the Current Task.
7. Run relevant automated verification.
8. Review changed files and diff.
9. If runtime/manual verification is required:
   - set status to `AWAITING PLAYTEST`,
   - commit/push according to `AGENTS.md`,
   - stop for user verification.
10. Do not mark runtime-dependent work `COMPLETE` until the user confirms it.
11. After confirmed completion:
   - append to `Docs/COMPLETED_TASKS.md`,
   - update this file,
   - update `Docs/ROADMAP.md` if applicable,
   - update `Docs/DECISIONS.md` only when warranted,
   - commit/push documentation updates.

---

## 8. Git State

Intended workflow:

```text
main
└── dev
    └── milestone/<current-milestone>
```

Current repository baseline is on `main`.

Repository setup task should create:

- `dev` from the current stable `main`
- `milestone/ball-control` from `dev`

After that setup succeeds, update **Current branch at handoff** near the top of this file to:

`milestone/ball-control`

Rules:

- `main` = stable checkpoints/releases.
- `dev` = integration of completed milestones.
- `milestone/<name>` = active milestone development.
- Individual Current Tasks normally become commits on the current milestone branch.
- No milestone merge to `dev` without explicit user approval.
- No `dev` merge to `main` without explicit user approval.

---

## 9. Design Constraints To Preserve

- Bounce Theory is rhythm-first, not free-movement-first.
- Gameplay input is not gated by animation completion.
- Basketball actions share one underlying pulse.
- Different actions may occupy different rhythmic intervals on that pulse.
- Actions are rhythmically independent but physically contextual.
- Ball/game state determines valid follow-ups.
- Visual motion may overlap valid future inputs.
- Prototype timing and trajectory values remain tunable.
- No input does not automatically mean failure because intentional pauses/hesitations must remain possible.

> Every bounce creates an obligation, but not every obligation is another bounce.

---

## 10. Maintenance Rule

Keep this file short enough to function as a fast project handoff.

Update it when:

- the Current Task changes,
- a task changes status,
- a meaningful system becomes working,
- a blocker changes,
- a milestone starts or completes,
- important file ownership changes,
- an architecture change materially affects implementation.

Detailed completed-task narratives belong in `Docs/COMPLETED_TASKS.md`.
