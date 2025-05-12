import React from 'react'
import './personPage.css'
import { Link, Routes, Route, Navigate } from 'react-router-dom'
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
                            <Link to="memories">Воспоминания</Link>
                            <Link to="gallery">Галерея</Link>
                            <Link to="hikes">Маршруты</Link>
                            <Link to="bio">Жизнь</Link>
                            <Link to="photo">Фото</Link>
                        </div>
                    </div>
                </div>
                <div className="inmemoria-person-page-content">
                    <Routes>
                        <Route path="photo" element={<PersonPagePhoto />} />
                        <Route path="gallery" element={<PersonPageGallery />} />
                        <Route path="hikes" element={<PersonPageHikes />} />
                        <Route path="bio" element={<PersonPageBio />} />
                        <Route
                            path="memories"
                            element={<PersonPageMemories />}
                        />
                        <Route
                            index
                            element={<Navigate to="photo" replace />}
                        />
                    </Routes>
                </div>
            </div>
        </div>
    )
}
