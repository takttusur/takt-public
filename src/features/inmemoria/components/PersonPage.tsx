import React from 'react'
import './personPage.css'
import { Link } from 'react-router-dom'
import { PersonPagePhoto } from './PersonPagePhoto.tsx'
import { PersonPageGallery } from './PersonPageGallery.tsx'
import { PersonPageHikes } from './PersonPageHikes.tsx'
import { PersonPageBio } from './PersonPageBio.tsx'
import { PersonPageMemories } from './PersonPageMemories.tsx'

interface Person {
    name: string
    image: string
}

interface PersonPageProps {}

export const PersonPage: React.FC<PersonPageProps> = () => {
    const personData: Person = {
        name: 'Владислав-Александр Старосельский',
        image: 'src/features/inmemoria/images/fake_img1.png',
    }
    return (
        <div className="inmemoria-person-page">
            <div className="inmemoria-person-page-container">
                <div className="inmemoria-person-page-sidebar">
                    <div className="inmemoria-person-page-card">
                        <div className="inmemoria-person-page-card-header">
                            <img src={personData.image} alt="" />
                            <span title={personData.name}>
                                {personData.name}
                            </span>
                        </div>
                        <div className="inmemoria-person-page-card-content">
                            <Link to="person">Воспоминания</Link>
                            <Link to="person">Галерея</Link>
                            <Link to="person">Маршруты</Link>
                            <Link to="person">Жизнь</Link>
                        </div>
                    </div>
                </div>
                <div className="inmemoria-person-page-content">
                    <PersonPagePhoto />
                    <PersonPageGallery />
                    <PersonPageHikes />
                    <PersonPageBio />
                    <PersonPageMemories />
                </div>
            </div>
        </div>
    )
}
