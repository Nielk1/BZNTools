using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone, "weaponmine")]
    [ObjectClass(BZNFormat.BattlezoneN64, "weaponmine")]
    [ObjectClass(BZNFormat.Battlezone2, "weaponmine")]
    public class ClassWeaponMineFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassWeaponMine(preamble, classLabel);
            ClassWeaponMine.Hydrate(parent, reader, obj as ClassWeaponMine);
            return true;
        }
    }
    public class ClassWeaponMine : ClassMine
    {
        public ClassWeaponMine(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassWeaponMine? obj)
        {
            if (reader.Format == BZNFormat.Battlezone2)
            {
                if (reader.Version >= 1144)
                {
                    IBZNToken tok;

                    tok = reader.ReadToken();
                    if (!tok.Validate("curAmmo", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse curAmmo/FLOAT");
                    if (obj != null) obj.curAmmo = (int)tok.GetSingle();

                    tok = reader.ReadToken();
                    if (!tok.Validate("maxAmmo", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse maxAmmo/FLOAT");
                    if (obj != null) obj.maxAmmo = (int)tok.GetSingle();

                    tok = reader.ReadToken();
                    if (!tok.Validate("addAmmo", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse addAmmo/FLOAT");
                    float addAmmo = (uint)tok.GetSingle();
                }
            }

            ClassMine.Hydrate(parent, reader, obj as ClassMine);
        }
    }
}
