# Auto Import

[![Visual Studio Marketplace](https://img.shields.io/badge/Visual%20Studio-Marketplace-orange?logo=visual-studio&style=flat-square)](https://marketplace.visualstudio.com/items?itemName=floatas.auto-import)

## Overview

`Auto Import` is a Visual Studio extension that automates the process of importing classes and interfaces into your C# codebase. This can help streamline your development process by eliminating the need for manual importing of types, saving you time and reducing the chance of errors.

## Features

- Auto-detects the classes/interfaces used in your code and adds appropriate using statements.
- Refactors and renames identifiers, improving code readability and organization.
- Provides support for generic types.
- Allows integration into the constructor of a class.



![Introduce service](/Screenshots/IntroduceService.png "Introduce service")

![Introduce Interface](/Screenshots/introduceInterface.png "Introduce interface")

![Introduce Generic](/Screenshots/IntroduceGeneric.png "Introduce Generic")


## Installation

This extension can be installed through the Visual Studio Marketplace. Click [here](https://marketplace.visualstudio.com/items?itemName=floatas.auto-import) to download and install.

## Usage

Once the extension is installed, it will automatically run in the background. It analyzes your code as you type, detecting classes, interfaces, and binary expressions that require imports. If any are found, the extension will automatically generate the appropriate `using` statements at the top of your code file.

## Code Overview

The main functionality of the extension is defined in `AutoImportCodeRefactoringProvider`. This class inherits from the `CodeRefactoringProvider` class in the `Microsoft.CodeAnalysis` namespace. The primary method, `ComputeRefactoringsAsync`, performs the analysis and refactoring.

Additionally, the helper methods `Refactor`, `CreateDataModel`, and `RenameGeneric` assist in handling specific aspects of the code refactoring process.

## Contributing

Contributions are always welcome. If you have any ideas for improvement, please feel free to fork the repository and create a pull request.

## Support

If you encounter any problems or have any questions, please open an issue on the GitHub repository.

## License

This project is licensed under the MIT License.

---

Your feedback is important to make the project better. Feel free to contribute and help the developer community.
