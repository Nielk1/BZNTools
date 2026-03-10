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
        public UInt32? sObject { get; set; }
        public string label { get; set; }
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
            IBZNToken tok;

            if (!reader.InBinary && reader.Format == BZNFormat.Battlezone)
            {
                tok = reader.ReadToken();
                if (!tok.IsValidationOnly() || !tok.Validate("AiPath", BinaryFieldType.DATA_UNKNOWN))
                    throw new Exception("Failed to parse [AiPath]");
            }
            if (reader.Format == BZNFormat.Battlezone2)
            {
                string? name = reader.ReadSizedString_BZ2_1145("name", 40, obj?.Malformations);
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
                    if (!tok.Validate("old_ptr", BinaryFieldType.DATA_PTR))
                        throw new Exception("Failed to parse old_ptr/PTR");
                    if (obj != null) obj.sObject = tok.GetUInt32H();
                }
                else
                {
                    // 1030 1032 1034 1035 1037 1038 1039 1040 1043 1044 1045 1049 2003 2004 2010 2011
                    tok = reader.ReadToken();
                    if (!tok.Validate("old_ptr", BinaryFieldType.DATA_VOID))
                        throw new Exception("Failed to parse old_ptr/VOID");
                    if (obj != null) obj.sObject = tok.GetUInt32HR(); // confirm correctness
                }
                //Int32 x = tok.GetUInt32H();
            }
            else if (reader.Format == BZNFormat.Battlezone2)
            {
                // must figure out why this sometimes is missing
                reader.Bookmark.Mark();
                tok = reader.ReadToken();
                if (!tok.Validate("sObject", BinaryFieldType.DATA_PTR))
                {
                    reader.Bookmark.RevertToBookmark();
                    //throw new Exception("Failed to parse sObject/PTR");
                }
                else
                {
                    reader.Bookmark.Commit();
                    if (obj != null) obj.sObject = tok.GetUInt32H();
                }
            }

            string? label = null;
            if (reader.Format == BZNFormat.BattlezoneN64)
            {
                tok = reader.ReadToken();
                label = string.Format("bzn64path_{0,4:X4}", tok.GetUInt16());
                if (obj != null) obj.label = label;
            }
            else
            {
                tok = reader.ReadToken();
                if (!tok.Validate("size", BinaryFieldType.DATA_LONG))
                    throw new Exception("Failed to parse size/LONG");
                int labelSize = tok.GetInt32();

                if (labelSize > 0)
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("label", BinaryFieldType.DATA_CHAR))
                        throw new Exception("Failed to parse label/CHAR");
                    label = tok.GetString();
                    if (obj != null)
                    {
                        if (label.Length != labelSize)
                        {
                            if (labelSize > label.Length)
                            {
                                obj.Malformations.AddStringPad("label", labelSize);
                            }
                            else
                            {
                                obj.Malformations.AddIncorrectTextParse("label", label);
                            }
                        }
                    }
                    if (label.Length > labelSize)
                        label = label.Substring(0, labelSize);
                }
                if (obj != null) obj.label = label ?? string.Empty;
            }
            //Console.WriteLine($"AiPath[{i.ToString().PadLeft(countPaths.ToString().Length)}]: {(label ?? string.Empty)}");

            tok = reader.ReadToken();
            if (!tok.Validate("pointCount", BinaryFieldType.DATA_LONG))
                throw new Exception("Failed to parse pointCount/LONG");
            int pointCount = tok.GetInt32();

            tok = reader.ReadToken();
            if (!tok.Validate("points", BinaryFieldType.DATA_VEC2D))
                throw new Exception("Failed to parse point/VEC2D");
            Vector2D[] points = new Vector2D[pointCount];
            for (int j = 0; j < pointCount; j++)
            {
                points[j] = tok.GetVector2D(j);
                tok.CheckMalformationsVector2D(points[j].Malformations, reader.FloatFormat, j);
                //tok.GetSubToken(j, 0).CheckMalformationsSingle("  x", points[j].Malformations, reader.FloatFormat);
                //tok.GetSubToken(j, 1).CheckMalformationsSingle("  z", points[j].Malformations, reader.FloatFormat);
            }
            if (obj != null) obj.points = points;

            tok = reader.ReadToken();
            if (!tok.Validate("pathType", BinaryFieldType.DATA_VOID))
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
                writer.WriteSizedString_BZ2_1145("name", 40, "AiPath", Malformations);
            }

            if (writer.Format == BZNFormat.Battlezone || writer.Format == BZNFormat.BattlezoneN64)
            {
                //if (writer.Format == BZNFormat.BattlezoneN64 || writer.Version >= 2016)
                if (writer.Format == BZNFormat.BattlezoneN64 || writer.Version > 2011)
                {
                    // 2016
                    writer.WriteBZ1_Ptr("old_ptr", sObject.Value);
                }
                else
                {
                    // 1030 1032 1034 1035 1037 1038 1039 1040 1043 1044 1045 1049 2003 2004 2010 2011
                    writer.WriteVoidBytesL("old_ptr", sObject.Value);
                }
                //Int32 x = tok.GetUInt32H();
            }
            else if (writer.Format == BZNFormat.Battlezone2)
            {
                if (sObject.HasValue)
                    writer.WritePtr32("sObject", sObject.Value);
            }

            if (writer.Format == BZNFormat.BattlezoneN64)
            {
                // extract number from label and write as UInt16
                if (label.StartsWith("bzn64path_") && UInt16.TryParse(label.Substring(10), System.Globalization.NumberStyles.HexNumber, null, out UInt16 labelNum))
                {
                    writer.WriteUnsignedValues(null, labelNum);
                }
                else
                {
                    throw new Exception("Failed to parse label for N64 path");
                }
            }
            else
            {
                string textToWrite = label;
                int lengthToWrite = label.Length;

                var malText = Malformations.GetMalformations(Malformation.INCORRECT_TEXT, "label");
                var malPad = Malformations.GetMalformations(Malformation.STRING_PAD, "label");
                if (preserveMalformations && malText.Length > 0)
                {
                    // string was truncated
                    textToWrite = (string)malText[0].Fields[0];
                    //lengthToWrite = textToWrite.Length;
                }
                if (preserveMalformations && malPad.Length > 0)
                {
                    // string reported as longer
                    lengthToWrite = (int)malPad[0].Fields[0];
                }
                writer.WriteSignedValues("size", lengthToWrite);

                //if (label.Length > 0)
                if (lengthToWrite > 0)
                    writer.WriteChars("label", textToWrite, Malformations);
            }

            writer.WriteSignedValues("pointCount", points.Length);
            writer.WriteVector2Ds("points", preserveMalformations, points);
            writer.WriteVoidBytes("pathType", pathType); // BZ2 it's written as a hex-string
        }
    }
}
