using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone, "i76building")] // is not in code in the main area but appears to be valid?
    [ObjectClass(BZNFormat.Battlezone, "i76building2")] // is in code directly
    [ObjectClass(BZNFormat.Battlezone, "i76sign")]

    [ObjectClass(BZNFormat.BattlezoneN64, "i76building")] // is not in code in the main area but appears to be valid?
    [ObjectClass(BZNFormat.BattlezoneN64, "i76building2")] // is in code directly
    [ObjectClass(BZNFormat.BattlezoneN64, "i76sign")]

    [ObjectClass(BZNFormat.Battlezone2, "i76building")]
    [ObjectClass(BZNFormat.Battlezone2, "i76sign")]

    [ObjectClass(BZNFormat.Battlezone, "repairdepot")]
    [ObjectClass(BZNFormat.BattlezoneN64, "repairdepot")]

    [ObjectClass(BZNFormat.Battlezone, "artifact")]
    [ObjectClass(BZNFormat.BattlezoneN64, "artifact")]
    [ObjectClass(BZNFormat.Battlezone2, "artifact")]
    public class ClassBuildingFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
            {
                obj = new ClassBuilding(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                ClassBuilding.Hydrate(parent, reader, obj as ClassBuilding);
                return true;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
        }
    }
    public class ClassBuilding : ClassGameObject
    {
        // BZ1/BZn64 only
        protected bool tempBuilding { get; set; } // seems to only be used in SAVEs, is forced false for BZNs


        // BZ2 only
        protected Matrix? saveMatrix { get; set; } // matrix of object replaced
        protected SizedString saveClass { get; set; } // class of object replaced
        protected int saveTeam { get; set; } // team of object replaced
        protected uint saveSeqno { get; set; } // seqno of object replaced
        protected SizedString saveLabel { get; set; } // label of object replaced
        protected SizedString saveName { get; set; } // name of object replaced


        // BZ2 ODF values (if the ODF doesn't match these a load likely fails)
        public bool CLASS_AlignsToObject { get; private set; } // Class fields are from the ODF and are readonly
        public bool CLASS_loadAsDummy { get; private set; } // Class fields are from the ODF and are readonly

        public ClassBuilding(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel)
        {
            tempBuilding = false;
            saveMatrix = null;
            saveClass = new SizedString();
            saveTeam = 0;
            saveSeqno = 0;
            saveLabel = new SizedString();
            saveName = new SizedString();
            CLASS_AlignsToObject = false;
            CLASS_loadAsDummy = false;
        }

        public override void ClearMalformations()
        {
            Malformations.Clear();
            saveMatrix?.ClearMalformations();
            saveClass.ClearMalformations();
            saveLabel.ClearMalformations();
            saveName.ClearMalformations();
            base.ClearMalformations();
        }

        public override void DisableMalformationAutoFix()
        {
            saveMatrix?.DisableMalformationAutoFix();
            saveClass.DisableMalformationAutoFix();
            saveLabel.DisableMalformationAutoFix();
            saveName.DisableMalformationAutoFix();
            base.DisableMalformationAutoFix();
        }

        public override void EnableMalformationAutoFix()
        {
            saveMatrix?.EnableMalformationAutoFix();
            saveClass.EnableMalformationAutoFix();
            saveLabel.EnableMalformationAutoFix();
            saveName.EnableMalformationAutoFix();
            base.EnableMalformationAutoFix();
        }


        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassBuilding? obj)
        {
            IBZNToken? tok;

            if (reader.Format == BZNFormat.Battlezone2)
            {
                bool m_AlignsToObject = false;
                string? saveClass = null;

                if (reader.Version >= 1147)
                {
                    if (reader.Version < 1155)
                    {
                        (_, saveClass) = reader.ReadSizedString("config", obj, x => x.saveClass);
                    }
                    else
                    {
                        (_, saveClass) = reader.ReadSizedString("saveClass", obj, x => x.saveClass);
                    }

                    if (!string.IsNullOrEmpty(saveClass))
                    {
                        if (reader.Version >= 1148)
                        {
                            reader.Bookmark.Mark();
                            try
                            {
                                reader.ReadMatrix("saveMatrix", obj, x => x.saveMatrix!);
                            }
                            catch
                            {
                                reader.Bookmark.RevertToBookmark();
                                m_AlignsToObject = true;
                                if (obj != null) obj.CLASS_AlignsToObject = true;
                            }
                        }
                        else
                        {
                            if (obj != null)
                            {
                                // we don't know if we are aligned here or not as there's no way to guess
                                // if we are not, we want to use another matrix for the saveMatrix, such as the transform matrix.
                                // TODO WARNING "Assumed ODF is AlignsToObject"
                                obj.CLASS_AlignsToObject = true;
                            }
                        }

                        tok = reader.ReadToken();
                        if (tok == null || !tok.Validate("saveTeam", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse saveTeam/LONG");
                        tok.ApplyInt32(obj, x => x.saveTeam);

                        tok = reader.ReadToken();
                        if (tok == null || !tok.Validate("saveSeqno", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse saveSeqno/LONG");
                        tok.ApplyUInt32h(obj, x => x.saveSeqno); // TODO might need to be Int32 instead of UInt32, unsure

                        reader.ReadSizedString("saveLabel", obj, x => x.saveLabel);
                        reader.ReadSizedString("saveName", obj, x => x.saveName);
                    }
                }

                bool loadAsDummy = false;
                reader.Bookmark.Mark();
                tok = reader.ReadToken();
                loadAsDummy = tok != null && tok.Validate("name", BinaryFieldType.DATA_CHAR);
                reader.Bookmark.RevertToBookmark();
                if (obj != null) obj.CLASS_loadAsDummy = loadAsDummy;
                if (loadAsDummy)
                {
                    reader.ReadSizedString("name", obj, x => x.name);
                    return;
                }

                ClassGameObject.Hydrate(parent, reader, obj as ClassGameObject);

                if (!string.IsNullOrEmpty(saveClass) && (reader.Version < 1148 || m_AlignsToObject))
                {
                    //if (obj != null) obj.saveMatrix = obj.transform; // TODO: this may be incorrect, figure that out
                }
                return;
            }

            // BZ1/BZn64
            if (parent.SaveType != SaveType.BZN)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("tempBuilding", BinaryFieldType.DATA_BOOL))
                    throw new Exception("Failed to parse tempBuilding/BOOL");
                tok.ApplyBoolean(obj, x => x.tempBuilding);
            }
            else
            {
                if (obj != null) obj.tempBuilding = false;
            }
            ClassGameObject.Hydrate(parent, reader, obj as ClassGameObject);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassBuilding obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            if (writer.Format == BZNFormat.Battlezone2)
            {
                if (writer.Version >= 1147)
                {
                    if (writer.Version < 1155)
                    {
                        writer.WriteSizedString("config", obj, x => x.saveClass);
                    }
                    else
                    {
                        writer.WriteSizedString("saveClass", obj, x => x.saveClass);
                    }

                    if (!string.IsNullOrEmpty(obj.saveClass.Value))
                    {
                        if (writer.Version >= 1148)
                        {
                            if (!obj.CLASS_AlignsToObject)
                            {
                                writer.WriteMatrix("saveMatrix", obj, x => x.saveMatrix!, (v) => v ?? obj.transform); // if we don't have a matrix, fall back to the transform
                            }
                        }

                        writer.WriteInt32("saveTeam", obj, x => x.saveTeam);
                        writer.WriteUInt32h("saveSeqno", obj, x => x.saveSeqno);
                        writer.WriteSizedString("saveLabel", obj, x => x.saveLabel);
                        writer.WriteSizedString("saveName", obj, x => x.saveName);
                    }
                }

                if (obj.CLASS_loadAsDummy)
                {
                    writer.WriteSizedString("name", obj, x => x.name);
                    return;
                }

                ClassGameObject.Dehydrate(obj, parent, writer, binary, save);
                return;
            }

            // BZ1/BZn64
            if (parent.SaveType != SaveType.BZN)
            {
                writer.WriteBoolean("tempBuilding", obj, x => x.tempBuilding);
            }

            ClassGameObject.Dehydrate(obj, parent, writer, binary, save);
        }
    }
}
