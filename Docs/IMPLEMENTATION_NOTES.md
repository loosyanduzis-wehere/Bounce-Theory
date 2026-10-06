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
