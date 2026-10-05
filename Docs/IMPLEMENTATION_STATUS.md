# Bounce Theory — Implementation Status

**Status:** Active prototype  
**Completed milestone:** Ball Control Language — core vocabulary  
**Next planned milestone:** Stance  
**Current task:** `BT-ST-01 — Stance State + Plain Space Flow`  
**Git repository:** Configured  
**Remote:** `https://github.com/loosyanduzis-wehere/Bounce-Theory.git`  
**Baseline commit:** `078def9` — prototype through legacy Chunk 4.75  
**Current branch at handoff:** `milestone/stance`  
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

## 5. Current Task — BT-ST-01

**Title:** Stance State + Space Control Foundation  
**Status:** AWAITING PLAYTEST

### Goal

Create the minimum three-stance gameplay foundation, support the approved plain-Space flow, and wire the first direct stance modifier inputs into existing dribble actions:

`Medium → Low → Medium → High → Medium → Low → ...`

Direct stance modifiers:

- **Space + Crossover → Low**
- **Space + Pound → Medium**
- **Space + Hesitation → High**

This task proves stance state and fast mid-dribble stance switching before stance begins modifying dribble behavior.

### Why This Task Exists

The core dribble vocabulary is now working. Stance is the next gameplay layer, but the first implementation should establish only the state model and basic Space control rather than mixing in stance-dependent trajectories, timing, exposure, or modifier combinations.

### Scope

- Add explicit **Low**, **Medium**, and **High** stance states.
- Default the player to **Medium** stance.
- Implement plain **Space** flow exactly as:
  - Medium → Low
  - Low → Medium
  - Medium → High
  - High → Medium
  - then repeat.
- Preserve the alternating extreme so returning to Medium remembers whether the next plain-Space destination should be Low or High.
- Allow stance input during an active dribble sequence; do not gate Space on ball-animation completion.
- Make the current stance temporarily readable in Play Mode using existing debug feedback or another minimal prototype-only indication.
- Holding Space while an accepted crossover, pound, or hesitation input occurs must set the mapped stance while still performing that dribble action.
- A modified dribble must consume that Space press so releasing Space does not also advance the plain-Space stance flow.
- Preserve all existing pound, crossover, hesitation, behind-the-back, rhythm, and contact behavior.

### Relevant Files / Systems

Likely relevant:

- `Assets/BounceTheory/Scripts/PoundDribbleController.cs`
- `Assets/BounceTheory/Editor/BounceTheorySceneBuilder.cs`
- `Assets/Scenes/BounceTheoryPrototype.unity`

Codex should inspect the project and choose the smallest sensible owner for stance state. Prefer keeping stance state conceptually separate from visual animation timing.

### Acceptance Criteria

- [ ] The prototype exposes three stance states: Low, Medium, and High.
- [ ] Play begins in Medium stance.
- [ ] Repeated plain Space presses produce `Medium → Low → Medium → High → Medium → Low...`.
- [ ] Low always returns to Medium with one plain Space press.
- [ ] High always returns to Medium with one plain Space press.
- [ ] Returning to Medium preserves which extreme should come next.
- [ ] Space can change stance while a dribble visual is still active.
- [ ] Current stance is readable during prototype playtesting.
- [ ] Holding Space + Crossover performs the crossover and leaves the player in Low stance.
- [ ] Holding Space + Pound performs the pound and leaves the player in Medium stance.
- [ ] Holding Space + Hesitation performs the hesitation and leaves the player in High stance.
- [ ] Releasing Space after a modified dribble does not also advance the plain-Space stance cycle.
- [ ] Existing dribble controls and rhythm/contact behavior remain unchanged.
- [ ] Project compiles with no new errors.

### Out of Scope

Do **not** implement during this task:

- Any special `Space + Behind-the-back` behavior
- Stance effects on bounce height or trajectory
- Stance effects on rhythm/timing windows
- Stance effects on exposure, defender reaction, or move legality
- Final stance animation
- Player locomotion
- Defender behavior
- Unrelated refactors

### Verification Required

Automated:

- [ ] Project compiles.
- [ ] Existing dribble/rhythm/contact validators still pass.
- [ ] Add targeted validation for the complete plain-Space stance cycle.
- [ ] Verify stance can change independently of current dribble visual completion.
- [ ] Review `git diff`.
- [ ] Confirm unrelated files were not changed.

User/manual:

- [ ] Start Play Mode and confirm the initial stance is Medium.
- [ ] Press Space repeatedly and confirm `Medium → Low → Medium → High → Medium → Low...`.
- [ ] Press Space during active dribble motion and confirm stance still changes.
- [ ] Hold Space + crossover and confirm the crossover occurs and stance becomes Low.
- [ ] Hold Space + pound and confirm the pound occurs and stance becomes Medium.
- [ ] Hold Space + hesitation and confirm the hesitation occurs and stance becomes High.
- [ ] Confirm W/Up, D/Left, A/Right, and S/Down dribble controls still work as before.

Implementation note: the stance input/state patches were made directly through GitHub. Plain Space changes stance on release when it was not used as a modifier. Holding Space with an accepted crossover, pound, or hesitation sets Low, Medium, or High respectively while preserving the dribble action. A separate prototype-only visual now gives Low, Medium, and High distinct silhouettes without moving the player root or ball anchors. Unity compilation and automated stance/dribble validation pass; user Play Mode verification is still required.

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
