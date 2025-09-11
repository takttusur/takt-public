import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react'
import { CarouselPerson } from '../types/CarouselPerson.ts'

export type PersonListItemDto = {
    id: number
    firstName: string
    lastName: string
    maidenName: string
    patronymic: string
    nickname: string
    birthday: string
    deathDay: string
    backgroundImage: string
    photoImage: string
}

export type PersonDto = {
    id: number
    firstName: string
    lastName: string
    maidenName: string
    patronymic: string
    nickname: string
    birthday: string
    deathDay: string
    backgroundImage: string
    photoImage: string
    bio: string
}

export type PagedResultModel<T> = {
    items: T[]
    totalCount: number
    take: number
    skip: number
}

export type LettersResultModel = {
    letter: string
    persons: PersonListItemDto[]
}

export type MemoriesResultModel = {
    text: string
    author: string
    date: string
    id: number
}

export type PhotosResultModel = {
    id: number
    image: string
    title: string
}

const baseUrl = import.meta.env.VITE_API_URL as string

export const inmemoriaApi = createApi({
    reducerPath: 'inmemoriaApi',
    baseQuery: fetchBaseQuery({ baseUrl: baseUrl ?? '/api' }),
    endpoints: (build) => ({
        getCarouselPerson: build.query<
            CarouselPerson[],
            void,
            PagedResultModel<PersonListItemDto>
        >({
            query: () => `v1/person?random=true`,
            transformResponse: (
                response: PagedResultModel<PersonListItemDto>
            ): CarouselPerson[] =>
                response.items.map((item) => ({
                    id: item.id,
                    name:
                        item.firstName +
                        ' ' +
                        item.lastName +
                        ' ' +
                        item.maidenName,
                    imageSrc: item.photoImage,
                    backgroundImage: item.backgroundImage,
                })),
            keepUnusedDataFor: 60,
        }),
        getPersonGroupByLetters: build.query<LettersResultModel[], void>({
            query: () => `v1/person/letters`,
            keepUnusedDataFor: 60,
        }),
        getPersonById: build.query<PersonDto, string>({
            query: (id) => `v1/person/${id}`,
            keepUnusedDataFor: 60,
        }),
        getMemoriesByPersonId: build.query<MemoriesResultModel[], string>({
            query: (id) => `v1/person/${id}/memories`,
            keepUnusedDataFor: 60,
        }),
        getPhotosByPersonId: build.query<PhotosResultModel[], string>({
            query: (id) => `v1/person/${id}/photos`,
            keepUnusedDataFor: 60,
        }),
    }),
})

export const {
    useGetCarouselPersonQuery,
    useGetPersonGroupByLettersQuery,
    useGetPersonByIdQuery,
    useGetMemoriesByPersonIdQuery,
    useGetPhotosByPersonIdQuery,
} = inmemoriaApi
