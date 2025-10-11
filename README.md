# FileCompressionCSharp

A simple file compression program developed in C# to efficiently compress and decompress files using a LZ77/LZ78 AND Huffman compression algorithms.

## Project Overview

The `FileCompressionCSharp` project provides a straightforward implementation of file compression and decompression functionalities. The user may choose from 3 different compression algorithims.
- **Sliding Window (LZ77/LZ78)**:
- **Huffman Algorithm**:
- **Sliding Window AND Huffman**:

## Features

- **Compression Algorithms**: Utilizes standard algorithms to compress files effectively.
   * **Sliding Window (LZ77/LZ78)**: Compresses data by replacing repeated occurrences with references to previous occurrences.
   * **Huffman Algorithm**: Uses frequency-based encoding to reduce file size efficiently.
   * **Sliding Window AND Huffman**: Combines both methods to maximize compression efficiency and reduce redundancy.    
- **Decompression Support**: Allows for the decompression of files back to their original state.
- **Simple Interface**: Provides an intuitive API for integrating compression functionalities into applications.
- **Cross-Platform Compatibility**: Designed to work seamlessly across different platforms supported by .NET.

## Project Structure

The repository is organized into the following key components:

- **FileCompressionCSharp**: Acts as the presentation layer and provides a simple and intuitive UI. 
- **LogicLayer**: Implements the core logic for compression/decompression algorithms.
- **LogicLayerAccessor**: Provides access to the logic layer, facilitating interaction with other components.
- **DataObjects**: Defines data models representing the structure of compressed files and related metadata.
- **UnitTesting**: Includes unit tests to validate the functionality and reliability of the application.

## Getting Started

To get started with the `FileCompressionCSharp` project:

1. **Clone the Repository**:

   ```bash
   git clone https://github.com/Ialbashair/FileCompressionCSharp.git
   cd FileCompressionCSharp
   ```

2. **Open the Solution**:

   Open the `FileCompressionCSharp.sln` solution file in Visual Studio or your preferred C# development environment.

3. **Build the Solution**:

   Build the solution to restore dependencies and compile the project.

4. **Run the Application**:

   Execute the application to start compressing and decompressing files.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
