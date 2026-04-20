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
            {
                obj = new ClassHoverCraft(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                return ClassHoverCraft.Hydrate(parent, reader, obj as ClassHoverCraft).Success;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
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

        public ClassHoverCraft(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel)
        {
            setAltitude = 0;
            accelDragStop = 0;
            accelDragFull = 0;
            alphaTrack = 0;
            alphaDamp = 0;
            pitchPitch = 0;
            pitchThrust = 0;
            rollStrafe = 0;
            rollSteer = 0;
            velocForward = 0;
            velocReverse = 0;
            velocStrafe = 0;
            accelThrust = 0;
            accelBrake = 0;
            omegaSpin = 0;
            omegaTurn = 0;
            alphaSteer = 0;
            accelJump = 0;
            thrustRatio = 0;
            throttle = 0;
            airBorne = 0;
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


        public static ParseResult Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassHoverCraft? obj)
        {
            if (reader.Format == BZNFormat.Battlezone && reader.Version > 1001 && reader.Version < 1026)
            {
                IBZNToken? tok;

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("setAltitude", BinaryFieldType.DATA_FLOAT))
                    return ParseResult.Fail("Failed to parse setAltitude/FLOAT");
                tok.ApplySingle(obj, x => x.setAltitude);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("accelDragStop", BinaryFieldType.DATA_FLOAT))
                    return ParseResult.Fail("Failed to parse accelDragStop/FLOAT");
                tok.ApplySingle(obj, x => x.accelDragStop);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("accelDragFull", BinaryFieldType.DATA_FLOAT))
                    return ParseResult.Fail("Failed to parse accelDragFull/FLOAT");
                tok.ApplySingle(obj, x => x.accelDragFull);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("alphaTrack", BinaryFieldType.DATA_FLOAT))
                    return ParseResult.Fail("Failed to parse alphaTrack/FLOAT");
                tok.ApplySingle(obj, x => x.alphaTrack);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("alphaDamp", BinaryFieldType.DATA_FLOAT))
                    return ParseResult.Fail("Failed to parse alphaDamp/FLOAT");
                tok.ApplySingle(obj, x => x.alphaDamp);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("pitchPitch", BinaryFieldType.DATA_FLOAT))
                    return ParseResult.Fail("Failed to parse pitchPitch/FLOAT");
                tok.ApplySingle(obj, x => x.pitchPitch);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("pitchThrust", BinaryFieldType.DATA_FLOAT))
                    return ParseResult.Fail("Failed to parse pitchThrust/FLOAT");
                tok.ApplySingle(obj, x => x.pitchThrust);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("rollStrafe", BinaryFieldType.DATA_FLOAT))
                    return ParseResult.Fail("Failed to parse rollStrafe/FLOAT");
                tok.ApplySingle(obj, x => x.rollStrafe);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("rollSteer", BinaryFieldType.DATA_FLOAT))
                    return ParseResult.Fail("Failed to parse rollSteer/FLOAT");
                tok.ApplySingle(obj, x => x.rollSteer);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("velocForward", BinaryFieldType.DATA_FLOAT))
                    return ParseResult.Fail("Failed to parse velocForward/FLOAT");
                tok.ApplySingle(obj, x => x.velocForward);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("velocReverse", BinaryFieldType.DATA_FLOAT))
                    return ParseResult.Fail("Failed to parse velocReverse/FLOAT");
                tok.ApplySingle(obj, x => x.velocReverse);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("velocStrafe", BinaryFieldType.DATA_FLOAT))
                    return ParseResult.Fail("Failed to parse velocStrafe/FLOAT");
                tok.ApplySingle(obj, x => x.velocStrafe);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("accelThrust", BinaryFieldType.DATA_FLOAT))
                    return ParseResult.Fail("Failed to parse accelThrust/FLOAT");
                tok.ApplySingle(obj, x => x.accelThrust);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("accelBrake", BinaryFieldType.DATA_FLOAT))
                    return ParseResult.Fail("Failed to parse accelBrake/FLOAT");
                tok.ApplySingle(obj, x => x.accelBrake);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("omegaSpin", BinaryFieldType.DATA_FLOAT))
                    return ParseResult.Fail("Failed to parse omegaSpin/FLOAT");
                tok.ApplySingle(obj, x => x.omegaSpin);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("omegaTurn", BinaryFieldType.DATA_FLOAT))
                    return ParseResult.Fail("Failed to parse omegaTurn/FLOAT");
                tok.ApplySingle(obj, x => x.omegaTurn);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("alphaSteer", BinaryFieldType.DATA_FLOAT))
                    return ParseResult.Fail("Failed to parse alphaSteer/FLOAT");
                tok.ApplySingle(obj, x => x.alphaSteer);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("accelJump", BinaryFieldType.DATA_FLOAT))
                    return ParseResult.Fail("Failed to parse accelJump/FLOAT");
                tok.ApplySingle(obj, x => x.accelJump);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("thrustRatio", BinaryFieldType.DATA_FLOAT))
                    return ParseResult.Fail("Failed to parse thrustRatio/FLOAT");
                tok.ApplySingle(obj, x => x.thrustRatio);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("throttle", BinaryFieldType.DATA_FLOAT))
                    return ParseResult.Fail("Failed to parse throttle/FLOAT");
                tok.ApplySingle(obj, x => x.throttle);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("airBorne", BinaryFieldType.DATA_FLOAT))
                    return ParseResult.Fail("Failed to parse airBorne/FLOAT");
                tok.ApplySingle(obj, x => x.airBorne);
            }

            return ClassCraft.Hydrate(parent, reader, obj as ClassCraft);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassHoverCraft obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            if (writer.Format == BZNFormat.Battlezone && writer.Version > 1001 && writer.Version < 1026)
            {
                writer.WriteSingle("setAltitude", obj, x => x.setAltitude);
                writer.WriteSingle("accelDragStop", obj, x => x.accelDragStop);
                writer.WriteSingle("accelDragFull", obj, x => x.accelDragFull);
                writer.WriteSingle("alphaTrack", obj, x => x.alphaTrack);
                writer.WriteSingle("alphaDamp", obj, x => x.alphaDamp);
                writer.WriteSingle("pitchPitch", obj, x => x.pitchPitch);
                writer.WriteSingle("pitchThrust", obj, x => x.pitchThrust);
                writer.WriteSingle("rollStrafe", obj, x => x.rollStrafe);
                writer.WriteSingle("rollSteer", obj, x => x.rollSteer);
                writer.WriteSingle("velocForward", obj, x => x.velocForward);
                writer.WriteSingle("velocReverse", obj, x => x.velocReverse);
                writer.WriteSingle("velocStrafe", obj, x => x.velocStrafe);
                writer.WriteSingle("accelThrust", obj, x => x.accelThrust);
                writer.WriteSingle("accelBrake", obj, x => x.accelBrake);
                writer.WriteSingle("omegaSpin", obj, x => x.omegaSpin);
                writer.WriteSingle("omegaTurn", obj, x => x.omegaTurn);
                writer.WriteSingle("alphaSteer", obj, x => x.alphaSteer);
                writer.WriteSingle("accelJump", obj, x => x.accelJump);
                writer.WriteSingle("thrustRatio", obj, x => x.thrustRatio);
                writer.WriteSingle("throttle", obj, x => x.throttle);
                writer.WriteSingle("airBorne", obj, x => x.airBorne);
            }

            ClassCraft.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
