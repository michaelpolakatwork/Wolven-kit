﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using WolvenKit.CR2W.Reflection;

namespace WolvenKit.CR2W.Types
{
    /// <summary>
    /// A pointer to a chunk within the same cr2w file.
    /// </summary>
    [REDMeta]
    public class CPtr<T> : CVariable, IPtrAccessor where T : CVariable
    {
       

        public CPtr(CR2WFile cr2w, CVariable parent, string name) : base(cr2w, parent, name)
        {
        }

        #region Properties

        public CR2WExportWrapper Reference { get; set; }
        public string ReferenceType => REDType.Split(':').Last();
        #endregion

        #region Methods
        public string GetPtrTargetType()
        {
            return ReferenceType;
            //try
            //{
            //    if (Reference == null)
            //        return "NULL";
            //    return Reference.REDType;
            //}
            //catch (Exception ex)
            //{
            //    throw new InvalidPtrException(ex.Message);
            //}
        }

        /// <summary>
        /// Reads an int from the stream and stores a reference to a chunk.
        /// A value of 0 means a null reference, all other chunk indeces are shifted by 1.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="size"></param>
        public override void Read(BinaryReader file, uint size)
        {
            SetValueInternal(file.ReadInt32());
        }

        private void SetValueInternal(int val)
        {
            Reference = val == 0 ? null : cr2w.chunks[val - 1];

            // Try reparenting on virtual mountpoint
            if (Reference != null)
            {
                Reference.Referrers.Add(this); //Populate the reverse-lookup

                if (/*cr2w.chunks[this.GetVarChunkIndex()].data.REDName == "CEntity" && this.ParentVar.REDName == "Components"*/false)
                {
                    return;
                }

                Reference.VirtualParentChunkIndex = GetVarChunkIndex();

/*                    case "parent" when !cr2w.chunks[GetVarChunkIndex()].IsVirtuallyMounted:
                        cr2w.chunks[GetVarChunkIndex()].VirtualParentChunkIndex = Reference.ChunkIndex;
                        case "child" when Reference.IsVirtuallyMounted:
                            Reference.IsVirtuallyMounted = false;
                            Reference.VirtualParentChunkIndex = GetVarChunkIndex();
                            break;*/
            }
        }

        public override void Write(BinaryWriter file)
        {
            int val = 0;
            if (Reference != null)
                val = Reference.ChunkIndex + 1;

            file.Write(val);
        }

        public override CVariable SetValue(object val)
        {
            switch (val)
            {
                case CR2WExportWrapper wrapper:
                    Reference = wrapper;
                    break;
                case IPtrAccessor cval:
                    Reference = cval.Reference;
                    break;
            }

            return this;
        }

        public override CVariable Copy(CR2WCopyAction context)
        {
            var copy = (CPtr<T>)base.Copy(context);

            if (Reference != null)
            {
                CR2WExportWrapper newref = context.DestinationFile.TryLookupReference(copy, Reference);
                if (newref != null)
                    copy.SetValue(newref);
            }
            

            return copy;
        }

        

        public override string ToString()
        {
            if (Reference == null)
                return "NULL";
            return $"{Reference.REDType} #{Reference.ChunkIndex}";
        }

        //public override void SerializeToXml(XmlWriter xw)
        //{
        //    DataContractSerializer ser = new DataContractSerializer(GetType());
        //    using (var ms = new MemoryStream())
        //    {
        //        ser.WriteStartObject(xw, this);
        //        ser.WriteObjectContent(xw, this);
        //        xw.WriteElementString("PtrTargetType", GetPtrTargetType());
        //        xw.WriteElementString("Target", ToString());
        //        ser.WriteEndObject(xw);
        //    }
        //}
        #endregion
    }

    
}