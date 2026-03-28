using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using static BZNParser.Tokenizer.BZNStreamReader;

namespace BZNParser.Battlezone
{
    public class AiPath : IMalformable
    {
        public SizedString? AiPathDummy { get; set; } // only needed to preserve malformation trash on this field if it exists, as its value is hard-coded as "AiPath"
        public UInt32? sObject { get; set; }
        public SizedString label { get; set; }
        public int pointCount { get; set; } // this is an override of points.length, need to decide how to handle that in malformations
        public Vector2D[] points { get; set; }
        public UInt32 pathType { get; set; }


        private readonly IMalformable.MalformationManager _malformationManager;
        public IMalformable.MalformationManager Malformations => _malformationManager;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public AiPath()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            this._malformationManager = new IMalformable.MalformationManager(this);
        }
        public void ClearMalformations()
        {
            AiPathDummy?.ClearMalformations();
            label?.ClearMalformations();
            Malformations.Clear();
        }

        public static bool Create(BZNFileBattlezone parent, BZNStreamReader reader, int countPaths, int countLeft, out AiPath? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new AiPath();
            AiPath.Hydrate(parent, reader, countPaths, countLeft, obj);
            return true;
        }

        public static bool Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, int countPaths, int countLeft, AiPath? obj)
        {
            IBZNToken? tok;

            if (!reader.InBinary && reader.Format == BZNFormat.Battlezone)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.IsValidationOnly() || !tok.Validate("AiPath", BinaryFieldType.DATA_UNKNOWN))
                    throw new Exception("Failed to parse [AiPath]");
            }
            if (reader.Format == BZNFormat.Battlezone2)
            {
                //string? name = reader.ReadSizedString_BZ2_1145("name", 40, obj?.Malformations);
                (string? name, _) = reader.ReadSizedString("name", obj, x => x.AiPathDummy);
                if (name != "AiPath")
                {
                    throw new Exception("Failed to parse AiPath");
                }
            }

            if (reader.Format == BZNFormat.Battlezone || reader.Format == BZNFormat.BattlezoneN64)
            {
                if (reader.Format == BZNFormat.BattlezoneN64 || reader.Version > 2011)
                {
                    // 2016
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("old_ptr", BinaryFieldType.DATA_PTR))
                        throw new Exception("Failed to parse old_ptr/PTR");
                    if (obj != null) obj.sObject = tok.GetUInt32H();
                }
                else
                {
                    // 1030 1032 1034 1035 1037 1038 1039 1040 1043 1044 1045 1049 2003 2004 2010 2011
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("old_ptr", BinaryFieldType.DATA_VOID))
                        throw new Exception("Failed to parse old_ptr/VOID");
                    //if (obj != null) obj.sObject = tok.GetUInt32HR(); // confirm correctness
                    tok.ApplyVoidBytes(obj, x => x.sObject, 0, (v) => BitConverter.ToUInt32(v), expectedCase: 'L');
                }
                //Int32 x = tok.GetUInt32H();
            }
            else if (reader.Format == BZNFormat.Battlezone2)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("sObject", BinaryFieldType.DATA_PTR))
                    throw new Exception("Failed to parse sObject/PTR");
                //if (obj != null) obj.sObject = tok.GetUInt32H();
                tok.ApplyUInt32H8(obj, x => x.sObject);
            }

            if (reader.Format == BZNFormat.BattlezoneN64)
            {
                tok = reader.ReadToken();
                if (tok == null)
                    throw new Exception("Failed to parse label");
                //label = string.Format("bzn64path_{0,4:X4}", tok.GetUInt16());
                //if (obj != null) obj.label = new SizedString() { Value = label };
                tok.ApplyUInt16(obj, x => x.label, 0, (v) => new SizedString(string.Format("bzn64path_{0,4:X4}", v)));
            }
            else
            {
                reader.ReadSizedStringType2("label", obj, x => x.label);
            }
            //Console.WriteLine($"AiPath[{i.ToString().PadLeft(countPaths.ToString().Length)}]: {(label ?? string.Empty)}");

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("pointCount", BinaryFieldType.DATA_LONG))
                throw new Exception("Failed to parse pointCount/LONG");
            //int pointCount = tok.GetInt32();
            (int pointCount, _) = tok.ApplyInt32(obj, x => x.pointCount);

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("points", BinaryFieldType.DATA_VEC2D))
                throw new Exception("Failed to parse point/VEC2D");
            Vector2D[] points = new Vector2D[tok.GetCount()];
            if (obj != null)
                obj.points = points;
            for (int j = 0; j < points.Length; j++)
            {
                //points[j] = tok.GetVector2D(j);
                //tok.CheckMalformationsVector2D(points[j].Malformations, reader.FloatFormat, j);
                ////tok.GetSubToken(j, 0).CheckMalformationsSingle("  x", points[j].Malformations, reader.FloatFormat);
                ////tok.GetSubToken(j, 1).CheckMalformationsSingle("  z", points[j].Malformations, reader.FloatFormat);

                tok.ReadVector2D(obj, x => x.points, j, format: reader.FloatFormat);
            }
            //if (obj != null) obj.points = points;

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("pathType", BinaryFieldType.DATA_VOID))
                throw new Exception("Failed to parse pathType/VOID");
            // 02 00 00 00 - binary for 2
            // "02000000" - ASCII for 2
            if (obj != null) obj.pathType = tok.GetUInt32HR();

            return true;
        }

        public void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            if (writer.Format == BZNFormat.Battlezone)
            {
                writer.WriteValidation("AiPath");
            }

            if (writer.Format == BZNFormat.Battlezone2)
            {
                //writer.WriteSizedString_BZ2_1145("name", 40, "AiPath", Malformations);
                // TODO move this to a differnt malformation to get rid of the property, or just don't have malformations at all on it
                writer.WriteSizedString("name", this, x => x.AiPathDummy, (val) => val ?? new SizedString("AiPath"));
            }

            if (writer.Format == BZNFormat.Battlezone || writer.Format == BZNFormat.BattlezoneN64)
            {
                //if (writer.Format == BZNFormat.BattlezoneN64 || writer.Version >= 2016)
                if (writer.Format == BZNFormat.BattlezoneN64 || writer.Version > 2011)
                {
                    // 2016
                    writer.WritePtr("old_ptr", this, x => x.sObject);
                }
                else
                {
                    // 1030 1032 1034 1035 1037 1038 1039 1040 1043 1044 1045 1049 2003 2004 2010 2011
                    writer.WriteVoidBytesL("old_ptr", this, x => x.sObject); // upper case identified in some rare cases
                }
                //Int32 x = tok.GetUInt32H();
            }
            else if (writer.Format == BZNFormat.Battlezone2)
            {
                if (sObject.HasValue)
                    //writer.WritePtr32("sObject", sObject.Value);
                    writer.WritePtr("sObject", this, x => x.sObject);
            }

            if (writer.Format == BZNFormat.BattlezoneN64)
            {
                writer.WriteUInt16("label", this, x => x.label, (v) =>
                {
                    // extract number from label and write as UInt16
                    if (label.Value.StartsWith("bzn64path_") && UInt16.TryParse(label.Value.Substring(10), System.Globalization.NumberStyles.HexNumber, null, out UInt16 labelNum))
                        return labelNum;
                    throw new Exception("Failed to parse label for N64 path");
                });
            }
            else
            {
                writer.WriteSizedStringType2("label", this, x => x.label);
            }

            //writer.WriteSignedValues("pointCount", points.Length);
            writer.WriteInt32("pointCount", this, x => x.pointCount);
            //writer.WriteVector2Ds("points", preserveMalformations, points);
            writer.WriteVector2D("points", this, x => x.points);
            if (writer.Format == BZNFormat.Battlezone2)
            {
                writer.WriteVoidBytes("pathType", this, x => x.pathType);
            }
            else
            {
                writer.WriteVoidBytesL("pathType", this, x => x.pathType);
            }
        }
    }
}
