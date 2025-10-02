import { FC, useState, useCallback, useEffect } from 'react'
import './photosCarousel.css'
import { Link } from 'react-router-dom'
import { useGetAttachmentsQuery } from '../data/inmemoriaApi.ts'

interface CarouselPhoto {
    id: number
    photoUrl: string
    personId: number
}

const PhotosCarousel: FC = () => {
    const attachements = useGetAttachmentsQuery()
    const [photos, setPhotos] = useState<CarouselPhoto[]>([])

    useEffect(() => {
        const p: CarouselPhoto[] =
            attachements.data?.map((d) => ({
                id: d.id,
                photoUrl: d.data,
                personId: d.personId,
            })) ?? []
        setPhotos(p)
    }, [attachements.data])

    const handleLeftClick: () => void = useCallback(() => {
        setPhotos((prevPhotos) => {
            const newPhotos = [...prevPhotos]
            const lastPhoto = newPhotos.pop()
            if (lastPhoto) {
                newPhotos.unshift(lastPhoto)
            }
            return newPhotos
        })
    }, [])

    const handleRightClick: () => void = useCallback(() => {
        setPhotos((prevPhotos) => {
            const newPhotos = [...prevPhotos]
            const firstPhoto = newPhotos.shift()
            if (firstPhoto) {
                newPhotos.push(firstPhoto)
            }
            return newPhotos
        })
    }, [])

    if (attachements.isLoading) {
        return <div></div>
    }

    return (
        <div className="inmemoria-photos-carousel">
            <div
                className="inmemoria-photos-carousel-button-left"
                onClick={handleLeftClick}
            ></div>
            <div className="inmemoria-photos-carousel-content">
                <div className="inmemoria-photos-carousel-content-line">
                    {photos.map((photo) => (
                        <Link
                            to={`/inmemoria/person/${photo.personId}/gallery`}
                            key={photo.id}
                        >
                            <img src={photo.photoUrl} alt="Gallery Photo" />
                        </Link>
                    ))}
                </div>
            </div>
            <div
                className="inmemoria-photos-carousel-button-right"
                onClick={handleRightClick}
            ></div>
        </div>
    )
}

export default PhotosCarousel
