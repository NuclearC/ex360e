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
 * This is a patched version of original ASMEX source, able to load PE and PE+
 * files. Thanks to Sendersu for his great work on this.
 */

using System;
using System.IO;
using System.Collections;

namespace Asmex.FileViewer
{
    /// <summary>
    /// The structure with which PE files start, the DOS stub
    /// </summary>
    public class DOSStub : Region
    {
        private uint _PEPos;

        public uint PEHeaderOffset
        {
            get
            {
                return _PEPos;
            }
        }

        public DOSStub(BinaryReader reader)
        {
            if (reader.ReadUInt16() != 0x5A4D)
            {
                throw new ModException("DOSStub: Invalid DOS header.");
            }

            reader.BaseStream.Position = 0x3c;
            _PEPos = reader.ReadUInt32();

            Start = 0;
            Length = 64;
        }

        public uint PEPos
        {
            get
            {
                return _PEPos;
            }
        }
    }

    /// <summary>
    /// A single PE data directory
    /// </summary>
    public class DataDir : Region
    {
        private uint _Rva;
        private uint _Size;
        private string _Name;

        public uint Rva
        {
            get
            {
                return _Rva;
            }
        }

        public uint Size
        {
            get
            {
                return _Size;
            }
        }

        public string Name
        {
            get
            {
                return _Name;
            }
        }

        public DataDir(BinaryReader reader, string name)
        {
            Start = reader.BaseStream.Position;
            Length = 8;
            _Name = name;

            _Rva = reader.ReadUInt32();
            _Size = reader.ReadUInt32();
        }

        public override string ToString()
        {
            return base.ToString() + " " + Name + " points to {" + Rva.ToString("X8") + " - " + (Rva + Size).ToString("X8") + "}";
        }
    }

    /// <summary>
    /// The state of the image file.
    /// </summary>
    /// <see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/ms680339%28v=vs.85%29.aspx"/>
    public enum PEKind
    {
        /// <summary>
        /// Unknown image
        /// </summary>
        Unknown,

        /// <summary>
        /// The file is an executable image.
        /// </summary>
        IMAGE_NT_OPTIONAL_HDR32_MAGIC = 0x10b,

        /// <summary>
        /// The file is an executable image.
        /// </summary>
        IMAGE_NT_OPTIONAL_HDR64_MAGIC = 0x20b,

        /// <summary>
        /// The file is a ROM image.
        /// </summary>
        IMAGE_ROM_OPTIONAL_HDR_MAGIC = 0x107
    }

    /// <summary>
    /// The PE header plus the so-called optional header
    /// </summary>
    public class PEHeader : Region
    {
        private uint _Magic;
        private byte _MajorLinkerVersion;
        private byte _MinorLinkerVersion;
        private uint _SizeOfCode;
        private uint _SizeOfInitializedData;
        private uint _SizeOfUninitializedData;
        private uint _AddressOfEntryPoint;
        private uint _BaseOfCode;
        private uint _BaseOfData;
        private uint _ImageBase32;
        private ulong _ImageBase64;
        private uint _SectionAlignment;
        private uint _FileAlignment;
        private ushort _OsMajor;
        private ushort _OsMinor;
        private ushort _UserMajor;
        private ushort _UserMinor;
        private ushort _SubSysMajor;
        private ushort _SubSysMinor;
        private uint _Reserved;
        private uint _ImageSize;
        private uint _HeaderSize;
        private uint _FileChecksum;
        private ushort _SubSystem;
        private ushort _DllFlags;
        private uint _StackReserveSize32;
        private uint _StackCommitSize32;
        private uint _HeapReserveSize32;
        private uint _HeapCommitSize32;
        private ulong _StackReserveSize64;
        private ulong _StackCommitSize64;
        private ulong _HeapReserveSize64;
        private ulong _HeapCommitSize64;
        private uint _LoaderFlags;
        private uint _NumberOfDataDirectories;
        private DataDir[] _DataDirs;
        private PEKind _PEKind;

        public uint Magic
        {
            get
            {
                return _Magic;
            }
        }

        public byte MajorLinkerVersion
        {
            get
            {
                return _MajorLinkerVersion;
            }
        }

        public byte MinorLinkerVersion
        {
            get
            {
                return _MinorLinkerVersion;
            }
        }

        public uint SizeOfCode
        {
            get
            {
                return _SizeOfCode;
            }
        }

        public uint SizeOfInitializedData
        {
            get
            {
                return _SizeOfInitializedData;
            }
        }

