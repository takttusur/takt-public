import { JSX, useEffect, useState } from 'react'
import './listPage.css'
import { Link } from 'react-router-dom'
import {
    LettersResultModel,
    useGetPersonGroupByLettersQuery,
} from '../data/inmemoriaApi.ts'

const ListPage = (): JSX.Element => {
    const { data, isLoading, isError } = useGetPersonGroupByLettersQuery()
    const [people, setPeople] = useState<LettersResultModel[][]>([])

    const columnsCount = 3
    useEffect(() => {
        if (isLoading || isError || !data) return

        const itemsPerColumn = Math.ceil(data.length / columnsCount)
        const groupedPeople = data.reduce((acc, curr, i) => {
            const columnIndex = Math.floor(i / itemsPerColumn)
            if (!acc[columnIndex]) {
                acc[columnIndex] = []
            }
            acc[columnIndex].push(curr)
            return acc
        }, [] as LettersResultModel[][])
        setPeople(groupedPeople)
    }, [data, isLoading, isError])

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
                            <div key={item.letter}>
                                <h2>{item.letter}</h2>
                                {item.persons.map((person) => (
                                    <div key={person.id}>
                                        <Link
                                            to={`/inmemoria/person/${person.id}/bio`}
                                        >
                                            {person.firstName} {person.lastName}
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
