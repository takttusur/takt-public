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
                    imageSrc="src/features/inmemoria/images/fake_img1.png"
                    url=""
                    galleryUrl=""
                    lifeUrl=""
                    memoriesUrl=""
                    tracksUrl=""
                />
                <PeopleCarouselCard
                    name="Петров Пётр"
                    imageSrc="src/features/inmemoria/images/fake_img2.png"
                    url=""
                    galleryUrl=""
                    lifeUrl=""
                    memoriesUrl=""
                    tracksUrl=""
                />
                <PeopleCarouselCard
                    name="Владислав-Александр Старосельсконевский"
                    imageSrc="src/features/inmemoria/images/fake_img1.png"
                    url=""
                    galleryUrl=""
                    lifeUrl=""
                    memoriesUrl=""
                    tracksUrl=""
                />
                <PeopleCarouselCard
                    name="Ким Олег"
                    imageSrc="src/features/inmemoria/images/fake_img2.png"
                    url=""
                    galleryUrl=""
                    lifeUrl=""
                    memoriesUrl=""
                    tracksUrl=""
                />
            </div>
            <div className="inmemoria-people-carousel-right" />
        </div>
    )
}

export default PeopleCarousel
