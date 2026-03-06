using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "sensor")]
    public class ClassMotionSensorFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassMotionSensor(preamble, classLabel);
            ClassMotionSensor.Hydrate(parent, reader, obj as ClassMotionSensor);
            return true;
        }
    }
    public class ClassMotionSensor : ClassPoweredBuilding
    {
        public ClassMotionSensor(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassMotionSensor? obj)
        {
            ClassPoweredBuilding.Hydrate(parent, reader, obj as ClassPoweredBuilding);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            Dehydrate(this, parent, writer, binary, save, preserveMalformations);
        }

        public static void Dehydrate(ClassMotionSensor obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            ClassPoweredBuilding.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
        }
    }
}
