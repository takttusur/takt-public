import React from 'react'
import './personPageMemories.css'

interface MemoriesRecord {
    id: number
    text: string
    author: string
    date: Date
}

interface PersonPageMemoriesProps {}

export const PersonPageMemories: React.FC<PersonPageMemoriesProps> = () => {
    const memories: MemoriesRecord[] = [
        {
            // eslint-disable-next-line max-len
            text: 'Была прекрасным человеком с золотым сердцем. Всегда готова была прийти на помощь и поддержать добрым словом.',
            author: 'Мария Иванова',
            date: new Date('2023-12-25'),
            id: 0,
        },
        {
            // eslint-disable-next-line max-len
            text: 'Замечательный был человек, настоящий профессионал своего дела. Буду всегда помнить наши интересные беседы за чашкой чая.',
            author: 'Александр Петров',
            date: new Date('2023-12-24'),
            id: 1,
        },
        {
            text: 'Светлая память. Навсегда останется в наших сердцах.',
            author: 'Елена Сидорова',
            date: new Date('2023-12-23'),
            id: 2,
        },
    ]

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
