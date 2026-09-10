import React from 'react'
import './epigraph.css'
import { useGetEpigraphQuery } from '../data/inmemoriaApi.ts'
import Skeleton, {
    SKELETON_GRAY,
    SKELETON_RED,
} from '../../../components/Common/Skeleton.tsx'

const Epigraph: React.FC = () => {
    const epigraphQuery = useGetEpigraphQuery()
    const epigraph =
        !epigraphQuery.isLoading && !!epigraphQuery.data
            ? epigraphQuery.data
            : undefined
    const rand = new Date().getMilliseconds()

    return (
        <div className="inmemoria-epigraph">
            {epigraphQuery.isError && (
                <Skeleton linesCount={4} width={160} color={SKELETON_RED} />
            )}

            {!epigraph && epigraphQuery.isLoading && (
                <Skeleton linesCount={4} width={160} color={SKELETON_GRAY} />
            )}

            {!!epigraph && epigraph.length > 0 && (
                <div
                    dangerouslySetInnerHTML={{
                        __html: epigraph[rand % epigraph.length].text,
                    }}
                ></div>
            )}
        </div>
    )
}

export default Epigraph
