/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LogicLayer;
using Xunit;

namespace UnitTesting
{
    public class HuffmanTest
    {
        private readonly Huffman _huffman;

        public HuffmanTest()
        {
            _huffman = new Huffman();
        }


        [Fact]
        public void Compress_ShouldReturnNonEmptyOutput_ForSimpleInput()
        {
            byte[] input = Encoding.UTF8.GetBytes("aaaabbbbbccccddde");           

            byte[] output = _huffman.Compress(input, "" );

            Assert.NotNull(output);
            Assert.NotEmpty(output);
        }

        [Fact]
        public void Compress_ShouldReturnSmallerOutput_ForRepetitiveInput()
        {
            byte[] input = Encoding.UTF8.GetBytes("aaaaaaaaaaaaaaa"); // 15 Length.
            byte[] output = _huffman.Compress(input);

            Assert.NotNull(output);
            Assert.True(output.Length < input.Length, $"Length of compressed data is expected to be < input data length. " +
                $"Input: {input.Length}, Output: {output.Length} ");
        }

        [Fact]
        public void Compress_ShouldHandleSingleCharacterInput()
        {
            byte[] input = Encoding.UTF8.GetBytes("a");
            byte[] output = _huffman.Compress(input);

            Assert.NotNull(output);
            Assert.NotEmpty(output);    
        }

        [Fact]
        public void Compress_ShouldHandleEmptyInput()
        {
            byte[] input = Array.Empty<byte>();
            byte[] output = _huffman.Compress(input);

            Assert.NotNull(output);
            Assert.Empty(output); // Expecting empty output for empty input
        }

        [Fact]
        public void Compress_ShouldProduceDeterministicOutput_ForSameInput()
        {           
            byte[] input = Encoding.UTF8.GetBytes("abcabcabcabc");           
            byte[] compressed1 = _huffman.Compress(input);
            byte[] compressed2 = _huffman.Compress(input);
           
            Assert.Equal(compressed1, compressed2);
        }
    }
}
*/