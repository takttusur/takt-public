import React, { useState } from 'react'
import './search.css'

const Search: React.FC = () => {
    const [search, setSearch] = useState<string>('')

    const handleSearchChange = (
        e: React.ChangeEvent<HTMLInputElement>
    ): void => {
        setSearch(e.target.value)
    }

    const handleSearchSubmit = (e: React.FormEvent<HTMLFormElement>): void => {
        e.preventDefault()
        // Handle search submission
        console.log('Search submitted:', search)
    }

    return (
        <form
            style={{ display: 'none' }}
            className="inmemoria-header-search-form"
            action="search.php"
            method="get"
            onSubmit={handleSearchSubmit}
        >
            <input
                disabled={true}
                type="text"
                name="name"
                placeholder="Фамилия, Имя, Прозвище"
                value={search}
                onChange={handleSearchChange}
            />
            <input type="submit" value="" />
        </form>
    )
}

export default Search
