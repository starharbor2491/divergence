# UX audit follow-up

November 7, 2026. These are local, unreleased changes following `UX-AUDIT.md`. The audit is an independent worker review using Laws of UX, not the unavailable requested “haiku 5.5 xhigh” model. It is not proof that no other problems exist.

## Outcomes

| Finding | Local fix | Evidence |
|---|---|---|
| UX-01 | Menu, Restart and valid Load require explicit confirmation. Cancel preserves the Run and its previous pause state. Save requests a download and leaves the confirmation open. | Unity action/replacement tests; confirmation screenshots at both sizes. |
| UX-02 | Terminal extinction finalizes Species before the final sample. CSV and saves receive zero living populations and terminal extinction times. Legacy terminal saves are repaired without mutating their input snapshots. | Terminal extinction, CSV, snapshot/hash and legacy-save tests. |
| UX-03 | Generation uses actual population means, including decreases. Nearest-sample lookup no longer assumes monotone values. | Axis test with means 12, 9, 11; shared explanatory copy. |
| UX-04 | Binding, Trait/axis changes and refresh invalidate hover. Redraw recomputes stationary hover, and draw/click paths validate indices. | Unity decimation/change tests; real-layout stationary hover across forced decimation in both short tours. |
| UX-05 | Empty terminal populations do not draw a false zero Trait mean or a frequency collapse. Hover says “No living Cells.” | Empty-population mean/readout and bounds tests; rendering guards inspected. |
| UX-06 | Chart cards live in the window overlay, use window bounds and support bounded scrolling. | A 21-layer Species readout remained visible and inside the window in both native tours. |
| UX-07 | Layer order is fixed. Dialogs no longer move above tooltips or the minimum-size warning. Ordinary tooltip descendants ignore pointer picking. | Layer-order and tooltip-picking tests; resizing with Help open shows only the warning. |
| UX-08 | Dropdowns close on detach, relevant geometry/panel/tab changes and Escape. Open dropdowns block World shortcuts. | Popup/modal and panel-collapse tests; input gate inspected. |
| UX-09 | The native browser gesture must press and release inside an eligible Load region. No later unrelated click is armed. Eligibility is checked again synchronously in C#. | Bridge gesture/cancel/outside-click cases; compiled browser cancel followed by an unrelated click produced no new chooser request. |
| UX-10 | Loads have request IDs, a blocking/cancellable loading state and a Run-context check. Cancellation invalidates late callbacks and aborts pending readers. | Bridge duplicate/stale-read cases; C# stale-ID tests; compiled cancellation followed by a fresh Load. |
| UX-11 | File success, cancellation and read failure are distinct. Empty selected files reach validation rather than masquerading as cancellation. | Node error/cancel/empty cases; C# callback test; a real browser FileReader read a zero-byte File and another Load remained available. |
| UX-12 | Event commits and previews reject centres outside the World. A rejected commit stays armed. | Outside-boundary action test; shared preview boundary guard inspected. |
| UX-13 | A successful Whole World Event clears any previously armed local Event. | Action test verifies the arm clears and the correct Event is recorded. |
| UX-14 | Event-dialog capacity refreshes while open; a paused Run explains how to let active Events expire. | Capacity-expires-while-open test. |
| UX-15 | Whole World Toxin spills keep Radius editable because it controls quantity. The dialog explains the requested particle count and capacity limit. | Toxin/non-Toxin enablement tests and Whole World screenshot; formula/copy checks. |
| UX-16 | Colliding Event chips form neutral groups with all grouped records retained for their explanation. Individual dashed intervention lines remain. | Simultaneous-Event grouping test and grouped chips in screenshots. |
| UX-17 | Tree uses the shared time-based Event markers. | Tree Event mapping test and screenshots. |
| UX-18 | Tree grows by a fixed readable row height instead of shrinking every row to fit. The surrounding panel scrolls. | 100-Species height test; 30-Species native fixtures at both sizes. |
| UX-19 | Extinct Tree line widths use the last nonzero checked population, not the historical peak. This value persists; old saves recover it from available historical samples. | Last-population versus peak and save/restore tests; Tree width source inspected. |
| UX-20 | Highlighting clears when no living Cells belong to that Species, including immediately after a click. | Highlight test removes the last matching Cells. |
| UX-21 | Deaths copy distinguishes the latest ten minutes of bars from whole-Run legend totals. | Shared-copy validation and Stats screenshots. |
| UX-22 | Other has a swatch and explanation. Aggregated living Species are labelled; individually drawn Species are not. Classification is ready before first layout. | Other-membership/count test before layout; native legend assertion and screenshots. |
| UX-23 | Ten Trait definitions give ranges, units and trade-offs. Reproduction and sexual-share explanations describe the implemented rules. Help and the one-page rulebook match. | 78-entry copy validator, ten Help-section comparisons and PDF text/bounds checks; Help screenshot. |
| UX-24 | Parent uses its own parent-ID explanation, not the Generation explanation. | Inspector binding inspected; parent copy included in shared-copy validation. |
| UX-25 | Reset requires both clicks to hit the thumb. A double track click changes the value without resetting it. | Real-layout track/then-thumb interactions in both tours; unlaid-out slider guard test. |
| UX-26 | Finite positive pixel ratios below one remain intact; invalid ratios fall back to one. | Tests at 0.8, 2, 0, negative, NaN and infinity; matching bridge check. Browser integration used DPR 2. |
| UX-27 | Panel toggling no longer schedules a camera refit. F remains the explicit fit command. | Camera position, zoom and deferred-fit state test. |
| UX-28 | Save/PNG say “Download requested.” CSV explains that two downloads may need permission. No disk-write guarantee is claimed. | Action/copy inspection and shared-copy validation. |
| UX-29 | **Deferred.** No new terminal-review state or fourth extinction action was added. | DESIGN 6.4 explicitly specifies a blocking overlay with Restart, Export CSV and Menu. This needs approval before changing the screen pattern. |

