import './inmemoria.css'

const ListPage = (): JSX.Element => {
    return (
        <div className="inmemoria-content">
            <h1>InMemoria List</h1>
            <p>
                This is the List page for the InMemoria feature. It displays a
                list of all memorials.
            </p>
            <ul>
                <li>Memorial 1</li>
                <li>Memorial 2</li>
                <li>Memorial 3</li>
                <li>Memorial 4</li>
                <li>Memorial 5</li>
            </ul>
        </div>
    )
}

export default ListPage
