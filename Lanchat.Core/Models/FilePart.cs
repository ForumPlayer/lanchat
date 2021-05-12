using System.ComponentModel.DataAnnotations;
using Lanchat.Core.Encryption;

namespace Lanchat.Core.Models
{
    internal class FilePart
    {
        [MaxLength(1398102)] [Encrypt] internal string Data { get; init; }
    }
}