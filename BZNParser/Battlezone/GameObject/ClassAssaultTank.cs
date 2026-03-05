using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "assaulttank")]
    public class ClassAssaultTankFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassAssaultTank(preamble, classLabel);
            ClassAssaultTank.Hydrate(parent, reader, obj as ClassAssaultTank);
            return true;
        }
    }
    public class ClassAssaultTank : ClassTrackedVehicle
    {
        public ClassAssaultTank(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassAssaultTank? obj)
        {
            if (parent.SaveType != SaveType.BZN)
            {
                // turret control
            }

            ClassTrackedVehicle.Hydrate(parent, reader, obj as ClassTrackedVehicle);
        }
    }
}
