// Playwright QA helper; run before using google-direct.html on localhost:55412.
// Captures one already validated Google diagnostic request in memory, then
// sends its credential to three fixed loopback API endpoints expecting 401.
// No arbitrary URL, token output, persistence or change to API trust.
async (page) => {
  const publish = result => page.evaluate(value => { window.qaGoogleBusinessNegative = value }, result)
  await publish({ state: 'waiting-for-google' })
  page.on('response', async response => {
    const url = new URL(response.url())
    if (url.origin !== 'http://localhost:55412' || url.pathname !== '/spike/google-negative-observation'
      || response.request().method() !== 'POST') return
    try {
      const proof = await response.json()
      if (!response.ok() || proof.googleSignatureAndClientContractValid !== true) {
        await publish({ state: 'google-proof-rejected' })
        return
      }
      const authorization = await response.request().headerValue('authorization')
      if (!authorization?.startsWith('Bearer ')) throw new Error('Missing diagnostic credential')
      const endpoints = [
        ['Users', 'http://localhost:28082/api/v1/users/me'],
        ['ShoppingList', 'http://localhost:28080/api/v1/purchaselists/mine'],
        ['ProductCatalog', 'http://localhost:28083/api/v1/catalog-products'],
      ]
      const results = await Promise.all(endpoints.map(async ([api, endpoint]) => {
        const result = await page.request.get(endpoint, { headers: { Authorization: authorization }, timeout: 10000 })
        const status = result.status()
        await result.dispose()
        return { api, status }
      }))
      await publish({ state: 'complete', realGoogleSignatureValid: true,
        results, allRejected: results.every(result => result.status === 401) })
    } catch {
      await publish({ state: 'inconclusive' })
    }
  })
  return { state: 'attached', tokenPersistence: false }
}
