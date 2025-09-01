---
description: Designs and implements a vertical slice
argument-hint: Describe the vertical slice to implement
---

## Context

- Current git status: !`git status`
- Recent commits: !`git log --oneline -5`
- User input: $ARGUMENTS

## Your task

1. Check user input: user should have described a vertical slice - some functionality to be implemented end-to-end.
   If user input is missing or not understandable, ask clarification, and stop here.

2. Use architecture-planner to make an architectural plan. DO NOT modify anything at this stage.

3. After architectural plan is completed, YOU MUST read the following files:
   - REST_CONVENTIONS.md
   - ARCHITECTURE.md
   - CODING_CONVENTIONS.md

4. Implement all the necessary parts to complete the full vertical slice. Think hard.

5. Build the project and run all the tests.

6. Ask user if they'd like system tests to be written for the functioanlity
   Example: "Do you want me to implement system tests for the functionality just created?"

   6.1: If AND ONLY IF user requests the creation of systm tests, do the following:
      - Read TESTING_CONVENTIONS.md
      - Implement the system tests. Think.
      - After implementation is completed, run all the tests.