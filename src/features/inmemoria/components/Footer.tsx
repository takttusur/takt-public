import React from 'react'

interface FooterProps {
    // No props needed for now
}

const Footer: React.FC<FooterProps> = () => {
    return (
        <div id="foot">
            <a href="about.php">О проекте</a>
            <a style={{ marginLeft: '50px' }} href="http://takt.tomsk.ru">
                ТАКТ
            </a>
            <span style={{ float: 'right', marginRight: '50px' }}>
                Все вопросы, предложения или замечания отправляйте на адрес{' '}
                <a href="mailto:inmemoria@ngs.ru">inmemoria@ngs.ru</a>
            </span>
        </div>
    )
}

export default Footer
