using System;
using Lanchat.Core.Encryption.Models;

namespace Lanchat.Core.Encryption
{
    internal interface IPublicKeyEncryption : IDisposable
    {
        PublicKey ExportKey();
        void ImportKey(PublicKey publicKey);
        byte[] Encrypt(byte[] bytes);
        byte[] Decrypt(byte[] encryptedBytes);
    }
}