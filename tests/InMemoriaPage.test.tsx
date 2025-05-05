import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import App from '../src/App'

// Mock the components
vi.mock('../src/components/Navigation/MainMenu', () => ({
    default: () => <div data-testid="main-menu">MainMenu</div>,
}))

vi.mock('../src/components/Footer', () => ({
    default: () => <div data-testid="footer">Footer</div>,
}))

vi.mock('../src/features/inmemoria/components/InMemoriaPage', () => ({
    default: () => <div data-testid="inmemoria-page">InMemoriaPage</div>,
}))

// Mock the routes
vi.mock('../src/routes/', () => ({
    default: {
        getRoutes: () => [
            {
                path: '/inmemoria',
                element: (
                    <div data-testid="inmemoria-route">InMemoriaRoute</div>
                ),
            },
            {
                path: '/home',
                element: <div data-testid="home-route">HomeRoute</div>,
            },
        ],
    },
}))

// Mock YandexMetrikaCounter
vi.mock('../src/components/Common/YandexMetrikaCounter.tsx', () => ({
    default: () => null,
}))

// Mock EnvironmentService
vi.mock('../src/services/EnvironmentService', () => ({
    default: {
        YandexMetrikaEnabled: false,
        YandexMetrikaId: '12345',
    },
}))

describe('InMemoriaPage', () => {
    it('should not render MainMenu and Footer when on /inmemoria route', () => {
        render(
            <MemoryRouter initialEntries={['/inmemoria']}>
                <App />
            </MemoryRouter>
        )

        // MainMenu and Footer should not be in the document
        expect(screen.queryByTestId('main-menu')).not.toBeInTheDocument()
        expect(screen.queryByTestId('footer')).not.toBeInTheDocument()

        // InMemoriaRoute should be in the document
        expect(screen.getByTestId('inmemoria-route')).toBeInTheDocument()
    })

    it('should render MainMenu and Footer when on other routes', () => {
        render(
            <MemoryRouter initialEntries={['/home']}>
                <App />
            </MemoryRouter>
        )

        // MainMenu and Footer should be in the document
        expect(screen.getByTestId('main-menu')).toBeInTheDocument()
        expect(screen.getByTestId('footer')).toBeInTheDocument()

        // HomeRoute should be in the document
        expect(screen.getByTestId('home-route')).toBeInTheDocument()
    })
})
