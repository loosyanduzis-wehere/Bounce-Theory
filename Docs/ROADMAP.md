# Bounce Theory — Roadmap

> This file describes planned future work.
> It is **not** an automatic implementation queue.
> A roadmap item may be implemented only after it is promoted into the Current Task in `IMPLEMENTATION_STATUS.md` or explicitly requested by the user.

## Milestone 1 — Core Rhythm + Pound Dribble Foundation

**Status:** COMPLETE

Implemented:

- Clean stationary prototype foundation
- Pound dribble
- DSP rhythm clock
- Timing judgments
- Shared/global rhythmic grid
- Input authority independent of animation completion
- Rhythm-driven target floor contact
- Normal / compressed / unreachable contact handling
- Floor-impact audio tied to actual floor contact

Key completed task:

- `BT-BC-05 — Rhythm-Driven Floor Contact`

---

## Milestone 2 — Ball Control Language

**Status:** PLANNED

Planned areas:

- Crossover
- Hesitation
- Behind-the-back
- Ball-state branching
- Legal follow-up transitions
- Interaction between rhythmic intervals and different dribble actions

Do not implement all of these as one task. Promote one small task at a time into `IMPLEMENTATION_STATUS.md`.

---

## Milestone 3 — Stance

**Status:** PLANNED

Planned areas:

- Mid-dribble stance changes
- Stance effects on rhythm
- Stance effects on trajectory
- Stance effects on exposure
- Stance-dependent follow-up actions

---

## Milestone 4 — Defender

**Status:** PLANNED

Planned areas:

- Centered state
- Lean
- Recovery
- Reach
- Overcommit
- Beaten state
- Rhythm-based steal opportunities

---

## Milestone 5 — Finishes

**Status:** PLANNED

Planned areas:

- Shot
- Stepback
- Drive
- Dunk / finish expansion
- Defender-dependent timing windows

---

## Milestone 6 — Possession Scoring

**Status:** PLANNED

Planned areas:

- Rhythm quality
- Efficiency
- Style / move variety
- Defender manipulation
- Finish difficulty

---

## Milestone 7 — Animation and Spectacle

**Status:** PLANNED

Planned areas:

- Replace prototype visual motion with polished animation
- Preserve rhythm/game-state authority underneath animation
- Expand expressive and intentionally outrageous finishes

---

## Maintenance / Technical Backlog

These items are worth revisiting, but they are not active tasks:

- `BounceTheorySceneBuilder.cs` has accumulated legacy `UpgradeToChunkX` and validation behavior. As the prototype matures, separate scene construction, upgrade/setup logic, and validation rather than continuing to grow one editor file.
- Audit Unity template leftovers such as `SampleScene`, tutorial/readme assets, and the default input-action asset only after confirming they are unused.
- Audit unnecessary Unity packages later; do not remove packages merely for cleanup while core prototype systems are still being proven.
