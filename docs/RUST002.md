# RUST002: Incomplete Hook Method Signature

## Problem

The hook method is missing required parameters or has incorrect parameter types. Oxide hooks must be implemented with the exact parameter types specified in the hook signature.

## Solution

Implement the hook method with all required parameters using the correct types.

## Examples

### Incorrect Implementation ❌

```csharp
// Missing required BasePlayer parameter
void OnPlayerConnected()
{
    // This will trigger warning RUST002
}

// Incorrect parameter type (string instead of BasePlayer)
void OnPlayerConnected(string player)
{
    // This will trigger warning RUST002
}
```

### Correct Implementation ✅

```csharp
void OnPlayerConnected(BasePlayer player)
{
    // This is correct - matches the expected signature
}
```

## Configuration

This rule is enabled by default and will trigger a warning when:
- A method name matches a known Oxide hook
- The method's parameter list doesn't exactly match the hook's expected parameters

## Rule Description

| Property | Value |
|----------|--------|
| Category | Usage |
| Severity | Warning |
| Enabled | Yes |
| Code | RUST002 |

## When to Suppress

This warning should rarely be suppressed as incorrect hook signatures will prevent the hook from being called by Oxide. Only suppress this warning if you are absolutely certain that:
1. You are intentionally creating a method with the same name as a hook but different functionality
2. You have verified that this won't conflict with Oxide's hook system

To suppress the warning, use:

```csharp
#pragma warning disable RUST002
void OnPlayerConnected() // Suppressed warning
{
    // Your code here
}
#pragma warning restore RUST002
```

## Common Hook Signatures

Here are some commonly used hook signatures:

```csharp
void OnPlayerConnected(BasePlayer player)
void OnPlayerDisconnected(BasePlayer player, string reason)
void OnEntityTakeDamage(BaseCombatEntity entity, HitInfo info)
void OnPlayerRespawn(BasePlayer player)
void OnPlayerChat(BasePlayer player, string message, ConVar.Chat.ChatChannel channel)
```