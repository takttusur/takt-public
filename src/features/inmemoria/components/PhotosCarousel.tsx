import { FC, useState, useCallback } from 'react'
import './photosCarousel.css'

interface CarouselPhoto {
    id: number
    photoUrl: string
    href: string
}

interface PhotosCarouselProps {}

const PhotosCarousel: FC<PhotosCarouselProps> = () => {
    const photosArray: CarouselPhoto[] = [
        {
            id: 1,
            photoUrl: 'src/features/inmemoria/images/fake_img1.png',
            href: '',
        },
        {
            id: 2,
            photoUrl: 'src/features/inmemoria/images/fake_img2.png',
            href: '',
        },
        {
            id: 3,
            photoUrl: 'src/features/inmemoria/images/fake_img1.png',
            href: '',
        },
        {
            id: 4,
            photoUrl: 'src/features/inmemoria/images/fake_img2.png',
            href: '',
        },
        {
            id: 5,
            photoUrl: 'src/features/inmemoria/images/fake_img1.png',
            href: '',
        },
        {
            id: 6,
            photoUrl: 'src/features/inmemoria/images/fake_img2.png',
            href: '',
        },
        {
            id: 7,
            photoUrl: 'src/features/inmemoria/images/fake_img1.png',
            href: '',
        },
        {
            id: 8,
            photoUrl: 'src/features/inmemoria/images/fake_img2.png',
            href: '',
        },
        {
            id: 9,
            photoUrl: 'src/features/inmemoria/images/fake_img1.png',
            href: '',
        },
        {
            id: 10,
            photoUrl: 'src/features/inmemoria/images/fake_img2.png',
            href: '',
        },
        {
            id: 11,
            photoUrl: 'src/features/inmemoria/images/fake_img1.png',
            href: '',
        },
        {
            id: 12,
            photoUrl: 'src/features/inmemoria/images/fake_img2.png',
            href: '',
        },
        {
            id: 13,
            photoUrl: 'src/features/inmemoria/images/fake_img1.png',
            href: '',
        },
        {
            id: 14,
            photoUrl: 'src/features/inmemoria/images/fake_img2.png',
            href: '',
        },
        {
            id: 15,
            photoUrl: 'src/features/inmemoria/images/fake_img1.png',
            href: '',
        },
        {
            id: 16,
            photoUrl: 'src/features/inmemoria/images/fake_img2.png',
            href: '',
        },
        {
            id: 17,
            photoUrl: 'src/features/inmemoria/images/fake_img1.png',
            href: '',
        },
        {
            id: 18,
            photoUrl: 'src/features/inmemoria/images/fake_img2.png',
            href: '',
        },
        {
            id: 19,
            photoUrl: 'src/features/inmemoria/images/fake_img1.png',
            href: '',
        },
        {
            id: 20,
            photoUrl: 'src/features/inmemoria/images/fake_img2.png',
            href: '',
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
                {photos.map((photo) => (
                    <a href={photo.href} key={photo.id}>
                        <img src={photo.photoUrl} alt="Gallery Photo" />
                    </a>
                ))}
            </div>
            <div
                className="inmemoria-photos-carousel-button-right"
                onClick={handleRightClick}
            ></div>
        </div>
    )
}

export default PhotosCarousel
