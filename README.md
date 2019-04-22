# Asmichi.StreamJsonRpcMessagePack

A .NET library that provides a MessagePack for C# based serializeer for StreamJsonRpc.

This library can be obtained via [NuGet](https://www.nuget.org/packages/Asmichi.StreamJsonRpcMessagePack/).

[![Build Status](https://dev.azure.com/asmichi/StreamJsonRpcMessagePack/_apis/build/status/StreamJsonRpcMessagePack-CI?branchName=master)](https://dev.azure.com/asmichi/StreamJsonRpcMessagePack/_build/latest?definitionId=3&branchName=master)

# Usage

- Add `JsonRpcMessagePackResolver.Instance` to your custom `IFormatterResolver`.
- Instantiate `JsonRpcMessagePackFormatter` with the `IFormatterResolver`.
- Done; just pass it to `JsonRpc`!

See [src/JsonRpcMessagePackFormatter.Example](src/JsonRpcMessagePackFormatter.Example/) for an example.

# License

[MIT License](LICENSE)

# Limitations

- Methods that return non-generic `Task` cannot be invoked due to https://github.com/Microsoft/vs-streamjsonrpc/issues/259.
- Support for parameter objects is experimental, disabled by default and subject to change.
    - Currently the implementation tries to respect Json.NET attributes; this is not a fixed specification.

# Supported Frameworks

- `net461` or later
- `netstandard2.0`

# TODOs

- Proper tests.
    - Currently [reusing StreamJsonRpc tests](https://github.com/asmichi/vs-streamjsonrpc/commits/TestStreamJsonRpcMessagePack).
- Implement post-processing of `<inheritdoc/>`.
