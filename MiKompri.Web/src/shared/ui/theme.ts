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
  colorNeutralForeground1: '#172B3A',
  colorNeutralForeground2: '#526575',
  colorNeutralBackground1: '#FFFFFF',
  colorNeutralBackground2: '#FBFCF9',
  colorNeutralStroke1: '#D8E1E8',
  fontSizeBase300: '16px',
  lineHeightBase300: '24px',
  borderRadiusMedium: '8px',
  colorStrokeFocus2: '#006B68',
  strokeWidthThick: '3px',
}
