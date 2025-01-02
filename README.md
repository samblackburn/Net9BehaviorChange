## Behaviour change in .NET 9

### What changed?
An overflow in a [numeric cast](Program.cs) now gets clamped to max value instead of returning zero.

### Any official documentation?
Here's the nearest we could find (thanks @nyctef): https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/numeric-conversions
> When you convert a double or float value to an integral type, this value is rounded towards zero to the nearest integral value. If the resulting integral value is outside the range of the destination type, the result depends on the [overflow-checking context](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/statements/checked-and-unchecked). In a checked context, an [OverflowException](https://learn.microsoft.com/en-us/dotnet/api/system.overflowexception) is thrown, while in an unchecked context, the result is **an unspecified value of the destination type**.

### To reproduce
run [`repro.sh`](repro.sh)

### Output on my machine
(Git Bash on Windows 10 64-bit):
```
$ ./repro.sh
Output with net8:
Huge value casted to ulong: 0
Output with net9:
Huge value casted to ulong: 18446744073709551615
```
