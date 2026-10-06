# AGENTS.md — Bounce Theory

## Project

Bounce Theory is a rhythm-first basketball game prototype built in Unity.

## Source of Truth

The authoritative game-design reference is:

`Docs/Game Design.md`

Read it when a task depends on game rules, mechanics, design intent, or unresolved design questions.

Do **not** modify the source-of-truth file unless the user explicitly asks for a design-document update.

## Project Documentation

Use these files for their specific roles:

- `Docs/Game Design.md` — authoritative game/design truth.
- `Docs/ROADMAP.md` — planned future milestones and work.
- `Docs/IMPLEMENTATION_STATUS.md` — current implementation state and the one active task.
- `Docs/COMPLETED_TASKS.md` — concise completed implementation history.
- `Docs/DECISIONS.md` — important architecture/design decisions and why they exist.
- `Docs/OPEN_QUESTIONS.md` — recognized unresolved decisions; do not silently guess these.
- `Docs/IMPLEMENTATION_NOTES.md` — current technical realities, workarounds, discoveries, and implementation-specific constraints.
- `Docs/WORKFLOW.md` — project development and Git workflow.

`ROADMAP.md` is planning context, **not an automatic implementation queue**.

Do not implement a roadmap item unless it has been promoted into the Current Task in
`Docs/IMPLEMENTATION_STATUS.md` or the user explicitly requests it.

## Core Architecture Rules

- Gameplay input must be detected and judged by rhythm, game state, and ball state.
- Do not gate valid gameplay input solely on animation completion.
- Visual animation duration and rhythmic input timing may overlap.
- Preserve this conceptual pipeline:

  `keypress -> DSP timestamp -> rhythm judgment -> action decision -> visual execution`

- Inputs during active motion may be valid, mistimed, queued, redirected, or physically impossible depending on state.
- Basketball actions share one global pulse rather than separate independent BPM systems.
- Rhythm values such as `0.5`, `0.75`, `1.0`, `1.5`, and `2.0` beats are prototype vocabulary unless the Source of Truth explicitly locks a mapping.
- Prior actions may affect hand, stance, ball state, defender state, exposure, and legal follow-ups without forcing a predetermined combo rhythm.
- Stance changes may occur during an active dribble sequence.
- Timing quality can affect control, sound, exposure, and defender steal opportunities.
- Prototype tuning values are not permanent design decisions unless explicitly promoted into the Source of Truth.


## Standard AI-Assisted Development Loop

For meaningful feature, architecture, or unfamiliar-system work, follow:

```text
Goal
  ↓
Unknowns Pass
  ↓
targeted research / reference hunt
  ↓
decisions + OPEN_QUESTIONS
  ↓
implementation plan
  ↓
observable success criteria + verification plan
  ↓
ChatGPT implements one coherent feature-sized change through GitHub
  ↓
Git commit / branch / PR = handoff boundary
  ↓
fresh-context Codex review + build/test + scoped repair
  ↓
human/manual verification when required
  ↓
capture learnings in the correct project document
  ↓
next chunk
```

Do not start meaningful implementation before the Unknowns Pass and success/verification criteria are explicit.

### Unknowns Pass

Before implementing a meaningful change, deliberately surface:

- assumptions being treated as facts,
- unresolved design decisions,
- hidden technical constraints,
- interactions with existing systems,
- edge cases and failure states,
- hard-to-reverse choices,
- platform/tooling/performance constraints,
- information currently known only implicitly.

Classify the result:

- **Resolved decision:** record in the Source of Truth or `Docs/DECISIONS.md` when appropriate.
- **Unresolved decision:** record in `Docs/OPEN_QUESTIONS.md`.
- **Technical reality/workaround/discovery:** record in `Docs/IMPLEMENTATION_NOTES.md`.
- **Temporary tuning:** keep as implementation tuning unless explicitly promoted.
- **Irrelevant/temporary noise:** discard.

Research only unknowns that materially affect the current decision. Prefer the actual repository, official documentation, concrete reference implementations, and primary sources over generic summaries.

### Implementation Size Rule

> Small enough to reason about; large enough to justify the context/setup cost.

A coherent feature-sized change may touch multiple files when they all belong to one understandable behavior. Avoid artificial micro-tasks such as one task per enum, field, helper method, or trivial file edit.

## AI Role Split

For normal medium or large coding work:

- **Main ChatGPT conversation:** context-rich architect and first implementer. Clarify the goal, run the Unknowns Pass, resolve or record open questions, define success criteria and verification, then author one coherent feature-sized change through GitHub.
- **Git branch / commit / PR:** handoff boundary between implementation and independent review.
- **Codex:** fresh-context reviewer, fixer, and verifier. Inspect the actual diff and relevant source of truth, build/compile, run the narrowest meaningful executable checks, find correctness/regression/architecture/edge-case problems, directly fix material issues that are clear and in scope, rerun verification, and report remaining manual checks.

Do **not** use Codex as the primary implementer merely because code is involved.

Use Codex as the primary implementer when the task materially benefits from:

- local iterative build/test/debug loops while authoring,
- broad mechanical repository changes,
- migrations,
- refactors whose correctness depends on repeated executable feedback,
- or capabilities the GitHub-authoring path cannot safely verify.

Prefer one consolidated Codex review per coherent feature-sized change rather than repeated reviews after tiny edits.

## Codex Review / Fixer Mode

When reviewing a ChatGPT/GitHub implementation, Codex should treat it as untrusted until verified.

Codex should:

