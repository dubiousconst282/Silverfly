# Silverfly

[![CodeFactor](https://www.codefactor.io/repository/github/furesoft/furesoft.prattparser/badge)](https://www.codefactor.io/repository/github/furesoft/furesoft.prattparser)
[![Build and Publish NuGet Package](https://github.com/furesoft/Furesoft.PrattParser/actions/workflows/pack.yaml/badge.svg)](https://github.com/furesoft/Furesoft.PrattParser/actions/workflows/pack.yaml)

# Silverfly

Silverfly is a versatile parsing framework that provides extensive support for building custom parsers with ease. It supports Pratt parsing, a powerful method for parsing expressions and statements in a flexible manner.

## Features

- **Flexible Parsing**: Supports Pratt parsing for complex expression handling.
- **Extensible**: Easily extend the parser and lexer with custom rules.
- **Documentation**: Comprehensive instructions available in the wiki.

## Installation

To install Silverfly, you can use NuGet:

```bash
dotnet add package Silverfly
```

## Usage

```csharp
﻿using Silverfly;

namespace Sample;

public class Program
{
    public static void Main(string[] args)
    {
        while (true)
        {
            Console.Write("> ");
            var input = Console.ReadLine();

            var parsed = Parser.Parse<ExpressionGrammar>(input);
            var evaluated = parsed.Tree.Accept(new EvaluationVisitor());

            Console.WriteLine("> " + evaluated);
        }
    }
}
```

For more detailed instructions and advanced usage, please refer to the [Wiki](https://github.com/furesoft/Silverfly/wiki).
A great example can be found [here](https://github.com/furesoft/Silverfly/tree/main/Source/Sample)

## Contributing
We welcome contributions! Please see our contributing guidelines for more details on how to get involved.
