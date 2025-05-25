import { FC, useState, useCallback } from 'react'
import './photosCarousel.css'
import { Link } from 'react-router-dom'

interface CarouselPhoto {
    id: number
    photoUrl: string
    personId: number
}

const PhotosCarousel: FC = () => {
    const photosArray: CarouselPhoto[] = [
        {
            id: 1,
            personId: 1,
            photoUrl: 'src/features/inmemoria/images/fake_img1.png',
        },
        {
            id: 2,
            personId: 1,
            photoUrl: 'src/features/inmemoria/images/fake_img2.png',
        },
        {
            id: 3,
            personId: 1,
            photoUrl: 'src/features/inmemoria/images/fake_img1.png',
        },
        {
            id: 4,
            personId: 1,
            photoUrl: 'src/features/inmemoria/images/fake_img2.png',
        },
        {
            id: 5,
            personId: 1,
            photoUrl: 'src/features/inmemoria/images/fake_img1.png',
        },
        {
            id: 6,
            personId: 1,
            photoUrl: 'src/features/inmemoria/images/fake_img2.png',
        },
        {
            id: 7,
            personId: 1,
            photoUrl: 'src/features/inmemoria/images/fake_img1.png',
        },
        {
            id: 8,
            personId: 1,
            photoUrl: 'src/features/inmemoria/images/fake_img2.png',
        },
        {
            id: 9,
            personId: 1,
            photoUrl: 'src/features/inmemoria/images/fake_img1.png',
        },
        {
            id: 10,
            personId: 1,
            photoUrl: 'src/features/inmemoria/images/fake_img2.png',
        },
        {
            id: 11,
            personId: 1,
            photoUrl: 'src/features/inmemoria/images/fake_img1.png',
        },
        {
            id: 12,
            personId: 1,
            photoUrl: 'src/features/inmemoria/images/fake_img2.png',
        },
        {
            id: 13,
            personId: 1,
            photoUrl: 'src/features/inmemoria/images/fake_img1.png',
        },
        {
            id: 14,
            personId: 1,
            photoUrl: 'src/features/inmemoria/images/fake_img2.png',
        },
        {
            id: 15,
            personId: 1,
            photoUrl: 'src/features/inmemoria/images/fake_img1.png',
        },
        {
            id: 16,
            personId: 1,
            photoUrl: 'src/features/inmemoria/images/fake_img2.png',
        },
        {
            id: 17,
            personId: 1,
            photoUrl: 'src/features/inmemoria/images/fake_img1.png',
        },
        {
            id: 18,
            personId: 1,
            photoUrl: 'src/features/inmemoria/images/fake_img2.png',
        },
        {
            id: 19,
            personId: 1,
            photoUrl: 'src/features/inmemoria/images/fake_img1.png',
        },
        {
            id: 20,
            personId: 1,
            photoUrl: 'src/features/inmemoria/images/fake_img2.png',
        },
    ]
    const [photos, setPhotos] = useState<CarouselPhoto[]>(photosArray)

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
