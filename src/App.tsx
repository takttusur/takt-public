import './App.css'

import { useRoutes } from 'react-router-dom'
import rootRoutes from './routes/'
import YandexMetrikaCounter from './components/Common/YandexMetrikaCounter.tsx'
import EnvironmentService from './services/EnvironmentService'
import { Provider } from 'react-redux'
import { JSX } from 'react'
import { store } from './store'

const routes = rootRoutes.getRoutes()

export default function App(): JSX.Element {
    const routesElement = useRoutes(routes)
    return (
        <Provider store={store}>
            {routesElement}
            <YandexMetrikaCounter
                isEnabled={EnvironmentService.YandexMetrikaEnabled}
                id={EnvironmentService.YandexMetrikaId}
            ></YandexMetrikaCounter>
        </Provider>
    )
}
