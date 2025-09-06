---
description: Reviews and improves code for readability and maintainability
argument-hint: Optional: describe the scope (if left undescribed, uncommitted changes is the scope)
---

## Context

- Current git status: !`git status`
- User input: $ARGUMENTS

## Instructions

- You are to review code and improve its readability and maintainability
- Do not do large architectural changes. Concentrate on code-level improvements.
- Things to consider while reviewing / fixing:
  - Does the code follow project's coding conventions? 
  - Can the code be made simpler or easier to read?
  - Consider the following refactorings (but only if they improve code):
    - Rename Variable 
    - Rename Function 
    - Extract Method
    - Replace Temp with Query 
    - Decompose Conditional 
    - Replace Nested Conditional with Guard Clauses 
    - Replace Magic Number with Named Constant 
    - Introduce Explaining Variable
    
## Your task

1. Check user input: user should have described some scope which you are to review and improve. If there is no
   user input, assume that the scope is code not committed to Git.

2. Consider: are there tests in the scope of the review? If yes, read TESTING_CONVENTIONS.md

3. Read CODING_CONVENTIONS.md

4. Go through the code in the review socpe, and improve code where you can. Think hard.

5. Build the project and run all tests.