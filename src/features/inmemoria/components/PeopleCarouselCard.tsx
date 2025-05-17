import React from 'react'
import './peopleCarouselCard.css'
import { Link } from 'react-router-dom'

interface PeopleCarouselCardProps {
    imageSrc?: string
    backgroundImage: string
    name: string
    id: number
}

const getRandomNumber = (min: number, max: number): number => {
    return Math.floor(Math.random() * (max - min + 1)) + min
}

const PeopleCarouselCard: React.FC<PeopleCarouselCardProps> = ({
    imageSrc,
    backgroundImage,
    name,
    id,
}) => {
    // Margins for the links
    const m1 = getRandomNumber(1, 4) * 5 // max 20px
    const m2 = getRandomNumber(1, 4) * 5
    const m3 = getRandomNumber(1, 6) * 5 // max 30px
    const m4 = getRandomNumber(1, 6) * 5

    // Horizontal position
    const h1 = getRandomNumber(1, 10) * 5 // max 50px
    const h2 = getRandomNumber(1, 10) * 5
    const h3 = getRandomNumber(1, 10) * 5
    const h4 = getRandomNumber(1, 10) * 5

    const imgUrl = new URL(`../images/${backgroundImage}`, import.meta.url).href

    return (
        <div
            className="inmemoria-people-carousel-card"
            style={{
                backgroundImage: `url(${imgUrl})`,
            }}
        >
            <div className="inmemoria-people-carousel-card-header">
                {imageSrc && (
                    <Link to={`/inmemoria/person/${id}/photo`}>
                        <img src={imageSrc} alt={name} />
                    </Link>
                )}
                <Link
                    to={`/inmemoria/person/${id}/bio`}
                    className="inmemoria-people-carousel-card-name"
                >
                    {name}
                </Link>
            </div>
            <div className="inmemoria-people-carousel-card-links">
                <Link
                    to={`/inmemoria/person/${id}/memories`}
                    style={{ marginTop: m1, marginLeft: h1 }}
                >
                    Воспоминания
                </Link>
                <Link
                    to={`/inmemoria/person/${id}/gallery`}
                    style={{ marginTop: m2, marginLeft: h2 }}
                >
                    Галерея
                </Link>
                <Link
                    to={`/inmemoria/person/${id}/hikes`}
                    style={{ marginTop: m3, marginLeft: h3 }}
                >
                    Маршруты
                </Link>
                <Link
                    to={`/inmemoria/person/${id}/bio`}
                    style={{ marginTop: m4, marginLeft: h4 }}
                >
                    Жизнь
                </Link>
            </div>
        </div>
    )
}

export default PeopleCarouselCard
