import './inmemoria.css'
import PeopleCarousel from './PeopleCarousel.tsx'
import PhotosCarousel from './PhotosCarousel.tsx'
import { JSX } from 'react'

const MainPage = (): JSX.Element => {
    return (
        <div className="inmemoria-content">
            <PeopleCarousel />
            <PhotosCarousel />
        </div>
    )
}

export default MainPage
