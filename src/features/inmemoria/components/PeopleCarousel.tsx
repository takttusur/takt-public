import React, { useState, useEffect } from 'react'
import './peopleCarousel.css'
import PeopleCarouselCard from './PeopleCarouselCard.tsx'
import { CarouselPerson } from '../types/CarouselPerson'
import { useGetCarouselPersonQuery } from '../data/inmemoriaApi.ts'

const PeopleCarousel: React.FC = () => {
    const { data, isLoading, isError, refetch } = useGetCarouselPersonQuery()
    const [peopleSequence, setPeopleSequence] = useState<CarouselPerson[]>([])

    useEffect(() => {
        if (!isLoading && !isError && data) {
            setPeopleSequence(data)
        }
    }, [data, isLoading, isError, refetch])

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

    if (isLoading) {
        return <div>Loading people...</div>
    }

    if (peopleSequence.length === 0) {
        return <div>No people available</div>
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
                        backgroundImage={i.backgroundImage}
                        id={i.id}
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
