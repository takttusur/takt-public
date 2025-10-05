import { configureStore } from '@reduxjs/toolkit'
import { rootReducer } from '../reducers/rootReducer'
import { inmemoriaApi } from '../features/inmemoria/data/inmemoriaApi.ts'

// Configure the store with the root reducer
export const store = configureStore({
    reducer: rootReducer,
    middleware: (getDefault) => getDefault().concat(inmemoriaApi.middleware),
})

// Infer the `RootState` and `AppDispatch` types from the store itself
export type RootState = ReturnType<typeof store.getState>
export type AppDispatch = typeof store.dispatch
