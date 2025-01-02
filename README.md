## Behaviour change in .NET 9

To reproduce: run `repro.sh`

Output on my machine (Git Bash on Windows 10 64-bit):

```
$ ./repro.sh
Output with net8:
Huge value casted to ulong: 0
Output with net9:
Huge value casted to ulong: 18446744073709551615
```
