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
    public class ModHeaders
    {
        private OSHeaders _hdr;
        private MetaDataHeaders _md;
        private COR20Header _cor20;
        private MetaDataTableHeader _mdth;

        public ModHeaders(BinaryReader reader)
        {
            //read coff, pe, section headers
            _hdr = new OSHeaders(reader);

            //find and read COR20 header
            try
            {
                reader.BaseStream.Position = Rva2Offset(_hdr.PEHeader.DataDirs[14].Rva);
                _cor20 = new COR20Header(reader);
            }
            catch (Exception)
            {
                return;
            }

            //find and read md headers
            try
            {
                reader.BaseStream.Position = Rva2Offset(_cor20.MetaData.Rva);
                _md = new MetaDataHeaders(reader);
            }
            catch (Exception)
            {
                return;
            }

            try
            {
                reader.BaseStream.Position = _md.TableStreamHeader.Offset + _md.StorageSigAndHeader.Start;
                _mdth = new MetaDataTableHeader(reader);
            }
            catch
            {
            }
        }

        public uint Rva2Offset(uint rva)
        {
            for (int i = 0; i < _hdr.SectionHeaders.Length; i++)
            {
                SectionHeader sh = _hdr.SectionHeaders[i];
                if ((sh.VirtualAddress <= rva) && (sh.VirtualAddress + sh.SizeOfRawData > rva))
                {
                    return (sh.PointerToRawData + (rva - sh.VirtualAddress));
                }
            }

            throw new ModException("Module: Invalid RVA address.");
        }

        public OSHeaders OSHeaders
        {
            get
            {
                return _hdr;
            }
        }

        public MetaDataHeaders MetaDataHeaders
        {
            get
            {
                return _md;
            }
        }

        public COR20Header COR20Header
        {
            get
            {
                return _cor20;
            }
        }

        public MetaDataTableHeader MetaDataTableHeader
        {
            get
            {
                return _mdth;
            }
        }
    }
}