import React from 'react'
import './footer.css'

interface FooterProps {
    // No props needed for now
}

const Footer: React.FC<FooterProps> = () => {
    return (
        <div className="inmemoria-footer">
            <div className="inmemoria-footer-links">
                <a href="about.php">О проекте</a>
                <a href="http://takt.tomsk.ru">ТАКТ</a>
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
