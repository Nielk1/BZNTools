# BZCC map layouts and round trips

A BZN version is not a complete engine-layout identifier. Public BZCC 2.0.185
and later maps can both declare version 1196 while using different Craft
records. The optional-field detection below applies to maps (`saveType = 0`);
it does not infer the layout of prediction snapshots, join streams, or saved
games from these map observations.

## Craft ejection ratio

After the common Craft prefix:

```text
abandoned, curAmmo, maxAmmo, addAmmo, curPilot
```

the public-185 layout continues with `illumination` and Euler physics. Later
maps insert `ejectRatio` immediately before `illumination`. A binary FLOAT
record has type 5, a two-byte little-endian payload length of 4, and four
payload bytes, so the difference is seven bytes per Craft.

In text maps the next field name identifies presence. In binary maps the
parser checks the complete local sequence: seven or eight scalar FLOAT
records, followed by three VEC3D records with 12-byte payloads. The seven
mandatory scalars are illumination and the six Euler scalars. Lookahead
restores the reader position and token index before the normal field readers
run. Other shapes fail parsing; they are not treated as arbitrary extensions.

`ClassCraft.HasEjectRatio` records presence independently of `m_ejectRatio`.
It defaults to true for newly constructed objects, preserving the previous
writer behavior. Reading an older-layout version-1196+ map sets it to false,
and the writer omits the record. A present ratio of zero remains present.
Version 1195 retains the existing `m_ejectRatio` spelling and required record.
Binary values, including negative zero and NaN payloads, are retained.

## Damping mask

Version-1197+ maps can contain:

```text
saveFlags, isVisible, isDamped, EffectsMask, isSeen, groupNumber
```

Maps prepared for public185 omit `isDamped` without changing the version.
The binary mask has type 3 (SHORT) and a two-byte payload; `EffectsMask` has
type 4 (LONG). The parser distinguishes these records at the expected field
position. `ClassGameObject.HasDampingMask` retains presence separately from
the `UInt16 isDamped` value and defaults to true for new objects. It is used
by the writer only at version 1197 and above.

Both flags belong to individual objects. Maps containing different layouts
on different objects do not depend on a global inferred engine version.
Reading and writing a map preserves its existing flags. Setting a flag to
false explicitly omits that record; it does not emulate later engine behavior.

## Trailing state

The BZ2 reader already consumed SatellitePanel and padding records, but the
writer ended after AiPaths. This truncated otherwise successfully parsed maps
and prevented reading them back. The reader and writer now retain:

| Property | Serialized field | Version handling |
| --- | --- | --- |
| `SatelliteHasEntered` | `hasEntered` | 1125+, nullable when the entire SatellitePanel block is absent |
| `SatelliteOwnerObjects` | Three `ownerObj` LONG records | Written with a present SatellitePanel block; requires exactly three owners |
| `SatelliteTurnOff` | `turnOff` BOOL | Optional, observed in version 1200; read and written for 1200+ |
| `PadData` | Opaque VOID record | 1115+ |
| `PadData2` | Opaque VOID record | 1119+ |

The version-1200 text map `STBolt.bzn` contains `turnOff` between the third
`ownerObj` and `PadData`. Its value is preserved independently of presence,
including a present false value. The padding blocks are stored as byte arrays
without guessing at their internal meaning.

Writing an EntityDescriptor also preserves its unnamed-object flag in
`seqNo`. Previously, the writer cleared bit `0x800000` on the in-memory object
after emitting it. A second write could therefore change the object header
and add a label record. Writing the same model repeatedly now leaves that
state intact.

## Verification

`BZNParser.Tests` independently encodes small text and binary maps, with no
third-party game assets. It checks both layouts of versions 1196, 1197 and
1200, the version-1195 spelling, mixed layouts within one map, zero-valued
extensions, encoding changes, raw float bits, trailing-state preservation,
repeated writes, and malformed/truncated input. It also verifies that a BZ2
Building cannot discard a failed common GameObject parse result.

The command-line corpus check is reproducible against locally available maps:

```sh
dotnet run --project BZNParserTest -c Release -- --check --exact "path/to/maps"
```

Byte-exact parser round trips establish preservation of the inspected files.
They do not establish gameplay compatibility with an executable. Changing
record flags or the numeric version alone is not a general map-conversion
guarantee. Keep original maps when experimenting with conversions.
