import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  const hmrClientPort = env.VITE_HMR_CLIENT_PORT
    ? Number(env.VITE_HMR_CLIENT_PORT)
    : undefined

  return {
    plugins: [react()],
    server: {
      hmr: hmrClientPort
        ? {
            clientPort: hmrClientPort,
            host: env.VITE_HMR_HOST || undefined,
          }
        : undefined,
      proxy: env.VITE_API_PROXY_TARGET
        ? {
            '/api': {
              target: env.VITE_API_PROXY_TARGET,
              changeOrigin: true,
              secure: false,
            },
          }
        : undefined,
    },
  }
})
