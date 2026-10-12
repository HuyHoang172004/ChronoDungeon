from pathlib import Path
from PIL import Image


ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / "Assets/ArtSource/Characters/Player/SideView/Player_SideView_CombatAtlas_v01.png"
OUTPUT = ROOT / "Assets/Art/Characters/Player/SideView/CombatFrames"
ROWS = ("Jump", "Attack", "HitDeath", "DashCast")


def main() -> None:
    atlas = Image.open(SOURCE).convert("RGBA")
    for row, state in enumerate(ROWS):
        folder = OUTPUT / state
        folder.mkdir(parents=True, exist_ok=True)
        for column in range(8):
            # Image generation may trim a few transparent edge pixels, so derive
            # cell boundaries proportionally instead of requiring exact dimensions.
            frame = atlas.crop((
                round(column * atlas.width / 8),
                round(row * atlas.height / 4),
                round((column + 1) * atlas.width / 8),
                round((row + 1) * atlas.height / 4),
            ))
            frame.save(folder / f"Player_{state}_{column:02d}.png")


if __name__ == "__main__":
    main()
