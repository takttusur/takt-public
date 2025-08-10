import { http, HttpResponse } from 'msw'
import {
    PagedResultModel,
    PersonListItemDto,
    MemoriesResultModel,
    PhotosResultModel,
} from '../features/inmemoria/data/inmemoriaApi.ts'

const fakePersons: (PersonListItemDto & {
    memories: MemoriesResultModel[]
    photos: PhotosResultModel[]
})[] = [
    {
        id: 1,
        firstName: 'Пётр',
        lastName: 'Петров',
        maidenName: '',
        patronymic: '',
        nickname: '',
        birthday: '1980-01-01',
        deathDay: '2020-01-01',
        photoImage: 'src/features/inmemoria/images/fake_img2.png',
        backgroundImage: 'memo1.gif',
        memories: [
            {
                id: 1,
                text: 'Помню его как отзывчивого человека',
                author: 'Иван Иванов',
                date: '2020-02-01',
            },
        ],
        photos: [
            {
                id: 1,
                image: 'src/features/inmemoria/images/gallery/photo1.jpg',
                title: 'На отдыхе',
            },
            {
                id: 2,
                image: 'src/features/inmemoria/images/gallery/photo2.jpg',
                title: 'С семьей',
            },
            {
                id: 3,
                image: 'src/features/inmemoria/images/gallery/photo3.jpg',
                title: 'На работе',
            },
        ],
    },
    {
        id: 2,
        firstName: 'Владислав-Александр',
        lastName: 'Старосельсконевский',
        maidenName: '',
        patronymic: '',
        nickname: '',
        birthday: '1975-02-15',
        deathDay: '2019-11-20',
        photoImage: 'src/features/inmemoria/images/fake_img1.png',
        backgroundImage: 'memo1.gif',
        memories: [
            {
                id: 2,
                text: 'Великолепный человек и профессионал',
                author: 'Петр Сидоров',
                date: '2019-12-01',
            },
        ],
        photos: [
            {
                id: 4,
                image: 'src/features/inmemoria/images/gallery/photo4.jpg',
                title: 'В походе',
            },
            {
                id: 5,
                image: 'src/features/inmemoria/images/gallery/photo5.jpg',
                title: 'День рождения',
            },
        ],
    },
    {
        id: 3,
        firstName: 'Михаил',
        lastName: 'Иванов',
        maidenName: '',
        patronymic: '',
        nickname: '',
        birthday: '1982-03-10',
        deathDay: '2021-05-05',
        photoImage: 'src/features/inmemoria/images/fake_img2.png',
        backgroundImage: 'memo2.gif',
        memories: [
            {
                id: 3,
                text: 'Замечательный друг и товарищ',
                author: 'Андрей Петров',
                date: '2021-06-10',
            },
        ],
        photos: [
            {
                id: 6,
                image: 'src/features/inmemoria/images/gallery/photo6.jpg',
                title: 'На природе',
            },
            {
                id: 7,
                image: 'src/features/inmemoria/images/gallery/photo7.jpg',
                title: 'С друзьями',
            },
        ],
    },
    {
        id: 4,
        firstName: 'Елена',
        lastName: 'Смирнова',
        maidenName: '',
        patronymic: '',
        nickname: '',
        birthday: '1985-04-20',
        deathDay: '2022-02-15',
        photoImage: 'src/features/inmemoria/images/fake_img1.png',
        backgroundImage: 'memo1.gif',
        memories: [
            {
                id: 4,
                text: 'Светлый и добрый человек',
                author: 'Ольга Иванова',
                date: '2022-03-01',
            },
        ],
        photos: [
            {
                id: 8,
                image: 'src/features/inmemoria/images/gallery/photo8.jpg',
                title: 'На прогулке',
            },
            {
                id: 9,
                image: 'src/features/inmemoria/images/gallery/photo9.jpg',
                title: 'В парке',
            },
        ],
    },
    {
        id: 5,
        firstName: 'Андрей',
        lastName: 'Кузнецов',
        maidenName: '',
        patronymic: '',
        nickname: '',
        birthday: '1978-05-25',
        deathDay: '2018-09-30',
        photoImage: 'src/features/inmemoria/images/fake_img2.png',
        backgroundImage: 'memo1.gif',
        memories: [
            {
                id: 5,
                text: 'Талантливый руководитель',
                author: 'Сергей Кузьмин',
                date: '2018-10-15',
            },
        ],
        photos: [
            {
                id: 10,
                image: 'src/features/inmemoria/images/gallery/photo10.jpg',
                title: 'В офисе',
            },
            {
                id: 11,
                image: 'src/features/inmemoria/images/gallery/photo11.jpg',
                title: 'На конференции',
            },
        ],
    },
    {
        id: 6,
        firstName: 'Мария',
        lastName: 'Соколова',
        maidenName: '',
        patronymic: '',
        nickname: '',
        birthday: '1990-06-15',
        deathDay: '2023-01-10',
        photoImage: 'src/features/inmemoria/images/fake_img1.png',
        backgroundImage: 'memo1.gif',
        memories: [
            {
                id: 6,
                text: 'Прекрасный педагог',
                author: 'Наталья Орлова',
                date: '2023-02-01',
            },
        ],
        photos: [
            {
                id: 12,
                image: 'src/features/inmemoria/images/gallery/photo12.jpg',
                title: 'В школе',
            },
            {
                id: 13,
                image: 'src/features/inmemoria/images/gallery/photo13.jpg',
                title: 'С учениками',
            },
        ],
    },
    {
        id: 7,
        firstName: 'Дмитрий',
        lastName: 'Попов',
        maidenName: '',
        patronymic: '',
        nickname: '',
        birthday: '1983-07-05',
        deathDay: '2020-11-11',
        photoImage: 'src/features/inmemoria/images/fake_img2.png',
        backgroundImage: 'memo1.gif',
        memories: [
            {
                id: 7,
                text: 'Отличный специалист',
                author: 'Максим Волков',
                date: '2020-12-01',
            },
        ],
        photos: [
            {
                id: 14,
                image: 'src/features/inmemoria/images/gallery/photo14.jpg',
                title: 'За работой',
            },
            {
                id: 15,
                image: 'src/features/inmemoria/images/gallery/photo15.jpg',
                title: 'На отдыхе',
            },
        ],
    },
    {
        id: 8,
        firstName: 'Анна',
        lastName: 'Лебедева',
        maidenName: '',
        patronymic: '',
        nickname: '',
        birthday: '1987-08-18',
        deathDay: '2021-12-25',
        photoImage: 'src/features/inmemoria/images/fake_img1.png',
        backgroundImage: 'memo1.gif',
        memories: [
            {
                id: 8,
                text: 'Заботливая мама и жена',
                author: 'Елена Соколова',
                date: '2022-01-15',
            },
        ],
        photos: [
            {
                id: 16,
                image: 'src/features/inmemoria/images/gallery/photo16.jpg',
                title: 'С детьми',
            },
            {
                id: 17,
                image: 'src/features/inmemoria/images/gallery/photo17.jpg',
                title: 'На даче',
            },
        ],
    },
    {
        id: 9,
        firstName: 'Сергей',
        lastName: 'Морозов',
        maidenName: '',
        patronymic: '',
        nickname: '',
        birthday: '1976-09-22',
        deathDay: '2019-07-07',
        photoImage: 'src/features/inmemoria/images/fake_img2.png',
        backgroundImage: 'memo1.gif',
        memories: [
            {
                id: 9,
                text: 'Надежный друг и партнер',
                author: 'Виктор Козлов',
                date: '2019-08-01',
            },
        ],
        photos: [
            {
                id: 18,
                image: 'src/features/inmemoria/images/gallery/photo18.jpg',
                title: 'В командировке',
            },
            {
                id: 19,
                image: 'src/features/inmemoria/images/gallery/photo19.jpg',
                title: 'На встрече',
            },
        ],
    },
    {
        id: 10,
        firstName: 'Ольга',
        lastName: 'Волкова',
        maidenName: '',
        patronymic: '',
        nickname: '',
        birthday: '1992-10-30',
        deathDay: '2022-08-18',
        photoImage: 'src/features/inmemoria/images/fake_img1.png',
        backgroundImage: 'memo1.gif',
        memories: [
            {
                id: 10,
                text: 'Творческий и жизнерадостный человек',
                author: 'Анна Морозова',
                date: '2022-09-01',
            },
        ],
        photos: [
            {
                id: 20,
                image: 'src/features/inmemoria/images/gallery/photo20.jpg',
                title: 'В студии',
            },
            {
                id: 21,
                image: 'src/features/inmemoria/images/gallery/photo21.jpg',
                title: 'На выставке',
            },
        ],
    },
    {
        id: 11,
        firstName: 'Алексей',
        lastName: 'Козлов',
        maidenName: '',
        patronymic: '',
        nickname: '',
        birthday: '1981-11-12',
        deathDay: '2020-03-03',
        photoImage: 'src/features/inmemoria/images/fake_img2.png',
        backgroundImage: 'memo1.gif',
        memories: [
            {
                id: 11,
                text: 'Профессионал своего дела',
                author: 'Дмитрий Соловьев',
                date: '2020-04-01',
            },
        ],
        photos: [
            {
                id: 22,
                image: 'src/features/inmemoria/images/gallery/photo22.jpg',
                title: 'В мастерской',
            },
            {
                id: 23,
                image: 'src/features/inmemoria/images/gallery/photo23.jpg',
                title: 'За станком',
            },
        ],
    },
    {
        id: 12,
        firstName: 'Татьяна',
        lastName: 'Новикова',
        maidenName: '',
        patronymic: '',
        nickname: '',
        birthday: '1989-12-24',
        deathDay: '2023-04-14',
        photoImage: 'src/features/inmemoria/images/fake_img1.png',
        backgroundImage: 'memo1.gif',
        memories: [
            {
                id: 12,
                text: 'Отзывчивая коллега',
                author: 'Мария Петрова',
                date: '2023-05-01',
            },
        ],
        photos: [
            {
                id: 24,
                image: 'src/features/inmemoria/images/gallery/photo24.jpg',
                title: 'В коллективе',
            },
            {
                id: 25,
                image: 'src/features/inmemoria/images/gallery/photo25.jpg',
                title: 'На корпоративе',
            },
        ],
    },
    {
        id: 13,
        firstName: 'Владимир',
        lastName: 'Макаров',
        maidenName: '',
        patronymic: '',
        nickname: '',
        birthday: '1977-01-15',
        deathDay: '2018-06-06',
        photoImage: 'src/features/inmemoria/images/fake_img2.png',
        backgroundImage: 'memo1.gif',
        memories: [
            {
                id: 13,
                text: 'Мудрый наставник',
                author: 'Александр Иванов',
                date: '2018-07-01',
            },
        ],
        photos: [
            {
                id: 26,
                image: 'src/features/inmemoria/images/gallery/photo26.jpg',
                title: 'На лекции',
            },
            {
                id: 27,
                image: 'src/features/inmemoria/images/gallery/photo27.jpg',
                title: 'Со студентами',
            },
        ],
    },
    {
        id: 14,
        firstName: 'Наталья',
        lastName: 'Степанова',
        maidenName: '',
        patronymic: '',
        nickname: '',
        birthday: '1984-02-28',
        deathDay: '2021-09-19',
        photoImage: 'src/features/inmemoria/images/fake_img1.png',
        backgroundImage: 'memo2.gif',
        memories: [
            {
                id: 14,
                text: 'Всегда готова помочь',
                author: 'Ирина Смирнова',
                date: '2021-10-01',
            },
        ],
        photos: [
            {
                id: 28,
                image: 'src/features/inmemoria/images/gallery/photo28.jpg',
                title: 'В больнице',
            },
            {
                id: 29,
                image: 'src/features/inmemoria/images/gallery/photo29.jpg',
                title: 'С пациентами',
            },
        ],
    },
    {
        id: 15,
        firstName: 'Константин',
        lastName: 'Егоров',
        maidenName: '',
        patronymic: '',
        nickname: '',
        birthday: '1979-03-17',
        deathDay: '2019-10-10',
        photoImage: 'src/features/inmemoria/images/fake_img2.png',
        backgroundImage: 'memo2.gif',
        memories: [
            {
                id: 15,
                text: 'Замечательный семьянин',
                author: 'Павел Николаев',
                date: '2019-11-01',
            },
        ],
        photos: [
            {
                id: 30,
                image: 'src/features/inmemoria/images/gallery/photo30.jpg',
                title: 'С семьей',
            },
            {
                id: 31,
                image: 'src/features/inmemoria/images/gallery/photo31.jpg',
                title: 'На пикнике',
            },
        ],
    },
    {
        id: 16,
        firstName: 'Екатерина',
        lastName: 'Никитина',
        maidenName: '',
        patronymic: '',
        nickname: '',
        birthday: '1991-04-05',
        deathDay: '2022-12-12',
        photoImage: 'src/features/inmemoria/images/fake_img1.png',
        backgroundImage: 'memo1.gif',
        memories: [
            {
                id: 16,
                text: 'Душа компании',
                author: 'Светлана Андреева',
                date: '2023-01-01',
            },
        ],
        photos: [
            {
                id: 32,
                image: 'src/features/inmemoria/images/gallery/photo32.jpg',
                title: 'На вечеринке',
            },
            {
                id: 33,
                image: 'src/features/inmemoria/images/gallery/photo33.jpg',
                title: 'С друзьями',
            },
        ],
    },
    {
        id: 17,
        firstName: 'Игорь',
        lastName: 'Захаров',
        maidenName: '',
        patronymic: '',
        nickname: '',
        birthday: '1980-05-10',
        deathDay: '2020-02-02',
        photoImage: 'src/features/inmemoria/images/fake_img2.png',
        backgroundImage: 'memo1.gif',
        memories: [
            {
                id: 17,
                text: 'Талантливый инженер',
                author: 'Андрей Михайлов',
                date: '2020-03-01',
            },
        ],
        photos: [
            {
                id: 34,
                image: 'src/features/inmemoria/images/gallery/photo34.jpg',
                title: 'На проекте',
            },
            {
                id: 35,
                image: 'src/features/inmemoria/images/gallery/photo35.jpg',
                title: 'В лаборатории',
            },
        ],
    },
    {
        id: 18,
        firstName: 'Светлана',
        lastName: 'Борисова',
        maidenName: '',
        patronymic: '',
        nickname: '',
        birthday: '1986-06-20',
        deathDay: '2021-08-08',
        photoImage: 'src/features/inmemoria/images/fake_img1.png',
        backgroundImage: 'memo1.gif',
        memories: [
            {
                id: 18,
                text: 'Добрый и отзывчивый человек',
                author: 'Татьяна Федорова',
                date: '2021-09-01',
            },
        ],
        photos: [
            {
                id: 36,
                image: 'src/features/inmemoria/images/gallery/photo36.jpg',
                title: 'В саду',
            },
            {
                id: 37,
                image: 'src/features/inmemoria/images/gallery/photo37.jpg',
                title: 'На природе',
            },
        ],
    },
    {
        id: 19,
        firstName: 'Артём',
        lastName: 'Королёв',
        maidenName: '',
        patronymic: '',
        nickname: '',
        birthday: '1975-07-30',
        deathDay: '2018-04-04',
        photoImage: 'src/features/inmemoria/images/fake_img2.png',
        backgroundImage: 'memo1.gif',
        memories: [
            {
                id: 19,
                text: 'Прирожденный лидер',
                author: 'Евгений Васильев',
                date: '2018-05-01',
            },
        ],
        photos: [
            {
                id: 38,
                image: 'src/features/inmemoria/images/gallery/photo38.jpg',
                title: 'На совещании',
            },
            {
                id: 39,
                image: 'src/features/inmemoria/images/gallery/photo39.jpg',
                title: 'С командой',
            },
        ],
    },
    {
        id: 20,
        firstName: 'Ирина',
        lastName: 'Медведева',
        maidenName: '',
        patronymic: '',
        nickname: '',
        birthday: '1988-08-15',
        deathDay: '2023-05-05',
        photoImage: 'src/features/inmemoria/images/fake_img1.png',
        backgroundImage: 'memo1.gif',
        memories: [
            {
                id: 20,
                text: 'Прекрасный человек и специалист',
                author: 'Марина Кузнецова',
                date: '2023-06-01',
            },
        ],
        photos: [
            {
                id: 40,
                image: 'src/features/inmemoria/images/gallery/photo40.jpg',
                title: 'В клинике',
            },
            {
                id: 41,
                image: 'src/features/inmemoria/images/gallery/photo41.jpg',
                title: 'С коллегами',
            },
        ],
    },
]

