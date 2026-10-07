# Bounce Theory — Implementation Status

**Project status:** Active prototype  
**Current branch:** `milestone/defender-interaction`  
**Current task:** `BT-DF-02/03 — Reach, Steal, Overcommit + Beaten`  
**Task status:** `IN PROGRESS`  
**Design source of truth:** `Docs/Game Design.md`  
**Project instructions:** `AGENTS.md`  
**Last updated:** 2026-10-07

> This branch is a parallel implementation branch created while Codex reviews BT-DF-01 on `milestone/defender`. It intentionally builds on the BT-DF-01 API/shape that existed at branch creation. Before final integration, absorb any material Codex fixes from the parent defender branch rather than overwriting them.

---

## Accepted Foundation

- Core rhythm/contact foundation: COMPLETE.
- Core dribble vocabulary: COMPLETE.
- Stance milestone: PROTOTYPE COMPLETE.
- Dribble-state milestone: PROTOTYPE COMPLETE.
- BT-DF-01 provides Centered / LeaningLeft / LeaningRight / Recovering plus timing-quality-dependent recovery.
- Codex executable verification for BT-DF-01 passed on `milestone/defender` on 2026-10-07; BT-DF-01 is awaiting user Play Mode acceptance.
- The verified BT-DF-01 accepted-action timing fix and serialized scene setup have been merged into this branch.

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

- [ ] Secure action does not trigger Reaching.
- [ ] Recovering-quality action triggers Reaching with Contested opportunity.
- [ ] Exposed-quality action triggers Reaching with Vulnerable opportunity.
- [ ] Vulnerable reach resolves to DefenderSteal at floor contact.
- [ ] Contested reach without queued follow-up resolves to DefenderSteal.
- [ ] Contested reach with a valid queued follow-up misses and creates Overcommitted.
- [ ] Queued input is preserved when the reach misses.
- [ ] Secure crossover / hesitation / BTB during Overcommitted creates Beaten.
- [ ] Pound does not create Beaten.
- [ ] Overcommitted times out to Centered.
- [ ] Beaten times out to Centered.
- [ ] Restart clears reach opportunity/outcome/timers and returns Centered.
- [ ] Defender root remains stationary.
- [ ] Existing BT-DF-01 lean/recovery behavior still works.
- [ ] Existing BT-DS-01, stance, rhythm, and contact behavior remains intact.

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

- [ ] Unity editor project compiles.
- [ ] Run the existing BT-DF-01 scene upgrade if needed.
- [ ] Run the new BT-DF-02/03 defender interaction validator.
- [ ] Run BT-DF-01 validator.
- [ ] Run BT-DS-01 complete dribble-state validator.
- [ ] Run representative stance/rhythm/contact regressions.
- [ ] Confirm no unrelated changes.

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
**BT-DF-02/03 executable verification:** NOT YET CLAIMED  
**Task status remains:** IN PROGRESS

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

### Next Codex pass

Work on `milestone/defender-interaction`.

Treat this as one coherent feature review, not two micro-reviews.

Codex should:

1. synchronize the branch and inspect the actual diff,
2. run `Bounce Theory/Upgrade Prototype Scene To BT-DF-02-03 Defender Interaction`,
3. compile the Unity editor project,
4. run `Bounce Theory/Validate BT-DF-02-03 Defender Interaction`,
5. rerun `Bounce Theory/Validate BT-DF-01 Defender Lean Recovery`,
6. rerun `Bounce Theory/Validate BT-DS-01 Complete Dribble State`,
7. run representative stance/rhythm/contact regressions,
8. fix clear material in-scope issues,
9. rerun verification and inspect final status/diff.

If executable verification succeeds, change BT-DF-02/03 to `AWAITING PLAYTEST` and stop.

Do not mark COMPLETE without user Play Mode acceptance.
