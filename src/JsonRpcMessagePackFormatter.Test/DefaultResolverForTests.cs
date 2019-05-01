// Copyright (c) @asmichi (https://github.com/asmichi). Licensed under the MIT License. See LICENCE in the project root for details.

using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;

namespace Asmichi.StreamJsonRpcAdapters
{
    internal sealed class DefaultResolverForTests : IFormatterResolver
    {
        public static readonly IFormatterResolver Instance = new DefaultResolverForTests();

        private static readonly IFormatterResolver[] Resolvers = new[]
        {
            JsonRpcMessagePackResolver.Instance,
            StandardResolverAllowPrivate.Instance,  // allow internal types for tests
        };

        public IMessagePackFormatter<T> GetFormatter<T>() => FormatterCache<T>.Formatter;

        private static IMessagePackFormatter<T> FormatterCacheImpl<T>()
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
            public static readonly IMessagePackFormatter<T> Formatter = DefaultResolverForTests.FormatterCacheImpl<T>();
        }
    }
}
