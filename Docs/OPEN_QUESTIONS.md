# Bounce Theory — Open Questions

> Recognized unresolved decisions belong here until they are intentionally resolved.
> Do not silently turn these into implementation assumptions.
> When resolved, move the decision to `Docs/Game Design.md` or `Docs/DECISIONS.md` as appropriate and remove it from this file.

## Active Gameplay Questions

- **Stance modifier forgiveness:** the current `0.18 s` Space-modifier grace works but remains provisional. Revisit alongside final input feel and animation rather than treating it as a locked value.
- **Final stance expression:** exact Low / Medium / High trajectory, posture, speed, and animation differences remain tuning questions. Current prototype profiles prove the state architecture, not final feel.
- **Stance and rhythm:** decide whether stance eventually changes rhythmic intervals or timing forgiveness, and if so how, without creating separate stance-specific clocks.
- **Stance and exposure:** define how Low / Medium / High affect ball exposure and defender steal opportunities.
- **Stance-dependent follow-ups:** define which follow-up actions become more or less favorable or physically legal from each stance. The prototype now preserves stance and follow-up relation context but does not impose final restrictions.
- **Final follow-up legality/advantage:** decide whether specific previous-action → next-action relationships should become favored, redirected, impossible, or merely riskier once defender/exposure systems exist. Do not convert the current relation labels into a canned combo table by default.
- **Space + Behind-the-back:** no stance destination is assigned. Leave unassigned until it has a clear gameplay purpose.
- **Continuation / grace after a bounce:** decide how much late continuation forgiveness Easy mode should provide after control returns without weakening rhythm scoring.
- **Bounce resolution:** exact final rules for gather, mishandle, exposed ball, travel, double-dribble, and legal intentional pauses remain unresolved. The prototype may end a possession when a new dribble is attempted after the current finite rhythmic continuation window is exhausted; this is a testing rule, not the final basketball interpretation.
- **Possession-end causes:** beyond the provisional expired-continuation case, decide which future events end a possession (steal, mishandle, gather, violation, made/missed finish, etc.) and which merely change state.
- **Final defender model:** Centered / lean / recovery / reach / overcommit / beaten now have provisional prototype rules. Final anticipation/intelligence, random-vs-deterministic steal tuning, stance sensitivity, foul behavior, exact reach windows, recovery tuning, and how Beaten feeds finishes remain open.
- **Final finish model:** BT-FN-01 uses a two-stage commit/release prototype with Q/E/F, whole-beat targets, and defender-state window tiers. Final per-finish timing curves, spatial contest math, mid-bounce finish queueing, stance effects, difficulty effects, make/miss semantics, ball/rim presentation, and exact animation timing remain open.

## Rhythm / Tuning Questions

- Exact BPM and long-term reference-groove behavior.
- Exact mapping between actions and rhythmic intervals/subdivisions.
- Exact final timing windows by difficulty.
- Calibration / latency compensation approach.
- Exact input queue, redirect, cancel, and physically-impossible-request rules.

## Presentation Questions

- Final character animation style and how animation sells stance differences.
- How much overlapping action motion remains readable at different tempos.
- Camera behavior during extreme finishes.
