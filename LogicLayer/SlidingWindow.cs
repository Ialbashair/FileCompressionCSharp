using DataAccessInterface;
using DataAccess;
using LogicLayerInterface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LogicLayer
{
    public class SlidingWindow : ISlidingWindow
    {
        private readonly FileReaderInterface _fileReader;
        private readonly FileWriterInterface _fileWriter;

        private const int DefaultWindowSize = 4096;  // 4KB
        private const int DefaultLookAheadBufferSize = 18; // bytes

        private struct Match
        {
            public int Offset { get; set; }
            public int Length { get; set; }
            public byte NextSymbol { get; set; }
        }

        public SlidingWindow()
        {
            _fileReader = new FileReader();
            _fileWriter = new FileWriter();
        }

        public SlidingWindow(FileReaderInterface fileReader, FileWriterInterface fileWriter)
        {
            _fileReader = fileReader;
            _fileWriter = fileWriter;
        }

        // --- Encoding and Decoding of Matches ---
        private List<Match> DecodeMatches(byte[] data)
        {
            var matches = new List<Match>();
            using (var ms = new MemoryStream(data))
            using (var br = new BinaryReader(ms))
            {
                while (ms.Position < ms.Length)
                {
                    matches.Add(new Match
                    {
                        Offset = br.ReadUInt16(),
                        Length = br.ReadByte(),
                        NextSymbol = br.ReadByte()
                    });
                }
            }
            return matches;
        }

        private byte[] EncodeMatches(List<Match> matches)
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                foreach (var match in matches)
                {
                    bw.Write((ushort)match.Offset);
                    bw.Write((byte)match.Length);
                    bw.Write(match.NextSymbol);
                }
                return ms.ToArray();
            }
        }

        // --- Core Compression and Decompression ---
        private List<Match> CompressData(byte[] input, int windowSize, int lookAheadSize)
        {
            var matches = new List<Match>();
            int pos = 0;

            // small hash table for sequences
            var dict = new Dictionary<int, List<int>>();
            const int MaxCandidates = 32;

            while (pos < input.Length)
            {
                int bestLength = 0;
                int bestOffset = 0;

                if (pos >= 3)
                {
                    // compute a small hash for the current 3-byte sequence
                    int hash = (input[pos - 3] << 16) | (input[pos - 2] << 8) | input[pos - 1];

                    if (!dict.ContainsKey(hash))
                        dict[hash] = new List<int>();

                    var candidates = dict[hash];
                    for (int i = candidates.Count - 1; i >= 0 && candidates.Count - i < MaxCandidates; i--)
                    {
                        int candidatePos = candidates[i];
                        int offset = pos - candidatePos;
                        if (offset > windowSize) break;

                        int matchLength = 0;
                        while (matchLength < lookAheadSize &&
                               pos + matchLength < input.Length &&
                               input[pos + matchLength] == input[candidatePos + matchLength])
                        {
                            matchLength++;
                        }

                        if (matchLength > bestLength)
                        {
                            bestLength = matchLength;
                            bestOffset = offset;
                            if (bestLength == lookAheadSize) break; // early stop
                        }
                    }
                }

                byte nextSymbol = (pos + bestLength < input.Length) ? input[pos + bestLength] : (byte)0;
                matches.Add(new Match { Offset = bestOffset, Length = bestLength, NextSymbol = nextSymbol });

                // update rolling hash index
                int start = Math.Max(0, pos - windowSize);
                for (int j = pos; j < pos + bestLength + 1 && j < input.Length; j++)
                {
                    if (j + 2 < input.Length)
                    {
                        int h = (input[j] << 16) | (input[j + 1] << 8) | input[j + 2];
                        if (!dict.ContainsKey(h)) dict[h] = new List<int>();
                        dict[h].Add(j);

                        // keep dictionary size bounded
                        if (dict[h].Count > MaxCandidates * 4)
                            dict[h].RemoveRange(0, dict[h].Count - MaxCandidates * 2);
                    }
                }

                pos += bestLength + 1;
            }

            return matches;
        }


        private byte[] DecompressData(List<Match> matches)
        {        
            int estimatedSize = matches.Count * 8;
            byte[] buffer = new byte[estimatedSize];
            int writePos = 0;

            foreach (var match in matches)
            {
                // Copy repeated sequence
                if (match.Offset > 0)
                {
                    int start = writePos - match.Offset;
                    for (int i = 0; i < match.Length; i++)
                    {
                        if (writePos >= buffer.Length)
                            Array.Resize(ref buffer, buffer.Length * 2);

                        buffer[writePos++] = buffer[start + i];
                    }
                }

                // Write next symbol
                if (match.NextSymbol != 0)
                {
                    if (writePos >= buffer.Length)
                        Array.Resize(ref buffer, buffer.Length * 2);

                    buffer[writePos++] = match.NextSymbol;
                }
            }

            // Trim to size
            if (writePos < buffer.Length)
            {
                Array.Resize(ref buffer, writePos);
            }

            return buffer;
        }


        // --- File Writing Helpers ---
        public bool WriteCompressedFile(byte[] compressedData, string outputPath)
        {
            try
            {
                string directory = Path.GetDirectoryName(outputPath);
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                _fileWriter.WriteCompressedFile(compressedData, outputPath);
                return true;
            }
            catch (Exception e)
            {
                throw new IOException("Failed to write compressed file.", e);
            }
        }

        public bool WriteDecompressedFile(byte[] decompressedData, string outputPath)
        {
            try
            {
                string directory = Path.GetDirectoryName(outputPath);
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                _fileWriter.WriteDecompressedFile(decompressedData, outputPath);
                return true;
            }
            catch (Exception e)
            {
                throw new IOException("Failed to write decompressed file.", e);
            }
        }

        // --- Header Serialization ---
        private byte[] BuildHeader(string originalPath, byte[] encodedData)
        {
            string fileName = Path.GetFileName(originalPath);
            byte[] nameBytes = Encoding.UTF8.GetBytes(fileName);
            int nameLength = nameBytes.Length;

            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write(nameLength);
                bw.Write(nameBytes);
                bw.Write(encodedData.Length);
                bw.Write(encodedData);

                return ms.ToArray();
            }
        }

        private (string FileName, byte[] EncodedData) ReadHeader(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            using (var br = new BinaryReader(ms))
            {
                int nameLength = br.ReadInt32();
                string fileName = Encoding.UTF8.GetString(br.ReadBytes(nameLength));

                int encodedLength = br.ReadInt32();
                byte[] encoded = br.ReadBytes(encodedLength);

                return (fileName, encoded);
            }
        }

        // --- Public Compress / Decompress ---
        public async Task<bool> Compress(string filePath, string outputPath, CancellationToken ct = default)
        {
            byte[] input = await _fileReader.ReadAllBytesAsync(filePath, ct);

            var matches = CompressData(input, DefaultWindowSize, DefaultLookAheadBufferSize);
            byte[] encoded = EncodeMatches(matches);

            byte[] finalBytes = BuildHeader(filePath, encoded);

            return WriteCompressedFile(finalBytes, outputPath);
        }

        public async Task<bool> Decompress(string filePath, CancellationToken ct = default)
        {
            byte[] compressed = await _fileReader.ReadAllBytesAsync(filePath, ct);

            var (originalFileName, encodedData) = ReadHeader(compressed);

            var matches = DecodeMatches(encodedData);
            byte[] decompressed = DecompressData(matches);

            string outputDir = Path.GetDirectoryName(filePath) ?? "";
            string outputPath = Path.Combine(outputDir, originalFileName);

            return WriteDecompressedFile(decompressed, outputPath);
        }
    }
}
