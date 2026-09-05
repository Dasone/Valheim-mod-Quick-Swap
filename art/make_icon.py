"""Generates the Thunderstore icon: 256x256 PNG with a 5px black border.

Rendered at 4x and downscaled, which is what keeps the diagonals and the text
edges clean at this size. Run: python art/make_icon.py
"""
from pathlib import Path

from PIL import Image, ImageChops, ImageDraw, ImageFilter, ImageFont

SIZE = 256
BORDER = 5
SS = 4  # supersample factor

S = SIZE * SS
B = BORDER * SS

BG_TOP = (74, 74, 82)
BG_BOTTOM = (40, 40, 46)
SLOT_FILL = (34, 34, 39)
SLOT_EDGE = (128, 117, 96)
SLOT_NUMBER = (150, 141, 122)
AMBER = (255, 210, 74)
WHITE = (255, 255, 255)
BLACK = (0, 0, 0)

FONT_TITLE = r"C:\Windows\Fonts\ariblk.ttf"
FONT_SLOT = r"C:\Windows\Fonts\ariblk.ttf"


def gradient() -> Image.Image:
    base = Image.new("RGB", (S, S), BG_BOTTOM)
    draw = ImageDraw.Draw(base)
    for y in range(S):
        t = y / (S - 1)
        draw.line(
            [(0, y), (S, y)],
            fill=tuple(round(a + (b - a) * t) for a, b in zip(BG_TOP, BG_BOTTOM)),
        )
    return base


def vignette(image: Image.Image) -> Image.Image:
    """Darkens the corners so the border reads as a frame rather than a stripe."""
    mask = Image.new("L", (S, S), 0)
    ImageDraw.Draw(mask).ellipse(
        [-S * 0.42, -S * 0.42, S * 1.42, S * 1.42], fill=255
    )
    return Image.composite(image, Image.new("RGB", (S, S), (26, 26, 30)), mask)


def fit_font(path: str, text: str, target_width: int, start: int) -> ImageFont.FreeTypeFont:
    size = start
    while size > 8:
        font = ImageFont.truetype(path, size)
        if font.getlength(text) <= target_width:
            return font
        size -= 2
    return ImageFont.truetype(path, 8)


def slot(draw: ImageDraw.ImageDraw, box, label: str, anchored: bool) -> None:
    x0, y0, x1, y1 = box
    draw.rounded_rectangle(box, radius=14 * SS, fill=SLOT_FILL, outline=SLOT_EDGE, width=3 * SS)

    font = ImageFont.truetype(FONT_SLOT, 31 * SS)
    draw.text(
        ((x0 + x1) / 2, (y0 + y1) / 2 - 2 * SS),
        label,
        font=font,
        fill=SLOT_NUMBER,
        anchor="mm",
    )

    if anchored:
        # The same marker the mod draws under the anchored slot in game.
        inset = 7 * SS
        bar_height = 5 * SS
        draw.rounded_rectangle(
            [x0 + inset, y1 - inset - bar_height, x1 - inset, y1 - inset],
            radius=bar_height / 2,
            fill=AMBER,
        )


def arrow(
    draw: ImageDraw.ImageDraw,
    cx: float,
    cy: float,
    length: float,
    pointing_right: bool,
    shaft: float = 8 * SS,
    head_w: float = 17 * SS,
    head_h: float = 22 * SS,
    fill=None,
) -> None:
    fill = fill or WHITE
    half = length / 2
    tip = cx + half if pointing_right else cx - half
    back = cx - half if pointing_right else cx + half
    neck = tip - head_w if pointing_right else tip + head_w

    draw.rounded_rectangle(
        [min(back, neck), cy - shaft / 2, max(back, neck), cy + shaft / 2],
        radius=shaft / 2,
        fill=fill,
    )
    draw.polygon(
        [(tip, cy), (neck, cy - head_h / 2), (neck, cy + head_h / 2)],
        fill=fill,
    )


def glow(image: Image.Image, cx: float, cy: float, radius: float, colour) -> Image.Image:
    """Soft warm light behind the anchored slot, so the eye lands there first."""
    layer = Image.new("RGB", (S, S), (0, 0, 0))
    ImageDraw.Draw(layer).ellipse(
        [cx - radius, cy - radius, cx + radius, cy + radius], fill=colour
    )
    return ImageChops.add(image, layer.filter(ImageFilter.GaussianBlur(radius * 0.55)))


def build() -> Image.Image:
    image = vignette(gradient())

    slot_size = 70 * SS
    slot_top = 26 * SS
    gap = 48 * SS
    left = (S - (slot_size * 2 + gap)) / 2

    right_slot_cx = left + slot_size * 1.5 + gap
    image = glow(image, right_slot_cx, slot_top + slot_size / 2, 60 * SS, (74, 56, 16))
    draw = ImageDraw.Draw(image)

    slot(draw, [left, slot_top, left + slot_size, slot_top + slot_size], "1", anchored=False)
    slot(
        draw,
        [left + slot_size + gap, slot_top, left + slot_size * 2 + gap, slot_top + slot_size],
        "8",
        anchored=True,
    )

    mid_x = S / 2
    mid_y = slot_top + slot_size / 2
    arrow(draw, mid_x, mid_y - 13 * SS, gap - 4 * SS, pointing_right=True)
    arrow(draw, mid_x, mid_y + 13 * SS, gap - 4 * SS, pointing_right=False)

    lines = ["QUICK", "SWAP"]
    inner_width = S - 2 * B - 12 * SS
    font = min(
        (fit_font(FONT_TITLE, line, inner_width, 62 * SS) for line in lines),
        key=lambda f: f.size,
    )
    for line, cy in zip(lines, (152 * SS, 207 * SS)):
        draw.text(
            (S / 2, cy),
            line,
            font=font,
            fill=WHITE,
            anchor="mm",
            stroke_width=4 * SS,
            stroke_fill=BLACK,
        )

    image = image.resize((SIZE, SIZE), Image.LANCZOS)

    # Border last, so nothing above can bleed into it.
    ImageDraw.Draw(image).rectangle(
        [0, 0, SIZE - 1, SIZE - 1], outline=BLACK, width=BORDER
    )
    return image


def build_marker(width: int = 96, height: int = 64) -> Image.Image:
    """The in-game anchor badge: the icon's swap arrows alone, white on transparent.

    White so the Image component can tint it to whatever the marker colour is set to;
    transparent so the badge's backing plate shows through around it.
    """
    w, h = width * SS, height * SS
    glyph = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(glyph)

    length = w * 0.80
    gap = h * 0.25
    for cy, right in ((h / 2 - gap, True), (h / 2 + gap, False)):
        arrow(draw, w / 2, cy, length, right,
              shaft=h * 0.20, head_w=w * 0.28, head_h=h * 0.46, fill=WHITE)

    return glyph.resize((width, height), Image.LANCZOS)


if __name__ == "__main__":
    root = Path(__file__).resolve().parent.parent

    icon = root / "Thunderstore" / "icon.png"
    icon.parent.mkdir(parents=True, exist_ok=True)
    build().save(icon)
    print(f"wrote {icon}")

    marker = root / "QuickSwap" / "Assets" / "anchor-arrows.png"
    marker.parent.mkdir(parents=True, exist_ok=True)
    build_marker().save(marker)
    print(f"wrote {marker}")
