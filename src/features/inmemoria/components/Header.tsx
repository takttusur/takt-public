import React from 'react'

interface HeaderProps {
    // No props needed for now
}

const Header: React.FC<HeaderProps> = () => {
    return (
        <a href="http://inmemoria.tusur.ru/">
            <div id="logo"></div>
        </a>
    )
}

export default Header