1. Read the objective, success criteria, relevant project instructions, Source of Truth, and any relevant open questions/implementation notes.
2. Inspect the actual Git diff and surrounding code rather than trusting the implementation summary.
3. Build or compile when possible.
4. Run the narrowest meaningful automated tests, Unity validators, or executable checks.
5. Look specifically for correctness issues, regressions, architecture violations, hidden coupling, edge cases, and missing verification.
6. Fix material problems directly when the correction is clear, safe, and within scope.
7. Rerun relevant verification after any fixes.
8. Review final `git status` and diff for unrelated changes.
9. Report the final verdict, issues found/fixed, verification evidence, files changed during review, remaining uncertainty, and manual tests still required.

Do not create stylistic churn, broad refactors, or unrelated cleanup merely because another implementation style is possible.

## Current Design Principles

- The game is rhythm-first, not conventional free-movement-first.
- The basketball and body sounds should increasingly become the player's percussion.
- Early prototypes should favor readable, forgiving timing rather than final difficulty tuning.
- Planned difficulty modes are Easy, Medium, Hard, and Insane.
- Difficulty should primarily change rhythm/timing forgiveness while preserving the same core mechanics.
- A bounce may eventually create an obligation to resolve the ball state, but no input should not automatically count as failure because hesitation and pauses can be intentional.
- Preserve the principle:

  > Every bounce creates an obligation, but not every obligation is another bounce.

## Working Style

- Inspect the existing implementation before creating a new system.
- Prefer extending the current architecture over duplicating functionality.
- Make the smallest coherent change that satisfies the Current Task.
- Keep changes tightly scoped.
- Do not silently redesign Bounce Theory.
- Do not change unrelated files, systems, assets, settings, dependencies, or documentation.
- Prefer simple, readable solutions over unnecessary abstractions.
- Avoid broad refactors unless necessary for the active task.
- Preserve working behavior unless the task explicitly changes it.
- Do not promote temporary tuning values into permanent design decisions.

## Task Status

Use these statuses:

- `PLANNED`
- `IN PROGRESS`
- `AWAITING PLAYTEST`
- `COMPLETE`

Rules:

- Move a task to `IN PROGRESS` when implementation begins.
- If automated verification passes but user/runtime verification is still needed, mark it `AWAITING PLAYTEST`.
- Gameplay, visual, audio, feel, editor, or device-dependent work must not become `COMPLETE` solely because it compiles or automated validators pass.
- When hands-on verification is required, wait for explicit user confirmation before marking the task `COMPLETE`.
- If verification fails, keep the same task ID and return it to `IN PROGRESS`.

After a task is confirmed `COMPLETE`:

1. Add a concise entry to `Docs/COMPLETED_TASKS.md`.
2. Update `Docs/IMPLEMENTATION_STATUS.md`.
3. Update `Docs/ROADMAP.md` when applicable.
4. Update `Docs/DECISIONS.md` only if an important decision was actually made.

## Git Workflow

### Before Starting Any Task

When a Git remote/upstream is configured, synchronize the current branch before implementation:

1. Run `git status`.
2. If there are unexpected uncommitted changes, do **not** pull, overwrite, stash, reset, or discard them automatically. Stop and report them.
3. Confirm the current branch is the intended working branch.
4. Run `git fetch origin`.
5. Run `git pull --ff-only origin <current-branch>`.
6. Only after the branch is synchronized should implementation begin.

Use `--ff-only` so a routine task start does not silently create a merge commit. If the pull cannot fast-forward, stop and report the branch state rather than resolving history automatically.

The intended branch model is:

```text
main
└── dev
    └── milestone/<name>
```

- `main` = stable checkpoints/releases.
- `dev` = integration of completed milestones.
- `milestone/<name>` = active milestone development.
- Individual implementation tasks normally become commits on the current milestone branch.
- Do not create a new branch for every small task unless explicitly requested.
- Do not merge milestone branches into `dev` without explicit user approval.
- Do not merge `dev` into `main` without explicit user approval.
- Never force-push, rebase shared history, reset shared history, or discard unrelated user changes unless explicitly instructed.

After a successful implementation when Git is configured:

1. Run relevant build/tests/validators.
2. Review `git status`.
3. Review `git diff`.
4. Confirm unrelated files did not change.
5. Stage only task-related files.
6. Create one clear commit.
7. Push the current branch.
8. Report commit and push status.

A commit does not automatically make a task `COMPLETE`.

## Unity / Project Safety

- Preserve Unity `.meta` files and serialized references.
- Be careful when changing MonoBehaviour field names or types.
- Prefer normal Unity serialization/editor tooling over manually rewriting scene or prefab YAML.
- Do not alter package versions or project-wide settings unless explicitly requested.
- Avoid unnecessary scene changes when a script-level change is sufficient.
- Never claim Play Mode behavior was verified unless Play Mode was actually run.

## Verification

Before reporting implementation work complete:

- Build/compile when possible.
- Run targeted validators/tests when available.
- Inspect changed code.
- Review `git status` and `git diff`.
- Confirm unrelated files were not changed.
- Verify the stated acceptance criteria as far as the environment permits.

If something could not be directly verified, say so.

## Scope Discipline

If an unrelated issue is discovered:

- Do not fix it automatically unless it directly blocks the active task.
- Mention it briefly as a follow-up.
- Keep the current task focused.

Codex should review, verify, and repair decided work by default rather than silently redesign Bounce Theory. Use Codex as primary implementer only under the exceptions defined in **AI Role Split**.
