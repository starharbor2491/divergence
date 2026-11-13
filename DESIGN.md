# Divergence — Interface & Visual Notes

This document summarizes the visual tokens, typography, rendering layers, and screen layout used in `Evo.UnityLayer`. Core simulation rules live in `PLAN.md` and domain terms live in `CONTEXT.md`.

## Visual System

- **Ground & Glass**: Deep neutral water ground (`#0C1117`) across both the HTML loader and Unity camera so there is no startup flash or letterbox seam. Floating HUD surfaces use a real-time 1/3-resolution blurred and bevel-refracted backdrop (`GlassBackdrop` + `LiquidGlassBackdrop.shader`) paired with subtle neutral hairline borders (`rgba(255, 255, 255, 0.09)`–`0.24`) and solid `#141920` modals over a `#05070A` (`76%`) scrim.
- **Actions & Semantic Colour**: Primary actions use off-white (`#EDEFF3` with `#0C0F14` text); destructive actions (`Discard` / `Replace`) use `#D64D4D`; chart means, slider fills, and founder comparisons use muted sage (`#6EC9A8`). World particles and chart series share semantic hues for food (`#68CC9B`), toxin (`#D95B82`), cold (`#3F8AE0`), and heat (`#DE6459`).
- **Typography**: `Plus Jakarta Sans` (`Regular`, `Medium`, `SemiBold`, `Bold`) in sentence case for prose, labels, and controls; `JetBrains Mono` (`Regular`, `Medium`, `Bold`) for live counters, timecodes, seeds, slider readouts, and chart axes.
- **Radii & Spacing**: 4 px spacing grid; outer panels `16 px`, cards/dropdowns `12 px`, controls/buttons `8 px`.

## World & HUD Layout

- **Setup**: Runs a live background preview simulation behind a two-column floating panel with four starting presets (*Mild world*, *Cold gradient*, *Toxic basin*, *Rapid evolution*), seed input, five range-annotated sliders with default ticks, and *Start run* / *Load file* / *How it works*.
- **Run HUD**: Top bar with transport, `1x–10x` speed selector, live metrics, thermal overlay toggle, view reset, snapshot/CSV/PNG actions, guide, and charts toggle; bottom *Events* dock centered in the open viewport; top-left *CellInspector* with live cell portrait, energy bar, founder-referenced trait bars, and neural outputs; top-right *Field log* summarizing placed events, trait shifts, and speciation splits; right-hand *SidePanel* (*Traits*, *Species*, *Tree*, *Stats*) with sexagesimal time ticks, bounded hover readouts, and centered lineage trees.
