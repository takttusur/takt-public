import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react'
import { CarouselPerson } from '../types/CarouselPerson.ts'
import Epigraph from '../types/Epigraph.ts'

export type PersonListItemDto = {
    id: number
    firstName: string
    lastName: string
    maidenName: string
    patronymic: string
    nickname: string
    birthday: string
    deathDay: string
    layout: string
    photoImage: string
}

export type PersonFullModel = PersonListItemDto & {
    biography: string
}

export type Attachment = {
    id: number
    title: string
    subtitle: string
    data: string
    category: number
    attachmentType: number
    order: number
    createdAt: string
    personId: number
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
    layout: string
    photoImage: string
    biography: string
    memories: Attachment[]
    photos: Attachment[]
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

const baseUrl =
    (import.meta.env.VITE_API_URL as string | undefined) ??
    `${import.meta.env.BASE_URL}api`

export const inmemoriaApi = createApi({
    reducerPath: 'inmemoriaApi',
    baseQuery: fetchBaseQuery({ baseUrl: baseUrl }),
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
                    layout: item.layout,
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
        getEpigraph: build.query<Epigraph[], void, PagedResultModel<Epigraph>>({
            query: () => `v1/epigraph`,
            transformResponse: (
                response: PagedResultModel<Epigraph>
            ): Epigraph[] => response.items,
        }),
        getAttachments: build.query<
            Attachment[],
            void,
            PagedResultModel<Attachment>
        >({
            query: () =>
                `v1/attachments?category=Gallery&skip=0&take=50&random=true`,
            transformResponse: (
                response: PagedResultModel<Attachment>
            ): Attachment[] => response.items,
        }),
    }),
})

export const {
    useGetCarouselPersonQuery,
    useGetPersonGroupByLettersQuery,
    useGetPersonByIdQuery,
    useGetEpigraphQuery,
    useGetAttachmentsQuery,
} = inmemoriaApi
