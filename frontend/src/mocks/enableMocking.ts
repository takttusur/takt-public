export async function tryEnableMocking() {
    if (import.meta.env.VITE_MSW_ENABLED !== 'true') return

    const { worker } = await import('./browser')
    await worker.start({
        onUnhandledRequest: 'bypass',
        serviceWorker: {
            url: `${import.meta.env.BASE_URL || '/'}mockServiceWorker.js`,
        },
    })
}
