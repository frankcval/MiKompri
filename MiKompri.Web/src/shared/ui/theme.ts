import { createLightTheme, type Theme } from '@fluentui/react-components'

const brandColors = {
  10: '#001b1a', 20: '#00312f', 30: '#00423f', 40: '#00514e',
  50: '#005956', 60: '#00615e', 70: '#006663', 80: '#006B68',
  90: '#25817e', 100: '#479693', 110: '#69aba8', 120: '#8bc0bd',
  130: '#add5d2', 140: '#cfe9e6', 150: '#e5f4f2', 160: '#f4fbfa',
}

export const appTheme: Theme = {
  ...createLightTheme(brandColors),
  fontFamilyBase: "'MiKompri Sans', 'Segoe UI', sans-serif",
  fontFamilyNumeric: "'MiKompri Sans', 'Segoe UI', sans-serif",
  colorNeutralForeground1: 'var(--mk-ink)',
  colorNeutralForeground2: 'var(--mk-muted)',
  colorNeutralBackground1: 'var(--mk-white)',
  colorNeutralBackground2: 'var(--mk-fridge)',
  colorNeutralStroke1: 'var(--mk-control)',
  fontSizeBase300: '16px',
  lineHeightBase300: '24px',
  borderRadiusMedium: '8px',
  colorStrokeFocus2: 'var(--mk-teal)',
  strokeWidthThick: '3px',
}
