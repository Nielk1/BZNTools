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
        public UInt32 portalState { get; set; }
        public float portalBeginTime { get; set; }
        public float portalEndTime { get; set; }
        public bool isIn { get; set; }

        public ClassPortal(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassPortal? obj)
        {
            IBZNToken? tok;

            if (reader.Version >= 2004)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("portalState", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse portalState/LONG");
                tok.ApplyUInt32(obj, x => x.portalState); // should be hex?

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("portalBeginTime", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse portalBeginTime/FLOAT");
                tok.ApplySingle(obj, x => x.portalBeginTime);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("portalEndTime", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse portalEndTime/FLOAT");
                tok.ApplySingle(obj, x => x.portalEndTime);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("isIn", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse isIn/BOOL");
                tok.ApplyBoolean(obj, x => x.isIn);
            }

            ClassGameObject.Hydrate(parent, reader, obj as ClassGameObject);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            Dehydrate(this, parent, writer, binary, save, preserveMalformations);
        }

        public static void Dehydrate(ClassPortal obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            if (writer.Version >= 2004)
            {
                writer.WriteUInt32("portalState", obj, x => x.portalState); // should be hex?
                writer.WriteSingle("portalBeginTime", obj, x => x.portalBeginTime);
                writer.WriteSingle("portalEndTime", obj, x => x.portalEndTime);
                writer.WriteBoolean("isIn", obj, x => x.isIn);
            }
            ClassGameObject.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
        }
    }
}
