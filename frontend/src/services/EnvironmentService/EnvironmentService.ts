import { IEnvironmentService } from './IEnvironmentService.ts'

const env = import.meta.env
export default class EnvironmentService implements IEnvironmentService {
    public readonly YandexMetrikaEnabled: boolean =
        (env.VITE_YANDEX_METRIKA_ENABLED as string)?.toLowerCase() === 'true'

    public readonly YandexMetrikaId: string =
        (env.VITE_YANDEX_METRIKA_ID as string) ?? ''

    public readonly LogrocketId: string =
        (env.VITE_LOGROCKET_ID as string) ?? ''

    public readonly LogrocketEnabled: boolean =
        (env.VITE_LOGROCKET_ENABLED as string)?.toLowerCase() === 'true'

    public constructor() {}
}
