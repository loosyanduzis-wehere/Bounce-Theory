# Bounce Theory — Implementation Status

**Project status:** Active prototype  
**Current branch:** `milestone/defender-interaction`  
**Current task:** `BT-DF-02/03 — Reach, Steal, Overcommit + Beaten`  
**Task status:** `AWAITING PLAYTEST`
**Design source of truth:** `Docs/Game Design.md`  
**Project instructions:** `AGENTS.md`  
**Last updated:** 2026-10-11

> This branch is the active defender-interaction branch. BT-DF-01 passed Codex verification and user Play Mode acceptance on 2026-10-07, and that accepted checkpoint is merged into this branch.

---

## Accepted Foundation

- Core rhythm/contact foundation: COMPLETE.
- Core dribble vocabulary: COMPLETE.
- Stance milestone: PROTOTYPE COMPLETE.
- Dribble-state milestone: PROTOTYPE COMPLETE.
- BT-DF-01 provides Centered / LeaningLeft / LeaningRight / Recovering plus timing-quality-dependent recovery.
- BT-DF-01 passed Codex executable verification and user Play Mode verification on **2026-10-07**.
- The verified/user-accepted BT-DF-01 accepted-action timing fix and serialized scene setup are merged into this branch.

---

## Active Task — BT-DF-02/03

**Title:** Reach, Steal Opportunity, Overcommit + Beaten  
**Status:** AWAITING PLAYTEST

### Goal

Complete the next meaningful defender-interaction layer in one coherent chunk:

`Lean / Recover → Reach → Steal or Overcommit → Beaten`

The defender should now punish poor offensive timing, miss when the offense protects a contested dribble with a valid queued response, and become exploitable after that failed reach.

### Unknowns Pass — Resolved For This Prototype

- Keep defender behavior deterministic; do not introduce random steal rolls.
- Use existing `BallControlQuality` as the first steal-opportunity input.
- `Secure` offense is protected from automatic reach attempts.
- `Recovering` offense creates a **contested** reach opportunity; the reach itself does not automatically steal.
- `Exposed` offense creates a **vulnerable** reach opportunity.
- Vulnerable reach at floor contact results in a prototype steal.
- Contested reach **without** a valid queued follow-up misses into short defender recovery; Early/Late timing alone must not end possession.
- Escaping a contested reach with a queued follow-up makes the defender `Overcommitted`.
- A prototype **1.15-second reach cooldown** prevents immediate repeated reaches; restart clears it.
- While Overcommitted, a **Secure non-pound** action (crossover, hesitation, or behind-the-back) is treated as a successful counter and moves the defender to `Beaten`.
- Pound may maintain possession but does not count as the prototype “counter” that beats an overcommit.
- Overcommitted and Beaten are timed readable windows, not final animation or AI.
- Prototype steal ends the possession through the existing possession lifecycle with a new `DefenderSteal` reason.
- Restart clears all defender reach/overcommit/beaten state.
- Defender root remains stationary; visual reactions remain on the defender visual pivot.
- Do not add randomness, locomotion, scoring, finishes, shot contests, or final AI.

### State / Outcome Additions

#### Defender states

- `Reaching`
- `Overcommitted`
- `Beaten`

#### Steal opportunity

- `Protected`
- `Contested`
- `Vulnerable`

#### Reach outcome

- `None`
- `Missed`
- `Stolen`

### Prototype Flow

```text
Secure offense
→ no reach
→ normal lean/recovery behavior

Recovering offense (Early/Late timing)
→ Reaching / Contested
→ queued valid follow-up already accepted?
    yes → reach misses → Overcommitted
            ↓ Secure crossover / hesitation / BTB
          Beaten
    no  → reach misses → Recovering (offense retains possession)
→ reach cooldown prevents immediate repeat attempt

Exposed offense
→ Reaching / Vulnerable
→ DefenderSteal → possession Ended

Overcommitted
→ timer expires → Centered
→ or Secure non-pound counter → Beaten

Beaten
→ readable timed opening
→ Centered
```

### Scope

- Extend `PrototypeDefenderController`.
- Add deterministic steal-opportunity classification.
- Add readable Reaching / Overcommitted / Beaten visual states.
- Add prototype successful-steal possession ending.
- Reuse the existing queued-input system as the contested-reach escape condition.
- Add counters/debug state for reach attempts, steals, overcommits, beaten results.
- Add deterministic validator covering the complete BT-DF-02/03 loop.
- Preserve BT-DF-01 behavior and all accepted offense/rhythm/stance behavior.

### Success Criteria

