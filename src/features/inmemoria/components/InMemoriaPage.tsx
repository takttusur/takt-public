import { useEffect, useState } from 'react'
import '../../inmemoria/styles/main.css'
import '../../inmemoria/styles/index.css'
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

interface RandomPhoto {
    id: number
    personId: number
    url: string
}

const InMemoriaPage = (): JSX.Element => {
    const [epigraph, setEpigraph] = useState<string>('')
    const [search, setSearch] = useState<string>('')
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    const [persons, setPersons] = useState<Person[]>([])
    const [randomPhotos, setRandomPhotos] = useState<RandomPhoto[]>([])
    const [visiblePersons, setVisiblePersons] = useState<Person[]>([])
    const [visiblePhotos, setVisiblePhotos] = useState<RandomPhoto[]>([])

    // This would normally fetch data from an API
    useEffect(() => {
        setEpigraph(
            // eslint-disable-next-line max-len
            '<em>И качнется бессмысленной мыслью</em><br><em>Пара фраз долетевших оттуда -</em><br><em>Я тебя никогда не забуду,</em><br><em>Я тебя никогда не увижу...</em><br><em>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;&nbsp; А.Рыбников</em>'
        )

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

        // Mock data for random photos
        const mockPhotos: RandomPhoto[] = [
            {
                id: 1,
                personId: 19,
                url: 'http://inmemoria.tusur.ru/media/19/20120819235925.jpg',
            },
            {
                id: 2,
                personId: 62,
                url: 'http://inmemoria.tusur.ru/media/62/20221004175345.jpg',
            },
            {
                id: 3,
                personId: 50,
                url: 'http://inmemoria.tusur.ru/media/50/20121029095155.jpg',
            },
            {
                id: 4,
                personId: 58,
                url: 'http://inmemoria.tusur.ru/media/58/20220503124210.jpg',
            },
            {
                id: 5,
                personId: 27,
                url: 'http://inmemoria.tusur.ru/media/27/20120826235552.jpg',
            },
        ]

        setPersons(mockPersons)
        setRandomPhotos(mockPhotos)

        // Initialize visible persons and photos
        setVisiblePersons(mockPersons)
        setVisiblePhotos(mockPhotos)
    }, [])

    // Initialize j-isLast and j-isLastView classes on the last photo
    useEffect(() => {
        if (visiblePhotos.length > 0) {
            const lastPhotoElement = document.querySelector(
                '#photo_list .photo_item:last-child'
            )
            if (lastPhotoElement) {
                lastPhotoElement.classList.add('j-isLast', 'j-isLastView')
            }
        }
    }, [visiblePhotos])

    const handleSearchChange = (
        e: React.ChangeEvent<HTMLInputElement>
    ): void => {
        setSearch(e.target.value)
    }

    const handleSearchSubmit = (e: React.FormEvent<HTMLFormElement>): void => {
        e.preventDefault()
        // Handle search submission
        console.log('Search submitted:', search)
    }

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

    const loadRightRandomPhoto = (): void => {
        // Check if the last photo is visible in the viewport
        const lastPhotoElement = document.querySelector('#photo_list .j-isLast')
        if (
            lastPhotoElement &&
            lastPhotoElement.getBoundingClientRect().right < window.innerWidth
        ) {
            // In a real implementation, this would fetch a new random photo from the API using AJAX
            // For now, we'll just add a mock photo
            fetch('/random_photo.php')
                .then(() => {
                    // Since we don't have a real API, we'll simulate a response
                    const newPhoto: RandomPhoto = {
                        id: Math.floor(Math.random() * 1000) + 100,
                        personId: Math.floor(Math.random() * 50) + 1,
                        url: `http://inmemoria.tusur.ru/media/${
                            Math.floor(Math.random() * 50) + 1
                        }/${Math.floor(Math.random() * 10000000000)}.jpg`,
                    }

                    // Update the last photo marker
                    const lastPhotoElements = document.querySelectorAll(
                        '#photo_list .j-isLast'
                    )
                    lastPhotoElements.forEach((el) =>
                        el.classList.remove('j-isLast')
                    )

                    // Add the new photo
                    setRandomPhotos([...randomPhotos, newPhoto])
                    setVisiblePhotos([...visiblePhotos, newPhoto])

                    // Mark the new last photo
                    setTimeout(() => {
                        const newLastPhoto = document.querySelector(
                            '#photo_list .photo_item:last-child'
                        )
                        if (newLastPhoto) {
                            newLastPhoto.classList.add('j-isLast')
                        }
                    }, 0)
                })
                .catch((error) => {
                    console.error('Error loading random photo:', error)
                })
        }
    }

    const loadLeftRandomPhoto = (): void => {
        // Check if the last view photo is visible in the viewport
        const lastViewPhotoElement = document.querySelector(
            '#photo_list .j-isLastView'
        )
        if (
            !lastViewPhotoElement ||
            lastViewPhotoElement.getBoundingClientRect().right <
                window.innerWidth
        ) {
            // In a real implementation, this would fetch a new random photo from the API using AJAX
            // For now, we'll just add a mock photo
            fetch('/random_photo.php')
                .then(() => {
                    // Since we don't have a real API, we'll simulate a response
                    const newPhoto: RandomPhoto = {
                        id: Math.floor(Math.random() * 1000) + 100,
                        personId: Math.floor(Math.random() * 50) + 1,
                        url: `http://inmemoria.tusur.ru/media/${
                            Math.floor(Math.random() * 50) + 1
                        }/${Math.floor(Math.random() * 10000000000)}.jpg`,
                    }

                    // Update the last view photo marker
                    const lastViewPhotoElements = document.querySelectorAll(
                        '#photo_list .j-isLastView'
                    )
                    lastViewPhotoElements.forEach((el) =>
                        el.classList.remove('j-isLastView')
                    )

                    // Check if the last photo is visible in the viewport
                    const lastPhotoElement = document.querySelector(
                        '#photo_list .photo_item:last-child'
                    )
                    if (
                        lastPhotoElement &&
                        lastPhotoElement.getBoundingClientRect().right <
                            window.innerWidth
                    ) {
                        // If the last photo is visible, mark it as the last view and append the new photo
                        if (lastPhotoElement) {
                            lastPhotoElement.classList.add('j-isLastView')
                        }
                        setRandomPhotos([...randomPhotos, newPhoto])
                        setVisiblePhotos([...visiblePhotos, newPhoto])
                    } else {
                        // If the last photo is not visible, find the last visible photo and insert the new photo after it
                        const photoItems = document.querySelectorAll(
                            '#photo_list .photo_item'
                        )
                        let insertAfterIndex = -1

                        photoItems.forEach((el, index) => {
                            if (
                                el.getBoundingClientRect().right >
                                window.innerWidth
                            ) {
                                if (insertAfterIndex === -1) {
                                    insertAfterIndex = index - 1
                                    el.classList.add('j-isLastView')
                                }
                            }
                        })

                        if (insertAfterIndex !== -1) {
                            const updatedPhotos = [...visiblePhotos]
                            updatedPhotos.splice(
                                insertAfterIndex + 1,
                                0,
                                newPhoto
                            )
                            setRandomPhotos([...randomPhotos, newPhoto])
                            setVisiblePhotos(updatedPhotos)
                        } else {
                            // If we couldn't find a suitable insertion point, just append the new photo
                            setRandomPhotos([...randomPhotos, newPhoto])
                            setVisiblePhotos([...visiblePhotos, newPhoto])
                        }
                    }
                })
                .catch((error) => {
                    console.error('Error loading random photo:', error)
                })
        }
    }

    const handleRightRandomPhotoButtonClick = (): void => {
        // Animate the photo table to move left
        const photoTableElement = document.getElementById('photo_table')
        const firstPhotoElement = document.querySelector(
            '#photo_list .photo_item:first-child'
        )

        if (photoTableElement && firstPhotoElement) {
            const firstPhotoWidth = (firstPhotoElement as HTMLElement)
                .offsetWidth

            // Animate the photo table to move left
            photoTableElement.style.transition = 'left 100ms'
            photoTableElement.style.left = `-${firstPhotoWidth}px`

            // After the animation completes, move the first photo to the end and reset the position
            setTimeout(() => {
                const updatedPhotos = [...visiblePhotos]
                const firstPhoto = updatedPhotos.shift()
                if (firstPhoto) {
                    updatedPhotos.push(firstPhoto)
                    setVisiblePhotos(updatedPhotos)
                }

                // Reset the position without animation
                photoTableElement.style.transition = 'none'
                photoTableElement.style.left = '0px'

                // Load a new random photo if needed
                loadRightRandomPhoto()
            }, 120)
        }
    }

    const handleLeftRandomPhotoButtonClick = (): void => {
        // First move the last photo to the beginning of the list
        const updatedPhotos = [...visiblePhotos]
        const lastPhoto = updatedPhotos.pop()
        if (lastPhoto) {
            updatedPhotos.unshift(lastPhoto)
            setVisiblePhotos(updatedPhotos)

            // Then set the initial position of the photo table to the left
            const photoTableElement = document.getElementById('photo_table')
            const firstPhotoElement = document.querySelector(
                '#photo_list .photo_item:first-child'
            )

            if (photoTableElement && firstPhotoElement) {
                const firstPhotoWidth = (firstPhotoElement as HTMLElement)
                    .offsetWidth

                // Set the initial position without animation
                photoTableElement.style.transition = 'none'
                photoTableElement.style.left = `-${firstPhotoWidth}px`

                // Animate the photo table to move right
                setTimeout(() => {
                    photoTableElement.style.transition = 'left 100ms'
                    photoTableElement.style.left = '0px'

                    // Load a new random photo if needed
                    loadLeftRandomPhoto()
                }, 0)
            }
        }
    }

    return (
        <div id="ie_fix" className="inmemoria-page">
            <a href="http://inmemoria.tusur.ru/">
                <div id="logo"></div>
            </a>
            <div id="epigraph">
                <p dangerouslySetInnerHTML={{ __html: epigraph }}></p>
                <div id="listlink">
                    <i>
                        <a href="list.php">Те, кого с нами нет...</a>
                    </i>
                </div>
            </div>
            <div id="mainsearch">
                <form
                    className="searchform"
                    action="search.php"
                    method="get"
                    onSubmit={handleSearchSubmit}
                >
                    <input
                        id="textinput"
                        type="text"
                        name="name"
                        placeholder="Фамилия, Имя, Прозвище"
                        value={search}
                        onChange={handleSearchChange}
                    />
                    <input id="opsubmit" type="submit" value="" />
                </form>
            </div>

            <div id="list_and_photo" style={{ textAlign: 'center' }}>
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
                                <a
                                    href={`person.php?id=${person.id}#biography`}
                                >
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
                                <a
                                    href={`person.php?id=${person.id}#biography`}
                                >
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

                <div id="RandomPhoto">
                    <div style={{ overflow: 'hidden', marginTop: '7px' }}>
                        <table id="photo_table">
                            <tbody>
                                <tr id="photo_list">
                                    {visiblePhotos.map((photo, index) => (
                                        <td
                                            key={photo.id}
                                            className={`photo_item ${
                                                index ===
                                                visiblePhotos.length - 1
                                                    ? 'j-isLast j-isLastView'
                                                    : ''
                                            }`}
                                        >
                                            <a
                                                href={`person.php?id=${photo.personId}#gallery`}
                                            >
                                                <img
                                                    height="100"
                                                    src={photo.url}
                                                    alt=""
                                                />
                                            </a>
                                        </td>
                                    ))}
                                </tr>
                            </tbody>
                        </table>
                    </div>
                    <div
                        id="left_randomphoto_button"
                        onClick={handleLeftRandomPhotoButtonClick}
                    ></div>

                    <div
                        id="right_randomphoto_button"
                        onClick={handleRightRandomPhotoButtonClick}
                    ></div>
                </div>
            </div>

            <div id="foot">
                <a href="about.php">О проекте</a>
                <a style={{ marginLeft: '50px' }} href="http://takt.tomsk.ru">
                    ТАКТ
                </a>
                <span style={{ float: 'right', marginRight: '50px' }}>
                    Все вопросы, предложения или замечания отправляйте на адрес{' '}
                    <a href="mailto:inmemoria@ngs.ru">inmemoria@ngs.ru</a>
                </span>
            </div>
        </div>
    )
}

export default InMemoriaPage
