// Preloaded Documentation Topics & Code Examples
const lessons = {
    welcome: {
        badge: "Topic 01",
        title: "Welcome to Kaedehara",
        text: "<p>Kaedehara is a modern, custom strongly-typed programming language compiled directly down to executable MSIL (Microsoft Intermediate Language) bytecode using a custom <code>System.Reflection.Emit</code> pipeline.</p><p>The entry point of any Kaedehara script is its global block statement list. Let's start with a classic hello world script!</p><div class='feature-note'><p><strong>Key Feature:</strong> Kaedehara runs top-level scripts seamlessly. You don't need a boilerplate <code>static void Main</code> wrapper. The code is compiled into a dynamic method and executed immediately.</p></div>",
        code: `print("Hello, Kaedehara!");`
    },
    variables: {
        badge: "Topic 02",
        title: "Variables & Reassignment",
        text: "<p>Kaedehara supports two modes of variable declaration:</p><ul><li><code>let</code>: Defines read-only/immutable variables. Once initialized, they cannot be reassigned.</li><li><code>var</code>: Defines mutable variables that can be reassigned freely.</li></ul><p>Type annotations are optional (e.g. <code>: int</code>). If omitted, the compiler binds variables using structural type inference from the initializer expression.</p>",
        code: `let x: int = 10;
var y = 20;
y = x + y;
print("y = " + y);`
    },
    types: {
        badge: "Topic 03",
        title: "Primitive Data Types",
        text: "<p>Kaedehara supports five main primitive types:</p><ul><li><code>int</code>: Integer numbers (e.g., <code>42</code>)</li><li><code>float</code>: Floating-point decimal numbers (e.g., <code>3.14</code>, backed by double precision)</li><li><code>char</code>: Single characters wrapped in single quotes (e.g., <code>'A'</code>)</li><li><code>bool</code>: Boolean values (<code>true</code> or <code>false</code>)</li><li><code>string</code>: UTF-8 character sequences (e.g., <code>\"hello\"</code>)</li></ul>",
        code: `let score: float = 85.5;
let grade: char = 'A';
let passed: bool = score >= 50.0;
print("Grade: " + grade);
print("Passed: " + passed);`
    },
    arrays: {
        badge: "Topic 04",
        title: "Arrays & Indexing",
        text: "<p>Kaedehara supports strongly-typed arrays declared with brackets, for example: <code>let arr: int[] = [10, 20, 30];</code>.</p><p>Arrays support element-level lookup and mutation using standard subscript syntax: <code>arr[0] = 99;</code>. During initialization, the compiler infers the best common element type and performs implicit promotion if elements are compatible (e.g., ints to floats).</p>",
        code: `let arr: int[] = [10, 20, 30];
arr[0] = arr[1] + arr[2];
print(arr[0]);`
    },
    controlflow: {
        badge: "Topic 05",
        title: "Control Flow Structures",
        text: "<p>Kaedehara features standard structures for branch evaluations and loop iterations:</p><ul><li><code>if</code> / <code>else</code>: Standard conditional branches.</li><li><code>while</code>: Repeatedly executes a block while a condition evaluates to true.</li></ul>",
        code: `var count = 1;
while (count <= 3) {
    print("Count is: " + count);
    count = count + 1;
}
if (count > 3) {
    print("Done!");
}`
    },
    functions: {
        badge: "Topic 06",
        title: "Functions & Recursion",
        text: "<p>User-defined functions are declared using the <code>fn</code> keyword.</p><p>Parameters must have explicit type clauses, and functions must declare their return type (or default to <code>void</code>). Functions can call themselves recursively, enabling powerful mathematical computations.</p>",
        code: `fn fibonacci(n: int): int {
    if (n <= 1) {
        return n;
    }
    return fibonacci(n - 1) + fibonacci(n - 2);
}
print(fibonacci(6));`
    },
    playground: {
        badge: "Playground",
        title: "Kaedehara Code Playground",
        text: "<p>Welcome to the full Code Playground! Feel free to modify, write, and execute any custom Kaedehara script. Try combining arrays, loops, and custom recursive functions to see what you can build!</p>",
        code: `// Write your custom Kaedehara script here!
fn square(x: int): int {
    return x * x;
}

let list: int[] = [1, 2, 3, 4, 5];
var i = 0;
while (i < 5) {
    print("Square of " + list[i] + " = " + square(list[i]));
    i = i + 1;
}`
    }
};

