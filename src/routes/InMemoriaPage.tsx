import InMemoriaPage from '../features/inmemoria/components/InMemoriaPage'

// This is a wrapper component that ensures the InMemoriaPage doesn't use any parent components
const InMemoriaPageRoute = (): JSX.Element => {
    return <InMemoriaPage />
}

export default InMemoriaPageRoute
