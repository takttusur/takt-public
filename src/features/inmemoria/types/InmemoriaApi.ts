import { Person } from './Person.ts'
import { GalleryPhoto } from './GalleryPhoto.ts'
import { MemoriesRecord } from './MemoriesRecord.ts'
import { CarouselPerson } from './CarouselPerson.ts'
import { HikeRecord } from './HikeRecord.ts'

export interface InmemoriaApi {
    getPersons(): Promise<CarouselPerson[]>
    getPerson(id: number): Promise<Person>
    getGalleryPhotos(id: number): Promise<GalleryPhoto[]>
    getMemories(id: number): Promise<MemoriesRecord[]>
    getBio(id: number): Promise<string>
    getHikes(id: number): Promise<HikeRecord[]>
}
