/***
 * 
 *  ASMEX by RiskCare Ltd.
 * 
 * This source is copyright (C) 2002 RiskCare Ltd. All rights reserved.
 * 
 * Disclaimer:
 * This code is provided 'as is', with absolutely no warranty expressed or
 * implied.  Any use of this code is at your own risk.
 *   
 * You are hereby granted the right to redistribute this source unmodified
 * in its original archive. 
 * You are hereby granted the right to use this code, or code based on it,
 * provided that you acknowledge RiskCare Ltd somewhere in the documentation
 * of your application. 
 * You are hereby granted the right to distribute changes to this source, 
 * provided that:
 * 
 * 1 -- This copyright notice is retained unchanged 
 * 2 -- Your changes are clearly marked 
 * 
 * Enjoy!
 * 
 * --------------------------------------------------------------------
 * 
 * If you use this code or have comments on it, please mail me at 
 * support@jbrowse.com or ben.peterson@riskcare.com
 * 
 */

using System;
using System.IO;

namespace Asmex.FileViewer
{
    public class MModule
    {
        private ModHeaders _mh;
        private MDStringHeap _stringHeap;
        private MDBlobHeap _blobHeap;
        private MDGUIDHeap _GUIDHeap;
        private MDBlobHeap _USHeap;
        private MDTables _tables;
        private ImpExports _imp;
        private Relocations _fix;

        public MModule(BinaryReader reader)
        {
            try
            {
                _mh = new ModHeaders(reader);
            }
            catch (Exception)
            {
                return;
            }

            //imports
            try
            {
                _imp = new ImpExports(reader, this);
            }
            catch (Exception)
            {
                return;
            }

            //relocs
            try
            {
                _fix = new Relocations(reader, this);
            }
            catch (Exception)
            {
                return;
            }

            //heaps
            try
            {
                _stringHeap = new MDStringHeap(reader, _mh.MetaDataHeaders.StringStreamHeader.Offset + _mh.MetaDataHeaders.StorageSigAndHeader.Start,
                                               _mh.MetaDataHeaders.StringStreamHeader.Size, _mh.MetaDataHeaders.StringStreamHeader.Name);
                _blobHeap = new MDBlobHeap(reader, _mh.MetaDataHeaders.BlobStreamHeader.Offset + _mh.MetaDataHeaders.StorageSigAndHeader.Start,
                                           _mh.MetaDataHeaders.BlobStreamHeader.Size, _mh.MetaDataHeaders.BlobStreamHeader.Name);
                _GUIDHeap = new MDGUIDHeap(reader, _mh.MetaDataHeaders.GUIDStreamHeader.Offset + _mh.MetaDataHeaders.StorageSigAndHeader.Start,
                                           _mh.MetaDataHeaders.GUIDStreamHeader.Size, _mh.MetaDataHeaders.GUIDStreamHeader.Name);

                if (_mh.MetaDataHeaders.USStreamHeader != null)
                {
                    _USHeap = new MDBlobHeap(reader, _mh.MetaDataHeaders.USStreamHeader.Offset + _mh.MetaDataHeaders.StorageSigAndHeader.Start,
                                             _mh.MetaDataHeaders.USStreamHeader.Size, _mh.MetaDataHeaders.USStreamHeader.Name);
                }
            }
            catch (Exception)
            {
                return;
            }

            //tables
            try
            {
                reader.BaseStream.Position = _mh.MetaDataTableHeader.End;
                _tables = new MDTables(reader, this);
            }
            catch
            {
            }
        }

        public ModHeaders ModHeaders
        {
            get
            {
                return _mh;
            }
        }

        public MDStringHeap StringHeap
        {
            get
            {
                return _stringHeap;
            }
        }

        public MDBlobHeap BlobHeap
        {
            get
            {
                return _blobHeap;
            }
        }

        public MDGUIDHeap GUIDHeap
        {
            get
            {
                return _GUIDHeap;
            }
        }

        public MDBlobHeap USHeap
        {
            get
            {
                return _USHeap;
            }
        }

        public MDTables MDTables
        {
            get
            {
                return _tables;
            }
        }

        public ImpExports ImportExport
        {
            get
            {
                return _imp;
            }
        }

        public Relocations Relocations
        {
            get
            {
                return _fix;
            }
        }

        public static int DecodeInt32(BinaryReader reader)
        {
            int length = reader.ReadByte();
            if ((length & 0x80) == 0)
            {
                return length;
            }
            if ((length & 0xC0) == 0x80)
            {
                return ((length & 0x3F) << 8) | reader.ReadByte();
            }
            return ((length & 0x3F) << 24) | (reader.ReadByte() << 16) | (reader.ReadByte() << 8) | reader.ReadByte();
        }

        public string StringFromRVA(BinaryReader reader, uint rva)
        {
            long offs = reader.BaseStream.Position;
            string s;

            try
            {
                s = "";
                uint nameOffs = ModHeaders.Rva2Offset(rva);
                reader.BaseStream.Position = nameOffs;
                byte b = reader.ReadByte();
                while (b != 0)
                {
                    s += (char) b;
                    b = reader.ReadByte();
                }
            }
            catch (Exception)
            {
                s = "Unable to find string";
            }
            finally
            {
                reader.BaseStream.Position = offs;
            }
            return s;
        }
    }
}