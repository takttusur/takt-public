/* eslint-disable max-len */
import React, { useState, useEffect } from 'react'
import './personPageBio.css'
import { inmemoriaFakeApi } from '../data/api/inmemoriaFakeApi.ts'

interface PersonPageBioProps {}

export const PersonPageBio: React.FC<PersonPageBioProps> = () => {
    const [bio, setBio] = useState<string>('')
    const [loading, setLoading] = useState(true)

    useEffect(() => {
        const fetchData = async (): Promise<void> => {
            try {
                const data = await inmemoriaFakeApi.getBio()
                setBio(data)
            } catch (error) {
                console.error('Error fetching bio:', error)
            } finally {
                setLoading(false)
            }
        }

        void fetchData()
    }, [])
    if (loading) {
        return <div>Loading bio...</div>
    }

    if (!bio) {
        return <div>No bio available</div>
    }

    return (
        <div
            className="inmemoria-person-page-bio"
            dangerouslySetInnerHTML={{ __html: bio }}
        />
    )
}
