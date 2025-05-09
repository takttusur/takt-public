import React from 'react'
import './peopleCarouselCard.css'

interface PeopleCarouselCardProps {
    imageSrc?: string
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
    name,
    url,
    memoriesUrl,
    galleryUrl,
    tracksUrl,
    lifeUrl,
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

    return (
        <div className="inmemoria-people-carousel-card">
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
                <a href={memoriesUrl} style={{ marginTop: m1, marginLeft: h1 }}>
                    Воспоминания
                </a>
                <a href={galleryUrl} style={{ marginTop: m2, marginLeft: h2 }}>
                    Галерея
                </a>
                <a href={tracksUrl} style={{ marginTop: m3, marginLeft: h3 }}>
                    Маршруты
                </a>
                <a href={lifeUrl} style={{ marginTop: m4, marginLeft: h4 }}>
                    Жизнь
                </a>
            </div>
        </div>
    )
}

export default PeopleCarouselCard
