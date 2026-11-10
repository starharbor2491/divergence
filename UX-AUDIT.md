# Divergence v1.0.0 — fresh independent Laws of UX audit

**Implementation follow-up:** UX-01 through UX-28 have local fixes. UX-29 remains deferred because it changes DESIGN 6.4. See `UX-FIXES.md` for outcomes, validation evidence and remaining limits. The original baseline findings below are preserved.

## Scope, evidence and limits

Read-only implementation audit of the supplied v1.0.0 workspace. Only this report was written. No Unity process, browser, build, test suite, deployment or simulation was launched. No journals were changed.

This is a fresh worker review, not a claim to have run the unavailable requested “haiku 5.5 xhigh” model.

Sources: `PLAN.md` for behaviour, `DESIGN.md` for appearance, `CONTEXT.md` for vocabulary; App, Input, Interop, UI Controls/Screens/Charts/Theme, camera/World rendering, WebGL bridge/template, and relevant Core statistics/Species/Event paths. Existing development-player screenshots `01-setup.png` and `03` through `09` were inspected. They show the baseline composition, not browser interactions or long-run edge cases. The findings below are static code findings unless explicitly described as screenshot observations.

Laws of UX is a reasoning lens, not a replacement specification. The homepage and the individual linked law pages were fetched using FetchUrl. No arbitrary number of findings was targeted. This review cannot establish that no other defects exist.

Severity:

- **High:** irreversible loss of work, materially false scientific data, or a chart failure in a supported workflow.
- **Medium:** a concrete blocked/misleading workflow, hidden state or substantial interpretation problem.
- **Low:** local interaction/copy inconsistency with a bounded workaround.

Compatibility:

- **Restore:** implement an existing PLAN/DESIGN requirement correctly.
- **Compatible safeguard/extension:** preserves deliberate decisions, but adds behaviour or copy not explicitly required.
- **Design-constrained:** a useful remedy needs an explicit change to a specified screen pattern. Not an implementation violation.

## Findings

### UX-01 — High — one click can irreversibly discard an unsaved Run

**Evidence:** `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/UI/Screens/TopBar.cs:90-92` wires Home directly to `GoToSetup`; `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/App/AppController.cs:103-110` immediately sets `Sim = null`. A valid Load unconditionally calls `Begin` at `AppController.cs:198-209`. Extinction Menu and Restart directly replace/discard the Run at `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/UI/Screens/ExtinctionOverlay.cs:45-55`. There is no confirmation or recovery branch in those paths.

**Scenario:** after a twenty-minute experiment, a Player mistakes the adjacent Home icon for the panel toggle, or selects the wrong valid saved file. The accumulated experiment is gone. A hover warning is not protection against an accidental activation.

