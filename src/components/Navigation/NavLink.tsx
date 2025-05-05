import { Link } from 'react-router-dom'
import './NavLink.css'

export interface INavLinkProps {
    label: string
    link: string
}

export default function NavLink(goto: INavLinkProps): JSX.Element {
    return (
        <Link to={goto.link} title={goto.label}>
            <div className="nav-link">{goto.label}</div>
        </Link>
    )
}
