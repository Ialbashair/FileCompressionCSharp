using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LogicLayer;
using Xunit;

namespace UnitTesting
{
    public class HuffmanTest : IDisposable
    {
        private readonly Huffman _huffman;
        private readonly string _tempDir;

        public HuffmanTest()
        {
            _huffman = new Huffman();
            _tempDir = Path.Combine(Path.GetTempPath(), "HuffmanTests");
            Directory.CreateDirectory(_tempDir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }

        private string CreateTempFile(string content)
        {
            string path = Path.Combine(_tempDir, $"{Guid.NewGuid()}.txt");
            File.WriteAllText(path, content, Encoding.UTF8);
            return path;
        }

        [Fact]
        public async Task Compress_ShouldReturnTrue_ForValidInput()
        {
            string inputPath = CreateTempFile("aaaabbbbbccccddde");
            string outputPath = Path.Combine(_tempDir, "output1.huff");

            bool result = await _huffman.Compress(inputPath, outputPath, CancellationToken.None);

            Assert.True(result);
            Assert.True(File.Exists(outputPath));
            Assert.True(new FileInfo(outputPath).Length > 0);
        }

        [Fact]
        public async Task Compress_ShouldCreateSmallerOutput_ForRepetitiveInput()
        {
            string content = new string('a', 1000);
            string inputPath = CreateTempFile(content);
            string outputPath = Path.Combine(_tempDir, "output2.huff");

            await _huffman.Compress(inputPath, outputPath);

            var inputSize = new FileInfo(inputPath).Length;
            var outputSize = new FileInfo(outputPath).Length;

            Assert.True(outputSize < inputSize, $"Expected compressed < input. Input={inputSize}, Output={outputSize}");
        }

        [Fact]
        public async Task Compress_ShouldHandleSingleCharacterFile()
        {
            string inputPath = CreateTempFile("a");
            string outputPath = Path.Combine(_tempDir, "output3.huff");

            bool result = await _huffman.Compress(inputPath, outputPath);

            Assert.True(result);
            Assert.True(File.Exists(outputPath));
        }

        [Fact]
        public async Task Compress_ShouldReturnFalse_ForEmptyFile()
        {
            string inputPath = CreateTempFile(string.Empty);
            string outputPath = Path.Combine(_tempDir, "output4.huff");

            bool result = await _huffman.Compress(inputPath, outputPath);

            // Adjust according to your implementation. If your Compress returns true even when input is empty,
            // change this assert accordingly.
            Assert.False(result);
        }

        [Fact]
        public async Task Compress_ShouldBeDeterministic_ForSameInput()
        {
            string content = "abcabcabcabc";
            string inputPath1 = CreateTempFile(content);
            string inputPath2 = CreateTempFile(content);

            string output1 = Path.Combine(_tempDir, "outA.huff");
            string output2 = Path.Combine(_tempDir, "outB.huff");

            await _huffman.Compress(inputPath1, output1);
            await _huffman.Compress(inputPath2, output2);

            // Use synchronous File.ReadAllBytes for compatibility
            byte[] file1 = File.ReadAllBytes(output1);
            byte[] file2 = File.ReadAllBytes(output2);

            Assert.Equal(file1, file2);
        }

        [Fact]
        public async Task Compress_ShouldRespectCancellationToken()
        {
            string inputPath = CreateTempFile(new string('x', 500000)); // large file
            string outputPath = Path.Combine(_tempDir, "cancelTest.huff");

            using var cts = new CancellationTokenSource();
            cts.CancelAfter(10); // cancel almost immediately

            await Assert.ThrowsAsync<TaskCanceledException>(async () =>
            {
                await _huffman.Compress(inputPath, outputPath, cts.Token);
            });
        }
    }
}