**Law:** [Jakob’s Law](https://lawsofux.com/jakobs-law/), [Peak-End Rule](https://lawsofux.com/peak-end-rule/).

**Smallest remedy:** guard replacement/discard with a plain dialog offering Cancel, Save, and an explicit Discard/Replace action. Validate a candidate Load before asking to replace the current Run. Do not require a confirmation just to open the file picker or when no Run exists.

**Compatibility:** compatible safeguard; PLAN 15.1 still allows replacement and DESIGN’s Home action still returns to Setup. No autosave system is necessary.

### UX-02 — High — total extinction leaves Species recorded as alive

**Evidence:** `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/Core/Simulation.cs:379-383` calls `Species.Check` at extinction, but `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/Core/SpeciesTracker.cs:116-119` returns immediately when the living Cell count is zero. Thus the population clearing/extinction logic at `SpeciesTracker.cs:235-255` never runs for the final Species. `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/Core/CsvExport.cs:55-59` exports these stale records.

**Scenario:** a Run ends with “Population extinct,” but `species.csv` still gives a positive `current_population` and an empty `extinct_time_s` for the last Species. The final Sample also copies stale living/extinct Species counts. A teacher cannot reconcile the evidence.

**Law:** [Mental Model](https://lawsofux.com/mental-model/).

**Smallest remedy:** on terminal population extinction explicitly set remaining Species’ populations to zero and record their terminal extinction times before the final Sample/export. Preserve the two-check rule during an ongoing Run; do not wait for checks that can never occur once the Run is frozen.

**Compatibility:** restore truthful Species tracking, terminal counts and export semantics; PLAN 9.3, 12.1 and 15.2.

### UX-03 — High — the Generation axis silently plots a different quantity

**Evidence:** `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/UI/Charts/SimCharts.cs:16-26` replaces each mean Generation with its running maximum (`Mathf.Max(run, v)`). The UI promises mean Generation at `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/UI/Theme/UiText.cs:80`; hover titles still use the actual sample mean at `SimCharts.cs:101-105`.

**Scenario:** an Event kills relatively high-Generation Cells, so mean Generation falls from 12 to 9. The sample is plotted at 12 but its tooltip says 9. Later samples pile onto the same x coordinate. The chart conceals the actual population response.

**Law:** [Mental Model](https://lawsofux.com/mental-model/), [Cognitive Load](https://lawsofux.com/cognitive-load/).

**Smallest remedy:** plot the actual means. Replace the binary-search assumption with a nearest-sample search suitable for non-monotone values, preserve chronological drawing order, and explain briefly that mean Generation can fall. Do not quietly relabel a running maximum as a mean.

**Compatibility:** restore PLAN 9.1/12.2 and DESIGN 5.8. Determinism is unaffected.

### UX-04 — High — hover state survives data changes and can address removed samples

**Evidence:** `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/UI/Charts/ChartElement.cs:64-74` redraws without rebuilding/resetting hover. `SetHover` exits when the index is unchanged at `ChartElement.cs:193-208`. Binding, changing Trait and changing axis refresh data at `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/UI/Charts/SimCharts.cs:47-65,113-117`, without invalidation. `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/Core/StatsRecorder.cs:195-201` halves sample count at decimation. `TraitLineChart.DrawHover` directly indexes `Samples[index]` at `SimCharts.cs:175-177`.

**Scenario:** hovering near the newest sample across the first decimation (about one simulated hour) leaves an index around 3,000 in a list now around 1,800. The next paint can throw an index exception. Before that boundary, a stationary hover or a changed Trait can display old values while the marks represent new values. A newly bound Run can inherit old hover state too.

**Law:** [Mental Model](https://lawsofux.com/mental-model/).

**Smallest remedy:** clear/recompute hover on Bind, Trait/axis changes and every data revision; validate indices in draw/click paths. Refresh the card for a stationary pointer when its sample or Tree row changes.

**Compatibility:** restore DESIGN 5.1 exact hover read-outs; retain deterministic decimation.

### UX-05 — Medium — a population with no Cells is drawn as having zero-valued Traits

**Evidence:** `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/Core/StatsRecorder.cs:138-179` leaves Trait means/SD at zero for an empty Sample. `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/Core/Simulation.cs:379-383` deliberately records that final Sample. `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/UI/Charts/SimCharts.cs:124-137,166-177,180-186` includes it as an ordinary numeric mean and draws it without a zero-count guard.

**Scenario:** extinction produces an apparent instantaneous evolution to size 0, speed 0 and sense range 0—even though those values are outside the allowed ranges. The line can also extend below its plotted y range. This is especially consequential if terminal review is enabled (UX-29).

**Law:** [Mental Model](https://lawsofux.com/mental-model/).

**Smallest remedy:** treat Trait statistics for `Count == 0` as unavailable: break/stop the mean and band, and show “No living Cells” in hover rather than zero. Keep population/death samples and the extinction time.

**Compatibility:** compatible data-display correction; neither document defines an empty population as a Trait mean of zero.

### UX-06 — Medium — large chart hover cards are confined to the chart instead of the window

**Evidence:** `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/UI/Charts/ChartElement.cs:30-42,203-208` parents the hover card inside the chart. `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/UI/Controls/Tooltips.cs:87-98` constrains placement against that parent, not the window, and cannot shrink an over-height card. A Species card can contain twenty plotted Species plus Other/total at `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/UI/Charts/SimCharts.cs:378-390`, while the chart is only 260 reference pixels high at `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/UI/Screens/SpeciesTab.cs:18-24`. Legend content is a later-painted sibling.

**Scenario:** a diverse Run produces a hover card taller than its plot. Placement clamps its top to 4 but leaves its bottom outside the chart, where later legend content can cover it and the enclosing ScrollView can clip it. Exact values become unavailable just when there are many Species to distinguish.

**Law:** [Cognitive Load](https://lawsofux.com/cognitive-load/).

**Smallest remedy:** put chart read-outs in the top overlay, convert coordinates to that overlay and clamp against the window. Give exceptionally tall read-outs a bounded, readable presentation rather than relying on plot overflow.

**Compatibility:** restore DESIGN 4.10/5.1 window-contained hover cards; do not replace Genome colours.

### UX-07 — Medium — modal bring-to-front operations defeat tooltip and warning priority

**Evidence:** `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/UI/Screens/UiRoot.cs:117-125` initially puts tooltips/popups above modal screens and the minimum-size warning. `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/UI/Screens/HelpScreen.cs:92-98` and `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/UI/Screens/ExtinctionOverlay.cs:72-79` then bring their entire scrims above that overlay. `UiRoot.cs:228-234` handles EventDialog differently. The small-window branch merely shows the warning at `UiRoot.cs:281-287`, without restoring its priority.

**Scenario:** hover Help’s icon-only Close, or an extinction action: its explanation is under the scrim/possibly under the opaque card instead of above it. Resize below the supported minimum after opening a frontmost modal: that modal can remain above the supposedly full-window warning, while input has already been disabled.

**Law:** [Paradox of the Active User](https://lawsofux.com/paradox-of-the-active-user/), [Law of Common Region](https://lawsofux.com/law-of-common-region/).

**Smallest remedy:** centralize layer ordering: content, messages, modal scrim/card, minimum-size blocking layer when active, tooltip overlay. Opening one screen must not move it above system warnings or its own explanations.

**Compatibility:** restore DESIGN 2.1, 4.10 and the icon-tooltip decision. No aesthetic change.

### UX-08 — Medium — a dropdown can outlive the panel that owns it

**Evidence:** `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/UI/Screens/UiRoot.cs:32` includes EventsMenu but not EvoDropdown in `ModalOpen`. `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/Input/InputRouter.cs:46-54` therefore still processes Tab/transport shortcuts. `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/UI/Controls/EvoDropdown.cs:80-101` creates an independent, full-window overlay catcher at fixed owner coordinates; there is no owner hide/detach cleanup.

**Scenario:** open the Trait dropdown, then press Tab. The charts collapse and the camera refits, but their detached-looking option list and full-window catcher remain over the World. Selecting an item affects a now-hidden panel. This is not a request for full keyboard accessibility; it is a conflict between existing features.

**Law:** [Law of Common Region](https://lawsofux.com/law-of-common-region/), [Jakob’s Law](https://lawsofux.com/jakobs-law/).

**Smallest remedy:** expose dropdown open state to shortcut gating, and close/reanchor popups whenever their owner is hidden, detached or moved by a layout transition. Escape should continue to close only the top popup.

**Compatibility:** compatible repair of existing popup ownership and Tab behaviour.

### UX-09 — Medium — cancelling a Load-button press can still open a file picker

**Evidence:** `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/UI/Screens/TopBar.cs:84-85` and `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/UI/Screens/SetupScreen.cs:87-90` arm on pointer-down. `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Plugins/WebGL/FileBridge.jslib:17-21,68-71` opens on any document click within three seconds. There is no disarm path. `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/UI/Controls/EvoButton.cs:104-114` only registers the C# Load callback after a valid release inside the button.

**Scenario:** press Load, drag out and release elsewhere on the same canvas. The Unity button correctly cancels its click, but the browser’s canvas click can still consume the arm and open a picker. A later click within the arm window can do the same. No matching Load callback need have been registered.

**Law:** [Jakob’s Law](https://lawsofux.com/jakobs-law/).

**Smallest remedy:** associate browser arming with a specific eligible Load press and release; disarm on cancelled press/capture loss and reject unrelated document clicks. Retain a real browser gesture for the actual picker launch.

**Compatibility:** compatible repair of WebGL file-picker lifecycle, not replacement with an unsupported asynchronous gesture.

### UX-10 — Medium — asynchronous file results are not tied to the Run/request that initiated them

**Evidence:** `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/Interop/FileBridge.cs:74-78,92-96` has a single overwriteable `pending` callback. `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Plugins/WebGL/FileBridge.jslib:34-40,80-85` asynchronously reads/delivers text. `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/App/AppController.cs:193-209` has no pending flag, request token or current-Run check and always starts a valid result. Normal update continues at `AppController.cs:59-67`.

**Scenario:** after choosing a large save and returning to the canvas, a Player begins a new Run or initiates another Load while FileReader is pending. An earlier result can replace the newer context. This code race is established statically; its timing/frequency was not browser-tested.

**Law:** [Mental Model](https://lawsofux.com/mental-model/), [Postel’s Law](https://lawsofux.com/postels-law/).

**Smallest remedy:** give picker/read operations request IDs, reject stale completions after cancellation/context change, and provide a visible pending state with duplicate Loads disabled. Pause/restore the original transport state if the operation is intended to be modal.

**Compatibility:** compatible safeguard around existing Load. Do not claim that the native picker necessarily blocks every browser’s simulation loop.

### UX-11 — Low — file-read failure is indistinguishable from cancellation

**Evidence:** `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Plugins/WebGL/FileBridge.jslib:36-42` sends `''` both for cancellation and FileReader error. `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/App/AppController.cs:198-200` silently returns on that value.

**Scenario:** a selected file becomes unreadable, or an empty file is selected. The chooser closes and nothing explains why the Run did not load. The existing malformed-JSON message is never reached for an empty file.

**Law:** [Postel’s Law](https://lawsofux.com/postels-law/).

**Smallest remedy:** deliver explicit cancelled/read-error/success outcomes, with an ordinary recoverable-error message for a failed or empty selected file. Keep true Cancel silent and preserve the current Run.

**Compatibility:** restore the intent of DESIGN 6.6 load errors; no new file format required.

### UX-12 — Medium — Event placement accepts the blank margins outside the World

**Evidence:** `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/Input/InputRouter.cs:59-70,98-102,109-114` checks the camera viewport, not World coordinates. `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/App/AppController.cs:162-168` stores the unchecked point and clears placement. The fitted World intentionally has margins at `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/Rendering/CameraController.cs:56-69`. The black bands are visible in `/Users/Yumei/Desktop/science L4/Builds/shots/03-run-5min-traits.png`.

**Scenario:** a Player clicks a black band above/below the outlined World. The Event is consumed and logged at an out-of-World centre; a small heat wave/famine may affect nothing. Toxin/Food positions instead clamp to boundaries via `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/Core/Simulation.cs:202-213`, which can create an unexpected edge pile.

**Law:** [Mental Model](https://lawsofux.com/mental-model/), [Law of Common Region](https://lawsofux.com/law-of-common-region/).

**Smallest remedy:** require the Event centre to lie inside World bounds before showing an active preview or committing placement. An invalid click should leave the Event armed with clear feedback.

**Compatibility:** restore PLAN 10’s “click the World,” without forbidding Events whose radius extends beyond a wall.

### UX-13 — Medium — applying a new Whole World Event leaves an old local Event armed

**Evidence:** `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/App/AppController.cs:148-175` sets `Armed` for local placement, but `ApplyWholeWorld` never cancels it. `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/UI/Screens/UiRoot.cs:228-234` opens a new Event dialog without clearing placement. The Apply branch at `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/UI/Screens/EventDialog.cs:182-186` takes that path.

**Scenario:** arm a local heat wave, reconsider and apply a Whole World food bloom instead. The bloom applies but the heat-wave placement banner/crosshair remain. The next World click still places the abandoned heat wave.

**Law:** [Mental Model](https://lawsofux.com/mental-model/).

**Smallest remedy:** define one pending Event at a time; choosing/committing a replacement must explicitly replace or cancel the prior arm, including the Whole World branch. A cancelled replacement dialog may intentionally restore the old arm, but that state should be explicit.

**Compatibility:** compatible cleanup of the existing placement model; no new Event type or rule.

### UX-14 — Medium — “Wait for one to finish” never re-enables the waiting dialog

**Evidence:** `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/UI/Screens/EventDialog.cs:160-175` computes capacity once during Open and sets `place.Disabled`. No update reevaluates it. `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/App/AppController.cs:59-67` continues simulating while the dialog is open, so active Events can expire.

**Scenario:** with eight active Events, open a ninth. The warning says to wait. After an Event finishes, Place is still disabled until the Player closes and reopens the entire dialog.

**Law:** [Mental Model](https://lawsofux.com/mental-model/).

**Smallest remedy:** reevaluate capacity while the dialog is open, update/remove the warning and enable the existing action as soon as placement becomes valid. If paused, explain that Events finish only while the simulation advances.

**Compatibility:** restore truthful feedback around the planned eight-Event cap.

### UX-15 — Medium — a disabled radius still controls Whole World Toxin quantity

**Evidence:** `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/UI/Screens/EventDialog.cs:135-136,176-180` disables Radius when Whole World is checked. `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/Core/Simulation.cs:248-255` nevertheless computes Toxin quantity from `copy.Radius`. Whole World changes only sampling coordinates at `Simulation.cs:204-208`.

**Scenario:** two apparently equivalent Whole World spills release different quantities depending on an earlier radius edit: radius 5 requests 150 particles; radius 20 requests 600; radius 60 requests 1,800 before the global cap. The quantity-controlling field looks irrelevant/disabled.

**Law:** [Mental Model](https://lawsofux.com/mental-model/).

**Smallest remedy:** expose the requested particle quantity for Whole World spills and keep its controlling value editable with an appropriate explanation. Retaining the existing radius-to-quantity formula avoids changing simulation rules.

**Compatibility:** compatible UI clarification of PLAN 10; do not silently change Toxin dosing semantics.

### UX-16 — Medium — simultaneous/nearby Event markers hide each other

**Evidence:** `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/UI/Charts/ChartElement.cs:130-142` gives every Event chip the same y coordinate and positions it solely by time. Each chip’s `userData`/tooltip contains only one Event at `ChartElement.cs:164-178`.

**Scenario:** while paused, place a famine and heat wave. Both have exactly the same timestamp, so the later 20-pixel chip covers the earlier one. On a long Run, distinct nearby times also collapse into overlapping targets. The history looks as though only one intervention occurred.

**Law:** [Fitts’s Law](https://lawsofux.com/fittss-law/), [Cognitive Load](https://lawsofux.com/cognitive-load/).

**Smallest remedy:** group colliding chips into a neutral cluster with a tooltip listing every Event, or stack chips with a collision layout. Preserve exact timestamps, settings and neutral styling.

**Compatibility:** restore accessible Event history within DESIGN 4.17/5.9, without assigning decorative Event colours.

### UX-17 — Medium — the time-based family tree omits all Event markers

**Evidence:** `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/UI/Charts/SimCharts.cs:527-533` uses simulated time but explicitly sets `eventTimeToX = null`, disabling marker layout.

**Scenario:** after an intervention, a Player switches to Tree to see whether a Species split after it. The intervention time disappears from that chart, forcing a cross-tab memory comparison.

**Law:** [Law of Uniform Connectedness](https://lawsofux.com/law-of-uniform-connectedness/), [Cognitive Load](https://lawsofux.com/cognitive-load/).

**Smallest remedy:** use the existing time-to-x Event mapping for Tree and the same collision-safe chips as other time charts.

**Compatibility:** restore PLAN 10 (“every chart”) and DESIGN 5.9 (“every time-based chart”).

### UX-18 — Medium — more Species make the Tree’s labels and hit targets shrink without limit

**Evidence:** `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/UI/Screens/TreeTab.cs:13-15` fixes the chart height at 460. `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/UI/Charts/SimCharts.cs:532,536,540-550,578-582` divides the remaining plot height by all Species records, leaves caption-size labels unchanged, and uses the resulting row height as the hit region.

**Scenario:** at 25 records the 396-reference-pixel plot gives about 15.8 pixels per row—less than the 18-pixel caption line token. At 100 records it gives about 4 pixels. Names overlap and choosing a particular Species becomes very difficult. This is static geometry, not an observed screenshot of a diverse Run.

**Law:** [Fitts’s Law](https://lawsofux.com/fittss-law/), [Law of Proximity](https://lawsofux.com/law-of-proximity/).

**Smallest remedy:** grow chart content height with a minimum readable row height inside the existing scrolling panel; virtualize only if needed later. Keep line widths 1–6 pixels as specified rather than enlarging the data marks.

**Compatibility:** compatible responsive-content correction; DESIGN 5.5 defines one row per Species but does not require a fixed 460-pixel total.

### UX-19 — Low — extinct Tree line width means peak population, not the specified last population

**Evidence:** `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/UI/Charts/SimCharts.cs:564-571` uses `s.PeakPopulation` for extinct Species. DESIGN 5.5 specifies current or last population. `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/Core/SpeciesTracker.cs:7-19` has no stored last nonzero population.

**Scenario:** a formerly large Species that dwindled to a tiny remnant remains a thick extinct branch, while living branches use current counts. A comparison of thickness quietly mixes two quantities.

**Law:** [Law of Similarity](https://lawsofux.com/law-of-similarity/), [Mental Model](https://lawsofux.com/mental-model/).

**Smallest remedy:** either store/use last nonzero population for extinct branches (the specified encoding), or explicitly revise the design and explain that extinct widths use peaks. Do not present the mixed encoding without explanation.

**Compatibility:** using last population restores DESIGN 5.5; keeping peak is a design change requiring approval and truthful copy.

### UX-20 — Medium — an extinct highlighted Species can leave every living Cell dimmed

**Evidence:** `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/App/AppController.cs:142-145` only clears highlight by toggling the same ID. `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/Rendering/WorldView.cs:193-198` dims every nonmatching Cell. `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/UI/Screens/SpeciesTab.cs:38-54` removes extinct Species from the interactive legend. No regular update clears a now-unrepresented highlighted ID.

**Scenario:** highlight a living Species and let it die out. Every remaining Cell is now at 35% opacity, but the selected legend row has disappeared. Escape clears Cell inspection, not this highlight. Recovery requires discovering a different row’s toggle cycle or finding the extinct branch in Tree.

**Law:** [Mental Model](https://lawsofux.com/mental-model/), [Selective Attention](https://lawsofux.com/selective-attention/).

**Smallest remedy:** clear a highlight when it no longer has living members, or retain a visible “Highlighted Species … / Clear” affordance across tabs. Do not leave a filter with no direct reset.

**Compatibility:** compatible safeguard preserving click-to-highlight and the specified 35% dimming.

### UX-21 — Low — Deaths silently becomes a rolling chart while its legend remains all-time

**Evidence:** `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/UI/Charts/SimCharts.cs:394-418` discards display access to all but the newest sixty 10-second bins. `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/UI/Screens/StatsTab.cs:38,83-84` gives no window caption and fills legend values from `TotalDeaths`. Its explanatory copy at `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/UI/Theme/UiText.cs:85,109` does not identify either scope.

**Scenario:** during the planned twenty-minute control experiment, an early mortality spike disappears from the chart. The legend totals still include it, so adding the visible bars no longer reconciles with the adjacent numbers. There is no history navigation.

**Law:** [Mental Model](https://lawsofux.com/mental-model/).

**Smallest remedy:** at minimum label “Last 10 minutes” and “Run totals” explicitly. If the intention is full-Run comparison, add a simple history/window selector using the already retained bins.

**Compatibility:** labels are a compatible correction; history navigation is a compatible extension. Neither PLAN 12.2 nor DESIGN 5.6 explicitly mandates this hidden ten-minute window.

### UX-22 — Medium — the Species key does not identify the plotted Other aggregate

**Evidence:** `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/UI/Charts/SimCharts.cs:335-355` selects the top twenty Species by historical peak and renders the rest as grey Other. `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/UI/Screens/SpeciesTab.cs:20,38-54` builds the only visible key from all living Species and never creates an Other row or explains aggregation.

**Scenario:** a small living Species is outside the historical top twenty. The legend shows its Genome swatch, but its population is inside a grey band with no visible key. The Player searches for a coloured area that is not drawn. Historical extinct layers also have no persistent key, although hover can identify them.

**Law:** [Law of Similarity](https://lawsofux.com/law-of-similarity/), [Cognitive Load](https://lawsofux.com/cognitive-load/).

**Smallest remedy:** keep the required living-Species list, but add an explicit Other swatch/aggregation note and mark living rows that are aggregated in the chart. Historical plotted-layer identification can remain in the repaired hover read-out.

**Compatibility:** compatible extension of DESIGN 5.4; do not remove the top-twenty cap or replace Genome colours.

### UX-23 — Medium — the explanatory surface cannot decode Traits and misdescribes implemented reproduction

**Evidence:** `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/UI/Theme/UiText.cs:33-34,105-111` lists Trait names and generic inheritance, gives every Trait bar the same generic explanation, and describes reproduction only as division. `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/UI/Screens/StatsTab.cs:24,73-81` shows “sexual share” but points it to “A live count taken from the current Run.” Actual partner-only reproduction exists at `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/Core/Simulation.cs:550` and in PLAN 11. Trait units/trade-offs are defined in `/Users/Yumei/Desktop/science L4/PLAN.md:283-292`, not in the interface.

**Scenario:** a school Player sees Diet 0.70 or reproduction threshold 0.80 but cannot find what the direction or unit means by hovering that Trait or opening its Help section. A sexual Cell waits without dividing despite apparently satisfying the Help’s explanation. “Sexual share” is not identified as the share with tendency at least 0.5, rather than the share of births that were sexual.

**Law:** [Paradox of the Active User](https://lawsofux.com/paradox-of-the-active-user/), [Mental Model](https://lawsofux.com/mental-model/).

**Smallest remedy:** add concise Trait-specific definitions/units and trade-offs to the existing hover surface, clarify sexual share’s denominator/threshold, and update Reproduction Help to include partner-dependent reproduction and crossover.

**Compatibility:** compatible explanatory extension satisfying PLAN 12.1’s explanation intent; no extra Trait charts or guided tour. Because DESIGN 8 specifies shared copy, update its copy table and the matching rulebook together rather than allowing three accounts to diverge.

### UX-24 — Low — parent ID has a Generation tooltip

**Evidence:** `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/UI/Screens/CellInspector.cs:62-65` attaches `inspect-generation` to the “parent” readout. That text defines lineage depth at `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/UI/Theme/UiText.cs:105`, not a parent identifier.

**Scenario:** hovering an unfamiliar integer in “parent” tells the Player it is births from a Founder, which is false for an ID.

**Law:** [Mental Model](https://lawsofux.com/mental-model/).

**Smallest remedy:** add a dedicated parent-ID tooltip, including what “none” means. Do not label an identifier with a Generation definition.

**Compatibility:** restore correct, centralized explanatory copy for the existing inspector field.

### UX-25 — Low — double-clicking anywhere on a slider resets it

**Evidence:** `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/UI/Controls/EvoSlider.cs:64,103-109` registers on the whole 32-pixel interaction area and resets whenever `clickCount >= 2`; it does not test the thumb. DESIGN 4.3 reserves reset for double-click on the thumb.

**Scenario:** two quick track clicks while fine-tuning a setting unexpectedly restore its default, potentially altering an experimental setup instead of refining it.

**Law:** [Jakob’s Law](https://lawsofux.com/jakobs-law/).

**Smallest remedy:** restrict reset to thumb hits; repeated track clicks should continue to select the pointed value. Keep the existing convenient thumb reset.

**Compatibility:** restore DESIGN 4.3, without changing the 40-pixel control decision or adding text-entry controls.

### UX-26 — Medium — fractional browser pixel ratios can falsely trigger the minimum-size block

**Evidence:** `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/Interop/FileBridge.cs:42-48` clamps device pixel ratio to at least 1. `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Plugins/WebGL/FileBridge.jslib:64-65` returns the actual browser ratio. `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/UI/Screens/UiRoot.cs:281-282` divides Screen dimensions by the clamped value to infer CSS size.

**Scenario:** on a 1× display at 80% browser zoom, a valid 1600×900 CSS-pixel canvas can have a 1280×720 backing buffer. The code treats the ratio as 1 rather than 0.8. At a 1280×720 CSS window the same mistake reads roughly 1024×576 and blocks a supported window. This is a conditional arithmetic defect, not an observed browser reproduction.

**Law:** [Postel’s Law](https://lawsofux.com/postels-law/).

**Smallest remedy:** allow any finite positive ratio, or measure the canvas CSS bounds directly. Validate 1×, Retina and zoomed browser sizing before shipping.

**Compatibility:** restore the existing CSS-pixel 1280×720 rule; do not lower the minimum or add a UI-scale preference.

### UX-27 — Medium — hiding/showing charts discards the Player’s camera context

**Evidence:** `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/App/AppController.cs:128` sets `fitFrames = 4` for every panel toggle. `AppController.cs:72-73` then calls `Cam.Fit`, which resets centre and zoom at `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/Rendering/CameraController.cs:66-69`.

**Scenario:** zoom into a particular Cell/region, then hide the panel to get more room. Instead of more room around the same subject, the view jumps back to the entire World. Reopening the panel repeats the loss of spatial context.

**Law:** [Mental Model](https://lawsofux.com/mental-model/), [Cognitive Load](https://lawsofux.com/cognitive-load/).

**Smallest remedy:** preserve the camera’s World centre and scale through the viewport change, clamping only as necessary. Keep F as the explicit “fit World” action.

**Compatibility:** compatible behavioural correction; PLAN 12.4 assigns fitting to F and neither document requires panel toggles to reset exploration.

### UX-28 — Low — download notifications assert success that the bridge cannot establish

**Evidence:** `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/App/AppController.cs:179-190,226-228` immediately says “saved”/“Files exported” after void bridge calls. `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Plugins/WebGL/FileBridge.jslib:48-61` only clicks an anchor and never reports delivery. Two CSV anchor clicks are issued by `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/Interop/RunFiles.cs:69-72`.

**Scenario:** browser download permissions suppress a file or require allowing multiple downloads. The app still assures the Player that both files were exported. Believing a Run is safely saved can compound UX-01. This review did not test a particular browser’s policy or assert that normal downloads always fail.

**Law:** [Mental Model](https://lawsofux.com/mental-model/), [Postel’s Law](https://lawsofux.com/postels-law/).

**Smallest remedy:** say “Download requested” and, for CSV, explain that two files may require browser permission. Surface errors the bridge can actually detect; do not promise completed disk writes that browser APIs cannot confirm.

**Compatibility:** compatible truthful-feedback refinement of DESIGN 6.6; retaining two CSV files preserves PLAN 15.2.

### UX-29 — Medium — extinction prevents reviewing or saving the final experiment

**Evidence:** `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/UI/Screens/ExtinctionOverlay.cs:14-18,42-56,72-79` installs a full-window blocking overlay with only Menu, CSV and Restart. `/Users/Yumei/Desktop/science L4/EvoSim/Assets/Scripts/UnityLayer/UI/Screens/UiRoot.cs:32,237-243` treats Extinct as modal but offers no dismissal/review path. The top-bar Save/Screenshot and chart hover are underneath it.

**Scenario:** a famine causes total extinction during an experiment. The Player wants to inspect its Trait trajectory, capture the final charts or save that Run. CSV export is possible, but the final on-screen evidence and full JSON save are inaccessible. Menu/Restart destroy the only retained state. Loading an already extinct save leads into the same trap.

**Law:** [Peak-End Rule](https://lawsofux.com/peak-end-rule/), [Paradox of the Active User](https://lawsofux.com/paradox-of-the-active-user/).

**Smallest remedy:** add a “Review Run” dismissal into a frozen terminal-review mode with charts, Save, CSV and Screenshot available, while Resume/Events stay disabled; preserve a way back to the extinction summary.

**Compatibility:** **design-constrained, not an implementation violation**. The blocking summary with three actions follows DESIGN 6.4. A review action/state is a small proposed screen/behaviour extension requiring approval and a DESIGN/PLAN update, not a demand to remove the extinction summary.

## Deliberate decisions not treated as defects

The audit does **not** recommend replacing the single monospace font; icon-only transport/file controls with labels everywhere; the flat dark palette with gradients/glow; instantaneous state changes with animation; 40-pixel controls with a different density; deterministic Seeds with non-repeatable behaviour; Presets with a complex setup wizard; the eight charted main Traits with ten charts; or CPU Brain simulation with a GPU implementation. Those would reverse supplied choices rather than fix the paths above.

Likewise, this report does not demand a guided tour, mobile support, sub-1280×720 support, a UI-scale setting, or a full keyboard/accessibility expansion. UX-08 repairs an existing shortcut/popup conflict, not a new keyboard-navigation commitment. Improving existing tooltip correctness/visibility is specifically consistent with the chosen discoverability strategy.

Potential preferences such as always-visible speed labels, a permanently labelled toolbar, a mandatory initial tutorial, decorative Species differentiation, automatic pausing for every modal, and automatic checkpoint recovery were not promoted to defects. They would require separate product/design decisions or stronger evidence.

## Coverage and unverified boundaries

| Surface | What was examined | Result / boundary |
|---|---|---|
| Setup | Preset/Seed binding, invalid empty Seed, custom transitions, sliders, Start/Load/Help, screenshot composition | Empty Seed disables Start; Setup is excluded from global Run shortcuts. UX-09/11/23/25 apply. No claim that all Preset combinations were executed. |
| Run/root hit-testing | Ignore/Position ownership, camera viewport, top-bar actions, keyboard gating, panel toggle, size warning | Empty World containers deliberately ignore picking; no evidence for a blanket “World is unclickable” bug. UX-01/07/08/12/26/27 cover concrete exceptions. |
| Inspector | selection lifecycle, eight Trait rows, Brain outputs, parent/Generation/age/energy formatting | Selection clears when its Cell dies. UX-23/24 cover explanatory gaps. No request to add charts for mating Traits. |
| Events | menu, dialog defaults, Whole World, local placement, cap, preview, cancellation, marker history | UX-12 through UX-17; no runtime interaction or browser screenshot was fabricated. |
| Help | section selection, shared copy, modal ownership, wrapping approach | UX-07/23. Existing help screenshot supports the ordinary two-column composition, not all text/aspect-ratio cases. |
| Extinction | state transition, summary actions, Species finalization, final sample, exports | UX-02/05/29. No extinction screenshot exists in the supplied set. |
| Files/WebGL | native gesture bridge, async text result, error/cancel distinction, downloads, naming, template | UX-01/09/10/11/28. JSON parse/version errors preserve the current Run; the file-picker and browser-policy findings were not runtime-tested. |
| Charts | axes, line/band, stacked areas, frequency ranges, Species aggregation, Tree, deaths, chips, hover lifecycle, decimation | UX-02 through UX-06 and UX-15 through UX-22. Sampling decimation and top-twenty aggregation are deliberate constraints, not themselves bugs. |
| Readouts | time formatting, counts, current/mean Generation, rates, sexual share | `Ui.Mmss` handles hours and `Ui.Count` uses separators from 10,000. “Highest generation” currently means highest among living Cells, while the extinction summary uses highest-ever; the wording is ambiguous, so this is not asserted to be an incorrect counter. Field-specific explanatory copy can clarify it under UX-23. |
| Minimum-size/layout | reference scaling, fixed card/panel dimensions, tab/control widths, existing screenshots | No fresh screenshots at 1920×1080, 1366×768 or 1280×720 were produced. No unsupported claim that a 16:9 minimum layout clips. Narrow/tall supported aspect ratios and the long Trait names remain layout-validation gaps, distinct from the proven ratio arithmetic in UX-26. |
| Performance/heat | static cadence and lifecycle inspection only | No measured latency, CPU usage, battery or memory claim. No Doherty-threshold performance violation asserted without measurement. |

## Suggested validation after approved fixes

When the user permits interactive testing, prioritize: discard/replace cancellation; final-extinction CSV/Species consistency; decreasing-Generation samples; a stationary hover through sample decimation and Run replacement; twenty-plus Species hover/Tree layouts; same-time Event collisions; opening a dropdown then Tab; cancelled Load presses and overlapping file reads; Whole World spills after radius edits; cap expiry with its dialog left open; outside-wall clicks; fractional browser zoom; and frozen post-extinction review.

The existing unit tests inspected cover token/copy presence, JSON round-trip, filenames and invalid files. They do not demonstrate these pointer/layer/popup/long-run interaction paths. They were not run during this audit.
