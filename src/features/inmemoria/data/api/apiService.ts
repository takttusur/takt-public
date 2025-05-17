import { Person } from '../../types/Person'
import { CarouselPerson } from '../../types/CarouselPerson'
import { GalleryPhoto } from '../../types/GalleryPhoto'
import { MemoriesRecord } from '../../types/MemoriesRecord'

import { personData, persons } from './people'
import { galleryPhotos } from './gallery'
import { memories } from './memories'
import { bio } from './bio'

/**
 * Fake API service for inmemoria feature
 * This service provides functions to get data that would normally be fetched from a real API
 */
export const apiService = {
    /**
     * Get person data
     * @returns Person data
     */
    getPerson: (): Promise<Person> => {
        return Promise.resolve(personData)
    },

    /**
     * Get list of persons for carousel
     * @returns List of persons
     */
    getPersons: (): Promise<CarouselPerson[]> => {
        return Promise.resolve(persons)
    },

    /**
     * Get gallery photos
     * @returns List of gallery photos
     */
    getGalleryPhotos: (): Promise<GalleryPhoto[]> => {
        return Promise.resolve(galleryPhotos)
    },

    /**
     * Get memories
     * @returns List of memories
     */
    getMemories: (): Promise<MemoriesRecord[]> => {
        return Promise.resolve(memories)
    },

    /**
     * Get bio
     * @returns Bio HTML string
     */
    getBio: (): Promise<string> => {
        return Promise.resolve(bio)
    },
}
