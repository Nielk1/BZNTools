using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "commvehicle")]
    public class ClassCommVehicleFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
            {
                obj = new ClassCommVehicle(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                return ClassCommVehicle.Hydrate(parent, reader, obj as ClassCommVehicle).Success;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
        }
    }
    public class ClassCommVehicle : ClassTrackedDeployable
    {
        public ClassCommVehicle(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static ParseResult Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassCommVehicle? obj)
        {
            // commvehicle is only present from the BZ2 source baseline (save version 1183 per GBD) onward. Fail
            // fast on older BZN files so it is not considered by the "try everything" MultiClass candidate loop.
            if (reader.Version < 1183)
                return ParseResult.Fail("commvehicle requires BZN save version >= 1183 (file is " + reader.Version + ")");

            return ClassTrackedDeployable.Hydrate(parent, reader, obj as ClassTrackedDeployable);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassCommVehicle obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            ClassTrackedDeployable.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}