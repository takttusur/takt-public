import { LinkBox, LinkOverlay, Text, Box, useColorModeValue, Center } from '@chakra-ui/react';

export default function EquipmentWidget() {
  return (
    <Box borderWidth='1px' borderColor='black' bg={useColorModeValue('gray.200', 'gray.700')} h={400}>
        <Center>
            <Box w='90%' h={398} bg='gray.400'>
                <Box position='relative' w='100%' h={398} bg='gray'>
                  <Text position='absolute' top='50%' w='100%' textAlign='center'> Equipment </Text>
                  <LinkBox 
                    position='absolute' bottom='0%' right='0%' w='fit-content'
                    textDecorationLine='underline'      
                    fontWeight={500}              
                    pb={2}
                    pr={2}                  
                  >
                    <LinkOverlay href = '/Equipment'>
                      <Text> Подробнее </Text>
                    </LinkOverlay>                  
                  </LinkBox>                   
                </Box>         
            </Box>
        </Center>            
    </Box>
  );
}