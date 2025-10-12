using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogicLayerInterface;

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
        private byte[] EncodeDate(byte[] input, Dictionary<byte, string> codes) 
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

        public byte[] Compress(byte[] inputData)
        {
            // Handle empty input
            if (inputData == null || inputData.Length == 0) 
            {
                return Array.Empty<byte>();
            }

            var frequencyTable = BuildFrequencyTable(inputData);
            var huffmanTree = BuildTree(frequencyTable);
            var codes = BuildCodeTable(huffmanTree);
            var compressedData = EncodeDate(inputData, codes);

            return compressedData;
        }

        public byte[] Decompress(byte[] CompressedData)
        {
            throw new NotImplementedException();
        }

        
    }
}
