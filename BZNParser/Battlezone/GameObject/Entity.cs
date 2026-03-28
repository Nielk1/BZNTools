using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BZNParser.Battlezone.GameObject
{

    public class Entity : IMalformable
    {
        protected EntityDescriptor preamble;
        protected string classLabel;

        public SizedString PrjID => preamble.PrjID;
        public UInt32 seqNo { get {return preamble.seqNo; } set { preamble.seqNo = value; } }
        public Vector3D pos { get { return preamble.pos; } set { preamble.pos = value; } }
        public Vector3D? pos2 { get; set; }
        public UInt32 team { get { return preamble.team; } set { preamble.team = value; } }
        public SizedString label { get { return preamble.label; } set { preamble.label = value; } }
        public bool isUser { get { return preamble.isUser; } set { preamble.isUser = value; } }
        public UInt64 obj_addr { get { return preamble.obj_addr; } set { preamble.obj_addr = value; } }
        public Matrix transform { get { return preamble.transform; } set { preamble.transform = value; } }


        private readonly IMalformable.MalformationManager _malformationManager;
        public IMalformable.MalformationManager Malformations => _malformationManager;
        public virtual string ClassLabel { get { return classLabel; } }
        public Entity(EntityDescriptor preamble, string classLabel)
        {
            this.preamble = preamble;

            this.classLabel = classLabel;

            this._malformationManager = new IMalformable.MalformationManager(this);
        }
        public void ClearMalformations()
        {
            pos2?.ClearMalformations();
            Malformations.Clear();
        }

        public virtual void Write(BZNFileBattlezone parent, BZNStreamWriter writer, bool binary, bool save, bool preserveMalformations)
        {

        }
    }
}
