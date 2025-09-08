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
     * Maps HTML attributes to Falco attributes.
     * @param {object} attributes
     * @returns {string[]}
     */
    function mapAttributes(attributes) {
        const props = [];
        for (const [key, value] of Object.entries(attributes)) {
            switch (key) {                
                default:
                    props.push(`_${key}_ "${escapeString(value)}"`);
                    break;
            }
        }
        return props;
    }

    /**
     * Processes a DOM node and returns its F# representation.
     * @param {object} node
     * @param {number} nestedIndentLevel
     * @returns {string}
     */
    function processNode(node, nestedIndentLevel) {
        if (node.type === 'text') {
            const text = node.data.trim();
            if (text === '') return '';
            return `_text "${escapeString(text)}"`;
        } else if (node.type === 'tag') {            
            let element = `_${node.name}`;

            const attributes = mapAttributes(node.attribs || {});

            const children = node.children.map(child => processNode(child, nestedIndentLevel + 1)).filter(child => child !== '').map(child => {                            
                indentLevel++;
                const indentedChild = ' '.repeat(nestedIndentLevel * indentLevel * indentSize) + child;
                indentLevel--;
                return indentedChild;
            });

            if (attributes.length > 0) {
                element += ` [ ${attributes.join('; ')} ]`;
            } else {
                element += ' []';
            }

            if (children.length > 0) {
                indentLevel++;         
                element += ' [';                    
                element += ' '.repeat(nestedIndentLevel * indentLevel * indentSize) + `\n${children.join('\n')} ]`;  
                indentLevel--;
            } else {
                element += ' []';
            }

            return element;
        }
        
        return '';
    }

    /**
     * Processes the entire DOM and returns F# code.
     * @param {object} dom
     * @returns {string}
     */
    function processDOM(dom) {
        return dom.children.map(child => processNode(child, 1)).filter(node => node !== '').join('\n');
    }

    return processDOM(dom);
}

document.getElementById('source').addEventListener('change', evt => {
    document.getElementById('target').innerHTML = htmlToFalco(evt.target.value);
});