        public uint SizeOfUninitializedData
        {
            get
            {
                return _SizeOfUninitializedData;
            }
        }

        public uint AddressOfEntryPoint
        {
            get
            {
                return _AddressOfEntryPoint;
            }
        }

        public uint BaseOfCode
        {
            get
            {
                return _BaseOfCode;
            }
        }

        public uint BaseOfData
        {
            get
            {
                return _BaseOfData;
            }
        }

        public ulong ImageBase
        {
            get
            {
                if (this._PEKind == PEKind.IMAGE_NT_OPTIONAL_HDR32_MAGIC)
                {
                    return _ImageBase32;
                }

                if (this._PEKind == PEKind.IMAGE_NT_OPTIONAL_HDR64_MAGIC)
                {
                    return _ImageBase64;
                }

                return 0;
            }
        }

        public uint SectionAlignment
        {
            get
            {
                return _SectionAlignment;
            }
        }

        public uint FileAlignment
        {
            get
            {
                return _FileAlignment;
            }
        }

        public ushort OsMajor
        {
            get
            {
                return _OsMajor;
            }
        }

        public ushort OsMinor
        {
            get
            {
                return _OsMinor;
            }
        }

        public ushort UserMajor
        {
            get
            {
                return _UserMajor;
            }
        }

        public ushort UserMinor
        {
            get
            {
                return _UserMinor;
            }
        }

        public ushort SubSysMajor
        {
            get
            {
                return _SubSysMajor;
            }
        }

        public ushort SubSysMinor
        {
            get
            {
                return _SubSysMinor;
            }
        }

        public uint Reserved
        {
            get
            {
                return _Reserved;
            }
        }

        public uint ImageSize
        {
            get
            {
                return _ImageSize;
            }
        }

        public uint HeaderSize
        {
            get
            {
                return _HeaderSize;
            }
        }

        public uint FileChecksum
        {
            get
            {
                return _FileChecksum;
            }
        }

        public ushort SubSystem
        {
            get
            {
                return _SubSystem;
            }
        }

        public ushort DllFlags
        {
            get
            {
                return _DllFlags;
            }
        }

        public ulong StackReserveSize
        {
            get
            {
                if (this._PEKind == PEKind.IMAGE_NT_OPTIONAL_HDR32_MAGIC)
                {
                    return _StackReserveSize32;
                }

                if (this._PEKind == PEKind.IMAGE_NT_OPTIONAL_HDR64_MAGIC)
                {
                    return _StackReserveSize64;
                }

                return 0;
            }
        }

        public ulong StackCommitSize
        {
            get
            {
                if (this._PEKind == PEKind.IMAGE_NT_OPTIONAL_HDR32_MAGIC)
                {
                    return _StackCommitSize32;
                }

                if (this._PEKind == PEKind.IMAGE_NT_OPTIONAL_HDR64_MAGIC)
                {
                    return _StackCommitSize64;
                }

                return 0;
            }
        }

        public ulong HeapReserveSize
        {
            get
            {
                if (this._PEKind == PEKind.IMAGE_NT_OPTIONAL_HDR32_MAGIC)
                {
                    return _HeapReserveSize32;
                }

                if (this._PEKind == PEKind.IMAGE_NT_OPTIONAL_HDR64_MAGIC)
                {
                    return _HeapReserveSize64;
                }

                return 0;
            }
        }

        public ulong HeapCommitSize
        {
            get
            {
                if (this._PEKind == PEKind.IMAGE_NT_OPTIONAL_HDR32_MAGIC)
                {
                    return _HeapCommitSize32;
                }

                if (this._PEKind == PEKind.IMAGE_NT_OPTIONAL_HDR64_MAGIC)
                {
                    return _HeapCommitSize64;
                }

                return 0;
            }
        }

        public uint LoaderFlags
        {
            get
            {
                return _LoaderFlags;
            }
        }

        public uint NumberOfDataDirectories
        {
            get
            {
                return _NumberOfDataDirectories;
            }
        }

        public DataDir[] DataDirs
        {
            get
            {
                return _DataDirs;
            }
        }

        public PEKind PEImageType
        {
            get
            {
                return this._PEKind;
            }
        }

        public string PEImageTypeDescription
        {
            get
            {
                switch (this.PEImageType)
                {
                    case PEKind.IMAGE_NT_OPTIONAL_HDR32_MAGIC:
                        return "PE";
                    case PEKind.IMAGE_NT_OPTIONAL_HDR64_MAGIC:
                        return "PE32+";
                    case PEKind.IMAGE_ROM_OPTIONAL_HDR_MAGIC:
                        return "ROM";
                    default:
                        return "Unknown";
                }
            }
        }

