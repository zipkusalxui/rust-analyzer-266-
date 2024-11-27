# Rust Plugin Analyzer

A Roslyn-based code analyzer for Rust game plugins that helps maintain code quality by detecting common issues.

## Features

1. **Empty Method Detection (RUST001)**
   - Detects empty methods that might indicate incomplete implementation
   - Helps identify forgotten code or unnecessary methods
   - [Documentation](docs/RUST001.md)

2. **Unused Method Detection (RUST003)**
   - Finds methods that are never used in the codebase
   - Helps maintain clean code by identifying dead code
   - [Documentation](docs/RUST003.md)

## Installation

1. Install via NuGet Package Manager:
   ```powershell
   Install-Package rust-analyzer
   ```

2. Or via .NET CLI:
   ```bash
   dotnet add package rust-analyzer
   ```

## Requirements

- .NET Standard 2.0 or higher
- Visual Studio 2019 or higher
- C# 8.0 or higher

## Usage

The analyzer will automatically start working once added to your project. It provides:

- Real-time analysis in Visual Studio
- Warning messages with detailed explanations
- Quick fixes for common issues
- Documentation links for each rule

## Rules

| Rule ID | Category | Severity | Description |
|---------|----------|----------|-------------|
| RUST001 | Design   | Warning  | Empty method detection |
| RUST003 | Design   | Warning  | Unused method detection |

## Configuration

No additional configuration is required. The analyzer works out of the box with default settings.

## Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Built using the [.NET Compiler Platform (Roslyn)](https://github.com/dotnet/roslyn)
- Inspired by the Rust game modding community
