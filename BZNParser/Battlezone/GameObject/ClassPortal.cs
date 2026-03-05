using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone, "portal")]
    public class ClassPortalFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassPortal(preamble, classLabel);
            ClassPortal.Hydrate(parent, reader, obj as ClassPortal);
            return true;
        }
    }
    public class ClassPortal : ClassGameObject
    {
        public ClassPortal(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassPortal? obj)
        {
            IBZNToken tok;

            if (reader.Version >= 2004)
            {
                tok = reader.ReadToken();
                if (!tok.Validate("portalState", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse portalState/LONG");
                UInt32 portalState = tok.GetUInt32H();

                tok = reader.ReadToken();
                if (!tok.Validate("portalBeginTime", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse portalBeginTime/FLOAT");
                float portalBeginTime = tok.GetSingle();

                tok = reader.ReadToken();
                if (!tok.Validate("portalEndTime", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse portalEndTime/FLOAT");
                float portalEndTime = tok.GetSingle();

                tok = reader.ReadToken();
                if (!tok.Validate("isIn", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse isIn/BOOL");
                bool isIn = tok.GetBoolean();
            }

            ClassGameObject.Hydrate(parent, reader, obj as ClassGameObject);
        }
    }
}
