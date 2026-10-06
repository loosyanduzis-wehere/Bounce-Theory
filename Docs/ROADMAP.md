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

**Status:** CORE VOCABULARY COMPLETE

Completed:

- Basic crossover foundation — `BT-BC-06`
- Basic hesitation foundation — `BT-BC-07`
- Basic behind-the-back foundation — `BT-BC-08`

Planned areas:
- Ball-state branching
- Legal follow-up transitions
- Interaction between rhythmic intervals and different dribble actions
- Revisit continuation/grace timing after the ball returns to control; current behavior ends the active dribble sequence if the player does not continue in time. Preserve the possibility of a gathered/dead-ball consequence, but later test whether Easy should allow additional late input grace without weakening rhythm scoring.

Do not implement all of these as one task. Promote one small task at a time into `IMPLEMENTATION_STATUS.md`.

---

## Milestone 3 — Stance

**Status:** PROTOTYPE COMPLETE

Working control design:

- Three stances: Low / Medium / High
- Medium is the neutral/home stance
- Plain Space flow: Medium → Low → Medium → High → Medium → Low...
- Space + Crossover → Low
- Space + Pound → Medium
- Space + Hesitation → High
- Space + Behind-the-back has no assigned stance destination yet

Completed foundation:

- Mid-dribble stance changes — `BT-ST-01`
- Quick Space stance flow — `BT-ST-01`
- Space + dribble stance modifiers — `BT-ST-01`
- Prototype Low / Medium / High visual readability — `BT-ST-01`

Completed behavior experiment:

- Stance-dependent pound bounce profile — `BT-ST-02`

Completed behavior pass:

- Complete prototype stance behavior across crossover, hesitation, and behind-the-back — `BT-ST-03`

Deferred until interacting systems exist:

- Stance effects on rhythm
- Stance effects on exposure
- Stance-dependent follow-up rules

These remain future tuning/integration work rather than blockers for the prototype stance milestone.

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
- Blend or redirect visual ball motion when a new valid action is accepted before the previous motion visually finishes; do not solve this with gameplay input locks
- Exaggerate hesitation presentation with body/shoulder selling and potentially a slight lateral weight shift or step while preserving rhythm authority
- Expand expressive and intentionally outrageous finishes

---

## Maintenance / Technical Backlog

These items are worth revisiting, but they are not active tasks:

- `BounceTheorySceneBuilder.cs` has accumulated legacy `UpgradeToChunkX` and validation behavior. As the prototype matures, separate scene construction, upgrade/setup logic, and validation rather than continuing to grow one editor file.
- Audit Unity template leftovers such as `SampleScene`, tutorial/readme assets, and the default input-action asset only after confirming they are unused.
- Audit unnecessary Unity packages later; do not remove packages merely for cleanup while core prototype systems are still being proven.
