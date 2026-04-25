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
            {
                obj = new ClassWeaponMine(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                return ClassWeaponMine.Hydrate(parent, reader, obj as ClassWeaponMine).Success;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
        }
    }
    public class ClassWeaponMine : ClassMine
    {
        public ClassWeaponMine(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static ParseResult Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassWeaponMine? obj)
        {
            if (reader.Format == BZNFormat.Battlezone2)
            {
                if (reader.Version >= 1144)
                {
                    IBZNToken? tok;

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("curAmmo", BinaryFieldType.DATA_FLOAT))
                        return ParseResult.Fail("Failed to parse curAmmo/FLOAT");
                    tok.ApplySingle(obj, x => x.curAmmo, format: reader.FloatFormat);

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("maxAmmo", BinaryFieldType.DATA_FLOAT))
                        return ParseResult.Fail("Failed to parse maxAmmo/FLOAT");
                    tok.ApplySingle(obj, x => x.maxAmmo, format: reader.FloatFormat);

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("addAmmo", BinaryFieldType.DATA_FLOAT))
                        return ParseResult.Fail("Failed to parse addAmmo/FLOAT");
                    tok.ApplySingle(obj, x => x.addAmmo, format: reader.FloatFormat);
                }
            }

            return ClassMine.Hydrate(parent, reader, obj as ClassMine);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassWeaponMine obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            if (writer.Format == BZNFormat.Battlezone2)
            {
                if (writer.Version >= 1144)
                {
                    writer.WriteSingle("curAmmo", obj, x => x.curAmmo);
                    writer.WriteSingle("maxAmmo", obj, x => x.maxAmmo);
                    writer.WriteSingle("addAmmo", obj, x => x.addAmmo);
                }
            }

            ClassMine.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
