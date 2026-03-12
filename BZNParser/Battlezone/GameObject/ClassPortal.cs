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
            IBZNToken tok;

            if (reader.Version >= 2004)
            {
                tok = reader.ReadToken();
                if (!tok.Validate("portalState", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse portalState/LONG");
                if (obj != null) obj.portalState = tok.GetUInt32H();

                tok = reader.ReadToken();
                if (!tok.Validate("portalBeginTime", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse portalBeginTime/FLOAT");
                if (obj != null) obj.portalBeginTime = tok.GetSingle();

                tok = reader.ReadToken();
                if (!tok.Validate("portalEndTime", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse portalEndTime/FLOAT");
                if (obj != null) obj.portalEndTime = tok.GetSingle();

                tok = reader.ReadToken();
                if (!tok.Validate("isIn", BinaryFieldType.DATA_BOOL)) throw new Exception("Failed to parse isIn/BOOL");
                tok.ReadBoolean(obj, x => x.isIn);
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
                writer.WriteUnsignedValues("portalState", obj.portalState);
                writer.WriteFloats("portalBeginTime", preserveMalformations ? obj.Malformations : null, obj.portalBeginTime);
                writer.WriteFloats("portalEndTime", preserveMalformations ? obj.Malformations : null, obj.portalEndTime);
                writer.WriteBoolean("isIn", obj, x => x.isIn);
            }
            ClassGameObject.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
        }
    }
}
