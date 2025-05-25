import React, { useState, useEffect } from 'react'
import './personPage.css'
import { Link, Routes, Route, Navigate, useParams } from 'react-router-dom'
import { PersonPagePhoto } from './PersonPagePhoto.tsx'
import { PersonPageGallery } from './PersonPageGallery.tsx'
import { PersonPageHikes } from './PersonPageHikes.tsx'
import { PersonPageBio } from './PersonPageBio.tsx'
import { PersonPageMemories } from './PersonPageMemories.tsx'
import { Person } from '../types/Person'
import { inmemoriaFakeApi } from '../data/api/inmemoriaFakeApi.ts'
import { personCardDesigns } from '../data/personCardDesigns.ts'
import { useAppSelector } from '../../../store/hooks'

interface PersonPageParams extends Record<string, string> {
    id: string
}

export const PersonPage: React.FC = () => {
    const [personData, setPersonData] = useState<Person | null>(null)
    const personState = useAppSelector((state) => state.inmemoria)
    const params = useParams<PersonPageParams>()

    useEffect(() => {
        const fetchData = async (): Promise<void> => {
            try {
                const data = await inmemoriaFakeApi.getPerson()
                setPersonData(data)
            } catch (error) {
                console.error('Error fetching person data:', error)
            }
        }
        console.log('Params')
        console.log(params)

        void fetchData()
    }, [params])
    if (!personState.current) {
        return <div>Loading...</div>
    }

    return (
        <div className="inmemoria-person-page">
            <div className="inmemoria-person-page-container">
                <div className="inmemoria-person-page-sidebar">
                    <div className="inmemoria-person-page-card">
                        <div className="inmemoria-person-page-card-header">
                            <Link to="photo" title={personData.name}>
                                <img
                                    src={
                                        personCardDesigns[personData.card].image
                                    }
                                    alt="Go to photo"
                                />
                            </Link>
                            <Link
                                to="bio"
                                title={personData.name}
                                className="inmemoria-person-page-card-header-name"
                            >
                                {personData.name}
                            </Link>
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
                    <h1 className="inmemoria-person-page-title">
                        {personData.name}
                    </h1>
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
