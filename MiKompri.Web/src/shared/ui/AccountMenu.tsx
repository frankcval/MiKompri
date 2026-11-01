import { Link } from 'react-router-dom'
import { AppIcon } from './icons/AppIcon'
export function AccountMenu({ className }: { className?: string }) {
  return <Link className={className} to="/profile" aria-label="Tu cuenta y sesión"><AppIcon name="account" /><span>Mi cuenta</span></Link>
}
