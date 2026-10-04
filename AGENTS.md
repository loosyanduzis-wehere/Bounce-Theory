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

Codex should implement decided work, not silently redesign Bounce Theory.
