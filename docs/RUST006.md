# RUST006: Member Not Found

## Summary
Detects attempts to use non-existent members (methods, properties, or fields) on types, providing helpful suggestions for similar existing members.

## Description
This analyzer helps developers identify and fix member access errors by:
1. Detecting attempts to use non-existent members on types
2. Providing detailed error messages in Rust-style format
3. Suggesting similar existing members that might have been intended
4. Showing exact location of the error with visual pointer
5. Including type information and available alternatives

## How to Fix Violations
To fix a violation of this rule, either:
1. Use the correct member name (check the suggestions in the error message)
2. Verify that you're using the correct type
3. Ensure required using directives or references are included
4. Check if the member is accessible (public/protected/internal)

## When to Suppress Warnings
This warning should rarely be suppressed as it indicates a definite error. However, you can suppress it if:
1. You're dealing with dynamic member access that the analyzer cannot verify
2. You're using reflection to access members

```csharp
[SuppressMessage("RustAnalyzer", "RUST006", Justification = "Dynamic member access handled at runtime")]
public void InvokeDynamicMember(object target, string memberName)
{
    // Member access handled through reflection
    target.GetType().GetMethod(memberName)?.Invoke(target, null);
}
```

## Example of a Violation

```csharp
public class MyPlugin : RustPlugin
{
    void Init()
    {
        permission.RegisterPermission1(PermissionUse, this);  // Error: Method doesn't exist
    }
}
```

## Example of How to Fix

```csharp
public class MyPlugin : RustPlugin
{
    void Init()
    {
        permission.RegisterPermission(PermissionUse, this);  // Fixed: Using correct method name
    }
}
```

## Error Message Format
The analyzer provides detailed error messages in Rust-style format:

```
error[E0599]: no method named `RegisterPermission1` found for type `Oxide.Core.Libraries.Permission` in the current scope
  --> MyPlugin.cs:66:24
   |
66 |             permission.RegisterPermission1(PermissionUse, this);
   |                        ^^^^^^^^^^^^^^^^^^^ method not found in `Oxide.Core.Libraries.Permission`
   |
   = note: the type `Oxide.Core.Libraries.Permission` does not have a method named `RegisterPermission1`
   = help: did you mean one of these?
           - `void RegisterPermission(string permission, Oxide.Core.Plugins.Plugin owner)`
```

## Configuration
This rule has no configurable options.

## Rule Details

| Property | Value |
|----------|-------|
| Category | Usage |
| Severity | Error |
| Enabled | Yes |
| Code | RUST006 | 