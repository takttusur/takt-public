/* eslint-disable max-len */
import React from 'react'
import './personPageBio.css'
import { useParams } from 'react-router-dom'
import { useGetPersonByIdQuery } from '../data/inmemoriaApi.ts'
import { PersonPageParams } from './PersonPage.tsx'

export const PersonPageBio: React.FC = () => {
    const params = useParams<PersonPageParams>()
    const { data, isLoading, isError } = useGetPersonByIdQuery(params.id ?? '')

    if (!params.id) {
        return <div>No id provided</div>
    }

    if (isLoading || isError || !data) {
        return <div>Загрузка...</div>
    }

    return (
        <div
            className="inmemoria-person-page-bio"
            dangerouslySetInnerHTML={{ __html: data.biography }}
        />
    )
}
