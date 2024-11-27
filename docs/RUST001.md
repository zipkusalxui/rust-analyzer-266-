# RUST001: Empty Method Detection

## Summary
Detects empty methods in Rust plugins that might indicate incomplete implementation or forgotten code.

## Description
Empty methods in Rust plugins can indicate several issues:
1. Incomplete implementation of a required hook
2. Forgotten code that needs to be implemented
3. Unnecessary method that should be removed

## How to Fix Violations
To fix a violation of this rule, either:
1. Implement the method with the required functionality
2. Remove the method if it's not needed
3. Add a comment explaining why the method is intentionally empty

## When to Suppress Warnings
Suppress this warning only when:
1. The method is intentionally empty (e.g., for interface implementation)
2. The method serves as a placeholder for future implementation

```csharp
[SuppressMessage("RustAnalyzer", "RUST001", Justification = "Method intentionally left empty for future implementation")]
public void OnServerInitialized()
{
    // Will be implemented in future release
}
```

## Example of a Violation

```csharp
public class RustPlugin : Plugin
{
    // Empty method - violation
    void OnPlayerConnected(BasePlayer player)
    {
    }
}
```

## Example of How to Fix

```csharp
public class RustPlugin : Plugin
{
    // Fixed - method has implementation
    void OnPlayerConnected(BasePlayer player)
    {
        PrintToChat($"Welcome {player.displayName}!");
        player.inventory.GiveItem(ItemManager.CreateByName("wood", 100));
    }
}
```

## Configuration
This rule has no configurable options.
