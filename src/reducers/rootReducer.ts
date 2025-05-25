import InmemoriaReducer from '../features/inmemoria/store/InmemoriaSlice.ts'
import { combineReducers } from '@reduxjs/toolkit'

// Define the root reducer object with all reducers
export const rootReducer = {
    inmemoria: InmemoriaReducer,
}

// Create a combined reducer for use with createStore if needed
export const combinedReducer = combineReducers(rootReducer)
