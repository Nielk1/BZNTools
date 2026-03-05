using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public ClassPoweredBuilding(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassPoweredBuilding? obj)
        {
            IBZNToken tok;

            //TapHelper::Save((this + 2060), a2); // used by PoweredBuilding and Turret (gun tower) to save lung data
            if (reader.Version >= 1062)
            {
                // we don't know how many taps there are without the ODF, so just try to read forever
                reader.Bookmark.Push();
                tok = reader.ReadToken();
                if (tok.Validate("powerHandle", BinaryFieldType.DATA_LONG))
                {
                    UInt32 powerHandle = tok.GetUInt32();
                    if (tok.GetCount() > 1)
                    {
                        UInt32 powerHandle2 = tok.GetUInt32(1);
                    }
                    reader.Bookmark.Discard();
                }
                else
                {
                    reader.Bookmark.Pop();
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
                if (!tok.Validate("scriptPowerOverride", BinaryFieldType.DATA_LONG))
                    throw new Exception("Failed to parse scriptPowerOverride/LONG");
                Int32 autoTarget = tok.GetInt32();
            }

            ClassBuilding.Hydrate(parent, reader, obj as ClassBuilding);
        }
    }
}
