import React, { useState, useCallback, useEffect } from 'react'
import './personPageGallery.css'
import { GalleryPhoto } from '../types/GalleryPhoto'
import { inmemoriaFakeApi } from '../data/api/inmemoriaFakeApi.ts'

interface PersonPageGalleryProps {}

export const PersonPageGallery: React.FC<PersonPageGalleryProps> = () => {
    const [photos, setPhotos] = useState<GalleryPhoto[]>([])
    const [loading, setLoading] = useState(true)

    useEffect(() => {
        const fetchData = async (): Promise<void> => {
            try {
                const data = await inmemoriaFakeApi.getGalleryPhotos()
                setPhotos(data)
            } catch (error) {
                console.error('Error fetching gallery photos:', error)
            } finally {
                setLoading(false)
            }
        }

        void fetchData()
    }, [])

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

    if (loading) {
        return <div>Loading gallery...</div>
    }

    if (photos.length === 0) {
        return <div>No photos available</div>
    }

    return (
        <div className="inmemoria-person-page-gallery">
            <button
                className="inmemoria-person-page-gallery-button-left"
                onClick={handleLeftClick}
            ></button>
            <div className="inmemoria-person-page-gallery-photos">
                {photos.slice(0, 3).map((photo) => (
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
