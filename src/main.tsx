import React from 'react'
import ReactDOM from 'react-dom/client'
import App from './App'
import './index.css'
import { HashRouter } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import LogRocket from 'logrocket'
import setupLogRocketReact from 'logrocket-react'
import EnvironmentService from './services/EnvironmentService'

const queryClient = new QueryClient()
if (
    EnvironmentService.LogrocketEnabled &&
    EnvironmentService.LogrocketId?.length > 0
) {
    LogRocket.init(EnvironmentService.LogrocketId)
    setupLogRocketReact(LogRocket)
}

ReactDOM.createRoot(document.getElementById('root')!).render(
    <React.StrictMode>
        <QueryClientProvider client={queryClient}>
            <HashRouter>
                <App />
            </HashRouter>
        </QueryClientProvider>
    </React.StrictMode>
)
