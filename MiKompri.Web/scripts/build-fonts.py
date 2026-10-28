"""Regenerate local OFL webfonts. Development-only dependency: fonttools[woff]."""
from pathlib import Path
from fontTools.ttLib import TTFont
from fontTools import subset

fonts = Path(__file__).resolve().parents[1] / 'src/assets/fonts'
coverage = 'U+0000-024F,U+1E00-1EFF,U+2000-206F,U+20A0-20CF,U+2100-21FF,U+2600-27BF'
for weight, style in [(400, 'Regular'), (600, 'Semibold'), (700, 'Bold')]:
    for latin in [False, True]:
        font = TTFont(fonts / f'source-sans-3-{weight}.ttf', recalcTimestamp=False)
        if latin:
            options = subset.Options()
            options.name_IDs = ['*']  # Retain copyright, license and attribution.
            worker = subset.Subsetter(options=options)
            worker.populate(unicodes=subset.parse_unicodes(coverage))
            worker.subset(font)
        # Source is a Reserved Font Name. Give all derived webfonts a distinct
        # internal family, retaining the original copyright and OFL metadata.
        names = {1: 'MiKompri Sans', 2: style, 3: f'MiKompriSans-{style}-008',
                 4: f'MiKompri Sans {style}', 6: f'MiKompriSans-{style}',
                 16: 'MiKompri Sans', 17: style, 18: f'MiKompri Sans {style}',
                 20: f'MiKompriSans-{style}', 21: 'MiKompri Sans', 22: style,
                 25: 'MiKompriSans'}
        for name in font['name'].names:
            if name.nameID in names:
                name.string = names[name.nameID]
        font.flavor = 'woff2'
        output = fonts / f'mikompri-sans-{weight}{"-latin" if latin else ""}.woff2'
        font.save(output)
        print(f'{output.name}: {output.stat().st_size} bytes')
