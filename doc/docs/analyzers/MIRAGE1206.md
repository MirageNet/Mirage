# MIRAGE1206: Invalid RateLimit Attribute Settings

## The Problem
The `[RateLimit]` attribute contains invalid configurations. Settings are invalid if:

- `Interval` or `Refill` is less than or equal to zero.
- `MaxTokens` is less than or equal to zero, or less than `Refill`.
- `Penalty` is negative.

Rate limiting requires positive settings to function correctly. Invalid configurations prevent token replenishment or cause server issues.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1206.cs' Name:'mirage1206-triggering' }}}

---

## How to Resolve

Configure the `[RateLimit]` attribute with positive values where `MaxTokens` is greater than or equal to `Refill`, and `Penalty` is non-negative.

{{{ Path:'Snippets/Analyzers/Mirage1206.cs' Name:'mirage1206-resolved' }}}
