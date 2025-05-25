import { CurrentPersonModel } from './CurrentPersonModel.ts'
import { createSlice, PayloadAction } from '@reduxjs/toolkit'

export interface InmemoriaState {
    current?: CurrentPersonModel
}

const defaultState: InmemoriaState = {
    current: undefined,
}

export const inmemoriaSlice = createSlice({
    name: 'inmemoria',
    initialState: defaultState,
    reducers: {
        setCurrentPerson: (
            state,
            action: PayloadAction<CurrentPersonModel>
        ) => {
            state.current = action.payload
        },
    },
})

export const { setCurrentPerson } = inmemoriaSlice.actions

// Type for the slice reducer
export type InmemoriaReducer = typeof inmemoriaSlice.reducer

export default inmemoriaSlice.reducer
