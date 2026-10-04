# Bounce Theory — Implementation Status

**Status:** Active prototype  
**Completed milestone:** Core Rhythm + Pound Dribble Foundation  
**Next planned milestone:** Ball Control Language  
**Current task:** `BT-BC-07 — Basic Hesitation Foundation`  
**Git repository:** Configured  
**Remote:** `https://github.com/loosyanduzis-wehere/Bounce-Theory.git`  
**Baseline commit:** `078def9` — prototype through legacy Chunk 4.75  
**Current branch at handoff:** `milestone/ball-control`  
**Project path:** `C:\Users\jerry\Bounce Theory v2`  
**Design source of truth:** `Docs/Game Design.md`  
**Project instructions:** `AGENTS.md`  
**Last updated:** 2026-10-04

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
- Hesitation behavior
- Behind-the-back behavior
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

## 5. Current Task — BT-BC-07

**Title:** Basic Hesitation Foundation  
**Status:** AWAITING PLAYTEST

### Goal

Add the first functional hesitation action so the player can interrupt the normal dribble cadence with a readable same-hand hesitation while remaining inside Bounce Theory's existing rhythm/game-state architecture.

The hesitation should establish the idea that intentional space or delay can be a valid basketball action rather than automatically being treated as missed input or broken rhythm.

### Why This Task Exists

The prototype now supports pound dribbles and crossovers. Hesitation is the next distinct ball-control action because it tests a different design requirement: an action can intentionally create space in the rhythm without changing hand ownership.

This task should prove the basic hesitation language without locking final timing, animation, stance effects, defender reactions, or long-term gather rules.

### Scope

- Support the keyboard hesitation inputs defined by the Source of Truth:
  - **A** when the left hand controls the ball = left-hand hesitation.
  - **Right Arrow** when the right hand controls the ball = right-hand hesitation.
- Keep logical hand ownership on the same hand through the hesitation.
- Give the hesitation a clearly readable visual pause/hold or change of ball motion distinct from a normal pound dribble.
- Treat the hesitation as an intentional gameplay action, not as silence/failure.
- Preserve the shared DSP/rhythm judgment architecture and existing action-decision flow.
- Allow valid rhythmically/physically legal follow-up input to be considered without waiting for all visual motion to finish.
- Preserve pound-dribble and crossover behavior.
- Add targeted validation for left-hand and right-hand hesitation behavior where practical.

The exact hesitation duration, ball height, pause shape, rhythmic interval, and final animation remain prototype tuning choices.

### Relevant Files / Systems

Likely relevant:

- `Assets/BounceTheory/Scripts/PoundDribbleController.cs`
- `Assets/BounceTheory/Scripts/RhythmClock.cs`
- `Assets/BounceTheory/Editor/BounceTheorySceneBuilder.cs`
- `Assets/Scenes/BounceTheoryPrototype.unity`

Codex must inspect the existing action flow before deciding which files actually need changes.

### Acceptance Criteria

- [ ] With the ball controlled by the left hand, pressing **A** can initiate a left-hand hesitation.
- [ ] With the ball controlled by the right hand, pressing **Right Arrow** can initiate a right-hand hesitation.
- [ ] Hesitation preserves logical ownership of the current hand.
- [ ] Hesitation is visually distinguishable from a normal pound dribble and from a crossover.
- [ ] Hesitation is treated as an intentional action rather than a missed/no-input state.
- [ ] Hesitation uses the existing shared rhythm/game-state architecture rather than a separate timing system.
- [ ] A valid follow-up action is not blocked solely because the hesitation's visual motion has not completely finished.
- [ ] Existing **W / Up Arrow** pound dribbles still work.
- [ ] Existing **D / Left Arrow** crossovers still work.
- [ ] Existing rhythm/contact validators remain intact.
- [ ] No conventional player locomotion is introduced.
- [ ] Project compiles with no new errors.

### Out of Scope

Do **not** implement during this task:

- Behind-the-back
- Stance changes
- Defender reactions or steals
- Final hesitation timing values
- Permanent hesitation-to-rhythm interval mapping
- Gather/travel/double-dribble rules
- Full unresolved-bounce logic
- Finish/shooting logic
- Possession scoring
- Full final dribble state-machine redesign
- Final animation or animation blending
- Unrelated SceneBuilder cleanup/refactor

### Verification Required

Automated:

- [ ] Project compiles.
- [ ] Existing pound, crossover, rhythm, and contact validators still pass.
- [ ] Add or run targeted checks for left-hand and right-hand hesitation.
- [ ] Verify hesitation preserves hand ownership.
- [ ] Verify hesitation uses the existing action/rhythm path.
- [ ] Review `git diff`.
- [ ] Confirm unrelated files were not changed.

User/manual:

- [ ] In Play Mode, verify **A** creates a readable hesitation while left owns the ball.
- [ ] In Play Mode, verify **Right Arrow** creates a readable hesitation while right owns the ball.
- [ ] Confirm hesitation feels meaningfully different from simply doing nothing.
- [ ] Confirm pound and crossover controls still behave as before.
- [ ] Try a follow-up pound or crossover during/after the hesitation and confirm the rhythm remains responsive.

Codex should stop at `AWAITING PLAYTEST` after automated verification and push the implementation for user testing.

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
