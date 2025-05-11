import React from 'react'
import './peopleCarouselCard.css'
import { Link } from 'react-router-dom'

interface PeopleCarouselCardProps {
    imageSrc?: string
    backgroundImage: string
    name: string
    url: string
    memoriesUrl: string
    galleryUrl: string
    tracksUrl: string
    lifeUrl: string
}

const getRandomNumber = (min: number, max: number): number => {
    return Math.floor(Math.random() * (max - min + 1)) + min
}

const PeopleCarouselCard: React.FC<PeopleCarouselCardProps> = ({
    imageSrc,
    backgroundImage,
    name,
    url,
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
                    <a href={url}>
                        <img src={imageSrc} alt={name} />
                    </a>
                )}
                <a href={url} className="inmemoria-people-carousel-card-name">
                    {name}
                </a>
            </div>
            <div className="inmemoria-people-carousel-card-links">
                <Link to="person" style={{ marginTop: m1, marginLeft: h1 }}>
                    Воспоминания
                </Link>
                <Link to="person" style={{ marginTop: m2, marginLeft: h2 }}>
                    Галерея
                </Link>
                <Link to="person" style={{ marginTop: m3, marginLeft: h3 }}>
                    Маршруты
                </Link>
                <Link to="person" style={{ marginTop: m4, marginLeft: h4 }}>
                    Жизнь
                </Link>
            </div>
        </div>
    )
}

export default PeopleCarouselCard
