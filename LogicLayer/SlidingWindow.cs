using DataAccessInterface;
using DataAccess;
using LogicLayerInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.CompilerServices;


namespace LogicLayer
{
    public class SlidingWindow : ISlidingWindow
    {
        private readonly FileReaderInterface _fileReader;
        private readonly FileWriterInterface _fileWriter;

        private const int DefaultWindowSize = 4096; // 4KB
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

        private List<Match> ComrpessData(byte[] input, int windowSize, int lookAheadSize)
        {
            var matches = new List<Match>();
            int pos = 0;

            while (pos < input.Length)
            {
                int bestLength = 0;
                int bestOffset = 0;

                int windowStart = Math.Max(0, pos - windowSize);
                int windowLength = pos - windowStart;

                for (int offset = 1; offset <= windowLength; offset++)
                {
                    int matchLength = 0;

                    while ((matchLength < lookAheadSize) && (pos + matchLength < input.Length) && (input[pos + matchLength] == input[pos - offset + matchLength]))
                    {
                        matchLength++;
                    }

                    if (matchLength > bestLength)
                    {
                        bestLength = matchLength;
                        bestOffset = offset;
                    }
                }

                byte nextSymbol = (pos + bestLength < input.Length) ? input[pos + bestLength] : (byte)0; // record next symbol

                matches.Add(new Match
                {
                    Offset = bestOffset,
                    Length = bestLength,
                    NextSymbol = nextSymbol
                });
            }

            return matches;
        }

        private byte[] DecompressData(List<Match> matches)
        {
            var output = new List<byte>();

            foreach (var match in matches)
            {
                if (match.Offset > 0)
                {
                    int startPoint = output.Count - match.Offset;
                    for (int i = 0; i < match.Length; i++)
                    {
                        output.Add(output[startPoint + i]);
                    }
                }


                if (match.NextSymbol != 0)
                {
                    output.Add(match.NextSymbol);
                }
            }

            return output.ToArray();

        }

        public bool WriteCompressedFile(byte[] compressedData, string outputPath) 
        {
            try
            {
                // Check if directory exists, if not create it
                string directory = Path.GetDirectoryName(outputPath);
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory); // Create.
                }

                File.WriteAllBytes(outputPath, compressedData);
                return true;
            }
            catch (Exception e)
            {

                throw new IOException("Failed to write compressed file: ", e); // shouldn't ever be called 
            }
        }

        public Task<bool> Compress(string filePath, string outputPath, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeCompress(string filePath, string outputPath, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
