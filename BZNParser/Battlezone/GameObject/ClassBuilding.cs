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
                obj = new ClassBuilding(preamble, classLabel);
            ClassBuilding.Hydrate(parent, reader, obj as ClassBuilding);
            return true;
        }
    }
    public class ClassBuilding : ClassGameObject
    {
        // BZ1/BZn64 only
        protected bool tempBuilding { get; set; }

        // BZ2 only
        protected Matrix? saveMatrix { get; set; }
        protected string saveClass { get; set; }
        protected int saveTeam { get; set; }
        protected int saveSeqno { get; set; }
        protected string saveLabel { get; set; }
        protected string saveName { get; set; }
        public bool CLASS_m_AlignsToObject { get; private set; } // Class fields are from the ODF and are readonly
        public bool CLASS_loadAsDummy { get; private set; } // Class fields are from the ODF and are readonly

        public ClassBuilding(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel) { }
        public static void Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassBuilding? obj)
        {
            IBZNToken tok;

            if (reader.Format == BZNFormat.Battlezone2)
            {
                bool m_AlignsToObject = false;
                string saveClass = null;

                if (reader.Version >= 1147)
                {
                    //if (reader.Version == 1147 || reader.Version == 1148 || reader.Version == 1149 || reader.Version == 1151 || reader.Version == 1154)
                    if (reader.Version < 1155)
                    {
                        saveClass = reader.ReadGameObjectClass_BZ2(parent, "config");
                    }
                    else
                    {
                        saveClass = reader.ReadGameObjectClass_BZ2(parent, "saveClass");
                    }
                    if (obj != null) obj.saveClass = saveClass;

                    if (!string.IsNullOrEmpty(saveClass))
                    {
                        if (reader.Version >= 1148)
                        {
                            reader.Bookmark.Push();
                            tok = reader.ReadToken();
                            if (tok.Validate("saveMatrix", BinaryFieldType.DATA_MAT3D))
                            {
                                reader.Bookmark.Discard();
                                if (obj != null) obj.saveMatrix = tok.GetMatrix();
                            }
                            else
                            {
                                //throw new Exception("Failed to parse saveMatrix/MAT3D"); // type not confirmed
                                reader.Bookmark.Pop();
                                m_AlignsToObject = true;
                                if (obj != null) obj.CLASS_m_AlignsToObject = true;
                            }
                        }

                        tok = reader.ReadToken();
                        if (!tok.Validate("saveTeam", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse saveTeam/LONG");
                        if (obj != null) obj.saveTeam = tok.GetInt32();

                        tok = reader.ReadToken();
                        if (!tok.Validate("saveSeqno", BinaryFieldType.DATA_LONG)) throw new Exception("Failed to parse saveSeqno/LONG");
                        if (obj != null) obj.saveSeqno = tok.GetInt32H();

                        string saveLabel = reader.ReadSizedString_BZ2_1145("saveLabel", 32);
                        if (obj != null) obj.saveLabel = saveLabel;
                        string saveName = reader.ReadSizedString_BZ2_1145("saveName", 32);
                        if (obj != null) obj.saveName = saveName;
                    }
                }

                bool loadAsDummy = false;
                reader.Bookmark.Push();
                tok = reader.ReadToken();
                loadAsDummy = tok.Validate("name", BinaryFieldType.DATA_CHAR);
                reader.Bookmark.Pop();
                if (obj != null) obj.CLASS_loadAsDummy = loadAsDummy;
                if (loadAsDummy)
                {
                    string name = reader.ReadSizedString_BZ2_1145("name", 32);
                    if (obj != null) obj.name = name;
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
                if (!tok.Validate("tempBuilding", BinaryFieldType.DATA_BOOL))
                    throw new Exception("Failed to parse tempBuilding/BOOL");
                if (obj != null) obj.tempBuilding = tok.GetBoolean();
            }
            else
            {
                if (obj != null) obj.tempBuilding = false;
            }
            ClassGameObject.Hydrate(parent, reader, obj as ClassGameObject);
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            Dehydrate(this, parent, writer, binary, save, preserveMalformations);
        }

        public static void Dehydrate(ClassBuilding obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {
            if (writer.Format == BZNFormat.Battlezone2)
            {
                bool m_AlignsToObject = false;

                if (writer.Version >= 1147)
                {
                    //if (reader.Version == 1147 || reader.Version == 1148 || reader.Version == 1149 || reader.Version == 1151 || reader.Version == 1154)
                    if (writer.Version < 1155)
                    {
                        writer.WriteGameObjectClass_BZ2(parent, "config", obj.saveClass);
                    }
                    else
                    {
                        writer.WriteGameObjectClass_BZ2(parent, "saveClass", obj.saveClass ?? string.Empty);
                    }

                    if (!string.IsNullOrEmpty(obj.saveClass))
                    {
                        if (writer.Version >= 1148)
                        {
                            if (obj.saveMatrix != null)
                            {
                                writer.WriteMat3Ds("saveMatrix", preserveMalformations, obj.saveMatrix);
                            }
                            else
                            {
                                m_AlignsToObject = true;
                            }
                        }

                        writer.WriteSignedValues("saveTeam", obj.saveTeam);
                        writer.WriteUnsignedHexLValues("saveSeqno", (UInt32)obj.saveSeqno);
                        writer.WriteSizedString_BZ2_1145("saveLabel", 32, obj.saveLabel);
                        writer.WriteSizedString_BZ2_1145("saveName", 32, obj.saveName);
                    }
                }

                if (obj.CLASS_loadAsDummy)
                {
                    writer.WriteSizedString_BZ2_1145("name", 32, obj.name);
                    return;
                }

                ClassGameObject.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
                return;
            }

            // BZ1/BZn64
            if (parent.SaveType != SaveType.BZN)
            {
                writer.WriteBooleans("tempBuilding", obj.tempBuilding);
            }

            ClassGameObject.Dehydrate(obj, parent, writer, binary, save, preserveMalformations);
        }
    }
}
