import { Navigate, RouteObject } from 'react-router-dom'
import HomePage from './HomePage.tsx'
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

    public readonly InMemoria: IAppRoute = {
        path: '/inmemoria/*',
        element: <InMemoriaPageRoute />,
        title: 'Inmemoria',
    }

    public getRoutes(): RouteObject[] {
        const keys = Object.getOwnPropertyNames(this).filter(
            (name) => Object.getOwnPropertyDescriptor(this, name)?.enumerable
        ) as Array<keyof RootAppRoutingMap>

        return keys
            .map((k) => {
                const route = this[k]
                if (this.isAppRoute(route)) {
                    return route
                }
                return null
            })
            .filter((r): r is IAppRoute => r !== null)
            .map(
                (r): RouteObject => ({
                    element: r.element,
                    path: r.path,
                })
            )
    }

    private isAppRoute(route: unknown): route is IAppRoute {
        return (
            typeof route === 'object' &&
            route !== null &&
            'path' in route &&
            'element' in route &&
            'title' in route
        )
    }
}

const rootAppRoutingMap: RootAppRoutingMap = new RootAppRoutingMap()
export default rootAppRoutingMap
