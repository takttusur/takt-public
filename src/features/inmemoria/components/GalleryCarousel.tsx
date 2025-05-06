import React, { useState, useEffect } from 'react'

interface RandomPhoto {
    id: number
    personId: number
    url: string
}

interface GalleryCarouselProps {
    // No props needed for now
}

const GalleryCarousel: React.FC<GalleryCarouselProps> = () => {
    const [randomPhotos, setRandomPhotos] = useState<RandomPhoto[]>([])
    const [visiblePhotos, setVisiblePhotos] = useState<RandomPhoto[]>([])

    // This would normally fetch data from an API
    useEffect(() => {
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

        setRandomPhotos(mockPhotos)
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
        <div id="RandomPhoto">
            <div style={{ overflow: 'hidden', marginTop: '7px' }}>
                <table id="photo_table">
                    <tbody>
                        <tr id="photo_list">
                            {visiblePhotos.map((photo, index) => (
                                <td
                                    key={photo.id}
                                    className={`photo_item ${
                                        index === visiblePhotos.length - 1
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
    )
}

export default GalleryCarousel
