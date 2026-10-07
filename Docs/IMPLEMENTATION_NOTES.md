# Bounce Theory — Implementation Notes

> Active technical realities, workarounds, discoveries, and prototype-specific constraints.
> This is not design truth, roadmap history, or the current task definition.

## Active Branch / Handoff

- Active development branch for the next parallel feature: `milestone/defender-interaction`.
- Current task definition and verification gate live in `Docs/IMPLEMENTATION_STATUS.md`.
- `dev` remains the completed-milestone integration branch.
- No milestone merge to `dev` without explicit user approval.

### Branch-history note

`milestone/dribble-state` was created from `milestone/stance` while BT-ST-03 was still under user playtest.

The stance branch later received documentation-only completion commits. The accepted stance gameplay state is already present on the dribble-state branch, and its accepted milestone status has been carried into this branch's documentation.

Do not merge/rebase solely to erase this historical divergence. Treat the actual feature branch and documented state as authoritative unless integration work is explicitly requested.

## Rhythm / Input Technical Reality

- Gameplay pipeline:
  `keypress → DSP timestamp → rhythm judgment → action decision → visual execution`.
- Target floor contact is scheduled against the shared/global rhythm grid.
- Animation completion is not authoritative for accepting gameplay input.
- Pending actions preserve their original judgment/contact target rather than being rejudged at execution time.
- The current prototype interval vocabulary has a maximum supported continuation target of `2.0 beats`.

## Continuation / Restart Behavior

Before BT-DS-01, an attempted continuation after the latest supported target could repeatedly resolve against an expired rhythm reference and remain unreachable.

Current prototype behavior:

- time passing alone does not end possession,
- an attempted continuation after no reachable supported target remains ends the prototype possession,
- restart resets ball/stance/transient input state, sequence context, and rhythm event history.

The final basketball interpretation of that boundary remains intentionally unresolved. Do not hard-code it as permanent gather, mishandle, travel, or double-dribble semantics.

## Dribble-State Technical Model

The implementation uses separate state dimensions rather than a monolithic enum:

- possession: Active / Ended,
- motion phase: Controlled / Descending / FloorContact / Returning,
- hand ownership,
- stance,
- timing judgment / control quality,
- resolved sequence context,
- active/pending follow-up relation.

Control-quality mapping:

- Perfect / Good → Secure,
- Early / Late → Recovering,
- Broken Rhythm → Exposed.

Follow-up relation:

- FirstAction,
- Repeat,
- SameHandVariation,
- Transfer,
- CounterTransfer.

A queued relation is captured when the input is accepted and must not be recomputed when it later executes.

## Stance Technical Reality

- Current stance-modifier grace is `0.18 s`; treat it as provisional tuning.
- Prototype stance visuals are readability placeholders, not final animation.
- Stance-specific dribble profiles capture stance at action acceptance.
- Final stance/rhythm, stance/exposure, and exact animation feel remain deferred.

## Verification Infrastructure

- `BounceTheorySceneBuilder.cs` currently owns scene-construction, upgrade, and validation utilities.
- It has accumulated legacy validation/setup code.
- Do not refactor it merely for cleanliness during active gameplay prototyping.
- Separate scene construction, upgrade logic, and validation later as maintenance work.
- Gameplay/visual/feel work still requires user Play Mode acceptance even when compile/validators pass.

## BT-DS-01 Fresh-Context Verification

- The consolidated Unity validator initially exposed that a queued action could be accepted and judged, then discarded when the visual handoff reapplied the prototype maximum-speed heuristic. Queued actions now preserve gameplay authority and use compressed prototype motion when needed; a genuinely passed queued target is rejected and ends the prototype possession instead of disappearing silently.
- An attempted continuation beyond the finite `2.0-beat` vocabulary now reports both `ContinuationWindowExpired` possession end and an explicit `Unreachable` motion outcome, preserving the existing contact-scheduling diagnostic contract.
- `BT-DS-01 Complete Dribble State` passes after the fixes, including representative stance-profile, pound, crossover, hesitation, behind-the-back, queued-input, rhythm, and target-contact regressions.
- The C# editor project compiles with zero errors. The remaining warnings are pre-existing Unity API obsolescence warnings in editor validation code.


## BT-DF-01 Defender Lean / Recovery Implementation

ChatGPT/GitHub first implementation is present on `milestone/defender`.

Technical shape:

- New `PrototypeDefenderController` subscribes to the existing `PoundDribbleController` gameplay events.
- Defender reaction timing does not own a separate rhythm clock.
- A dedicated `DefenderReactionVisual` pivot carries temporary body lean/recovery motion while the `Defender` root remains stationary.
- Dribble start commits the defender toward the action source-hand side unless the defender is already Recovering.
- Crossover and behind-the-back transfer resolution at floor contact enter Recovering when ownership resolves away from the committed side.
- Recovery duration is derived from the offense's accepted `BallControlQuality`: Secure > Recovering > Exposed.
- A hesitation that actually begins while defender state is Recovering extends the active recovery duration.
- `PoundDribbleController.PossessionRestarted` is a new narrow integration event so restart can reset defender state without polling UI/input state.
- The existing scene is upgraded through `Bounce Theory/Upgrade Prototype Scene To BT-DF-01 Defender Lean Recovery`; do not manually rewrite scene YAML.
- Targeted validator: `Bounce Theory/Validate BT-DF-01 Defender Lean Recovery`.

ChatGPT did not claim Unity compilation or executable validation. Codex must run the scene upgrade, compile, run the targeted validator, run the existing BT-DS-01 and representative stance/rhythm regressions, and repair clear in-scope issues before the task can move to `AWAITING PLAYTEST`.


## BT-DF-02/03 Defender Interaction Implementation

This branch was created while Codex reviewed BT-DF-01. Codex subsequently completed BT-DF-01 executable verification on `milestone/defender` and found one material architecture issue: hesitation recovery extension must occur at accepted command time, not delayed visual execution. That fix, the accepted-action event, the corrected validator behavior, and the serialized defender scene setup were carried forward and merged into this branch before the larger defender feature handoff.

Combined prototype rules:

- Secure offense → Protected; defender does not auto-reach.
- Recovering offense → Contested reach.
- Exposed offense → Vulnerable reach.
- Vulnerable reach steals at floor contact even if another action was queued.
- Contested reach steals if no follow-up was already accepted.
- Contested reach with an accepted queued follow-up misses and creates Overcommitted.
- A Secure crossover, hesitation, or behind-the-back that executes during Overcommitted creates Beaten.
- Pound does not count as the overcommit-breaking counter.
- Overcommitted and Beaten are timed readability windows.
- Successful prototype steal calls the existing possession lifecycle with `PossessionEndReason.DefenderSteal`.
- Defender root remains stationary; presentation stays on `DefenderReactionVisual`.

No random steal roll is used in this prototype. Final defender intelligence, steal percentages, stance-based vulnerability, fouls, finishes, and scoring remain unresolved.

Targeted validator:

`Bounce Theory/Validate BT-DF-02-03 Defender Interaction`

Current branch code has not been Unity-compiled or executable-validated by ChatGPT. The next Codex pass must rerun the combined scene upgrade, compile, run BT-DF-02/03, BT-DF-01, BT-DS-01, and representative stance/rhythm regressions.
