# 🍁 Kaedehara Programming Language & Compiler

Welcome to **Kaedehara**, a custom strongly-typed programming language written in C# and compiled directly to executable MSIL (Microsoft Intermediate Language) bytecode using a dynamic `System.Reflection.Emit` compilation pipeline.

Inspired by Immo Landwerth's *Minsk* compiler series, Kaedehara features a complete lexical analyzer, recursive-descent parser, binder, lowerer, and a dynamic MSIL bytecode generation backend.

> [!TIP]
> **Try the Interactive Web Tutorial!**  
> We have created a fully interactive, offline-capable W3Schools-style documentation page and code playground inside this repository. Simply open [tutorial/index.html](file:///c:/project/Kaedehara_Compiler/tutorial/index.html) in your browser to learn, write, and execute Kaedehara code with syntax highlighting and a live console output directly in your browser!

---

## 📖 Language Tutorial & Features

Kaedehara is an expression-oriented, block-scoped language. Let's look at its syntax and capabilities like a tutorial.

### 1. Variables and Mutability
Kaedehara enforces mutability at the variable declaration level using two keywords:
* `let`: Defines read-only/immutable constants.
* `var`: Defines mutable variables that can be reassigned.

Type annotations are optional. If omitted, the binder automatically infers the variable type based on the initializer expression:

```rust
let limit: int = 100;     // Explicit int constant
let name = "Kaedehara";   // Inferred string constant
var count = 0;            // Inferred mutable variable

count = count + 1;        // Reassignment is allowed
// limit = 200;           // Compile Error: limit is read-only!
```

---

### 2. Primitive Data Types
Kaedehara supports five core primitive data types:
* `int`: 32-bit signed integers (e.g. `42`, `-5`).
* `float`: 64-bit double-precision floating-point numbers (e.g. `3.14`, `-0.007`).
* `char`: Single characters wrapped in single quotes (e.g. `'A'`, `'\n'`).
* `bool`: Boolean values (`true` or `false`).
* `string`: UTF-8 character sequences (e.g. `"Hello, World!"`).

Implicit conversions are performed automatically when safe (e.g., from `int` to `float` during arithmetic expressions):

```rust
let a = 10;
let b = 2.5;
let result = a + b; // result is implicitly promoted to float (12.5)
```

---

### 3. Arrays and Subscript Indexing
Arrays are declared using bracket syntax (`[]`) and initialized using bracket literals:

```rust
let list: int[] = [10, 20, 30];
```

* **Type Inference**: When initializing arrays, the binder determines the best common element type. If elements are compatible, it promotes them implicitly (e.g. `[1, 2.5]` promotes to `float[]`).
* **Subscript Indexing**: You can read and write elements using index brackets:

```rust
let first = list[0];       // Reads 10
var scores = [80.5, 90.0];
scores[1] = 95.5;          // Mutates element in place
```

---

### 4. Control Flow
Kaedehara supports standard conditional branches (`if`/`else`) and loops (`while`):

```rust
var index = 0;
while (index < 3) {
    print("Loop index: " + index);
    index = index + 1;
}

if (index == 3) {
    print("Finished!");
} else {
    print("Still running...");
}
```

---

### 5. User-Defined Functions and Recursion
Functions are declared with the `fn` keyword. Parameters must declare types, and functions can optionally specify a return type (defaulting to `void`):

```rust
// Recursive Fibonacci function
fn fibonacci(n: int): int {
    if (n <= 1) {
        return n;
    }
    return fibonacci(n - 1) + fibonacci(n - 2);
}

let result = fibonacci(6);
print("Fibonacci(6) = " + result); // Outputs: 8
```

---

## 🛠️ Compiler Architecture & MSIL Emitter

Rather than interpreting code using a slow AST tree-walk, Kaedehara compiles scripts on the fly using a high-performance **MSIL Emitter** backend.

### Compilation Phases
1. **Lexer** ([Lexer.cs](file:///c:/project/Kaedehara_Compiler/Kaedehara/CodeAnalysis/Syntax/Lexer.cs)): Tokenizes source text.
2. **Parser** ([Parser.cs](file:///c:/project/Kaedehara_Compiler/Kaedehara/CodeAnalysis/Syntax/Parser.cs)): Constructs a Concrete Syntax Tree.
3. **Binder** ([Binder.cs](file:///c:/project/Kaedehara_Compiler/Kaedehara/CodeAnalysis/Binding/Binder.cs)): Annotates variables, resolves type checking, checks for assignment safety, and emits compiler diagnostics.
4. **Lowerer** ([Lowerer.cs](file:///c:/project/Kaedehara_Compiler/Kaedehara/CodeAnalysis/Lowering/Lowerer.cs)): Simplifies complex statements (like loops) into flat goto instructions.
5. **Emitter** ([Emitter.cs](file:///c:/project/Kaedehara_Compiler/Kaedehara/CodeAnalysis/Emit/Emitter.cs)): Generates dynamic intermediate language instructions via `ILGenerator`.

### High-Performance Backend Details
* **Thread-Safe Global Scopes**: Global variables persist across submissions inside a static state manager [GlobalState](file:///c:/project/Kaedehara_Compiler/Kaedehara/CodeAnalysis/Emit/Emitter.cs#L11-L64) and are looked up via indexed pointers.
* **Native Local Variables**: Variables declared inside function declarations are mapped directly to native CLR stack slots using `ILGenerator.DeclareLocal(Type)` for native-speed execution.
* **Mutual Recursion Support**: The compiler scans and declares `DynamicMethod` function headers globally before compiling function bodies, ensuring self-calls and mutual calls resolve correctly.

---

## 🚀 Getting Started

### Prerequisites
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download)

### Building the Project
Clone the repository and run build commands in the root folder:

```bash
dotnet build
```

### Running the REPL Driver
The `kdhc` console driver provides an interactive terminal session where you can execute code, look up abstract syntax trees, and view MSIL generation dumps.

Run the driver:
```bash
dotnet run --project kdhc
```

#### Interactive REPL Enhancements
* **Tab Autocompletion**: Hit `Tab` to cycle suggestions through reserved keywords and active variables in scope.
* **Error Underlining**: Diagnostic error listings point to coordinates and draw character underlines using `~` symbols precisely aligned with tabs and character widths.
* **Meta Commands**:
  * `#ShowTree`: Toggle displaying the AST parse tree.
  * `#ShowProgram`: Toggle displaying the lowered bound tree layout.
  * `#clear`: Clear the console screen.
  * `#reset`: Reset the compiler context and clear all active variables.

---

## 🧪 Testing

Kaedehara includes a comprehensive compiler test suite validating diagnostics, parser precedence, type conversions, array mutations, and execution pipelines.

Run the tests:
```bash
dotnet test
```
