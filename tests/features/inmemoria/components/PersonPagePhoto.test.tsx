import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import '@testing-library/jest-dom'
import { PersonPagePhoto } from '../../../../src/features/inmemoria/components/PersonPagePhoto'
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

describe('PersonPagePhoto', () => {
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

        render(<PersonPagePhoto />)

        const img = screen.getByAltText('Загрузка...')
        expect(img).toBeInTheDocument()
        expect(img.tagName.toLowerCase()).toBe('img')
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

        render(<PersonPagePhoto />)

        const img = screen.getByAltText('Загрузка...')
        expect(img).toBeInTheDocument()
        expect(img.tagName.toLowerCase()).toBe('img')
    })

    it('should render no id provided state', () => {
        // Mock the useParams hook to return no ID
        vi.spyOn(reactRouterDom, 'useParams').mockReturnValue({})

        render(<PersonPagePhoto />)

        const img = screen.getByAltText('Неправильный ID')
        expect(img).toBeInTheDocument()
        expect(img.tagName.toLowerCase()).toBe('img')
    })

    it('should render photo', () => {
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

        render(<PersonPagePhoto />)

        const img = screen.getByAltText('Фото профиля')
        expect(img).toBeInTheDocument()
        expect(img.tagName.toLowerCase()).toBe('img')
        expect(img).toHaveAttribute('src', 'photo.jpg')
    })
})
