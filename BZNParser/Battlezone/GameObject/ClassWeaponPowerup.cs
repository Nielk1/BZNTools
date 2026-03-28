using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone, "wpnpower")]
    [ObjectClass(BZNFormat.BattlezoneN64, "wpnpower")]
    [ObjectClass(BZNFormat.Battlezone2, "wpnpower")]
    public class ClassWeaponPowerupFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassWeaponPowerup(preamble, classLabel);
            ClassWeaponPowerup.Hydrate(parent, reader, obj as ClassWeaponPowerup);
            return true;
        }
    }
    public class ClassWeaponPowerup : ClassPowerUp
    {
        public ClassWeaponPowerup(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassWeaponPowerup? obj)
        {
            ClassPowerUp.Hydrate(parent, reader, obj as ClassPowerUp);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassWeaponPowerup obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            ClassPowerUp.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