let currentTopic = 'welcome';

// DOM Elements
const lessonBadge = document.getElementById('lesson-badge');
const lessonTitle = document.getElementById('lesson-title');
const lessonText = document.getElementById('lesson-text');
const lessonCodeDisplay = document.getElementById('lesson-code-display');
const codeEditor = document.getElementById('code-editor');
const lineNumbers = document.getElementById('line-numbers');
const consoleBody = document.getElementById('console-body');
const highlightLayer = document.getElementById('highlight-layer');
const highlightCode = document.getElementById('highlight-code');

// Syntax Highlighting Engine
function highlightSyntax(code) {
    let result = "";
    let i = 0;
    while (i < code.length) {
        let char = code[i];

        // Whitespaces
        if (/\s/.test(char)) {
            result += char;
            i++;
            continue;
        }

        // Comments
        if (char === '/' && code[i+1] === '/') {
            let val = "";
            while (i < code.length && code[i] !== '\n') {
                val += code[i];
                i++;
            }
            result += `<span class="hl-comment">${escapeHtml(val)}</span>`;
            continue;
        }

        // Strings
        if (char === '"') {
            let val = '"';
            i++;
            while (i < code.length && code[i] !== '"') {
                val += code[i];
                i++;
            }
            if (i < code.length) {
                val += '"';
                i++;
            }
            result += `<span class="hl-string">${escapeHtml(val)}</span>`;
            continue;
        }

        // Characters
        if (char === "'") {
            let val = "'";
            i++;
            while (i < code.length && code[i] !== "'") {
                val += code[i];
                i++;
            }
            if (i < code.length) {
                val += "'";
                i++;
            }
            result += `<span class="hl-char">${escapeHtml(val)}</span>`;
            continue;
        }

        // Numbers
        if (/[0-9]/.test(char)) {
            let val = "";
            while (i < code.length && /[0-9.]/.test(code[i])) {
                val += code[i];
                i++;
            }
            result += `<span class="hl-number">${val}</span>`;
            continue;
        }

        // Identifiers & Keywords
        if (/[a-zA-Z_]/.test(char)) {
            let val = "";
            while (i < code.length && /[a-zA-Z0-9_]/.test(code[i])) {
                val += code[i];
                i++;
            }
            
            const keywords = ['let', 'var', 'fn', 'return', 'if', 'else', 'while', 'for', 'to', 'print'];
            const types = ['int', 'float', 'char', 'bool', 'string', 'void'];
            
            if (keywords.includes(val)) {
                result += `<span class="hl-keyword">${val}</span>`;
            } else if (types.includes(val)) {
                result += `<span class="hl-type">${val}</span>`;
            } else if (val === 'true' || val === 'false') {
                result += `<span class="hl-boolean">${val}</span>`;
            } else {
                result += `<span class="hl-id">${val}</span>`;
            }
            continue;
        }

        // Symbols and Operators
        result += escapeHtml(char);
        i++;
    }
    return result;
}

function escapeHtml(text) {
    return text
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;");
}

function updateHighlighting() {
    highlightCode.innerHTML = highlightSyntax(codeEditor.value);
}

function onEditorInput() {
    updateLineNumbers();
    updateHighlighting();
}

function onEditorScroll() {
    highlightLayer.scrollTop = codeEditor.scrollTop;
    highlightLayer.scrollLeft = codeEditor.scrollLeft;
}

