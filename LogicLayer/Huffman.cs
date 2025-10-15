using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogicLayerInterface;
using DataAccessInterface;
using DataAccess;

namespace LogicLayer
{
    public class Huffman : IHuffman
    {
        // Helper class for Huffman tree nodes
        private class Node
        {
            public byte? Symbol { get; set; }
            public int Frequency { get; set; }
            public Node Left { get; set; }
            public Node Right { get; set; }

            public bool IsLeaf => Left == null && Right == null;
        }

        FileReaderInterface _fileReader;

        public Huffman()
        {
            _fileReader = new FileReader();
        }

        public Huffman(FileReaderInterface fileReader)
        {
            _fileReader = fileReader;
        }

        // Build frequency table from input data
        private Dictionary<byte, int> BuildFrequencyTable(byte[] data)
        {
            var frequencyTable = new Dictionary<byte, int>();

            foreach (byte b in data)
            {
                if (!frequencyTable.ContainsKey(b))
                {
                    frequencyTable[b] = 0;
                }
                frequencyTable[b]++;
            }
            return frequencyTable;
        }

        // Build Huffman tree from frequency table
        private Node BuildTree(Dictionary<byte, int> freq)
        {
            // PriorityQueue can be used in newer versions of C#. this is a manual implementation of a simple priority queue
            var priorityQueue = new Utils.SimplePriorityQueue<Node>();

            foreach (var kvp in freq)
            {
                priorityQueue.Enqueue(new Node
                {
                    Symbol = kvp.Key,
                    Frequency = kvp.Value
                }, kvp.Value);
            }

            while (priorityQueue.Count > 1)
            {
                priorityQueue.TryDequeue(out var left, out _);
                priorityQueue.TryDequeue(out var right, out _);

                var parent = new Node
                {
                    Left = left,
                    Right = right,
                    Frequency = left.Frequency + right.Frequency
                };

                priorityQueue.Enqueue(parent, parent.Frequency);
            }

            priorityQueue.TryDequeue(out var root, out _);

            return root;
        }

        // Build code table from Huffman tree
        private Dictionary<byte, string> BuildCodeTable(Node node)
        {
            var codes = new Dictionary<byte, string>();

            BuildCodeRecursive(node, "", codes);

            return codes;
        }

        // Recursive helper to build codes
        private void BuildCodeRecursive(Node node, string prefix, Dictionary<byte, string> codes)
        {
            if (node.IsLeaf && node.Symbol.HasValue)
            {
                codes[node.Symbol.Value] = prefix.Length > 0 ? prefix : "0";
                return;
            }

            if (node.Left != null)
            {
                BuildCodeRecursive(node.Left, prefix + "0", codes);
            }

            if (node.Right != null)
            {
                BuildCodeRecursive(node.Right, prefix + "1", codes);
            }
        }

        // Encode input data using the code table
        private byte[] EncodeData(byte[] input, Dictionary<byte, string> codes)
        {
            var bitString = new StringBuilder();

            foreach (byte b in input)
            {
                bitString.Append(codes[b]);
            }

            int numBytes = (bitString.Length + 7) / 8;
            byte[] output = new byte[numBytes];

            for (int i = 0; i < bitString.Length; i++)
            {
                if (bitString[i] == '1')
                {
                    output[i / 8] |= (byte)(1 << (7 - (i % 8)));
                }
            }

            return output;
        }
        // Write compressed data to a file with error handling
        public bool WriteCompressedFile(byte[] compressedData, string outputPath)
        {
            bool success = false;

            try
            {
                // Check if directory exists, if not create it
                string directory = Path.GetDirectoryName(outputPath);
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory); // Create.
                }

                // Write file data.
                File.WriteAllBytes(outputPath, compressedData);
                success = true;
            }
            catch (UnauthorizedAccessException)
            {
                throw new Exception("Permission denied at this write location.");
            }
            catch (DirectoryNotFoundException) // This should not occur due to the directory creation step, but just in case.
            {
                throw new Exception("The specified directory was not found.");
            }
            catch (IOException ioEx)
            {
                throw new Exception($"An I/O error occurred: {ioEx.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"An unexpected error occurred: {ex.Message}");
            }

            return success;
        }
        
        public byte[] LoadInputData(string filePath)
        {
            byte[] inputData;
            try
            {
                return inputData = _fileReader.FileToByteArray(filePath);
            }
            catch (Exception e)
            {

                throw new ApplicationException("Failed to get file byte array." + e.Message);
            }
        }

