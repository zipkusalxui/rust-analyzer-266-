# RUST003: Unused Method Detection

## Summary
Detects unused methods in Rust plugins that may indicate dead code or unnecessary implementations.

## Description
Unused methods in Rust plugins can indicate several issues:
1. Dead code that is no longer needed
2. Hooks that were implemented but are not being used
3. Utility methods that were created but never called

## How to Fix Violations
To fix a violation of this rule, either:
1. Remove the unused method if it's no longer needed
2. Start using the method where appropriate
3. Mark the method as intentionally unused if it's for future use

## When to Suppress Warnings
Suppress this warning only when:
1. The method is intentionally unused (e.g., for future implementation)
2. The method is required by an interface but not currently needed
3. The method is called via reflection or other indirect means

```csharp
[SuppressMessage("RustAnalyzer", "RUST003", Justification = "Method used via reflection")]
public void OnPluginLoaded(Plugin plugin)
{
    // Called dynamically by the plugin system
}
```

## Example of a Violation

```csharp
public class RustPlugin : Plugin
{
    // Unused method - violation
    void CalculateDamage(BasePlayer player, float amount)
    {
        return amount * GetDamageMultiplier(player);
    }

    void OnPlayerAttack(BasePlayer attacker, HitInfo info)
    {
        // Direct damage calculation without using CalculateDamage
        info.damageAmount *= 1.5f;
    }
}
```

## Example of How to Fix

```csharp
public class RustPlugin : Plugin
{
    float CalculateDamage(BasePlayer player, float amount)
    {
        return amount * GetDamageMultiplier(player);
    }

    void OnPlayerAttack(BasePlayer attacker, HitInfo info)
    {
        // Now using the CalculateDamage method
        info.damageAmount = CalculateDamage(attacker, info.damageAmount);
    }
}
```

## Configuration
This rule has no configurable options.