## Validation

- `tools/test.sh`: **109 passed, 0 failed, 0 skipped**. These include the existing Core and Unity-layer suites plus UX regression cases.
- `node tools/test-file-bridge.cjs`: **12/12 passed**, executing the shipped `.jslib` with a mock DOM and FileReader.
- Native development build: succeeded. Two final short, serial tours at **1920×1080** and **1366×768** passed. They use **Metal on Apple M4 Max**, pause immediately and build synthetic history, rather than simulating a long Run. Each process exits automatically. The tours also check real-layout slider clicks and stationary hover during decimation. Screenshots are under `Builds/ux-shots/1920/` and `Builds/ux-shots/1366/`.
- The below-minimum test resizes to a requested 1200×675; macOS reports a 1200×676 surface. With Help open, the full-window warning takes priority.
- Final WebGL build: **succeeded, 53 MB**, Unity build time **37.3 seconds**, in `Builds/WebGL/`. The build used two Unity job workers.
- Compiled WebGL integration used the embedded browser and reported **ANGLE Metal Renderer: Apple M4 Max**. Three eligible Load gestures reached the picker with `navigator.userActivation.isActive === true`. Cancel removed the input; an unrelated click did not reopen it; an empty File passed through FileReader; later Loads still opened. No page errors were reported.
- The embedded pane was narrower than the supported minimum. For this integration check only, its canvas was enlarged and CSS-scaled to a 1280×720 rendered canvas. Native tours, not this clipped browser pane, supply the supported-layout evidence.
- To avoid an unattended OS dialog, the browser check intercepted the final file-input `click()`. It exercised actual browser mouse gestures, compiled Unity callbacks and FileReader, **not manual operation of the native chooser**. This check preceded the final rebuild; subsequent code changes were confined to the opt-in visual harness and tests, with no changes to the tested Interop/UI production paths.
- Copy validation: **78 unique tooltip entries**, all ten Help sections match DESIGN and the rulebook; the PDF is one A4 page with selectable text inside page bounds.
- `git diff --check`: passed.

Earlier test failures (unlaid-out slider geometry and EditMode object persistence) were fixed before the final passing suites. No claim is made about Safari/Firefox, every supported aspect ratio, every Preset combination, manual OS file selection, disk delivery under every browser policy, long-run performance or thermal measurements. Long-run charts use synthetic/forced regression fixtures to avoid another extended simulation.

## Second audit: nine additional fixes

November 10, 2026. The following work addresses the nine findings from the later web-interface/ponytail review. These results are separate from the historical UX-01–29 evidence above.

