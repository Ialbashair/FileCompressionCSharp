using DataAccess;
using DataAccessInterface;
using LogicLayerInterface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        FileWriterInterface _fileWriter;

        public Huffman()
        {
            _fileReader = new FileReader();
            _fileWriter = new FileWriter();
        }

        public Huffman(FileReaderInterface fileReader, FileWriterInterface fileWriter)
        {
            _fileReader = fileReader;
            _fileWriter = fileWriter;
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
            using (var ms = new MemoryStream())
            {
                int bitPos = 0;
                byte crntByte = 0;

                foreach (byte b in input)
                {
                    string code = codes[b];
                    foreach (char bit in code)
                    {
                        if (bit == '1')
                        {
                            crntByte |= (byte)(1 << (7 - bitPos));
                        }
                        bitPos++;

                        if (bitPos == 8)
                        {
                            ms.WriteByte(crntByte);
                            crntByte = 0;
                            bitPos = 0;
                        }
                    }
                }

                // Write any remaining bits
                if (bitPos > 0)
                {
                    ms.WriteByte(crntByte);
                }

                return ms.ToArray();
            }
        }

        // Write compressed data to a file with error handling
        public bool WriteCompressedFile(byte[] compressedData, string outputPath)
        {
            bool success = false;

            try
            {
                _fileWriter.WriteCompressedFile(compressedData, outputPath);
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

        // Load input data from file with error handling
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

        // Serialize and deserialize frequency table for storage in compressed file
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

        // Serialize frequency table to byte array
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
        
        // Write decompressed data to a file with error handling
        public bool WriteDecompressedFile(string outputPath, byte[] decompressedData)
        {
            bool success = false;
            try
            {
                _fileWriter.WriteDecompressedFile(decompressedData, outputPath);
                success = true;
            }
            catch (Exception e)
            {

                throw new IOException("Failed to write to file: ", e);
            }
            return success;
        }

        // Main compress method
        public async Task<bool> Compress(string filePath, string outputPath, CancellationToken ct = default)
        {
            // Run heavy work on background thread to avoid blocking STA / UI thread.
            return await Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();

                byte[] inputData = LoadInputData(filePath); // keep using your FileReader if needed
                var frequencyTable = BuildFrequencyTable(inputData);
                var huffmanTree = BuildTree(frequencyTable);
                var codes = BuildCodeTable(huffmanTree);
                var compressedData = EncodeData(inputData, codes);

                // filename and meta
                string fileName = Path.GetFileName(filePath);
                byte[] fileNameBytes = Encoding.UTF8.GetBytes(fileName);
                int nameLength = fileNameBytes.Length;
                var freqTableBytes = SerializeFrequencyTable(frequencyTable);

                using (var ms = new MemoryStream())
                using (var writer = new BinaryWriter(ms))
                {
                    writer.Write(nameLength);
                    writer.Write(fileNameBytes);
                    writer.Write(freqTableBytes.Length);
                    writer.Write(freqTableBytes);
                    writer.Write(compressedData);

                    // Use synchronous file write here because we're already on a background thread,
                    // but you can replace with WriteAllBytesAsync if you want true async IO.
                    return WriteCompressedFile(ms.ToArray(), outputPath);
                }
            }, ct).ConfigureAwait(false);
            // Desired final layout:
            // | Int32 FileNameLength | FileNameBytes | Int32 FreqTableLength | FreqTableBytes | CompressedData |
        }

        // Main decompress method
        public async Task<bool> Decompress(string filePath, CancellationToken ct = default)
        {
            // Load compressed bytes on background thread
            var compressedData = await Task.Run(() => LoadInputData(filePath), ct).ConfigureAwait(false);

            using (var ms = new MemoryStream(compressedData))
            using (var reader = new BinaryReader(ms))
            {
                int nameLength = reader.ReadInt32();
                string originalFileName = Encoding.UTF8.GetString(reader.ReadBytes(nameLength));

                int freqTableLength = reader.ReadInt32();
                var freqTableBytes = reader.ReadBytes(freqTableLength);
                var freqTable = DeserializeFrequencyTable(freqTableBytes);

                var root = BuildTree(freqTable);

                // Remaining bytes = compressed payload
                var remainingBytes = reader.ReadBytes((int)(ms.Length - ms.Position));

                // Decode on the fly
                var output = new List<byte>();
                var current = root;

                for (int byteIndex = 0; byteIndex < remainingBytes.Length; byteIndex++)
                {
                    ct.ThrowIfCancellationRequested();

                    byte b = remainingBytes[byteIndex];
                    for (int bit = 7; bit >= 0; bit--)
                    {
                        bool bitIsOne = (b & (1 << bit)) != 0;
                        current = bitIsOne ? current.Right : current.Left;

                        if (current == null)
                        {
                            // Defensive: malformed data
                            throw new InvalidDataException("Huffman decoding failed: encountered null node.");
                        }

                        if (current.IsLeaf)
                        {
                            if (!current.Symbol.HasValue)
                                throw new InvalidDataException("Huffman decoding failed: leaf without symbol.");

                            output.Add(current.Symbol.Value);
                            current = root;
                        }
                    }
                }

                // Write decompressed file off the UI thread
                string outputDir = Path.GetDirectoryName(filePath) ?? "";
                string outputPath = Path.Combine(outputDir, originalFileName);

                // Use async write to avoid blocking threadpool thread for big files
                return await Task.Run(() => WriteDecompressedFile(outputPath, output.ToArray()), ct).ConfigureAwait(false);
            }
        }
    }    
}            
