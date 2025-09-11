import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import '@testing-library/jest-dom'
import { PersonPageBio } from '../../../../src/features/inmemoria/components/PersonPageBio'
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

describe('PersonPageBio', () => {
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

        render(<PersonPageBio />)

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

        render(<PersonPageBio />)

        expect(screen.getByText('Загрузка...')).toBeInTheDocument()
    })

    it('should render no id provided state', () => {
        // Mock the useParams hook to return no ID
        vi.spyOn(reactRouterDom, 'useParams').mockReturnValue({})

        render(<PersonPageBio />)

        expect(screen.getByText('No id provided')).toBeInTheDocument()
    })

    it('should render bio content', () => {
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

        const { container } = render(<PersonPageBio />)

        // Check that the bio content is rendered
        expect(screen.getByText('Bio text')).toBeInTheDocument()

        // Check that the container has the correct class
        const bioContainer = container.querySelector(
            '.inmemoria-person-page-bio'
        )
        expect(bioContainer).toBeInTheDocument()

        // Check that the HTML was set using dangerouslySetInnerHTML
        expect(bioContainer?.innerHTML).toContain('<p>Bio text</p>')
    })
})
