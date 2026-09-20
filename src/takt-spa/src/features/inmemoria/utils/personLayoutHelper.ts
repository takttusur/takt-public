import { PersonCardDesign } from '../types/PersonCardDesign.ts'
import { fallbackLayout, personLayouts } from '../data/personLayouts.ts'

export default function detectLayout(layoutName: string): PersonCardDesign {
    return Object.prototype.hasOwnProperty.call(personLayouts, layoutName)
        ? personLayouts[layoutName]
        : fallbackLayout
}
