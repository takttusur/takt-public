import { CurrentPersonModel } from './CurrentPersonModel.ts'
import { createSlice } from '@reduxjs/toolkit'

export interface InmemoriaState {
    current?: CurrentPersonModel
}

const defaultState: InmemoriaState = {
    current: undefined,
}

export const inmemoriaSlice = createSlice({
    name: 'inmemoria',
    initialState: defaultState,
    reducers: {},
})

// export const { setCurrentPerson } = inmemoriaSlice.actions

// Type for the slice reducer
export type InmemoriaReducer = typeof inmemoriaSlice.reducer

export default inmemoriaSlice.reducer
