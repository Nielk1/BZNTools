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
                obj = new ClassConstructionRig1(preamble, classLabel);
            ClassConstructionRig1.Hydrate(parent, reader, obj as ClassConstructionRig1);
            return true;
        }
    }
    public class ClassConstructionRig1 : ClassProducer
    {
        public Matrix dropMat { get; set; }
        public string dropClass { get; set; }
        public UInt32 lastRecycled { get; set; }

        public ClassConstructionRig1(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassConstructionRig1? obj)
        {
            IBZNToken tok;

            if (reader.Format == BZNFormat.BattlezoneN64 || reader.Version > 1030)
            {
                tok = reader.ReadToken();
                if (!tok.Validate("dropMat", BinaryFieldType.DATA_MAT3DOLD)) throw new Exception("Failed to parse dropMat/MAT3DOLD");
                if (obj != null)
                {
                    obj.dropMat = tok.GetMatrixOld();
                    MalformationExtensions.CheckMalformationsMatrix(tok, obj.dropMat.Malformations, reader.FloatFormat);
                }

                if (reader.Format == BZNFormat.BattlezoneN64)
                {
                    tok = reader.ReadToken();
                    UInt16 dropClassItemID = tok.GetUInt16();
                    if (obj != null) obj.dropClass = parent.Hints?.EnumerationPrjID?[dropClassItemID] ?? string.Format("bzn64prjid_{0,4:X4}", dropClassItemID);
                }
                else
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("dropClass", BinaryFieldType.DATA_ID)) throw new Exception("Failed to parse dropClass/ID");
                    if (obj != null) obj.dropClass = tok.GetString();
                }

                if (reader.Format == BZNFormat.Battlezone && reader.Version >= 2001)
                {
                    tok = reader.ReadToken();
                    //if (!tok.Validate("lastRecycled", BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse lastRecycled/FLOAT");
                    if (!tok.Validate("lastRecycled", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse lastRecycled/LONG");
                    if (obj != null) obj.lastRecycled = tok.GetUInt32();
                }
            }
            else
            {
                if (obj != null) obj.dropMat = obj.transform; // matches source
            }

            ClassProducer.Hydrate(parent, reader, obj as ClassProducer);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            Dehydrate(this, parent, writer, binary, save, preserveMalformations);
        }

        public static void Dehydrate(ClassConstructionRig1 obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            if (writer.Format == BZNFormat.BattlezoneN64 || writer.Version > 1030)
            {
                if (writer.Version >= 2010)
                {
                    writer.WriteMat3DOldEnhanceds("dropMat", preserveMalformations, obj.dropMat);
                }
                else
                {
                    writer.WriteMat3DOlds("dropMat", preserveMalformations, obj.dropMat);
                }

                if (writer.Format == BZNFormat.BattlezoneN64)
                {
                    if (obj.dropClass.StartsWith("bzn64prjid_"))
                    {
                        string possibleLabel = obj.dropClass.Substring("bzn64prjid_".Length);
                        if (ushort.TryParse(possibleLabel, System.Globalization.NumberStyles.HexNumber, null, out ushort possibleItemID))
                        {
                            writer.WriteUnsignedValues(null, possibleItemID);
                        }
                        else
                        {
                            throw new Exception("Failed to parse dropClass/ID");
                        }
                    }
                    else
                    {
                        var lookup = parent.Hints?.EnumerationPrjID;
                        if (lookup != null)
                        {
                            UInt16? key = lookup.Where(dr => dr.Value == obj.dropClass).Select(dr => dr.Key).FirstOrDefault();
                            if (key.HasValue)
                            {
                                writer.WriteUnsignedValues(null, key.Value);
                            }
                            else
                            {
                                throw new Exception("Failed to parse dropClass/ID");
                            }
                        }
                        else
                        {
                            throw new Exception("Failed to parse dropClass/ID");
                        }
                    }
                }
                else
                {
                    writer.WriteIDs("dropClass", obj.dropClass);
                }

                if (writer.Format == BZNFormat.Battlezone && writer.Version >= 2001)
                {
                    writer.WriteUnsignedValues("lastRecycled", obj.lastRecycled);
                }
            }
            else
            {
                if (obj != null) obj.dropMat = obj.transform; // matches source
            }

            ClassProducer.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
        }
    }
}
