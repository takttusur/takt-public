import { Link } from 'react-router-dom'
import './NavLink.css'
import { FC } from 'react'

export interface INavLinkProps {
    label: string
    link: string
}

const NavLink: FC<INavLinkProps> = (goto) => {
    return (
        <Link to={goto.link} title={goto.label}>
            <div className="nav-link">{goto.label}</div>
        </Link>
    )
}
export default NavLink
