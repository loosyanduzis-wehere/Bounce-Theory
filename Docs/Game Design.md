# Bounce Theory — Source of Truth v2
**Last updated:** 2026-10-04  
**Status:** Active design source of truth  
**Replaces:** The earlier conventional-movement prototype plan

---

## 1. Core Identity

**Bounce Theory is a rhythm-driven basketball duel where the basketball itself becomes the music.**

The game is not primarily about freely moving a character around a court with conventional third-person controls. The player performs basketball actions through timed dribble inputs, stance changes, reads of the defender, and rhythm.

The player uses rhythm to manipulate the defender, create an opening, and then cash out that advantage through a shot, stepback, drive, dunk, or other finish.

The game should feel expressive, musical, stylish, readable, and increasingly outrageous as the player progresses.

---

## 2. What Changed From the Earlier Prototype

The previous prototype started moving toward a traditional basketball game:

- Free player movement
- Camera-relative locomotion
- CharacterController movement
- Conventional court navigation
- Basketball following the player

That prototype is **not bad work** and should be preserved as a foundation for a different basketball project.

However, it is no longer the correct foundation for Bounce Theory.

### Restart decision

Bounce Theory should restart in a clean Unity project rather than continue layering the new design onto the old movement architecture.

The existing prototype should be left intact and treated as a separate project/reference.

---

## 3. Player View and Presentation

The intended camera is generally **behind the offensive player**, creating an intimate one-on-one view of the defender and basket.

The scene should already feel alive before the player performs a complex move:

- The player settles into a basketball stance.
- The defender moves and reacts naturally.
- The ball bounce establishes rhythm.
- Body motion, sneaker movement, ball contact, and camera feel reinforce the groove.

The game should not initially depend on realistic final character animation. Early prototypes can use simple geometry and scripted motion to prove the control system first.

---

## 4. The Ball Is the Music

The long-term idea is that the player creates the music through basketball.

Important sounds include:

- Ball hitting the floor
- Ball contacting the hand
- Sneaker squeaks
- Body movement
- Pauses and hesitations
- Different dribble heights and strengths
- Move transitions

A skilled possession should sound musical even without a traditional song playing.

### Tutorial progression

Early in the game, a simple backing beat or musical rhythm can teach the player how to feel timing.

Over time:

1. Strong external beat teaches the rhythm.
2. The backing rhythm becomes less dominant.
3. The player begins relying more on the ball and body sounds.
4. Eventually, the dribble performance itself becomes the music.

The intended progression is:

> Learn the rhythm from the music, then become the music through the dribble.


### Global pulse and player-created percussion

Bounce Theory can use a steady background soundtrack or reference groove to give the player a clear sense of musical time. A prototype reference around **130 BPM** is useful because it matches the cadence explored during reference analysis, but the exact BPM is a tunable design value rather than a locked constant.

The background track is **not** a sequence the player must copy. Its job is to establish the pulse.

The basketball is the expressive percussion layer. Ball-floor contact, hand contact, sneaker sounds, pauses, hesitations, and move transitions form the player's own rhythm on top of that pulse.

A useful principle is:

> **The soundtrack tells you where time is. The basketball decides what happens inside it.**

### Rhythmic vocabulary

Basketball actions do not need separate BPM values. They can occupy different amounts of one shared pulse.

Useful prototype interval families include:

- **0.5 beat** — quick action / double-time spacing
- **0.75 beat** — syncopated spacing
- **1.0 beat** — regular pound reference
- **1.5 beats** — hesitation / deliberate space
- **2.0 beats** — long pause, gather, or dramatic space

These are a **prototype rhythmic vocabulary**, not permanent assignments to specific moves.

A crossover is not automatically required to equal 0.5 beat, and a hesitation is not automatically required to equal 1.5 beats. The final move-to-rhythm relationships should be discovered through playtesting.

### Independence of actions

Each action is **rhythmically independent but physically contextual**.

The previous action can determine:

- which hand owns the ball,
- the ball's current position and phase,
- stance,
- exposure,
- defender state,
- and which follow-up actions are physically legal.

However, the previous action should not force a predetermined rhythmic combo. The player chooses when and how to act next within the shared rhythmic structure.

Bounce Theory should avoid becoming a fixed-pattern rhythm game where one move automatically dictates the timing or identity of the next move.

---

## 5. Core Keyboard Control Language

The keyboard mirrors the player's two hands.

