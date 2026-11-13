# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Users

Primary users are a science teacher and Grade 8 classmates (with little or no technical background) evaluating an L4 natural selection assignment, alongside the student author demonstrating evolutionary concepts in class or via a hosted browser link. Their job is to observe, test, and understand how environmental Pressures drive Trait changes, differential survival, and speciation over generations without needing prior technical or gaming literacy.

## Product Purpose

Divergence is an interactive 2D natural selection simulation running in the browser (Unity 6 WebGL). Cells with heritable Traits and small neural-network Brains live in a continuous aquatic World with Food, drifting Toxin, and spatially varying Temperature. Success means a non-technical viewer can immediately read what is happening in the World, trigger environmental Events, inspect individual Cells, and trace resulting Trait shifts and Species splits on the charts.

## Positioning

Unlike scripted animations or grid-based toy games, Divergence runs a deterministic, continuous-space population model where ten heritable Traits and a 100-weight neural Brain co-evolve under spatial Pressures, automatically detecting Species divergence and narrating real-time evolutionary milestones in plain classroom language.

## Operating Context

Used in browser windows (1280×720 minimum, 1920×1080 reference) on classroom laptops, desktops, and projectors via GitHub Pages or a local static server, accompanied by a one-page printable A4 PDF rulebook (`Rulebook/Divergence-Rulebook.pdf`).

## Capabilities and Constraints

- Pure deterministic C# simulation core (`Evo.Core`) with zero `UnityEngine` dependencies, fixed timestep (`Dt = 0.05s`), seeded RNG streams, JSON Run snapshot save/load compatibility (FormatVersion 1), and CSV/PNG exports.
- Strict domain vocabulary defined in `CONTEXT.md`: `Run`, `World`, `Seed`, `Player`, `Cell`, `Founder`, `Trait`, `Brain`, `Genome`, `Mutation`, `Generation`, `Trait frequency`, `Species`, `Pressure`, `Temperature`, `Food`, `Toxin`, `Event`, `Preset`. Never use `gene`, `tile`, `square`, `organism`, or `creature` in interface copy.
- Self-explanatory HUD without multi-step walkthrough popups: controls, Presets, Events, Cell Inspector, live Field Notes narration, and charts must explain themselves directly on screen and on hover.

## Brand Commitments

- **Name & subtitle**: `Divergence` — a natural selection simulation.
- **Visual material**: Apple-inspired **Liquid Glass** floating interface (translucent frosted panels with real-time blurred/refracted World backdrop, continuous rounded capsule/card geometry, directional specular meniscus rims, crisp humanist sans UI typography paired with tabular monospace telemetry) suspended over a full-bleed, lifelike bioluminescent aquatic World.

## Evidence on Hand

- Deterministic simulation model and 160+ EditMode unit/regression tests in `EvoSim/Assets/Tests/EditMode/`.
- Headless experiment runner (`tools/experiment.sh`) and scripted Metal screenshot verification harnesses (`DevHarness`, `UxSmokeTour`).
- Canonical copy table (`UiText.cs`) shared across tooltips, Help screen, and `Rulebook/Divergence-Rulebook.pdf`.

## Product Principles

1. **Life leads from frame one.** The living World fills the entire viewport — including behind the Setup screen — so viewers see swimming Cells, Pressures, and adaptation before reading a single chart.
2. **Weightless optical instruments.** Interface panels float like translucent Liquid Glass lenses over the water rather than boxing the World into rigid developer chrome.
3. **Classroom clarity without hand-holding.** Plain labels, visual Preset cards, an always-accessible Event dock, and live evolutionary Field Notes make cause and effect obvious to first-time viewers without pop-up tutorials.
4. **Scientific honesty.** Every visual cue, colour shift, chart line, and narrated note reflects real state in the deterministic model, preserving repeatability and saved-Run integrity.
