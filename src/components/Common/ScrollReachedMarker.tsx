import { FC, useEffect, useRef } from 'react'

export interface IScrollReachedMarkerProps {
    onReached: () => void
}

const ScrollReachedMarker: FC<IScrollReachedMarkerProps> = (props) => {
    const userOnBottomMarker = useRef(null)

    useEffect(() => {
        const current = userOnBottomMarker.current
        const observer = new IntersectionObserver(
            (entries) => {
                if (entries[0].isIntersecting) {
                    props.onReached()
                }
            },
            { threshold: 1 }
        )

        if (current) {
            observer.observe(current)
        }

        return () => {
            if (current) {
                observer.unobserve(current)
            }
        }
    }, [userOnBottomMarker, props])

    return <div ref={userOnBottomMarker}></div>
}

export default ScrollReachedMarker
