using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LogicLayerInterface
{
    public interface IHuffman
    {
        Task<bool> Compress(string inputPath, string outputPath, CancellationToken ct);
        Task<bool> Decompress(string inputPath, CancellationToken ct);
    }
}
