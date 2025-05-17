import React, { useState, useEffect } from 'react'
import './peopleCarousel.css'
import PeopleCarouselCard from './PeopleCarouselCard.tsx'
import { CarouselPerson } from '../types/CarouselPerson'
import { inmemoriaFakeApi } from '../data/api/inmemoriaFakeApi.ts'

interface PeopleCarouselProps {
    // No props needed for now
}

const PeopleCarousel: React.FC<PeopleCarouselProps> = () => {
    const [peopleSequence, setPeopleSequence] = useState<CarouselPerson[]>([])
    const [loading, setLoading] = useState(true)

    useEffect(() => {
        const fetchData = async (): Promise<void> => {
            try {
                const data = await inmemoriaFakeApi.getPersons()
                setPeopleSequence(data)
            } catch (error) {
                console.error('Error fetching persons:', error)
            } finally {
                setLoading(false)
            }
        }

        void fetchData()
    }, [])

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

    if (loading) {
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