// Navigation and Sidebar Logic
function selectTopic(topicId) {
    currentTopic = topicId;
    
    // Update active state in sidebar
    document.querySelectorAll('.nav-item').forEach(item => {
        item.classList.remove('active');
    });
    const selectedBtn = document.getElementById(`nav-${topicId}`);
    if (selectedBtn) {
        selectedBtn.classList.add('active');
    }

    // Update lesson panel content
    const lesson = lessons[topicId];
    lessonBadge.innerText = lesson.badge;
    lessonTitle.innerText = lesson.title;
    lessonText.innerHTML = lesson.text;
    lessonCodeDisplay.innerText = lesson.code;
    
    // In Playground mode, hide the example card
    const exampleCard = document.getElementById('lesson-example-card');
    if (topicId === 'playground') {
        exampleCard.style.display = 'none';
    } else {
        exampleCard.style.display = 'block';
    }

    // Set editor text
    codeEditor.value = lesson.code;
    updateLineNumbers();
    updateHighlighting();
    codeEditor.scrollTop = 0;
    codeEditor.scrollLeft = 0;
    onEditorScroll();
}

function loadExampleIntoPlayground() {
    // Scroll editor into view (on mobile) and load code
    codeEditor.value = lessons[currentTopic].code;
    updateLineNumbers();
    updateHighlighting();
    codeEditor.scrollTop = 0;
    codeEditor.scrollLeft = 0;
    onEditorScroll();
    
    // Visual flash animation on editor to show it was loaded
    codeEditor.style.backgroundColor = 'rgba(234, 88, 12, 0.08)';
    setTimeout(() => {
        codeEditor.style.backgroundColor = 'transparent';
    }, 300);

    logToConsole(`Loaded example for "${lessons[currentTopic].title}" into editor.`, 'system');
}

// Line Numbers Helper
function updateLineNumbers() {
    const lines = codeEditor.value.split('\n');
    let numbersHtml = '';
    for (let i = 1; i <= Math.max(lines.length, 1); i++) {
        numbersHtml += `<div>${i}</div>`;
    }
    lineNumbers.innerHTML = numbersHtml;
}

// Console helpers
function logToConsole(text, type = 'output') {
    const line = document.createElement('div');
    line.className = `console-line ${type}`;
    if (type === 'system') {
        line.innerText = `[System] ${text}`;
    } else if (type === 'error') {
        line.innerText = `[Error] ${text}`;
    } else {
        line.innerText = text;
    }
    consoleBody.appendChild(line);
    consoleBody.scrollTop = consoleBody.scrollHeight;
}

function clearConsole() {
    consoleBody.innerHTML = '';
}

function resetPlayground() {
    codeEditor.value = lessons[currentTopic].code;
    updateLineNumbers();
    updateHighlighting();
    codeEditor.scrollTop = 0;
    codeEditor.scrollLeft = 0;
    onEditorScroll();
    clearConsole();
    logToConsole('Playground reset to default template.', 'system');
}

// Custom Interpreter / Evaluator Implementation
function tokenize(input) {
    const tokens = [];
    let i = 0;
    while (i < input.length) {
        let char = input[i];
        
        if (/\s/.test(char)) {
            i++;
            continue;
        }
        
        if (char === '/' && input[i+1] === '/') {
            while (i < input.length && input[i] !== '\n') {
                i++;
            }
            continue;
        }
        
        if (char === '=' && input[i+1] === '=') {
            tokens.push({ type: 'OP', value: '==' });
            i += 2;
            continue;
        }
        if (char === '!' && input[i+1] === '=') {
            tokens.push({ type: 'OP', value: '!=' });
            i += 2;
            continue;
        }
        if (char === '<' && input[i+1] === '=') {
            tokens.push({ type: 'OP', value: '<=' });
            i += 2;
            continue;
        }
        if (char === '>' && input[i+1] === '=') {
            tokens.push({ type: 'OP', value: '>=' });
            i += 2;
            continue;
        }
        if (char === '&' && input[i+1] === '&') {
            tokens.push({ type: 'OP', value: '&&' });
            i += 2;
            continue;
        }
        if (char === '|' && input[i+1] === '|') {
            tokens.push({ type: 'OP', value: '||' });
            i += 2;
            continue;
        }

        if ('+-*/%=()[]{};,!:<>'.includes(char)) {
            tokens.push({ type: 'SYM', value: char });
            i++;
            continue;
        }

        if (char === '"') {
            let val = "";
            i++;
            while (i < input.length && input[i] !== '"') {
                val += input[i];
                i++;
            }
            i++;
            tokens.push({ type: 'STR', value: val });
            continue;
        }

        if (char === "'") {
            let val = input[i+1];
            i += 3;
            tokens.push({ type: 'CHAR', value: val });
            continue;
        }

        if (/[0-9]/.test(char)) {
            let val = "";
            while (i < input.length && /[0-9.]/.test(input[i])) {
                val += input[i];
                i++;
            }
            tokens.push({ type: 'NUM', value: parseFloat(val) });
            continue;
        }

        if (/[a-zA-Z_]/.test(char)) {
            let val = "";
            while (i < input.length && /[a-zA-Z0-9_]/.test(input[i])) {
                val += input[i];
                i++;
            }
            if (['let', 'var', 'fn', 'return', 'if', 'else', 'while', 'for', 'to', 'true', 'false', 'int', 'float', 'char', 'bool', 'string', 'void', 'print'].includes(val)) {
                tokens.push({ type: 'KEY', value: val });
            } else {
                tokens.push({ type: 'ID', value: val });
            }
            continue;
        }

        throw new Error(`Lexer Error: Unknown character '${char}'`);
    }
    return tokens;
}

