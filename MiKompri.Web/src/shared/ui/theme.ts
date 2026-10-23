import { createLightTheme, webLightTheme, type Theme } from '@fluentui/react-components'

const brandColors = {
  10: '#050812',
  20: '#0e1a3a',
  30: '#173166',
  40: '#1f4692',
  50: '#265abe',
  60: '#2e6dea',
  70: '#4f85f0',
  80: '#6f9cf5',
  90: '#90b4fa',
  100: '#b1cbff',
  110: '#d1e1ff',
  120: '#edf4ff',
  130: '#f8fbff',
  140: '#ffffff',
  150: '#ffffff',
  160: '#ffffff',
}

export const appTheme: Theme = {
  ...webLightTheme,
  ...createLightTheme(brandColors),
  colorStrokeFocus2: '#173166',
  strokeWidthThick: '3px',
  colorBrandForeground1: '#173166',
  colorBrandBackground: '#2e6dea',
}