        public PEHeader(BinaryReader reader)
        {
            Start = reader.BaseStream.Position;

            // Read Standard fields
            _Magic = reader.ReadUInt16();

            try
            {
                this._PEKind = (PEKind) Enum.Parse(typeof (PEKind), this._Magic.ToString(), true);
            }
            catch (ArgumentException)
            {
            }

            if ((this._PEKind == PEKind.IMAGE_NT_OPTIONAL_HDR32_MAGIC) || (this._PEKind == PEKind.IMAGE_NT_OPTIONAL_HDR64_MAGIC))
            {
                _MajorLinkerVersion = reader.ReadByte();
                _MinorLinkerVersion = reader.ReadByte();
                _SizeOfCode = reader.ReadUInt32();
                _SizeOfInitializedData = reader.ReadUInt32();
                _SizeOfUninitializedData = reader.ReadUInt32();
                _AddressOfEntryPoint = reader.ReadUInt32();
                _BaseOfCode = reader.ReadUInt32();

                // Read NT-specific fields
                // Many thanks to Sendersu about spotting this out
                if (this._PEKind == PEKind.IMAGE_NT_OPTIONAL_HDR32_MAGIC)
                {
                    _BaseOfData = reader.ReadUInt32();
                    _ImageBase32 = reader.ReadUInt32();
                }
                else if (this._PEKind == PEKind.IMAGE_NT_OPTIONAL_HDR64_MAGIC)
                {
                    _ImageBase64 = reader.ReadUInt64();
                }
                _SectionAlignment = reader.ReadUInt32();
                _FileAlignment = reader.ReadUInt32();
                _OsMajor = reader.ReadUInt16();
                _OsMinor = reader.ReadUInt16();
                _UserMajor = reader.ReadUInt16();
                _UserMinor = reader.ReadUInt16();
                _SubSysMajor = reader.ReadUInt16();
                _SubSysMinor = reader.ReadUInt16();
                _Reserved = reader.ReadUInt32();
                _ImageSize = reader.ReadUInt32();
                _HeaderSize = reader.ReadUInt32();
                _FileChecksum = reader.ReadUInt32();
                _SubSystem = reader.ReadUInt16();
                _DllFlags = reader.ReadUInt16();
                if (this._PEKind == PEKind.IMAGE_NT_OPTIONAL_HDR32_MAGIC)
                {
                    _StackReserveSize32 = reader.ReadUInt32();
                    _StackCommitSize32 = reader.ReadUInt32();
                    _HeapReserveSize32 = reader.ReadUInt32();
                    _HeapCommitSize32 = reader.ReadUInt32();
                }
                else if (this._PEKind == PEKind.IMAGE_NT_OPTIONAL_HDR64_MAGIC)
                {
                    _StackReserveSize64 = reader.ReadUInt64();
                    _StackCommitSize64 = reader.ReadUInt64();
                    _HeapReserveSize64 = reader.ReadUInt64();
                    _HeapCommitSize64 = reader.ReadUInt64();
                }
                _LoaderFlags = reader.ReadUInt32();
                _NumberOfDataDirectories = reader.ReadUInt32();
                if (NumberOfDataDirectories < 16)
                {
                    throw new ModException("PEHeader: Invalid number of data directories in file header.");
                }

                _DataDirs = new DataDir[NumberOfDataDirectories];

                string[] PEDirNames = new[]
                                      {
                                          "Export Table", "Import Table", "Resource Table", "Exception Table", "Certificate Table",
                                          "Base Relocation Table", "Debug", "Copyright", "Global Ptr", "TLS Table", "Load Config Table", "Bound Import",
                                          "IAT", "Delay Import Descriptor", "CLI Header", "Reserved"
                                      };

                for (int i = 0; i < NumberOfDataDirectories; ++i)
                {
                    _DataDirs[i] = new DataDir(reader, (i < 16) ? PEDirNames[i] : "Unknown");
                }

                Length = reader.BaseStream.Position - Start;
            }
            else
            {
                throw new ModException("PEHeader: Loaded module is not a recognized PE / PE+ file");
            }
        }
    }

    public class COFFHeader : Region
    {
        private ushort _Machine;
        private ushort _NumberOfSections;
        private uint _TimeDateStamp;
        private uint _SymbolTablePointer;
        private uint _NumberOfSymbols;
        private ushort _OptionalHeaderSize;
        private ushort _Characteristics;

