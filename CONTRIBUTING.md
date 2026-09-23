# Contributing

Group 9's working agreement for the CLMS project. Delivers CLMS-1.

## Branch strategy

`main` is always releasable and is never committed to directly. All work happens on a
short-lived branch off `main`, one branch per story:

```
feature/CLMS-14-patient-record      # a story from the plan
fix/CLMS-30-parser-timestamps       # a defect in delivered work
chore/ci-workflow                   # enabler work with no story
```

Keep a branch open for days, not weeks. A branch that cannot be merged inside a sprint is
a sign the story needed a task breakdown (see Appendix A of the project plan).

Rebase on `main` before opening the PR so the history stays linear:

```bash
git fetch origin && git rebase origin/main
```

## Pull requests

Every change reaches `main` through a reviewed PR. The template in
`.github/pull_request_template.md` fills in automatically; complete every section.

- **One story per PR.** If the PR delivers two stories, it should have been two branches.
- **Reviewed by someone who did not write it.** This is the team's definition of done, and
  it matters most for generated code: a codebase nobody can account for is the failure mode
  this project is graded on.
- **Squash and merge**, so each story is one commit on `main` with its ID in the subject.
- **Delete the branch after merge.**

### Commit messages

Lead with the story ID so `main`'s history traces back to the plan and the ConOps:

```
CLMS-14: add patient create, update and search

Covers ConOps 6.1.4. Search matches on name, DOB or ID.
```

## Branch protection

`main` is protected on GitHub: a PR with at least one approving review is required before
merge, and direct pushes are rejected. If a push to `main` is refused, that is the rule
working — branch and open a PR.

## Before you open the PR

```bash
dotnet build Clms.sln && dotnet test Clms.sln
```

Both must pass. Once CI is in place (CLMS-5), the same run happens on the PR and a failing
test blocks the merge.
