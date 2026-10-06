# Bounce Theory — Completed Tasks

> Concise human-readable implementation history.
> Git remains the detailed record of exact code changes.

## BT-BC-05 — Rhythm-Driven Floor Contact

**Completed:** 2026-10-04  
**Legacy designation:** Chunk 4.75  
**Milestone:** Core Rhythm + Pound Dribble Foundation

### Result

Accepted dribble input now produces a target floor-contact time anchored to the global rhythm grid. The visible/physical bounce can reshape to meet the accepted rhythmic target rather than waiting for a fixed visual bounce cycle to finish.

Key behavior includes:

- Keypress time is treated as command time rather than impact time.
- Accepted input creates a `ContactTimingPlan`.
- Contact targets remain anchored to the global grid.
- Faster intervals can use compressed bounce motion.
- Pending actions can execute during the earliest physically feasible return phase.
- Actual floor contact is measured independently from keypress time.
- Impact audio fires from the measured floor-contact event.
- Physically impossible targets are explicitly reported as `Unreachable` instead of silently shifted.

### Important Files

- `Assets/BounceTheory/Scripts/RhythmClock.cs`
- `Assets/BounceTheory/Scripts/PoundDribbleController.cs`
- `Assets/BounceTheory/Editor/BounceTheorySceneBuilder.cs`
- `Assets/Scenes/BounceTheoryPrototype.unity`

### Verification

Automated validation reported:

- normal high-bounce contact error of approximately `+0.8 ms`,
- compressed half-beat contact error of approximately `+0.9 ms`,
- previous rhythm/input regression validators passing,
- no compiler errors or validation exceptions.

User Play Mode verification was accepted on 2026-10-04.

### Follow-Up

Future ball-control work should build on this contact-timing architecture rather than reintroducing animation-gated input or fixed-cycle authority.

---


## BT-BC-06 — Basic Crossover Foundation

**Completed:** 2026-10-04  
**Milestone:** Ball Control Language

### Result

The prototype now supports a basic rhythm-driven crossover in both directions while preserving the existing shared DSP/contact-timing architecture.

Key behavior includes:

- **D** crosses from left-hand control toward the right hand.
- **Left Arrow** crosses from right-hand control toward the left hand.
- Crossover uses the existing rhythm judgment and global contact-planning path rather than a separate timing system.
- The ball follows a readable lateral path across the body.
- Logical hand ownership transfers to the target hand at floor contact.
- A valid follow-up action can be accepted before the crossover's visual return has fully finished.
- Existing pound-dribble behavior remains intact.

### Important Files

- `Assets/BounceTheory/Scripts/PoundDribbleController.cs`
- `Assets/BounceTheory/Editor/BounceTheorySceneBuilder.cs`
- `Docs/IMPLEMENTATION_STATUS.md`

### Verification

Automated crossover validation was added for both left-to-right and right-to-left transfer while preserving prior rhythm/contact validators.

User Play Mode verification was accepted on 2026-10-04. Both crossover directions and existing pound dribbles worked as intended.

### Follow-Up

The accepted next action may begin while the crossover is still visually traveling toward the receiving hand. This is intentional gameplay behavior, not an input bug. Future animation/blending should visually reconcile the overlap without reintroducing animation-gated input.

---


## BT-BC-07 — Basic Hesitation Foundation

**Completed:** 2026-10-04  
**Milestone:** Ball Control Language

### Result

The prototype now supports a basic hesitation action on either hand while preserving the existing rhythm/game-state architecture.

Key behavior includes:

- **A** performs a left-hand hesitation while the left hand owns the ball.
- **Right Arrow** performs a right-hand hesitation while the right hand owns the ball.
- Hesitation preserves same-hand ownership.
- Hesitation has a distinct hold/lift visual behavior rather than reading as a normal pound dribble.
- Hesitation is treated as an intentional action, not as silence or automatic failure.
- Pound and crossover controls remain available within the shared rhythm/action system.

### Important Files

- `Assets/BounceTheory/Scripts/PoundDribbleController.cs`
- `Assets/BounceTheory/Editor/BounceTheorySceneBuilder.cs`
- `Docs/IMPLEMENTATION_STATUS.md`

### Verification

Automated hesitation validation was added while preserving previous pound, crossover, rhythm, and contact validation.

User Play Mode verification was accepted on 2026-10-04. Left- and right-hand hesitation worked as intended, and existing ball-control actions remained functional.

### Follow-Up

The hesitation should likely become more exaggerated during the animation pass. Possible presentation work includes a stronger body/shoulder sell and a slight lateral player weight shift or step. This should remain visual/readability work and should not change the underlying rhythm authority unless later playtesting identifies a gameplay reason to do so.

---

## BT-BC-08 — Basic Behind-the-Back Foundation

