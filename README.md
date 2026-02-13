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
It can store data in both ASCII and Binary mode but all start in ASCII made except in special curcumstances.
The format is also used for save files but with additional data.
All flavors of the BZN format differ slightly in their internal handling depending on which game they are for.
BZN files store data as Type+Size+Value in Binary mode and Name+Value in ASCII mode.
This parser is signifigantly more complex and capable than the handling in the games themselves.
Note that ASCII format BZNs do not have a regular format and are effectively written by crude text writes in their originating games.
Binary mode is always prefered for stability but ASCII mode is easily edited.

This parser is built into 2 stages.
The Tokenizer stage utilizes the BZNStreamReader to convert the BZN file into a stream of BZNTokens.
The BZNStreamReader will peek at the beginning of the file to evaluate its flavor and format.

| Game                               | TypeSize | SizeSize | Alignment | Endian |
|:-----------------------------------|---------:|---------:|----------:|:-------|
| Star Trek Armada                   |       4* |        4 |         0 | Little |
| Star Trek Armada II                |       4* |        4 |         0 | Little |
| Battlezone                         |        2 |        2 |         0 | Little |
| Battlezone: Rise of the Black Dogs |        0 |        2 |         2 | Big    |
| Battlezone II: Combat Commander    |        1 |        2 |         0 | Little |
| Battlezone 98 Redux                |        2 |        2 |         0 | Little |
| Battlezone Combat Commander        |        1 |        2 |         0 | Little |
* The Type field in Armada BZNs is 4 bytes but 3 of those bytes are garbage data. For now we just always use 1 byte for the type for all games as there are less than 256 types in all games.

Difficulties:
* There are no markers for the ends of objects so the format is very fragile.
* There are markers for the starts of some objects in ASCII mode, but it's effectively a comment in the game's code. We use it for Validation tokens.

The decoupling of the Tokenizer and game specific Parser allows for larger changes or differences in the BZN format between games or versions to be ignored.
This allows for the BZNFileBattlezone class to be used for all Battlezone games with only minor changes to the parsing logic and may facilitate the easier conversion of maps between those games.

## Star Trek Armada
No work has yet been done on parsing Armada BZNs but the framework is in place to do so. The token stream will be created properly.

## Battlezone
The BZNFileBattlezone class is the main class for handling BZN files from all Battlezone games.
This class can ingest hints to help guide it in parsing BZN files as the specific properties of objects are required to parse their data properly.
The parser is capable of reading BZNs even with no hint data through the user of heavy memoization and "try everything" paths.
This "try everything" logic may result in the emission of "MultiClass" objects when it is not possible to determine the correct object type based on the limited information available.
