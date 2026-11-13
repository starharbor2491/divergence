# Product Notes — Divergence

## Overview

Divergence is a browser-based 2D natural selection simulation built with Unity 6 (WebGL) for an L4 science assignment. Cells with heritable traits and small neural-network brains move through a continuous aquatic world containing food pellets, drifting toxin particles, and spatially varying temperature.

## Audience

Science teachers and Grade 8 classmates exploring natural selection, adaptation, and speciation in a browser without needing prior technical background.

## Core Mechanics & Constraints

- Deterministic C# simulation core (`Evo.Core`) with zero `UnityEngine` dependencies, fixed timestep (`Dt = 0.05 s`), seeded RNG streams, JSON snapshot save/load (version 1), and CSV/PNG exports.
- Ten heritable traits plus a 100-weight neural brain per cell, automatic genetic-distance species tracking, five interactive environmental events (heat wave, ice age, famine, food bloom, toxin spill), and live plain-English field notes.
- Minimum supported viewport: `1280 × 720` (`1920 × 1080` reference).
