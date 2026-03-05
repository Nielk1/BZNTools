using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone, "powerup")]
    public class ClassPowerUpFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassPowerUp(preamble, classLabel);
            ClassPowerUp.Hydrate(parent, reader, obj as ClassPowerUp);
            return true;
        }
    }
    public class ClassPowerUp : ClassGameObject
    {
        public ClassPowerUp(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassPowerUp? obj)
        {
            if (reader.Format == BZNFormat.Battlezone && parent.SaveType != SaveType.BZN)
            {
                // flags
            }
            ClassGameObject.Hydrate(parent, reader, obj as ClassGameObject);
        }
    }
}
