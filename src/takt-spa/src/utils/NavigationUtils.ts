import { IAppRoute } from '../routes/common/IAppRoute.ts'
import { INavLinkProps } from '../components/Navigation/NavLink.tsx'

export function ToNavLinkProps(route: IAppRoute): INavLinkProps {
    return {
        label: route.title,
        link: route.path,
    }
}
