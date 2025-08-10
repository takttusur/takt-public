import InmemoriaReducer from '../features/inmemoria/store/InmemoriaSlice.ts'
import { combineReducers } from '@reduxjs/toolkit'
import { inmemoriaApi } from '../features/inmemoria/data/inmemoriaApi.ts'

// Define the root reducer object with all reducers
export const rootReducer = {
    inmemoria: InmemoriaReducer,
    [inmemoriaApi.reducerPath]: inmemoriaApi.reducer,
}

// Create a combined reducer for use with createStore if needed
export const combinedReducer = combineReducers(rootReducer)
