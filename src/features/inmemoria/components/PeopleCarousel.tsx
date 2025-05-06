import React, { useState, useEffect } from 'react'
import memoCardBg from '../images/memo.gif'

interface Person {
    id: number
    firstName: string
    lastName: string
    photoUrl: string
    cardStyle: string
    positions: {
        photo: { left: number; top: number }
        name: { left: number; top: number }
        memoir: { left: number; top: number }
        gallery: { left: number; top: number }
        routes: { left: number; top: number }
        life: { left: number; top: number }
    }
}

interface PeopleCarouselProps {
    // No props needed for now
}

const PeopleCarousel: React.FC<PeopleCarouselProps> = () => {
    const [visiblePersons, setVisiblePersons] = useState<Person[]>([])

    // This would normally fetch data from an API
    useEffect(() => {
        // Mock data for persons
        const mockPersons: Person[] = [
            {
                id: 20,
                firstName: 'Александр',
                lastName: 'Зернов',
                photoUrl: 'http://inmemoria.tusur.ru/media/20120730231119s.jpg',
                cardStyle: 'memo.gif',
                positions: {
                    photo: { left: 12, top: 15 },
                    name: { left: 82, top: 45 },
                    memoir: { left: 65, top: 160 },
                    gallery: { left: 90, top: 200 },
                    routes: { left: 35, top: 240 },
                    life: { left: 120, top: 265 },
                },
            },
            {
                id: 31,
                firstName: 'Владимир',
                lastName: 'Солобоев',
                photoUrl: 'http://inmemoria.tusur.ru/media/20120905121432s.jpg',
                cardStyle: 'memo.gif',
                positions: {
                    photo: { left: 12, top: 15 },
                    name: { left: 82, top: 45 },
                    memoir: { left: 60, top: 125 },
                    gallery: { left: 75, top: 175 },
                    routes: { left: 68, top: 200 },
                    life: { left: 110, top: 280 },
                },
            },
            {
                id: 14,
                firstName: 'Николай',
                lastName: 'Денисов',
                photoUrl: 'http://inmemoria.tusur.ru/media/20120828235925s.jpg',
                cardStyle: 'memo.gif',
                positions: {
                    photo: { left: 12, top: 15 },
                    name: { left: 82, top: 45 },
                    memoir: { left: 65, top: 160 },
                    gallery: { left: 65, top: 212 },
                    routes: { left: 65, top: 245 },
                    life: { left: 110, top: 290 },
                },
            },
        ]

        setVisiblePersons(mockPersons)
    }, [])

    const handleRightListButtonClick = (): void => {
        // Animate the list to move left
        const listElement = document.getElementById('list')
        if (listElement) {
            // Animate the list to move left
            listElement.style.transition = 'left 200ms'
            listElement.style.left = '-170px'

            // After the animation completes, move the first person to the end and reset the position
            setTimeout(() => {
                const updatedPersons = [...visiblePersons]
                const firstPerson = updatedPersons.shift()
                if (firstPerson) {
                    updatedPersons.push(firstPerson)
                    setVisiblePersons(updatedPersons)
                }

                // Reset the position without animation
                listElement.style.transition = 'none'
                listElement.style.left = '0px'
            }, 300)
        }
    }

    const handleLeftListButtonClick = (): void => {
        // First move the last person to the beginning of the list
        const updatedPersons = [...visiblePersons]
        const lastPerson = updatedPersons.pop()
        if (lastPerson) {
            updatedPersons.unshift(lastPerson)
            setVisiblePersons(updatedPersons)

            // Then set the initial position of the list to the left
            const listElement = document.getElementById('list')
            if (listElement) {
                listElement.style.transition = 'none'
                listElement.style.left = '-170px'

                // Animate the list to move right
                setTimeout(() => {
                    listElement.style.transition = 'left 200ms'
                    listElement.style.left = '0px'
                }, 0)
            }
        }
    }

    return (
        <div style={{ textAlign: 'center' }}>
            <div id="list">
                {visiblePersons.map((person) => (
                    <div
                        key={person.id}
                        className="card"
                        style={{
                            background: `url(${memoCardBg}) no-repeat scroll 0pt 0pt transparent`,
                            position: 'relative',
                        }}
                    >
                        <a href={`person.php?id=${person.id}#photo`}>
                            <div
                                className="small_photo"
                                style={{
                                    left: `${person.positions.photo.left}px`,
                                    top: `${person.positions.photo.top}px`,
                                    background: `url('${person.photoUrl}')`,
                                }}
                            ></div>
                        </a>
                        <div className="personname">
                            <a href={`person.php?id=${person.id}#biography`}>
                                {person.firstName}
                                <br />
                                {person.lastName}
                                <br />
                            </a>
                        </div>
                        <div
                            className="memoir"
                            style={{
                                left: `${person.positions.memoir.left}px`,
                                top: `${person.positions.memoir.top}px`,
                            }}
                        >
                            <a href={`person.php?id=${person.id}#memoir`}>
                                Воспоминания
                            </a>
                        </div>
                        <div
                            className="gallery"
                            style={{
                                left: `${person.positions.gallery.left}px`,
                                top: `${person.positions.gallery.top}px`,
                            }}
                        >
                            <a href={`person.php?id=${person.id}#gallery`}>
                                Галерея
                            </a>
                        </div>
                        <div
                            className="gallery"
                            style={{
                                left: `${person.positions.routes.left}px`,
                                top: `${person.positions.routes.top}px`,
                            }}
                        >
                            <a href={`person.php?id=${person.id}#hike`}>
                                Маршруты
                            </a>
                        </div>
                        <div
                            className="life"
                            style={{
                                left: `${person.positions.life.left}px`,
                                top: `${person.positions.life.top}px`,
                            }}
                        >
                            <a href={`person.php?id=${person.id}#biography`}>
                                Жизнь
                            </a>
                        </div>
                    </div>
                ))}
            </div>

            <div
                id="left_list_button"
                onClick={handleLeftListButtonClick}
            ></div>
            <div
                id="right_list_button"
                onClick={handleRightListButtonClick}
            ></div>
        </div>
    )
}

export default PeopleCarousel
