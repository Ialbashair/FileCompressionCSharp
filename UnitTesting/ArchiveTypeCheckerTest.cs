using DataObjects;
using LogicLayer;
using System;
using System.IO;
using Xunit;

namespace UnitTesting
{
    public class ArchiveTypeCheckerTest : IDisposable
    {
        private readonly string _testDir;
        private readonly ArchiveTypeChecker _checker;

        public ArchiveTypeCheckerTest()
        {
            _testDir = System.IO.Path.Combine(Path.GetTempPath(), "ArchiveTests_" + Guid.NewGuid());
            _checker = new ArchiveTypeChecker();

            Directory.CreateDirectory(_testDir);
        }
      
        public void Dispose()
        {
            Directory.Delete(_testDir, true);
        }

        private string CreateTempFile(byte[] bytes, string extension)
        {
            string filePath = Path.Combine(_testDir, "test" + extension);
            File.WriteAllBytes(filePath, bytes);
            return filePath;
        }

        [Fact]
        public void GetArchiveType_ReturnsVoid_ForInvalidPath()
        {
            var res = _checker.GetArchiveType("incorrectFilePath.file");
            Assert.Equal(ArchiveType.None, res);
        }

        [Fact]
        public void GetArchiveType_ReturnsZip_WhenPKSignature()
        {
            byte[] zipHeader = { 0x50, 0x4B, 0x03, 0x04 }; // start of Zip file
            string path = CreateTempFile(zipHeader, ".zip");
            var result = _checker.GetArchiveType(path);
            Assert.Equal(ArchiveType.Zip, result);
        }

        [Fact]
        public void GetArchiveType_ReturnsRar_WhenRarV4Signature()
        {
            byte[] rarHeader = { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x00 };
            string path = CreateTempFile(rarHeader, ".rar");
            var result = _checker.GetArchiveType(path);
            Assert.Equal(ArchiveType.Rar, result);
        }

        [Fact]
        public void GetArchiveType_ReturnsSevenZip_When7zSignature()
        {
            byte[] sevenZipHeader = { 0x37, 0x7A, 0xBC, 0xAF, 0x27, 0x1C };
            string path = CreateTempFile(sevenZipHeader, ".7z");
            var result = _checker.GetArchiveType(path);
            Assert.Equal(ArchiveType.SevenZip, result);
        }

        [Fact]
        public void GetArchiveType_ReturnsGZip_WhenGZipSignature()
        {
            byte[] gzipHeader = { 0x1F, 0x8B };
            string path = CreateTempFile(gzipHeader, ".gz");
            var result = _checker.GetArchiveType(path);
            Assert.Equal(ArchiveType.GZip, result);
        }

        [Fact]
        public void GetArchiveType_ReturnsBZip2_WhenBZhSignature()
        {
            byte[] bzip2Header = { 0x42, 0x5A, 0x68 };
            string path = CreateTempFile(bzip2Header, ".bz2");
            var result = _checker.GetArchiveType(path);
            Assert.Equal(ArchiveType.BZip2, result);
        }

        [Fact]
        public void GetArchiveType_ReturnsXZ_WhenXZSignature()
        {
            byte[] xzHeader = { 0xFD, 0x37, 0x7A, 0x58, 0x5A, 0x00 };
            string path = CreateTempFile(xzHeader, ".xz");
            var result = _checker.GetArchiveType(path);
            Assert.Equal(ArchiveType.XZ, result);
        }

        [Fact]
        public void GetArchiveType_ReturnsVoid_WhenUstarAtOffeset() 
        {
            byte[] tarHeader = new byte[512];
            tarHeader[0x101] = (byte)'u';
            tarHeader[0x102] = (byte)'s';
            tarHeader[0x103] = (byte)'t';
            tarHeader[0x104] = (byte)'a';
            tarHeader[0x105] = (byte)'r';

            string path = CreateTempFile(tarHeader, ".tar");
            var result = _checker.GetArchiveType(path);
            Assert.Equal(ArchiveType.Tar, result);
        }

        [Fact]
        public void GetArchiveType_ReturnsVoid_WhenNoKnownSignature()
        {
            byte[] randomBytes = { 0x12, 0x22, 0x32, 0x42 };
            string path = CreateTempFile(randomBytes, ".ran");
            var result = _checker.GetArchiveType(path);
            Assert.Equal(ArchiveType.None, result);
        }

    }
}