| Finding | Local fix | Evidence |
|---|---|---|
| Exceptions disabled in WebGL | Project setup now uses `FullWithoutStacktrace`; the generated Player settings retain it. | WebGL build and compiled malformed-JSON callback check. |
| Unsafe saved state | `Simulation.FromSnapshot` validates before constructing a World. Checks cover parameters/settings, allocation bounds, counters, nonzero RNG state, finite numbers, array shapes, Genomes, IDs/references, Events and statistics. JSON input length is bounded before parsing; browser/native file imports reject oversized files before reading. | Malformed-state cases, every scalar parameter, JSON rejection/current-Run preservation, v1 terminal repair and exact save/continue tests. A synthetic capacity case loads 600 Cells, 1500 Food, 1500 Toxin, 16 active Events and 3600 samples without running a long simulation. |
| No browser-exit protection | An existing Run installs one native `beforeunload` listener. Replacing/saving retains protection; discarding to setup removes it. | Bridge repeated-install/removal test and compiled application callbacks: listener count 0 → 1 → 0; the protected synthetic exit event is cancelled. |
| Final sample lost during decimation | Decimation retains the newest empty-population sample and saturates interval doubling instead of overflowing. | Extinction at capacities 20, 21 and 3600 retains terminal time, zero population and zero living Species; saves round-trip exactly. |
| Popup catchers above extinction | Entering Extinct closes dropdowns, the Events menu and tooltips. | Separate running-to-extinct tests with each popup open. The three specified terminal actions are unchanged. |
| Incomplete state hash | Both RNG streams hash by reference. Setup, every parameter and omitted Cell/Event/Species/candidate/statistics fields now contribute. | Reflection-based coverage of every settings field, deliberate saved-state perturbations, JSON/snapshot equality and continued-run equality. |
| Grouped Event details cannot scroll | Event chips use a separate interactive, window-bounded scroll card. It stays open while entering/reading the card; ordinary tooltips still ignore pointer picking. | Content/picking tests and short Metal tours at 1920×1080 and 1366×768. Both open 102 grouped Events, move into the card, scroll it with a wheel event and check window bounds. Screenshots: `Builds/audit-shots/{1920,1366}/10-grouped-events-scrolled.png`. |
| Broken experiment SDK fallback | The script checks for the .NET 8 SDK, not merely a runtime, then searches installed Unity SDKs. Requiring SDK 8 also supplies the matching runtime for the net8.0 runner. Missing prerequisites produce an actionable error. | Five stub-shell cases plus real `bash tools/experiment.sh "Mild world" 0`, which succeeds using the installed 6000.6 SDK. The project editor remains 6000.3.28f1. |
| Unused Seed helper | Removed the unused `SeedUtil.RandomText` and its alphabet. | Caller search and successful Core/Unity builds. |

### Current validation and limits

- Final Unity EditMode suite: **160 passed, 0 failed, 0 skipped** (`Builds/audit-test-results.xml`), test execution **8.73 seconds** plus editor startup. Two Unity job workers; no graphics.
- Browser bridge: **14/14** checks pass, including rejection before FileReader allocation. Experiment script: **5/5** checks pass.
- Final WebGL rebuild: **succeeded, 54 MB**, Unity build time **39.98 seconds** (`Builds/logs/audit-build-webgl.log`); output remains local in `Builds/WebGL/`.
- Native UI checks use **Metal on Apple M4 Max**, 30 fps, synthetic history and an immediately paused Run. Both final size tours pass and exit automatically.
- Compiled WebGL uses **ANGLE Metal Renderer: Apple M4 Max**. Calling the production load callback with malformed JSON, then a valid synthetic save, then malformed JSON and an invalid zero statistics interval leaves the application responsive. The exit listener remains installed after the rejected loads and is removed by the production discard callback. Browser errors are empty.
- The compiled callback check uses `SendMessage` to reach the existing application callbacks, not a new testing API. It does **not** prove a native OS chooser interaction, real navigation dialog, sticky-user-activation behaviour or download delivery. The tab was not visible for screenshot capture, and the embedded viewport did not provide standard supported-layout evidence.
- The compiled callback check precedes the final small validation refinements (Trait-mean bounds, accepting a population cap below the Species split threshold and the pre-read file-size guard). The final Unity/bridge checks cover these changes, and they are included in the final rebuild. Exception and exit-listener code did not change.
- Initial test-harness failures were corrected: detached pointer events did not exercise callbacks, and synthetic enter/leave events needed explicit targets. The opt-in native tour now catches nested coroutine failures and quits rather than leaving a failed tour running. One earlier harness attempt hit its 20-second timeout; both final tours pass.
- Save-format version remains **1**, including optional old `LastPopulation` recovery. Defensive import limits include 64 MiB files, 64 Mi characters of JSON, 10,000 Species/Event records, one million death bins and ten million seconds of saved time; extreme hand-edited or oversized files are rejected. Normal UI capacities remain 600 Cells/1500 Food/1500 Toxin.
- No long simulation, new dependency, commit, push or deployment. Safari/Firefox, real leave-dialog behaviour and long-run performance/thermal measurements remain unchecked. The temporary fixture was removed, its local server stopped, and the owned browser tab verified at `about:blank`.

## Journal and release state

The factual v1 release record is in the local `Process Journal.docx`; its original is preserved at `Journal/Process-Journal-before-v1.docx`. Personal reflection prompts remain for the user. The journal stays out of source control.

Source changes and the new WebGL build are local. The published `v1.0.0` tag, public GitHub Pages build and existing v1 release ZIP have **not** been changed by this follow-up. No commit, push or deployment was performed. The temporary test server was stopped, and the owned embedded browser tab was navigated away from the simulation.
