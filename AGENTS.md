# AGENTS.md

This file contains guidelines and commands for agentic coding agents working in this repository.

## Build/Test Commands

### Primary Commands
```bash
# Build and test everything (preferred)
dotnet test -c Release

# Step-by-step commands
dotnet build -c Release                     # Build all projects
dotnet test -c Release                      # Run tests
dotnet pack src/fsharp-html-dsl -c Release  # Create package

# Run benchmarks
dotnet run --project src/fsharp-html-dsl/benchmarks/benchmarks.fsproj -c Release
```

### Running Single Tests
```bash
# Run specific test class
dotnet test src/fsharp-html-dsl/fsharp-html-dsl-tests/ --filter "FullyQualifiedName~HtmlTests" -c Release

# Run specific test method
dotnet test src/fsharp-html-dsl/fsharp-html-dsl-tests/ --filter "FullyQualifiedName~Elements" -c Release

# Alternative filter syntax
dotnet test src/fsharp-html-dsl/fsharp-html-dsl-tests/ --filter "DisplayName~Elements" -c Release
```

### Frontend Commands (convert-html-to-fsharp-html-dsl)
```bash
cd src/convert-html-to-fsharp-html-dsl
npm start     # Development server
npm run build # Production build
npm run clean # Clean build artifacts
```

## Project Structure

### F# Projects
- `src/fsharp-html-dsl/fsharp-html-dsl/` - Main library
- `src/fsharp-html-dsl/fsharp-html-dsl-tests/` - Test suite
- `src/fsharp-html-dsl/benchmarks/` - Performance benchmarks

### Frontend Project
- `src/convert-html-to-fsharp-html-dsl/` - HTML to F# DSL converter (Parcel/JavaScript)

## Code Style Guidelines

### F# Conventions

#### Naming
- **Modules**: PascalCase (e.g., `XmlNode`, `Attr`, `Elem`)
- **Functions**: camelCase for public APIs with underscore prefix (e.g., `_div`, `_class_`)
- **Types**: PascalCase (e.g., `XmlAttribute`, `XmlNode`)
- **Private functions**: camelCase without underscore (e.g., `writeAttributes`, `buildXml`)
- **Reserved keywords**: Append prime (') for disambiguation (e.g., `class'`, `checked'`, `type'`)

#### Module Organization
- File order matters in F# - follow compilation order in `.fsproj`
- Core types first (`XmlNode.fs`), then modules (`Attr.fs`, `Elem.fs`, `Text.fs`)
- Public API modules last (`Html.fs`, `Svg.fs`, `Templates.fs`)
- Use `[<AutoOpen>]` for convenience modules that should be automatically available

#### Function Design
- **Element functions**: `attr list -> XmlNode list -> XmlNode` (for parent nodes)
- **Self-closing elements**: `attr list -> XmlNode` (for void elements)
- **Attribute functions**: `string -> XmlAttribute` or `unit -> XmlAttribute` (for boolean attrs)
- **Text functions**: `string -> XmlNode` or `string -> XmlNode` (formatted)

#### Imports and Namespaces
- Keep namespaces minimal and focused
- `open System` and `System.*` at top when needed
- Group related opens together
- Use qualified access when it improves readability (e.g., `Attr.merge`)

### Type System Guidelines

#### Discriminated Unions
- Use for XML node types: `TextNode`, `SelfClosingNode`, `ParentNode`
- Use for attribute types: `KeyValueAttr`, `NonValueAttr`
- Pattern match exhaustively

#### Records and Tuples
- Prefer records for named data structures
- Use tuples for simple return values when fields are self-evident

#### Option Types
- Use `Option` instead of null for missing values
- Leverage built-in Option module functions (`Option.map`, `Option.orElse`)

### Error Handling

#### Exceptions
- Use exceptions for truly exceptional conditions
- Prefer `Result` or `Option` for expected failure cases in public APIs

#### Validation
- Validate inputs at API boundaries
- Provide meaningful error messages

### Performance Guidelines

#### String Building
- Use `StringBuilderCache` for efficient string concatenation
- Follow the existing pattern for XML serialization
- Avoid string concatenation in loops

#### Memory Allocation
- Be mindful of allocations in hot paths (rendering)
- Use value types where appropriate
- Leverage .NET's built-in collections efficiently

### Testing Guidelines

#### Test Structure
- Use xUnit with `[<Fact>]` attributes
- Organize tests by module (`HtmlTests`, `SvgTests`, `TextTests`)
- Use FsUnit for fluent assertions (`should equal`)

#### Test Naming
- Use descriptive test names with backticks: `let ``Elements`` () =`
- Group related tests together
- Test both positive and negative cases

#### Test Data
- Keep test data simple and focused
- Use consistent patterns across test files
- Test edge cases (empty lists, null values, etc.)

### Documentation Standards

#### XML Documentation
- Generate documentation file enabled (`<GenerateDocumentationFile>true</GenerateDocumentationFile>`)
- Use triple-slash comments for public APIs
- Document parameters and return types

#### Comments
- Use comments sparingly - let code be self-documenting
- Explain "why" not "what" when necessary
- Document complex algorithms or business rules

### Git and Version Control

#### Commits
- Commit messages should be concise and descriptive
- Use conventional commit format when possible
- Commit related changes together

#### Branching
- Work on feature branches
- Keep main branch stable
- Use pull requests for review

## Tool Configuration

### Build Settings
- Target Framework: .NET 8.0
- Warning Level: 5 (treat warnings as errors enabled)
- F# Core version: 8.0.403 (managed centrally)

### Testing Framework
- xUnit for test runner
- FsUnit for assertions
- Microsoft.NET.Test.Sdk for test infrastructure

### Package Management
- Use NuGet for .NET dependencies
- Use npm for frontend dependencies
- Update F# Core centrally via `PackageReference Update="FSharp.Core"`

## Common Patterns

### Attribute Merging
```fsharp
let attrs' = Attr.merge [ _class_ "default-class" ] runtimeAttrs
```

### Component Creation
```fsharp
let myComponent (attrs: XmlAttribute list) (content: XmlNode list) =
    _div [ _class_ "my-component" ] content
```

### Custom Elements
```fsharp
module CustomElem =
    let myElement = Elem.create "my-element"

module CustomAttr =
    let myCustomAttr = Attr.create "data-custom"
```

## Performance Considerations

- The library is optimized for performance - be careful with changes to rendering logic
- StringBuilder caching is critical for high-throughput scenarios
- Benchmark changes using the BenchmarkDotNet project
- Test with realistic HTML document sizes

## Security Notes

- HTML encoding is handled by `Text.enc` for user input
- Be careful with raw text insertion - use encoded text when appropriate
- Validate attribute values when they come from external sources