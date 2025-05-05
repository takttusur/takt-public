import { Navigate, RouteObject } from 'react-router-dom'
import HomePage from './HomePage.tsx'
import MealEditorPage from './MealEditorPage.tsx'
import CurrentEventsPage from './CurrentEvents.tsx'
import EquipmentPage from './Eqiupment.tsx'
import InMemoriaPageRoute from './InMemoriaPage.tsx'
import { IAppRoutingMap } from './common/IAppRoutingMap.ts'
import { IAppRoute } from './common/IAppRoute.ts'

class RootAppRoutingMap extends Object implements IAppRoutingMap {
    public readonly Home: IAppRoute = {
        path: '/home',
        element: <HomePage />,
        title: 'Главная',
    }
    public readonly Root: IAppRoute = {
        path: '/',
        element: <Navigate to={this.Home.path} replace={true} />,
        title: '',
    }
    public readonly MealEditor: IAppRoute = {
        path: '/mealEditor',
        element: <MealEditorPage />,
        title: 'Редактор раскладок',
    }
    public readonly CurrentEvents: IAppRoute = {
        path: '/currentEvents',
        element: <CurrentEventsPage />,
        title: 'События',
    }
    public readonly Equipment: IAppRoute = {
        path: '/equipment',
        element: <EquipmentPage />,
        title: 'Снаряжение',
    }

    public readonly InMemoria: IAppRoute = {
        path: '/inmemoria',
        element: <InMemoriaPageRoute />,
        title: 'Inmemoria',
    }

    public getRoutes(): RouteObject[] {
        const keys = Object.keys(this) as Array<keyof RootAppRoutingMap>

        return keys
            .map((k) => this[k] as IAppRoute)
            .map((r) => ({
                element: r.element,
                path: r.path,
            }))
    }
}

const rootAppRoutingMap: RootAppRoutingMap = new RootAppRoutingMap()
export default rootAppRoutingMap
