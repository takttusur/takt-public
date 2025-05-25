import React, { useState, useEffect } from 'react'
import './epigraph.css'

const Epigraph: React.FC = () => {
    const [epigraph, setEpigraph] = useState<string>('')

    // This would normally fetch data from an API
    useEffect(() => {
        setEpigraph(
            // eslint-disable-next-line max-len
            '<em>И качнется бессмысленной мыслью</em><br><em>Пара фраз долетевших оттуда -</em><br><em>Я тебя никогда не забуду,</em><br><em>Я тебя никогда не увижу...</em><br><em>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;&nbsp; А.Рыбников</em>'
        )
    }, [])

    return (
        <div className="inmemoria-epigraph">
            <p dangerouslySetInnerHTML={{ __html: epigraph }}></p>
        </div>
    )
}

export default Epigraph
