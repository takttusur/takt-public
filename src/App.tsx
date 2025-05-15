import './App.css'

import { useRoutes } from 'react-router-dom'
import rootRoutes from './routes/'
import YandexMetrikaCounter from './components/Common/YandexMetrikaCounter.tsx'
import EnvironmentService from './services/EnvironmentService'

const routes = rootRoutes.getRoutes()

export default function App(): JSX.Element {
    const routesElement = useRoutes(routes)

    return (
        <>
            {routesElement}
            <YandexMetrikaCounter
                isEnabled={EnvironmentService.YandexMetrikaEnabled}
                id={EnvironmentService.YandexMetrikaId}
            ></YandexMetrikaCounter>
        </>
    )
}
