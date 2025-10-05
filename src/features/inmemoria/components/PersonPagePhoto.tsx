import React from 'react'
import { useParams } from 'react-router-dom'
import { useGetPersonByIdQuery } from '../data/inmemoriaApi.ts'
import { PersonPageParams } from './PersonPage.tsx'

export const PersonPagePhoto: React.FC = () => {
    const params = useParams<PersonPageParams>()
    const { data, isLoading, isError } = useGetPersonByIdQuery(params.id ?? '')

    if (!params.id) {
        return <img alt="Неправильный ID"></img>
    }

    if (isLoading || isError || !data) {
        return <img alt="Загрузка..."></img>
    }

    return (
        <img
            style={{ border: 'none' }}
            alt="Фото профиля"
            src={data.photoImage}
        />
    )
}
