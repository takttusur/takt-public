import React from 'react'

interface PersonPagePhotoProps {}

export const PersonPagePhoto: React.FC<PersonPagePhotoProps> = () => {
    return (
        <img
            style={{ border: 'none' }}
            alt="Profile photo"
            src="src/features/inmemoria/images/fake_img1.png"
        />
    )
}
