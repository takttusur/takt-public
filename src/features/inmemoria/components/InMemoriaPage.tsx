import React from 'react'
import '../../inmemoria/styles/main.css'
import '../../inmemoria/styles/index.css'
import Header from './Header'
import Search from './Search'
import Epigraph from './Epigraph'
import PeopleCarousel from './PeopleCarousel'
import GalleryCarousel from './GalleryCarousel'
import Footer from './Footer'

const InMemoriaPage = (): JSX.Element => {
    return (
        <div id="ie_fix" className="inmemoria-page">
            <Header />
            <Epigraph />
            <Search />

            <div id="list_and_photo" style={{ textAlign: 'center' }}>
                <PeopleCarousel />
                <GalleryCarousel />
            </div>

            <Footer />
        </div>
    )
}

export default InMemoriaPage