class Parser {
    constructor(tokens) {
        this.tokens = tokens;
        this.pos = 0;
    }
    peek() { return this.tokens[this.pos] || null; }
    next() { return this.tokens[this.pos++]; }
    match(type, val) {
        let t = this.peek();
        if (t && t.type === type && (val === undefined || t.value === val)) {
            return this.next();
        }
        return null;
    }
    expect(type, val) {
        let t = this.match(type, val);
        if (!t) {
            let actual = this.peek() ? `${this.peek().type} (${this.peek().value})` : "EOF";
            throw new Error(`Parser Error: Expected ${type} ${val || ''}, got ${actual}`);
        }
        return t;
    }
    
    parseProgram() {
        const statements = [];
        while (this.peek()) {
            statements.push(this.parseStatement());
        }
        return { type: 'Program', body: statements };
    }

    parseStatement() {
        let t = this.peek();
        if (!t) return null;

        if (t.type === 'KEY') {
            if (t.value === 'let' || t.value === 'var') {
                return this.parseVariableDeclaration();
            }
            if (t.value === 'fn') {
                return this.parseFunctionDeclaration();
            }
            if (t.value === 'if') {
                return this.parseIfStatement();
            }
            if (t.value === 'while') {
                return this.parseWhileStatement();
            }
            if (t.value === 'return') {
                return this.parseReturnStatement();
            }
        }

        let expr = this.parseExpression();
        this.match('SYM', ';');
        return { type: 'ExpressionStatement', expression: expr };
    }

    parseVariableDeclaration() {
        let isLet = this.next().value === 'let';
        let id = this.expect('ID').value;
        
        let varType = null;
        if (this.match('SYM', ':')) {
            let typeName = this.expect('KEY').value;
            if (this.match('SYM', '[')) {
                this.expect('SYM', ']');
                varType = typeName + '[]';
            } else {
                varType = typeName;
            }
        }
        
        this.expect('SYM', '=');
        let init = this.parseExpression();
        this.expect('SYM', ';');
        
        return { type: 'VariableDeclaration', id, varType, isLet, initializer: init };
    }

    parseFunctionDeclaration() {
        this.expect('KEY', 'fn');
        let name = this.expect('ID').value;
        this.expect('SYM', '(');
        let params = [];
        if (this.peek() && this.peek().value !== ')') {
            while (true) {
                let pid = this.expect('ID').value;
                this.expect('SYM', ':');
                let ptype = this.expect('KEY').value;
                params.push({ id: pid, type: ptype });
                if (!this.match('SYM', ',')) break;
            }
        }
        this.expect('SYM', ')');
        
        let returnType = 'void';
        if (this.match('SYM', ':')) {
            returnType = this.expect('KEY').value;
        }

        let body = this.parseBlock();
        return { type: 'FunctionDeclaration', name, params, returnType, body };
    }

