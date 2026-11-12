# BZNTools

This toolkit parses, inspects, modifies, and re-serializes **BZN** map files (and, for some games, save files) for the following titles:

* Star Trek Armada
* Star Trek Armada II
* Battlezone (1998)
* Battlezone: Rise of the Black Dogs (N64)
* Battlezone II: Combat Commander
* Battlezone 98 Redux
* Battlezone Combat Commander

## What is a BZN file?

BZN is a simple, loosely-structured data format. A few characteristics matter when working with it:

* **Two encodings.** The same logical data can be stored in **ASCII** (text) mode or **Binary** mode. In practice every file *starts* in ASCII mode and switches to binary for particular objects, except in special circumstances.
* **Encoding rules.** In Binary mode a field is stored as `Type + Size + Value`; in ASCII mode it is stored as `Name + Value`.
* **Game-dependent.** Every flavor handles the format slightly differently (see [BZN Format Differences](#bzn-format-differences)), and a single parser must cope with all of them.
* **Fragile.** ASCII BZNs in particular are produced by crude, ad-hoc text writes in the originating games, so there is no strictly regular structure to lean on. Binary mode is preferred for stability; ASCII mode is preferred for human editing.
* **Also used for saves.** The same format underlies save files, which carry additional data beyond the map definition.

This toolkit is considerably more capable than the in-game readers it targets: it survives a wide range of real-world defects, records each one, and (optionally) reproduces them on output so a file can be **round-tripped** while preserving exactly what was originally there.

## Architecture

The toolkit is deliberately built in two decoupled stages:

1. **Tokenizer.** [`BZNStreamReader`](BZNParser/Tokenizer/BZNStreamReader.cs) reads the raw bytes and turns them into an ordered stream of **tokens** (`BZNTokenString`, `BZNTokenBinary`, `BZNTokenNestedString`, `BZNTokenValidation`). As it goes it *sniffs* the start of the file to work out the game flavor and mode, and scans ahead to detect **token defects** (see [Token Stream & Defects](#token-stream--defects)).
2. **Parser.** A game-specific model walks the token stream and maps the tokens onto a strongly-typed object graph it owns. For the Battlezone family that model is [`BZNFileBattlezone`](BZNParser/Battlezone/BZNFileBattlezone.cs).

Keeping the low-level tokenizer independent of the high-level model means that format differences between games and versions are absorbed in one place. The same `BZNFileBattlezone` class therefore serves every Battlezone game with only minor parsing tweaks, which also makes converting maps *between* those games much easier.

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
using BZNParser.Reader;

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

### Bookmarks
A "Bookmark" is a marker in the token stream that allows for the parser to return to a specific point in the stream after parsing failures.
They are not intended to persist and exist in a stack structure.
They allow for the parser to create a "save point" and return to it should a guess about the structure of the underlying data be found to be incorrect.
Battlezone II and Battlezone Combat Commander BZNs rely heavily on object data not saved in the BZN and thus use the bookmark system heavily when parsing.

## Malformations

The BZN format is very fragile and real-world files are frequently malformed. Rather than silently guessing, the parser records every irregularity it survives in a **malformation list** — one per object, reachable as `BZNFileBattlezone.Malformations` (every model object carries its own `MalformationManager`).

Two properties of the design are important to understand:

* **Malformations are stored separately from the data, keyed by field.** Each malformation is attached to a specific property (and, for arrays, a specific index) plus a type and zero or more payload values — e.g. the original raw bytes or original text. The *value* of a property and its *malformation* are independent: the model holds the corrected value, while the malformation remembers how the file actually was.
* **They are applied at write time, based on presence, not on the data.** When a field is written, the writer asks "does this field have an `INCORRECT_RAW` / `RIGHT_TRIM` / `MISSING_FIELD` / … malformation?" and, if so, emits the defective form instead of the clean one. Whether the data "still needs" the malformation is never re-checked automatically.

### Malformation types

The full set is defined by the [`Malformation` enum](BZNParser/Tokenizer/IMalformable.cs):

| Type | Payload | Meaning |
|:-----|:--------|:--------|
| `UNKNOWN` | — | Placeholder; should never appear in a valid parse. |
| `EXTRA_FIELD` | — | The file contains data for a field the spec doesn't define; it is kept so it can be written back. |
| `DATA_AFTER_NULL` | `byte[] originalRaw` | A string parsed correctly, but extra data followed the terminating NUL (e.g. a `.ext` cut off into NULs). |
| `INCORRECT_RAW` | `byte[] originalRaw` | The value didn't parse to the expected type; the original raw bytes are kept and emitted verbatim (ASCII & Binary modes). |
| `INCORRECT_TEXT` | `string originalText` | Same as above but in ASCII mode, keeping the original text for verbatim re-emission. |
| `INCORRECT_FLOAT` | `string originalText` | A float was written in a non-standard way; the original text is kept for verbatim re-emission. |
| `INCORRECT_CASE` | `char 'U'/'L'` | A field name was present in an unexpected casing. |
| `INCORRECT_LENGTH` | `int` | The stored length/count of a value did not match its actual size. |
| `LINE_ENDING` | `string` | The file used non-CRLF line endings (`"CR"`, `"LF"`, or `"?"` for mixed). Applied file-wide on output. |
| `INCORRECT_NAME` | `string badFieldName` | A sub-field name did not match the expected one (rare; validation usually just fails instead). |
| `FLOAT_FORMAT` | `FloatTextFormat` | A float used an unexpected text format (e.g. `9e2` vs `G`); preserved so output matches. |
| `RIGHT_TRIM` | — | A one-liner token was right-trimmed (the space before `=` was missing). |
| `MISSING_FIELD` | — | A field required by the version spec was absent from the source. The value is left at its default and the writer **skips emitting the field entirely** in mirror mode. No payload is stored. |
| `LINE_WRAP` | `int wrapWidth` | **Abnormal** — BZN one-liners are not supposed to line-wrap at all (the cause here is unknown), but it was split with a mid-line CRLF; `wrapWidth` is the character count before the injected CRLF (typically `4095`). |

(The enum also carries a commented-out block of retired/planned types — `INCOMPAT`, `MISINTERPRET`, `OVERCOUNT`, `NOT_IMPLEMENTED`, `STRING_PAD` — that are no longer in use.)

### How malformations interact with editing

Because malformations are independent of the data, changing a value does **not** by itself guarantee its malformation is gone:

* **Some fields auto-clear on change.** A few value types — most notably the numeric struct fields in [`Types.cs`](BZNParser/Types.cs) (`Vector2D`/`Vector3D` components, `Euler`, `Matrix`) — use setters that clear *that field's* malformation when you assign a genuinely different value:

  ```csharp
  set
  {
      if (!blockAutoFixMalformations && value != x)
          Malformations.Clear<Vector3D, float>(x => x.X);
      x = value;
  }
  ```

* **Most fields do not.** Plain auto-properties (`{ get; set; }`) carry no such logic, so a malformation on them will happily survive a value change. For this reason the auto-clear is **not relied upon** — the API exposes explicit clearing instead:
  * `Malformations.Clear()` — drop everything on that object.
  * `Malformations.Clear<T, TProp>(x => x.Field)` — drop everything on one field.
  * `Malformations.Clear<T, TProp>(x => x.Field, index)` — drop everything on one array element.
  * `Malformations.Clear<T, TProp>(x => x.Field, Malformation.SOME_TYPE)` — drop only one type.

If you have modified the data, the recommended practice is to call `ClearMalformations()` (which walks the whole object graph) before writing, so you don't accidentally re-emit defects that no longer apply.

## Token Stream & Defects

Underneath the strongly-typed model, the tokenizer produces a **token stream** — an ordered list of small, self-describing value carriers. This is the canonical "what was actually in the file" representation, and it is where defects are first noticed.

**Token types** (all implement [`IBZNToken`](BZNParser/Tokenizer/IBZNToken.cs)):

* `BZNTokenString` — an ASCII `Name = Value` line (possibly a multi-value field).
* `BZNTokenNestedString` — an ASCII block of named sub-tokens (e.g. a `vec3` with `x`/`y`/`z`, a `matrix`, an `euler`).
* `BZNTokenBinary` — a raw binary field: a `Type` byte, a size, and a blob of data.
* `BZNTokenValidation` — a marker for an expected section start (the "comment" the game code relies on); used to line up guesses, it carries no data.

**Defects live on the tokens.** While reading, `BZNStreamReader` flags irregularities directly on the token object rather than throwing the original away:

* `BZNTokenString.RightTrimmedOneLiner` — the one-liner had been right-trimmed (no space before `=`).
* `BZNTokenString.LineWrapWidth` — **abnormal**: BZN one-liners are not supposed to line-wrap, but a mid-line CRLF split this one at a fixed column width (cause unknown); the reader transparently rejoins it and remembers the width so the writer can re-wrap it identically.
* `BZNTokenBinary.rawType` — the stored `Type` byte did not equal the type actually expected.

**Defects are promoted into malformations.** As the model parser pulls each token's value onto a property (via the `TokenExtensions.Apply*` helpers), any defect found on that token is translated into the matching malformation on the owning object — e.g. `RIGHT_TRIM`, `LINE_WRAP`, `INCORRECT_RAW`, `INCORRECT_NAME`. From that point on, the token defect and the malformation describe the same quirk, but the malformation is what the model and the writer actually operate on.

## Reproducing the original file (mirror mode)

When you hand the model to a `BZNStreamWriter` with **`preserveMalformations`** (mirror mode) turned on, the writer re-applies every malformation it finds on each field:

* it writes the **original raw bytes / text** for `INCORRECT_RAW` / `INCORRECT_TEXT` / `INCORRECT_FLOAT` fields instead of the corrected value,
* it reproduces `RIGHT_TRIM`, re-wraps any line at its recorded `LINE_WRAP` width (an abnormal condition, but reproduced faithfully), honors the non-standard `FLOAT_FORMAT`, and applies the file-wide `LINE_ENDING`,
* it writes back `EXTRA_FIELD` data that a clean re-serialization would drop, and
* it **omits** fields flagged `MISSING_FIELD`, so a field that was absent from the source stays absent.

The result is a byte-for-byte re-creation of the input, defects included — with one documented exception: **trailing padding at the end of the file** (any newlines after the last known token) is not reproduced. Turn mirror mode *off* and the writer instead emits a clean, normalized BZN of the current model values.
