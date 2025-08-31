---
description: Check outdated nuget pacakges and update them
argument-hint: Optional instructions
---

## Context

- Current git status: !`git status`
- Recent commits: !`git log --oneline -5`
- Information of the last commit: !`git show --name-status`
- User input: $ARGUMENTS

## Your Task

### 1. Figure what the scope of the review is

Use the first scope that is available:

1. If user input specifies review scope, then deduce the scope from the user input

2. otherwise, if there are modified files, not committed to git, (according to git status), then these files are to be reviewed.

3. otherwise, the files in the last commit are to be reviewed.

### 2. Review and fix

Use code-fixer agent to review the code in scope and fix any problems.