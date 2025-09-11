/* eslint-disable @typescript-eslint/no-unsafe-assignment */
import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import '@testing-library/jest-dom'
import { PersonPage } from '../../../../src/features/inmemoria/components/PersonPage'
import * as reactRouterDom from 'react-router-dom'
import * as inmemoriaApi from '../../../../src/features/inmemoria/data/inmemoriaApi'
import { QueryStatus } from '@reduxjs/toolkit/query'
import type { PersonDto } from '../../../../src/features/inmemoria/data/inmemoriaApi'

// Define a type for the query hook result
type PersonQueryResult = {
    data?: PersonDto
    isLoading: boolean
    isError: boolean
    status: QueryStatus
    currentData?: PersonDto
    isFetching: boolean
    isSuccess: boolean
    isUninitialized: boolean
    refetch: () => void
}

// Mock the useParams hook
vi.mock('react-router-dom', async () => {
    const actual = await vi.importActual('react-router-dom')
    return {
        ...actual,
        useParams: vi.fn(),
        Routes: vi.fn(({ children }) => (
            <div data-testid="routes">{children}</div>
        )),
        Route: vi.fn(({ path, element }) => (
            <div data-testid={`route-${path || 'index'}`}>{element}</div>
        )),
        Navigate: vi.fn(({ to }) => (
            <div data-testid={`navigate-${to}`}>Navigate to {to}</div>
        )),
        Link: vi.fn(({ to, children, ...props }) => (
            <a href={to} data-testid={`link-${to}`} {...props}>
                {children}
            </a>
        )),
    }
})

// Mock the useGetPersonByIdQuery hook
vi.mock('../../../../src/features/inmemoria/data/inmemoriaApi', async () => {
    const actual = await vi.importActual(
        '../../../../src/features/inmemoria/data/inmemoriaApi'
    )
    return {
        ...actual,
        useGetPersonByIdQuery: vi.fn(),
    }
})

// Mock the child components
vi.mock(
    '../../../../src/features/inmemoria/components/PersonPagePhoto',
    () => ({
        PersonPagePhoto: () => (
            <div data-testid="person-page-photo">PersonPagePhoto</div>
        ),
    })
)

vi.mock(
    '../../../../src/features/inmemoria/components/PersonPageGallery',
    () => ({
        PersonPageGallery: () => (
            <div data-testid="person-page-gallery">PersonPageGallery</div>
        ),
    })
)

vi.mock(
    '../../../../src/features/inmemoria/components/PersonPageHikes',
    () => ({
        PersonPageHikes: () => (
            <div data-testid="person-page-hikes">PersonPageHikes</div>
        ),
    })
)

vi.mock('../../../../src/features/inmemoria/components/PersonPageBio', () => ({
    PersonPageBio: () => <div data-testid="person-page-bio">PersonPageBio</div>,
}))

vi.mock(
    '../../../../src/features/inmemoria/components/PersonPageMemories',
    () => ({
        PersonPageMemories: () => (
            <div data-testid="person-page-memories">PersonPageMemories</div>
        ),
    })
)

