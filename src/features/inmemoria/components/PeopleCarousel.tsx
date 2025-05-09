import React from 'react'
import './peopleCarousel.css'
import PeopleCarouselCard from './PeopleCarouselCard.tsx'

interface PeopleCarouselProps {
    // No props needed for now
}

const PeopleCarousel: React.FC<PeopleCarouselProps> = () => {
    return (
        <div className="inmemoria-people-carousel">
            <div className="inmemoria-people-carousel-left" />
            <div className="inmemoria-people-carousel-content">
                <PeopleCarouselCard
                    name="Васильев Иван"
                    description="text"
                    imageSrc="src/features/inmemoria/images/fake_img1.png"
                />
                <PeopleCarouselCard
                    name="Петров Пётр"
                    description="text"
                    imageSrc="src/features/inmemoria/images/fake_img2.png"
                />
                <PeopleCarouselCard
                    name="Владислав-Александр Старосельсконевский"
                    description="text"
                    imageSrc="src/features/inmemoria/images/fake_img1.png"
                />
                <PeopleCarouselCard
                    name="Ким Олег"
                    description="text"
                    imageSrc="src/features/inmemoria/images/fake_img2.png"
                />
            </div>
            <div className="inmemoria-people-carousel-right" />
        </div>
    )
}

export default PeopleCarousel
