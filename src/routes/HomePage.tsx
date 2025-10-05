import { Link as RouterLink } from 'react-router-dom'
import taktLogo from '../assets/takt.svg'
import { FC } from 'react'

const HomePage: FC = () => {
    return (
        <div
            style={{
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
                justifyContent: 'center',
                height: '100vh',
                textAlign: 'center',
                padding: '20px',
            }}
        >
            <img
                src={taktLogo}
                alt="TAKT Logo"
                style={{
                    maxWidth: '300px',
                    marginBottom: '30px',
                }}
            />
            <RouterLink
                to="/inmemoria"
                style={{
                    marginTop: '20px',
                    textDecoration: 'none',
                    padding: '10px 20px',
                    borderRadius: '5px',
                    backgroundColor: '#f0f0f0',
                    color: '#333',
                    fontWeight: 'bold',
                    transition: 'all 0.3s ease',
                }}
            >
                <span>InMemoria</span>
            </RouterLink>
        </div>
    )
}
export default HomePage
