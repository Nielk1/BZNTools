# BZNTools

This toolkit allows for the parsing and processing of BZN map files from the following games:
* Star Trek Armada
* Star Trek Armada II
* Battlezone (1998)
* Battlezone: Rise of the Black Dogs (n64)
* Battlezone II: Combat Commander
* Battlezone 98 Redux
* Battlezone Combat Commander

The BZN format is a simplistic data array format.
It can store data in both ASCII and Binary mode but all start in ASCII mode except in special circumstances.
The format is also used for save files but with additional data.
All flavors of the BZN format differ slightly in their internal handling depending on which game they are for.
BZN files store data as Type+Size+Value in Binary mode and Name+Value in ASCII mode.
This parser is significantly more complex and capable than the handling in the games themselves.
Note that ASCII format BZNs do not have a regular format and are effectively written by crude text writes in their originating games.
Binary mode is always preferred for stability but ASCII mode is easily edited.

This parser is built in 2 stages.
The Tokenizer stage utilizes the BZNStreamReader to convert the BZN file into a stream of BZNTokens.
The BZNStreamReader will peek at the beginning of the file to evaluate its flavor and format.

## BZN Format Differences

| Game                               | TypeSize | SizeSize | Alignment | Endian |
|:-----------------------------------|---------:|---------:|----------:|:-------|
| Battlezone: Rise of the Black Dogs |        0 |        2 |         2 | Big    |
| Battlezone                         |       2* |        2 |         0 | Little |
| Battlezone 98 Redux                |        2 |        2 |         0 | Little |
| Battlezone II: Combat Commander    |        1 |        2 |         0 | Little |
| Battlezone Combat Commander        |        1 |        2 |         0 | Little |
| Star Trek Armada                   |       4* |        4 |         0 | Little |
| Star Trek Armada II                |       4* |        4 |         0 | Little |

\* The Type field in Armada BZNs is 4 bytes but 3 of those bytes are garbage data. Some Battlezone BZNs have shown this same behavior with their 2 byte type values. For now we just always use 1 byte for the type for all games as there are less than 256 types in all games.

Difficulties:
* There are no markers for the ends of objects so the format is very fragile.
* There are markers for the starts of some objects in ASCII mode, but it's effectively a comment in the game's code. We use it for Validation tokens.

The decoupling of the Tokenizer and game-specific Parser allows for larger changes or differences in the BZN format between games or versions to be ignored.
This allows for the BZNFileBattlezone class to be used for all Battlezone games with only minor changes to the parsing logic and may facilitate the easier conversion of maps between those games.

## Star Trek Armada
No work has yet been done on parsing Armada BZNs but the framework is in place to do so. The token stream will be created properly.

## Battlezone
The BZNFileBattlezone class is the main class for handling BZN files from all Battlezone games.
This class can ingest hints to help guide it in parsing BZN files as the specific properties of objects are required to parse their data properly.
The parser is capable of reading BZNs even with no hint data through the use of heavy memoization and "try everything" paths.
This "try everything" logic may result in the emission of "MultiClass" objects when it is not possible to determine the correct object type based on the limited information available.

### Usage
To parse a BZN file from a Battlezone game:
```csharp
using BZNParser.Battlezone;
using BZNParser.Tokenizer;

// Load hints for the game (e.g., BZ1 or BZ2)
var hints = BattlezoneBZNHints.BuildHintsBZ1();

using (var fileStream = File.OpenRead("path/to/file.bzn"))
using (var reader = new BZNStreamReader(fileStream, "path/to/file.bzn"))
{
    var bznFile = new BZNFileBattlezone(reader, Hints: hints);
    // Access parsed data, e.g., bznFile.Entities
}
```

### Hints
Hints are used to guide the parser in understanding the structure of the BZN file by providing information from outside, such as the ClassLabel of game object files.
See the `BuildHintsBZ1` and `BuildHintsBZ2` functions in the [`BattlezoneBZNHints`](BZNParser/Battlezone/BZNFileBattlezone.cs) class for examples of how to build the hints structure.

The built-in hint loaders first look in the current directory, then beside the
application, where the project copies its hint files. This allows running a
consumer from a different working directory while retaining local overrides.

### BZCC map layouts

Public 2.0.185 maps can share BZN version 1196 with later maps while omitting
the `ejectRatio` record. The parser recognizes both map layouts and preserves
record presence when writing. It also preserves optional `isDamped` masks in
version-1197+ maps and the `turnOff` SatellitePanel extension observed in
version-1200 maps. See [layout and round-trip notes](docs/BZCC-map-layouts.md)
for the detection rules and exposed properties.

### Build and regression checks

Use the .NET 8 SDK:

```sh
dotnet build BZNTools.sln --configuration Release
dotnet test BZNTools.sln --configuration Release
```

The automated tests use independently encoded synthetic maps and do not require
an installed game or a private map corpus. The existing `BZNParserTest` program
also accepts files and directories for a read-only corpus check:

```sh
dotnet run --project BZNParserTest --configuration Release -- --check "path/to/maps"
dotnet run --project BZNParserTest --configuration Release -- --check --exact "path/to/map.bzn"
```

These commands parse, write to memory, and parse the result again. `--exact`
additionally requires byte-for-byte equality. Exit codes are 0 for success,
1 for failed checks, and 2 for invalid usage or an empty selection. Source maps
are never written. Running without arguments retains the original developer
harness and its local corpus paths.

### Bookmarks
A "Bookmark" is a marker in the token stream that allows for the parser to return to a specific point in the stream after parsing failures.
They are not intended to persist and exist in a stack structure.
They allow for the parser to create a "save point" and return to it should a guess about the structure of the underlying data be found to be incorrect.
Battlezone II and Battlezone Combat Commander BZNs rely heavily on object data not saved in the BZN and thus use the bookmark system heavily when parsing.

### Malformations
The BZN format is very fragile and can easily become malformed.
This parser attempts to resolve malformations as best as it can and will track the malformations it encounters in the `BZNFileBattlezone.Malformations` list.

Currently supported Malformations:
* `UNKNOWN` (should not be used)
* `INCOMPAT` Not loadable by game (not currently used)
* `MISINTERPRET` A field `[0]` was misinterpreted by game as a `[1]` but thus is loadable despite being incorrect.
* `OVERCOUNT` Too many objects of the name `[0]`, maximum may have changed
* `NOT_IMPLEMENTED` Field named `[0]` not implemented, but it probably won't break the BZN read
* `INCORRECT` Value saved in field `[0]` is incorrect and has been corrected, old value was `[1]`
* `LINE_ENDING` Line ending is incorrect as Battlezone expects CRLF, `[0]` is the type present either `LF`, `CR`, or `?`
