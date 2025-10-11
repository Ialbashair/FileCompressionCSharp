using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicLayerInterface
{
    public interface IHuffman
    {
        byte[] Compress(byte[] inputData);
        byte[] Decompress(byte[] CompressedData);
    }
}
