from pathlib import Path
import re
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / "Assets" / "ArtSource" / "Characters"
OUTPUT = ROOT / "Assets" / "Screenshots" / "CharacterStateCatalog"
OUTPUT.mkdir(parents=True, exist_ok=True)

FONT_PATHS = [
    Path("C:/Windows/Fonts/segoeui.ttf"),
    Path("C:/Windows/Fonts/arial.ttf"),
]


def font(size):
    for path in FONT_PATHS:
        if path.exists():
            return ImageFont.truetype(str(path), size)
    return ImageFont.load_default()


TITLE = font(31)
LABEL = font(21)
SMALL = font(16)
BG = (20, 25, 34, 255)
PANEL = (35, 43, 56, 255)
BORDER = (62, 197, 255, 255)
TEXT = (240, 245, 250, 255)
MUTED = (165, 178, 194, 255)


def state_name(path):
    match = re.match(r"^CD_(?:Player|Enemy|Boss|NPC)_[^_]+_(.+?)_(?:4Dir(?:_4Frame)?|v\d+)", path.stem)
    return match.group(1) if match else path.stem


def character_groups():
    groups = []
    for directory in sorted({p.parent for p in SOURCE.rglob("*.png")}):
        images = sorted(directory.glob("*.png"), key=lambda p: state_name(p).lower())
        if images:
            groups.append((directory.relative_to(SOURCE), images))
    return groups


def checker(width, height, size=16):
    out = Image.new("RGBA", (width, height), (48, 55, 66, 255))
    draw = ImageDraw.Draw(out)
    for y in range(0, height, size):
        for x in range(0, width, size):
            if ((x // size) + (y // size)) % 2:
                draw.rectangle((x, y, x + size - 1, y + size - 1), fill=(57, 65, 78, 255))
    return out


def make_character_sheet(relative, images):
    columns = min(3, len(images))
    rows = (len(images) + columns - 1) // columns
    cell_w, cell_h = 470, 350
    title_h = 80
    sheet = Image.new("RGBA", (columns * cell_w, title_h + rows * cell_h), BG)
    draw = ImageDraw.Draw(sheet)
    character = relative.parts[-1]
    category = " / ".join(relative.parts[:-1])
    draw.text((24, 14), character, font=TITLE, fill=TEXT)
    draw.text((24, 50), category, font=SMALL, fill=MUTED)

    for index, path in enumerate(images):
        col, row = index % columns, index // columns
        x0, y0 = col * cell_w, title_h + row * cell_h
        draw.rounded_rectangle((x0 + 10, y0 + 8, x0 + cell_w - 10, y0 + cell_h - 10), 12, fill=PANEL, outline=BORDER, width=2)
        draw.text((x0 + 24, y0 + 18), state_name(path), font=LABEL, fill=TEXT)
        draw.text((x0 + 24, y0 + 48), "source sprite sheet", font=SMALL, fill=MUTED)
        area_x, area_y, area_w, area_h = x0 + 24, y0 + 80, cell_w - 48, cell_h - 105
        background = checker(area_w, area_h)
        with Image.open(path) as raw:
            image = raw.convert("RGBA")
            image.thumbnail((area_w - 12, area_h - 12), Image.Resampling.LANCZOS)
            px = (area_w - image.width) // 2
            py = (area_h - image.height) // 2
            background.alpha_composite(image, (px, py))
        sheet.alpha_composite(background, (area_x, area_y))

    safe = "_".join(relative.parts)
    destination = OUTPUT / f"{safe}_states.png"
    sheet.convert("RGB").save(destination, quality=94)
    return destination


def make_overview(title, sheets, filename):
    columns = 2
    thumb_w, thumb_h = 720, 430
    rows = (len(sheets) + columns - 1) // columns
    canvas = Image.new("RGB", (columns * thumb_w, 70 + rows * thumb_h), BG[:3])
    draw = ImageDraw.Draw(canvas)
    draw.text((24, 15), title, font=TITLE, fill=TEXT[:3])
    for index, sheet_path in enumerate(sheets):
        col, row = index % columns, index // columns
        x0, y0 = col * thumb_w, 70 + row * thumb_h
        with Image.open(sheet_path) as image:
            image = image.convert("RGB")
            image.thumbnail((thumb_w - 24, thumb_h - 24), Image.Resampling.LANCZOS)
            canvas.paste(image, (x0 + (thumb_w - image.width) // 2, y0 + (thumb_h - image.height) // 2))
    destination = OUTPUT / filename
    canvas.save(destination, quality=92)
    return destination


groups = character_groups()
generated = [(relative, make_character_sheet(relative, images)) for relative, images in groups]
categories = {
    "Player": [path for relative, path in generated if relative.parts[0] == "Player"],
    "Enemies Maps 01-04": [path for relative, path in generated if relative.parts[0] == "Enemies" and relative.parts[1] in {"Map01", "Map02", "Map03", "Map04"}],
    "Enemies Maps 05-08": [path for relative, path in generated if relative.parts[0] == "Enemies" and relative.parts[1] in {"Map05", "Map06", "Map07", "Map08"}],
    "Bosses": [path for relative, path in generated if relative.parts[0] == "Bosses"],
    "NPC": [path for relative, path in generated if relative.parts[0] == "NPC"],
}
for title, paths in categories.items():
    if paths:
        make_overview(title, paths, "Overview_" + re.sub(r"[^A-Za-z0-9]+", "_", title) + ".jpg")

print(f"Generated {len(generated)} character sheets and {len(categories)} overview sheets in {OUTPUT}")
