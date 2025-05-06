import React, { useState } from 'react'

interface SearchProps {
    // No props needed for now
}

const Search: React.FC<SearchProps> = () => {
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
        <div id="mainsearch">
            <form
                className="searchform"
                action="search.php"
                method="get"
                onSubmit={handleSearchSubmit}
            >
                <input
                    id="textinput"
                    type="text"
                    name="name"
                    placeholder="Фамилия, Имя, Прозвище"
                    value={search}
                    onChange={handleSearchChange}
                />
                <input id="opsubmit" type="submit" value="" />
            </form>
        </div>
    )
}

export default Search
