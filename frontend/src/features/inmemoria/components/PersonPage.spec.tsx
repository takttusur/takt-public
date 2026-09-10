import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import '@testing-library/jest-dom'
import { Provider } from 'react-redux'
import { configureStore } from '@reduxjs/toolkit'
import * as reactRouterDom from 'react-router-dom'
import * as inmemoriaApi from '../data/inmemoriaApi'
import { QueryStatus } from '@reduxjs/toolkit/query'
import { PersonPage } from './PersonPage'
import React from 'react'
import { MemoryRouter } from 'react-router-dom'

// Minimal store for RTK Query
const store = configureStore({
    reducer: {
        [inmemoriaApi.inmemoriaApi.reducerPath]:
            inmemoriaApi.inmemoriaApi.reducer,
    },
    middleware: (getDefaultMiddleware) =>
        getDefaultMiddleware().concat(inmemoriaApi.inmemoriaApi.middleware),
})

const renderWithProvider = (ui: React.ReactElement) =>
    render(
        <MemoryRouter>
            <Provider store={store}>{ui}</Provider>
        </MemoryRouter>
    )

vi.mock('react-router-dom', async () => {
    const actual =
        await vi.importActual<typeof reactRouterDom>('react-router-dom')
    return {
        ...actual,
        useParams: vi.fn(),
        Link: actual.Link,
        Routes: actual.Routes,
        Route: actual.Route,
        Navigate: actual.Navigate,
        useNavigate: actual.useNavigate,
    }
})

vi.mock('../../data/inmemoriaApi', async () => {
    const actual = await vi.importActual<typeof inmemoriaApi>(
        '../../data/inmemoriaApi'
    )
    return {
        ...actual,
        useGetPersonByIdQuery: vi.fn(),
        inmemoriaApi: actual.inmemoriaApi,
    }
})

describe('PersonPage', () => {
    beforeEach(() => {
        vi.clearAllMocks()
    })

    it('renders "No id provided" if params.id is missing', () => {
        vi.spyOn(reactRouterDom, 'useParams').mockReturnValue({})
        renderWithProvider(<PersonPage />)
        expect(screen.getByText('No id provided')).toBeInTheDocument()
    })

    it('renders loading state', () => {
        vi.spyOn(reactRouterDom, 'useParams').mockReturnValue({ id: '1' })
        vi.spyOn(inmemoriaApi, 'useGetPersonByIdQuery').mockReturnValue({
            data: undefined,
            isLoading: true,
            isError: false,
            status: QueryStatus.pending,
            isFetching: true,
            isSuccess: false,
            isUninitialized: false,
            refetch: () => undefined!,
        })
        renderWithProvider(<PersonPage />)
        expect(screen.getByText('Загрузка...')).toBeInTheDocument()
    })

    it('renders error state', () => {
        vi.spyOn(reactRouterDom, 'useParams').mockReturnValue({ id: '1' })
        vi.spyOn(inmemoriaApi, 'useGetPersonByIdQuery').mockReturnValue({
            data: undefined,
            isLoading: false,
            isError: true,
            status: QueryStatus.rejected,
            isFetching: false,
            isSuccess: false,
            isUninitialized: false,
            refetch: () => undefined!,
        })
        renderWithProvider(<PersonPage />)
        expect(screen.getByText('Загрузка...')).toBeInTheDocument()
    })

    it('renders person page with sidebar and title', () => {
        vi.spyOn(reactRouterDom, 'useParams').mockReturnValue({ id: '1' })
        vi.spyOn(inmemoriaApi, 'useGetPersonByIdQuery').mockReturnValue({
            data: {
                id: 1,
                firstName: 'John',
                lastName: 'Doe',
                maidenName: '',
                patronymic: '',
                nickname: '',
                birthday: '',
                deathDay: '',
                layout: 'default',
                photoImage: '/photo.jpg',
                biography: '',
                memories: [],
                photos: [],
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
                birthday: '',
                deathDay: '',
                layout: 'default',
                photoImage: '/photo.jpg',
                biography: '',
                memories: [],
                photos: [],
            },
            isFetching: false,
            isSuccess: true,
            isUninitialized: false,
            refetch: () => undefined!,
        })
        renderWithProvider(<PersonPage />)
        expect(screen.getAllByText('John Doe')[0]).toBeInTheDocument()
        expect(screen.getByText('Воспоминания')).toBeInTheDocument()
        expect(screen.getByText('Галерея')).toBeInTheDocument()
        expect(screen.getByText('Маршруты')).toBeInTheDocument()
        expect(screen.getByText('Жизнь')).toBeInTheDocument()
        expect(screen.getByText('Фото')).toBeInTheDocument()
    })
})
