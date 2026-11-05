using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "minelayert")]
    public class ClassMinelayerTFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
            {
                obj = new ClassMinelayerT(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                return ClassMinelayerT.Hydrate(parent, reader, obj as ClassMinelayerT).Success;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
        }
    }
    public class ClassMinelayerT : ClassTrackedVehicle
    {
        public ClassMinelayerT(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static ParseResult Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassMinelayerT? obj)
        {
            // minelayerT was introduced in BZ2 save version 1194 (git 171e3e17b, 2024-12-10 per GBD). Fail
            // fast on older BZN files so it is not considered by the "try everything" MultiClass candidate loop.
            if (reader.Version < 1194)
                return ParseResult.Fail("minelayerT requires BZN save version >= 1194 (file is " + reader.Version + ")");

            return ClassTrackedVehicle.Hydrate(parent, reader, obj as ClassTrackedVehicle);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassMinelayerT obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            ClassTrackedVehicle.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}