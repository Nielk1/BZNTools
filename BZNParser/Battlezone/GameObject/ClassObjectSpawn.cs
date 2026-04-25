using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "objectspawn")]
    public class ClassObjectSpawnFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
            {
                obj = new ClassObjectSpawn(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                return ClassObjectSpawn.Hydrate(parent, reader, obj as ClassObjectSpawn).Success;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
        }
    }
    public class ClassObjectSpawn : ClassBuilding
    {
        public Int32 spawnHandle { get; set; }
        public float spawnTimer { get; set; }

        public ClassObjectSpawn(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel)
        {
            spawnHandle = 0;
            spawnTimer = 0;
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


        public static ParseResult Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassObjectSpawn? obj)
        {
            IBZNToken? tok;

            if (reader.Format == BZNFormat.Battlezone2)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("spawnHandle", BinaryFieldType.DATA_LONG))
                    return ParseResult.Fail("Failed to parse spawnHandle/LONG"); // type not confirmed
                tok.ApplyInt32(obj, x => x.spawnHandle);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("spawnTimer", BinaryFieldType.DATA_FLOAT))
                    return ParseResult.Fail("Failed to parse spawnTimer/FLOAT"); // type not confirmed
                tok.ApplySingle(obj, x => x.spawnTimer, format: reader.FloatFormat);
            }

            return ClassBuilding.Hydrate(parent, reader, obj as ClassBuilding);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassObjectSpawn obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            if (writer.Format == BZNFormat.Battlezone2)
            {
                writer.WriteInt32("spawnHandle", obj, x => x.spawnHandle); // value not confirmed
                writer.WriteSingle("spawnTimer", obj, x => x.spawnTimer); // value not confirmed
            }

            ClassBuilding.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
