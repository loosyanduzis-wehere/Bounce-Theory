# Bounce Theory — Implementation Status

**Status:** Active prototype  
**Completed milestone:** Stance — prototype complete  
**Next planned milestone:** Dribble State / Possession Foundation  
**Current task:** `BT-DS-01 — Complete Prototype Dribble-State Milestone`  
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

**Title:** Complete Prototype Dribble-State Milestone  
**Status:** IN PROGRESS

### Goal

Finish the prototype dribble-state/state-machine layer before the first consolidated Codex review.

The completed milestone must represent the context promised by Phase 5 of the Source of Truth:

`hand + ball phase + stance + timing + previous move + follow-up input`

while retaining the already implemented possession/restart foundation and avoiding premature final combo, violation, defender, or scoring rules.

### Unknowns Pass — Resolved For This Milestone

- Possession lifecycle is separate from ball-motion phase.
- Restart is stable prototype infrastructure; final possession-end causes remain extensible.
- Silence/inactivity alone does not end a possession.
- Attempting continuation after the finite supported rhythm window has expired ends the prototype possession cleanly.
- Previous **resolved** action context must be preserved independently from the newest judged/pending input.
- A follow-up accepted during Descending/FloorContact should contextually follow the active action even before that action has visually returned.
- Ball-control quality should be explicit for future defender logic:
  - Perfect / Good → **Secure**
  - Early / Late → **Recovering**
  - Broken Rhythm → **Exposed**
- Current follow-up legality remains grounded in existing physical rules: active hand, ball phase, reachability, one pending slot, and possession state.
- Do not invent a fixed combo table or final stance-dependent follow-up restrictions merely to complete the milestone.
- Previous action already matters mechanically through hand ownership; explicit follow-up relation/context is added so future systems can reason about the sequence without reconstructing it from animation.

### State Model

Keep separate dimensions instead of one giant enum:

#### Possession

- `Active`
- `Ended`

#### Ball motion phase

- `Controlled`
- `Descending`
- `FloorContact`
- `Returning`

#### Ball-control quality

- `Secure`
- `Recovering`
- `Exposed`

#### Sequence context

Preserve, at minimum:

- whether a previous action has resolved,
- previous resolved action,
- previous resolved stance,
- previous resolved timing judgment,
- hand after the resolved action,
- previous resolved control quality,
- current sequence action count,
- active follow-up relation,
- pending follow-up relation.

### Follow-Up Relation

Classify accepted actions without imposing a fixed combo table:

- `FirstAction`
- `Repeat`
- `SameHandVariation`
- `Transfer`
- `CounterTransfer`

The relation is contextual/debuggable state for later defender, exposure, scoring, and legal-follow-up systems. It must be captured when the action is accepted and preserved if the action waits in the pending slot.

### Existing BT-DS-01 Foundation To Preserve

Already implemented on this branch:

- explicit Active / Ended possession state,
- prototype possession end reason,
- R-key restart,
- temporary restart overlay/button,
- clean reset of ball/stance/pending/rhythm history,
- expired finite-continuation attempt → Ended,
- previous-action tracking,
- initial BT-DS-01 validator.

### Scope

#### Complete resolved-action context

When an action reaches floor contact, preserve its resolved:

- action,
- stance,
- timing judgment,
- resulting hand,
- ball-control quality.

Increment sequence action count at resolution.

#### Complete accepted follow-up context

At action acceptance:

- determine its immediate preceding action context,
- classify the follow-up relation,
- preserve that relation through pending/queued execution,
- expose active and pending relation for debug/validation.

#### Ball-control quality

- Map the accepted timing judgment into Secure / Recovering / Exposed.
- Carry the active quality with the accepted action.
- Preserve the resolved quality at floor contact.
- Reset to Secure on possession restart.
- Do not add steal behavior yet.

#### Decision/readability state

Debug output should make the current state machine legible enough to verify:

- possession,
- motion phase,
- hand,
- stance,
- active action,
- active control quality,
- previous resolved action/context,
- active/pending follow-up relation,
- sequence count.

### Success Criteria

- [ ] BT-DS-01 possession Ended/restart behavior remains intact.
- [ ] Fresh first action is classified `FirstAction`.
- [ ] Repeating the same action is classified `Repeat`.
- [ ] A different same-hand action can classify `SameHandVariation`.
- [ ] A hand-transferring action after a non-transfer context classifies `Transfer`.
- [ ] A hand-transferring action following another transfer action classifies `CounterTransfer`.
- [ ] A queued action preserves the relation it had when accepted.
- [ ] Resolved action context does not get overwritten by a newer pending input before the current action resolves.
- [ ] Perfect/Good actions expose Secure control quality.
- [ ] Early/Late actions expose Recovering control quality.
- [ ] Broken Rhythm exposes Exposed control quality.
- [ ] Resolved control quality is preserved for future systems.
- [ ] Sequence action count increments on floor contact and clears on restart.
- [ ] Existing hand ownership rules remain correct.
- [ ] Existing ball-phase immediate/queued behavior remains correct.
- [ ] Existing stance-specific profiles remain correct.
- [ ] Existing DSP/contact targets and rhythm judgments remain authoritative.
- [ ] Restart clears sequence/follow-up/control context.
- [ ] Player root, defender, and camera remain stationary.

### Out of Scope

Do **not** implement:

- fixed/canned combo sequences,
- a full move-to-move legality table,
- final stance-dependent follow-up advantages,
- automatic gather from inactivity,
- travel/double-dribble rules,
- loose-ball physics,
- defender reactions, steals, reach, lean, or recovery,
- scoring,
- finishes,
- polished UI,
- final animation,
- unrelated refactors.

### Verification Plan

Automated / executable:

- [ ] Project compiles.
- [ ] Expand the dribble-state validator to cover possession lifecycle plus sequence/follow-up/control-quality context.
- [ ] Validate each follow-up relation deterministically.
- [ ] Validate queued relation capture.
- [ ] Validate resolved context is not overwritten by a pending input.
- [ ] Validate Secure / Recovering / Exposed timing mapping.
- [ ] Validate restart clears all new context.
- [ ] Run representative stance, pound, crossover, hesitation, behind-the-back, rhythm/contact regressions.
- [ ] Review final branch diff against `milestone/stance`.
- [ ] Confirm unrelated files were not changed.

User/manual after Codex:

- [ ] Confirm normal quick chains still feel responsive.
- [ ] Confirm the debug state follows the actual sequence being performed.
- [ ] Confirm expired continuation produces the restart overlay.
- [ ] Confirm R and the Restart button restore a clean possession.
- [ ] Confirm stance behavior remains intact.

### Handoff Rule

Do **not** send this branch to Codex until the full prototype dribble-state milestone above is implemented through GitHub.

Then use **one consolidated Codex pass** as fresh reviewer/fixer/verifier across the entire `milestone/dribble-state` diff.

Keep status `IN PROGRESS` until Codex has compiled, run the targeted validator/regressions, inspected the actual diff, and repaired any clear in-scope material issues. After successful Codex verification, move to `AWAITING PLAYTEST`.

Do not mark COMPLETE until user/manual verification is accepted.

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