    parseIfStatement() {
        this.expect('KEY', 'if');
        this.expect('SYM', '(');
        let cond = this.parseExpression();
        this.expect('SYM', ')');
        let thenBranch = this.parseBlockOrStatement();
        let elseBranch = null;
        if (this.match('KEY', 'else')) {
            elseBranch = this.parseBlockOrStatement();
        }
        return { type: 'IfStatement', condition: cond, thenBranch, elseBranch };
    }

    parseWhileStatement() {
        this.expect('KEY', 'while');
        this.expect('SYM', '(');
        let cond = this.parseExpression();
        this.expect('SYM', ')');
        let body = this.parseBlockOrStatement();
        return { type: 'WhileStatement', condition: cond, body };
    }

    parseReturnStatement() {
        this.expect('KEY', 'return');
        let value = null;
        if (this.peek() && this.peek().value !== ';') {
            value = this.parseExpression();
        }
        this.match('SYM', ';');
        return { type: 'ReturnStatement', value };
    }

    parseBlock() {
        this.expect('SYM', '{');
        const statements = [];
        while (this.peek() && this.peek().value !== '}') {
            statements.push(this.parseStatement());
        }
        this.expect('SYM', '}');
        return { type: 'Block', body: statements };
    }

    parseBlockOrStatement() {
        if (this.peek() && this.peek().value === '{') {
            return this.parseBlock();
        }
        return this.parseStatement();
    }

    parseExpression() {
        return this.parseAssignment();
    }

    parseAssignment() {
        let expr = this.parseOr();
        if (this.match('SYM', '=')) {
            let right = this.parseAssignment();
            return { type: 'Assignment', left: expr, right };
        }
        return expr;
    }

    parseOr() {
        let left = this.parseAnd();
        while (this.peek() && this.peek().value === '||') {
            let op = this.next().value;
            let right = this.parseAnd();
            left = { type: 'Binary', op, left, right };
        }
        return left;
    }

    parseAnd() {
        let left = this.parseEquality();
        while (this.peek() && this.peek().value === '&&') {
            let op = this.next().value;
            let right = this.parseEquality();
            left = { type: 'Binary', op, left, right };
        }
        return left;
    }

    parseEquality() {
        let left = this.parseComparison();
        while (this.peek() && ['==', '!='].includes(this.peek().value)) {
            let op = this.next().value;
            let right = this.parseComparison();
            left = { type: 'Binary', op, left, right };
        }
        return left;
    }

    parseComparison() {
        let left = this.parseAddition();
        while (this.peek() && ['<', '<=', '>', '>='].includes(this.peek().value)) {
            let op = this.next().value;
            let right = this.parseAddition();
            left = { type: 'Binary', op, left, right };
        }
        return left;
    }

    parseAddition() {
        let left = this.parseMultiplication();
        while (this.peek() && ['+', '-'].includes(this.peek().value)) {
            let op = this.next().value;
            let right = this.parseMultiplication();
            left = { type: 'Binary', op, left, right };
        }
        return left;
    }

    parseMultiplication() {
        let left = this.parsePrimary();
        while (this.peek() && ['*', '/'].includes(this.peek().value)) {
            let op = this.next().value;
            let right = this.parsePrimary();
            left = { type: 'Binary', op, left, right };
        }
        return left;
    }

