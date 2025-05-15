import React, { useState, useCallback } from 'react'
import './personPageGallery.css'

interface GalleryPhoto {
    id: number
    src: string
    title: string
}

interface PersonPageGalleryProps {}

export const PersonPageGallery: React.FC<PersonPageGalleryProps> = () => {
    const initialPhotos: GalleryPhoto[] = [
        {
            id: 1,
            src: 'src/features/inmemoria/images/fake_img1.png',
            title: 'Тестовая подпись 1',
        },
        {
            id: 2,
            src: 'src/features/inmemoria/images/fake_img1.png',
            title: 'Тестовая подпись 2',
        },
        {
            id: 3,
            src: 'src/features/inmemoria/images/fake_img2.png',
            title: 'Тестовая подпись 3',
        },
        {
            id: 4,
            src: 'src/features/inmemoria/images/fake_img1.png',
            title: 'Тестовая подпись 4',
        },
        {
            id: 5,
            src: 'src/features/inmemoria/images/fake_img2.png',
            title: 'Тестовая подпись 5',
        },
    ]
    const [photos, setPhotos] = useState<GalleryPhoto[]>(initialPhotos)

    const handleLeftClick: () => void = useCallback(() => {
        const newPhotos = [...photos]
        const last = newPhotos.pop()
        if (last) {
            newPhotos.unshift(last)
        }
        setPhotos(() => newPhotos)
    }, [photos])

    const handleRightClick: () => void = useCallback(() => {
        const newPhotos = [...photos]
        const first = newPhotos.shift()
        if (first) {
            newPhotos.push(first)
        }
        setPhotos(() => newPhotos)
    }, [photos])

    return (
        <div className="inmemoria-person-page-gallery">
            <button
                className="inmemoria-person-page-gallery-button-left"
                onClick={handleLeftClick}
            ></button>
            <div className="inmemoria-person-page-gallery-photos">
                {photos.map((photo) => (
                    <div
                        key={photo.id}
                        className="inmemoria-person-page-gallery-photos-item"
                    >
                        <img src={photo.src} alt={photo.title} />
                        <div className="inmemoria-person-page-gallery-photos-item-title">
                            {photo.title}
                        </div>
                    </div>
                ))}
            </div>
            <button
                className="inmemoria-person-page-gallery-button-right"
                onClick={handleRightClick}
            ></button>
        </div>
    )
}