        public ushort Machine
        {
            get
            {
                return _Machine;
            }
        }

        public ArchitectureType MachineType
        {
            get
            {
                try
                {
                    return (ArchitectureType) Enum.Parse(typeof (ArchitectureType), this._Machine.ToString(), true);
                }
                catch (ArgumentException)
                {
                    return ArchitectureType.IMAGE_FILE_MACHINE_UNKNOWN;
                }
            }
        }

        public string MachineTypeDescription
        {
            get
            {
                switch (this.MachineType)
                {
                    case ArchitectureType.IMAGE_FILE_MACHINE_I386:
                        return "I386";
                    case ArchitectureType.IMAGE_FILE_MACHINE_AMD64:
                        return "AMD64";
                    case ArchitectureType.IMAGE_FILE_MACHINE_IA64:
                        return "IA64";
                    default:
                        return "Unsupported architecture";
                }
            }
        }

        public ushort NumberOfSections
        {
            get
            {
                return _NumberOfSections;
            }
        }

        public uint TimeDateStamp
        {
            get
            {
                return _TimeDateStamp;
            }
        }

        public uint SymbolTablePointer
        {
            get
            {
                return _SymbolTablePointer;
            }
        }

        public uint NumberOfSymbols
        {
            get
            {
                return _NumberOfSymbols;
            }
        }

        public ushort OptionalHeaderSize
        {
            get
            {
                return _OptionalHeaderSize;
            }
        }

        public ushort Characteristics
        {
            get
            {
                return _Characteristics;
            }
        }

        public COFFHeader(BinaryReader reader)
        {
            Start = reader.BaseStream.Position;
            Length = 20;

            _Machine = reader.ReadUInt16();

            if ((this.MachineType == ArchitectureType.IMAGE_FILE_MACHINE_I386) || (this.MachineType == ArchitectureType.IMAGE_FILE_MACHINE_AMD64) ||
                (this.MachineType == ArchitectureType.IMAGE_FILE_MACHINE_IA64))
            {
                _NumberOfSections = reader.ReadUInt16();
                _TimeDateStamp = reader.ReadUInt32();
                _SymbolTablePointer = reader.ReadUInt32();
                _NumberOfSymbols = reader.ReadUInt32();
                _OptionalHeaderSize = reader.ReadUInt16();
                _Characteristics = reader.ReadUInt16();
            }
            else
            {
                throw new ModException("COFFHeader: The architecture type of the file is not included in supported types (I386, AMD64, IA64)");
            }
        }
    }

    /// <summary>
    /// The architecture type of the computer. An image file can only be run on the specified computer or a system that emulates the specified computer.
    /// </summary>
    /// <see cref="http://msdn.microsoft.com/en-us/library/ms680313%28VS.85%29.aspx"/>
    public enum ArchitectureType : ushort
    {
        IMAGE_FILE_MACHINE_UNKNOWN = 0x0,
        IMAGE_FILE_MACHINE_AM33 = 0x1d3,
        IMAGE_FILE_MACHINE_AMD64 = 0x8664,
        IMAGE_FILE_MACHINE_ARM = 0x1c0,
        IMAGE_FILE_MACHINE_EBC = 0xebc,
        IMAGE_FILE_MACHINE_I386 = 0x14c,
        IMAGE_FILE_MACHINE_IA64 = 0x200,
        IMAGE_FILE_MACHINE_M32R = 0x9041,
        IMAGE_FILE_MACHINE_MIPS16 = 0x266,
        IMAGE_FILE_MACHINE_MIPSFPU = 0x366,
        IMAGE_FILE_MACHINE_MIPSFPU16 = 0x466,
        IMAGE_FILE_MACHINE_POWERPC = 0x1f0,
        IMAGE_FILE_MACHINE_POWERPCFP = 0x1f1,
        IMAGE_FILE_MACHINE_R4000 = 0x166,
        IMAGE_FILE_MACHINE_SH3 = 0x1a2,
        IMAGE_FILE_MACHINE_SH3DSP = 0x1a3,
        IMAGE_FILE_MACHINE_SH4 = 0x1a6,
        IMAGE_FILE_MACHINE_SH5 = 0x1a8,
        IMAGE_FILE_MACHINE_THUMB = 0x1c2,
        IMAGE_FILE_MACHINE_WCEMIPSV2 = 0x169
    }

