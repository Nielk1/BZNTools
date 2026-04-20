using BZNParser.Tokenizer;
using System.Numerics;
using System.Reflection.PortableExecutable;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone, "torpedo")]
    [ObjectClass(BZNFormat.BattlezoneN64, "torpedo")]
    public class ClassTorpedo1Factory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
            {
                obj = new ClassTorpedo1(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                return ClassTorpedo1.Hydrate(parent, reader, obj as ClassTorpedo1).Success;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
        }
    }
    public class ClassTorpedo1 : ClassPowerUp
    {
        // legacy, guess based on similar legac craft data
        public UInt32 energy0current { get; set; }
        public UInt32 energy0maximum { get; set; }
        public UInt32 energy1current { get; set; }
        public UInt32 energy1maximum { get; set; }
        public UInt32 energy2current { get; set; }
        public UInt32 energy2maximum { get; set; }
        public Vector3D[]? bumpers { get; set; }




        public Int32 abandoned { get; set; } // only used in older code paths where Torpedo is actually Craft based

        public ClassTorpedo1(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel)
        {
            energy0current = 0;
            energy0maximum = 0;
            energy1current = 0;
            energy1maximum = 0;
            energy2current = 0;
            energy2maximum = 0;
            bumpers = new Vector3D[6];
            for (int i = 0; i < bumpers.Length; i++)
                bumpers[i] = new Vector3D();

            abandoned = 0;
        }

        public override void ClearMalformations()
        {
            Malformations.Clear();
            base.ClearMalformations();
        }

        public override void DisableMalformationAutoFix()
        {
            base.DisableMalformationAutoFix();
        }

        public override void EnableMalformationAutoFix()
        {
            base.EnableMalformationAutoFix();
        }


        public static ParseResult Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassTorpedo1? obj)
        {
            if (reader.Format == BZNFormat.Battlezone)
            {
                if (reader.Version < 1031)
                {
                    if (reader.Version < 1019)
                    {
                        IBZNToken? tok;

                        tok = reader.ReadToken(); // energy0current
                        if (tok == null || !tok.Validate("energy0current", BinaryFieldType.DATA_LONG))
                            return ParseResult.Fail("Failed to parse energy0current/LONG");
                        tok.ApplyUInt32(obj, x => x.energy0current);

                        tok = reader.ReadToken(); // energy0maximum
                        if (tok == null || !tok.Validate("energy0maximum", BinaryFieldType.DATA_LONG))
                            return ParseResult.Fail("Failed to parse energy0maximum/LONG");
                        tok.ApplyUInt32(obj, x => x.energy0maximum);

                        tok = reader.ReadToken(); // energy1current
                        if (tok == null || !tok.Validate("energy1current", BinaryFieldType.DATA_LONG))
                            return ParseResult.Fail("Failed to parse energy1current/LONG");
                        tok.ApplyUInt32(obj, x => x.energy1current);

                        tok = reader.ReadToken(); // energy1maximum
                        if (tok == null || !tok.Validate("energy1maximum", BinaryFieldType.DATA_LONG))
                            return ParseResult.Fail("Failed to parse energy1maximum/LONG");
                        tok.ApplyUInt32(obj, x => x.energy1maximum);

                        tok = reader.ReadToken(); // energy2current
                        if (tok == null || !tok.Validate("energy2current", BinaryFieldType.DATA_LONG))
                            return ParseResult.Fail("Failed to parse energy2current/LONG");
                        tok.ApplyUInt32(obj, x => x.energy2current);

                        tok = reader.ReadToken(); // energy2maximum
                        if (tok == null || !tok.Validate("energy2maximum", BinaryFieldType.DATA_LONG))
                            return ParseResult.Fail("Failed to parse energy2maximum/LONG");
                        tok.ApplyUInt32(obj, x => x.energy2maximum);

                        tok = reader.ReadToken(); // bumpers
                        if (tok == null || !tok.Validate("bumpers", BinaryFieldType.DATA_VEC3D) || tok.GetCount() != 6)
                            return ParseResult.Fail("Failed to parse energy2maximum/LONG");

                        if (obj != null)
                        {
                            obj.bumpers = new Vector3D[6];
                            for (int i = 0; i < 6; i++)
                                obj.bumpers[i] = tok.GetVector3D(i);
                        }
                    }
                    else if (reader.Version > 1027)
                    {
                        // read in abandoned flag
                        IBZNToken? tok;
                        tok = reader.ReadToken();
                        if (tok == null || !tok.Validate("abandoned", BinaryFieldType.DATA_LONG))
                            return ParseResult.Fail("Failed to parse abandoned/LONG");
                        if (tok.GetCount(BinaryFieldType.DATA_LONG) != 1)
                            return ParseResult.Fail("Failed to parse abandoned/LONG (wrong entry count)");
                        tok.ApplyInt32(obj, x => x.abandoned);
                    }
                }
            }

            if (reader.Format == BZNFormat.Battlezone && reader.Version < 1031)
            {
                return ClassGameObject.Hydrate(parent, reader, obj as ClassGameObject);
            }
            return ClassPowerUp.Hydrate(parent, reader, obj as ClassPowerUp);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassTorpedo1 obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            if (writer.Version < 1031)
            {
                if (writer.Version < 1019)
                {
                    writer.WriteUInt32("energy0current", obj, x => x.energy0current);
                    writer.WriteUInt32("energy0maximum", obj, x => x.energy0maximum);
                    writer.WriteUInt32("energy1current", obj, x => x.energy1current);
                    writer.WriteUInt32("energy1maximum", obj, x => x.energy1maximum);
                    writer.WriteUInt32("energy2current", obj, x => x.energy2current);
                    writer.WriteUInt32("energy2maximum", obj, x => x.energy2maximum);

                    writer.WriteVector3D("bumpers", obj, x => x.bumpers);
                }
                else if (writer.Version > 1027)
                {
                    writer.WriteInt32("abandoned", obj, x => x.abandoned);
                }
            }

            if (writer.Format == BZNFormat.Battlezone && writer.Version < 1031)
            {
                ClassGameObject.Dehydrate(obj, parent, writer, binary, save);
                return;
            }
            ClassPowerUp.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
