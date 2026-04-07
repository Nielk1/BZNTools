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
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace BZNParser.Battlezone
{
    public class EntityDescriptor : IMalformable
    {
        public SizedString PrjID { get; set; } // All
        public UInt32 seqNo { get; set; } // All
        public Vector3D pos { get; set; } // BZ1/BZn64
        public UInt32 team { get; set; } // All
        public SizedString label { get; set; } // All
        public bool isUser { get; set; } // All
        public UInt64 obj_addr { get; set; } // BZ1/BZn64
        public Matrix transform { get; set; } // All (could be missing in some BZ1 versions)

        public Entity? gameObject { get; set; }


        private readonly IMalformable.MalformationManager _malformationManager;
        public IMalformable.MalformationManager Malformations => _malformationManager;
        public EntityDescriptor()
        {
            this._malformationManager = new IMalformable.MalformationManager(this);

            this.PrjID = new SizedString("");
            this.seqNo = 0;
            this.pos = new Vector3D(0, 0, 0);
            this.team = 0;
            this.label = new SizedString("");
            this.isUser = false;
            this.obj_addr = 0;
            this.transform = new Matrix(pos);
        }
        public void ClearMalformations()
        {
            PrjID.ClearMalformations();
            pos.ClearMalformations();
            label.ClearMalformations();
            transform.ClearMalformations();
            gameObject?.ClearMalformations();
            Malformations.Clear();
        }
        public void DisableMalformationAutoFix()
        {
            PrjID.DisableMalformationAutoFix();
            pos.DisableMalformationAutoFix();
            label.DisableMalformationAutoFix();
            transform.DisableMalformationAutoFix();
            gameObject?.DisableMalformationAutoFix();
        }
        public void EnableMalformationAutoFix()
        {
            PrjID.EnableMalformationAutoFix();
            pos.EnableMalformationAutoFix();
            label.EnableMalformationAutoFix();
            transform.EnableMalformationAutoFix();
            gameObject?.EnableMalformationAutoFix();
        }

        public static bool Create(BZNFileBattlezone parent, BZNStreamReader reader, int countLeft, out EntityDescriptor? obj, bool create = true, BattlezoneBZNHints? Hints = null)
        {
            obj = null;
            if (create)
            {
                obj = new EntityDescriptor();
                obj.DisableMalformationAutoFix();
            }
            try
            {
                EntityDescriptor.Hydrate(parent, reader, countLeft, obj, Hints);
                return true;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
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
                if (tok == null)
                    throw new Exception("Failed to parse PrjID/ID");
                tok.ApplyUInt16(obj, x => x.PrjID, 0, (v) => new SizedString(parent.Hints?.EnumerationPrjID?[v] ?? string.Format("bzn64prjid_{0,4:X4}", v)));
            }
            else if (reader.Format == BZNFormat.Battlezone)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("PrjID", BinaryFieldType.DATA_ID))
                    throw new Exception("Failed to parse PrjID/ID");

                // version 1001 may require the string be 8 bytes but our only sample is 1 ASCII atm
                // version 1001 has it written as a raw 1-liner and not a normal ID, but that might be how IDs work that far back
                tok.ApplyID(obj, x => x.PrjID);
            }
            else if (reader.Format == BZNFormat.Battlezone2)
            {
                if (reader.Version < 1155)
                {
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
                            reader.ReadSizedString("GetClass()", obj, x => x.PrjID);
                        }
                        else
                        {
                            // 1183 1187 1188 1192
                            reader.ReadSizedString("objClass", obj, x => x.PrjID);
                        }
                    }
                }
            }

            uint seqNo = 0;
            if (reader.Format == BZNFormat.Battlezone2)
            {
                if (reader.Version == 1101)// || reader.Version == 1070)
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("seqno", BinaryFieldType.DATA_SHORT))
                        throw new Exception("Failed to parse seqno/SHORT");
                    (seqNo, _) = tok.ApplyUInt16h(obj, x => x.seqNo);
                }
                else if (reader.Version <= 1070)
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("seqno", BinaryFieldType.DATA_SHORT))
                        throw new Exception("Failed to parse seqno/SHORT");
                    (seqNo, _) = tok.ApplyUInt16(obj, x => x.seqNo);
                }
                else if (reader.Version <= 1100)
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("seqno", BinaryFieldType.DATA_LONG))
                        throw new Exception("Failed to parse seqno/LONG");
                    (seqNo, _) = tok.ApplyUInt32(obj, x => x.seqNo);
                }
                else
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("seqno", BinaryFieldType.DATA_LONG))
                        throw new Exception("Failed to parse seqno/LONG");
                    (seqNo, _) = tok.ApplyUInt32h(obj, x => x.seqNo);
                }
            }
            else if (reader.Format == BZNFormat.Battlezone || reader.Format == BZNFormat.BattlezoneN64)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("seqno", BinaryFieldType.DATA_SHORT))
                    throw new Exception("Failed to parse seqno/SHORT");
                (seqNo, _) = tok.ApplyUInt16(obj, x => x.seqNo);
            }

            if (reader.Format == BZNFormat.Battlezone || reader.Format == BZNFormat.BattlezoneN64)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("pos", BinaryFieldType.DATA_VEC3D))
                    throw new Exception("Failed to parse pos/VEC3D");
                tok.ApplyVector3D(obj, x => x.pos);
            }

            tok = reader.ReadToken();
            if (reader.Format == BZNFormat.Battlezone || reader.Format == BZNFormat.BattlezoneN64)
            {
                if (tok == null || !tok.Validate("team", BinaryFieldType.DATA_LONG))
                    throw new Exception("Failed to parse team/LONG");
                //if (obj != null) obj.team = tok.GetUInt32();
                tok.ApplyUInt32(obj, x => x.team);
            }
            if (reader.Format == BZNFormat.Battlezone2)
            {
                if (reader.Version < 1145)
                {
                    if (tok == null || !tok.Validate("team", BinaryFieldType.DATA_LONG))
                        throw new Exception("Failed to parse team/LONG");
                    //if (obj != null) obj.team = tok.GetUInt32();
                    tok.ApplyUInt32(obj, x => x.team);
                }
                else
                {
                    if (tok == null || !tok.Validate("team", BinaryFieldType.DATA_CHAR))
                        throw new Exception("Failed to parse team/CHAR");
                    //if (obj != null) obj.team = tok.GetUInt8(); // does this include perceived team in the high nybble? probably
                    tok.ApplyUInt8(obj, x => x.team);
                }
            }

            if (reader.Format == BZNFormat.BattlezoneN64)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("label", BinaryFieldType.DATA_SHORT))
                    throw new Exception("Failed to parse label/CHAR");
                if (obj != null) obj.label = new SizedString(string.Format("bzn64label_{0,4:X4}", tok.GetUInt16()));
            }
            else if (reader.Format == BZNFormat.Battlezone)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("label", BinaryFieldType.DATA_CHAR))
                    throw new Exception("Failed to parse label/CHAR");
                tok.ApplyChars(obj, x => x.label);
            }
            else if (reader.Format == BZNFormat.Battlezone2)
            {
                if (reader.Version < 1145)
                {
                    reader.ReadSizedString("label", obj, x => x.label);
                }
                else
                {
                    // TODO change this to set a flag
                    bool noLabel = (seqNo & 0x800000) != 0;
                    seqNo &= ~(UInt32)0x800000;

                    if (noLabel)
                    {

                    }
                    else
                    {
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
                        (_, UInt32 raw) = tok.ApplyUInt32<EntityDescriptor, bool>(obj, x => x.isUser, 0, (isUser) => isUser != 0);
                        if (raw > 1)
                            obj?.Malformations.AddIncorrectRaw<EntityDescriptor, bool>(x => x.isUser, 0, BitConverter.GetBytes(raw));
                    }
                    else
                    {
                        if (tok == null || !tok.Validate("isUser", BinaryFieldType.DATA_BOOL))
                            throw new Exception("Failed to parse isUser/BOOL");
                        tok.ApplyBoolean(obj, x => x.isUser);
                    }
                }
            }
            else if (reader.Format == BZNFormat.Battlezone || reader.Format == BZNFormat.BattlezoneN64)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("isUser", BinaryFieldType.DATA_LONG))
                    throw new Exception("Failed to parse isUser/LONG");
                (_, UInt32 raw) = tok.ApplyUInt32<EntityDescriptor, bool>(obj, x => x.isUser, 0, (isUser) => isUser != 0);
                if (raw > 1)
                    obj?.Malformations.AddIncorrectRaw<EntityDescriptor, bool>(x => x.isUser, 0, BitConverter.GetBytes(raw));
            }

            if (reader.Format == BZNFormat.Battlezone || reader.Format == BZNFormat.BattlezoneN64)
            {
                if (reader.Format == BZNFormat.BattlezoneN64 || reader.Version < 1002)
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("obj_addr", BinaryFieldType.DATA_VOID))
                        throw new Exception("Failed to parse objAddr/VOID");
                    tok.ApplyVoidBytesRaw(obj, x => x.obj_addr, 0, (v) => BitConverter.ToUInt32(v));
                }
                else
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("obj_addr", BinaryFieldType.DATA_PTR))
                        throw new Exception("Failed to parse objAddr/VOID");
                    tok.ApplyUInt32H8(obj, x => x.obj_addr);
                }
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
            else if ((reader.Format == BZNFormat.Battlezone && reader.Version > 1001) || reader.Format == BZNFormat.BattlezoneN64)
            {
                reader.ReadMatrixOld("transform", obj, x => x.transform);
            }
            else
            {
                // older BZ1 versions may not have the transform, so we just set it to identity and move on
                if (obj != null) obj.transform = new Matrix(obj.pos);
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
                    IBZNToken? tok = reader.ReadToken();
                    reader.Bookmark.RevertToBookmark();
                    if (tok == null || !tok.IsValidationOnly() || !tok.Validate("GameObject", BinaryFieldType.DATA_UNKNOWN))
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

        public void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            writer.WriteValidation("GameObject");


            if (writer.Format == BZNFormat.BattlezoneN64)
            {
                writer.WriteUInt16("PrjID", this, x => x.PrjID, (v) =>
                {
                    if (v.Value.StartsWith("bzn64prjid_"))
                    {
                        string possibleLabel = v.Value.Substring("bzn64prjid_".Length);
                        if (ushort.TryParse(possibleLabel, System.Globalization.NumberStyles.HexNumber, null, out ushort possibleItemID))
                            return possibleItemID;
                    }
                    else
                    {
                        var lookup = parent.Hints?.EnumerationPrjID;
                        if (lookup != null)
                        {
                            UInt16? key = lookup.Where(dr => dr.Value == v.Value.ToLowerInvariant()).Select(dr => dr.Key).FirstOrDefault();
                            if (key.HasValue)
                            {
                                return key.Value;
                            }
                        }
                    }
                    throw new Exception("Failed to parse dropClass/ID");
                });
            }
            else if (writer.Format == BZNFormat.Battlezone)
            {
                writer.WriteID("PrjID", this, x => x.PrjID);
            }
            else if (writer.Format == BZNFormat.Battlezone2)
            {
                if (writer.Version < 1155)
                {
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
                             writer.WriteSizedString("GetClass()", this, x => x.PrjID);
                        }
                        else
                        {
                            writer.WriteSizedString("objClass", this, x => x.PrjID);
                        }
                    }
                }
            }

            if (writer.Format == BZNFormat.Battlezone2)
            {
                if (writer.Version == 1101)// || writer.Version == 1070)
                {
                    //writer.WriteUnsignedHexLValues("seqno", (UInt16)seqNo);
                    writer.WriteUInt16h("seqno", this, x => x.seqNo);
                }
                else if (writer.Version <= 1070)
                {
                    writer.WriteUInt16("seqno", this, x => x.seqNo);
                }
                else if (writer.Version <= 1100)
                {
                    writer.WriteUInt32("seqno", this, x => x.seqNo);
                }
                else
                {
                    //writer.WriteUnsignedHexLValues("seqno", seqNo); // lowercase hex
                     writer.WriteUInt32h("seqno", this, x => x.seqNo);
                }
            }
            else if (writer.Format == BZNFormat.Battlezone || writer.Format == BZNFormat.BattlezoneN64)
            {
                //writer.WriteUnsignedValues("seqno", (UInt16)seqNo);
                writer.WriteUInt16("seqno", this, x => x.seqNo);
            }

            if (writer.Format == BZNFormat.Battlezone || writer.Format == BZNFormat.BattlezoneN64)
            {
                //writer.WriteVector3Ds("pos", pos);
                writer.WriteVector3D("pos", this, x => x.pos);
            }

            if (writer.Format == BZNFormat.Battlezone || writer.Format == BZNFormat.BattlezoneN64)
            {
                //writer.WriteUnsignedValues("team", team);
                writer.WriteUInt32("team", this, x => x.team);
            }
            if (writer.Format == BZNFormat.Battlezone2)
            {
                if (writer.Version < 1145)
                {
                    //writer.WriteUnsignedValues("team", team);
                    writer.WriteUInt32("team", this, x => x.team);
                }
                else
                {
                    //writer.WriteUnsignedValues("team", (byte)team);
                    writer.WriteUInt8("team", this, x => x.team);
                }
            }

            if (writer.Format == BZNFormat.BattlezoneN64)
            {
                writer.WriteUInt16(null, this, x => x.label, (label) =>
                {
                    if (label != null && label.Value.StartsWith("bzn64label_"))
                    {
                        string possibleLabelNum = label.Value.Substring("bzn64label_".Length);
                        if (ushort.TryParse(possibleLabelNum, System.Globalization.NumberStyles.HexNumber, null, out ushort possibleLabelID))
                        {
                            return possibleLabelID;
                        }
                        throw new Exception("Failed to parse label/CHAR");
                    }
                    throw new Exception("Failed to parse label/CHAR");
                });
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
                    // TODO change this to read a flag
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
                            //writer.WriteUnsignedValues("isUser", isUser ? 1U : 0U);
                            writer.WriteUInt32("isUser", this, x => x.isUser, (isUser) => isUser ? 1U : 0U);
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
                    //writer.WriteUnsignedValues("isUser", isUser ? 1U : 0U);
                    writer.WriteUInt32("isUser", this, x => x.isUser, (isUser) => isUser ? 1U : 0U);
                }
            }

            if (writer.Format == BZNFormat.Battlezone || writer.Format == BZNFormat.BattlezoneN64)
            {
                if (writer.Format == BZNFormat.BattlezoneN64 || writer.Version < 1002)
                {
                    //writer.WriteBZ1_PtrDepricated("obj_addr", (UInt32)obj_addr, raw: true); // string name unconfirmed
                    writer.WriteVoidBytesRaw("obj_addr", this, x => x.obj_addr, (v) => BitConverter.GetBytes(v).Take(4).ToArray()); // might need logic for BigEndian
                }
                else
                {
                    //writer.WriteBZ1_Ptr("obj_addr", obj_addr);
                    writer.WritePtr("obj_addr", this, x => x.obj_addr);
                }
            }
            else if (writer.Format == BZNFormat.Battlezone2)
            {
                writer.WritePtr("objAddr", this, x => x.obj_addr);
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
            if (gameObject is MultiClass mc)
            {
                // TODO if they all serialize the same, spit out that data, else throw an error
                // for now we just cheat and use the first one
                mc.Candidates.OrderBy(dr => dr.Expected ? 0 : 1).First().Object.Write(parent, writer, binary, save);
            }
            else
            {
                gameObject?.Write(parent, writer, binary, save);
            }
        }
    }
}
