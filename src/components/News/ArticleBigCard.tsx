import {Button, Card, CardBody, CardFooter, Image, Stack, Text} from '@chakra-ui/react';


export default function ArticleBigCard() : JSX.Element {
    
    return <Card w="lg">
        <CardBody>
            <Image
                src='https://images.unsplash.com/photo-1555041469-a586c61ea9bc?ixlib=rb-4.0.3&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=1770&q=80'
                alt='Green double couch with wooden legs'
                borderRadius='lg'
            />
            <Stack mt='6' spacing='3'>
                <Text>
                    This sofa is perfect for modern tropical spaces, baroque inspired
                    spaces, earthy toned spaces and for people who love a chic design with a
                    sprinkle of vintage design.
                </Text>
            </Stack>
        </CardBody>
        <CardFooter justify="end">
            <Button variant='solid' colorScheme='blue'>
                Подробнее
            </Button>
        </CardFooter>
    </Card>
}