using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lanchat.Core.Encryption;
using Lanchat.Core.Extensions;
using Lanchat.Core.Json;
using Lanchat.Core.Network;

namespace Lanchat.Core.Api
{
    /// <inheritdoc />
    public class Resolver : IResolver
    {
        private readonly IModelEncryption encryption;
        private readonly List<IApiHandler> handlers = new();
        private readonly JsonUtils jsonUtils;
        private readonly INodeInternal node;
        private readonly Validation validation;

        public Resolver(INodeInternal node, IModelEncryption encryption, IEnumerable<IApiHandler> handlers)
        {
            this.node = node;
            this.encryption = encryption;
            jsonUtils = new JsonUtils();
            validation = new Validation(node);
            handlers.ForEach(RegisterHandler);
        }

        /// <inheritdoc />
        public void RegisterHandler(IApiHandler apiHandler)
        {
            handlers.Add(apiHandler);
            jsonUtils.KnownModels.Add(apiHandler.HandledType);
        }

        /// <inheritdoc />
        public void CallHandler(string item)
        {
            var data = jsonUtils.Deserialize(item);
            var handler = GetHandler(data.GetType());
            if (!validation.CheckPreconditions(handler, data))
            {
                return;
            }

            encryption.DecryptObject(data);
            Trace.WriteLine($"Node {node.Id} received {handler.HandledType.Name}");
            handler.Handle(data);
        }

        private IApiHandler GetHandler(Type jsonType)
        {
            return handlers.First(x => x.HandledType == jsonType);
        }
    }
}