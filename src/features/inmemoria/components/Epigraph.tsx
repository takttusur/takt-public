import React, { useState, useEffect } from 'react'

interface EpigraphProps {
    // No props needed for now
}

const Epigraph: React.FC<EpigraphProps> = () => {
    const [epigraph, setEpigraph] = useState<string>('')

    // This would normally fetch data from an API
    useEffect(() => {
        setEpigraph(
            // eslint-disable-next-line max-len
            '<em>И качнется бессмысленной мыслью</em><br><em>Пара фраз долетевших оттуда -</em><br><em>Я тебя никогда не забуду,</em><br><em>Я тебя никогда не увижу...</em><br><em>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;&nbsp; А.Рыбников</em>'
        )
    }, [])

    return (
        <div id="epigraph">
            <p dangerouslySetInnerHTML={{ __html: epigraph }}></p>
            <div id="listlink">
                <i>
                    <a href="list.php">Те, кого с нами нет...</a>
                </i>
            </div>
        </div>
    )
}

export default Epigraph
