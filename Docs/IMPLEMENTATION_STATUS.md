# Bounce Theory — Implementation Status

**Status:** Active prototype  
**Completed milestone:** Ball Control Language — core vocabulary  
**Next planned milestone:** Stance  
**Current task:** `BT-ST-02 — Stance-Dependent Pound Bounce Height`  
**Git repository:** Configured  
**Remote:** `https://github.com/loosyanduzis-wehere/Bounce-Theory.git`  
**Baseline commit:** `078def9` — prototype through legacy Chunk 4.75  
**Current branch at handoff:** `milestone/stance`  
**Project path:** `C:\Users\jerry\Bounce Theory v2`  
**Design source of truth:** `Docs/Game Design.md`  
**Project instructions:** `AGENTS.md`  
**Last updated:** 2026-10-05

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

## 5. Current Task — BT-ST-02

**Title:** Stance-Dependent Pound Bounce Height  
**Status:** PLANNED

### Goal

Make the three stance states begin affecting basketball behavior by giving the **pound dribble** a clearly different vertical profile in Low, Medium, and High stance while preserving the existing rhythm/contact-timing architecture.

This is a focused prototype experiment. Do not generalize stance effects to every dribble action yet.

### Desired Behavior

- **Medium stance** preserves the current pound-dribble behavior exactly as the baseline.
- **Low stance** produces a visibly lower/tighter pound-dribble profile.
- **High stance** produces a visibly higher/more upright pound-dribble profile.
- The differences should be obvious enough to feel in Play Mode but remain prototype tuning rather than locked final values.
- The scheduled floor-contact target remains authoritative.
- The pound input keeps its real DSP timestamp and normal rhythm judgment regardless of stance.
- Stance changes may still occur during active dribble motion.

### Why This Task Exists

BT-ST-01 proved the three-state control language, direct stance modifiers, and prototype stance silhouettes.

The design now needs one isolated test of whether stance actually changing move behavior creates useful basketball decisions. Pound is the cleanest first experiment because it provides a simple vertical action without also introducing lateral ownership transfer or hesitation-specific presentation.

### Scope

- Add tunable prototype values for Low and High pound-dribble vertical behavior.
- Keep Medium as the exact existing baseline.
- Apply the stance-specific profile only when a **new pound action is accepted**.
- Capture/use the relevant stance for that accepted pound without making animation completion authoritative.
- Preserve the existing global-grid target floor-contact time.
- Preserve normal/compressed/unreachable contact handling.
- Preserve stance controls and the 0.18-second modifier grace behavior from BT-ST-01.
- Make the active pound's stance/profile readable in debug output if useful for verification.

### Acceptance Criteria

- [ ] Medium stance pound looks and behaves like the current baseline.
- [ ] Low stance pound is visibly lower/tighter than Medium.
- [ ] High stance pound is visibly higher than Medium.
- [ ] All three still target the same rhythm/contact architecture rather than separate stance-specific clocks.
- [ ] Stance does not change the original dribble keypress timestamp.
- [ ] A stance change during an already active pound does not retroactively rewrite that pound's accepted rhythm judgment.
- [ ] Crossover, hesitation, and behind-the-back behavior are unchanged.
- [ ] Plain Space and Space + dribble stance controls remain intact.
- [ ] Player root, defender, and camera remain stationary.
- [ ] Project compiles with no new errors.

### Out of Scope

Do **not** implement during this task:

- stance-dependent crossover trajectory,
- stance-dependent hesitation behavior,
- stance-dependent behind-the-back behavior,
- stance-dependent rhythmic intervals,
- stance-dependent timing windows,
- ball exposure,
- defender reactions,
- move legality/follow-up restrictions,
- final animation,
- player locomotion,
- unrelated refactors.

### Verification Required

Automated:

- [ ] Project compiles.
- [ ] Add targeted checks proving Low < Medium < High for the accepted pound vertical profile.
- [ ] Verify Medium retains the previous baseline tuning.
- [ ] Verify floor-contact targeting remains grid-driven and within existing tolerance.
- [ ] Verify crossover, hesitation, and behind-the-back regressions still pass.
- [ ] Review `git diff`.
- [ ] Confirm unrelated files were not changed.

User/manual:

- [ ] Compare repeated pounds in Low, Medium, and High stance.
- [ ] Confirm Low feels/readably lower than Medium.
- [ ] Confirm High feels/readably higher than Medium.
- [ ] Confirm rhythm still feels like the same underlying pulse.
- [ ] Confirm stance controls and other dribble moves still work.

After automated verification, set this task to `AWAITING PLAYTEST`, commit/push, and stop for user verification. Do not mark COMPLETE until the user accepts the Play Mode behavior.

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
