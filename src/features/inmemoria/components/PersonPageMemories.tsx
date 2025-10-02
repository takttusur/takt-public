import React from 'react'
import './personPageMemories.css'
import { useParams } from 'react-router-dom'
import { useGetPersonByIdQuery } from '../data/inmemoriaApi.ts'
import { PersonPageParams } from './PersonPage.tsx'

export const PersonPageMemories: React.FC = () => {
    const params = useParams<PersonPageParams>()
    const { data, isLoading, isError } = useGetPersonByIdQuery(params.id ?? '')

    if (!params.id) {
        return <div>No id provided</div>
    }

    if (isLoading || isError) {
        return <div>Загружаем воспоминания...</div>
    }

    const memories = data?.memories

    if (!memories) {
        return <div>Пока нет воспоминаний</div>
    }

    return (
        <div className="inmemoria-person-page-memories">
            {memories.map((memory) => (
                <div
                    className="inmemoria-person-page-memories-card"
                    key={memory.id}
                >
                    <div
                        className="inmemoria-person-page-memories-card-text"
                        dangerouslySetInnerHTML={{ __html: memory.data }}
                    />
                    <div className="inmemoria-person-page-memories-card-signature">
                        <span>{memory.subtitle}</span>
                        &nbsp;
                        <span>
                            {new Date(memory.createdAt).toLocaleDateString()}
                        </span>
                    </div>
                </div>
            ))}
        </div>
    )
}