export const handlers = [
    http.get('api/v1/person', ({ params }) => {
        if (params['query']) {
            const query = params['query'] as string
            const data = fakePersons.filter(
                (person) =>
                    `${person.firstName} ${person.lastName}`.includes(query) ||
                    person.firstName.includes(query) ||
                    person.lastName.includes(query)
            )
            return HttpResponse.json({
                totalCount: data.length,
                take: data.length,
                skip: 0,
                items: data,
            } as PagedResultModel<PersonListItemDto>)
        }
        if (!!params['skip'] && !!params['take']) {
            const skip = Number.parseInt(<string>params['skip'])
            const take = Number.parseInt(<string>params['take'])
            const data = fakePersons.slice(skip, skip + take)
            return HttpResponse.json({
                totalCount: fakePersons.length,
                skip: skip,
                take: take,
                items: data,
            } as PagedResultModel<PersonListItemDto>)
        }
        return HttpResponse.json({
            totalCount: fakePersons.length,
            skip: 0,
            take: fakePersons.length,
            items: fakePersons,
        } as PagedResultModel<PersonListItemDto>)
    }),
    http.get('api/v1/person/letters', () => {
        const letters = [
            {
                letter: 'A',
                persons: fakePersons,
            },
            {
                letter: 'B',
                persons: fakePersons,
            },
            {
                letter: 'C',
                persons: fakePersons,
            },
            {
                letter: 'D',
                persons: fakePersons,
            },
        ]
        return HttpResponse.json(letters)
    }),
    http.get('api/v1/person/:id', ({ params }) => {
        const id = Number.parseInt(<string>params['id'])
        const person = fakePersons.find((person) => person.id === id)
        return HttpResponse.json(person)
    }),
    http.get('api/v1/person/:id/memories', ({ params }) => {
        const id = Number.parseInt(<string>params['id'])
        const person = fakePersons.find((person) => person.id === id)
        return HttpResponse.json(person?.memories ?? [])
    }),
    http.get('api/v1/person/:id/photos', ({ params }) => {
        const id = Number.parseInt(<string>params['id'])
        const person = fakePersons.find((person) => person.id === id)
        return HttpResponse.json(person?.photos ?? [])
    }),
]
