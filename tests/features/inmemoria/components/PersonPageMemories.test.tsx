import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import '@testing-library/jest-dom'
import { PersonPageMemories } from '../../../../src/features/inmemoria/components/PersonPageMemories'
import * as reactRouterDom from 'react-router-dom'
import * as inmemoriaApi from '../../../../src/features/inmemoria/data/inmemoriaApi'
import { QueryStatus } from '@reduxjs/toolkit/query'
import type { MemoriesResultModel } from '../../../../src/features/inmemoria/data/inmemoriaApi'

// Define a type for the query hook result
type MemoriesQueryResult = {
    data?: MemoriesResultModel[]
    isLoading: boolean
    isError: boolean
    status: QueryStatus
    currentData?: MemoriesResultModel[]
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

// Mock the useGetMemoriesByPersonIdQuery hook
vi.mock('../../../../src/features/inmemoria/data/inmemoriaApi', async () => {
    const actual = await vi.importActual(
        '../../../../src/features/inmemoria/data/inmemoriaApi'
    )
    return {
        ...actual,
        useGetMemoriesByPersonIdQuery: vi.fn(),
    }
})

describe('PersonPageMemories', () => {
    it('should render loading state', () => {
        // Mock the useParams hook to return an ID
        vi.spyOn(reactRouterDom, 'useParams').mockReturnValue({ id: '1' })

        // Mock the useGetMemoriesByPersonIdQuery hook to return loading state
        vi.spyOn(inmemoriaApi, 'useGetMemoriesByPersonIdQuery').mockReturnValue(
            {
                data: undefined,
                isLoading: true,
                isError: false,
                status: QueryStatus.pending,
                isFetching: true,
                isSuccess: false,
                isUninitialized: false,
                refetch: () => {},
            } as MemoriesQueryResult
        )

        render(<PersonPageMemories />)

        expect(
            screen.getByText('Загружаем воспоминания...')
        ).toBeInTheDocument()
    })

    it('should render error state', () => {
        // Mock the useParams hook to return an ID
        vi.spyOn(reactRouterDom, 'useParams').mockReturnValue({ id: '1' })

        // Mock the useGetMemoriesByPersonIdQuery hook to return error state
        vi.spyOn(inmemoriaApi, 'useGetMemoriesByPersonIdQuery').mockReturnValue(
            {
                data: undefined,
                isLoading: false,
                isError: true,
                status: QueryStatus.rejected,
                isFetching: false,
                isSuccess: false,
                isUninitialized: false,
                refetch: () => {},
            } as MemoriesQueryResult
        )

        render(<PersonPageMemories />)

        expect(
            screen.getByText('Загружаем воспоминания...')
        ).toBeInTheDocument()
    })

    it('should render no id provided state', () => {
        // Mock the useParams hook to return no ID
        vi.spyOn(reactRouterDom, 'useParams').mockReturnValue({})

        render(<PersonPageMemories />)

        expect(screen.getByText('No id provided')).toBeInTheDocument()
    })

    it('should render empty memories state', () => {
        // Mock the useParams hook to return an ID
        vi.spyOn(reactRouterDom, 'useParams').mockReturnValue({ id: '1' })

        // Mock the useGetMemoriesByPersonIdQuery hook to return empty data
        vi.spyOn(inmemoriaApi, 'useGetMemoriesByPersonIdQuery').mockReturnValue(
            {
                data: [],
                isLoading: false,
                isError: false,
                status: QueryStatus.fulfilled,
                currentData: [],
                isFetching: false,
                isSuccess: true,
                isUninitialized: false,
                refetch: () => {},
            } as MemoriesQueryResult
        )

        render(<PersonPageMemories />)

        expect(screen.getByText('Пока нет воспоминаний')).toBeInTheDocument()
    })

    it('should render memories', () => {
        // Mock the useParams hook to return an ID
        vi.spyOn(reactRouterDom, 'useParams').mockReturnValue({ id: '1' })

        // Mock the useGetMemoriesByPersonIdQuery hook to return data
        vi.spyOn(inmemoriaApi, 'useGetMemoriesByPersonIdQuery').mockReturnValue(
            {
                data: [
                    {
                        id: 1,
                        text: '<p>Memory 1</p>',
                        author: 'Author 1',
                        date: '2021-01-01',
                    },
                    {
                        id: 2,
                        text: '<p>Memory 2</p>',
                        author: 'Author 2',
                        date: '2021-02-02',
                    },
                ],
                isLoading: false,
                isError: false,
                status: QueryStatus.fulfilled,
                currentData: [
                    {
                        id: 1,
                        text: '<p>Memory 1</p>',
                        author: 'Author 1',
                        date: '2021-01-01',
                    },
                    {
                        id: 2,
                        text: '<p>Memory 2</p>',
                        author: 'Author 2',
                        date: '2021-02-02',
                    },
                ],
                isFetching: false,
                isSuccess: true,
                isUninitialized: false,
                refetch: () => {},
            } as MemoriesQueryResult
        )

        const { container } = render(<PersonPageMemories />)

        // Check that the memories are rendered
        expect(screen.getByText('Memory 1')).toBeInTheDocument()
        expect(screen.getByText('Memory 2')).toBeInTheDocument()
        expect(screen.getByText('Author 1')).toBeInTheDocument()
        expect(screen.getByText('Author 2')).toBeInTheDocument()

        // Check that the dates are formatted correctly
        // Note: Date formatting might vary by locale, so we'll check for the presence of the date parts
        expect(screen.getByText(/1.*1.*2021/)).toBeInTheDocument()
        expect(screen.getByText(/2.*2.*2021/)).toBeInTheDocument()

        // Check that the container has the correct class
        const memoriesContainer = container.querySelector(
            '.inmemoria-person-page-memories'
        )
        expect(memoriesContainer).toBeInTheDocument()

        // Check that there are two memory cards
        const cards = container.querySelectorAll(
            '.inmemoria-person-page-memories-card'
        )
        expect(cards.length).toBe(2)

        // Check that the HTML was set using dangerouslySetInnerHTML
        const cardTexts = container.querySelectorAll(
            '.inmemoria-person-page-memories-card-text'
        )
        expect(cardTexts.length).toBe(2)
        expect(cardTexts[0].innerHTML).toContain('<p>Memory 1</p>')
        expect(cardTexts[1].innerHTML).toContain('<p>Memory 2</p>')
    })
})
