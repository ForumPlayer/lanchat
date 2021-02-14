using Lanchat.Core;
using Lanchat.Core.Encryption;
using Lanchat.Core.NetworkIO;
using Lanchat.Tests.Mock;
using NUnit.Framework;

namespace Lanchat.Tests
{
    public class MessagesReceiveTests
    {
        private NetworkOutput networkOutput;
        private NetworkElement networkElement;
        private Messaging messaging;
        private Encryptor encryptor;

        [SetUp]
        public void Setup()
        {
            CoreConfig.MaxMessageLenght = 5;
            encryptor = new Encryptor();
            encryptor.ImportPublicKey(encryptor.ExportPublicKey());
            encryptor.ImportAesKey(encryptor.ExportAesKey());
            networkElement = new NetworkElement();
            networkOutput = new NetworkOutput(networkElement, new NodeState());
            messaging = new Messaging(networkOutput, encryptor);
        }

        [Test]
        public void ValidMessageReceive()
        {
            var testMessage = "test";
            var eventResult = string.Empty;

            messaging.MessageReceived += (_, s) => { eventResult = s; };

            messaging.HandleMessage(encryptor.Encrypt(testMessage));
            Assert.AreEqual(testMessage, eventResult);
        }

        [Test]
        public void ValidPrivateMessageReceive()
        {
            var testMessage = "test";
            var eventResult = string.Empty;

            messaging.PrivateMessageReceived += (_, s) => { eventResult = s; };

            messaging.HandlePrivateMessage(encryptor.Encrypt(testMessage));
            Assert.AreEqual(testMessage, eventResult);
        }

        [Test]
        public void TooLongMessageReceive()
        {
            var testMessage = "1234567890";
            var eventResult = string.Empty;

            messaging.MessageReceived += (_, s) => { eventResult = s; };

            messaging.HandleMessage(encryptor.Encrypt(testMessage));
            Assert.AreEqual("12345...", eventResult);
        }

        [Test]
        public void TooLongPrivateMessageReceive()
        {
            var testMessage = "1234567890";
            var eventResult = string.Empty;

            messaging.PrivateMessageReceived += (_, s) => { eventResult = s; };
            messaging.HandlePrivateMessage(encryptor.Encrypt(testMessage));
            Assert.AreEqual("12345...", eventResult);
        }
    }
}