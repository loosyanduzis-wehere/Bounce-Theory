# Bounce Theory — Development Workflow

## Project Knowledge Flow

```text
Game Design / Source of Truth
        ↓
ROADMAP
        ↓
IMPLEMENTATION_STATUS
        ↓
Codex implementation
        ↓
automated verification
        ↓
AWAITING PLAYTEST when needed
        ↓
user verification
        ↓
COMPLETED_TASKS
        ↓
Git history
```

### File Roles

- `Game Design.md` explains what Bounce Theory is.
- `ROADMAP.md` describes future planned work.
- `IMPLEMENTATION_STATUS.md` describes what exists now and contains the one active implementation task.
- `COMPLETED_TASKS.md` provides concise readable implementation history.
- `DECISIONS.md` records why important architecture choices exist.
- Git records exact code changes.

The Roadmap is not an automatic queue.

---

## Task Workflow

Before implementation:

1. User + ChatGPT choose one small next task.
2. Resolve important design questions conversationally.
3. Promote that task into `IMPLEMENTATION_STATUS.md`.
4. Give it a stable task ID, goal, scope, acceptance criteria, and explicit out-of-scope items.

Codex then:

1. Reads `AGENTS.md`.
2. Reads `IMPLEMENTATION_STATUS.md`.
3. Reads only relevant supporting documentation.
4. Inspects the actual implementation.
5. Sets the task to `IN PROGRESS`.
6. Implements only that task.
7. Runs relevant automated verification.
8. Reviews `git status` and `git diff`.
9. Commits and pushes the task.

If hands-on verification is required:

1. Set the task to `AWAITING PLAYTEST`.
2. User tests it.
3. If it passes, mark it `COMPLETE`.
4. If it fails, return the same task ID to `IN PROGRESS`.

After completion:

- append a concise record to `COMPLETED_TASKS.md`,
- update `IMPLEMENTATION_STATUS.md`,
- update the Roadmap when applicable,
- record a decision only when an important architectural/design choice was actually made.

---

## Git Branch Workflow

```text
main
└── dev
    └── milestone/<name>
```

### `main`

Stable checkpoints/releases.

Do not merge `dev` into `main` without explicit user approval.

### `dev`

Integration branch for completed milestones.

Do not merge a milestone into `dev` without explicit user approval.

### `milestone/<name>`

Active milestone development branch.

Small implementation tasks become commits on the current milestone branch instead of receiving a new branch for every tiny change.

---

## Normal Implementation Loop

```text
choose task
→ document Current Task
→ implement
→ verify
→ review diff
→ commit
→ push
→ playtest if needed
→ mark complete
→ update docs
→ choose next task
```

Do not silently move to the next Roadmap item.
