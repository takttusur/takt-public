import { Box, Center, VStack, Image, Link, Text } from '@chakra-ui/react'
import { Link as RouterLink } from 'react-router-dom'
import taktLogo from '../assets/takt.svg'

export default function HomePage(): JSX.Element {
    return (
        <Box
            height="100vh"
            display="flex"
            alignItems="center"
            justifyContent="center"
        >
            <VStack spacing={6}>
                <Center>
                    <Image src={taktLogo} alt="TAKT Logo" boxSize="200px" />
                </Center>
                <Link as={RouterLink} to="/inmemoria">
                    <Text fontSize="2xl" fontWeight="bold">
                        InMemoria
                    </Text>
                </Link>
            </VStack>
        </Box>
    )
}