    parsePrimary() {
        let t = this.peek();
        if (!t) throw new Error("Unexpected end of input");

        if (t.type === 'NUM') {
            return { type: 'Literal', value: this.next().value };
        }
        if (t.type === 'STR') {
            return { type: 'Literal', value: this.next().value };
        }
        if (t.type === 'CHAR') {
            return { type: 'Literal', value: this.next().value };
        }
        if (t.type === 'KEY' && (t.value === 'true' || t.value === 'false')) {
            return { type: 'Literal', value: this.next().value === 'true' };
        }

        if (t.type === 'KEY' && t.value === 'print') {
            this.next();
            this.expect('SYM', '(');
            let arg = this.parseExpression();
            this.expect('SYM', ')');
            return { type: 'Print', argument: arg };
        }

        if (t.type === 'ID') {
            let name = this.next().value;
            let expr = { type: 'Variable', name };
            
            while (true) {
                if (this.match('SYM', '[')) {
                    let index = this.parseExpression();
                    this.expect('SYM', ']');
                    expr = { type: 'IndexAccess', array: expr, index };
                } else if (this.peek() && this.peek().type === 'SYM' && this.peek().value === '(') {
                    this.next();
                    let args = [];
                    if (this.peek() && this.peek().value !== ')') {
                        while (true) {
                            args.push(this.parseExpression());
                            if (!this.match('SYM', ',')) break;
                        }
                    }
                    this.expect('SYM', ')');
                    expr = { type: 'Call', callee: name, arguments: args };
                } else {
                    break;
                }
            }
            return expr;
        }

        if (this.match('SYM', '[')) {
            let elements = [];
            if (this.peek() && this.peek().value !== ']') {
                while (true) {
                    elements.push(this.parseExpression());
                    if (!this.match('SYM', ',')) break;
                }
            }
            this.expect('SYM', ']');
            return { type: 'ArrayLiteral', elements };
        }

        if (this.match('SYM', '(')) {
            let expr = this.parseExpression();
            this.expect('SYM', ')');
            return expr;
        }

        throw new Error(`Unexpected token '${t.value}'`);
    }
}

class ReturnException extends Error {
    constructor(value) {
        super();
        this.value = value;
    }
}

class Environment {
    constructor(parent = null) {
        this.vars = {};
        this.parent = parent;
    }
    declare(name, val) {
        this.vars[name] = val;
    }
    assign(name, val) {
        if (name in this.vars) {
            this.vars[name] = val;
            return;
        }
        if (this.parent) {
            this.parent.assign(name, val);
            return;
        }
        throw new Error(`Variable '${name}' is not defined in scope`);
    }
    lookup(name) {
        if (name in this.vars) {
            return this.vars[name];
        }
        if (this.parent) {
            return this.parent.lookup(name);
        }
        throw new Error(`Variable '${name}' is not defined in scope`);
    }
}

class Evaluator {
    constructor(outFn) {
        this.outFn = outFn;
        this.functions = {};
    }