| Left-hand input | Action | Right-hand input |
|---|---|---|
| **W** | Downward / pound dribble | **Up Arrow** |
| **D** | Crossover toward opposite hand | **Left Arrow** |
| **A** | Hesitation | **Right Arrow** |
| **S** | Behind-the-back | **Down Arrow** |
| **Space** | Change stance | **Space** |

The exact controller mapping remains open and should eventually mirror the same conceptual two-hand language.

### Important principle

Inputs are not merely buttons that play canned moves.

The result of an input can depend on:

- Which hand currently controls the ball
- Current stance
- Current ball phase
- Timing relative to the active rhythm
- Previous move
- Defender state
- Follow-up input

This allows moves to branch while the ball is already in motion.

---

## 6. Dribble State and Timing

The ball should exist in meaningful states such as:

- Left hand
- Right hand
- Downward bounce
- Crossing
- Behind-the-back transition
- Hesitation
- Catch/recovery window
- Exposed
- Broken rhythm

The player can begin one action and alter what it becomes during the bounce.

Example:

**Left hand → D starts crossover → ball enters transition → follow-up timing and stance determine how the sequence resolves.**

The game should judge timing without relying forever on a traditional rhythm-game note highway.

Possible timing results:

- Perfect
- Good
- Early
- Late
- Broken Rhythm

These labels are provisional. The important part is their gameplay effect.

### Rhythm consequences

Good timing can produce:

- Cleaner ball motion
- Better sound
- Faster recovery
- Lower exposure
- Stronger deception
- Better defender reaction

Poor timing can produce:

- Awkward ball motion
- Audible rhythm disruption
- Longer recovery
- Greater ball exposure
- Better steal opportunity for the defender

The player should eventually be able to **hear** when the rhythm is right or wrong.


### Bounce resolution and no-input outcomes — design consideration

Once the ball leaves the player's hand, the bounce should eventually need to resolve.

The game should not wait indefinitely for another input. Depending on the move, ball phase, stance, and defender pressure, failing to resolve the bounce may eventually lead to a basketball-specific consequence rather than a generic rhythm-game "miss."

Possible outcomes include:

- **Loose ball / mishandle:** the player fails to meet or control the returning ball.
- **Exposed ball:** poor or missing timing creates a larger steal opportunity for the defender.
- **Gather:** the player regains control and the active dribble sequence ends.
- **Travel / footwork violation:** a gathered state followed by illegal movement may eventually be treated as a violation.
- **Double dribble:** attempting to restart a dribble after a completed gather may eventually be treated as a violation.

This is **not yet an implementation requirement**. It should remain a future design consideration while the basic rhythm and dribble language are being proven.

Important exception: **silence can be intentional.** A hesitation, gather, or other basketball action may deliberately include a pause. Therefore, "no input" should not automatically equal failure. The state machine must eventually distinguish between:

- a bounce that requires a timely resolution,
- a legal pause or hesitation,
- a completed gather,
- and a true loss of control.

A useful design principle is:

> **Every bounce creates an obligation, but not every obligation is another bounce.**

---

## 7. Input Authority and Visual Motion

Bounce Theory should treat **gameplay state and rhythm judgment as authoritative**, not animation completion.

Player input should be detected when it occurs and then evaluated according to:

- rhythm timing,
- current ball state and phase,
- current hand,
- stance,
- physical legality,
- and current defender/gameplay context.

An input may be:

- valid,
- early,
- late,
- broken rhythm,
- queued for a later legal phase,
- redirected into another action,
- or rejected because the requested action is physically impossible from the current state.

However, input should **not** simply be ignored because a previous ball or character animation has not visually finished.

A useful architecture principle is:

> **Input → rhythm judgment → gameplay/ball state → action decision → animation, sound, and ball motion**

rather than:

> **Animation finishes → input becomes legal again**

### Visual duration vs. input timing

Visual motion duration and rhythmic input timing are related, but they do not have to be identical.

The ball may still be visually completing part of a bounce while the game has already accepted, judged, or queued the next valid action.

This is especially important for future:

- crossovers,
- hesitations,
- behind-the-back transitions,
- stance changes,
- reactive follow-ups,
- animation blending,
- and cancel/redirect behavior.

### Readability tuning

Ball height, bounce duration, hand height, floor height, contact hold time, camera framing, and similar visual values remain **prototype readability/tuning choices** unless explicitly locked later.

The game should prefer readable, expressive motion over rigid physical realism during prototyping.

