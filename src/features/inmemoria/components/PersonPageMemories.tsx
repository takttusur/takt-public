import React, { useState, useEffect } from 'react'
import './personPageMemories.css'
import { MemoriesRecord } from '../types/MemoriesRecord'
import { inmemoriaFakeApi } from '../data/api/inmemoriaFakeApi.ts'

interface PersonPageMemoriesProps {}

export const PersonPageMemories: React.FC<PersonPageMemoriesProps> = () => {
    const [memories, setMemories] = useState<MemoriesRecord[]>([])
    const [loading, setLoading] = useState(true)

    useEffect(() => {
        const fetchData = async (): Promise<void> => {
            try {
                const data = await inmemoriaFakeApi.getMemories()
                setMemories(data)
            } catch (error) {
                console.error('Error fetching memories:', error)
            } finally {
                setLoading(false)
            }
        }

        void fetchData()
    }, [])

    if (loading) {
        return <div>Loading memories...</div>
    }

    if (memories.length === 0) {
        return <div>No memories available</div>
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
                        dangerouslySetInnerHTML={{ __html: memory.text }}
                    />
                    <div className="inmemoria-person-page-memories-card-signature">
                        <span>{memory.author}</span>
                        &nbsp;
                        <span>{memory.date.toLocaleDateString()}</span>
                    </div>
                </div>
            ))}
        </div>
    )
}
