import { useState } from 'react'
import taktLogo from '../../assets/takt.svg'
import { HamburgerIcon, CloseIcon } from './icons'
import NavLink, { INavLinkProps } from './NavLink'
import { Link } from 'react-router-dom'
import rootRoutes from '../../routes'
import { ToNavLinkProps } from '../../utils/NavigationUtils.ts'
import './MainMenu.css'

const links: INavLinkProps[] = [
    {
        link: 'http://vk.com/takt_tusur',
        label: 'Группа VK',
    },
    ToNavLinkProps(rootRoutes.Equipment),
    ToNavLinkProps(rootRoutes.CurrentEvents),
]

export default function MainMenu(): JSX.Element {
    const [isOpen, setIsOpen] = useState(false)
    const toggleMenu = (): void => setIsOpen(!isOpen)

    return (
        <>
            <div className="main-menu-container">
                <div className="main-menu-flex">
                    <button
                        className="menu-button"
                        aria-label="Open Menu"
                        onClick={toggleMenu}
                    >
                        {isOpen ? <CloseIcon /> : <HamburgerIcon />}
                    </button>
                    <div className="menu-stack">
                        <Link to="/Home" title="Перейти главную страницу">
                            <img
                                src={taktLogo}
                                alt="Takt Logo"
                                width="50"
                                height="50"
                            />
                        </Link>
                        <nav className="nav-links">
                            {links.map((link) => (
                                <NavLink
                                    key={link.label}
                                    {...{
                                        label: link.label,
                                        link: link.link,
                                    }}
                                />
                            ))}
                        </nav>
                    </div>
                </div>

                <div className={`mobile-menu ${isOpen ? 'open' : ''}`}>
                    <nav className="mobile-nav-stack">
                        <NavLink {...ToNavLinkProps(rootRoutes.Home)} />
                        {links.map((link) => (
                            <NavLink
                                key={link.label}
                                {...{
                                    label: link.label,
                                    link: link.link,
                                }}
                            />
                        ))}
                    </nav>
                </div>
            </div>
            <div className="spacer" />
        </>
    )
}
