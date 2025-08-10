import React, { useState, useCallback, useEffect } from 'react'
import './personPageGallery.css'
import { useParams } from 'react-router-dom'
import {
    PhotosResultModel,
    useGetPhotosByPersonIdQuery,
} from '../data/inmemoriaApi.ts'
import { PersonPageParams } from './PersonPage.tsx'

export const PersonPageGallery: React.FC = () => {
    const [photos, setPhotos] = useState<PhotosResultModel[]>([])
    const params = useParams<PersonPageParams>()
    const { data, isLoading, isError } = useGetPhotosByPersonIdQuery(
        params.id ?? ''
    )

    useEffect(() => {
        if (isLoading || isError || !data) return
        const photos: PhotosResultModel[] = []
        data.forEach((photo) => {
            photos.push(photo)
        })
        setPhotos(() => photos)
    }, [data, isLoading, isError])

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

    if (!params.id) {
        return <div>No id provided</div>
    }

    if (isLoading || isError || !data) {
        return <div>Загрузка...</div>
    }

    if (photos.length === 0) {
        return <div>Нет фотографий</div>
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
                        <img src={photo.image} alt={photo.title} />
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
