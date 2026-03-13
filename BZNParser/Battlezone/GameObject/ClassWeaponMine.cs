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
                    IBZNToken? tok;

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("curAmmo", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse curAmmo/FLOAT");
                    if (obj != null) obj.curAmmo = new DualModeValue<int, float>(tok.GetSingle());

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("maxAmmo", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse maxAmmo/FLOAT");
                    if (obj != null) obj.maxAmmo = new DualModeValue<int, float>(tok.GetSingle());

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("addAmmo", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse addAmmo/FLOAT");
                    if (obj != null) obj.addAmmo = new DualModeValue<int, float>(tok.GetSingle());
                }
            }

            ClassMine.Hydrate(parent, reader, obj as ClassMine);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            Dehydrate(this, parent, writer, binary, save, preserveMalformations);
        }

        public static void Dehydrate(ClassWeaponMine obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            if (writer.Format == BZNFormat.Battlezone2)
            {
                if (writer.Version >= 1144)
                {
                    writer.WriteFloats("curAmmo", preserveMalformations ? obj.Malformations : null, obj.curAmmo.Get<Single>());
                    writer.WriteFloats("maxAmmo", preserveMalformations ? obj.Malformations : null, obj.maxAmmo.Get<Single>());
                    writer.WriteFloats("addAmmo", preserveMalformations ? obj.Malformations : null, obj.addAmmo.Get<Single>());
                }
            }

            ClassMine.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
        }
    }
}
