// Copyright (c) @asmichi (https://github.com/asmichi). Licensed under the MIT License. See LICENCE in the project root for details.

using Asmichi.StreamJsonRpcAdapters;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;

namespace JsonRpcMessagePackFormatterExample
{
    internal sealed class MyCompositeResolver : IFormatterResolver
    {
        public static readonly IFormatterResolver Instance = new MyCompositeResolver();

        private static readonly IFormatterResolver[] Resolvers = new[]
        {
            // Add JsonRpcMessagePackResolver to your resolver.
            JsonRpcMessagePackResolver.Instance,

            // Use any combination of other resolvers that meets your requirement...
            StandardResolver.Instance,
        };

        public IMessagePackFormatter<T>? GetFormatter<T>() => FormatterCache<T>.Formatter;

        private static IMessagePackFormatter<T>? FormatterCacheImpl<T>()
        {
            foreach (var item in Resolvers)
            {
                var f = item.GetFormatter<T>();
                if (f != null)
                {
                    return f;
                }
            }

            return null;
        }

        private static class FormatterCache<T>
        {
            public static readonly IMessagePackFormatter<T>? Formatter = MyCompositeResolver.FormatterCacheImpl<T>();
        }
    }
}
