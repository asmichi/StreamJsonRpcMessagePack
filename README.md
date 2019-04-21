# Asmichi.StreamJsonRpcMessagePack

A .NET library that provides a MessagePack for C# based serializeer for StreamJsonRpc.

# Usage

- Add `JsonRpcMessagePackResolver.Instance` to your custom `IFormatterResolver`.
- Instantiate `JsonRpcMessagePackFormatter` with the `IFormatterResolver`.
- Done; just pass it to `JsonRpc`!

# Limitations

- Support for parameter objects is experimental, disabled by default, slow and subject to change.

# TODOs

- Implement post-processing of `<inheritdoc/>`.
