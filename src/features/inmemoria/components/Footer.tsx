import React from 'react'
import './footer.css'
import { Link, useLocation } from 'react-router-dom'

const Footer: React.FC = () => {
    const location = useLocation()
    const isMainPage = location.pathname === '/inmemoria'

    return (
        <div className="inmemoria-footer">
            <div className="inmemoria-footer-links">
                {!isMainPage && <Link to="">Главная</Link>}
                <Link to="about">О проекте</Link>
                <a href="https://vk.com/takt_tusur">ТАКТ</a>
            </div>
            <div className="inmemoria-footer-feedback">
                <span>
                    Все вопросы, предложения или замечания отправляйте на адрес{' '}
                    <a href="mailto:inmemoria@ngs.ru">inmemoria@ngs.ru</a>
                </span>
            </div>
        </div>
    )
}

export default Footer
