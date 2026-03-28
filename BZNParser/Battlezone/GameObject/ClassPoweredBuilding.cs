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
                obj = new ClassPoweredBuilding(preamble, classLabel);
            ClassPoweredBuilding.Hydrate(parent, reader, obj as ClassPoweredBuilding);
            return true;
        }
    }
    public class ClassPoweredBuilding : ClassBuilding
    {
        public int scriptPowerOverride { get; set; }
        public Int32[]? powerHandle { get; set; }

        public ClassPoweredBuilding(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassPoweredBuilding? obj)
        {
            IBZNToken tok;

            //TapHelper::Save((this + 2060), a2); // used by PoweredBuilding and Turret (gun tower) to save lung data
            if (reader.Version >= 1062)
            {
                // we don't know how many taps there are without the ODF, so just try to read forever
                reader.Bookmark.Mark();
                tok = reader.ReadToken();
                if (tok.Validate("powerHandle", BinaryFieldType.DATA_LONG))
                {
                    obj.powerHandle = Enumerable.Range(0, tok.GetCount()).Select(i => tok.GetInt32(i)).ToArray();
                    reader.Bookmark.Commit();
                }
                else
                {
                    reader.Bookmark.RevertToBookmark();
                }
            }

            if (parent.SaveType != SaveType.BZN)
            {
                //(a2->vftable->read_long)(a2, this + 2052, 4, "terminalUser");
                //(a2->vftable->out_bool)(a2, this + 2056, 1, "terminalRemote");
            }

            if (reader.Version >= 1193)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("scriptPowerOverride", BinaryFieldType.DATA_LONG))
                    throw new Exception("Failed to parse scriptPowerOverride/LONG");
                //if (obj != null) obj.scriptPowerOverride = tok.GetInt32();
                tok.ApplyInt32(obj, x => x.scriptPowerOverride);
            }

            ClassBuilding.Hydrate(parent, reader, obj as ClassBuilding);
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
