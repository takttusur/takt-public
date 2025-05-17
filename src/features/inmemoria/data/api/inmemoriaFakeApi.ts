import { Person } from '../../types/Person'
import { CarouselPerson } from '../../types/CarouselPerson'
import { GalleryPhoto } from '../../types/GalleryPhoto'
import { MemoriesRecord } from '../../types/MemoriesRecord'

import { personData, persons } from './people'
import { galleryPhotos } from './gallery'
import { memories } from './memories'
import { bio } from './bio'
import { InmemoriaApi } from '../../types/InmemoriaApi.ts'
import { HikeRecord } from '../../types/HikeRecord.ts'
import { hikesData } from './hikes.ts'

class InmemoriaFakeApi implements InmemoriaApi {
    getPersons(): Promise<CarouselPerson[]> {
        return Promise.resolve(persons)
    }

    getPerson(): Promise<Person> {
        return Promise.resolve(personData)
    }

    getGalleryPhotos(): Promise<GalleryPhoto[]> {
        return Promise.resolve(galleryPhotos)
    }

    getMemories(): Promise<MemoriesRecord[]> {
        return Promise.resolve(memories)
    }

    getBio(): Promise<string> {
        return Promise.resolve(bio)
    }

    getHikes(): Promise<HikeRecord[]> {
        return Promise.resolve(hikesData)
    }
}

/**
 * Fake API service for inmemoria feature
 * This service provides functions to get data that would normally be fetched from a real API
 */
export const inmemoriaFakeApi = new InmemoriaFakeApi()
