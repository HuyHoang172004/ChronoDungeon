# M6.5 - Puzzle Tutorialization review

## Implemented

- Added `PuzzleTutorialGuide` to Echo Chamber with a short world-space instruction.
- Safe entry: `STEP ON A TO BEGIN`.
- First-loop failure guidance: `HOLD A - REWIND - FOLLOW YOUR GHOST`.
- Ghost-loop guidance: `GHOST HOLDS A - ACTIVATE B`.
- Solved state: `TIME LINK COMPLETE` with the Ghost marker hidden.
- The guide resets with the puzzle and uses a small marker instead of a text wall.

## Verification

- `PuzzleTutorialPlayModeChecks`: **4/4 PASS**.
- Covered safe entry, first-loop guidance, Ghost-loop complementary hint and solved completion message.
- Compile clean.
- Final Console after stopping and clearing frame-step diagnostics: **0 errors, 0 warnings**.
- Scene saved, test runner removed, Play Mode exited.

Touch/Android, localization and final readability across aspect ratios remain manual follow-up.
