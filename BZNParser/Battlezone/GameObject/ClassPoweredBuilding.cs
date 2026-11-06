using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "commtower")] // commtower > commbunker > powered
    [ObjectClass(BZNFormat.Battlezone2, "commbunker")]
    [ObjectClass(BZNFormat.Battlezone2, "supplydepot")]
    [ObjectClass(BZNFormat.Battlezone2, "barracks")]
    [ObjectClass(BZNFormat.Battlezone2, "powered")]
    [ObjectClass(BZNFormat.Battlezone2, "techcenter")]
    public class ClassPoweredBuildingFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
            {
                obj = new ClassPoweredBuilding(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                return ClassPoweredBuilding.Hydrate(parent, reader, obj as ClassPoweredBuilding).Success;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
        }
    }
    public class ClassPoweredBuilding : ClassBuilding
    {
        public int scriptPowerOverride { get; set; }
        public Int32[]? powerHandle { get; set; }

        public ClassPoweredBuilding(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel)
        {
            scriptPowerOverride = -1;
            powerHandle = null;
        }

        public override void ClearMalformations()
        {
            Malformations.Clear();
            base.ClearMalformations();
        }

        public override void DisableMalformationAutoFix()
        {
            base.DisableMalformationAutoFix();
        }

        public override void EnableMalformationAutoFix()
        {
            base.EnableMalformationAutoFix();
        }


        public static ParseResult Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassPoweredBuilding? obj)
        {
            // scriptPowerOverride (a single LONG) can be misread as a powerHandle (a countable LONG array),
            // because a lone leading LONG validates as a 1-element powerHandle and then swallows the next
            // value(s). Disambiguate by scanning twice: once assuming powerHandle is present, and once
            // assuming it is not. A pass is only accepted if every token validates AND ClassBuilding.Hydrate
            // succeeds; otherwise restore the position and try the other arrangement. (Per maintainer:
            // don't fight the version gates -- just attempt all the fields regardless of version; the
            // validation plus the base parse resolve which layout is real.)
            //
            // We save/restore the stream position with Get()/Set() rather than the bookmark stack, so this
            // retry is not sensitive to any bookmarks the delegated-to reads (e.g. ClassBuilding.Hydrate)
            // may have left on the stack.
            long startPos = reader.Bookmark.Get();

            if (TryHydratePass(parent, reader, obj, hasPowerHandle: true))
                return ParseResult.Ok();

            reader.Bookmark.Set(startPos);

            if (TryHydratePass(parent, reader, obj, hasPowerHandle: false))
                return ParseResult.Ok();

            reader.Bookmark.Set(startPos);
            return ParseResult.Fail("Failed to parse ClassPoweredBuilding: neither a powerHandle-present nor a powerHandle-absent layout matched");
        }

        // Reads one candidate arrangement of the PoweredBuilding fields and reports whether it fully parsed.
        // Fails (returns false) on the first bad token validation or if ClassBuilding.Hydrate does not
        // succeed. Does not save/restore the position itself -- the caller does that between attempts.
        private static bool TryHydratePass(BZNFileBattlezone parent, BZNStreamReader reader, ClassPoweredBuilding? obj, bool hasPowerHandle)
        {
            IBZNToken? tok;

            //TapHelper::Save((this + 2060), a2); // used by PoweredBuilding and Turret (gun tower) to save lung data
            if (hasPowerHandle)
            {
                // we don't know how many taps there are without the ODF, so just try to read forever
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("powerHandle", BinaryFieldType.DATA_LONG))
                    return false;
                if (obj != null)
                    obj.powerHandle = Enumerable.Range(0, tok.GetCount()).Select(i => tok.GetInt32(i)).ToArray();
            }
            else if (obj != null)
            {
                // make sure a failed pass 1 did not leave a bogus powerHandle behind
                obj.powerHandle = null;
            }

            if (parent.SaveType != SaveType.BZN)
            {
                //(a2->vftable->read_long)(a2, this + 2052, 4, "terminalUser");
                //(a2->vftable->out_bool)(a2, this + 2056, 1, "terminalRemote");
            }

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("scriptPowerOverride", BinaryFieldType.DATA_LONG))
                return false;
            tok.ApplyInt32(obj, x => x.scriptPowerOverride);

            var retVal = ClassBuilding.Hydrate(parent, reader, obj as ClassBuilding);
            return retVal.Success;
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassPoweredBuilding obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            if (writer.Version >= 1062)
            {
                if (obj.powerHandle != null && obj.powerHandle.Length > 0)
                {
                    writer.WriteSignedValues("powerHandle", obj.powerHandle);
                    //writer.WriteInt32("powerHandle", obj, obj.powerHandle); // TODO update this to handle enumerable
                }
            }

            if (parent.SaveType != SaveType.BZN)
            {
                //(a2->vftable->read_long)(a2, this + 2052, 4, "terminalUser");
                //(a2->vftable->out_bool)(a2, this + 2056, 1, "terminalRemote");
            }

            if (writer.Version >= 1193)
            {
                //writer.WriteSignedValues("scriptPowerOverride", obj.scriptPowerOverride);
                writer.WriteInt32("scriptPowerOverride", obj, x => x.scriptPowerOverride);
            }

            ClassBuilding.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
