# StrobePages

**StrobePages** is a Unity URP renderer feature that creates a flipbook-style
page-turning post-processing effect. It captures frames at a fixed interval
and animates them as if a page is flipping over the previous frame.

# System requirements

- Unity 6000.0 or later
- Universal Render Pipeline (URP)

# How to install

Install the StrobePages package (`jp.keijiro.strobe-pages`) from the "Keijiro"
scoped registry in Package Manager. Follow [these instructions] to add the
registry to your project.

[these instructions]:
  https://gist.github.com/keijiro/f8c7e8ff29bfe63d86b888901b82644c

# How to use

1. Add **StrobePagesRendererFeature** to your URP Renderer Asset.
2. Add **StrobePagesController** to a Camera.

# Parameters

**Page Interval** controls how often a new page is captured. Shorter values
flip more frequently.

**Page Stiffness** controls the easing of the page flipping animation. Higher
values make the page turn feel stiffer and more sudden.

**Motion Blur** adds motion blur to the page flipping animation by increasing
the temporal spread of samples.

**Sample Count** controls the number of samples used for motion blur.

**Shade Width** and **Shade Strength** add shading on the base page.

**Opacity** blends the effect over the source image.
