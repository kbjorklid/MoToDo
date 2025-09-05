---
description: Prime context by reading .md documents in the repository
argument-hint: all | list of: code, architecture, tests, rest
---

## Context

User input: $ARGUMENTS

## Your task

You will read documents, selected based on user input. These docuements contain information needed to understand
how to develop the project. Your job is to read the documents, and nothing else. You'll need the information
when user subsequently requests changes to the project.

**User Input:** code, coding, or similar: **Read:** CODING_CONVENTIONS.md
**User Input:** test, testing, or similar: **Read:** TESTING_CONVENTIONS.md
**User Input:** rest, api, or similar: **Read:** REST_CONVENTIONS.md
**User Input:** archtecture, or similar: **Read:** ARCHITECTURE.md
**User Input:** all or everything: **Read:** All the documents mentioned above.

If user input is missing, ask what what you should read. Example:
"What do you want me to read? You can choose 'all', or any of: 'code', 'test', 'rest', and/or 'architecture'."

After reading, simply state: "I've finished reading. What's next?"