const htmlparser2 = require('htmlparser2');

/**
 * Converts an HTML string to Falco F# DSL.
 * @param {string} html - The HTML markup to convert.
 * @returns {string} - The generated F# DSL code.
 */
function htmlToFalco(html) {
    const dom = htmlparser2.parseDocument(html);

    const indentSize = 4;

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
        // List of boolean attributes per HTML spec and Falco.Markup
        const booleanAttrs = new Set([
            'allowfullscreen', 'async', 'autofocus', 'autoplay', 'checked', 'controls', 'default',
            'defer', 'disabled', 'formnovalidate', 'hidden', 'ismap', 'itemscope', 'loop',
            'multiple', 'muted', 'nomodule', 'novalidate', 'open', 'readonly', 'required',
            'reversed', 'selected'
        ]);
        for (const [key, value] of Object.entries(attributes)) {
            if (booleanAttrs.has(key)) {
                props.push(`_${key}_`);
            } else if (/^on[a-z]+/.test(key)) {
                // Explicitly handle event attributes (e.g., onclick, onchange)
                props.push(`_${key}_ "${escapeString(value)}"`);
            } else {
                props.push(`_${key}_ "${escapeString(value)}"`);
            }
        }
        return props;
    }

    /**
     * Processes a DOM node and returns its F# representation.
     * @param {object} node
     * @param {number} indentLevel
     * @returns {string}
     */
    function processNode(node, indentLevel) {
        if (node.type === 'text') {
            const text = node.data.trim();
            if (text === '') return '';
            return `_text "${escapeString(text)}"`;
        } else if (node.type === 'tag') {            
            let element = `_${node.name}`;

            const attributes = mapAttributes(node.attribs || {});

            const children = node.children.map(child => processNode(child, indentLevel + 1)).filter(child => child !== '').map(child => {                            
                indentLevel++;
                const indentedChild = ' '.repeat((indentLevel + 1) * indentSize) + child;
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
                element += ' '.repeat(indentLevel * indentSize) + `\n${children.join('\n')} ]`;
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

const sourceElement = document.getElementById('source');
if (!sourceElement) {
    return;
}

sourceElement.addEventListener('change', evt => {
    const targetElement = document.getElementById('target');
    if (!targetElement) {
        return;
    }
    targetElement.innerHTML = htmlToFalco(evt.target.value);
});
