# CAD 3P Log Browser — Next-Level Improvement Plan (April 22, 2026)

## Objective
This document proposes a practical roadmap to improve:
1. Features,
2. Performance,
3. UI/UX,
4. Product maturity for broader adoption.

The recommendations are based on the current architecture, implementation patterns, and existing user documentation/help.

---

## 1) Current Strengths to Build On

- The app already has broad functionality (search, filter, navigation, performance tab, flame graph, timeline, exports), so the next step is depth and workflow integration rather than adding isolated features.
- The architecture already separates services/managers/models, which enables incremental refactoring without a rewrite.
- Virtual-mode log rendering is already in place and is a strong foundation for large-log performance.

---

## 2) High-Impact Feature Improvements

### F1. Investigation Workspaces (Sessions 2.0)
**What:** Save/restore complete analysis context per case:
- opened files,
- filters,
- search terms,
- selected line,
- bookmarks,
- tab layout,
- diff baselines.

**Why:** Current session persistence focuses on last-opened files only; analysts need full investigation continuity.

**Implementation direction:**
- Extend settings/session model to persist a richer `WorkspaceState` object.
- Add "Save Workspace", "Load Workspace", and auto-checkpointing.
- Optional portable `.cad3pworkspace` file for team sharing.

### F2. Multi-log Correlation View
**What:** Correlate events across multiple logs (by timestamp/thread/API) in one synchronized timeline.

**Why:** Real issues often span app + adapter + sync logs; single-file analysis is limiting.

**Implementation direction:**
- Add correlation key extraction in parser layer.
- Build a merged event stream with source tags.
- Add side-by-side + unified modes.

### F3. Rule-based Anomaly Detection
**What:** User-defined rules such as:
- "API X > 500 ms",
- "more than N retries",
- "unmatched ENTER/EXIT",
- "error bursts in 60 seconds".

**Why:** Moves product from passive viewer to proactive detector.

**Implementation direction:**
- Add a small rule engine with templates.
- Emit findings into a new "Insights" panel with severity and jump links.

### F4. Investigation Report Generator
**What:** One-click report output (HTML/Markdown/PDF) containing:
- summary metrics,
- top regressions,
- key timelines/charts,
- bookmarked evidence lines.

**Why:** Converts analysis into sharable output for engineering/support leadership.

---

## 3) Performance Improvements (Priority)

### P1. Remove O(n²) slow-call matching paths
**Observation:** `FindTopSlowestCalls` currently scans forward from each ENTER using `FirstOrDefault`, which can degrade badly on large logs.

**Improve:**
- Build one pass stack/index matching map (`enterLine -> exitLine`).
- Reuse the map across all analytics methods.
- Keep complexity near O(n).

### P2. Incremental parsing + background pipeline
**Observation:** File load reads all lines into memory before full analysis.

**Improve:**
- Add staged pipeline: `Read chunk -> Parse chunk -> Emit partial UI updates`.
- Use bounded producer/consumer channels to avoid UI freezes and peak memory spikes.
- Support cancellation and resume cleanly.

### P3. Highlight/search indexing for repeated queries
**Observation:** search/highlighting walks all visible text repeatedly.

**Improve:**
- Add optional token index (or trigram/term index) after load.
- Cache match sets per query options.
- Perform viewport-first highlighting and lazy extend off-screen.

### P4. Reduce UI object churn in virtual list retrieval
**Observation:** Virtual list retrieval creates `ListViewItem` and subitems per retrieval call.

**Improve:**
- Add cache/pooling strategy for frequently requested rows.
- Benchmark with 100k–1M lines and track GC pressure.

### P5. Profiling and performance budgets
**What to add:**
- Baseline benchmarks for load/parse/search/filter/render/export.
- CI gate with thresholds (e.g., parse throughput, memory ceiling, UI responsiveness).

---

## 4) UI/UX Improvements

### U1. Command Palette + Universal Quick Actions
**What:** `Ctrl+Shift+P` style command palette with fuzzy search for every action.

**Why:** Faster feature discoverability vs deep menu traversal.

### U2. Progressive disclosure layout
**What:**
- Beginner mode: minimal controls.
- Advanced mode: full analysis toolchain.

**Why:** Reduces cognitive load while keeping power users productive.

### U3. Investigation dock preset system
**What:** Named layouts (e.g., "Performance Triage", "API Debug", "Diff Review").

### U4. Context-rich line details
**What:** Expand line inspector with:
- parsed fields,
- correlation IDs,
- call depth,
- nearest errors/warnings,
- quick links (copy as JSON, pin evidence, add rule from line).

### U5. Accessibility polish
**What:**
- stronger keyboard-only workflow,
- high-contrast validation,
- screen reader labels for custom controls/visualizations,
- configurable font scaling presets.

---

## 5) Architecture Upgrades for “Next Level”

### A1. Break down MainForm orchestrator responsibilities
**Observation:** `MainForm` coordinates many concerns (file I/O, theme, tabs, services, operations).

**Improve:**
- Introduce an application controller or mediator layer.
- Keep MainForm focused on view bindings/events.
- Move workflows into testable use-case services.

### A2. Consolidate duplicate/legacy service paths and naming
**Observation:** There is both `Services/Search/SearchService.cs` and an empty `Services/SearchService.cs`, which can confuse maintenance.

**Improve:**
- Remove obsolete file(s) and enforce one canonical namespace/path per service.
- Add architecture tests or script checks for duplicates/stubs.

### A3. Introduce plugin-style analyzers
**What:** Load analyzers from assemblies/interfaces:
- core app provides contracts,
- teams add domain-specific detectors without modifying main codebase.

### A4. Telemetry for product usage (opt-in)
**What:** Local/optional telemetry to understand:
- most used features,
- slow operations,
- common failure points.

**Why:** Lets roadmap be data-driven rather than assumption-driven.

---

## 6) Suggested Delivery Roadmap

### Phase 1 (2–4 weeks): quick wins
1. Refactor slow-call matching to O(n).
2. Remove duplicate/obsolete service artifacts.
3. Add profiling harness + baseline metrics.
4. Add command palette (MVP).

### Phase 2 (4–8 weeks): core product lift
1. Investigation workspaces.
2. Rule-based anomaly detection MVP.
3. Incremental load/parse pipeline.
4. UI layout presets.

### Phase 3 (8+ weeks): differentiation
1. Multi-log correlation view.
2. Report generator.
3. Plugin analyzer ecosystem.
4. Advanced insights (trend/regression detection across runs).

---

## 7) Success Metrics (Recommended)

- **Performance:**
  - 2–5x faster large-log analysis for top workflows.
  - Memory usage reduced for very large files.
  - UI interaction latency < 100 ms for common actions.

- **Feature adoption:**
  - Workspace save/load usage,
  - anomaly rules created per user,
  - report generation frequency.

- **Quality:**
  - Fewer regressions via benchmark gates,
  - reduced issue reopen rate for "cannot reproduce" cases.

---

## 8) Immediate Next Steps

1. Implement O(n) matching map and benchmark before/after.
2. Draft `WorkspaceState` schema and persistence contract.
3. Prototype command palette with top 20 actions.
4. Build anomaly rule templates for the 5 most common support pain points.

These four steps will deliver visible product value quickly while laying a strong technical foundation for bigger features.
