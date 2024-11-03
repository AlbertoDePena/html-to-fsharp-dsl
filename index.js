const htmlparser2 = require('htmlparser2');

/**
 * Converts an HTML string to Falco F# DSL.
 * @param {string} html - The HTML markup to convert.
 * @returns {string} - The generated F# DSL code.
 */
function htmlToFalco(html) {
    const dom = htmlparser2.parseDocument(html);

    const indentSize = 4;
    let indentLevel = 0;

    /**
     * Escapes special characters in strings.
     * @param {string} str
     * @returns {string}
     */
    function escapeString(str) {
        return str
            .replace(/\\/g, '\\\\')
            .replace(/"/g, '\\"')
            .replace(/\n/g, '\\n')
            .replace(/\r/g, '\\r');
    }

    /**
     * Maps HTML attributes to Falco atributes.
     * @param {object} attribs
     * @returns {string[]}
     */
    function mapAttributes(attribs) {
        const props = [];
        for (const [key, value] of Object.entries(attribs)) {
            switch (key) {
                case 'class':
                    props.push(`Attr.class' "${escapeString(value)}"`);
                    break;
                case 'id':
                    props.push(`Attr.id "${escapeString(value)}"`);
                    break;
                case 'style':
                    // Simple style parser: assumes styles are in "key: value;" format
                    const styles = value.split(';').map(s => s.trim()).filter(s => s);
                    const styleProps = styles.map(s => {
                        const [k, v] = s.split(':').map(part => part.trim());
                        // Convert CSS property to camelCase if necessary
                        const camelCaseKey = k.replace(/-([a-z])/g, g => g[1].toUpperCase());
                        return `style.${camelCaseKey} "${escapeString(v)}"`
                    });
                    if (styleProps.length > 0) {
                        props.push(`Attr.style [ ${styleProps.join('; ')} ]`);
                    }
                    break;
                // Add more attribute mappings as needed
                default:
                    // Handle data-* attributes or others
                    if (key.startsWith('data-')) {
                        const dataKey = key.slice(5);
                        props.push(`Attr.create "data-${dataKey}" "${escapeString(value)}"`);
                    } else {
                        // Generic attribute
                        props.push(`Attr.create "${key}" "${escapeString(value)}"`);
                    }
                    break;
            }
        }
        return props;
    }

    /**
     * Processes a DOM node and returns its F# representation.
     * @param {object} node
     * @returns {string}
     */
    function processNode(node) {
        if (node.type === 'text') {
            const text = node.data.trim();
            if (text === '') return '';
            return `Text.raw "${escapeString(text)}"`;
        } else if (node.type === 'tag') {
            const tagName = node.name;
            const falcoTag = `Elem.${tagName}`;

            const attribs = node.attribs || {};
            const props = mapAttributes(attribs);

            const children = node.children.map(child => processNode(child)).filter(child => child !== '').map(child => {
                // Increase indentation for children
                indentLevel++;
                const indentedChild = ' '.repeat(indentLevel * indentSize) + child;
                indentLevel--;
                return indentedChild;
            });

            let elementAlt = falcoTag;

            if (props.length > 0) {
                indentLevel++;
                const propsString = props.map(prop => {
                    const indentedProp = ' '.repeat(indentLevel * indentSize) + prop;
                    return indentedProp;
                }).join('\n');
                elementAlt += ' [';
                elementAlt += `\n${propsString}`;
                elementAlt += '\n]';
                indentLevel--;
            } else {
                elementAlt += ' []';
            }

            if (children.length > 0) {
                if (props.length > 0) {
                    //elementAlt += '\n';
                }
                const childrenString = children.join('\n');
                elementAlt += ' [';
                //elementAlt += childrenString;
                elementAlt += `\n${childrenString}`;
                elementAlt += '\n]';
            } else {
                elementAlt += ' []';
            }

            elementAlt += '\n' + ' '.repeat(indentLevel * indentSize);

            return elementAlt;
        }
        // Handle other node types if necessary
        return '';
    }

    /**
     * Processes the entire DOM and returns F# code.
     * @param {object} dom
     * @returns {string}
     */
    function processDOM(dom) {
        return dom.children.map(child => processNode(child)).filter(node => node !== '').join('\n');
    }

    return processDOM(dom);
}

document.getElementById('source').addEventListener('change', evt => {
    document.getElementById('target').innerHTML = htmlToFalco(evt.target.value);
});
