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
        public SizedString PrjID { get; set; }
        public UInt32 seqNo { get; set; }
        public Vector3D pos { get; set; }
        public UInt32 team { get; set; }
        public SizedString label { get; set; }
        public bool isUser { get; set; }
        public UInt64 obj_addr { get; set; }
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
            IBZNToken? tok;
            if (!reader.InBinary)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.IsValidationOnly() || !tok.Validate("GameObject", BinaryFieldType.DATA_UNKNOWN))
                    throw new Exception("Failed to parse [GameObject]");
            }

            if (reader.Format == BZNFormat.BattlezoneN64)
            {
                tok = reader.ReadToken();
                UInt16 ItemID = tok.GetUInt16();
                string PrjID = parent.Hints?.EnumerationPrjID?[ItemID] ?? string.Format("bzn64prjid_{0,4:X4}", ItemID);
                if (obj != null) obj.PrjID = new SizedString() { Value = PrjID };
            }
            else if (reader.Format == BZNFormat.Battlezone)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("PrjID", BinaryFieldType.DATA_ID))
                    throw new Exception("Failed to parse PrjID/ID");
                //if (tok == null || !tok.Validate("PrjID", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse PrjID/ID");
                string PrjID = tok.GetString();

                // version 1001 may require the string be 8 bytes but our only sample is 1 ASCII atm
                // version 1001 has it written as a raw 1-liner and not a normal ID, but that might be how IDs work that far back
                tok.ReadID(obj, x => x.PrjID);
            }
            else if (reader.Format == BZNFormat.Battlezone2)
            {
                //if (reader.HasBinary && reader.Version > 1105) // not sure the version for this one
                //{
                //    tok = reader.ReadToken();
                //    if (tok == null || !tok.Validate(null, BinaryFieldType.DATA_CHAR))
                //        throw new Exception("Failed to parse ?/CHAR");
                //    //string odfName = tok.GetString();
                //    byte odfLength = tok.GetUInt8();
                //}

                //if (reader.Version < 1145)
                if (reader.Version < 1155)
                {
                    //PrjID = reader.ReadGameObjectClass_BZ2(parent, "config", obj?.Malformations);
                    reader.ReadSizedString("config", obj, x => x.PrjID);
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
                            //PrjID = reader.ReadGameObjectClass_BZ2(parent, "GetClass()", obj?.Malformations);
                            reader.ReadSizedString("GetClass()", obj, x => x.PrjID);
                        }
                        else
                        {
                            // 1183 1187 1188 1192
                            //PrjID = reader.ReadGameObjectClass_BZ2(parent, "objClass", obj?.Malformations);
                            reader.ReadSizedString("objClass", obj, x => x.PrjID);
                        }
                    }
                }
            }

            uint seqNo = 0;
            if (reader.Format == BZNFormat.Battlezone2)
            {
                if (reader.Version == 1101 || reader.Version == 1070)
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("seqno", BinaryFieldType.DATA_SHORT))
                        throw new Exception("Failed to parse seqno/SHORT");
                    seqNo = tok.GetUInt16H();
                }
                else
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("seqno", BinaryFieldType.DATA_LONG))
                        throw new Exception("Failed to parse seqno/LONG");
                    seqNo = tok.GetUInt32H();
                }
            }
            else if (reader.Format == BZNFormat.Battlezone || reader.Format == BZNFormat.BattlezoneN64)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("seqno", BinaryFieldType.DATA_SHORT))
                    throw new Exception("Failed to parse seqno/SHORT");
                seqNo = tok.GetUInt16();
            }
            if (obj != null) obj.seqNo = seqNo;

            if (reader.Format == BZNFormat.Battlezone || reader.Format == BZNFormat.BattlezoneN64)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("pos", BinaryFieldType.DATA_VEC3D))
                    throw new Exception("Failed to parse pos/VEC3D");
                if (obj != null)
                {
                    obj.pos = tok.GetVector3D();
                    tok.CheckMalformationsVector3D(obj.pos.Malformations, reader.FloatFormat);
                }
            }

            tok = reader.ReadToken();
            if (reader.Format == BZNFormat.Battlezone || reader.Format == BZNFormat.BattlezoneN64)
            {
                if (tok == null || !tok.Validate("team", BinaryFieldType.DATA_LONG))
                    throw new Exception("Failed to parse team/LONG");
                if (obj != null) obj.team = tok.GetUInt32();
            }
            if (reader.Format == BZNFormat.Battlezone2)
            {
                if (reader.Version < 1145)
                {
                    if (tok == null || !tok.Validate("team", BinaryFieldType.DATA_LONG))
                        throw new Exception("Failed to parse team/LONG");
                    if (obj != null) obj.team = tok.GetUInt32();
                }
                else
                {
                    if (tok == null || !tok.Validate("team", BinaryFieldType.DATA_CHAR))
                        throw new Exception("Failed to parse team/CHAR");
                    if (obj != null) obj.team = tok.GetUInt8(); // does this include perceived team in the high nybble? probably
                }
            }

            if (reader.Format == BZNFormat.BattlezoneN64)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("label", BinaryFieldType.DATA_SHORT))
                    throw new Exception("Failed to parse label/CHAR");
                if (obj != null) obj.label = new SizedString() { Value = string.Format("bzn64label_{0,4:X4}", tok.GetUInt16()) };
            }
            else if (reader.Format == BZNFormat.Battlezone)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("label", BinaryFieldType.DATA_CHAR))
                    throw new Exception("Failed to parse label/CHAR");
                //if (obj != null) obj.label = tok.GetString();
                tok.ReadChars(obj, x => x.label);
            }
            else if (reader.Format == BZNFormat.Battlezone2)
            {
                if (reader.Version < 1145)
                {
                    //tok = reader.ReadToken();
                    //if (tok == null || !tok.Validate("label", BinaryFieldType.DATA_CHAR))
                    //    throw new Exception("Failed to parse label/CHAR");
                    //label = tok.GetString();

                    //string label = reader.ReadSizedString_BZ2_1145("label", 40, obj?.Malformations);
                    //if (obj != null) obj.label = label;
                    reader.ReadSizedString("label", obj, x => x.label);
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
                        //if (tok == null || !tok.Validate(null, BinaryFieldType.DATA_CHAR)) throw new Exception("Failed to parse ?/CHAR");
                        //
                        //tok = reader.ReadToken();
                        //if (tok == null || !tok.Validate("label", BinaryFieldType.DATA_CHAR))
                        //    throw new Exception("Failed to parse label/CHAR");
                        //label = tok.GetString();
                        //string label = reader.ReadSizedString_BZ2_1145("label", 40, obj?.Malformations);
                        //if (obj != null) obj.label = label;
                        reader.ReadSizedString("label", obj, x => x.label);
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
                        if (tok == null || !tok.Validate("isUser", BinaryFieldType.DATA_LONG))
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
                        if (tok == null || !tok.Validate("isUser", BinaryFieldType.DATA_BOOL))
                            throw new Exception("Failed to parse isUser/BOOL");
                        tok.ReadBoolean(obj, x => x.isUser);
                    }
                }
                //else{}
            }
            else if (reader.Format == BZNFormat.Battlezone || reader.Format == BZNFormat.BattlezoneN64)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("isUser", BinaryFieldType.DATA_LONG))
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
                    UInt64 obj_addr = reader.ReadBZ1_Ptr("obj_addr", reader.Version);
                    if (obj != null) obj.obj_addr = obj_addr;
                }
                // might have posit x,y,z here
            }
            else if (reader.Format == BZNFormat.Battlezone2)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("objAddr", BinaryFieldType.DATA_PTR))
                    throw new Exception("Failed to parse objAddr/PTR");
                if (obj != null) obj.obj_addr = tok.GetUInt32H();
            }

            if (reader.Format == BZNFormat.Battlezone2)
            {
                reader.ReadMatrix("transform", obj, x => x.transform);
            }
            if ((reader.Format == BZNFormat.Battlezone && reader.Version > 1001) || reader.Format == BZNFormat.BattlezoneN64)
            {
                reader.ReadMatrixOld("transform", obj, x => x.transform);
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
                string lookupKey = obj.PrjID.Value.ToLowerInvariant();
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
            reader.Bookmark.Mark();
                
            List<(Entity Object, bool Expected, long Next, string Name)> Candidates = new List<(Entity Object, bool Expected, long Next, string Name)>();

            foreach (var kv in parent.ClassLabelMap.OrderBy(dr => ValidClassLabels != null && ValidClassLabels.Contains(dr.Key) ? 0 : 1).ThenBy(dr => dr.Key))
            {
                string classLabel = kv.Key;
                if (!parent.LongTermClassLabelLookupCache.ContainsKey(obj.PrjID.Value.ToLowerInvariant()) || parent.LongTermClassLabelLookupCache[obj.PrjID.Value.ToLowerInvariant()].Contains(classLabel))
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
                    reader.Bookmark.RewindToBookmark();
                }
            }
            reader.Bookmark.RevertToBookmark();

            if (!parent.LongTermClassLabelLookupCache.ContainsKey(obj.PrjID.Value.ToLowerInvariant()))
                parent.LongTermClassLabelLookupCache[obj.PrjID.Value.ToLowerInvariant()] = new HashSet<string>(Candidates.Select(dr => dr.Name));
            parent.LongTermClassLabelLookupCache[obj.PrjID.Value.ToLowerInvariant()] = new HashSet<string>(parent.LongTermClassLabelLookupCache[obj.PrjID.Value.ToLowerInvariant()].Intersect(Candidates.Select(dr => dr.Name)));

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
                reader.Bookmark.Mark();
                try
                {
                    parent.TailParse(reader);
                }
                catch
                {
                    reader.Bookmark.RevertToBookmark();
                    return false;
                }
                reader.Bookmark.RevertToBookmark();
                return true;
            }
            else
            {
                if (!reader.InBinary)
                {
                    reader.Bookmark.Mark();
                    IBZNToken tok = reader.ReadToken();
                    reader.Bookmark.RevertToBookmark();
                    if (!tok.IsValidationOnly() || tok == null || !tok.Validate("GameObject", BinaryFieldType.DATA_UNKNOWN))
                    {
                        // next field isn't the start of a GameObject
                        return false;
                    }
                    return true;
                }
                else
                {
                    reader.Bookmark.Mark();
                    try
                    {
                        if (EntityDescriptor.Hydrate(parent, reader, countLeft, null, Hints: Hints))
                        {
                            reader.Bookmark.RevertToBookmark();
                            return true;
                        }
                        else
                        {
                            reader.Bookmark.RevertToBookmark();
                            return false;
                        }
                    }
                    catch
                    {
                        // next field isn't the start of a GameObject (since a shallow gameobject crashed)
                        reader.Bookmark.RevertToBookmark();
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
                if (PrjID.Value.StartsWith("bzn64prjid_"))
                {
                    string possibleLabel = PrjID.Value.Substring("bzn64prjid_".Length);
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
                        UInt16? key = lookup.Where(dr => dr.Value == PrjID.Value).Select(dr => dr.Key).FirstOrDefault();
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
                if (writer.Version == 1001)
                    writer.WriteID("PrjID", this, x => x.PrjID.Value, oneLiner: true); // confirm when we can if this actually an ID
                else
                    writer.WriteID("PrjID", this, x => x.PrjID.Value);
            }
            else if (writer.Format == BZNFormat.Battlezone2)
            {
                if (writer.Version < 1155)
                {
                    //writer.WriteGameObjectClass_BZ2(parent, "config", PrjID, Malformations);
                    writer.WriteSizedString("config", this, x => x.PrjID);
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
                             //writer.WriteGameObjectClass_BZ2(parent, "GetClass()", PrjID, Malformations);
                             writer.WriteSizedString("GetClass()", this, x => x.PrjID);
                        }
                        else
                        {
                            //writer.WriteGameObjectClass_BZ2(parent, "objClass", PrjID, Malformations);
                            writer.WriteSizedString("objClass", this, x => x.PrjID);
                        }
                    }
                }
            }

            if (writer.Format == BZNFormat.Battlezone2)
            {
                if (writer.Version == 1101 || writer.Version == 1070)
                {
                    writer.WriteUnsignedHexLValues("seqno", (UInt16)seqNo);
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
                writer.WriteVector3Ds("pos", preserveMalformations, pos);
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
                if (label != null && label.Value.StartsWith("bzn64label_"))
                {
                    string possibleLabelNum = label.Value.Substring("bzn64label_".Length);
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
                writer.WriteChars("label", this, x => x.label);
            }
            else if (writer.Format == BZNFormat.Battlezone2)
            {
                if (writer.Version < 1145)
                {
                    //writer.WriteSizedString_BZ2_1145("label", 40, label, Malformations);
                    writer.WriteSizedString("label", this, x => x.label);
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
                        //writer.WriteSizedString_BZ2_1145("label", 40, label, Malformations);
                        writer.WriteSizedString("label", this, x => x.label);
                    }
                }
            }

            if (writer.Format == BZNFormat.Battlezone2)
            {
                if (parent.SaveType != SaveType.JOIN)
                {
                    if (writer.Version < 1145)
                    {
//                        var mals = Malformations.GetMalformations(Malformation.INCORRECT_RAW, "isUser");
//                        if (preserveMalformations && mals.Length > 0)
//                        {
//                            UInt32 malValue = (UInt32)mals[0].Fields[0];
//                            // consider clearing malformations when you edit a malformed field
//
//                            writer.WriteUnsignedValues("isUser", isUser ? malValue : 0U);
//                        }
//                        else
                        {
                            writer.WriteUnsignedValues("isUser", isUser ? 1U : 0U);
                        }
                    }
                    else
                    {
                        writer.WriteBoolean("isUser", this, x => x.isUser);
                    }
                }
                //else{}
            }
            else if (writer.Format == BZNFormat.Battlezone || writer.Format == BZNFormat.BattlezoneN64)
            {
//                var mals = Malformations.GetMalformations(Malformation.INCORRECT_RAW, "isUser");
//                if (preserveMalformations && mals.Length > 0)
//                {
//                    UInt32 malValue = (UInt32)mals[0].Fields[0];
//                    // consider clearing malformations when you edit a malformed field
//
//                    writer.WriteUnsignedValues("isUser", isUser ? malValue : 0U);
//                }
//                else
                {
                    writer.WriteUnsignedValues("isUser", isUser ? 1U : 0U);
                }
            }

            if (writer.Format == BZNFormat.Battlezone || writer.Format == BZNFormat.BattlezoneN64)
            {
                if (writer.Format == BZNFormat.BattlezoneN64 || writer.Version < 1002)
                {
                    writer.WriteBZ1_PtrDepricated("obj_addr", (UInt32)obj_addr, raw: true); // string name unconfirmed
                }
                else
                {
                    writer.WriteBZ1_Ptr("obj_addr", obj_addr);
                }
            }
            else if (writer.Format == BZNFormat.Battlezone2)
            {
                writer.WritePtr32("objAddr", (UInt32)obj_addr);
            }

            if (writer.Format == BZNFormat.Battlezone2)
            {
                writer.WriteMatrix("transform", this, x => x.transform);
            }
            if ((writer.Format == BZNFormat.Battlezone && writer.Version > 1001) || writer.Format == BZNFormat.BattlezoneN64)
            {
                writer.WriteMatrixOld("transform", this, x => x.transform);
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
