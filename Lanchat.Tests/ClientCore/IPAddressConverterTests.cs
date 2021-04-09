using System.Net;
using System.Text.Json;
using Lanchat.ClientCore;
using Lanchat.Tests.Mock.Models;
using NUnit.Framework;

namespace Lanchat.Tests.ClientCore
{
    public class IpAddressConverterTests
    {
        private static JsonSerializerOptions JsonSerializerOptions =>
            new()
            {
                Converters =
                {
                    new IpAddressConverter()
                }
            };

        private static readonly IpAddressModel Model = new()
        {
            IpAddress = IPAddress.Loopback
        };

        private const string SerializedModel = "{\"IpAddress\":\"127.0.0.1\"}";

        [Test]
        public void Serialize()
        {
            var json = JsonSerializer.Serialize(Model, JsonSerializerOptions);
            Assert.AreEqual(SerializedModel, json);
        }

        [Test]
        public void Deserialize()
        {
            var json = JsonSerializer.Deserialize<IpAddressModel>(SerializedModel, JsonSerializerOptions);
            Assert.AreEqual(Model.IpAddress, json?.IpAddress);
        }
    }
}