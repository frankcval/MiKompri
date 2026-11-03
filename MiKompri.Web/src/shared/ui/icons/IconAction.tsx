import { forwardRef } from 'react'
import { Button, Tooltip, type ButtonProps } from '@fluentui/react-components'
import { Link, type LinkProps } from 'react-router-dom'
import { AppIcon } from './AppIcon'
import type { IconName } from './iconRegistry'
import s from './IconAction.module.css'
type Meaning = { name: IconName; label: string; tooltip?: string }
function checkedLabel(label: string) {
  if (!label.trim()) throw new Error('La acción con icono requiere un nombre accesible.')
  return label
}
export type IconActionProps = Meaning & Omit<Extract<ButtonProps, { as?: 'button' }>, 'as' | 'icon' | 'children' | 'aria-label'> & { busy?: boolean }
export const IconAction = forwardRef<HTMLButtonElement, IconActionProps>(function IconAction({ name, label, tooltip, busy = false, disabled, className, ...props }, ref) {
  const text = checkedLabel(label)
  return <Tooltip content={tooltip || text} relationship={tooltip && tooltip !== text ? 'description' : 'label'}>
    <Button {...props} as="button" ref={ref} className={[s.action, className].filter(Boolean).join(' ')} icon={<AppIcon name={name} />}
      aria-label={text} aria-busy={busy || undefined} disabled={disabled || busy} />
  </Tooltip>
})
export const IconActionLink = forwardRef<HTMLAnchorElement, Meaning & Omit<LinkProps, 'children' | 'aria-label'>>(function IconActionLink({ name, label, tooltip, className, ...props }, ref) {
  const text = checkedLabel(label)
  return <Tooltip content={tooltip || text} relationship={tooltip && tooltip !== text ? 'description' : 'label'}>
    <Link {...props} ref={ref} className={[s.action, s.link, className].filter(Boolean).join(' ')} aria-label={text}><AppIcon name={name} /></Link>
  </Tooltip>
})
