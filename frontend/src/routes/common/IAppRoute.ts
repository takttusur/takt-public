import React from 'react'

export interface IAppRoute {
    readonly path: string
    readonly element: React.ReactNode
    readonly title: string
}
