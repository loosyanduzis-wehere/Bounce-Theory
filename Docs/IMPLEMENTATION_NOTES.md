# Bounce Theory — Implementation Notes

> Current technical realities, workarounds, discoveries, and prototype-specific constraints.
> This file is not the game-design Source of Truth.

## Current Branch / Milestone

- Active development branch: `milestone/stance`.
- `dev` is the completed-milestone integration branch.
- Current task at the time this file was introduced: `BT-ST-03 — Complete Prototype Stance Behavior Pass`, awaiting user Play Mode verification.

## Rhythm / Input Architecture

- Gameplay pipeline remains:
  `keypress → DSP timestamp → rhythm judgment → action decision → visual execution`.
- Target floor contact is scheduled against the shared/global rhythm grid.
- Animation completion is not authoritative for accepting gameplay input.
- Pending actions preserve their original judgment/contact target rather than being rejudged when execution becomes physically possible.

## Stance Implementation

- Stance states are Low / Medium / High; Medium is default.
- Plain Space alternates through Medium → Low → Medium → High → Medium...
- Direct stance modifiers:
  - Space + Crossover → Low
  - Space + Pound → Medium
  - Space + Hesitation → High
- Space + Behind-the-back intentionally has no stance destination.
- Current Space-modifier grace is `0.18 s`; treat this as provisional tuning.
- Prototype stance visuals use `PrototypeStanceVisual`; they are readability placeholders, not final animation.
- Stance-dependent dribble profiles capture stance at action acceptance so later stance changes do not rewrite the active action.
- BT-ST-02 confirmed stance-dependent pound behavior works mechanically, but the perceptual difference is subtle without final animation.

## Verification Infrastructure

- `BounceTheorySceneBuilder.cs` currently owns a growing set of prototype upgrade and validation utilities.
- This is useful for deterministic Unity validation but has accumulated substantial legacy validation/setup code.
- Do not refactor it merely for cleanliness during active gameplay prototyping; treat separation of scene construction, upgrade logic, and validation as maintenance backlog.
- Gameplay/visual/feel work still requires user Play Mode acceptance even after compile/validators pass.

## Workflow

- Default implementation path is now:
  ChatGPT architecture/Unknowns Pass → GitHub implementation → Git handoff → fresh Codex review/fix/verification → user Play Mode when needed.
- Codex is not the default first implementer for ordinary feature work.
- Use Codex as primary implementer when local iterative Unity build/test/debug work or broad mechanical changes make that clearly more efficient.


## Dribble Continuation / Restart Discovery

- Parallel next-feature branch: `milestone/dribble-state`, created from `milestone/stance` while BT-ST-03 remains in user Play Mode verification.
- The current rhythm planner uses a finite continuation vocabulary through a maximum prototype interval of `2.0 beats`.
- After the latest target derived from the previous floor-contact reference has passed, the current planner can return a target in the past; `BeginDribble` then reports the target as unreachable. Because no new floor event is registered, repeated continuation attempts can remain stuck on the same expired reference.
- The next feature should convert this dead continuation boundary into an explicit prototype possession end state and provide a restart path that resets ball/stance/transient input state and rhythm event history.
- Exact final basketball meaning of this boundary remains open; do not hard-code it as permanent travel, double-dribble, mishandle, or gather semantics yet.


## Stance Milestone Acceptance

- BT-ST-03 user Play Mode verification was accepted on **2026-10-06**.
- Milestone 3 — Stance is now **PROTOTYPE COMPLETE**.
- Remaining stance/rhythm, stance/exposure, final animation, and exact feel tuning are deferred until the systems they interact with exist.


## Complete Prototype Dribble-State Implementation

ChatGPT/GitHub implementation for the full prototype dribble-state milestone is now present on `milestone/dribble-state` and awaits the consolidated Codex review/compile/validator pass.

Implemented state dimensions:

- **Possession:** Active / Ended.
- **Motion phase:** Controlled / Descending / FloorContact / Returning.
- **Hand ownership:** Left / Right.
- **Stance:** Low / Medium / High.
- **Control quality:** Secure / Recovering / Exposed.
- **Sequence context:** previous resolved action, stance, judgment, resulting hand, resolved control quality, sequence action count.
- **Follow-up relation:** FirstAction / Repeat / SameHandVariation / Transfer / CounterTransfer.
- **Pending context:** accepted stance, timing/contact plan, and follow-up relation are preserved until queued execution.

Control-quality mapping:

- Perfect / Good → Secure.
- Early / Late → Recovering.
- Broken Rhythm → Exposed.

The current system does not impose a canned combo table. Existing physical/context rules remain authoritative: possession state, active hand, ball phase, reachability, one pending slot, stance profile, rhythm judgment, and follow-up input.

The follow-up relation is captured at input acceptance. A queued action therefore does not recompute its sequence relation when it later becomes visually/physically executable.

The branch was created while `milestone/stance` was still under user playtest. The stance branch later received documentation-only completion commits. Their accepted stance state has been carried into this branch's documentation; no accepted stance gameplay code is missing from the dribble-state branch.
