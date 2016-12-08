using System;
using System.Security.Cryptography;
using System.Text;

namespace RiskTracker
{
    public class Helper
    {
        public static string GetHash(string input)
        {
            HashAlgorithm hashAlgo = new SHA256CryptoServiceProvider();

            byte[] byteValue = Encoding.UTF8.GetBytes(input);

            byte[] byteHash = hashAlgo.ComputeHash(byteValue);

            return Convert.ToBase64String(byteHash);
        } // GetHash
    }
}