import React from 'react'
import './peopleCarouselCard.css'

interface PeopleCarouselCardProps {
    imageSrc?: string
    name?: string
    description?: string
}

const PeopleCarouselCard: React.FC<PeopleCarouselCardProps> = ({
    imageSrc,
    name,
    description,
}) => {
    return (
        <div className="inmemoria-people-carousel-card">
            <div className="inmemoria-people-carousel-card-header">
                {imageSrc && (
                    <a href="#">
                        <img src={imageSrc} alt={name} />
                    </a>
                )}
                {name && (
                    <a href="#" className="inmemoria-people-carousel-card-name">
                        {name}
                    </a>
                )}
            </div>
            {description && (
                <div className="inmemoria-people-carousel-card-description">
                    {description}
                </div>
            )}
        </div>
    )
}

export default PeopleCarouselCard
