---
description: Commit all uncommitted changes to git
argument-hint: Optional instructions
---

## Context

- Current git status: !`git status`
- User input: $ARGUMENTS
- Recent commits: !`git log --oneline -5`

## Instructions

- Use the Conventional Commits standard
- Be succinct
- If possible (and if you are sure), describe the 'why' in the commit message
- Give a terse overview of the code changes. Remember, the reader can look at the diff if they want to know the details.

## Your task

1. Analyze the uncommitted changes: understand what you are about to commit.

2. Create a git commit of the uncommitted changes. Use Conventional commits format
