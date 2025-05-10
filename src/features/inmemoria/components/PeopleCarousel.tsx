import React, { useState } from 'react'
import './peopleCarousel.css'
import PeopleCarouselCard from './PeopleCarouselCard.tsx'

interface CarouselPerson {
    id: number
    name: string
    imageSrc: string
}

interface PeopleCarouselProps {
    // No props needed for now
}

const PeopleCarousel: React.FC<PeopleCarouselProps> = () => {
    const persons: CarouselPerson[] = [
        {
            id: 1,
            name: 'Петров Пётр',
            imageSrc: 'src/features/inmemoria/images/fake_img2.png',
        },
        {
            id: 2,
            name: 'Владислав-Александр Старосельсконевский',
            imageSrc: 'src/features/inmemoria/images/fake_img1.png',
        },
        {
            id: 3,
            name: 'Иванов Михаил',
            imageSrc: 'src/features/inmemoria/images/fake_img2.png',
        },
        {
            id: 4,
            name: 'Смирнова Елена',
            imageSrc: 'src/features/inmemoria/images/fake_img1.png',
        },
        {
            id: 5,
            name: 'Кузнецов Андрей',
            imageSrc: 'src/features/inmemoria/images/fake_img2.png',
        },
        {
            id: 6,
            name: 'Соколова Мария',
            imageSrc: 'src/features/inmemoria/images/fake_img1.png',
        },
        {
            id: 7,
            name: 'Попов Дмитрий',
            imageSrc: 'src/features/inmemoria/images/fake_img2.png',
        },
        {
            id: 8,
            name: 'Лебедева Анна',
            imageSrc: 'src/features/inmemoria/images/fake_img1.png',
        },
        {
            id: 9,
            name: 'Морозов Сергей',
            imageSrc: 'src/features/inmemoria/images/fake_img2.png',
        },
        {
            id: 10,
            name: 'Волкова Ольга',
            imageSrc: 'src/features/inmemoria/images/fake_img1.png',
        },
        {
            id: 11,
            name: 'Козлов Алексей',
            imageSrc: 'src/features/inmemoria/images/fake_img2.png',
        },
        {
            id: 12,
            name: 'Новикова Татьяна',
            imageSrc: 'src/features/inmemoria/images/fake_img1.png',
        },
        {
            id: 13,
            name: 'Макаров Владимир',
            imageSrc: 'src/features/inmemoria/images/fake_img2.png',
        },
        {
            id: 14,
            name: 'Степанова Наталья',
            imageSrc: 'src/features/inmemoria/images/fake_img1.png',
        },
        {
            id: 15,
            name: 'Егоров Константин',
            imageSrc: 'src/features/inmemoria/images/fake_img2.png',
        },
        {
            id: 16,
            name: 'Никитина Екатерина',
            imageSrc: 'src/features/inmemoria/images/fake_img1.png',
        },
        {
            id: 17,
            name: 'Захаров Игорь',
            imageSrc: 'src/features/inmemoria/images/fake_img2.png',
        },
        {
            id: 18,
            name: 'Борисова Светлана',
            imageSrc: 'src/features/inmemoria/images/fake_img1.png',
        },
        {
            id: 19,
            name: 'Королёв Артём',
            imageSrc: 'src/features/inmemoria/images/fake_img2.png',
        },
        {
            id: 20,
            name: 'Медведева Ирина',
            imageSrc: 'src/features/inmemoria/images/fake_img1.png',
        },
    ]
    const [peopleSequence, setPeopleSequence] =
        useState<CarouselPerson[]>(persons)

    const handleLeftButtonClick = (): void => {
        setPeopleSequence((prevPeopleSequence) => {
            const newPeopleSequence = [...prevPeopleSequence]
            newPeopleSequence.unshift(newPeopleSequence.pop()!)
            return newPeopleSequence
        })
    }

    const handleRightButtonClick = (): void => {
        setPeopleSequence((prevPeopleSequence) => {
            const newPeopleSequence = [...prevPeopleSequence]
            newPeopleSequence.push(newPeopleSequence.shift()!)
            return newPeopleSequence
        })
    }

    return (
        <div className="inmemoria-people-carousel">
            <div
                className="inmemoria-people-carousel-left"
                onClick={handleLeftButtonClick}
            />
            <div className="inmemoria-people-carousel-content">
                {peopleSequence.map((i) => (
                    <PeopleCarouselCard
                        key={i.id}
                        name={i.name}
                        imageSrc={i.imageSrc}
                        url=""
                        galleryUrl=""
                        lifeUrl=""
                        memoriesUrl=""
                        tracksUrl=""
                    />
                ))}
            </div>
            <div
                className="inmemoria-people-carousel-right"
                onClick={handleRightButtonClick}
            />
        </div>
    )
}

export default PeopleCarousel
