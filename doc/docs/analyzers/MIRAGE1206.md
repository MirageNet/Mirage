# MIRAGE1206: Invalid RateLimit Attribute Settings

## The Problem
The `[RateLimit]` attribute contains invalid configurations. This includes:
1. `Interval` is less than or equal to zero.
2. `Refill` is less than or equal to zero.
3. `MaxTokens` is less than or equal to zero, or is less than the `Refill` rate.

Rate limiting buckets require positive numbers for intervals, refill rates, and max tokens to correctly configure token replenishment cycles. If any of these values are zero or negative, or if `MaxTokens` is set to a value less than `Refill`, the rate limiting logic will fail to function or cause infinite loops/resource starvation on the server.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1206.cs' Name:'mirage1206-triggering' }}}

---

## How to Resolve

Correct the parameters of the `[RateLimit]` attribute to ensure they are positive, valid values. Ensure `MaxTokens` is at least equal to the `Refill` value.

{{{ Path:'Snippets/Analyzers/Mirage1206.cs' Name:'mirage1206-resolved' }}}
