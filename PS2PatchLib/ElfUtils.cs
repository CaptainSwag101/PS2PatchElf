using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PS2PatchLib
{
    public struct Elf
    {
        public struct ElfHeader
        {
            public struct HeaderIdentifier
            {
                public string Magic;       // 4 bytes
                public byte Class;
                public byte ByteOrder;
                public byte Version;
                public byte OSABI;
                public byte ABIVersion;
                public byte[] Padding;     // 7 bytes
            }

            public HeaderIdentifier Identifier;
            public ushort Type;
            public ushort Machine;
            public uint Version;
            public uint Entry;
            public uint PHOffset;
            public uint SHOffset;
            public uint Flags;
            public ushort EHSize;
            public ushort PHEntrySize;
            public ushort PHNum;
            public ushort SHEntrySize;
            public ushort SHNum;
            public ushort SHStrTblIndex;
        }

        public struct ProgramHeader
        {
            public uint Type;
            public uint Offset;
            public uint VirtAddr;
            public uint PhysAddr;
            public uint FileSize;
            public uint MemSize;
            public uint Flags;
            public uint Align;
        }

        public struct SectionHeader
        {

        }

        public ElfHeader Header;
        public List<ProgramHeader> ProgramHeaders;
        public List<SectionHeader> SectionHeaders;
        public byte[] RawData;
    }

    public static class ElfUtils
    {
        public static Elf? ParseElf(string elfPath)
        {
            Elf elf;

            elf.RawData = File.ReadAllBytes(elfPath);
            using BinaryReader reader = new(new FileStream(elfPath, FileMode.Open));

            // Read ELF header identifier
            elf.Header.Identifier.Magic = new ASCIIEncoding().GetString(reader.ReadBytes(4));
            if (!elf.Header.Identifier.Magic.EndsWith("ELF"))
            {
                Console.WriteLine($"ERROR: ELF has invalid magic number: {elf.Header.Identifier.Magic}.");
                return null;
            }
            elf.Header.Identifier.Class = reader.ReadByte();
            if (elf.Header.Identifier.Class != 1)
            {
                Console.WriteLine($"ERROR: ELF specifies a 64-bit CPU, which is invalid for PS2 games.");
                return null;
            }
            elf.Header.Identifier.ByteOrder = reader.ReadByte();
            elf.Header.Identifier.Version = reader.ReadByte();
            elf.Header.Identifier.OSABI = reader.ReadByte();
            elf.Header.Identifier.ABIVersion = reader.ReadByte();
            elf.Header.Identifier.Padding = reader.ReadBytes(7);

            // Read ELF header
            elf.Header.Type = reader.ReadUInt16();
            elf.Header.Machine = reader.ReadUInt16();
            if (elf.Header.Machine != 8)
            {
                Console.WriteLine($"ERROR: ELF specifies a non-MIPS instruction set, which is invalid for PS2 games.");
                return null;
            }
            elf.Header.Version = reader.ReadUInt32();
            elf.Header.Entry = reader.ReadUInt32();
            elf.Header.PHOffset = reader.ReadUInt32();
            elf.Header.SHOffset = reader.ReadUInt32();
            elf.Header.Flags = reader.ReadUInt32();
            elf.Header.EHSize = reader.ReadUInt16();
            elf.Header.PHEntrySize = reader.ReadUInt16();
            elf.Header.PHNum = reader.ReadUInt16();
            elf.Header.SHEntrySize = reader.ReadUInt16();
            elf.Header.SHNum = reader.ReadUInt16();
            elf.Header.SHStrTblIndex = reader.ReadUInt16();

            // Read Program headers
            elf.ProgramHeaders = new();
            for (int ph = 0; ph < elf.Header.PHNum; ++ph)
            {
                Elf.ProgramHeader pHeader;

                pHeader.Type = reader.ReadUInt32();
                pHeader.Offset = reader.ReadUInt32();
                pHeader.VirtAddr = reader.ReadUInt32();
                pHeader.PhysAddr = reader.ReadUInt32();
                pHeader.FileSize = reader.ReadUInt32();
                pHeader.MemSize = reader.ReadUInt32();
                pHeader.Flags = reader.ReadUInt32();
                pHeader.Align = reader.ReadUInt32();

                elf.ProgramHeaders.Add(pHeader);
            }

            // Read Section headers
            elf.SectionHeaders = new List<Elf.SectionHeader>();
            for (int sh = 0; sh < elf.Header.SHNum; ++sh)
            {

            }

            return elf;
        }
    }
}
