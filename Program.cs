using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CrossLanguageRSA
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Cross-Language RSA Encryption Demo ===");
            Console.WriteLine("JavaScript (HTML) Encryption → C# Decryption\n");

            // Generate RSA Key Pair
            Console.WriteLine("Generating RSA Key Pair...\n");
            var (publicKeyXml, privateKeyXml) = GenerateRSAKeyPair();

            // Display Public Key for JavaScript
            Console.WriteLine("══════════════════════════════════════════════");
            Console.WriteLine("📋 COPY THIS PUBLIC KEY TO YOUR HTML PAGE:");
            Console.WriteLine("══════════════════════════════════════════════");
            Console.WriteLine(publicKeyXml);
            Console.WriteLine("══════════════════════════════════════════════\n");

            // Save keys to files
            System.IO.File.WriteAllText("public_key.xml", publicKeyXml);
            System.IO.File.WriteAllText("private_key.xml", privateKeyXml);
            Console.WriteLine("✓ Keys saved to: public_key.xml and private_key.xml\n");

            // Wait for encrypted input from HTML page
            Console.WriteLine("══════════════════════════════════════════════");
            Console.WriteLine("Now use the HTML page to encrypt your JSON data");
            Console.WriteLine("══════════════════════════════════════════════");
            Console.WriteLine("\nPaste the encrypted Base64 string from HTML here:");
            Console.WriteLine("(Press Enter after pasting)\n");

            string encryptedBase64 = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(encryptedBase64))
            {
                try
                {
                    // Decrypt the data
                    Console.WriteLine("\n🔓 Decrypting...");
                    string decryptedJson = DecryptData(encryptedBase64.Trim(), privateKeyXml);

                    Console.WriteLine("\n══════════════════════════════════════════════");
                    Console.WriteLine("✅ DECRYPTION SUCCESSFUL!");
                    Console.WriteLine("══════════════════════════════════════════════");
                    //Console.WriteLine("\n📄 Decrypted JSON Data:");
                    //Console.WriteLine(decryptedJson);

                    // Pretty print if valid JSON
                    try
                    {
                        var jsonDoc = JsonDocument.Parse(decryptedJson);
                        string prettyJson = JsonSerializer.Serialize(jsonDoc, new JsonSerializerOptions { WriteIndented = true });
                        Console.WriteLine("\n📝 Formatted JSON:");
                        Console.WriteLine(prettyJson);
                    }
                    catch { }

                    Console.WriteLine("\n══════════════════════════════════════════════");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n❌ Decryption failed: {ex.Message}");
                }
            }

            Console.WriteLine("\n\nPress any key to exit...");
            Console.ReadKey();
        }

        /// <summary>
        /// Generate RSA Key Pair (2048-bit)
        /// </summary>
        public static (string publicKey, string privateKey) GenerateRSAKeyPair()
        {
            using (RSA rsa = RSA.Create(2048))
            {
                string publicKey = rsa.ToXmlString(false);   // Public key only
                string privateKey = rsa.ToXmlString(true);    // Private key (includes public)
                return (publicKey, privateKey);
            }
        }

        /// <summary>
        /// Decrypt Base64 encrypted data using RSA private key
        /// Compatible with JavaScript Web Crypto API (RSA-OAEP-SHA256)
        /// </summary>
        public static string DecryptData(string encryptedBase64, string privateKeyXml)
        {
            using (RSA rsa = RSA.Create())
            {
                // Import private key
                rsa.FromXmlString(privateKeyXml);

                // Convert Base64 to bytes
                byte[] encryptedBytes = Convert.FromBase64String(encryptedBase64);

                // Decrypt using RSA-OAEP with SHA-256 (matches JavaScript)
                byte[] decryptedBytes = rsa.Decrypt(encryptedBytes, RSAEncryptionPadding.OaepSHA256);

                // Convert to string
                return Encoding.UTF8.GetString(decryptedBytes);
            }
        }
    }
}