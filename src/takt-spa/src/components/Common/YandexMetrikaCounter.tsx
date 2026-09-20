import ym, { YMInitializer } from 'react-yandex-metrika'
import { FC, useCallback, useEffect } from 'react'
import { useLocation } from 'react-router-dom'

export interface IYandexMetrikaProps {
    isEnabled: boolean
    id?: string
}

const YandexMetrikaCounter: FC<IYandexMetrikaProps> = (props) => {
    const location = useLocation()
    const hit = useCallback(
        (url: string) => {
            if (props.isEnabled) {
                ym('hit', url)
            } else {
                console.log(`%c[YM](HIT)`, `color: orange`, url)
            }
        },
        [props.isEnabled]
    )

    useEffect(() => {
        hit(location.pathname)
    }, [location, hit])

    if (!props.isEnabled) return <div className="no-yandex-metrika"></div>
    return (
        <YMInitializer
            accounts={[Number(props.id)]}
            options={{ defer: true, webvisor: true, trackHash: true }}
            version="2"
        />
    )
}
export default YandexMetrikaCounter
