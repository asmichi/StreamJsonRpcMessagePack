// Copyright (c) @asmichi (on github). Licensed under the MIT License. See LICENCE in the project root for details.

using System;
using System.Buffers;
using System.Collections.Generic;
using MessagePack;
using Nerdbank.Streams;
using StreamJsonRpc;
using StreamJsonRpc.Protocol;

namespace Asmichi.StreamJsonRpcAdapters
{
    /// <summary>
    /// An <see cref="IJsonRpcMessageFormatter"/> that offers <see cref="JsonRpcMessage"/> serialization using MessagePack for C#.
    /// </summary>
    public sealed class JsonRpcMessagePackFormatter : IJsonRpcMessageFormatter
    {
        private readonly IFormatterResolver _resolver;
        private readonly MessagePackSerializerKind _serializerKind;
        private readonly bool _allowParameterObject;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonRpcMessagePackFormatter"/> class.
        /// </summary>
        /// <param name="resolver">An <see cref="IFormatterResolver"/> instance that will be used for (de)serialization.</param>
        public JsonRpcMessagePackFormatter(IFormatterResolver resolver)
            : this(resolver, MessagePackSerializerKind.LZ4MessagePackSerializer, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonRpcMessagePackFormatter"/> class.
        /// </summary>
        /// <param name="options">A <see cref="JsonRpcMessagePackFormatterOptions"/>.</param>
        public JsonRpcMessagePackFormatter(JsonRpcMessagePackFormatterOptions options)
            : this(options.Resolver, options.SerializerKind, options.AllowParameterObject)
        {
        }

        private JsonRpcMessagePackFormatter(
            IFormatterResolver resolver,
            MessagePackSerializerKind serializerKind,
            bool allowParameterObject)
        {
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            _serializerKind = serializerKind;
            _allowParameterObject = allowParameterObject;

            if (resolver.GetFormatter<JsonRpcMessage>() == null)
            {
                // NOTE: For maximal performance, dynamically composing `resolver` and StreamJsonRpcResolver.Instance is not an option
                //       because that would disable the FormatterCache<T> technique.
                throw new ArgumentException("`resolver` must have StreamJsonRpcResolver as its subresolver.", nameof(resolver));
            }

            switch (serializerKind)
            {
                case MessagePackSerializerKind.LZ4MessagePackSerializer:
                case MessagePackSerializerKind.MessagePackSerializer:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(serializerKind));
            }
        }

        /// <inheritdoc/>
        public object GetJsonText(JsonRpcMessage message) => MessagePackSerializer.ToJson(ToValidRequest(message), _resolver);

        /// <inheritdoc/>
        public JsonRpcMessage Deserialize(ReadOnlySequence<byte> contentBuffer)
        {
            switch (_serializerKind)
            {
                case MessagePackSerializerKind.LZ4MessagePackSerializer:
                    return LZ4MessagePackSerializer.Deserialize<JsonRpcMessage>(contentBuffer.AsStream(), _resolver);
                case MessagePackSerializerKind.MessagePackSerializer:
                    return MessagePackSerializer.Deserialize<JsonRpcMessage>(contentBuffer.AsStream(), _resolver);
                default:
                    throw new InvalidOperationException("internal error");
            }
        }

        /// <inheritdoc/>
        public void Serialize(IBufferWriter<byte> contentBuffer, JsonRpcMessage message)
        {
            var correctedMessage = ToValidRequest(message);
            switch (_serializerKind)
            {
                case MessagePackSerializerKind.LZ4MessagePackSerializer:
                    LZ4MessagePackSerializer.Serialize(contentBuffer.AsStream(), correctedMessage, _resolver);
                    return;
                case MessagePackSerializerKind.MessagePackSerializer:
                    MessagePackSerializer.Serialize(contentBuffer.AsStream(), correctedMessage, _resolver);
                    return;
                default:
                    throw new InvalidOperationException("internal error");
            }
        }

        // If message is a JsonRpcRequest and its Arguments is a paramter object,
        // converts the parameter object to a valid JSON-RPC parameter structure.
        private JsonRpcMessage ToValidRequest(JsonRpcMessage message)
        {
            if (!(message is JsonRpcRequest request))
            {
                return message;
            }

            if (IsValidParameterStructure(request.Arguments))
            {
                return message;
            }
            else
            {
                if (!_allowParameterObject)
                {
                    throw new NotSupportedException("This JsonRpcMessagePackFormatter is not configured to allow parameter objects.");
                }

                // NOTE: How a member of a parameter object is matched against a parameter of an RPC method
                //       is not a concern of serializers, but the concern of the StreamJsonRpc interface,
                //       introduced by the `InvokeWithParameterObjectAsync` contract.
                //
                //       Json.NET happens to implement this matching algorithm since during serialization
                //       every object is converted into a map (a JSON object) and loses its type information
                //       and thus JToken.ToObject converts this map from JToken to the actual type on demand.
                //
                // Shallowly convert the parameter object to a Dictionary<string, object> before serialization.
                return new JsonRpcRequest()
                {
                    Id = request.Id,
                    Method = request.Method,
                    Version = request.Version,
                    NamedArguments = ToParameterStructure(request.Arguments),
                };
            }
        }

        // Returns if value is a valid JSON-RPC parameter structure: null, an Array or an Object.
        private static bool IsValidParameterStructure(object value) =>
               value is null
            || value is IReadOnlyList<object>
            || value is IReadOnlyDictionary<string, object>;

        // Converts the parameter object to a valid JSON-RPC parameter structure: null, an Array or an Object.
        private IReadOnlyDictionary<string, object> ToParameterStructure(object parameterObject)
        {
            throw new NotImplementedException();
        }
    }
}
