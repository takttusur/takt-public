import './inmemoria.css'
import Footer from './Footer'
import Header from './Header'
import MainPage from './MainPage'
import AboutPage from './AboutPage'
import ListPage from './ListPage'
import { Routes, Route } from 'react-router-dom'
import { PersonPage } from './PersonPage.tsx'

const InMemoriaPage = (): JSX.Element => {
    return (
        <div className="inmemoria-page">
            <Header />
            <Routes>
                <Route index element={<MainPage />} />
                <Route path="about" element={<AboutPage />} />
                <Route path="list" element={<ListPage />} />
                <Route path="person/*" element={<PersonPage />} />
            </Routes>
            <Footer />
        </div>
    )
}

export default InMemoriaPage
