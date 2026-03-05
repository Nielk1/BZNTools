using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public ClassTurretCraft(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassTurretCraft? obj)
        {
            List<UInt32> powerHandles = new List<uint>();

            IBZNToken tok;

            if (reader.Format == BZNFormat.Battlezone2)
            {
                if (reader.Version >= 1068)
                {
                    if (reader.Version >= 1072)
                    {
                        // we don't know how many taps there are without the ODF, so just try to read forever
                        //List<UInt32> powerHandles = new List<uint>();
                        if (reader.InBinary)
                        {
                            for (; ; )
                            {
                                reader.Bookmark.Push();
                                tok = reader.ReadToken();
                                if (tok.Validate(null, BinaryFieldType.DATA_LONG)) // "powerHandle"
                                {
                                    UInt32 powerHandle = tok.GetUInt32();
                                    powerHandles.Add(powerHandle);
                                }
                                else
                                {
                                    reader.Bookmark.Pop(); // jump back to before this item which was a non-LONG

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
                                            reader.Bookmark.Pop();
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
                                reader.Bookmark.Discard(); // discard the bookmarks of the start of each powerHandle token
                        }
                        else
                        {
                            for (; ; )
                            {
                                reader.Bookmark.Push();
                                tok = reader.ReadToken();
                                if (tok.Validate("powerHandle", BinaryFieldType.DATA_LONG))
                                {
                                    reader.Bookmark.Discard();
                                    UInt32 powerHandle = tok.GetUInt32();
                                }
                                else
                                {
                                    reader.Bookmark.Pop();
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        // we don't know how many taps there are without the ODF, so just try to read forever
                        reader.Bookmark.Push();
                        tok = reader.ReadToken();
                        if (tok.Validate("powerHandle", BinaryFieldType.DATA_LONG))
                        {
                            reader.Bookmark.Discard();
                            UInt32 powerHandle = tok.GetUInt32();
                            if (tok.GetCount() > 1)
                            {
                                UInt32 powerHandle2 = tok.GetUInt32(1);
                            }
                        }
                        else
                        {
                            reader.Bookmark.Pop();
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
                    string saveClass = reader.ReadGameObjectClass_BZ2(parent, "saveClass");

                    //if (*(this + 376))
                    if (!string.IsNullOrEmpty(saveClass))
                    {
                        reader.Bookmark.Push();
                        tok = reader.ReadToken();
                        if (tok.Validate("saveMatrix", BinaryFieldType.DATA_MAT3D))
                        {
                            reader.Bookmark.Discard();
                            Matrix saveMatrix = tok.GetMatrix();
                        }
                        else
                        {
                            //throw new Exception("Failed to parse saveMatrix/MAT3D"); // type not confirmed
                            reader.Bookmark.Pop();
                            m_AlignsToObject = true;
                        }

                        tok = reader.ReadToken();
                        if (!tok.Validate("saveTeam", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse saveTeam/LONG");
                        //tok.GetUInt32();

                        tok = reader.ReadToken();
                        if (!tok.Validate("saveSeqno", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse saveSeqno/LONG");
                        //seqno = tok.GetUInt32H();

                        //tok = reader.ReadToken();
                        //if (!tok.Validate("saveLabel", BinaryFieldType.DATA_CHAR)) throw new Exception("Failed to parse saveLabel/CHAR");
                        //tok.GetString();
                        string saveLabel = reader.ReadBZ2InputString("saveLabel");

                        //tok = reader.ReadToken();
                        //if (!tok.Validate("saveName", BinaryFieldType.DATA_CHAR)) throw new Exception("Failed to parse saveName/CHAR");
                        //tok.GetString();
                        string saveName = reader.ReadBZ2InputString("saveName");
                    }
                }

                if (reader.Version >= 1193)
                {
                    // because the version needs of this are even higher than that of the above we know the above will have to have run if this will run
                    // so we know the powerHandle loop is safe since it will trip into a CHAR if it overruns due to the above.
                    tok = reader.ReadToken();
                    if (!tok.Validate("scriptPowerOverride", BinaryFieldType.DATA_LONG))
                        throw new Exception("Failed to parse scriptPowerOverride/LONG");
                    Int32 autoTarget = tok.GetInt32();
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
    }
}
