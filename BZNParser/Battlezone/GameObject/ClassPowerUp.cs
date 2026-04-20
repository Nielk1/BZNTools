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
            {
                obj = new ClassPowerUp(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                return ClassPowerUp.Hydrate(parent, reader, obj as ClassPowerUp).Success;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
        }
    }
    public class ClassPowerUp : ClassGameObject
    {
        public ClassPowerUp(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static ParseResult Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassPowerUp? obj)
        {
            if (reader.Format == BZNFormat.Battlezone && parent.SaveType != SaveType.BZN)
            {
                // flags
            }
            return ClassGameObject.Hydrate(parent, reader, obj as ClassGameObject);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassPowerUp obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            if (writer.Format == BZNFormat.Battlezone && parent.SaveType != SaveType.BZN)
            {
                // flags
            }
            ClassGameObject.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
