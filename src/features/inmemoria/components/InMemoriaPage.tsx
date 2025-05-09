import './inmemoria.css'
import Footer from './Footer'
import Header from './Header'
import PeopleCarousel from './PeopleCarousel.tsx'

const InMemoriaPage = (): JSX.Element => {
    return (
        <div className="inmemoria-page">
            <Header />
            <div className="inmemoria-content">
                <PeopleCarousel />
            </div>
            <Footer />
        </div>
    )
}

export default InMemoriaPage
