import './App.css'
import Footer from './components/Footer'

import { useRoutes, useLocation } from 'react-router-dom'
import rootRoutes from './routes/'
import YandexMetrikaCounter from './components/Common/YandexMetrikaCounter.tsx'
import EnvironmentService from './services/EnvironmentService'

const routes = rootRoutes.getRoutes()

export default function App(): JSX.Element {
    const routesElement = useRoutes(routes)
    const location = useLocation()

    // Check if the current route is /inmemoria
    const isInMemoriaRoute = location.pathname === '/inmemoria'

    return (
        <>
            {routesElement}
            {!isInMemoriaRoute && <Footer />}
            <YandexMetrikaCounter
                isEnabled={EnvironmentService.YandexMetrikaEnabled}
                id={EnvironmentService.YandexMetrikaId}
            ></YandexMetrikaCounter>
        </>
    )
}
