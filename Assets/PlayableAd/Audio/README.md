# Playable Ad Audio Placeholders

This project currently contains no third-party or externally sourced audio files.

When an AudioClip field is empty and `useProceduralPlaceholders` is enabled,
`ProceduralAudioLibrary` creates a mono 22.05 kHz placeholder clip once during
game initialization. These synthesized placeholders are original project output
and contain no sampled recordings or third-party audio material.

Replace any field in `AudioFeedbackSettings` with an authored AudioClip to bypass
the matching placeholder. Record the final asset author, source URL, license, and
modifications in this file when production audio is imported.
