# refactor_v5 — Feature Implementation List

**Branch:** refactor_v5 from master  
**Audit date:** 2026-04-12  
**Starting state:** 63/79 features implemented  

## Status Legend
- ✅ Already implemented  
- 🔨 Implementing this session  
- 🔵 Deferred (complex / low value)

---

## Missing Features — Priority Order

### HIGH PRIORITY (implement now)

| ID | Feature | Complexity | Notes |
|----|---------|------------|-------|
| B10 | Next/Prev Error/Warning navigation | Small | Toolbar buttons + shortcuts missing from MainForm wiring |
| D5 | Cross-Reference Jump API ↔ CallTree | Small | Sync selection between trees |
| D6 | Sort APIs in tree | Small | Sort by name / count / time |
| D3 | API Invocation Details Panel | Small | Show selected API stats in side panel |
| A6 | Merge Log Files (time-sorted) | Medium | Merge 2+ logs by epoch timestamp |
| C2 | Lazy Loading for Large Logs | Medium | Expand-on-demand for 50k+ node trees |

### MEDIUM PRIORITY

| ID | Feature | Complexity | Notes |
|----|---------|------------|-------|
| F4 | Dependency Graph | Large | Who-calls-whom directed graph |
| L2 | NL Search / Q&A | Medium | Claude API — structured summaries only |
| L3 | Anomaly Detection | Medium | Claude API — flag statistical outliers |
| L4 | Root Cause Suggester | Medium | Claude API — suggest likely causes |
| L5 | Auto-Generate Bug Report | Medium | Claude API — structured bug report |
| L6 | Conversational Log Assistant | Medium | Claude API — chat about the log |

### LOW PRIORITY / DEFERRED

| ID | Feature | Complexity | Notes |
|----|---------|------------|-------|
| A5 | Multiple Logs in Tabs | Very Large | Major architecture change — defer to v4 |
| G3 | Dockable Panels | Very Large | Requires 3rd-party library — defer |
| K6 | Plugin Architecture | Very Large | IPlugin system — defer to v4 |
| K7 | Monitoring Tools Integration | External | Depends on external tools — defer |

---

## Implementation Plan

1. **B10** — Wire Next/Prev Error buttons + F7/Shift+F7 shortcuts  
2. **D5** — Cross-reference: click API node → highlight in CallTree  
3. **D6** — Add sort toolbar to API tree panel  
4. **D3** — Show API stats panel on right-click / selection  
5. **A6** — Merge logs dialog + time-sorted merge service  
6. **C2** — Lazy tree loading with placeholder nodes  
7. **F4** — Dependency graph panel (extends CallGraphPanel)  
8. **L1-L6** — AI features tab (Claude API, structured summaries only)  

---

## Commit Convention

`feat(XX): description` per feature, pushed immediately after each.
