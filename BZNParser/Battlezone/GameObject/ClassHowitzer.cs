using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone, "howitzer")]
    [ObjectClass(BZNFormat.BattlezoneN64, "howitzer")]
    public class ClassHowitzerFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
            {
                obj = new ClassHowitzer(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                ClassHowitzer.Hydrate(parent, reader, obj as ClassHowitzer);
                return true;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
        }
    }
    public class ClassHowitzer : ClassTurretTank1
    {
        public ClassHowitzer(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassHowitzer? obj)
        {
            if (reader.Format == BZNFormat.Battlezone && reader.Version < 1020)
            {
                ClassHoverCraft.Hydrate(parent, reader, obj as ClassHoverCraft);
                return;
            }
            ClassTurretTank1.Hydrate(parent, reader, obj as ClassTurretTank1);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassHowitzer obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            if (writer.Format == BZNFormat.Battlezone && writer.Version < 1020)
            {
                ClassHoverCraft.Dehydrate(obj, parent, writer, binary, save);
                return;
            }
            ClassTurretTank1.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
