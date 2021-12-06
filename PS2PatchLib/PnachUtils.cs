using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PS2PatchLib
{
    public struct PnachCheat
    {
        //public byte Mode;          // 1 usually, doesn't matter once converted to an ELF modification
        //public string HwTarget;    // "EE" usually, doesn't matter once converted to an ELF modification
        public uint Address;
        public string Length;
        public uint Data;
    }

    public static class PnachUtils
    {
        public static List<PnachCheat> ParseCheatFile(string pnachPath)
        {
            List<PnachCheat> cheatList = new();

            using StreamReader reader = new(pnachPath);
            string? line;
            while ((line = reader.ReadLine()?.ToLowerInvariant()) != null)
            {
                // We only care about cheat lines
                if (!line.StartsWith("patch"))
                    continue;

                string preComment = line.Split("//")[0];
                string[] splitLine = preComment.Split(',', StringSplitOptions.RemoveEmptyEntries);

                PnachCheat cheat;
                cheat.Address = uint.Parse(splitLine[2], System.Globalization.NumberStyles.HexNumber);
                cheat.Length = splitLine[3];
                cheat.Data = uint.Parse(splitLine[4], System.Globalization.NumberStyles.HexNumber);
                cheatList.Add(cheat);
            }

            return cheatList;
        }
    }
}
