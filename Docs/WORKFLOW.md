# Bounce Theory — Development Workflow

> `AGENTS.md` is the authoritative operating instruction file.
> This document is the concise project knowledge-flow map so a fresh chat, Codex session, or collaborator knows where information belongs.

## File Roles

- `Docs/Game Design.md` — authoritative game/design source of truth.
- `Docs/ROADMAP.md` — future milestones and planned work only.
- `Docs/IMPLEMENTATION_STATUS.md` — current implementation state, one active feature/task, success criteria, and verification gate.
- `Docs/COMPLETED_TASKS.md` — concise completed implementation history.
- `Docs/DECISIONS.md` — durable architecture/design decisions and rationale.
- `Docs/OPEN_QUESTIONS.md` — recognized unresolved decisions; never silently guess these.
- `Docs/IMPLEMENTATION_NOTES.md` — active technical realities, workarounds, discoveries, and implementation constraints.
- `AGENTS.md` — workflow rules, Git safety, AI role split, verification rules.
- Git history — exact code/document changes.

Do not duplicate the same information across files unless a short pointer is needed for navigation.

## Standard Development Loop

```text
Goal
→ Unknowns Pass
→ targeted research/reference hunt
→ decisions + Open Questions
→ implementation plan
→ observable success criteria + verification plan
→ ChatGPT implements one coherent feature-sized change through GitHub
→ Git commit/branch/PR handoff
→ Codex fresh-context review + build/test + scoped repair
→ user/manual verification when required
→ capture learnings in the correct file
→ next coherent chunk
```

Guiding size rule:

> Small enough to reason about; large enough to justify the context/setup cost.

## AI Role Split

### Main ChatGPT conversation

Default architect and first implementer for normal medium/large work:

- inspect the repository,
- run the Unknowns Pass,
- resolve or record unknowns,
- define success/verification,
- implement a coherent feature-sized change through GitHub.

### Codex

Default fresh reviewer/fixer/verifier after Git handoff:

- inspect the actual diff,
- compile/build,
- run targeted validators/tests,
- find correctness/regression/architecture/edge-case problems,
- fix clear material in-scope issues,
- rerun verification,
- report remaining manual tests.

Use Codex as primary implementer only when local iterative build/test/debug loops, broad mechanical repository changes, migrations, or refactors materially benefit from it.

## Status Flow

```text
PLANNED
→ IN PROGRESS
→ AWAITING PLAYTEST
→ COMPLETE
```

Runtime-dependent gameplay/visual/audio/feel work cannot become COMPLETE from automated verification alone.

After user-confirmed completion:

- append a concise record to `COMPLETED_TASKS.md`,
- update `IMPLEMENTATION_STATUS.md`,
- update `ROADMAP.md` when milestone state changes,
- update `DECISIONS.md` only for durable decisions,
- update `OPEN_QUESTIONS.md` when a question is resolved or newly recognized,
- keep `IMPLEMENTATION_NOTES.md` limited to still-relevant technical realities.

## Git Branch Model

```text
main
└── dev
    └── milestone/<name>
```

- `main` = stable checkpoints/releases.
- `dev` = completed-milestone integration.
- `milestone/<name>` = active milestone development.
- Do not merge milestone → dev without explicit user approval.
- Do not merge dev → main without explicit user approval.
- Do not rebase/reset/force-push shared history unless explicitly instructed.

Detailed branch synchronization and safety rules live in `AGENTS.md`.

## Context Hygiene

At meaningful checkpoints, audit project context:

- remove stale current-state claims,
- move completed narratives out of `IMPLEMENTATION_STATUS.md`,
- move future ideas out of implementation notes and into `ROADMAP.md` or `OPEN_QUESTIONS.md`,
- remove resolved questions from `OPEN_QUESTIONS.md`,
- keep temporary tuning out of the Source of Truth unless intentionally promoted,
- avoid copying conversation summaries into project files,
- prefer pointers over duplicate prose.

The repository files should be sufficient to restart development in a new conversation without relying on old chat context.
