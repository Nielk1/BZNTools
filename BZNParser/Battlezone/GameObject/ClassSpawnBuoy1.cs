using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone, "spawnpnt")]
    [ObjectClass(BZNFormat.BattlezoneN64, "spawnpnt")]
    public class ClassSpawnBuoy1Factory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassSpawnBuoy1(preamble, classLabel);
            ClassSpawnBuoy1.Hydrate(parent, reader, obj as ClassSpawnBuoy1);
            return true;
        }
    }
    public class ClassSpawnBuoy1 : ClassGameObject
    {
        public ClassSpawnBuoy1(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassSpawnBuoy1? obj)
        {
            ClassGameObject.Hydrate(parent, reader, obj as ClassGameObject);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            Dehydrate(this, parent, writer, binary, save, preserveMalformations);
        }

        public static void Dehydrate(ClassSpawnBuoy1 obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            ClassGameObject.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
        }
    }
}
