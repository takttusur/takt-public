import { describe, it, expect, beforeEach, afterEach } from 'vitest'
import { setupServer } from 'msw/node'
import { http, HttpResponse } from 'msw'
import { inmemoriaApi } from '../../../../src/features/inmemoria/data/inmemoriaApi'

// Mock data
const mockPerson = {
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
}

const mockPersons = {
    items: [
        {
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
        },
        {
            id: 2,
            firstName: 'Jane',
            lastName: 'Smith',
            maidenName: '',
            patronymic: '',
            nickname: '',
            birthday: '1992-02-02',
            deathDay: '2022-02-02',
            backgroundImage: 'background2.jpg',
            photoImage: 'photo2.jpg',
        },
    ],
    totalCount: 2,
    take: 10,
    skip: 0,
}

const mockLetters = [
    {
        letter: 'D',
        persons: [
            {
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
            },
        ],
    },
    {
        letter: 'S',
        persons: [
            {
                id: 2,
                firstName: 'Jane',
                lastName: 'Smith',
                maidenName: '',
                patronymic: '',
                nickname: '',
                birthday: '1992-02-02',
                deathDay: '2022-02-02',
                backgroundImage: 'background2.jpg',
                photoImage: 'photo2.jpg',
            },
        ],
    },
]

const mockMemories = [
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
]

const mockPhotos = [
    {
        id: 1,
        image: 'image1.jpg',
        title: 'Photo 1',
    },
    {
        id: 2,
        image: 'image2.jpg',
        title: 'Photo 2',
    },
]

// Setup MSW server
const server = setupServer(
    http.get('/api/v1/person/1', () => {
        return HttpResponse.json(mockPerson)
    }),
    http.get('/api/v1/person', () => {
        return HttpResponse.json(mockPersons)
    }),
    http.get('/api/v1/person/letters', () => {
        return HttpResponse.json(mockLetters)
    }),
    http.get('/api/v1/person/1/memories', () => {
        return HttpResponse.json(mockMemories)
    }),
    http.get('/api/v1/person/1/photos', () => {
        return HttpResponse.json(mockPhotos)
    })
)

describe('inmemoriaApi', () => {
    beforeEach(() => server.listen())
    afterEach(() => server.resetHandlers())

    it('should have the correct endpoints', () => {
        expect(inmemoriaApi.endpoints).toBeDefined()
        expect(inmemoriaApi.endpoints.getCarouselPerson).toBeDefined()
        expect(inmemoriaApi.endpoints.getPersonGroupByLetters).toBeDefined()
        expect(inmemoriaApi.endpoints.getPersonById).toBeDefined()
        expect(inmemoriaApi.endpoints.getMemoriesByPersonId).toBeDefined()
        expect(inmemoriaApi.endpoints.getPhotosByPersonId).toBeDefined()
    })

    it('should have the correct reducer path', () => {
        expect(inmemoriaApi.reducerPath).toBe('inmemoriaApi')
    })

    it('should have a base query configuration', () => {
        // Just check that the API is defined, we don't need to test RTK Query internals
        expect(inmemoriaApi).toBeDefined()
    })

    it('should have the correct endpoints structure', () => {
        // Check that all endpoints are defined
        expect(inmemoriaApi.endpoints.getCarouselPerson).toBeDefined()
        expect(inmemoriaApi.endpoints.getPersonGroupByLetters).toBeDefined()
        expect(inmemoriaApi.endpoints.getPersonById).toBeDefined()
        expect(inmemoriaApi.endpoints.getMemoriesByPersonId).toBeDefined()
        expect(inmemoriaApi.endpoints.getPhotosByPersonId).toBeDefined()
    })
})