describe('PersonPage', () => {
    it('should render loading state', () => {
        // Mock the useParams hook to return an ID
        vi.spyOn(reactRouterDom, 'useParams').mockReturnValue({ id: '1' })

        // Mock the useGetPersonByIdQuery hook to return loading state
        vi.spyOn(inmemoriaApi, 'useGetPersonByIdQuery').mockReturnValue({
            data: undefined,
            isLoading: true,
            isError: false,
            status: QueryStatus.pending,
            isFetching: true,
            isSuccess: false,
            isUninitialized: false,
            refetch: () => {},
        } as PersonQueryResult)

        render(<PersonPage />)

        expect(screen.getByText('Загрузка...')).toBeInTheDocument()
    })

    it('should render error state', () => {
        // Mock the useParams hook to return an ID
        vi.spyOn(reactRouterDom, 'useParams').mockReturnValue({ id: '1' })

        // Mock the useGetPersonByIdQuery hook to return error state
        vi.spyOn(inmemoriaApi, 'useGetPersonByIdQuery').mockReturnValue({
            data: undefined,
            isLoading: false,
            isError: true,
            status: QueryStatus.rejected,
            isFetching: false,
            isSuccess: false,
            isUninitialized: false,
            refetch: () => {},
        } as PersonQueryResult)

        render(<PersonPage />)

        expect(screen.getByText('Загрузка...')).toBeInTheDocument()
    })

    it('should render no id provided state', () => {
        // Mock the useParams hook to return no ID
        vi.spyOn(reactRouterDom, 'useParams').mockReturnValue({})

        render(<PersonPage />)

        expect(screen.getByText('No id provided')).toBeInTheDocument()
    })

    it('should render person page with routes', () => {
        // Mock the useParams hook to return an ID
        vi.spyOn(reactRouterDom, 'useParams').mockReturnValue({ id: '1' })

        // Mock the useGetPersonByIdQuery hook to return data
        vi.spyOn(inmemoriaApi, 'useGetPersonByIdQuery').mockReturnValue({
            data: {
                id: 1,
                firstName: 'John',
                lastName: 'Doe',
                maidenName: '',
                patronymic: '',
                nickname: '',
                birthday: '1990-01-01',
                deathDay: '2020-01-01',
                backgroundImage: 'background.jpg',
                photoImage: 'photo.jpg',
                bio: '<p>Bio text</p>',
            },
            isLoading: false,
            isError: false,
            status: QueryStatus.fulfilled,
            currentData: {
                id: 1,
                firstName: 'John',
                lastName: 'Doe',
                maidenName: '',
                patronymic: '',
                nickname: '',
                birthday: '1990-01-01',
                deathDay: '2020-01-01',
                backgroundImage: 'background.jpg',
                photoImage: 'photo.jpg',
                bio: '<p>Bio text</p>',
            },
            isFetching: false,
            isSuccess: true,
            isUninitialized: false,
            refetch: () => {},
        } as PersonQueryResult)

        render(<PersonPage />)

        // Check that the routes are rendered
        expect(screen.getByTestId('routes')).toBeInTheDocument()
        expect(screen.getByTestId('route-photo')).toBeInTheDocument()
        expect(screen.getByTestId('route-gallery')).toBeInTheDocument()
        expect(screen.getByTestId('route-hikes')).toBeInTheDocument()
        expect(screen.getByTestId('route-bio')).toBeInTheDocument()
        expect(screen.getByTestId('route-memories')).toBeInTheDocument()
        expect(screen.getByTestId('route-index')).toBeInTheDocument()
        expect(screen.getByTestId('navigate-photo')).toBeInTheDocument()

        // Check that the links are rendered
        expect(
            screen.getAllByTestId('link-/inmemoria/person/1/photo').length
        ).toBeGreaterThan(0)
        expect(
            screen.getAllByTestId('link-/inmemoria/person/1/bio').length
        ).toBeGreaterThan(0)
        expect(
            screen.getByTestId('link-/inmemoria/person/1/memories')
        ).toBeInTheDocument()
        expect(
            screen.getByTestId('link-/inmemoria/person/1/gallery')
        ).toBeInTheDocument()
        expect(
            screen.getByTestId('link-/inmemoria/person/1/hikes')
        ).toBeInTheDocument()

        // Check that the sidebar and content are rendered with the correct classes
        // Use getAllByText because there might be multiple elements with the same text
        const johnDoeElements = screen.getAllByText('John Doe')
        expect(johnDoeElements.length).toBeGreaterThan(0)

        // Check that at least one of the elements has the correct class
        const hasCardHeaderName = johnDoeElements.some(
            (el) =>
                el.closest('.inmemoria-person-page-card-header-name') !== null
        )
        expect(hasCardHeaderName).toBe(true)

        const hasPageTitle = johnDoeElements.some(
            (el) => el.closest('.inmemoria-person-page-title') !== null
        )
        expect(hasPageTitle).toBe(true)

        // Verify that the document contains the expected elements
        expect(
            document.querySelector('.inmemoria-person-page-card-header-name')
        ).not.toBeNull()
        expect(
            document.querySelector('.inmemoria-person-page-title')
        ).not.toBeNull()
    })
})
