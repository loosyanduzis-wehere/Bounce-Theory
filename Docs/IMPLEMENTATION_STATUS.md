# Bounce Theory — Implementation Status

**Status:** Active prototype  
**Completed milestone:** Ball Control Language — core vocabulary  
**Next planned milestone:** Stance  
**Current task:** `BT-ST-03 — Complete Prototype Stance Behavior Pass`  
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

## 5. Current Task — BT-ST-03

**Title:** Complete Prototype Stance Behavior Pass  
**Status:** PLANNED

### Goal

Extend the existing stance system so Low, Medium, and High produce clearly different **prototype expressions across the current dribble vocabulary**, while preserving the existing rhythm/contact architecture and keeping all stance tuning reversible.

This is intentionally a larger Codex task than the previous stance chunks so related stance behavior can be implemented and verified together.

### Working Stance Identity

Use the following prototype identity consistently:

- **Low:** compact, lower, tighter basketball expression.
- **Medium:** current neutral/baseline behavior.
- **High:** elevated, more upright/open basketball expression.

These are prototype presentation/gameplay profiles, not locked final animation values.

### Existing Behavior To Preserve

Already implemented and accepted:

- Low / Medium / High stance state.
- Medium default stance.
- Plain Space stance flow.
- Space + Crossover → Low.
- Space + Pound → Medium.
- Space + Hesitation → High.
- Space + Behind-the-back remains unassigned.
- 0.18-second stance-modifier grace window.
- Prototype Low / Medium / High player silhouettes.
- Pound dribble captures stance at acceptance and uses Low / Medium / High vertical profiles.
- Medium pound remains the original baseline.

### Scope

Extend stance-dependent move expression to the remaining existing dribble actions:

#### Crossover

- Capture the accepted stance when the crossover is accepted.
- Preserve Medium crossover as the current baseline.
- Give Low and High distinct tunable prototype trajectory/readability profiles consistent with the working stance identity.
- Preserve hand transfer, floor-contact target, DSP input timestamp, and rhythm judgment.

#### Hesitation

- Capture the accepted stance when hesitation is accepted.
- Preserve Medium hesitation as the current baseline.
- Give Low and High distinct tunable prototype hold/lift/body-readability behavior consistent with the working stance identity.
- Preserve same-hand ownership, floor-contact target, DSP input timestamp, and rhythm judgment.

#### Behind-the-Back

- Capture the accepted stance when behind-the-back is accepted.
- Preserve Medium behind-the-back as the current baseline.
- Give Low and High distinct tunable prototype depth/height/wrap readability consistent with the working stance identity.
- Do **not** assign a special Space + Behind-the-back stance destination.
- Preserve hand transfer, floor-contact target, DSP input timestamp, and rhythm judgment.

### Architecture Requirements

- Capture stance at **action acceptance**, not continuously during visual motion.
- Changing stance after an action is accepted must not retroactively rewrite that active action's accepted stance profile.
- Queued/pending actions must preserve the stance profile they were accepted with.
- Gameplay/rhythm state remains authoritative over animation completion.
- Do not create separate clocks or timing systems per stance.
- Prefer tunable serialized prototype values rather than hard-coded final-feel constants.
- Keep Medium behavior equal to the existing pre-stance baseline wherever practical.

### Acceptance Criteria

- [ ] Low / Medium / High produce visibly/readably different crossover profiles.
- [ ] Low / Medium / High produce visibly/readably different hesitation profiles.
- [ ] Low / Medium / High produce visibly/readably different behind-the-back profiles.
- [ ] Medium remains the current baseline for all three actions.
- [ ] Pound stance behavior from BT-ST-02 remains intact.
- [ ] Each action captures stance at acceptance and retains it if stance changes later.
- [ ] Pending actions retain the stance they were accepted with.
- [ ] Equivalent inputs across stances preserve the same DSP timestamp, rhythm judgment, and target contact time.
- [ ] Hand ownership rules remain correct.
- [ ] Plain Space and direct stance modifiers remain intact.
- [ ] Space + Behind-the-back remains unassigned.
- [ ] Player root, defender, and camera remain stationary.
- [ ] Project compiles with no new errors.

### Out of Scope

Do **not** implement during this task:

- stance-dependent rhythm intervals,
- stance-dependent timing windows,
- ball exposure,
- legal/illegal follow-up restrictions,
- defender reaction,
- defender AI,
- steal opportunities,
- shots, drives, or finishes,
- final animation assets,
- conventional locomotion,
- unrelated refactors.

### Verification Required

Automated:

- [ ] Project compiles.
- [ ] Add targeted stance-profile validation for crossover, hesitation, and behind-the-back.
- [ ] Verify Medium retains the current baseline behavior for each move.
- [ ] Verify stance capture survives later stance changes.
- [ ] Verify queued actions preserve their accepted stance profile.
- [ ] Verify equivalent inputs keep the same DSP timestamp, rhythm judgment, and target floor-contact time across stance profiles.
- [ ] Run representative pound, rhythm/contact, and stance-control regressions.
- [ ] Review `git diff`.
- [ ] Confirm unrelated files were not changed.

User/manual:

- [ ] Compare crossover in Low / Medium / High.
- [ ] Compare hesitation in Low / Medium / High.
- [ ] Compare behind-the-back in Low / Medium / High.
- [ ] Confirm Medium still feels like the existing baseline.
- [ ] Confirm stance differences are readable even if final tuning remains provisional until animation.
- [ ] Confirm all stance controls and pound behavior still work.

After automated verification, set this task to `AWAITING PLAYTEST`, commit/push `milestone/stance`, and stop for user verification.

Do not mark COMPLETE until the user accepts the Play Mode behavior.

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
