import React from 'react'
import './personPage.css'
import { Link, Routes, Route, Navigate, useParams } from 'react-router-dom'
import { PersonPagePhoto } from './PersonPagePhoto.tsx'
import { PersonPageGallery } from './PersonPageGallery.tsx'
import { PersonPageHikes } from './PersonPageHikes.tsx'
import { PersonPageBio } from './PersonPageBio.tsx'
import { PersonPageMemories } from './PersonPageMemories.tsx'
import { useGetPersonByIdQuery } from '../data/inmemoriaApi.ts'

export interface PersonPageParams extends Record<string, string> {
    id: string
}

export const PersonPage: React.FC = () => {
    const params = useParams<PersonPageParams>()
    const { data, isLoading, isError } = useGetPersonByIdQuery(params.id ?? '')

    if (!params.id) {
        return <div>No id provided</div>
    }

    if (isLoading || isError || !data) {
        return <div>Загрузка...</div>
    }

    return (
        <div className="inmemoria-person-page">
            <div className="inmemoria-person-page-container">
                <div className="inmemoria-person-page-sidebar">
                    <div className="inmemoria-person-page-card">
                        <div className="inmemoria-person-page-card-header">
                            <Link
                                to={`/inmemoria/person/${params.id}/photo`}
                                title={data.firstName + ' ' + data.lastName}
                            >
                                <img
                                    src={data.backgroundImage}
                                    alt="Go to photo"
                                />
                            </Link>
                            <Link
                                to={`/inmemoria/person/${params.id}/bio`}
                                title={data.firstName + ' ' + data.lastName}
                                className="inmemoria-person-page-card-header-name"
                            >
                                {data.firstName + ' ' + data.lastName}
                            </Link>
                        </div>
                        <div className="inmemoria-person-page-card-content">
                            <Link
                                to={`/inmemoria/person/${params.id}/memories`}
                            >
                                Воспоминания
                            </Link>
                            <Link to={`/inmemoria/person/${params.id}/gallery`}>
                                Галерея
                            </Link>
                            <Link to={`/inmemoria/person/${params.id}/hikes`}>
                                Маршруты
                            </Link>
                            <Link to={`/inmemoria/person/${params.id}/bio`}>
                                Жизнь
                            </Link>
                            <Link to={`/inmemoria/person/${params.id}/photo`}>
                                Фото
                            </Link>
                        </div>
                    </div>
                </div>
                <div className="inmemoria-person-page-content">
                    <h1 className="inmemoria-person-page-title">
                        {data.firstName + ' ' + data.lastName}
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
