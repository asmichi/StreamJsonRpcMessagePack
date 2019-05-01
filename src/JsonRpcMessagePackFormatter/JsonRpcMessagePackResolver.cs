// Copyright (c) @asmichi (https://github.com/asmichi). Licensed under the MIT License. See LICENCE in the project root for details.

using System;
using System.Collections.Generic;
using MessagePack;
using MessagePack.Formatters;
using StreamJsonRpc.Protocol;

namespace Asmichi.StreamJsonRpcAdapters
{
    /// <summary>
    /// An <see cref="IFormatterResolver"/> that provides formatters of <see cref="JsonRpcMessage"/>.
    /// <see cref="Instance"/> must be added to any resolver supplied to <see cref="JsonRpcMessagePackFormatter"/>.
    /// </summary>
    public sealed class JsonRpcMessagePackResolver : IFormatterResolver
    {
        /// <summary>
        /// The immutable instance of <see cref="JsonRpcMessagePackResolver"/>.
        /// </summary>
        public static readonly IFormatterResolver Instance = new JsonRpcMessagePackResolver();

        private static readonly Dictionary<Type, object> FormatterMap = new Dictionary<Type, object>()
        {
            { typeof(JsonRpcMessage), JsonRpcMessageFormatter.Instance },
            { typeof(JsonRpcRequest), JsonRpcRequestFormatter.Instance },
            { typeof(JsonRpcResult), JsonRpcResultFormatter.Instance },
            { typeof(JsonRpcError), JsonRpcErrorFormatter.Instance },
            { typeof(JsonRpcError.ErrorDetail), JsonRpcErrorErrorDetailFormatter.Instance },
        };

        private JsonRpcMessagePackResolver()
        {
        }

        /// <inheritdoc/>
        public IMessagePackFormatter<T> GetFormatter<T>() => FormatterCache<T>.Formatter;

        private static object FormatterCacheImpl(Type t)
        {
            if (FormatterMap.TryGetValue(t, out var formatter))
            {
                return formatter;
            }

            return null;
        }

        private static class FormatterCache<T>
        {
            public static readonly IMessagePackFormatter<T> Formatter = (IMessagePackFormatter<T>)FormatterCacheImpl(typeof(T));
        }
    }
}
