# Image to Arduino TFT Converter

A lightweight image conversion algorithm that converts **paletted images into optimized C++ data for Arduino TFT displays**.

Instead of storing and drawing an entire bitmap pixel-by-pixel, the converter analyzes the image and represents it as a collection of **colored rectangles (boxes)**. These boxes can then be rendered using `fillRect()`, significantly reducing the amount of drawing operations required on resource-constrained microcontrollers.

## Features

* 🖼️ Supports **paletted / indexed-color images**
* 🎨 Automatically analyzes the image color palette
* 📦 Converts images into **rectangle-based rendering instructions**
* ⚡ Optimized for relatively fast rendering on Arduino TFT displays
* 💾 Generates compact C++ data suitable for storing in program memory
* 🔢 Supports up to **256 palette colors**
* 🧩 Layered rendering algorithm allows later colors to overwrite earlier ones
* 📐 Designed for small TFT graphics and embedded devices
* 🚫 Avoids storing a full RGB bitmap in Arduino memory

## How It Works

A traditional bitmap renderer may need to draw every pixel individually:

```cpp
tft.drawPixel(x, y, color);
```

For a large image, this can result in thousands of drawing operations.

This converter instead analyzes the image and finds rectangular areas that can be rendered with a single operation:

```cpp
tft.fillRect(x, y, width, height, color);
```

For example, an image containing a large blue background with a smaller red shape could be represented as:

```text
┌──────────────────────────────┐
│                              │
│          BLUE                │
│                              │
│        ┌──────────┐          │
│        │   RED    │          │
│        └──────────┘          │
│                              │
└──────────────────────────────┘
```

Instead of drawing every pixel, the Arduino can draw a small number of rectangles.

## Layered Box Rendering

The main optimization is that the algorithm does not simply search for rectangles containing identical colors.

Colors are ordered by their frequency in the image, with the most common color becoming the first layer.

The first layer is used as the background:

```cpp
fillRect(0, 0, imageWidth, imageHeight, backgroundColor);
```

Subsequent layers are then rendered on top.

A rectangle belonging to a later layer is allowed to cover pixels that will eventually be replaced by an even later layer.

For example:

```text
Layer 0 ───────── Background
Layer 1 ───────── Large shapes
Layer 2 ───────── Details
Layer 3 ───────── Fine details
```

This makes it possible to create **large rectangles even when those rectangles contain pixels that will later be overwritten**.

The final rendered image is therefore produced by the complete sequence of boxes rather than by requiring every individual box to contain only its final color.

## Rendering Model

The generated Arduino code follows the basic model:

```cpp
for (const auto& box : imageBoxes)
{
    tft.fillRect(
        offsetX + box.x * scaleX,
        offsetY + box.y * scaleY,
        box.width * scaleX,
        box.height * scaleY,
        palette[box.color]
    );
}
```

The image can also be positioned and scaled during rendering.

## Box Structure

Each generated rectangle contains:

```cpp
struct ImageBox
{
    uint8_t x;
    uint8_t y;
    uint8_t width;
    uint8_t height;
    uint8_t color;
};
```

Where:

| Field    | Description         |
| -------- | ------------------- |
| `x`      | Horizontal position |
| `y`      | Vertical position   |
| `width`  | Rectangle width     |
| `height` | Rectangle height    |
| `color`  | Palette index       |

Using palette indices instead of storing a color inside every rectangle keeps the generated data compact.

## Palette

The image palette can be exported as RGB565 values, which are directly usable by most Arduino TFT libraries:

```cpp
const uint16_t palette[] PROGMEM =
{
    0x0000,
    0xFFFF,
    0xF800,
    0x07E0,
    0x001F
};
```

This works particularly well with displays using RGB565 color.

## Why Boxes?

Arduino-class microcontrollers have significantly less memory and processing power than desktop systems.

A conventional RGB image can require a considerable amount of memory:

```text
480 × 320 × 2 bytes
≈ 300 KB
```

A rectangle representation can be much smaller when the image contains large areas of similar colors.

For example:

```text
Bitmap:
307,200 bytes

Box representation:
A few hundred or a few thousand boxes
```

The actual size depends heavily on the image complexity.

## Performance

The goal of the algorithm is to reduce the number of TFT drawing operations.

Instead of:

```text
100,000+ pixels
        ↓
100,000+ drawing operations
```

the converter attempts to produce:

```text
Image
  ↓
Palette analysis
  ↓
Layer generation
  ↓
Rectangle detection
  ↓
Hundreds of fillRect() operations
```

This can make rendering considerably faster on Arduino TFT displays.

The effectiveness depends on the image. Graphics containing **large flat-color regions** generally compress extremely well, while photographs and highly detailed images may produce many rectangles.

## Best Use Cases

This approach works particularly well for:

* Splash screens
* Game interfaces
* Icons
* Menus
* Logos
* UI elements
* Cards
* Board-game graphics
* Backgrounds
* Pixel-art style graphics
* Images with relatively few colors

It is less suitable for:

* Photographs
* Complex gradients
* Highly detailed textures
* Images with hundreds of small color regions

## Image Size

The generated box format uses `uint8_t` coordinates and dimensions, making it particularly compact and suitable for small embedded graphics.

The intended image range is approximately:

```text
256 × 256 pixels
```

For images larger than this, a different coordinate representation would be required.

## Arduino Compatibility

The generated output is intended for Arduino-compatible TFT libraries such as:

* `Adafruit_GFX`
* `MCUFRIEND_kbv`

The generated drawing code can be adapted to other TFT libraries that provide rectangle rendering functionality.

## Typical Workflow

```text
        Input Image
             │
             ▼
      Indexed / Palette
          Analysis
             │
             ▼
       Color Frequency
          Analysis
             │
             ▼
       Layer Generation
             │
             ▼
       Rectangle Detection
             │
             ▼
       C++ / Arduino Data
             │
             ▼
       TFT fillRect()
             │
             ▼
       Rendered Image
```

## Example

An input image:

```text
┌──────────────────────┐
│██████████████████████│
│██████████████████████│
│██████░░░░░░░░████████│
│██████░░████░░████████│
│██████░░░░░░░░████████│
│██████████████████████│
└──────────────────────┘
```

Can become something similar to:

```cpp
boxes[] =
{
    // background
    { 0, 0, 20, 6, 0 },

    // main shape
    { 6, 2, 8, 3, 1 },

    // detail
    { 8, 3, 4, 1, 0 }
};
```

The exact output depends on the palette and rectangle optimization.

## Design Philosophy

The converter is built around a simple principle:

> **Use the TFT hardware to draw large areas instead of making the Arduino process every pixel individually.**

The algorithm trades some preprocessing complexity on the PC side for significantly less work on the Arduino side.

This makes it especially useful for embedded graphics where:

* RAM is limited
* Flash storage is available
* TFT drawing operations are relatively expensive
* Images are relatively small
* Images contain large areas of solid color

---

**MONETRIX Image-to-Arduino TFT Converter**

Convert images into compact, layered rectangle data for fast rendering on Arduino TFT displays.
