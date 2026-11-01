import { currentAccount, accountSessionKey } from '../../../auth/session/accountIdentity'
import ui from '../../../shared/ui/AppSurface.module.css'
import { AppIcon } from '../../../shared/ui/icons/AppIcon'
import { PageHeader } from '../../../shared/ui/PageHeader'
import { Avatar, Button, Card, Field, Input, Select, Spinner, Text } from '@fluentui/react-components'
import { useMsal } from '@azure/msal-react'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { loginRequest } from '../../../auth/msal/msalConfig'
import { createAuthSessionService } from '../../../auth/session/authSessionService'
import { env } from '../../../shared/config/env'
import { fetchMyProfile, updateMyProfile, type MyProfileDto, type ProfileFields } from '../api/profileApi'
import { useRef, useState } from 'react'
import { describeError } from '../../../shared/api/request'

export function ProfilePage() {
  const { instance, accounts } = useMsal()
  const account = currentAccount(instance, accounts)
  const key = accountSessionKey(account)
  const navigate = useNavigate()
  const client = useQueryClient()
  const session = createAuthSessionService(instance)
  const profile = useQuery({ queryKey: ['profile', key], enabled: Boolean(account), retry: false,
    queryFn: async () => fetchMyProfile(env.usersApiBaseUrl, await session.getAccessToken(loginRequest.scopes, account)) })
  const status = (profile.error as { status?: number } | null)?.status
  return <section className={ui.page}>
    <PageHeader icon="account" title="Perfil" description="Tus datos en MiKompri. Elige cómo te ven las personas con las que compartes tus compras." />
    {!account ? <Text>No hay sesión activa.</Text> : <>
      {profile.isPending ? <Spinner label="Cargando perfil" /> : profile.isError ? <div role="alert">
        <p>{status === 401 ? 'Vuelve a iniciar sesión para consultar tu perfil.' : describeError(profile.error)}</p>
        {status === 401 ? <Button onClick={() => void session.login(loginRequest.scopes)}>Volver a iniciar sesión</Button>
          : <Button onClick={() => void profile.refetch()}>Reintentar</Button>}
      </div> : profile.data ? <ProfileEditor key={key + ':' + profile.data.profileVersion} profile={profile.data}
        onSave={async fields => {
          const saved = await updateMyProfile(env.usersApiBaseUrl, await session.getAccessToken(loginRequest.scopes, account), fields, profile.data.profileVersion ?? 0)
          client.setQueryData(['profile', key], saved)
        }} /> : <Text>Perfil vacío.</Text>}
      <div className={ui.actions}><Button onClick={() => void profile.refetch()}>Actualizar datos</Button>
        <Button icon={<AppIcon name="logout" />} onClick={async () => { await session.logout(account); navigate('/login', { replace: true }) }}>Cerrar sesión</Button></div>
      <p>Para recuperar el acceso, utiliza la recuperación de tu proveedor original. Cambiar de proveedor no recupera tu cuenta existente.</p>
    </>}
  </section>
}

function ProfileEditor({ profile, onSave }: { profile: MyProfileDto; onSave: (fields: ProfileFields) => Promise<void> }) {
  const [values, setValues] = useState<ProfileFields>({ displayName: profile.displayName, username: profile.username ?? null,
    givenName: profile.givenName ?? null, familyName: profile.familyName ?? null,
    preferredLanguage: profile.preferredLanguage ?? null, defaultListSort: profile.defaultListSort ?? 'newest' })
  const [busy, setBusy] = useState(false)
  const pending = useRef(false)
  const [error, setError] = useState('')
  const set = (field: keyof ProfileFields, value: string) => setValues(before => ({ ...before, [field]: value || null }))
  return <Card className={ui.panel}>
    <Avatar name={values.displayName || 'Usuario'} aria-label={'Iniciales de ' + (values.displayName || 'Usuario')} />
    <h2>Tus datos</h2>
    <p>Tu username es un nombre público de MiKompri; no cambia el identificador con el que inicias sesión.</p>
    <form className={ui.stack} onSubmit={async event => {
      event.preventDefault()
      if (pending.current) return
      pending.current = true; setBusy(true); setError('')
      try { await onSave(values) } catch (cause) { setError(describeError(cause)) }
      finally { pending.current = false; setBusy(false) }
    }}>
      <Field label="Nombre visible" required><Input required maxLength={100} value={values.displayName ?? ''} onChange={(_, data) => set('displayName', data.value)} /></Field>
      <Field label="Username" hint="Opcional: 3–30 letras minúsculas, números o guion bajo; empieza por una letra."><Input maxLength={30} value={values.username ?? ''} onChange={(_, data) => set('username', data.value)} /></Field>
      <Field label="Nombre"><Input maxLength={100} value={values.givenName ?? ''} onChange={(_, data) => set('givenName', data.value)} /></Field>
      <Field label="Apellidos"><Input maxLength={100} value={values.familyName ?? ''} onChange={(_, data) => set('familyName', data.value)} /></Field>
      <Field label="Idioma preferido"><Select value={values.preferredLanguage ?? ''} onChange={(_, data) => set('preferredLanguage', data.value)}>
        <option value="">Predeterminado</option><option value="es">Español</option>
      </Select></Field>
      <Field label="Orden inicial de listas"><Select value={values.defaultListSort} onChange={(_, data) => set('defaultListSort', data.value)}>
        <option value="newest">Más recientes</option><option value="oldest">Más antiguas</option><option value="updated">Última modificación</option><option value="name">Nombre</option>
      </Select></Field>
      <p>Email de contacto: {profile.email || 'Sin indicar'}</p>
      {error ? <p role="alert">{error}</p> : null}
      <Button type="submit" appearance="primary" disabled={busy}>{busy ? 'Guardando…' : 'Guardar perfil'}</Button>
    </form>
  </Card>
}
