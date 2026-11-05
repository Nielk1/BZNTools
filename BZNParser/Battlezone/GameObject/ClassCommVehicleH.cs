using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "commvehicleH")]
    public class ClassCommVehicleHFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
            {
                obj = new ClassCommVehicleH(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                return ClassCommVehicleH.Hydrate(parent, reader, obj as ClassCommVehicleH).Success;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
        }
    }
    public class ClassCommVehicleH : ClassDeployable
    {
        public ClassCommVehicleH(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static ParseResult Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassCommVehicleH? obj)
        {
            // commvehicleH was introduced in BZ2 save version 1192 (git beeebbd, 2018-06-30 per GBD). Fail fast
            // on older BZN files so it is not considered by the "try everything" MultiClass candidate loop.
            if (reader.Version < 1192)
                return ParseResult.Fail("commvehicleH requires BZN save version >= 1192 (file is " + reader.Version + ")");

            return ClassDeployable.Hydrate(parent, reader, obj as ClassDeployable);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassCommVehicleH obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            ClassDeployable.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}