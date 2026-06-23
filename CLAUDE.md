# CLAUDE.md

## Global Protocols

### Language & state
- **User-facing:** Chinese. Respond User in chinese.
- **Tool/model-facing:** English.  
- If any tool returns a **SESSION_ID**, persist it and pass `--SESSION_ID <ID>` for follow-ups.

### Overall
- If the task is simple, multi-model collaboration may be skipped; however, you **must** immediately stop all actions and tell the user **why** collaboration is unnecessary, and **must not** proceed until the user explicitly approves. Example: “This is a simple <task>, so multi-model collaboration is not needed. Do you agree to proceed without any multi-model collaboration for this task? I will wait for your reply and strictly follow this specific collaboration rule.”
- Strictly follow the **Mandatory workflow**. Skipping any phase is considered a **high-risk operation**; you must stop immediately and explain **why** the phase would be skipped. Example: “In the current <phase>, I found <reason>, so the work of the next <phase> has effectively been resolved by <reason>. Do you agree that I skip <phase>? I will wait until you explicitly confirm before continuing to the next phase.”
- Except in rare special cases, **always** collaborate with **Codex and Gemini** by invoking the `Skill` tool directly with `collaborating-with-codex` and `collaborating-with-gemini`. **Do not** look for or run local scripts. **Run in parallel** and **do not** set a timeout.

### Mandatory workflow (do not skip phases)
1. **Phase 1 — Context Retrieval (Auggie)**
   - Call: `mcp__auggie__codebase-retrieval`
   - No assumptions. Retrieve **complete definitions/signatures** (recursive until sufficient).
   - Prefer semantic retrieval; avoid brittle keyword-only approaches.
2. **Phase 2 — Dual-model Planning (Codex + Gemini)**
   - Send **raw requirements** to both models; cross-validate.
   - Produce a **step-by-step plan** (light pseudocode ok).
   - **Hard stop:** end the message with **"Shall I proceed with this plan? (Y/N)"** and do nothing beyond planning until the user says **Y**.
3. **Phase 3 — Prototype Acquisition**
   - **Route A (UI/Styling):** Gemini.
   - **Route B (Backend/Logic):** Codex.
   - Prompts must require: **"OUTPUT: Unified Diff Patch ONLY. Strictly prohibit any actual modifications."**
4. **Phase 4 — Implementation (Claude + Codex + Gemini)**
   - Treat external diffs as **dirty prototypes**: mentally apply → validate → rewrite/refactor into production-quality code.
   - Minimal scope; no redundancy; minimal comments/docs.
   - Do not change externally observable behavior unless explicitly required; if changed, **call it out + explain impact + update/add tests**.
   - Any Error should directly raise a exception, including import error or something else. **Don't hide any error.** 
5. **Phase 5 — Audit & Delivery (Codex + Gemini)**
   - Run **parallel code review** using the produced unified diff + target files.
   - Integrate fixes, then deliver.

### Multi-model execution rules
- Prefer **parallel runs** for Codex/Gemini by calling both `Skill` tools in the same response.
- Always invoke via `Skill` tool — never look up or run local scripts.
- Use **no timeout** for all `Skill` calls.

### Safety & ownership
- External models: **zero filesystem write authority** (diff output only).
- Never hardcode secrets; never commit `.env` or credentials.
- Critical paths must have explicit error handling.
- No blind changes: trace dependencies/impact radius before edits.

### Git
- Before making changes, Checkout to a new dev branch related to the changes topic. Ask user if any changes are not commited
- Do not commit or push unless explicitly requested.
- Do not force-push to `main/master` without approval.

### Web research (no guessing)
- If something is unfamiliar or version-sensitive, search first (priority: official docs → changelog → upstream repo docs → community).
