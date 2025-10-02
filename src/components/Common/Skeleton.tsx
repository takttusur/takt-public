import React, { JSX } from 'react'
import './Skeleton.css'

export const SKELETON_GRAY = 'gray'
export const SKELETON_RED = 'red'

export interface ISkeletonProps {
    linesCount: number
    width: number
    color?: string
}

export default function Skeleton(
    props: React.HTMLProps<HTMLDivElement> & ISkeletonProps
): JSX.Element {
    const lines = Array.from({ length: props.linesCount - 1 })
    const color = props.color ?? SKELETON_GRAY

    return (
        <div
            className="skeleton-container"
            {...props}
            style={{ ...props.style }}
        >
            {lines.map((_, i) => (
                <p
                    key={i}
                    className="skeleton-container__line"
                    style={{
                        width: `${props.width}px`,
                        backgroundColor: color,
                    }}
                ></p>
            ))}
            <p
                className="skeleton-container__line"
                style={{
                    width: `${props.width * 0.8}px`,
                    backgroundColor: color,
                }}
            ></p>
        </div>
    )
}
