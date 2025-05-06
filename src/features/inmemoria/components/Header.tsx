import React from 'react'
import Epigraph from './Epigraph.tsx'
import Search from './Search.tsx'
import './header.css'

interface HeaderProps {
    // No props needed for now
}

const Header: React.FC<HeaderProps> = () => {
    return (
        <header className="inmemoria-header">
            <div className="inmemoria-header-line">
                <div className="inmemoria-header-title">
                    <a href="http://inmemoria.tusur.ru/">
                        <div className="inmemoria-header-logo"></div>
                    </a>
                </div>
                <div className="inmemoria-header-epigraph">
                    <Epigraph />
                </div>
            </div>
            <div className="inmemoria-header-line">
                <div className="inmemoria-header-search">
                    <Search />
                </div>
                <div className="inmemoria-header-link">
                    <a href="list.php">Те, кого с нами нет...</a>
                </div>
            </div>
        </header>
    )
}

export default Header
