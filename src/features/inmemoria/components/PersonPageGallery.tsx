import React, { useState, useCallback, useEffect } from 'react'
import './personPageGallery.css'
import { useParams } from 'react-router-dom'
import { useGetPersonByIdQuery } from '../data/inmemoriaApi.ts'
import { PersonPageParams } from './PersonPage.tsx'

interface PhotoModel {
    id: number
    image: string
    title: string
}

export const PersonPageGallery: React.FC = () => {
    const [photos, setPhotos] = useState<PhotoModel[]>([])
    const params = useParams<PersonPageParams>()
    const { data, isLoading, isError } = useGetPersonByIdQuery(params.id ?? '')

    useEffect(() => {
        if (isLoading || isError || !data) return
        const photos: PhotoModel[] = []
        data.photos.forEach((a) => {
            photos.push({
                id: a.id,
                title: a.title,
                image: a.data,
            })
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
        return <div>Такой человек не найден</div>
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
