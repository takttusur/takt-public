import { useEffect, useState } from 'react'
import './listPage.css'
import { Link } from 'react-router-dom'

interface PeopleRecord {
    name: string
    id: number
}

interface LiteralRecord {
    literal: string
    people: PeopleRecord[]
}

const ListPage = (): JSX.Element => {
    // eslint-disable-next-line react-hooks/exhaustive-deps
    const peopleDictionary: LiteralRecord[] = [
        {
            literal: 'А',
            people: [
                {
                    name: 'Антонов Иван',
                    id: 1,
                },
                {
                    name: 'Авоськин Владимир',
                    id: 2,
                },
            ],
        },
        {
            literal: 'Б',
            people: [
                {
                    name: 'Бондаренко Иван',
                    id: 3,
                },
                {
                    name: 'Бикмуллин Владимир',
                    id: 4,
                },
            ],
        },
        {
            literal: 'В',
            people: [
                {
                    name: 'Ви Иван',
                    id: 5,
                },
                {
                    name: 'Ватутин Владимир',
                    id: 6,
                },
            ],
        },
        {
            literal: 'Г',
            people: [
                {
                    name: 'Горохов Петр',
                    id: 7,
                },
                {
                    name: 'Грибов Алексей',
                    id: 8,
                },
            ],
        },
        {
            literal: 'Д',
            people: [
                {
                    name: 'Дятлов Андрей',
                    id: 9,
                },
                {
                    name: 'Дроздов Михаил',
                    id: 10,
                },
            ],
        },
        {
            literal: 'Е',
            people: [
                {
                    name: 'Егоров Сергей',
                    id: 11,
                },
                {
                    name: 'Ершов Дмитрий',
                    id: 12,
                },
            ],
        },
    ]
    const columnsCount = 3
    const [people, setPeople] = useState<LiteralRecord[][]>([])
    useEffect(() => {
        const itemsPerColumn = Math.ceil(peopleDictionary.length / columnsCount)
        const groupedPeople = peopleDictionary.reduce((acc, curr, i) => {
            const columnIndex = Math.floor(i / itemsPerColumn)
            if (!acc[columnIndex]) {
                acc[columnIndex] = []
            }
            acc[columnIndex].push(curr)
            return acc
        }, [] as LiteralRecord[][])
        setPeople(groupedPeople)
    }, [peopleDictionary])

    return (
        <div className="inmemoria-list-page">
            <div
                className="inmemoria-list-page-container"
                style={{
                    gridTemplateColumns: `repeat(${columnsCount}, 1fr)`,
                }}
            >
                {people.map((group, index) => (
                    <div className="inmemoria-list-page-column" key={index}>
                        {group.map((item) => (
                            <div key={item.literal}>
                                <h2>{item.literal}</h2>
                                {item.people.map((person) => (
                                    <div key={person.id}>
                                        <Link
                                            to={`/inmemoria/person/${person.id}/bio`}
                                        >
                                            {person.name}
                                        </Link>
                                    </div>
                                ))}
                            </div>
                        ))}
                    </div>
                ))}
            </div>
        </div>
    )
}

export default ListPage
