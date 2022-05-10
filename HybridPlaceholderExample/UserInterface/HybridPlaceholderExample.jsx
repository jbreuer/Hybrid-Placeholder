import React from 'react';
import { Text, RichText } from '@sitecore-jss/sitecore-jss-react';

const HybridPlaceholderExample = ({ fields }) => {
    let {
        heading,
        date,
        text,
        isLoaded,
    } = fields;
    if (isLoaded === null || isLoaded === undefined) {
        isLoaded = true;
    }
    return (
        <div>
            <p><Text field={heading} /></p>
            {isLoaded && (
                <p>{date}</p>
            )}
            {!isLoaded && (
                <p>Loading...</p>
            )}
            <RichText field={text} />
        </div>
    )};

export default HybridPlaceholderExample;
