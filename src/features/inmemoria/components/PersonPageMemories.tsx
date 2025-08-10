import React from 'react'
import './personPageMemories.css'
import { useParams } from 'react-router-dom'
import { useGetMemoriesByPersonIdQuery } from '../data/inmemoriaApi.ts'
import { PersonPageParams } from './PersonPage.tsx'

export const PersonPageMemories: React.FC = () => {
    const params = useParams<PersonPageParams>()
    const { data, isLoading, isError } = useGetMemoriesByPersonIdQuery(
        params.id ?? ''
    )

    if (!params.id) {
        return <div>No id provided</div>
    }

    if (isLoading || isError) {
        return <div>Загружаем воспоминания...</div>
    }

    if (!data || data.length === 0) {
        return <div>Пока нет воспоминаний</div>
    }

    return (
        <div className="inmemoria-person-page-memories">
            {data.map((memory) => (
                <div
                    className="inmemoria-person-page-memories-card"
                    key={memory.id}
                >
                    <div
                        className="inmemoria-person-page-memories-card-text"
                        dangerouslySetInnerHTML={{ __html: memory.text }}
                    />
                    <div className="inmemoria-person-page-memories-card-signature">
                        <span>{memory.author}</span>
                        &nbsp;
                        <span>
                            {new Date(memory.date).toLocaleDateString()}
                        </span>
                    </div>
                </div>
            ))}
        </div>
    )
}
