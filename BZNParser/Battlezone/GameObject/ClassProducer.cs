using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone, "producer")]
    public class ClassProducerFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
            {
                obj = new ClassProducer(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                return ClassProducer.Hydrate(parent, reader, obj as ClassProducer).Success;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
        }
    }
    public class ClassProducer : ClassHoverCraft
    {
        //public float setAltitude { get; set; }
        public float timeDeploy { get; set; }
        public float timeUndeploy { get; set; }
        public UInt32 powerSource { get; set; }
        //public byte[] state { get; set; }
        //public UInt32 state { get; set; }
        //public UInt32 delayTimer { get; set; }
        public float delayTimer { get; set; }
        public float nextRepair { get; set; }
        public string buildClass { get; set; }
        //public UInt32 buildDoneTime { get; set; }
        public float buildDoneTime { get; set; }


        // legacy?
        public Int32 buildCost { get; set;  }
        public float buildUpdateTime { get; set; }
        public float buildDt { get; set; }
        public Int32 buildDc { get; set; }


        public ClassProducer(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel)
        {
            //setAltitude = 0;
            timeDeploy = 0;
            timeUndeploy = 0;
            powerSource = 0;
            //state = new byte[4];
            //delayTimer = 0;
            delayTimer = 0;
            nextRepair = 0;
            buildClass = string.Empty;
            buildDoneTime = 0;
            buildCost = 0;
            buildUpdateTime = 0;
            buildDt = 0;
            buildDc = 0;
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


        public static ParseResult Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassProducer? obj)
        {
            IBZNToken? tok;

            if (reader.Format == BZNFormat.Battlezone && reader.Version < 1011)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("setAltitude", BinaryFieldType.DATA_FLOAT))
                    return ParseResult.Fail("Failed to parse setAltitude/FLOAT");
                tok.ApplySingle(obj, x => x.setAltitude, format: reader.FloatFormat);
            }

            if (reader.Format == BZNFormat.BattlezoneN64 || reader.Version != 1042)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("timeDeploy", BinaryFieldType.DATA_FLOAT))
                    return ParseResult.Fail("Failed to parse timeDeploy/FLOAT");
                tok.ApplySingle(obj, x => x.timeDeploy, format: reader.FloatFormat);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("timeUndeploy", BinaryFieldType.DATA_FLOAT))
                    return ParseResult.Fail("Failed to parse timeUndeploy/FLOAT");
                tok.ApplySingle(obj, x => x.timeUndeploy, format: reader.FloatFormat);
            }

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("undefptr", BinaryFieldType.DATA_PTR))
                return ParseResult.Fail("Failed to parse undefptr/PTR");
            tok.ApplyUInt32H8(obj, x => x.powerSource);

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("state", BinaryFieldType.DATA_VOID))
                return ParseResult.Fail("Failed to parse state/VOID");
            //state = tok.GetBytes(0, 4); // probably need to reverse for n64
            //if (obj != null) obj.state = (VEHICLE_STATE)tok.GetUInt32HR(); // probably need to reverse for n64
            tok.ApplyVoidBytes(obj, x => x.state, 0, (v) => (VEHICLE_STATE)BitConverter.ToUInt32(v.Concat(new byte[4]).Take(4).ToArray()));

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("delayTimer", BinaryFieldType.DATA_FLOAT))
                return ParseResult.Fail("Failed to parse delayTimer/FLOAT");
            tok.ApplySingle(obj, x => x.delayTimer, format: reader.FloatFormat);

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("nextRepair", BinaryFieldType.DATA_FLOAT))
                return ParseResult.Fail("Failed to parse nextRepair/FLOAT");
            tok.ApplySingle(obj, x => x.nextRepair, format: reader.FloatFormat);

            if (reader.Format == BZNFormat.BattlezoneN64 || reader.Version >= 1006)
            {
                if (reader.Format == BZNFormat.BattlezoneN64)
                {
                    tok = reader.ReadToken();
                    if (tok == null)
                        return ParseResult.Fail("Failed to parse buildClass/ID");
                    tok.ApplyUInt16(obj, x => x.buildClass, 0, (v) => parent.Hints?.EnumerationPrjID?[v] ?? string.Format("bzn64prjid_{0,4:X4}", v));
                }
                else
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("buildClass", BinaryFieldType.DATA_ID))
                        return ParseResult.Fail("Failed to parse buildClass/ID");
                    tok.ApplyID(obj, x => x.buildClass);
                }

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("buildDoneTime", BinaryFieldType.DATA_FLOAT))
                    return ParseResult.Fail("Failed to parse buildDoneTime/FLOAT");
                //if (obj != null)
                //{
                //    obj.buildDoneTime = tok.GetSingle();
                //    MalformationExtensions.CheckMalformationsSingle(tok, "buildDoneTime", obj.Malformations, reader.FloatFormat);
                //}
                tok.ApplySingle(obj, x => x.buildDoneTime, format: reader.FloatFormat);
                // BZn64 might be invalid when it has CDCDCDCA here.

                if (reader.Format == BZNFormat.Battlezone && reader.Version <= 1026)
                {
                    // dummied out and unused
                    tok = reader.ReadToken();//buildCost [1] =
                                             //-842150451 // CDCDCDCA
                    if (tok == null || !tok.Validate("buildCost", BinaryFieldType.DATA_LONG))
                        return ParseResult.Fail("Failed to parse buildCost/LONG");
                    tok.ApplyInt32(obj, x => x.buildCost);

                    tok = reader.ReadToken();//buildUpdateTime [1] =
                                             //-4.31602e+008
                    if (tok == null || !tok.Validate("buildUpdateTime", BinaryFieldType.DATA_FLOAT))
                        return ParseResult.Fail("Failed to parse buildUpdateTime/FLOAT");
                    tok.ApplySingle(obj, x => x.buildUpdateTime, format: reader.FloatFormat);


                    tok = reader.ReadToken();//buildDt [1] =
                                             //-4.31602e+008
                    if (tok == null || !tok.Validate("buildDt", BinaryFieldType.DATA_FLOAT))
                        return ParseResult.Fail("Failed to parse buildDt/FLOAT");
                    tok.ApplySingle(obj, x => x.buildDt, format: reader.FloatFormat);


                    tok = reader.ReadToken();//buildDc [1] =
                                             //-842150451 // CDCDCDCA
                    if (tok == null || !tok.Validate("buildDc", BinaryFieldType.DATA_LONG))
                        return ParseResult.Fail("Failed to parse buildDc/LONG");
                    tok.ApplyInt32(obj, x => x.buildDc);
                }
            }

            if (reader.Format == BZNFormat.Battlezone && reader.Version <= 1010)
            {
                return ClassCraft.Hydrate(parent, reader, obj as ClassCraft);
            }
            return ClassHoverCraft.Hydrate(parent, reader, obj as ClassHoverCraft);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassProducer obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            if (writer.Format == BZNFormat.Battlezone && writer.Version < 1011)
            {
                writer.WriteSingle("setAltitude", obj, x => x.setAltitude);
            }

            if (writer.Format == BZNFormat.BattlezoneN64 || writer.Version != 1042)
            {
                writer.WriteSingle("timeDeploy", obj, x => x.timeDeploy);
                writer.WriteSingle("timeUndeploy", obj, x => x.timeUndeploy);
            }

            writer.WritePtr("undefptr", obj, x => x.powerSource);
            writer.WriteVoidBytes("state", obj, x => x.state, (v) => BitConverter.GetBytes((UInt32)v));
            writer.WriteSingle("delayTimer", obj, x => x.delayTimer);
            writer.WriteSingle("nextRepair", obj, x => x.nextRepair);

            if (writer.Format == BZNFormat.BattlezoneN64 || writer.Version >= 1006)
            {
                if (writer.Format == BZNFormat.BattlezoneN64)
                {
                    writer.WriteUInt16("buildClass", obj, x => x.buildClass, (v) =>
                    {
                        if (v.StartsWith("bzn64prjid_"))
                        {
                            string possibleLabel = v.Substring("bzn64prjid_".Length);
                            if (ushort.TryParse(possibleLabel, System.Globalization.NumberStyles.HexNumber, null, out ushort possibleItemID))
                                return possibleItemID;
                        }
                        else
                        {
                            var lookup = parent.Hints?.EnumerationPrjID;
                            if (lookup != null)
                            {
                                UInt16? key = lookup.Where(dr => dr.Value == v.ToLowerInvariant()).Select(dr => dr.Key).FirstOrDefault();
                                if (key.HasValue)
                                {
                                    return key.Value;
                                }
                            }
                        }
                        throw new Exception("Failed to parse dropClass/ID");
                    });
                }
                else
                {
                    writer.WriteID("buildClass", obj, x => x.buildClass);
                }

                writer.WriteSingle("buildDoneTime", obj, x => x.buildDoneTime);
                // BZn64 might be invalid when it has CDCDCDCA here.

                if (writer.Format == BZNFormat.Battlezone && writer.Version <= 1026)
                {
                    // dummied out and unused
                    //tok = reader.ReadToken();//buildCost [1] =
                    //                         //-842150451 // CDCDCDCA
                    //tok = reader.ReadToken();//buildUpdateTime [1] =
                    //                         //-4.31602e+008
                    //tok = reader.ReadToken();//buildDt [1] =
                    //                         //-4.31602e+008
                    //tok = reader.ReadToken();//buildDc [1] =
                    //                         //-842150451 // CDCDCDCA

                    writer.WriteInt32("buildCost", obj, x => x.buildCost);
                    writer.WriteSingle("buildUpdateTime", obj, x => x.buildUpdateTime);
                    writer.WriteSingle("buildDt", obj, x => x.buildDt);
                    writer.WriteInt32("buildDc", obj, x => x.buildDc);
                }
            }

            if (writer.Format == BZNFormat.Battlezone && writer.Version <= 1010)
            {
                ClassCraft.Dehydrate(obj, parent, writer, binary, save);
                return;
            }
            ClassHoverCraft.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
