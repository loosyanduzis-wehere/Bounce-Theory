# Bounce Theory — Open Questions

> Recognized unresolved decisions belong here until they are intentionally resolved.
> Do not silently turn these into implementation assumptions.
> When resolved, move the decision to `Docs/Game Design.md` or `Docs/DECISIONS.md` as appropriate and remove it from this file.

## Active Gameplay Questions

- **Stance modifier forgiveness:** the current `0.18 s` Space-modifier grace works but remains provisional. Revisit alongside final input feel and animation rather than treating it as a locked value.
- **Final stance expression:** exact Low / Medium / High trajectory, posture, speed, and animation differences remain tuning questions. Current prototype profiles prove the state architecture, not final feel.
- **Stance and rhythm:** decide whether stance eventually changes rhythmic intervals or timing forgiveness, and if so how, without creating separate stance-specific clocks.
- **Stance and exposure:** define how Low / Medium / High affect ball exposure and defender steal opportunities.
- **Stance-dependent follow-ups:** define which follow-up actions become more or less favorable or physically legal from each stance.
- **Space + Behind-the-back:** no stance destination is assigned. Leave unassigned until it has a clear gameplay purpose.
- **Continuation / grace after a bounce:** decide how much late continuation forgiveness Easy mode should provide after control returns without weakening rhythm scoring.
- **Bounce resolution:** exact rules for gather, mishandle, exposed ball, travel, double-dribble, and legal intentional pauses remain unresolved.
- **Defender model:** exact defender state transitions, reaction rules, and AI logic remain open for the defender prototype milestone.

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