        private Dictionary<byte, int> DeserializeFrequencyTable(byte[] data)
        {
            var table = new Dictionary<byte, int>();

            using (var ms = new MemoryStream(data))
            using (var reader = new BinaryReader(ms))
            {
                int count = reader.ReadInt32(); // number of entries
                for (int i = 0; i < count; i++)
                {
                    byte symbol = reader.ReadByte();
                    int frequency = reader.ReadInt32();
                    table[symbol] = frequency;
                }
            }

            return table;
        }

        private byte[] SerializeFrequencyTable(Dictionary<byte, int> frq)
        {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                writer.Write(frq.Count);
                foreach (var kvp in frq)
                {
                    writer.Write(kvp.Key);
                    writer.Write(kvp.Value);
                }
                return ms.ToArray();
            }
        }

        public bool WriteDecompressedFile(string outputPath, byte[] decompressedData)
        {

            if (string.IsNullOrWhiteSpace(outputPath))
                throw new ArgumentException("Output path cannot be null or empty.", nameof(outputPath));

            if (decompressedData == null || decompressedData.Length == 0)
                throw new ArgumentException("Decompressed data cannot be null or empty.", nameof(decompressedData));

            bool success = false;

            try
            {
                File.WriteAllBytes(outputPath, decompressedData);
                success = true;
            }
            catch (Exception ex)
            {
                throw new IOException("An error occurred while writing the decompressed file.", ex);
            }
            return success;
        }

        public bool Compress(string filePath, string outputPath)
        {
            byte[] inputData = LoadInputData(filePath);
            var frequencyTable = BuildFrequencyTable(inputData);
            var huffmanTree = BuildTree(frequencyTable);
            var codes = BuildCodeTable(huffmanTree);
            var compressedData = EncodeData(inputData, codes);

            // Get file name
            string fileName = Path.GetFileName(filePath);
            byte[] fileNameBytes = Encoding.UTF8.GetBytes(fileName);
            int nameLength = fileNameBytes.Length;

            // Serialize frequency table
            var freqTableBytes = SerializeFrequencyTable(frequencyTable);

            // Combine headers
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                writer.Write(nameLength); // Length of file name
                writer.Write(fileNameBytes); // File name
                writer.Write(freqTableBytes.Length); // Length of frequency table                
                writer.Write(freqTableBytes); // Frequency table
                writer.Write(compressedData); // Compressed data

                return WriteCompressedFile(ms.ToArray(), outputPath);
            }

            // Desired final layout:
            // | Int32 FileNameLength | FileNameBytes | Int32 FreqTableLength | FreqTableBytes | CompressedData |
        }

        public bool Decompress(string filePath)
        {
            byte[] compressedData = LoadInputData(filePath);

            using (var ms = new MemoryStream(compressedData))
            using (var reader = new BinaryReader(ms))
            {
                // Read original file name
                int nameLength = reader.ReadInt32();
                string originalFileName = Encoding.UTF8.GetString(reader.ReadBytes(nameLength));

                // Read frequency table
                int freqTableLength = reader.ReadInt32();
                byte[] freqTableBytes = reader.ReadBytes(freqTableLength);
                var freqTable = DeserializeFrequencyTable(freqTableBytes);

                // Rebuild Huffman tree
                var root = BuildTree(freqTable);

                // Remaining bytes = compressed data
                var remainingBytes = reader.ReadBytes((int)(ms.Length - ms.Position));
                var bitString = new StringBuilder();

                foreach (byte b in remainingBytes)
                {
                    for (int i = 7; i >= 0; i--)
                    {
                        bitString.Append((b & (1 << i)) != 0 ? '1' : '0');
                    }
                }

                // Decode bits using Huffman tree
                var output = new List<byte>();
                var current = root;

                foreach (char bit in bitString.ToString())
                {
                    current = bit == '0' ? current.Left : current.Right;

                    if (current.IsLeaf)
                    {
                        output.Add(current.Symbol.Value);
                        current = root;
                    }
                }

                // Build output path
                string outputDir = Path.GetDirectoryName(filePath) ?? "";
                string outputPath = Path.Combine(outputDir, originalFileName);

                // Write decompressed data to file
                return WriteDecompressedFile(outputPath, output.ToArray());
            }
        }
    }    
}            
