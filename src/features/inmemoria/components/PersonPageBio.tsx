/* eslint-disable max-len */
import React from 'react'
import './personPageBio.css'

interface PersonPageBioProps {}

export const PersonPageBio: React.FC<PersonPageBioProps> = () => {
    // eslint-disable-next-line max-len
    const bio = `
<p>
    Алексей начал увлекаться горами ещё в школе, впервые оказавшись в походе по Восточному Саяну в 13 лет. С тех пор альпинизм стал смыслом его жизни. Он прошёл курсы спасателей, участвовал в десятках поисково-спасательных операций, а также организовал несколько экспедиций на Алтай, Памир и Кавказ.
    </p>
    <p>
В 2014 году поднялся на пик Ленина (7134 м), в 2017 — на пик Корженевской (7105 м), а в 2019 — на Ама-Даблам в Непале. Алексей — сторонник чистого альпинизма и минимального вмешательства в природу. Он активно продвигает принципы Leave No Trace и проводит образовательные семинары по технике безопасности в горах.
</p>
    `
    return (
        <div
            className="inmemoria-person-page-bio"
            dangerouslySetInnerHTML={{ __html: bio }}
        />
    )
}
