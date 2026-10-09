# Bounce Theory — Implementation Status

**Project status:** Active prototype  
**Current branch:** `milestone/defender-interaction`  
**Current task:** `BT-DF-02/03 — Reach, Steal, Overcommit + Beaten`  
**Task status:** `AWAITING PLAYTEST`
**Design source of truth:** `Docs/Game Design.md`  
**Project instructions:** `AGENTS.md`  
**Last updated:** 2026-10-09

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
**Status:** IN PROGRESS

### Goal

Complete the next meaningful defender-interaction layer in one coherent chunk:

`Lean / Recover → Reach → Steal or Overcommit → Beaten`

The defender should now punish poor offensive timing, miss when the offense protects a contested dribble with a valid queued response, and become exploitable after that failed reach.

### Unknowns Pass — Resolved For This Prototype

- Keep defender behavior deterministic; do not introduce random steal rolls.
- Use existing `BallControlQuality` as the first steal-opportunity input.
- `Secure` offense is protected from automatic reach attempts.
- `Recovering` offense creates a **contested** reach opportunity.
- `Exposed` offense creates a **vulnerable** reach opportunity.
- Vulnerable reach at floor contact results in a prototype steal.
- Contested reach results in a steal unless the offense already has a valid queued follow-up accepted before floor contact.
- Escaping a contested reach with a queued follow-up makes the defender `Overcommitted`.
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

Recovering offense
→ Reaching / Contested
→ queued valid follow-up already accepted?
    yes → reach misses → Overcommitted
            ↓ Secure crossover / hesitation / BTB
          Beaten
    no  → DefenderSteal → possession Ended

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
- [x] Contested reach without queued follow-up resolves to DefenderSteal.
- [x] Contested reach with a valid queued follow-up misses and creates Overcommitted.
- [x] Queued input is preserved when the reach misses.
- [x] Secure crossover / hesitation / BTB during Overcommitted creates Beaten.
- [x] Pound does not create Beaten.
- [x] Overcommitted times out to Centered.
- [x] Beaten times out to Centered.
- [x] Restart clears reach opportunity/outcome/timers and returns Centered.
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

## GitHub Implementation Handoff

**ChatGPT first implementation:** COMPLETE FOR HANDOFF  
**BT-DF-01 verified foundation:** MERGED INTO THIS BRANCH  
**BT-DF-02/03 executable verification:** PASSED 2026-10-09
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

- The combined defender interaction scene upgrade completed successfully.
- The Unity editor C# project compiled with zero errors; only the existing validator API-obsolescence warnings remain.
- BT-DF-02/03, BT-DF-01, and BT-DS-01 validators passed.
- Representative BT-ST-03 stance, Chunk 3 rhythm, and Chunk 4.75 target-contact regressions passed.
- Fresh-context review found no clear material in-scope gameplay defect requiring a code change.
- The serialized scene now contains the defender reach, overcommit, and beaten tuning values.

Next step: user Play Mode verification of readability and feel.

Do not mark COMPLETE without user Play Mode acceptance.