    /// <summary>
    /// Describes a PE file section
    /// </summary>
    public class SectionHeader : Region
    {
        private string _Name;
        private uint _Misc;
        private uint _VirtualAddress;
        private uint _SizeOfRawData;
        private uint _PointerToRawData;
        private uint _PointerToRelocations;
        private uint _PointerToLinenumbers;
        private ushort _NumberOfRelocations;
        private ushort _NumberOfLinenumbers;
        private uint _Characteristics;

        public string Name
        {
            get
            {
                return _Name;
            }
        }

        public uint Misc
        {
            get
            {
                return _Misc;
            }
        }

        public uint VirtualAddress
        {
            get
            {
                return _VirtualAddress;
            }
        }

        public uint SizeOfRawData
        {
            get
            {
                return _SizeOfRawData;
            }
        }

        public uint PointerToRawData
        {
            get
            {
                return _PointerToRawData;
            }
        }

        public uint PointerToRelocations
        {
            get
            {
                return _PointerToRelocations;
            }
        }

        public uint PointerToLinenumbers
        {
            get
            {
                return _PointerToLinenumbers;
            }
        }

        public ushort NumberOfRelocations
        {
            get
            {
                return _NumberOfRelocations;
            }
        }

        public ushort NumberOfLinenumbers
        {
            get
            {
                return _NumberOfLinenumbers;
            }
        }

        public uint Characteristics
        {
            get
            {
                return _Characteristics;
            }
        }

        public SectionHeader(BinaryReader reader)
        {
            Start = reader.BaseStream.Position;
            Length = 40;

            for (int i = 0; i < 8; ++i)
            {
                byte b = reader.ReadByte();
                if (b != 0)
                {
                    _Name += (char) b;
                }
            }

            _Misc = reader.ReadUInt32();
            _VirtualAddress = reader.ReadUInt32();
            _SizeOfRawData = reader.ReadUInt32();
            _PointerToRawData = reader.ReadUInt32();
            _PointerToRelocations = reader.ReadUInt32();
            _PointerToLinenumbers = reader.ReadUInt32();
            _NumberOfRelocations = reader.ReadUInt16();
            _NumberOfLinenumbers = reader.ReadUInt16();
            _Characteristics = reader.ReadUInt32();
        }

        public override string ToString()
        {
            return base.ToString() + " " + Name + " raw data at offsets {" + PointerToRawData.ToString("X8") + " - " +
                   (PointerToRawData + SizeOfRawData).ToString("X8") + "}";
        }
    }

    /// <summary>
    /// All the non-.NET specific headers in a PE file are gathered in this object
    /// </summary>
    public class OSHeaders : Region
    {
        private DOSStub _stub;
        private COFFHeader _coh;
        private PEHeader _peh;

        private SectionHeader[] _sech;

        private long _dataSectionsOffset;

        public OSHeaders(BinaryReader reader)
        {
            Start = 0;

            _stub = new DOSStub(reader);
            reader.BaseStream.Position = _stub.PEPos;

            // Read "PE\0\0" signature
            if (reader.ReadUInt32() != 0x00004550)
            {
                throw new ModException("File is not a portable executable.");
            }

            _coh = new COFFHeader(reader);

            // Compute data sections offset
            _dataSectionsOffset = reader.BaseStream.Position + _coh.OptionalHeaderSize;

            _peh = new PEHeader(reader);

            reader.BaseStream.Position = _dataSectionsOffset;

            _sech = new SectionHeader[_coh.NumberOfSections];

            for (int i = 0; i < _sech.Length; i++)
            {
                _sech[i] = new SectionHeader(reader);
            }

            Length = reader.BaseStream.Position;
        }

        public DOSStub DOSStub
        {
            get
            {
                return _stub;
            }
        }

        public COFFHeader COFFHeader
        {
            get
            {
                return _coh;
            }
        }

        public PEHeader PEHeader
        {
            get
            {
                return _peh;
            }
        }

        public SectionHeader[] SectionHeaders
        {
            get
            {
                return _sech;
            }
        }
    }

    /// <summary>
    /// An entry in a PE import table
    /// </summary>
    public class ImportAddress
    {
        private bool _ByOrdinal;
        private uint _Ordinal;
        private string _Name;

        public bool ByOrdinal
        {
            get
            {
                return _ByOrdinal;
            }
            set
            {
                _ByOrdinal = value;
            }
        }

