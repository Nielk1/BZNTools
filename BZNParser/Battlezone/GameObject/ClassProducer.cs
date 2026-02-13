using BZNParser.Reader;
using System;
using System.Collections.Generic;
using System.Linq;
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
                obj = new ClassProducer(preamble, classLabel);
            ClassProducer.Hydrate(parent, reader, obj as ClassProducer);
            return true;
        }
    }
    public class ClassProducer : ClassHoverCraft
    {
        public float timeDeploy { get; set; }
        public float timeUndeploy { get; set; }
        public UInt32 undefptr2 { get; set; }
        //public byte[] state { get; set; }
        public UInt32 state { get; set; }
        //public UInt32 delayTimer { get; set; }
        public float delayTimer { get; set; }
        public float nextRepair { get; set; }
        public string buildClass { get; set; }
        //public UInt32 buildDoneTime { get; set; }
        public float buildDoneTime { get; set; }

        public ClassProducer(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassProducer? obj)
        {
            IBZNToken tok;

            if (reader.Format == BZNFormat.Battlezone && reader.Version < 1011)
            {
                tok = reader.ReadToken();
                if (!tok.Validate("setAltitude", BinaryFieldType.DATA_FLOAT))
                    throw new Exception("Failed to parse setAltitude/FLOAT");
                float setAltitude = tok.GetSingle();
            }

            if (reader.Format == BZNFormat.BattlezoneN64 || reader.Version != 1042)
            {
                tok = reader.ReadToken();
                if (!tok.Validate("timeDeploy", BinaryFieldType.DATA_FLOAT))
                    throw new Exception("Failed to parse timeDeploy/FLOAT");
                if (obj != null) obj.timeDeploy = tok.GetSingle();

                tok = reader.ReadToken();
                if (!tok.Validate("timeUndeploy", BinaryFieldType.DATA_FLOAT))
                    throw new Exception("Failed to parse timeUndeploy/FLOAT");
                if (obj != null) obj.timeUndeploy = tok.GetSingle();
            }

            tok = reader.ReadToken();
            if (!tok.Validate("undefptr", BinaryFieldType.DATA_PTR))
                throw new Exception("Failed to parse undefptr/PTR");
            if (obj != null) obj.undefptr2 = tok.GetUInt32H(); // powerSource

            tok = reader.ReadToken();
            if (!tok.Validate("state", BinaryFieldType.DATA_VOID))
                throw new Exception("Failed to parse state/VOID");
            //state = tok.GetBytes(0, 4); // probably need to reverse for n64
            if (obj != null) obj.state = tok.GetUInt32(); // probably need to reverse for n64

            tok = reader.ReadToken();
            if (!tok.Validate("delayTimer", BinaryFieldType.DATA_FLOAT))
                throw new Exception("Failed to parse delayTimer/FLOAT");
            if (obj != null) obj.delayTimer = tok.GetSingle();//tok.GetUInt32();

            tok = reader.ReadToken();
            if (!tok.Validate("nextRepair", BinaryFieldType.DATA_FLOAT))
                throw new Exception("Failed to parse nextRepair/FLOAT");
            if (obj != null) obj.nextRepair = tok.GetSingle();

            if (reader.Format == BZNFormat.BattlezoneN64 || reader.Version >= 1006)
            {
                if (reader.Format == BZNFormat.BattlezoneN64)
                {
                    tok = reader.ReadToken();
                    UInt16 buildClassItemID = tok.GetUInt16();
                    if (obj != null) obj.buildClass = parent?.Hints?.EnumerationPrjID?[buildClassItemID] ?? string.Format("bzn64prjid_{0,4:X4}", buildClassItemID);
                }
                else
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("buildClass", BinaryFieldType.DATA_ID))
                        throw new Exception("Failed to parse buildClass/ID");
                    if (obj != null) obj.buildClass = tok.GetString();
                }

                tok = reader.ReadToken();
                if (!tok.Validate("buildDoneTime", BinaryFieldType.DATA_FLOAT))
                    throw new Exception("Failed to parse buildDoneTime/FLOAT");
                if (obj != null) obj.buildDoneTime = tok.GetSingle();
                // BZn64 might be invalid when it has CDCDCDCA here.

                if (reader.Format == BZNFormat.Battlezone && reader.Version <= 1026)
                {
                    // dummied out and unused
                    tok = reader.ReadToken();//buildCost [1] =
                                             //-842150451
                    tok = reader.ReadToken();//buildUpdateTime [1] =
                                             //-4.31602e+008
                    tok = reader.ReadToken();//buildDt [1] =
                                             //-4.31602e+008
                    tok = reader.ReadToken();//buildDc [1] =
                                             //-842150451
                }
            }

            if (reader.Format == BZNFormat.Battlezone && reader.Version <= 1010)
            {
                ClassCraft.Hydrate(parent, reader, obj as ClassCraft);
                return;
            }
            ClassHoverCraft.Hydrate(parent, reader, obj as ClassHoverCraft);
        }
    }
}
