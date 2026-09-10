import React from 'react'
import Epigraph from './Epigraph.tsx'
import Search from './Search.tsx'
import './header.css'
import { Link } from 'react-router-dom'

const Header: React.FC = () => {
    return (
        <header className="inmemoria-header">
            <div className="inmemoria-header-line">
                <Link to="/inmemoria" className="inmemoria-header-title">
                    <div className="inmemoria-header-logo"></div>
                </Link>
                <div className="inmemoria-header-epigraph">
                    <Epigraph />
                </div>
            </div>
            <div className="inmemoria-header-line">
                <div className="inmemoria-header-search">
                    <Search />
                </div>
                <div className="inmemoria-header-link">
                    <Link to="/inmemoria/list">Те, кого с нами нет...</Link>
                </div>
            </div>
        </header>
    )
}

export default Header