        public uint Ordinal
        {
            get
            {
                return _Ordinal;
            }
            set
            {
                _Ordinal = value;
            }
        }

        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
            }
        }

        public ImportAddress(uint n, BinaryReader reader, MModule mod)
        {
            if ((n & 0x80000000) != 0)
            {
                _ByOrdinal = true;
            }
            else
            {
                _ByOrdinal = false;
            }

            if (_ByOrdinal)
            {
                _Ordinal = n & 0x7fffffff;
            }
            else
            {
                uint nameOffs = n & 0x7fffffff;
                long offs = reader.BaseStream.Position;
                reader.BaseStream.Position = mod.ModHeaders.Rva2Offset(nameOffs);
                reader.ReadInt16(); //hint

                byte b = reader.ReadByte();

                while (b != 0)
                {
                    _Name += (char) b;
                    b = reader.ReadByte();
                }

                reader.BaseStream.Position = offs;
            }
        }

        public override string ToString()
        {
            if (_ByOrdinal)
            {
                return "By Ordinal: " + _Ordinal;
            }

            return "By Name: " + _Name;
        }
    }

    public class ImportDirectoryEntry : Region
    {
        private ImportAddress[] _ImportLookupTable = new ImportAddress[0];
        private uint _DateTimeStamp;
        private uint _ForwarderChain;
        private string _Name;
        private uint[] _ImportAddressTable = new uint[0];

        public ImportAddress[] ImportLookupTable
        {
            get
            {
                return _ImportLookupTable;
            }
        }

        public uint DateTimeStamp
        {
            get
            {
                return _DateTimeStamp;
            }
        }

        public uint ForwarderChain
        {
            get
            {
                return _ForwarderChain;
            }
        }

        public string Name
        {
            get
            {
                return _Name;
            }
        }

        public uint[] ImportAddressTable
        {
            get
            {
                return _ImportAddressTable;
            }
        }

        public ImportDirectoryEntry(BinaryReader reader, MModule mod)
        {
            Start = reader.BaseStream.Position;

            uint iltRVA = reader.ReadUInt32();
            _DateTimeStamp = reader.ReadUInt32();
            _ForwarderChain = reader.ReadUInt32();
            uint nameRVA = reader.ReadUInt32();
            _Name = mod.StringFromRVA(reader, nameRVA);
            uint iatRVA = reader.ReadUInt32(); //can also get this from the PEHeader's data dirs

            Length = reader.BaseStream.Position - Start;

            long offs = reader.BaseStream.Position; // remember our position at the end of the imp dir entry record

            if (nameRVA == 0)
            {
                //indicate that this is not valid, because we reached the null terminating record
                //or because we are hopelessly lost
                _Name = null;
                return;
            }

            try
            {
                //get imp look table from RVA
                ArrayList arr;
                uint tableOffs, field;

                if (iltRVA != 0)
                {
                    arr = new ArrayList();
                    tableOffs = mod.ModHeaders.Rva2Offset(iltRVA);
                    reader.BaseStream.Position = tableOffs;
                    field = reader.ReadUInt32();
                    while (field != 0)
                    {
                        arr.Add(new ImportAddress(field, reader, mod));
                        field = reader.ReadUInt32();
                    }

                    _ImportLookupTable = (ImportAddress[]) arr.ToArray(typeof (ImportAddress));
                }

                //get imp Addr table from RVA
                if (iatRVA != 0)
                {
                    arr = new ArrayList();
                    tableOffs = mod.ModHeaders.Rva2Offset(iatRVA);
                    reader.BaseStream.Position = tableOffs;
                    field = reader.ReadUInt32();
                    while (field != 0)
                    {
                        arr.Add(field);
                        field = reader.ReadUInt32();
                    }

                    _ImportAddressTable = (uint[]) arr.ToArray(typeof (uint));
                }
            }
            catch
            {
            }
            finally
            {
                //restore stream pos
                reader.BaseStream.Position = offs;
            }
        }
    }

    public class ExportDirTable : Region
    {
        private uint _ExportFlags;
        private uint _TimeStamp;
        private ushort _MajorVersion;
        private ushort _MinorVersion;
        private string _Name;
        private uint _OrdinalBase;
        private uint _AddressTableEntries;
        private uint _NamePointerCount;
        private uint _ExportAddressTableRVA;
        private uint _NamePointerRVA;
        private uint _OrdinalRVA;

        public uint ExportFlags
        {
            get
            {
                return _ExportFlags;
            }
        }

        public uint TimeStamp
        {
            get
            {
                return _TimeStamp;
            }
        }

        public ushort MajorVersion
        {
            get
            {
                return _MajorVersion;
            }
        }

        public ushort MinorVersion
        {
            get
            {
                return _MinorVersion;
            }
        }

        public string Name
        {
            get
            {
                return _Name;
            }
        }

        public uint OrdinalBase
        {
            get
            {
                return _OrdinalBase;
            }
        }

        public uint AddressTableEntries
        {
            get
            {
                return _AddressTableEntries;
            }
        }

        public uint NamePointerCount
        {
            get
            {
                return _NamePointerCount;
            }
        }

        public uint ExportAddressTableRVA
        {
            get
            {
                return _ExportAddressTableRVA;
            }
        }

        public uint NamePointerRVA
        {
            get
            {
                return _NamePointerRVA;
            }
        }

        public uint OrdinalRVA
        {
            get
            {
                return _OrdinalRVA;
            }
        }

        public ExportDirTable(BinaryReader reader, MModule mod)
        {
            _ExportFlags = reader.ReadUInt32();
            _TimeStamp = reader.ReadUInt32();
            _MajorVersion = reader.ReadUInt16();
            _MinorVersion = reader.ReadUInt16();
            _Name = mod.StringFromRVA(reader, reader.ReadUInt32());
            _OrdinalBase = reader.ReadUInt32();
            _AddressTableEntries = reader.ReadUInt32();
            _NamePointerCount = reader.ReadUInt32();
            _ExportAddressTableRVA = reader.ReadUInt32();
            _NamePointerRVA = reader.ReadUInt32();
            _OrdinalRVA = reader.ReadUInt32();
        }
    }

    /// <summary>
    /// In PE files, there are 3 different tables for ordinals, addresses and names of exports.
    /// This class gathers those three things into one type for simplicity
    /// </summary>
    public class ExportRecord
    {
        private uint _ord;
        private uint _addr;
        private string _name;

        public ExportRecord(uint ord, uint addr, string name)
        {
            _ord = ord;
            _addr = addr;
            _name = name;
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public uint Address
        {
            get
            {
                return _addr;
            }
        }

        public uint Ordinal
        {
            get
            {
                return _ord;
            }
        }

        public override string ToString()
        {
            return _name + ": Ordinal " + _ord + ", Addr " + _addr.ToString("X8");
        }
    }

    /// <summary>
    /// Contains misc information about the files imports and exports
    /// </summary>
    public class ImpExports
    {
        private ImportDirectoryEntry[] _ith;
        private ExportDirTable _extab;
        private uint[] _expAddrTab;
        private string[] _expNameTab;
        private uint[] _expOrdTab;
        private ExportRecord[] _exports;

        public ImportDirectoryEntry[] ImportDirectoryEntries
        {
            get
            {
                return _ith;
            }
        }

        public ExportDirTable ExportDirectoryTable
        {
            get
            {
                return _extab;
            }
        }

        //public uint[] ExportAddressTable{get{return _expAddrTab;}}
        //public string[] ExportNameTable{get{return _expNameTab;}}
        //public uint[] ExportOrdinalTable{get{return _expOrdTab;}}
        public ExportRecord[] Exports
        {
            get
            {
                return _exports;
            }
        }

        public ImpExports(BinaryReader reader, MModule mod)
        {
            ArrayList ides = new ArrayList();
            ImportDirectoryEntry ide = null;

            _exports = new ExportRecord[0];
            _ith = new ImportDirectoryEntry[0];

            //imports 

            if (mod.ModHeaders.OSHeaders.PEHeader.DataDirs[1].Rva != 0)
            {
                uint start, end;
                start = mod.ModHeaders.Rva2Offset(mod.ModHeaders.OSHeaders.PEHeader.DataDirs[1].Rva);
                end = mod.ModHeaders.OSHeaders.PEHeader.DataDirs[1].Size + start;

                reader.BaseStream.Position = start;

                while (reader.BaseStream.Position < end)
                {
                    ide = new ImportDirectoryEntry(reader, mod);

                    //in older PEs it seems there is no null terminating entry, but in .NET ones there is.
                    if (ide.Name == null)
                    {
                        break;
                    }

                    ides.Add(ide);
                }

                _ith = (ImportDirectoryEntry[]) ides.ToArray(typeof (ImportDirectoryEntry));
            }

            //exports

            if (mod.ModHeaders.OSHeaders.PEHeader.DataDirs[0].Rva != 0)
            {
                reader.BaseStream.Position = mod.ModHeaders.Rva2Offset(mod.ModHeaders.OSHeaders.PEHeader.DataDirs[0].Rva);
                _extab = new ExportDirTable(reader, mod);

                _expAddrTab = new uint[_extab.AddressTableEntries];
                _expNameTab = new string[_extab.NamePointerCount];
                _expOrdTab = new uint[_extab.NamePointerCount];

                reader.BaseStream.Position = mod.ModHeaders.Rva2Offset(_extab.ExportAddressTableRVA);

                for (int i = 0; i < _extab.AddressTableEntries; ++i)
                {
                    _expAddrTab[i] = reader.ReadUInt32();
                }

                reader.BaseStream.Position = mod.ModHeaders.Rva2Offset(_extab.OrdinalRVA);

                for (int i = 0; i < _extab.NamePointerCount; ++i)
                {
                    _expOrdTab[i] = reader.ReadUInt16();
                }

                reader.BaseStream.Position = mod.ModHeaders.Rva2Offset(_extab.NamePointerRVA);

                for (int i = 0; i < _extab.NamePointerCount; ++i)
                {
                    _expNameTab[i] = mod.StringFromRVA(reader, reader.ReadUInt32());
                }

                //assemble array of exportrecords
                uint len = _extab.AddressTableEntries;
                if (len > _extab.NamePointerCount)
                {
                    len = _extab.NamePointerCount;
                }
                _exports = new ExportRecord[len];
                for (int i = 0; i < len; ++i)
                {
                    _exports[i] = new ExportRecord(_expOrdTab[i], _expAddrTab[i], _expNameTab[i]);
                }
            }
        }
    }

    /// <summary>
    /// One relocation table entry
    /// </summary>
    public class Relocation
    {
        private int _type;
        private int _offs;

        public int Type
        {
            get
            {
                return _type;
            }
        }

        public int Offset
        {
            get
            {
                return _offs;
            }
        }

        public string TypeName
        {
            get
            {
                switch (_type)
                {
                    case 0:
                        return "Absolute";
                    case 1:
                        return "High";
                    case 2:
                        return "Low";
                    case 3:
                        return "HighLow";
                    case 4:
                        return "HighAdj";
                    case 5:
                        return "Mips Jump";
                    case 6:
                        return "Section";
                    case 7:
                        return "Rel32";
                    case 9:
                        return "Mips Jump 16";
                    case 10:
                        return "Dir64";
                    case 11:
                        return "High32Adj";
                    default:
                        return "Unknown";
                }
            }
        }

        public Relocation(ushort n)
        {
            _type = (n & 0xf000) >> 12;
            _offs = n & 0x0fff;
        }

        public override string ToString()
        {
            return _offs.ToString("X8") + " " + TypeName;
        }
    }

    /// <summary>
    /// A PE relocation block
    /// </summary>
    public class RelocationBlock : Region
    {
        private uint _PageRVA;
        private uint _BlockSize;
        private Relocation[] _entries;

        public uint PageRVA
        {
            get
            {
                return _PageRVA;
            }
        }

        public uint BlockSize
        {
            get
            {
                return _BlockSize;
            }
        }

        public Relocation[] Relocations
        {
            get
            {
                return _entries;
            }
        }

        public RelocationBlock(BinaryReader reader)
        {
            Start = reader.BaseStream.Position;

            _PageRVA = reader.ReadUInt32();
            _BlockSize = reader.ReadUInt32();

            uint numEntries = (_BlockSize/2 - 4);

            _entries = new Relocation[numEntries];

            for (int i = 0; i < numEntries; ++i)
            {
                _entries[i] = new Relocation(reader.ReadUInt16());
            }

            Length = reader.BaseStream.Position - Start;
        }
    }

    /// <summary>
    /// The collection of all relocation blocks in a PE file
    /// </summary>
    public class Relocations : Region
    {
        private RelocationBlock[] _blox;

        public RelocationBlock[] Blocks
        {
            get
            {
                return _blox;
            }
        }

        public Relocations(BinaryReader reader, MModule mod)
        {
            uint start, end;

            if (mod.ModHeaders.OSHeaders.PEHeader.DataDirs[5].Rva == 0)
            {
                return;
            }

            start = mod.ModHeaders.Rva2Offset(mod.ModHeaders.OSHeaders.PEHeader.DataDirs[5].Rva);
            end = start + mod.ModHeaders.OSHeaders.PEHeader.DataDirs[5].Size;

            //fill in Region props
            Start = start;
            Length = end - start;

            reader.BaseStream.Position = start;

            RelocationBlock block;
            ArrayList arr = new ArrayList();

            while (reader.BaseStream.Position < end)
            {
                block = new RelocationBlock(reader);
                arr.Add(block);
            }

            _blox = (RelocationBlock[]) arr.ToArray(typeof (RelocationBlock));
        }
    }
}