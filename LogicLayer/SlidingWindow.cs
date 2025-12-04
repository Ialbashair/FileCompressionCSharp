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

        private const int DefaultWindowSize = 4096;  // 12-bit offset (0..4095)
        private const int DefaultLookAheadBufferSize = 18; // 4-bit length encoded as (len-3) => max 18
        private const int MinMatchLength = 3;
        private const int MaxMatchLength = MinMatchLength + 15; // 3 + 15 = 18

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

        public async Task<bool> Compress(string filePath, string outputPath, CancellationToken ct = default)
        {
            byte[] input = await _fileReader.ReadAllBytesAsync(filePath, ct);

            byte[] encoded = CompressDataLzss(input, DefaultWindowSize, DefaultLookAheadBufferSize, ct);

            byte[] finalBytes = BuildHeader(filePath, encoded);

            return WriteCompressedFile(finalBytes, outputPath);
        }

        public async Task<bool> Decompress(string filePath, CancellationToken ct = default)
        {
            byte[] compressed = await _fileReader.ReadAllBytesAsync(filePath, ct);

            var (originalFileName, encodedData) = ReadHeader(compressed);

            byte[] decompressed = DecompressDataLzss(encodedData, ct);

            string outputDir = Path.GetDirectoryName(filePath) ?? "";
            string outputPath = Path.Combine(outputDir, originalFileName);

            return WriteDecompressedFile(decompressed, outputPath);
        }

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
        private byte[] CompressDataLzss(byte[] input, int windowSize, int lookAheadSize, CancellationToken ct)
        {
            if (lookAheadSize > MaxMatchLength) lookAheadSize = MaxMatchLength;
            if (lookAheadSize < MinMatchLength) lookAheadSize = MinMatchLength;

            using (var outMs = new MemoryStream())
            using (var bw = new BinaryWriter(outMs))
            {
                int pos = 0;
                int inputLen = input.Length;

                // We'll build groups of up to 8 tokens with a flag byte per group.
                while (pos < inputLen)
                {
                    if (ct.IsCancellationRequested) throw new OperationCanceledException(ct);

                    byte flagByte = 0;
                    var tokenBuffer = new MemoryStream(); // store tokens for this group temporarily
                    int tokensInGroup = 0;

                    for (int t = 0; t < 8 && pos < inputLen; t++)
                    {
                        if (ct.IsCancellationRequested) throw new OperationCanceledException(ct);

                        // Find longest match within window
                        int bestLength = 0;
                        int bestOffset = 0;

                        int maxOffset = Math.Min(pos, windowSize);                   
                        int searchStart = pos - maxOffset;

                        for (int candidate = pos - 1; candidate >= searchStart; candidate--)
                        {
                            int maxMatchHere = Math.Min(lookAheadSize, inputLen - pos);
                            int matchLen = 0;

                            // quick check for first byte equality to avoid inner loop costs
                            if (input[candidate] != input[pos]) continue;

                            while (matchLen < maxMatchHere && input[candidate + matchLen] == input[pos + matchLen])
                                matchLen++;

                            if (matchLen >= MinMatchLength && matchLen > bestLength)
                            {
                                bestLength = matchLen;
                                bestOffset = pos - candidate;

                                if (bestLength == lookAheadSize)
                                    break; // can't get better
                            }
                        }

                        if (bestLength >= MinMatchLength)
                        {
                            // Emit match token -> flag bit = 0
                            // Pack into 2 bytes: 12-bit offset, 4-bit (length - MinMatchLength)
                            int lengthField = bestLength - MinMatchLength;
                            if (lengthField > 15) lengthField = 15; // clamp (shouldn't happen due to lookAheadSize cap)

                            int offsetField = bestOffset & 0x0FFF;
                            ushort packed = (ushort)((offsetField << 4) | (lengthField & 0x0F));

                            tokenBuffer.WriteByte((byte)(packed >> 8));
                            tokenBuffer.WriteByte((byte)(packed & 0xFF));

                            pos += bestLength;
                        }
                        else
                        {
                            // Emit literal -> flag bit = 1
                            flagByte |= (byte)(1 << t); // set the bit for this token index
                            tokenBuffer.WriteByte(input[pos]);
                            pos++;
                        }

                        tokensInGroup++;
                    }

                    // Write flag byte then the token bytes for this group
                    bw.Write(flagByte);
                    tokenBuffer.Position = 0;
                    tokenBuffer.CopyTo(outMs);
                }

                return outMs.ToArray();
            }
        }
        private byte[] DecompressDataLzss(byte[] encoded, CancellationToken ct)
        {
            using (var ms = new MemoryStream(encoded))
            using (var br = new BinaryReader(ms))
            using (var outMs = new MemoryStream())
            {
                while (ms.Position < ms.Length)
                {
                    if (ct.IsCancellationRequested) throw new OperationCanceledException(ct);

                    byte flag = br.ReadByte();
                    for (int t = 0; t < 8 && ms.Position < ms.Length; t++)
                    {
                        bool isLiteral = ((flag >> t) & 1) == 1;

                        if (isLiteral)
                        {
                            // literal -> single byte
                            byte b = br.ReadByte();
                            outMs.WriteByte(b);
                        }
                        else
                        {
                            // match -> 2 bytes
                            if (ms.Position + 1 >= ms.Length)
                                throw new InvalidDataException("Truncated match token in stream.");

                            byte hi = br.ReadByte();
                            byte lo = br.ReadByte();
                            ushort packed = (ushort)((hi << 8) | lo);

                            int offset = (packed >> 4) & 0x0FFF;
                            int lengthField = packed & 0x0F;
                            int length = lengthField + MinMatchLength;

                            long start = outMs.Length - offset;
                            if (start < 0) throw new InvalidDataException("Invalid offset in compressed stream.");

                            // Copy bytes from previously output buffer
                            for (int i = 0; i < length; i++)
                            {
                                if (ct.IsCancellationRequested) throw new OperationCanceledException(ct);

                                outMs.Position = start + i;
                                int read = outMs.ReadByte();
                                if (read == -1) throw new InvalidDataException("Unexpected read error from output buffer.");

                                outMs.Position = outMs.Length; // move back to end to append
                                outMs.WriteByte((byte)read);
                            }
                        }
                    }
                }

                return outMs.ToArray();
            }
        }
    }
}