## 9. Stance

**Space changes stance.**

Stance can change during a dribble rather than only between actions.

Stance may affect:

- Bounce height
- Tempo
- Timing windows
- Move speed
- Ball exposure
- Available follow-ups
- Defender reaction
- Animation style

Possible stance categories are still open. Early prototypes may use simple Low / Normal / High states, but these are not locked.

---

## 9. Difficulty and Timing Forgiveness

Planned difficulty modes:

- **Easy**
- **Medium**
- **Hard**
- **Insane**

Difficulty should primarily change rhythm/timing forgiveness while preserving the same core control language and basketball logic.

Early prototyping should use forgiving, Easy-style windows so the player can learn the rhythm system before precision is demanded.

Exact timing windows are not locked. They must be tuned against:

- the selected BPM,
- the spacing of valid rhythmic targets,
- controller/display/audio latency,
- human anticipation rather than simple reaction time,
- and the readability of ball and sound feedback.

A future calibration option may be useful to compensate for individual/system latency.

## 11. Defender Interaction

The defender is not just a collision obstacle.

The defender should have readable basketball states such as:

- Centered
- Leaning left
- Leaning right
- Recovering
- Reaching
- Overcommitted
- Beaten

The offensive player's job is to read those states and exploit them.

Example:

**Defender leans the wrong way → crossover → defender tries to recover → hesitation punishes the recovery → opening created.**

The correct basketball decision is not enough by itself. It must still be executed in rhythm.

Poor timing gives the defender better opportunities to reach, recover, or steal.

---

## 11. Finish System

The player may choose to end a sequence at many points rather than waiting for a predetermined combo ending.

Possible finishes include:

- Shot
- Stepback
- Drive
- Dunk
- Other unlocked finishes

Finishes are primarily **animation-driven timing events**, not full basketball simulations.

### Green window concept

A finish begins with a relatively generous default timing window.

The defender reduces that window according to their position and recovery state.

Examples:

- Defender badly beaten → large green window
- Defender recovering → medium green window
- Heavy contest → small green window

The real basketball skill occurs before the finish: creating the advantage.

The finish timing cashes out that advantage.

---

## 12. Spectacle and Progression

Bounce Theory should eventually become visually outrageous without abandoning its basketball logic.

The basketball logic remains:

**Read defender → manipulate defender → create space → finish.**

The presentation can escalate dramatically.

A drive may begin as a simple layup or dunk and later unlock exaggerated finishes such as:

- 360 dunk
- Between-the-legs finish
- Extreme stepbacks
- 720 backflip dunk
- Other intentionally over-the-top animations

These spectacular finishes should reuse the same underlying opportunity and timing systems rather than requiring increasingly complex physics simulation.

---

## 13. Scoring Philosophy

The game should reward both **efficiency** and **expression**.

### Efficiency

Breaking down a defender quickly and scoring should award a strong bonus.

A short, intelligent sequence should be able to outscore meaningless move spam.

### Expression

Longer possessions can accumulate more move/style value through:

- Variety
- Difficulty
- Rhythm quality
- Defender manipulation
- Clean transitions
- Creative sequences

However, the longer the possession continues, the smaller the quick-breakdown / efficiency bonus becomes.

### Important distinction

Rhythm should **never stop mattering** late in the possession.

What fades is the bonus for efficiently exploiting an opening, not the underlying timing system.

A conceptual score may eventually include:

**Possession Score = Move/Style + Defender Manipulation + Finish Difficulty + Rhythm Quality + Efficiency Bonus**

The exact formula is not locked.

---

## 14. Design Philosophy

Bounce Theory should avoid becoming:

- A traditional basketball movement simulator
- NBA-style free locomotion with rhythm layered on top
- A simple note-matching rhythm game
- A game where flashy move spam is automatically optimal
- A physics-heavy shooting simulator

The central experience is:

> **Use rhythm as a basketball language. Create a dribble performance, manipulate the defender, and turn the opening into a spectacular finish.**

---

## 15. Prototype Philosophy

Do not begin with final character animation.

First prove the interaction using simple placeholders.

The early prototype should answer:

1. Is controlling two virtual hands through mirrored inputs understandable?
2. Does a timed downward dribble feel satisfying?
3. Does a crossover feel different from a pound dribble?
4. Can the player hear good timing versus bad timing?
5. Does changing stance during a bounce create interesting decisions?
6. Can moves naturally chain without feeling like canned combo prompts?
7. Does rhythm failure create meaningful vulnerability?
8. Can the game accept and judge follow-up input before the current visual bounce has completely finished?
9. Does increased ball height/readability improve feel without requiring the core tempo to slow down?