- [x] Secure action does not trigger Reaching.
- [x] Recovering-quality action triggers Reaching with Contested opportunity.
- [x] Exposed-quality action triggers Reaching with Vulnerable opportunity.
- [x] Vulnerable reach resolves to DefenderSteal at floor contact.
- [x] Contested reach without queued follow-up misses into recovery without stealing.
- [x] Contested reach with a valid queued follow-up misses and creates Overcommitted.
- [x] Queued input is preserved when the reach misses.
- [x] Secure crossover / hesitation / BTB during Overcommitted creates Beaten.
- [x] Pound does not create Beaten.
- [x] Overcommitted times out to Centered.
- [x] Beaten times out to Centered.
- [x] Restart clears reach opportunity/outcome/timers/cooldown and returns Centered.
- [x] Defender root remains stationary.
- [x] Existing BT-DF-01 lean/recovery behavior still works.
- [x] Existing BT-DS-01, stance, rhythm, and contact behavior remains intact.

### Out of Scope

Do not add:

- random steal percentages,
- difficulty-specific defender intelligence,
- defender locomotion/pathfinding,
- shot contest,
- finish triggers,
- scoring,
- final stance-based steal math,
- animation-driven authority,
- final foul/travel/gather rules,
- polished character animation.

### Verification Plan

Automated / executable after integration with latest BT-DF-01 fixes:

- [x] Unity editor project compiles.
- [x] Run the existing BT-DF-01 scene upgrade if needed.
- [x] Run the new BT-DF-02/03 defender interaction validator.
- [x] Run BT-DF-01 validator.
- [x] Run BT-DS-01 complete dribble-state validator.
- [x] Run representative stance/rhythm/contact regressions.
- [x] Confirm no unrelated changes.

Manual after Codex:

- [ ] Reach is visually readable.
- [ ] Poor timing visibly creates defensive danger.
- [ ] Queuing a response during a contested reach feels like escaping pressure.
- [ ] Failed reach → Overcommitted reads clearly.
- [ ] Clean counter → Beaten reads clearly.
- [ ] Steal ending/restart feels coherent enough for prototype use.
- [ ] Existing dribble responsiveness remains intact.

Do not mark COMPLETE without user Play Mode acceptance.


---

## Playtest Balance Correction — 2026-10-11

**User feedback:** The defender was stealing so often that the basic dribble loop was barely playable. This is a failed balance playtest; BT-DF-02/03 is not accepted.

**Root cause:** Early/Late judgments produce `Recovering` control quality, which previously triggered an immediate steal at floor contact unless a valid follow-up had already been queued. That punishes an ordinary small rhythm miss as harshly as an exposed/broken-rhythm mistake.

**Focused code changes on this branch:**

- Unqueued **Contested** reach (Early/Late) now misses and moves into defender recovery; possession stays active.
- Queued **Contested** reach still misses into **Overcommitted**, allowing a Secure non-pound counter to create **Beaten**.
- **Vulnerable** reach (BrokenRhythm/Exposed) can still steal; deterministic punishment remains for major errors.
- Reach attempts now have a tunable **1.15 s cooldown** to avoid constant repeated interruptions; restart resets that cooldown.
- The BT-DF-02/03 validator now checks the updated missed-reach, cooldown, queued-escape, and restart semantics.

**Verification passed on 2026-10-11:** The Unity editor project compiled, the updated BT-DF-02/03 validator passed, and BT-DF-01, BT-DS-01, stance, rhythm, and target-contact regressions passed. The scene upgrade serialized the 1.15-second cooldown. A new user Play Mode assessment is still required to determine whether ordinary dribbling is practical. Do not mark COMPLETE yet. No changes to `Docs/Game Design.md` were made by this balance pass.

---

## GitHub Implementation Handoff

**ChatGPT first implementation:** COMPLETE FOR HANDOFF  
**BT-DF-01 verified foundation:** MERGED INTO THIS BRANCH  
**BT-DF-02/03 balance-fix executable verification:** PASSED 2026-10-11
**Task status remains:** AWAITING PLAYTEST

Implemented in the combined chunk:

- Reaching / Overcommitted / Beaten defender states,
- Protected / Contested / Vulnerable steal-opportunity classification,
- deterministic DefenderSteal possession ending,
- queued-response escape for Contested reach,
- failed reach → Overcommitted,
- Secure non-pound counter → Beaten,
- timed Overcommitted / Beaten windows,
- restart cleanup,
- expanded debug state/counters,
- combined scene-upgrade entry point,
- full BT-DF-02/03 validator,
- Codex BT-DF-01 accepted-command timing fix carried forward,
- Codex BT-DF-01 scene serialization carried forward.

### Codex verification handoff

- The updated defender interaction scene upgrade completed successfully and serialized the 1.15-second reach cooldown.
- The Unity editor C# project compiled with zero errors; only the existing validator API-obsolescence warnings remain.
- The revised BT-DF-02/03 validator passed: Early/Late Contested reaches miss without stealing, Exposed Vulnerable reaches can steal, queued responses still create Overcommitted and Beaten, and restart clears the cooldown and defender state.
- BT-DF-01 and BT-DS-01 validators passed.
- Representative BT-ST-03 stance, Chunk 3 rhythm, and Chunk 4.75 target-contact regressions passed.
- Fresh-context review found no clear material in-scope gameplay defect requiring a code change.

Next step: user Play Mode verification of ordinary dribble practicality and defender readability/feel.

Do not mark COMPLETE without user Play Mode acceptance.
