using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicLayerInterface
{
    public interface IHuffman
    {
        bool Compress(string inputPath, string outputPath);
        byte[] Decompress(byte[] CompressedData);
    }
}
