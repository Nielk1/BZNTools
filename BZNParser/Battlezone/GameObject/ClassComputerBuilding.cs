using BZNParser.Tokenizer;

namespace BZNParser.Battlezone.GameObject
{
    [ObjectClass(BZNFormat.Battlezone2, "computer")]
    public class ClassComputerBuildingFactory : IClassFactory
    {
        public bool Create(BZNFileBattlezone parent, BZNStreamReader reader, EntityDescriptor preamble, string classLabel, out Entity? obj, bool create = true)
        {
            obj = null;
            if (create)
            {
                obj = new ClassComputerBuilding(preamble, classLabel);
                obj.DisableMalformationAutoFix();
            }
            try
            {
                return ClassComputerBuilding.Hydrate(parent, reader, obj as ClassComputerBuilding).Success;
            }
            finally
            {
                obj?.EnableMalformationAutoFix();
            }
        }
    }
    public class ClassComputerBuilding : ClassDummy
    {
        protected UInt32 Nozzle1_Handle { get; set; }
        protected UInt32 Nozzle2_Handle { get; set; }
        public ClassComputerBuilding(EntityDescriptor preamble, string classLabel) : base(preamble, classLabel)
        {
            Nozzle1_Handle = 0;
            Nozzle2_Handle = 0;
        }

        public override void ClearMalformations()
        {
            Malformations.Clear();
            base.ClearMalformations();
        }

        public override void DisableMalformationAutoFix()
        {
            base.DisableMalformationAutoFix();
        }

        public override void EnableMalformationAutoFix()
        {
            base.EnableMalformationAutoFix();
        }



        public static ParseResult Hydrate(BZNFileBattlezone parent, BZNStreamReader reader, ClassComputerBuilding? obj)
        {
            ClassDummy.Hydrate(parent, reader, obj as ClassDummy);

            IBZNToken? tok;

            if (reader.Version >= 1102)
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate("Nozzle1", BinaryFieldType.DATA_VOID))
                    return ParseResult.Fail("Failed to parse Nozzle1/VOID");
                if (obj != null) obj.Nozzle1_Handle = tok.GetUInt32HR();

                tok = reader.ReadToken();
                if (tok == null ||!tok.Validate("Nozzle2", BinaryFieldType.DATA_VOID))
                    return ParseResult.Fail("Failed to parse Nozzle2/VOID");
                if (obj != null) obj.Nozzle2_Handle = tok.GetUInt32HR();
            }
            else
            {
                // realy hackery here, but hopefully no BZNs exist with this
                if (obj != null)
                {
                    throw new NotImplementedException("Nozzle not implemented for version < 1102");
                    //obj.Malformations.AddNotImplemented("Nozzle1_Handle");
                    //obj.Malformations.AddNotImplemented("Nozzle2_Handle");
                }
            }

            return ParseResult.Ok();
        }

        public override void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            Dehydrate(this, parent, writer, binary, save);
        }

        public static void Dehydrate(ClassComputerBuilding obj, BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save)
        {
            ClassDummy.Dehydrate(obj, parent, writer, binary, save);

            if (writer.Version >= 1102)
            {
                writer.WriteVoidBytesL("Nozzle1", obj, x => x.Nozzle1_Handle);
                writer.WriteVoidBytesL("Nozzle2", obj, x => x.Nozzle2_Handle);
            }
        }
    }
}