    eval(node, env) {
        switch (node.type) {
            case 'Program':
                for (let stmt of node.body) {
                    this.eval(stmt, env);
                }
                return null;
            case 'Block':
                let blockEnv = new Environment(env);
                for (let stmt of node.body) {
                    this.eval(stmt, blockEnv);
                }
                return null;
            case 'VariableDeclaration':
                let initVal = this.eval(node.initializer, env);
                env.declare(node.id, initVal);
                return null;
            case 'FunctionDeclaration':
                this.functions[node.name] = node;
                return null;
            case 'IfStatement':
                let condVal = this.eval(node.condition, env);
                if (condVal) {
                    this.eval(node.thenBranch, env);
                } else if (node.elseBranch) {
                    this.eval(node.elseBranch, env);
                }
                return null;
            case 'WhileStatement':
                let loopCount = 0;
                while (this.eval(node.condition, env)) {
                    loopCount++;
                    if (loopCount > 10000) {
                        throw new Error("Execution aborted: Infinite loop safety limit exceeded (10,000 iterations)");
                    }
                    this.eval(node.body, env);
                }
                return null;
            case 'ReturnStatement':
                let retVal = node.value ? this.eval(node.value, env) : null;
                throw new ReturnException(retVal);
            case 'ExpressionStatement':
                this.eval(node.expression, env);
                return null;
            case 'Assignment':
                let rightVal = this.eval(node.right, env);
                if (node.left.type === 'Variable') {
                    env.assign(node.left.name, rightVal);
                } else if (node.left.type === 'IndexAccess') {
                    let arr = this.eval(node.left.array, env);
                    let idx = this.eval(node.left.index, env);
                    if (!Array.isArray(arr)) throw new Error("Target is not an array");
                    if (idx < 0 || idx >= arr.length) throw new Error("Subscript index out of bounds");
                    arr[idx] = rightVal;
                } else {
                    throw new Error("Invalid left-hand side assignment target");
                }
                return rightVal;
            case 'Binary':
                let leftVal = this.eval(node.left, env);
                let op = node.op;
                
                if (op === '&&') {
                    return leftVal && this.eval(node.right, env);
                }
                if (op === '||') {
                    return leftVal || this.eval(node.right, env);
                }

                let nextVal = this.eval(node.right, env);
                switch (op) {
                    case '+': return leftVal + nextVal;
                    case '-': return leftVal - nextVal;
                    case '*': return leftVal * nextVal;
                    case '/': 
                        if (nextVal === 0) throw new Error("ArithmeticException: Division by zero");
                        return leftVal / nextVal;
                    case '==': return leftVal === nextVal;
                    case '!=': return leftVal !== nextVal;
                    case '<': return leftVal < nextVal;
                    case '<=': return leftVal <= nextVal;
                    case '>': return leftVal > nextVal;
                    case '>=': return leftVal >= nextVal;
                    default: throw new Error(`Unknown operator ${op}`);
                }
            case 'Literal':
                return node.value;
            case 'Variable':
                return env.lookup(node.name);
            case 'IndexAccess':
                let arr = this.eval(node.array, env);
                let idx = this.eval(node.index, env);
                if (!Array.isArray(arr)) throw new Error("Subscript targeting non-array");
                if (idx < 0 || idx >= arr.length) throw new Error(`IndexOutOfRangeException: Array index ${idx} out of range (length ${arr.length})`);
                return arr[idx];
            case 'ArrayLiteral':
                return node.elements.map(e => this.eval(e, env));
            case 'Print':
                let argVal = this.eval(node.argument, env);
                if (Array.isArray(argVal)) {
                    this.outFn(`[${argVal.join(', ')}]`);
                } else {
                    this.outFn(argVal);
                }
                return null;
            case 'Call':
                let func = this.functions[node.callee];
                if (!func) throw new Error(`Function '${node.callee}' is not declared`);
                let callEnv = new Environment(new Environment());
                
                if (node.arguments.length !== func.params.length) {
                    throw new Error(`ArgumentException: Function '${node.callee}' expects ${func.params.length} arguments, got ${node.arguments.length}`);
                }

                for (let j = 0; j < func.params.length; j++) {
                    let argVal = this.eval(node.arguments[j], env);
                    callEnv.declare(func.params[j].id, argVal);
                }
                try {
                    this.eval(func.body, callEnv);
                } catch (e) {
                    if (e instanceof ReturnException) {
                        return e.value;
                    }
                    throw e;
                }
                return null;
            default:
                throw new Error(`Unknown node type: ${node.type}`);
        }
    }
}

// Playground runner action
function runPlayground() {
    clearConsole();
    logToConsole('Compiling main.kdh ...', 'system');
    logToConsole('Emitting MSIL DynamicMethod instructions ...', 'system');
    logToConsole('Executing...', 'system');

    const code = codeEditor.value;
    try {
        const tokens = tokenize(code);
        const parser = new Parser(tokens);
        const program = parser.parseProgram();
        
        const globalEnv = new Environment();
        const evaluator = new Evaluator((output) => {
            logToConsole(String(output), 'output');
        });

        evaluator.eval(program, globalEnv);
        logToConsole('Execution completed successfully.', 'system');
    } catch (err) {
        logToConsole(err.message, 'error');
    }
}

// Tab Key Interception in Textarea
codeEditor.addEventListener('keydown', (e) => {
    if (e.key === 'Tab') {
        e.preventDefault();
        const start = codeEditor.selectionStart;
        const end = codeEditor.selectionEnd;
        const val = codeEditor.value;
        
        // Insert a tab character
        codeEditor.value = val.substring(0, start) + '\t' + val.substring(end);
        
        // Move caret position
        codeEditor.selectionStart = codeEditor.selectionEnd = start + 1;
        
        onEditorInput();
    }
});

// Initial Setup
selectTopic('welcome');
updateLineNumbers();
updateHighlighting();
