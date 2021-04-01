using System;
using System.Collections.Generic;
using System.IO;
using PS2PatchLib;

namespace PS2PatchElf
{
    class Program
    {
        static void Main(string[] args)
        {
            // PS2PatchElf by CaptainSwag101
            if (args.Length < 2 || args.Length > 3)
            {
                Console.WriteLine("Improper number of arguments specified.\nPlease pass an input ELF/SLUS, a PNACH file, and optionally the patched ELF/SLUS's output path, in that order!");
                return;
            }
            
            Elf? originalElf = ElfUtils.ParseElf(args[0]);

            // Final catch-all sanity check to ensure the ELF/SLUS file is valid
            if (originalElf == null)
            {
                Console.WriteLine("Something went wrong while loading the ELF file.");
                return;
            }

            Elf patchedElf = (Elf)originalElf;

            // Verify the second file ends in .pnach
            if (!args[1].EndsWith(".pnach"))
            {
                Console.WriteLine($"ERROR: The file {args[1]} does not have a .pnach file extension.");
                return;
            }

            List<PnachCheat> cheatList = PnachUtils.ParseCheatFile(args[1]);

            foreach (PnachCheat cheat in cheatList)
            {
                // Calculate the number of bytes being modified by the cheat
                int cheatDataSize = -1;
                switch (cheat.Length)
                {
                    case "byte":
                        cheatDataSize = 1;
                        break;

                    case "short":
                        cheatDataSize = 2;
                        break;

                    case "word":
                        cheatDataSize = 4;
                        break;

                    case "extended":
                        if ((cheat.Address & 0x20000000) != 0)
                            cheatDataSize = 4;
                        else if ((cheat.Address & 0x10000000) != 0)
                            cheatDataSize = 2;
                        else
                            cheatDataSize = 1;
                        break;
                }

                // Correct the cheat address to remove the first hex digit
                uint adjustedCheatAddress = cheat.Address & 0x0FFFFFFF;

                // Check which Program header the cheat address lines up with
                bool foundProgHeader = false;
                foreach (Elf.ProgramHeader pHeader in patchedElf.ProgramHeaders)
                {
                    if (pHeader.VirtAddr <= adjustedCheatAddress && (pHeader.VirtAddr + pHeader.FileSize) > adjustedCheatAddress + cheatDataSize)
                    {
                        uint progOffset = pHeader.Offset + (adjustedCheatAddress - pHeader.VirtAddr);

                        // Modify the data in the patched ELF
                        byte[] cheatDataBytes = BitConverter.GetBytes(cheat.Data);
                        for (int b = 0; b < cheatDataSize; ++b)
                        {
                            patchedElf.RawData[progOffset + b] = cheatDataBytes[b];
                        }

                        foundProgHeader = true;
                        break;
                    }
                }

                // Sanity check: did we find a program header with an address that we could modify the data in?
                if (!foundProgHeader)
                {
                    Console.WriteLine($"ERROR: Unable to find a program header in this ELF that contains the virtual address {adjustedCheatAddress:X8}.");
                }
            }

            // Configure output file path
            string outputPath = args[0] + "_patchedElf";
            if (args.Length == 3)
                outputPath = args[2];

            File.WriteAllBytes(outputPath, patchedElf.RawData);
        }
    }
}
