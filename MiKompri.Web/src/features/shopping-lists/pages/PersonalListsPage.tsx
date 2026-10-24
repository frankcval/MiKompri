import { Button, Card, CardHeader, Field, Input, Spinner, Text } from '@fluentui/react-components'
import { useMsal } from '@azure/msal-react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { loginRequest } from '../../../auth/msal/msalConfig'
import { createAuthSessionService, executeAuthenticatedWrite } from '../../../auth/session/authSessionService'
import { env } from '../../../shared/config/env'
import { getCatalogProducts } from '../../product-catalog/api/productCatalogApi'
import { ListItemsPanel } from '../components/ListItemsPanel'
import { addPersonalListItem, createPersonalList, getMyPersonalLists } from '../api/shoppingListsApi'

export function PersonalListsPage() {
  const { instance, accounts } = useMsal()
  const account = accounts[0]
  const authSession = createAuthSessionService(instance)
  const queryClient = useQueryClient()
  const navigate = useNavigate()
  const { listId } = useParams<{ listId: string }>()
  const [name, setName] = useState('')
  const listQueryKey = ['personal-lists', account?.homeAccountId]

  const listsQuery = useQuery({
    queryKey: listQueryKey,
    enabled: Boolean(account),
    queryFn: async () => {
      const token = await authSession.getAccessToken(loginRequest.scopes, account)
      return getMyPersonalLists(env.shoppingListApiBaseUrl, token)
    },
  })
  // Only a list returned for the current account can become a write target.
  const selectedList = listsQuery.isSuccess ? listsQuery.data.find((list) => list.id === listId) : undefined
  const productsQuery = useQuery({
    queryKey: ['catalog-products', account?.homeAccountId],
    enabled: Boolean(account && selectedList),
    retry: false,
    queryFn: async () => {
      const token = await authSession.getAccessToken(loginRequest.scopes, account)
      return getCatalogProducts(env.productCatalogApiBaseUrl, token)
    },
  })

  const createListMutation = useMutation({
    mutationFn: () => executeAuthenticatedWrite(
      (token) => createPersonalList(env.shoppingListApiBaseUrl, token, { name: name.trim() }),
      () => authSession.getAccessToken(loginRequest.scopes, account),
      async () => authSession.login(loginRequest.scopes),
    ),
    onSuccess: async (createdId) => {
      setName('')
      await queryClient.invalidateQueries({ queryKey: listQueryKey })
      navigate(`/personal-lists/${createdId}`)
    },
  })

  if (!account) return <Text>No hay sesión activa.</Text>

  return (
    <section style={{ padding: '1.5rem', display: 'grid', gap: '1rem', minWidth: 0 }}>
      <h1>Listas personales</h1>
      <Card>
        <CardHeader header={<Text weight="semibold">Crear lista personal</Text>} />
        <form style={{ display: 'grid', gap: '0.75rem' }} onSubmit={(event) => {
          event.preventDefault()
          if (name.trim() && !createListMutation.isPending) createListMutation.mutate()
        }}>
          <Field label="Nombre de lista" required>
            <Input required value={name} disabled={createListMutation.isPending} onChange={(_, data) => setName(data.value)} />
          </Field>
          <Button type="submit" appearance="primary" disabled={!name.trim() || createListMutation.isPending}>
            {createListMutation.isPending ? 'Creando lista…' : 'Crear lista'}
          </Button>
          {createListMutation.isError ? <p role="alert">{createListMutation.error.message}</p> : null}
        </form>
      </Card>

      {listsQuery.isLoading ? <Spinner label="Cargando listas" /> : listsQuery.isError ? (
        <p role="alert">No fue posible cargar listas personales.</p>
      ) : (
        <section aria-label="Mis listas personales" style={{ display: 'grid', gap: '0.5rem', minWidth: 0 }}>
          {listsQuery.data?.map((list) => (
            <Card key={list.id}>
              <Link to={`/personal-lists/${list.id}`} aria-current={list.id === listId ? 'page' : undefined}
                style={{ padding: '0.5rem', overflowWrap: 'anywhere', fontWeight: list.id === listId ? 700 : 400 }}>
                {list.name}{list.id === listId ? ' (seleccionada)' : ''}
              </Link>
            </Card>
          ))}
          {listsQuery.data?.length === 0 ? <Text>Sin listas personales todavía. Crea una lista para empezar.</Text> : null}
        </section>
      )}

      {!listId ? <p>Selecciona una lista para ver sus ítems y agregar productos.</p> : null}
      {listId && listsQuery.isSuccess && !selectedList ? (
        <p role="alert">Esta lista no está disponible en tus listas personales. <Link to="/personal-lists">Volver a mis listas</Link></p>
      ) : null}
      {selectedList ? (
        <section aria-label={`Detalle de ${selectedList.name}`} style={{ display: 'grid', gap: '1rem', minWidth: 0 }}>
          <h2 style={{ overflowWrap: 'anywhere' }}>{selectedList.name}</h2>
          <Link to="/personal-lists">Volver a mis listas</Link>
          {selectedList.items?.length ? (
            <ul style={{ paddingInlineStart: '1.25rem' }}>
              {selectedList.items.map((item) => <li key={item.id} style={{ overflowWrap: 'anywhere' }}>
                {item.productName} · Cantidad: {item.quantity} · {item.isPurchased ? 'Comprado' : 'Pendiente'}
              </li>)}
            </ul>
          ) : <p>Esta lista todavía no tiene ítems.</p>}
          <Card>
            <CardHeader header={<h3 style={{ margin: 0, overflowWrap: 'anywhere' }}>Agregar ítem a {selectedList.name}</h3>} />
            {productsQuery.isPending ? <Spinner label="Cargando productos" /> : productsQuery.isError ? (
              <div role="alert">No fue posible cargar los productos del catálogo. <Button onClick={() => void productsQuery.refetch()}>Reintentar</Button></div>
            ) : productsQuery.data?.some((product) => product.isActive !== false) ? (
              <ListItemsPanel key={`${account.homeAccountId}:${selectedList.id}`} products={productsQuery.data}
                onAddItem={async (item) => {
                  await executeAuthenticatedWrite(
                    (token) => addPersonalListItem(env.shoppingListApiBaseUrl, token, selectedList.id, item),
                    () => authSession.getAccessToken(loginRequest.scopes, account),
                    async () => authSession.login(loginRequest.scopes),
                  )
                  await queryClient.invalidateQueries({ queryKey: listQueryKey })
                }}
              />
            ) : <p>No hay productos disponibles en el catálogo. Se necesita un producto del catálogo para agregar un ítem.</p>}
          </Card>
        </section>
      ) : null}
    </section>
  )
}
