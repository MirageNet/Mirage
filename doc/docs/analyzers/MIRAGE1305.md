# MIRAGE1305: Missing NetworkMessage Attribute

## The Problem
A class or struct is used in message-sending methods (like `Send<T>()`) or registered in a message-handling system (like `RegisterHandler<T>()`), but is missing the `[NetworkMessage]` attribute.

Mirage's post-compilation Weaver scans types decorated with `[NetworkMessage]` to generate serialization helper code and register unique message type IDs (hashes of the full type name). If a type is sent or registered as a handler without the `[NetworkMessage]` attribute, the Weaver will not have generated the necessary metadata or registration wrappers. This results in runtime errors such as failing to serialize, failing to unpack, or "Unexpected message ID" warnings when the message is received.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1305.cs' Name:'mirage1305-triggering' }}}

---

## How to Resolve

Decorate the target message class or struct with the `[NetworkMessage]` attribute. This instructs Mirage's Weaver to correctly weave code for message serialization, generation, and handler registration.

{{{ Path:'Snippets/Analyzers/Mirage1305.cs' Name:'mirage1305-resolved' }}}
