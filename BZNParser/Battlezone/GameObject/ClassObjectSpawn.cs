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
                obj = new ClassObjectSpawn(preamble, classLabel);
            ClassObjectSpawn.Hydrate(parent, reader, obj as ClassObjectSpawn);
            return true;
        }
    }
    public class ClassObjectSpawn : ClassBuilding
    {
        public Int32 spawnHandle { get; set; }
        public float spawnTimer { get; set; }

        public ClassObjectSpawn(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassObjectSpawn? obj)
        {
            IBZNToken? tok;

            if (reader.Format == BZNFormat.Battlezone2)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("spawnHandle", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse spawnHandle/LONG"); // type not confirmed
                tok.ApplyInt32(obj, x => x.spawnHandle);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("spawnTimer", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse spawnTimer/FLOAT"); // type not confirmed
                tok.ApplySingle(obj, x => x.spawnTimer);
            }

            ClassBuilding.Hydrate(parent, reader, obj as ClassBuilding);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            Dehydrate(this, parent, writer, binary, save, preserveMalformations);
        }

        public static void Dehydrate(ClassObjectSpawn obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            if (writer.Format == BZNFormat.Battlezone2)
            {
                writer.WriteInt32("spawnHandle", obj, x => x.spawnHandle); // value not confirmed
                writer.WriteSingle("spawnTimer", obj, x => x.spawnTimer); // value not confirmed
            }

            ClassBuilding.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
        }
    }
}
