using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DataAccessInterface;


namespace DataAccess
{
    public class FileReader : FileReaderInterface
    {
        // 
        public byte[] FileToByteArray(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
            }

            if (!File.Exists(filePath)) 
            {
                throw new FileNotFoundException("File not found.", filePath);
            }

            try
            {
                return File.ReadAllBytes(filePath);
            }
            catch (Exception ex)
            {                
                throw new IOException("An error occurred while reading the file.", ex);
            }   
        }
    }
}
