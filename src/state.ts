import { InmemoriaState } from './features/inmemoria/store/InmemoriaSlice.ts'

// This interface is deprecated, use RootState from './store' instead
export interface AppState {
    inmemoria: InmemoriaState
}
