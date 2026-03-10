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
        // temporary?
        public float setAltitude { get; set; }
        public float accelDragStop { get; set; }
        public float accelDragFull { get; set; }
        public float alphaTrack { get; set; }
        public float alphaDamp { get; set; }
        public float pitchPitch { get; set; }
        public float pitchThrust { get; set; }
        public float rollStrafe { get; set; }
        public float rollSteer { get; set; }
        public float velocForward { get; set; }
        public float velocReverse { get; set; }
        public float velocStrafe { get; set; }
        public float accelThrust { get; set; }
        public float accelBrake { get; set; }
        public float omegaSpin { get; set; }
        public float omegaTurn { get; set; }
        public float alphaSteer { get; set; }
        public float accelJump { get; set; }
        public float thrustRatio { get; set; }
        public float throttle { get; set; }
        public float airBorne { get; set; }

        public ClassHoverCraft(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassHoverCraft? obj)
        {
            if (reader.Format == BZNFormat.Battlezone && reader.Version > 1001 && reader.Version < 1026)
            {
                IBZNToken tok;

                tok = reader.ReadToken();
                if (!tok.Validate("setAltitude", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse setAltitude/FLOAT");
                if (obj != null) obj.setAltitude = tok.GetSingle();

                tok = reader.ReadToken();
                if (!tok.Validate("accelDragStop", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse accelDragStop/FLOAT");
                if (obj != null) obj.accelDragStop = tok.GetSingle();

                tok = reader.ReadToken();
                if (!tok.Validate("accelDragFull", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse accelDragFull/FLOAT");
                if (obj != null) obj.accelDragFull = tok.GetSingle();

                tok = reader.ReadToken();
                if (!tok.Validate("alphaTrack", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse alphaTrack/FLOAT");
                if (obj != null) obj.alphaTrack = tok.GetSingle();

                tok = reader.ReadToken();
                if (!tok.Validate("alphaDamp", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse alphaDamp/FLOAT");
                if (obj != null) obj.alphaDamp = tok.GetSingle();

                tok = reader.ReadToken();
                if (!tok.Validate("pitchPitch", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse pitchPitch/FLOAT");
                if (obj != null) obj.pitchPitch = tok.GetSingle();

                tok = reader.ReadToken();
                if (!tok.Validate("pitchThrust", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse pitchThrust/FLOAT");
                if (obj != null) obj.pitchThrust = tok.GetSingle();

                tok = reader.ReadToken();
                if (!tok.Validate("rollStrafe", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse rollStrafe/FLOAT");
                if (obj != null) obj.rollStrafe = tok.GetSingle();

                tok = reader.ReadToken();
                if (!tok.Validate("rollSteer", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse rollSteer/FLOAT");
                if (obj != null) obj.rollSteer = tok.GetSingle();

                tok = reader.ReadToken();
                if (!tok.Validate("velocForward", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse velocForward/FLOAT");
                if (obj != null) obj.velocForward = tok.GetSingle();

                tok = reader.ReadToken();
                if (!tok.Validate("velocReverse", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse velocReverse/FLOAT");
                if (obj != null) obj.velocReverse = tok.GetSingle();

                tok = reader.ReadToken();
                if (!tok.Validate("velocStrafe", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse velocStrafe/FLOAT");
                if (obj != null) obj.velocStrafe = tok.GetSingle();

                tok = reader.ReadToken();
                if (!tok.Validate("accelThrust", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse accelThrust/FLOAT");
                if (obj != null) obj.accelThrust = tok.GetSingle();

                tok = reader.ReadToken();
                if (!tok.Validate("accelBrake", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse accelBrake/FLOAT");
                if (obj != null) obj.accelBrake = tok.GetSingle();

                tok = reader.ReadToken();
                if (!tok.Validate("omegaSpin", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse omegaSpin/FLOAT");
                if (obj != null) obj.omegaSpin = tok.GetSingle();

                tok = reader.ReadToken();
                if (!tok.Validate("omegaTurn", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse omegaTurn/FLOAT");
                if (obj != null) obj.omegaTurn = tok.GetSingle();

                tok = reader.ReadToken();
                if (!tok.Validate("alphaSteer", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse alphaSteer/FLOAT");
                if (obj != null) obj.alphaSteer = tok.GetSingle();

                tok = reader.ReadToken();
                if (!tok.Validate("accelJump", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse accelJump/FLOAT");
                if (obj != null) obj.accelJump = tok.GetSingle();

                tok = reader.ReadToken();
                if (!tok.Validate("thrustRatio", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse thrustRatio/FLOAT");
                if (obj != null) obj.thrustRatio = tok.GetSingle();

                tok = reader.ReadToken();
                    if (!tok.Validate("throttle", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse throttle/FLOAT");
                if (obj != null) obj.throttle = tok.GetSingle();

                tok = reader.ReadToken();
                    if (!tok.Validate("airBorne", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse airBorne/FLOAT");
                if (obj != null) obj.airBorne = tok.GetSingle();
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
                writer.WriteFloats("setAltitude", obj.setAltitude);
                writer.WriteFloats("accelDragStop", obj.accelDragStop);
                writer.WriteFloats("accelDragFull", obj.accelDragFull);
                writer.WriteFloats("alphaTrack", obj.alphaTrack);
                writer.WriteFloats("alphaDamp", obj.alphaDamp);
                writer.WriteFloats("pitchPitch", obj.pitchPitch);
                writer.WriteFloats("pitchThrust", obj.pitchThrust);
                writer.WriteFloats("rollStrafe", obj.rollStrafe);
                writer.WriteFloats("rollSteer", obj.rollSteer);
                writer.WriteFloats("velocForward", obj.velocForward);
                writer.WriteFloats("velocReverse", obj.velocReverse);
                writer.WriteFloats("velocStrafe", obj.velocStrafe);
                writer.WriteFloats("accelThrust", obj.accelThrust);
                writer.WriteFloats("accelBrake", obj.accelBrake);
                writer.WriteFloats("omegaSpin", obj.omegaSpin);
                writer.WriteFloats("omegaTurn", obj.omegaTurn);
                writer.WriteFloats("alphaSteer", obj.alphaSteer);
                writer.WriteFloats("accelJump", obj.accelJump);
                writer.WriteFloats("thrustRatio", obj.thrustRatio);
                writer.WriteFloats("throttle", obj.throttle);
                writer.WriteFloats("airBorne", obj.airBorne);
            }

            ClassCraft.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
        }
    }
}
