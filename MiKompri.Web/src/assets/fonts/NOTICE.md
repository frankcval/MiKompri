# Local webfonts

Original typeface: Source Sans 3, Copyright 2010–2024 Adobe, under the SIL Open Font License 1.1. The unmodified TTF originals and complete [license](LICENSE.md) are retained here.

The derived WOFF2 files are named **MiKompri Sans**, including their internal font names, to respect the reserved name “Source”. They retain the original copyright, attribution and license metadata and are distributed under the same OFL. This derivative name does not claim authorship of the original design.

`scripts/build-fonts.py` generates the full and Latin subsets. CSS loads the small Latin files for the interface and the full files for other supported scripts; missing glyphs retain the system font fallback. No external font service is required.

Reference: [OFL FAQ, modification and reserved names](https://openfontlicense.org/ofl-faq/).
