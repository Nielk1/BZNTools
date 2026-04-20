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
            {
                obj = new ClassTurretCraft(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                return ClassTurretCraft.Hydrate(parent, reader, obj as ClassTurretCraft).Success;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
        }
    }
    public class ClassTurretCraft : ClassCraft
    {
        public UInt32[] powerHandles { get; set; }
        public SizedString saveClass { get; set; }
        public Matrix? saveMatrix { get; set; }
        public UInt32? saveTeam { get; set; }
        public UInt32? saveSeqno { get; set; }
        public SizedString? saveLabel { get; set; }
        public SizedString? saveName { get; set; }
        public Int32 scriptPowerOverride { get; set; }
        public ClassTurretCraft(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel)
        {
            powerHandles = Array.Empty<UInt32>();
            saveClass = new SizedString();
            scriptPowerOverride = -1;
        }

        public override void ClearMalformations()
        {
            Malformations.Clear();
            saveClass.ClearMalformations();
            saveMatrix?.ClearMalformations();
            saveLabel?.ClearMalformations();
            saveName?.ClearMalformations();
            base.ClearMalformations();
        }

        public override void DisableMalformationAutoFix()
        {
            saveClass.DisableMalformationAutoFix();
            saveMatrix?.DisableMalformationAutoFix();
            saveLabel?.DisableMalformationAutoFix();
            saveName?.DisableMalformationAutoFix();
            base.DisableMalformationAutoFix();
        }

        public override void EnableMalformationAutoFix()
        {
            saveClass.EnableMalformationAutoFix();
            saveMatrix?.EnableMalformationAutoFix();
            saveLabel?.EnableMalformationAutoFix();
            saveName?.EnableMalformationAutoFix();
            base.EnableMalformationAutoFix();
        }


        public static ParseResult Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassTurretCraft? obj)
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
                                if (tok != null && tok.Validate(null, BinaryFieldType.DATA_LONG)) // "powerHandle"
                                {
                                    UInt32 powerHandle = tok.GetUInt32();
                                    powerHandles.Add(powerHandle);
                                }
                                else
                                {
                                    reader.Bookmark.RevertToBookmark(); // jump back to before this item which was a non-LONG

                                    if (tok != null && tok.Validate(null /*"illumination"*/, BinaryFieldType.DATA_FLOAT))
                                    {
                                        if (reader.Version == 1041)
                                        {
                                            // version is special case for bz2001.bzn
                                            // if we're here, reading a float means it must be the illumination float of the GameObject base class
                                            // this means we didn't read an abandoned long, so we're done
                                            break;
                                        }

                                        if (powerHandles.Count == 0)
                                        {
                                            // we should have, in error, read the abandoned flag here to back out
                                            // since we didn't we know we're not a turret "craft"
                                            return ParseResult.Fail("Not a TurretCraft");
                                        }

                                        //UInt32 possibleAbandonedFlag = powerHandles.Last();
                                        //if (possibleAbandonedFlag == 0 || possibleAbandonedFlag == 1)
                                        {
                                            UInt32 possibleAbandonedFlag = powerHandles.Last();
                                            if (possibleAbandonedFlag != 0 && possibleAbandonedFlag != 1)
                                            {
                                                // very likely this isn't an abandoned flag, which it must be in all normal reads, so we are very unlikely to be this class
                                                // TODO downrank logic
                                            }

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
                                if (tok != null && tok.Validate("powerHandle", BinaryFieldType.DATA_LONG))
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
                        if (tok != null && tok.Validate("powerHandle", BinaryFieldType.DATA_LONG))
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
                        try
                        {
                            reader.ReadMatrix("saveMatrix", obj, x => x.saveMatrix!);
                            reader.Bookmark.Commit();
                        }
                        catch // TODO parse error only
                        {
                            reader.Bookmark.RevertToBookmark();
                            m_AlignsToObject = true;
                        }

                        tok = reader.ReadToken();
                        if (tok == null || !tok.Validate("saveTeam", BinaryFieldType.DATA_LONG))
                            return ParseResult.Fail("Failed to parse saveTeam/LONG");
                        //if (obj != null) obj.saveTeam = tok.GetUInt32();
                        tok.ApplyUInt32(obj, x => x.saveTeam);

                        tok = reader.ReadToken();
                        if (tok == null || !tok.Validate("saveSeqno", BinaryFieldType.DATA_LONG))
                            return ParseResult.Fail("Failed to parse saveSeqno/LONG");
                        //if (obj != null) obj.saveSeqno = tok.GetUInt32H();
                        tok.ApplyUInt32(obj, x => x.saveSeqno);

                        //tok = reader.ReadToken();
                        //if (tok == null || !tok.Validate("saveLabel", BinaryFieldType.DATA_CHAR)) throw new Exception("Failed to parse saveLabel/CHAR");
                        //tok.GetString();
                        //string saveLabel = reader.ReadBZ2InputString("saveLabel", obj?.Malformations);
                        //if (obj != null) obj.saveLabel = saveLabel;
                        reader.ReadSizedString("saveLabel", obj, x => x.saveLabel);

                        //tok = reader.ReadToken();
                        //if (tok == null || !tok.Validate("saveName", BinaryFieldType.DATA_CHAR)) throw new Exception("Failed to parse saveName/CHAR");
                        //tok.GetString();
                        //string saveName = reader.ReadBZ2InputString("saveName", obj?.Malformations);
                        //if (obj != null) obj.saveName = saveName;
                        reader.ReadSizedString("saveName", obj, x => x.saveName);
                    }
                }

                if (reader.Version >= 1193)
                {
                    // because the version needs of this are even higher than that of the above we know the above will have to have run if this will run
                    // so we know the powerHandle loop is safe since it will trip into a CHAR if it overruns due to the above.
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate("scriptPowerOverride", BinaryFieldType.DATA_LONG))
                        throw new Exception("Failed to parse scriptPowerOverride/LONG");
                    tok.ApplyInt32(obj, x => x.scriptPowerOverride);
                }

                ClassCraft.Hydrate(parent, reader, obj as ClassCraft);

                if (m_AlignsToObject)
                {
                    // saveMatrix = GetSimObjectMatrix();
                }

                return ParseResult.Ok();
            }

            return ClassCraft.Hydrate(parent, reader, obj as ClassCraft);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassTurretCraft obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
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
                            writer.WriteMatrix("saveMatrix", obj, x => x.saveMatrix!);
                        }
                        else
                        {
                            m_AlignsToObject = true;
                        }

                        writer.WriteUInt32("saveTeam", obj, x => x.saveTeam);
                        writer.WriteUInt32("saveSeqno", obj, x => x.saveSeqno);
                        writer.WriteSizedString("saveLabel", obj, x => x.saveLabel);
                        writer.WriteSizedString("saveName", obj, x => x.saveName);
                    }
                }

                if (writer.Version >= 1193)
                {
                    // because the version needs of this are even higher than that of the above we know the above will have to have run if this will run
                    // so we know the powerHandle loop is safe since it will trip into a CHAR if it overruns due to the above.
                    writer.WriteInt32("scriptPowerOverride", obj, x => x.scriptPowerOverride);
                }

                ClassCraft.Dehydrate(obj, parent, writer, binary, save);

                if (m_AlignsToObject)
                {
                    // saveMatrix = GetSimObjectMatrix();
                }

                return;
            }

            ClassCraft.Dehydrate(obj, parent, writer, binary, save);
            return;
        }
    }
}
