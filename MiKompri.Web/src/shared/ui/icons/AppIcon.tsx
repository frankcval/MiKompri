import { memo } from 'react'
import { iconRegistry, type IconName } from './iconRegistry'
import s from './IconAction.module.css'
export type AppIconProps = { name: IconName; size?: 20 | 24; selected?: boolean; className?: string }
export const AppIcon = memo(function AppIcon({ name, size = 20, selected = false, className }: AppIconProps) {
  const pair = iconRegistry[name]
  if (!pair) throw new Error('Icono de aplicación no registrado.')
  const Glyph = pair[selected ? 1 : 0]
  return <Glyph size={size} className={[s.icon, className].filter(Boolean).join(' ')} aria-hidden="true" focusable="false" />
})
