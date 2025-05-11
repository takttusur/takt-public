import React from 'react'
import './personPage.css'

interface Person {
    name: string
}

interface PersonPageProps {}

export const PersonPage: React.FC<PersonPageProps> = () => {
    const personData: Person = {
        name: 'Владислав-Александр Старосельский',
    }
    return (
        <div className="inmemoria-person-page">
            <div className="inmemoria-person-page-container">
                <div className="inmemoria-person-page-sidebar">
                    <div className="inmemoria-person-page-card">
                        <div className="inmemoria-person-page-card-header">
                            <img src="" alt="" />
                            <span>{personData.name}</span>
                        </div>
                        <div className="inmemoria-person-page-card-content"></div>
                    </div>
                </div>
                <div className="inmemoria-person-page-content">bio</div>
            </div>
        </div>
    )
}
