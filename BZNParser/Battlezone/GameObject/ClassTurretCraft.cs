using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone, "turret")]
    [ObjectClass(BZNFormat.BattlezoneN64, "turret")]
    [ObjectClass(BZNFormat.Battlezone2, "turret")]
    public class ClassTurretCraftFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
                obj = new ClassTurretCraft(preamble, classLabel);
            ClassTurretCraft.Hydrate(parent, reader, obj as ClassTurretCraft);
            return true;
        }
    }
    public class ClassTurretCraft : ClassCraft
    {
        public UInt32[] powerHandles { get; set; }
        public SizedString saveClass { get; set; }
        public Matrix? saveMatrix { get; set; }
        public UInt32? saveTeam { get; set; }
        public UInt32? saveSeqno { get; set; }
        public string? saveLabel { get; set; }
        public string? saveName { get; set; }
        public Int32? scriptPowerOverride { get; set; }
        public ClassTurretCraft(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassTurretCraft? obj)
        {

            IBZNToken? tok;

            if (reader.Format == BZNFormat.Battlezone2)
            {
                if (reader.Version >= 1068)
                {
                    if (reader.Version >= 1072)
                    {
                        // we don't know how many taps there are without the ODF, so just try to read forever
                        List<UInt32> powerHandles = new List<uint>();
                        if (reader.InBinary)
                        {
                            for (; ; )
                            {
                                reader.Bookmark.Mark();
                                tok = reader.ReadToken();
                                if (tok.Validate(null, BinaryFieldType.DATA_LONG)) // "powerHandle"
                                {
                                    UInt32 powerHandle = tok.GetUInt32();
                                    powerHandles.Add(powerHandle);
                                }
                                else
                                {
                                    reader.Bookmark.RevertToBookmark(); // jump back to before this item which was a non-LONG

                                    if (tok.Validate(null /*"illumination"*/, BinaryFieldType.DATA_FLOAT))
                                    {
                                        if (reader.Version == 1041)
                                        {
                                            // version is special case for bz2001.bzn
                                            // if we're here, reading a float means it must be the illumination float of the GameObject base class
                                            // this means we didn't read an abandoned long, so we're done
                                            break;
                                        }

                                        //UInt32 possibleAbandonedFlag = powerHandles.Last();
                                        //if (possibleAbandonedFlag == 0 || possibleAbandonedFlag == 1)
                                        {
                                            // we must have eaten an abandoned flag prior, based on its value, so lets walk back to before it and stop holding it
                                            reader.Bookmark.RevertToBookmark();
                                            powerHandles.Remove(powerHandles.Last());
                                            break;
                                        }
                                        //else
                                        //{
                                        //    // well, we ate a UInt32 that wasn't 0 or 1, so it's not an Abandoned flag for sure, so keep it
                                        //    break;
                                        //}
                                    }
                                    else
                                    {
                                        // we're done, we hit a non-LONG that is not a special case
                                        break;
                                    }
                                }
                            }
                            for (int i = 0; i < powerHandles.Count; i++)
                                reader.Bookmark.Commit(); // discard the bookmarks of the start of each powerHandle token
                        }
                        else
                        {
                            for (; ; )
                            {
                                List<UInt32> PowerHandles = new List<UInt32>();
                                reader.Bookmark.Mark();
                                tok = reader.ReadToken();
                                if (tok.Validate("powerHandle", BinaryFieldType.DATA_LONG))
                                {
                                    reader.Bookmark.Commit();
                                    UInt32 powerHandle = tok.GetUInt32();
                                    powerHandles.Add(powerHandle);
                                }
                                else
                                {
                                    reader.Bookmark.RevertToBookmark();
                                    break;
                                }
                            }
                        }
                        if (obj != null) obj.powerHandles = powerHandles.ToArray();
                    }
                    else
                    {
                        // we don't know how many taps there are without the ODF, so just try to read forever
                        reader.Bookmark.Mark();
                        tok = reader.ReadToken();
                        if (tok.Validate("powerHandle", BinaryFieldType.DATA_LONG))
                        {
                            if (obj != null) obj.powerHandles = Enumerable.Range(0, tok.GetCount()).Select(i => tok.GetUInt32(i)).ToArray();
                            reader.Bookmark.Commit();
                        }
                        else
                        {
                            reader.Bookmark.RevertToBookmark();
                        }
                    }
                }

                

                // parent.SaveType != SaveType.BZN
                /*if (a2[2].vftable)
                {
                    (a2->vftable->out_bool)(a2, this + 2376, 1, "terminalOn");
                    (a2->vftable->read_long)(a2, this + 2380, 4, "terminalUser");
                    (a2->vftable->read_long)(a2, this + 2332, 4, "originalTeam");
                    (a2->vftable->read_long)(a2, this + 2336, 4, "originalGroup");
                    v5 = 0;
                    v8 = 0;
                    if (*(this + 329) > 0)
                    {
                        v6 = (this + 2340);
                        do
                        {
                            if (*v6)
                            {
                                TurretControl::Save(*v6, a2);
                                v5 = v8;
                            }
                            v5 = (v5 + 1);
                            ++v6;
                            v8 = v5;
                        }
                        while (v5 < *(this + 329));
                        v4 = v7;
                    }
                }*/

                bool m_AlignsToObject = false;

                if (reader.Version >= 1158)
                {
                    // saveClass must have a CHAR token as its first token if it's in binary mode, meaning the above loop consuming all LONGs is fine
                    // if the version was lower we might have had a LONG conflict
                    //string saveClass = reader.ReadGameObjectClass_BZ2(parent, "saveClass", obj?.Malformations);
                    //if (obj != null) obj.saveClass = saveClass;
                    (_, string saveClass) = reader.ReadSizedString("saveClass", obj, x => x.saveClass);

                    //if (*(this + 376))
                    if (!string.IsNullOrEmpty(saveClass))
                    {
                        reader.Bookmark.Mark();
                        tok = reader.ReadToken();
                        if (tok.Validate("saveMatrix", BinaryFieldType.DATA_MAT3D))
                        {
                            reader.Bookmark.Commit();
                            Matrix saveMatrix = tok.GetMatrix();
                            if (obj != null)
                            {
                                obj.saveMatrix = saveMatrix;
                                tok.CheckMalformationsMatrix(obj.saveMatrix.Malformations, reader.FloatFormat);
                            }
                        }
                        else
                        {
                            //throw new Exception("Failed to parse saveMatrix/MAT3D"); // type not confirmed
                            reader.Bookmark.RevertToBookmark();
                            m_AlignsToObject = true;
                        }

                        tok = reader.ReadToken();
                        if (tok == null || !tok.Validate("saveTeam", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse saveTeam/LONG");
                        if (obj != null) obj.saveTeam = tok.GetUInt32();

                        tok = reader.ReadToken();
                        if (tok == null || !tok.Validate("saveSeqno", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse saveSeqno/LONG");
                        if (obj != null) obj.saveSeqno = tok.GetUInt32H();

                        //tok = reader.ReadToken();
                        //if (tok == null || !tok.Validate("saveLabel", BinaryFieldType.DATA_CHAR)) throw new Exception("Failed to parse saveLabel/CHAR");
                        //tok.GetString();
                        string saveLabel = reader.ReadBZ2InputString("saveLabel", obj?.Malformations);
                        if (obj != null) obj.saveLabel = saveLabel;

                        //tok = reader.ReadToken();
                        //if (tok == null || !tok.Validate("saveName", BinaryFieldType.DATA_CHAR)) throw new Exception("Failed to parse saveName/CHAR");
                        //tok.GetString();
                        string saveName = reader.ReadBZ2InputString("saveName", obj?.Malformations);
                        if (obj != null) obj.saveName = saveName;
                    }
                }

                if (reader.Version >= 1193)
                {
                    // because the version needs of this are even higher than that of the above we know the above will have to have run if this will run
                    // so we know the powerHandle loop is safe since it will trip into a CHAR if it overruns due to the above.
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("scriptPowerOverride", BinaryFieldType.DATA_LONG))
                        throw new Exception("Failed to parse scriptPowerOverride/LONG");
                    if (obj != null) obj.scriptPowerOverride = tok.GetInt32();
                }

                ClassCraft.Hydrate(parent, reader, obj as ClassCraft);

                if (m_AlignsToObject)
                {
                    // saveMatrix = GetSimObjectMatrix();
                }

                return;
            }

            ClassCraft.Hydrate(parent, reader, obj as ClassCraft);
            return;
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            Dehydrate(this, parent, writer, binary, save, preserveMalformations);
        }

        public static void Dehydrate(ClassTurretCraft obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            if (writer.Format == BZNFormat.Battlezone2)
            {
                if (obj.powerHandles != null)
                {
                    if (writer.Version >= 1068)
                    {
                        if (writer.Version >= 1072)
                        {
                            // we don't know how many taps there are without the ODF, so just try to read forever
                            foreach (UInt32 powerHandle in obj.powerHandles)
                            {
                                writer.WriteUnsignedValues("powerHandle", powerHandle);
                            }
                        }
                        else
                        {
                            writer.WriteUnsignedValues("powerHandle", obj.powerHandles); // length can't be over 2 without dying, maybe protect?
                        }
                    }
                }

                bool m_AlignsToObject = false;

                if (writer.Version >= 1158)
                {
                    // saveClass must have a CHAR token as its first token if it's in binary mode, meaning the above loop consuming all LONGs is fine
                    // if the version was lower we might have had a LONG conflict
                    //writer.WriteGameObjectClass_BZ2(parent, "saveClass", obj.saveClass ?? string.Empty, obj.Malformations);
                    writer.WriteSizedString("saveClass", obj, x => x.saveClass);

                    //if (*(this + 376))
                    if (!string.IsNullOrEmpty(obj.saveClass.Value))
                    {
                        if (obj.saveMatrix != null)
                        {
                            writer.WriteMat3Ds("saveMatrix", preserveMalformations, obj.saveMatrix);
                        }
                        else
                        {
                            m_AlignsToObject = true;
                        }

                        writer.WriteUnsignedValues("saveTeam", obj.saveTeam ?? 0);
                        writer.WriteUnsignedHexLValues("saveSeqno", obj.saveSeqno ?? 0);
                        writer.WriteBZ2InputString("saveLabel", obj.saveLabel, obj.Malformations);
                        writer.WriteBZ2InputString("saveName", obj.saveName, obj.Malformations);
                    }
                }

                if (writer.Version >= 1193)
                {
                    // because the version needs of this are even higher than that of the above we know the above will have to have run if this will run
                    // so we know the powerHandle loop is safe since it will trip into a CHAR if it overruns due to the above.
                    writer.WriteSignedValues("scriptPowerOverride", obj.scriptPowerOverride.Value);
                }

                ClassCraft.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);

                if (m_AlignsToObject)
                {
                    // saveMatrix = GetSimObjectMatrix();
                }

                return;
            }

            ClassCraft.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
            return;
        }
    }
}
