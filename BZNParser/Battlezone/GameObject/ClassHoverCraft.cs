using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone, "hover")]
    public class ClassHoverCraftFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassHoverCraft(preamble, classLabel);
            ClassHoverCraft.Hydrate(parent, reader, obj as ClassHoverCraft);
            return true;
        }
    }
    public class ClassHoverCraft : ClassCraft
    {
        public ClassHoverCraft(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassHoverCraft? obj)
        {
            if (reader.Format == BZNFormat.Battlezone && reader.Version > 1001 && reader.Version < 1026)
            {
                IBZNToken tok = reader.ReadToken(); // TODO is this line an error?
                tok = reader.ReadToken(); // accelDragStop
                tok = reader.ReadToken(); // accelDragFull
                tok = reader.ReadToken(); // alphaTrack
                tok = reader.ReadToken(); // alphaDamp
                tok = reader.ReadToken(); // pitchPitch
                tok = reader.ReadToken(); // pitchThrust
                tok = reader.ReadToken(); // rollStrafe
                tok = reader.ReadToken(); // rollSteer
                tok = reader.ReadToken(); // velocForward
                tok = reader.ReadToken(); // velocReverse
                tok = reader.ReadToken(); // velocStrafe
                tok = reader.ReadToken(); // accelThrust
                tok = reader.ReadToken(); // accelBrake
                tok = reader.ReadToken(); // omegaSpin
                tok = reader.ReadToken(); // omegaTurn
                tok = reader.ReadToken(); // alphaSteer
                tok = reader.ReadToken(); // accelJump
                tok = reader.ReadToken(); // thrustRatio
                tok = reader.ReadToken(); // throttle
                tok = reader.ReadToken(); // airBorne
            }

            ClassCraft.Hydrate(parent, reader, obj as ClassCraft);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            Dehydrate(this, parent, writer, binary, save, preserveMalformations);
        }

        public static void Dehydrate(ClassHoverCraft obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            if (writer.Format == BZNFormat.Battlezone && writer.Version > 1001 && writer.Version < 1026)
            {

            }

            ClassCraft.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
        }
    }
}