If those answers are yes, then invest in animation, defender AI, spectacle, and progression.

---

## 16. New Prototype Build Order

### Phase 1 — Clean foundation
Create a new minimal Unity project and a simple behind-the-player basketball scene.

No free locomotion system.

### Phase 2 — Two-hand dribble input
Implement:

- W / Up = downward dribble
- D / Left = crossover
- A / Right = hesitation
- S / Down = behind-the-back
- Space = stance change

Begin with only the simplest moves necessary to validate the input language.

### Phase 3 — Rhythm clock and timing
Add:

- Internal rhythm clock
- Timing windows
- Early / late detection
- Visible temporary debug feedback

### Phase 4 — Ball sound as feedback
Make timing visibly and audibly affect the bounce.

The player should begin recognizing good rhythm from sound.

### Phase 5 — Dribble state machine
Allow actions to transition and branch based on:

- Hand
- Ball phase
- Stance
- Timing
- Follow-up input

### Phase 6 — Stance
Allow stance changes during the bounce and test how stance modifies move behavior.

### Phase 7 — Defender prototype
Add readable defender lean, recovery, reach, and overcommit states.

### Phase 8 — Finishes
Add simple shot, stepback, and drive triggers with defender-dependent green windows.

### Phase 9 — Possession scoring
Add efficiency, rhythm, manipulation, style, and finish scoring.

### Phase 10 — Animation and spectacle
Replace placeholder movement with polished animation and begin expanding outrageous finishes.

---

## 17. Current Locked Decisions

These are the strongest current design decisions:

- Bounce Theory is rhythm-first, not free-movement-first.
- The basketball itself eventually becomes the music.
- A background soundtrack/reference groove may establish a steady pulse while the ball creates the expressive percussion.
- Basketball actions share one pulse rather than requiring separate BPMs.
- Actions may occupy different rhythmic intervals such as 0.5, 0.75, 1.0, 1.5, and 2.0 beats; exact move assignments remain tunable.
- Actions are rhythmically independent but physically contextual; no predetermined combo rhythm is required.
- Gameplay input is judged by rhythm and game/ball state rather than gated by animation completion.
- Visual motion duration and rhythmic input timing may overlap; animation should represent gameplay state rather than control input authority.
- Early teaching may use an external beat.
- Keyboard controls mirror the player's two hands.
- W / Up = downward dribble.
- D / Left = crossover.
- A / Right = hesitation.
- S / Down = behind-the-back.
- Space changes stance.
- Stance can change during a dribble.
- Timing affects ball control, sound, vulnerability, and defender opportunity.
- Defender lean and recovery should be readable and exploitable.
- Finishes use timing windows affected by defender position.
- The game may become visually outrageous while keeping readable basketball logic.
- Quick breakdowns earn efficiency value.
- Longer sequences can earn style/move value while the efficiency bonus fades.
- Rhythm remains important throughout the entire possession.
- The current conventional-movement Unity prototype should be preserved separately rather than converted into this version.

---

## 18. Open Questions

These should remain prototype decisions rather than locked assumptions:

- Exact BPM / internal rhythm structure
- Exact background soundtrack/reference-groove BPM and how strongly it remains audible in advanced play
- Exact mapping between basketball actions and rhythmic intervals/subdivisions
- Whether the game judges against a global grid, inferred interval choices, or a hybrid of both
- Player/system timing calibration and latency compensation
- Whether rhythm is fixed, dynamic, or player-created from the beginning of advanced play
- Exact stance categories
- Exact controller mapping
- Exact timing-window sizes
- Exact ball trajectories
- Exact defender AI model
- Exact scoring weights
- Shot-clock length
- Roguelite upgrade structure
- Unlock progression
- Final art direction
- Final animation style
- Camera behavior during extreme finishes
- Exact rules for input queuing, redirects, cancels, and physically impossible requests during active ball motion
- How much visual overlap between one action and the next feels readable at different tempos
- Exact rules for unresolved bounces, gathers, mishandles, travel, and double-dribble states
- How long a legal hesitation or pause may remain active before it becomes a gather or failure state

---

**This document is the active design source of truth for the restarted Bounce Theory project.**
