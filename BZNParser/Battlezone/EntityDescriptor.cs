using BZNParser;
using BZNParser.Battlezone.GameObject;
using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Formats.Asn1;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace BZNParser.Battlezone
{
    public class EntityDescriptor : IMalformable
    {
        public string PrjID { get; set; }
        public UInt32 seqNo { get; set; }
        public Vector3D pos { get; set; }
        public UInt32 team { get; set; }
        public string label { get; set; }
        public bool isUser { get; set; }
        public UInt32 obj_addr { get; set; }
        public Matrix transform { get; set; }

        public Entity? gameObject { get; set; }



        private readonly IMalformable.MalformationManager _malformationManager;
        public IMalformable.MalformationManager Malformations => _malformationManager;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public EntityDescriptor()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            this._malformationManager = new IMalformable.MalformationManager(this);
        }

        public static bool Create(BZNFileBattlezone parent, BZNStreamReader reader, int countLeft, out EntityDescriptor? obj, bool create = true, BattlezoneBZNHints? Hints = null)
        {
            obj = null;
            if (create)
                obj = new EntityDescriptor();
            EntityDescriptor.Hydrate(parent, reader, countLeft, obj, Hints);
            return true;
        }

        public static bool Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, int countLeft, EntityDescriptor? obj, BattlezoneBZNHints? Hints = null)
        {
            IBZNToken tok;
            if (!reader.InBinary)
            {
                tok = reader.ReadToken();
                if (!tok.IsValidationOnly() || !tok.Validate("GameObject", BinaryFieldType.DATA_UNKNOWN))
                    throw new Exception("Failed to parse [GameObject]");
            }

            if (reader.Format == BZNFormat.BattlezoneN64)
            {
                tok = reader.ReadToken();
                UInt16 ItemID = tok.GetUInt16();
                string PrjID = parent.Hints?.EnumerationPrjID?[ItemID] ?? string.Format("bzn64prjid_{0,4:X4}", ItemID);
                if (obj != null) obj.PrjID = PrjID;
            }
            else if (reader.Format == BZNFormat.Battlezone)
            {
                tok = reader.ReadToken();
                if (!tok.Validate("PrjID", BinaryFieldType.DATA_ID))
                    throw new Exception("Failed to parse PrjID/ID");
                //if (!tok.Validate("PrjID", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse PrjID/ID");
                string PrjID = tok.GetString();
                if (reader.Version == 1001)
                {
                    if (obj != null) obj.PrjID = PrjID = obj.Malformations.AddBinaryMessString("PrjID", PrjID);
                }
                else
                {
                    if (obj != null) obj.PrjID = PrjID;
                }
            }
            else if (reader.Format == BZNFormat.Battlezone2)
            {
                //if (reader.HasBinary && reader.Version > 1105) // not sure the version for this one
                //{
                //    tok = reader.ReadToken();
                //    if (!tok.Validate(null, BinaryFieldType.DATA_CHAR))
                //        throw new Exception("Failed to parse ?/CHAR");
                //    //string odfName = tok.GetString();
                //    byte odfLength = tok.GetUInt8();
                //}

                string PrjID = null;
                //if (reader.Version < 1145)
                if (reader.Version < 1155)
                {
                    PrjID = reader.ReadGameObjectClass_BZ2(parent, "config");
                }
                else
                {
                    if (parent.SaveType == SaveType.LOCKSTEP)
                    {

                    }
                    else
                    {
                        if (reader.Version == 1180)
                        {
                            PrjID = reader.ReadGameObjectClass_BZ2(parent, "GetClass()");
                        }
                        else
                        {
                            // 1183 1187 1188 1192
                            PrjID = reader.ReadGameObjectClass_BZ2(parent, "objClass");
                        }
                    }
                }
                if (PrjID == null)
                    throw new Exception("Failed to parse PrjID/ID");
                if (obj != null) obj.PrjID = PrjID;
            }

            uint seqNo = 0;
            if (reader.Format == BZNFormat.Battlezone2)
            {
                if (reader.InBinary)
                {
                    seqNo = reader.ReadCompressedNumberFromBinary();
                }
                //else if (reader.Version >= 1145 && reader.InBinary && false)// && omitBinarySaveHeadsers)
                //{
                //    tok = reader.ReadToken();
                //    if (tok.Validate("seqno", BinaryFieldType.DATA_SHORT))
                //        throw new Exception("Failed to parse seqno/SHORT");
                //    UInt16 seqno2a = tok.GetUInt16();
                //
                //    tok = reader.ReadToken();
                //    if (!tok.Validate("seqno", BinaryFieldType.DATA_CHAR))
                //        throw new Exception("Failed to parse seqno/CHAR");
                //    byte seqno2b = tok.GetUInt8();
                //
                //    seqNo = (uint)((seqno2b << 16) | (seqno2a));
                //}
                else
                {
                    tok = reader.ReadToken();
                    if (!tok.Validate("seqno", BinaryFieldType.DATA_LONG))
                        throw new Exception("Failed to parse seqno/LONG");
                    seqNo = tok.GetUInt32H();
                }
            }
            else if (reader.Format == BZNFormat.Battlezone || reader.Format == BZNFormat.BattlezoneN64)
            {
                tok = reader.ReadToken();
                if (!tok.Validate("seqno", BinaryFieldType.DATA_SHORT))
                    throw new Exception("Failed to parse seqno/SHORT");
                seqNo = tok.GetUInt16();
            }
            if (obj != null) obj.seqNo = seqNo;

            if (reader.Format == BZNFormat.Battlezone || reader.Format == BZNFormat.BattlezoneN64)
            {
                tok = reader.ReadToken();
                if (!tok.Validate("pos", BinaryFieldType.DATA_VEC3D))
                    throw new Exception("Failed to parse pos/VEC3D");
                if (obj != null) obj.pos = tok.GetVector3D();
            }

            tok = reader.ReadToken();
            if (reader.Format == BZNFormat.Battlezone || reader.Format == BZNFormat.BattlezoneN64)
            {
                if (!tok.Validate("team", BinaryFieldType.DATA_LONG))
                    throw new Exception("Failed to parse team/LONG");
                if (obj != null) obj.team = tok.GetUInt32();
            }
            if (reader.Format == BZNFormat.Battlezone2)
            {
                if (reader.Version < 1145)
                {
                    if (!tok.Validate("team", BinaryFieldType.DATA_LONG))
                        throw new Exception("Failed to parse team/LONG");
                    if (obj != null) obj.team = tok.GetUInt32();
                }
                else
                {
                    if (!tok.Validate("team", BinaryFieldType.DATA_CHAR))
                        throw new Exception("Failed to parse team/CHAR");
                    if (obj != null) obj.team = tok.GetUInt8(); // does this include perceived team in the high nybble? probably
                }
            }

            if (reader.Format == BZNFormat.BattlezoneN64)
            {
                tok = reader.ReadToken();
                if (!tok.Validate("label", BinaryFieldType.DATA_SHORT))
                    throw new Exception("Failed to parse label/CHAR");
                if (obj != null) obj.label = string.Format("bzn64label_{0,4:X4}", tok.GetUInt16());
            }
            else if (reader.Format == BZNFormat.Battlezone)
            {
                tok = reader.ReadToken();
                if (!tok.Validate("label", BinaryFieldType.DATA_CHAR))
                    throw new Exception("Failed to parse label/CHAR");
                if (obj != null) obj.label = tok.GetString();
            }
            else if (reader.Format == BZNFormat.Battlezone2)
            {
                if (reader.Version < 1145)
                {
                    //tok = reader.ReadToken();
                    //if (!tok.Validate("label", BinaryFieldType.DATA_CHAR))
                    //    throw new Exception("Failed to parse label/CHAR");
                    //label = tok.GetString();

                    string label = reader.ReadSizedString_BZ2_1145("label", 40);
                    if (obj != null) obj.label = label;
                }
                else
                {
                    bool noLabel = (seqNo & 0x800000) != 0;
                    seqNo &= ~(UInt32)0x800000;

                    if (noLabel)
                    {

                    }
                    else
                    {
                        //tok = reader.ReadToken();
                        //if (!tok.Validate(null, BinaryFieldType.DATA_CHAR)) throw new Exception("Failed to parse ?/CHAR");
                        //
                        //tok = reader.ReadToken();
                        //if (!tok.Validate("label", BinaryFieldType.DATA_CHAR))
                        //    throw new Exception("Failed to parse label/CHAR");
                        //label = tok.GetString();
                        string label = reader.ReadSizedString_BZ2_1145("label", 40);
                        if (obj != null) obj.label = label;
                    }
                }
            }

            if (reader.Format == BZNFormat.Battlezone2)
            {
                if (parent.SaveType != SaveType.JOIN)
                {
                    tok = reader.ReadToken();
                    if (reader.Version < 1145)
                    {
                        if (!tok.Validate("isUser", BinaryFieldType.DATA_LONG))
                            throw new Exception("Failed to parse isUser/LONG");
                        UInt32 isUser = tok.GetUInt32();
                        if (obj != null)
                        {
                            if (isUser != 0 && isUser != 1)
                            {
                                obj.Malformations.AddIncorrect("isUser", isUser);
                            }
                            obj.isUser = isUser != 0;
                        }
                    }
                    else
                    {
                        if (!tok.Validate("isUser", BinaryFieldType.DATA_BOOL))
                            throw new Exception("Failed to parse isUser/BOOL");
                        if (obj != null) obj.isUser = tok.GetBoolean();
                    }
                }
                //else{}
            }
            else if (reader.Format == BZNFormat.Battlezone || reader.Format == BZNFormat.BattlezoneN64)
            {
                tok = reader.ReadToken();
                if (!tok.Validate("isUser", BinaryFieldType.DATA_LONG))
                    throw new Exception("Failed to parse isUser/LONG");
                UInt32 isUser = tok.GetUInt32();
                if (obj != null)
                {
                    if (isUser != 0 && isUser != 1)
                    {
                        obj.Malformations.AddIncorrect("isUser", isUser);
                    }
                    obj.isUser = isUser != 0;
                }
            }
            
            if (reader.Format == BZNFormat.Battlezone || reader.Format == BZNFormat.BattlezoneN64)
            {
                if (reader.Format == BZNFormat.BattlezoneN64 || reader.Version < 1002)
                {
                    UInt32 obj_addr = reader.ReadBZ1_PtrDepricated("obj_addr"); // string name unconfirmed
                    if (obj != null) obj.obj_addr = obj_addr;
                }
                else
                {
                    UInt32 obj_addr = reader.ReadBZ1_Ptr("obj_addr");
                    if (obj != null) obj.obj_addr = obj_addr;
                }
            }
            else if (reader.Format == BZNFormat.Battlezone2)
            {
                tok = reader.ReadToken();
                if (!tok.Validate("objAddr", BinaryFieldType.DATA_PTR))
                    throw new Exception("Failed to parse objAddr/PTR");
                if (obj != null) obj.obj_addr = tok.GetUInt32H();
            }

            if (reader.Format == BZNFormat.Battlezone2)
            {
                tok = reader.ReadToken();
                if (!tok.Validate("transform", BinaryFieldType.DATA_MAT3D))
                    throw new Exception("Failed to parse transform/MAT3D");
                if (obj != null) obj.transform = tok.GetMatrix();
            }
            if ((reader.Format == BZNFormat.Battlezone && reader.Version > 1001) || reader.Format == BZNFormat.BattlezoneN64)
            {
                tok = reader.ReadToken();
                if (!tok.Validate("transform", BinaryFieldType.DATA_MAT3DOLD))
                    throw new Exception("Failed to parse transform/MAT3DOLD");
                if (obj != null) obj.transform = tok.GetMatrixOld();
            }

            if (obj == null)
                return true;
            // other save types here

            obj.gameObject = ParseGameObject(parent, reader, countLeft, Hints, obj);
            return true;
        }

        private static Entity? ParseGameObject(BZNFileBattlezone parent, BZNStreamReader reader, int countLeft, BattlezoneBZNHints? Hints, EntityDescriptor obj)
        {
            List<string>? ValidClassLabels = null;

            if (Hints?.ClassLabels != null)
            {
                ValidClassLabels = new List<string>();
                string lookupKey = obj.PrjID.ToLowerInvariant();
                if (Hints.ClassLabels.ContainsKey(lookupKey))
                {
                    var possibleClasses = Hints.ClassLabels[lookupKey];
                    foreach (string possibleClass in possibleClasses)
                    {
                        if (possibleClass != null && parent.ClassLabelMap.ContainsKey(possibleClass))
                        {
                            ValidClassLabels.Add(possibleClass);
                        }
                    }
                }
            }
            
            // try every possible object
            reader.Bookmark.Push();
                
            List<(Entity Object, bool Expected, long Next, string Name)> Candidates = new List<(Entity Object, bool Expected, long Next, string Name)>();

            foreach (var kv in parent.ClassLabelMap.OrderBy(dr => ValidClassLabels != null && ValidClassLabels.Contains(dr.Key) ? 0 : 1).ThenBy(dr => dr.Key))
            {
                string classLabel = kv.Key;
                if (!parent.LongTermClassLabelLookupCache.ContainsKey(obj.PrjID.ToLowerInvariant()) || parent.LongTermClassLabelLookupCache[obj.PrjID.ToLowerInvariant()].Contains(classLabel))
                {
                    if (!(Hints?.Strict ?? false) || ValidClassLabels == null || ValidClassLabels.Count == 0 || ValidClassLabels.Contains(classLabel))
                        try
                        {
                            Entity? tempGameObject;
                            IClassFactory classFactory = kv.Value;
                            if (classFactory.Create(parent, reader, obj, classLabel, out tempGameObject) && tempGameObject != null)
                                if (CheckNext(parent, reader, countLeft - 1, Hints))
                                    Candidates.Add((tempGameObject, ValidClassLabels?.Contains(classLabel) ?? false, reader.Bookmark.Get(), classLabel));
                        }
                        catch
                        {
                        }
                    reader.Bookmark.Peek();
                }
            }
            reader.Bookmark.Pop();

            if (!parent.LongTermClassLabelLookupCache.ContainsKey(obj.PrjID.ToLowerInvariant()))
                parent.LongTermClassLabelLookupCache[obj.PrjID.ToLowerInvariant()] = new HashSet<string>(Candidates.Select(dr => dr.Name));
            parent.LongTermClassLabelLookupCache[obj.PrjID.ToLowerInvariant()] = new HashSet<string>(parent.LongTermClassLabelLookupCache[obj.PrjID.ToLowerInvariant()].Intersect(Candidates.Select(dr => dr.Name)));

            if (Candidates.Count > 0)
            {
                // limit to only the shortest valid parsings, avoiding issues with objects that overflow over a 2nd object perfectly
                long minEnd = Candidates.Min(dr => dr.Next);
                reader.Bookmark.Set(minEnd);
                Candidates = Candidates.Where(dr => dr.Next == minEnd).ToList();

                if (Candidates.Count == 1)
                {
                    return Candidates[0].Object;
                }
                else
                {
                    return new MultiClass(obj, Candidates);
                }
            }
            else
            {
                throw new Exception($"Failed to parse GameObject {obj.PrjID}");
            }
        }

        private static bool CheckNext(BZNFileBattlezone parent, BZNStreamReader reader, int countLeft, BattlezoneBZNHints? Hints)
        {
            if (countLeft == 0)
            {
                reader.Bookmark.Push();
                try
                {
                    parent.TailParse(reader);
                }
                catch
                {
                    reader.Bookmark.Pop();
                    return false;
                }
                reader.Bookmark.Pop();
                return true;
            }
            else
            {
                if (!reader.InBinary)
                {
                    reader.Bookmark.Push();
                    IBZNToken tok = reader.ReadToken();
                    reader.Bookmark.Pop();
                    if (!tok.IsValidationOnly() || !tok.Validate("GameObject", BinaryFieldType.DATA_UNKNOWN))
                    {
                        // next field isn't the start of a GameObject
                        return false;
                    }
                    return true;
                }
                else
                {
                    reader.Bookmark.Push();
                    try
                    {
                        if (EntityDescriptor.Hydrate(parent, reader, countLeft, null, Hints: Hints))
                        {
                            reader.Bookmark.Pop();
                            return true;
                        }
                        else
                        {
                            reader.Bookmark.Pop();
                            return false;
                        }
                    }
                    catch
                    {
                        // next field isn't the start of a GameObject (since a shallow gameobject crashed)
                        reader.Bookmark.Pop();
                        return false;
                    }
                }
            }
        }

        public void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            writer.WriteValidation("GameObject");


            if (writer.Format == BZNFormat.BattlezoneN64)
            {
                if (PrjID.StartsWith("bzn64prjid_"))
                {
                    string possibleLabel = PrjID.Substring("bzn64prjid_".Length);
                    if (ushort.TryParse(possibleLabel, System.Globalization.NumberStyles.HexNumber, null, out ushort possibleItemID))
                    {
                        writer.WriteUnsignedValues(null, possibleItemID);
                    }
                    else
                    {
                        throw new Exception("Failed to parse PrjID/ID");
                    }
                }
                else
                {
                    var lookup = parent.Hints?.EnumerationPrjID;
                    if (lookup != null)
                    {
                        UInt16? key = lookup.Where(dr => dr.Value == PrjID).Select(dr => dr.Key).FirstOrDefault();
                        if (key.HasValue)
                        {
                            writer.WriteUnsignedValues(null, key.Value);
                        }
                        else
                        {
                            throw new Exception("Failed to parse PrjID/ID");
                        }
                    }
                    else
                    {
                        throw new Exception("Failed to parse PrjID/ID");
                    }
                }
            }
            else if (writer.Format == BZNFormat.Battlezone)
            {
                writer.WriteIDs("PrjID", PrjID);
            }
            else if (writer.Format == BZNFormat.Battlezone2)
            {
                if (writer.Version < 1155)
                {
                    writer.WriteGameObjectClass_BZ2(parent, "config", PrjID);
                }
                else
                {
                    if (parent.SaveType == SaveType.LOCKSTEP)
                    {

                    }
                    else
                    {
                        if (writer.Version == 1180)
                        {
                             writer.WriteGameObjectClass_BZ2(parent, "GetClass()", PrjID);
                        }
                        else
                        {
                            writer.WriteGameObjectClass_BZ2(parent, "objClass", PrjID);
                        }
                    }
                }
            }

            if (writer.Format == BZNFormat.Battlezone2)
            {
                if (writer.InBinary)
                {
                    //writer.WriteCompressedNumberFromBinary(seqNo);
                    writer.WriteUnsignedValues(null, seqNo);
                }
                else
                {
                    writer.WriteUnsignedHexLValues("seqno", seqNo); // lowercase hex
                }
            }
            else if (writer.Format == BZNFormat.Battlezone || writer.Format == BZNFormat.BattlezoneN64)
            {
                writer.WriteUnsignedValues("seqno", (UInt16)seqNo);
            }

            if (writer.Format == BZNFormat.Battlezone || writer.Format == BZNFormat.BattlezoneN64)
            {
                writer.WriteVector3Ds("pos", pos);
            }

            if (writer.Format == BZNFormat.Battlezone || writer.Format == BZNFormat.BattlezoneN64)
            {
                writer.WriteUnsignedValues("team", team);
            }
            if (writer.Format == BZNFormat.Battlezone2)
            {
                if (writer.Version < 1145)
                {
                    writer.WriteUnsignedValues("team", team);
                }
                else
                {
                    writer.WriteUnsignedValues("team", (byte)team);
                }
            }

            if (writer.Format == BZNFormat.BattlezoneN64)
            {
                if (label != null && label.StartsWith("bzn64label_"))
                {
                    string possibleLabelNum = label.Substring("bzn64label_".Length);
                    if (ushort.TryParse(possibleLabelNum, System.Globalization.NumberStyles.HexNumber, null, out ushort possibleLabelID))
                    {
                        writer.WriteUnsignedValues(null, possibleLabelID);
                    }
                    else
                    {
                        throw new Exception("Failed to parse label/CHAR");
                    }
                }
                else
                {
                    throw new Exception("Failed to parse label/CHAR");
                }
            }
            else if (writer.Format == BZNFormat.Battlezone)
            {
                writer.WriteChars("label", label);
            }
            else if (writer.Format == BZNFormat.Battlezone2)
            {
                if (writer.Version < 1145)
                {
                    writer.WriteSizedString_BZ2_1145("label", 40, label);
                }
                else
                {
                    bool noLabel = (seqNo & 0x800000) != 0;
                    seqNo &= ~(UInt32)0x800000;

                    if (noLabel)
                    {

                    }
                    else
                    {
                        writer.WriteSizedString_BZ2_1145("label", 40, label);
                    }
                }
            }

            if (writer.Format == BZNFormat.Battlezone2)
            {
                if (parent.SaveType != SaveType.JOIN)
                {
                    if (writer.Version < 1145)
                    {
                        var mals = Malformations.GetMalformations(Malformation.INCORRECT, "isUser");
                        if (preserveMalformations && mals.Length > 0)
                        {
                            UInt32 malValue = (UInt32)mals[0].Fields[0];
                            // consider clearing malformations when you edit a malformed field

                            writer.WriteUnsignedValues("isUser", isUser ? malValue : 0U);
                        }
                        else
                        {
                            writer.WriteUnsignedValues("isUser", isUser ? 1U : 0U);
                        }
                    }
                    else
                    {
                        writer.WriteBooleans("isUser", isUser);
                    }
                }
                //else{}
            }
            else if (writer.Format == BZNFormat.Battlezone || writer.Format == BZNFormat.BattlezoneN64)
            {
                var mals = Malformations.GetMalformations(Malformation.INCORRECT, "isUser");
                if (preserveMalformations && mals.Length > 0)
                {
                    UInt32 malValue = (UInt32)mals[0].Fields[0];
                    // consider clearing malformations when you edit a malformed field

                    writer.WriteUnsignedValues("isUser", isUser ? malValue : 0U);
                }
                else
                {
                    writer.WriteUnsignedValues("isUser", isUser ? 1U : 0U);
                }
            }

            if (writer.Format == BZNFormat.Battlezone || writer.Format == BZNFormat.BattlezoneN64)
            {
                if (writer.Format == BZNFormat.BattlezoneN64 || writer.Version < 1002)
                {
                    writer.WriteBZ1_PtrDepricated("obj_addr", obj_addr); // string name unconfirmed
                }
                else
                {
                    writer.WriteBZ1_PtrDepricated("obj_addr", obj_addr);
                }
            }
            else if (writer.Format == BZNFormat.Battlezone2)
            {
                writer.WritePtr("objAddr", obj_addr);
            }

            if (writer.Format == BZNFormat.Battlezone2)
            {
                writer.WriteMat3Ds("transform", transform);
            }
            if ((writer.Format == BZNFormat.Battlezone && writer.Version > 1001) || writer.Format == BZNFormat.BattlezoneN64)
            {
                writer.WriteMat3DOlds("transform", transform);
            }


            // GameObject
            if (gameObject is MultiClass)
            {
                // TODO if they all serialize the same, spit out that data, else throw an error
                // for now we just cheat and use the first one
                (gameObject as MultiClass).Candidates.OrderBy(dr => dr.Expected ? 0 : 1).First().Object.Write(parent, writer, binary, save, preserveMalformations);
            }
            else
            {
                gameObject.Write(parent, writer, binary, save, preserveMalformations);
            }
        }
    }
}
