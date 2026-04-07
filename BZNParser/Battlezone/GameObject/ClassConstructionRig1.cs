using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone, "constructionrig")]
    [ObjectClass(BZNFormat.BattlezoneN64, "constructionrig")]
    public class ClassConstructionRig1Factory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
            {
                obj = new ClassConstructionRig1(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                ClassConstructionRig1.Hydrate(parent, reader, obj as ClassConstructionRig1);
                return true;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
        }
    }
    public class ClassConstructionRig1 : ClassProducer
    {
        public Matrix dropMat { get; set; }
        public string dropClass { get; set; }
        public UInt32 lastRecycled { get; set; }

        public ClassConstructionRig1(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel)
        {
            dropMat = new Matrix();
            dropClass = string.Empty;
            lastRecycled = 0;
        }

        public override void ClearMalformations()
        {
            Malformations.Clear();
            dropMat.ClearMalformations();
            base.ClearMalformations();
        }

        public override void DisableMalformationAutoFix()
        {
            dropMat.DisableMalformationAutoFix();
            base.DisableMalformationAutoFix();
        }

        public override void EnableMalformationAutoFix()
        {
            dropMat.EnableMalformationAutoFix();
            base.EnableMalformationAutoFix();
        }


        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassConstructionRig1? obj)
        {
            IBZNToken? tok;

            if (reader.Format == BZNFormat.BattlezoneN64 || reader.Version > 1030)
            {
                reader.ReadMatrixOld("dropMat", obj, x => x.dropMat);

                if (reader.Format == BZNFormat.BattlezoneN64)
                {
                    tok = reader.ReadToken();
                    if (tok == null)
                        throw new Exception("Failed to parse dropClass/ID");
                    tok.ApplyUInt16(obj, x => x.dropClass, 0, (v) => parent.Hints?.EnumerationPrjID?[v] ?? string.Format("bzn64prjid_{0,4:X4}", v));
                }
                else
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("dropClass", BinaryFieldType.DATA_ID)) throw new Exception("Failed to parse dropClass/ID");
                    tok.ApplyID(obj, x => x.dropClass);
                }

                if (reader.Format == BZNFormat.Battlezone && reader.Version >= 2001)
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("lastRecycled", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse lastRecycled/LONG");
                    if (obj != null) obj.lastRecycled = tok.GetUInt32();
                }
            }
            else
            {
                if (obj != null) obj.dropMat = obj.transform; // matches source
            }

            ClassProducer.Hydrate(parent, reader, obj as ClassProducer);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassConstructionRig1 obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            if (writer.Format == BZNFormat.BattlezoneN64 || writer.Version > 1030)
            {
                writer.WriteMatrixOld("dropMat", obj, x => x.dropMat);

                if (writer.Format == BZNFormat.BattlezoneN64)
                {
                    writer.WriteUInt16("dropClass", obj, x => x.dropClass, (v) =>
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
                    writer.WriteID("dropClass", obj, x => x.dropClass);
                }

                if (writer.Format == BZNFormat.Battlezone && writer.Version >= 2001)
                {
                    writer.WriteUInt32("lastRecycled", obj, x => x.lastRecycled);
                }
            }
            else
            {
                if (obj != null) obj.dropMat = obj.transform; // matches source
            }

            ClassProducer.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
