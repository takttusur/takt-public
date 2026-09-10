import React from 'react'
import './peopleCarouselCard.css'
import { Link } from 'react-router-dom'
import { PersonCardDesign } from '../types/PersonCardDesign.ts'
import { layoutImagesPath } from '../data/personLayouts.ts'

interface PeopleCarouselCardProps {
    imageSrc?: string
    layout: PersonCardDesign
    name: string
    id: number
}

const PeopleCarouselCard: React.FC<PeopleCarouselCardProps> = ({
    imageSrc,
    layout,
    name,
    id,
}) => {
    const imgUrl = layoutImagesPath + layout.image

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
                    style={{
                        marginTop: layout.memoriesTop,
                        marginLeft: layout.memoriesLeft,
                    }}
                >
                    Воспоминания
                </Link>
                <Link
                    to={`/inmemoria/person/${id}/gallery`}
                    style={{
                        marginTop: layout.galleryTop,
                        marginLeft: layout.galleryLeft,
                    }}
                >
                    Галерея
                </Link>
                <Link
                    to={`/inmemoria/person/${id}/hikes`}
                    style={{
                        marginTop: layout.hikesTop,
                        marginLeft: layout.hikesLeft,
                    }}
                >
                    Маршруты
                </Link>
                <Link
                    to={`/inmemoria/person/${id}/bio`}
                    style={{
                        marginTop: layout.bioTop,
                        marginLeft: layout.bioLeft,
                    }}
                >
                    Жизнь
                </Link>
            </div>
        </div>
    )
}

export default PeopleCarouselCard
