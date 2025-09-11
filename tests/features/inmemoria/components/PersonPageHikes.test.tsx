import { describe, it, expect } from 'vitest'
import { render } from '@testing-library/react'
import '@testing-library/jest-dom'
import { PersonPageHikes } from '../../../../src/features/inmemoria/components/PersonPageHikes'

describe('PersonPageHikes', () => {
    it('should render correctly', () => {
        const { container } = render(<PersonPageHikes />)

        // Check that the component renders a div with the correct class
        const hikesDiv = container.querySelector('.inmemoria-person-page-hikes')
        expect(hikesDiv).toBeInTheDocument()
        expect(hikesDiv?.tagName.toLowerCase()).toBe('div')
        expect(hikesDiv?.children.length).toBe(0)
    })
})
