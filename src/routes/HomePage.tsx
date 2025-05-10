import { Link as RouterLink } from 'react-router-dom'
import taktLogo from '../assets/takt.svg'

export default function HomePage(): JSX.Element {
    return (
        <div>
            <img src={taktLogo} alt="TAKT Logo" />
            <RouterLink to="/inmemoria">
                <span>InMemoria</span>
            </RouterLink>
        </div>
    )
}