**Completed:** 2026-10-04  
**Milestone:** Ball Control Language

### Result

The prototype now supports a basic behind-the-back dribble in both directions while preserving the existing shared rhythm/contact architecture.

Key behavior includes:

- **S** performs left-to-right behind-the-back when the left hand owns the ball.
- **Down Arrow** performs right-to-left behind-the-back when the right hand owns the ball.
- Logical ownership transfers to the opposite hand when the move resolves.
- The move uses a distinct depth/wrap path rather than reading as a normal crossover.
- Behind-the-back uses the existing `DribbleAction`, DSP judgment, target-contact planning, and global rhythm path.
- Pound, crossover, and hesitation behavior remain intact.

### Verification

Automated validation was added for both behind-the-back directions while preserving previous pound, crossover, hesitation, rhythm, and contact checks.

User Play Mode verification was accepted on 2026-10-04.

### Follow-Up

The initial core dribble vocabulary is now working: pound, crossover, hesitation, and behind-the-back. Future ball-control work can deepen branching, legality, and animation without replacing this shared rhythm architecture.

---
## BT-ST-01 — Stance State + Space Control Foundation

**Completed:** 2026-10-05  
**Milestone:** Stance

### Result

The prototype now has an explicit three-state stance language that can operate during active dribble play without making animation completion authoritative.

Key behavior includes:

- Stance states are **Low**, **Medium**, and **High**.
- **Medium** is the default/home stance.
- Plain Space follows:
  `Medium → Low → Medium → High → Medium → Low...`
- Direct stance modifiers work while preserving the basketball action:
  - **Space + Crossover → Low**
  - **Space + Pound → Medium**
  - **Space + Hesitation → High**
- **Space + Behind-the-back** remains intentionally unassigned.
- A short prototype stance-modifier grace window allows a dribble input shortly after Space is released without changing the dribble input timestamp or rhythm judgment.
- Low, Medium, and High have distinct prototype silhouettes for Play Mode readability.
- Stance can change while a dribble visual is still active.

### Verification

The earlier stance revision compiled and passed targeted stance/dribble validation. User Play Mode testing confirmed that the stance flow and direct modifier controls work.

The modifier remains intentionally skillful. Exact modifier-window tuning and final stance feel are **provisional** and should be revisited alongside animation, input feel, and presentation rather than treated as locked design values.

### Follow-Up

The next stance work should begin making stance mechanically meaningful one variable at a time while preserving the existing rhythm/contact architecture.

---

## BT-ST-02 — Stance-Dependent Pound Bounce Height

**Completed:** 2026-10-05  
**Milestone:** Stance

### Result

Pound dribbles now capture the stance that was active when the action was accepted and use that stance to select a prototype vertical profile while preserving the shared rhythm/contact architecture.

Key behavior includes:

- **Low** pound uses a lower/tighter prototype profile.
- **Medium** preserves the previous pound baseline.
- **High** pound uses a higher prototype profile.
- The accepted pound keeps its original stance profile even if stance changes later during the active motion.
- Low, Medium, and High share the same input DSP timestamp, rhythm judgment, and target floor-contact clock for equivalent input.
- Crossover, hesitation, and behind-the-back behavior remain unchanged.

### Verification

Targeted automated validation passed for ordered Low < Medium < High midpoint profiles, unchanged Medium baseline behavior, preserved contact timing, and representative dribble regressions.

User Play Mode testing confirmed that the stance-dependent pound difference exists, though the current prototype distinction is subtle.

### Follow-Up

Do not over-tune the numeric pound-height values yet. Final perceptual differences should be revisited during the animation/presentation pass, where body posture, hand position, anticipation, and follow-through will materially affect how stance reads.

---

## BT-ST-03 — Complete Prototype Stance Behavior Pass

**Completed:** 2026-10-06  
**Milestone:** Stance

### Result

Low, Medium, and High now produce distinct prototype behavior across the full current dribble vocabulary while preserving the shared rhythm/contact architecture.

Implemented and accepted:

- stance-aware Pound profiles,
- stance-aware Crossover profiles,
- stance-aware Hesitation profiles,
- stance-aware Behind-the-back profiles,
- Medium remains the neutral/baseline behavior,
- each action captures stance at acceptance,
- queued actions preserve their accepted stance,
- later stance changes do not rewrite active-action timing/profile,
- Space + Behind-the-back remains intentionally unassigned,
- prototype stance visuals remain placeholders for later animation/presentation work.

### Verification

Automated stance/dribble validation passed before Play Mode handoff.

User Play Mode verification was accepted on **2026-10-06**.

Current numeric stance differences and the `0.18 s` modifier grace remain tunable prototype values and should be revisited during animation/input-feel work rather than treated as final design constants.

---

<!-- Append newly confirmed COMPLETE tasks below this line. -->
