using System.Collections.Generic;

namespace WoWmapperX.WoWInfoReader
{
    public class OffsetPattern
    {
        public static OffsetPattern GameState = new OffsetPattern
        {
            // Experimental WOTLK GameState offset, unsure if it'll work in every scenario because there is 60 other similar addresses.
            Pattern =
                    "5F" + // mov dword ptr ds:[eax*4+CA1610],ecx
                    "C7 05 ???????? ????????" +
                    "5E" + // add eax,01
                    "5D" + // mov dword ptr ds:[CA1654],eax
                    "C3" +
                    "33 F6" +
                    "E9 ????????" + // mov dword ptr ds:[CA1654],eax
                    "CC" +
                    "CC" +
                    "CC",
            Offset = 3
        };

        public static OffsetPattern PlayerBase = new OffsetPattern
        {
            // couldn't find sigscan patterns for player's health/maxhealth, using pointers instead.
        };

        public static OffsetPattern MouselookState = new OffsetPattern
        {
            // Experimental WOTLK MouselookState offset, only 2 similar addresses, this one should be good.
            Pattern =
                    "89 0D ????????" + //        - mov dword ptr ds:[b41828],ecx
                    "89 4D DC" + //              - mov dword ptr ss:[ebp-24],ecx
                    "89 55 E8" + //              - mov dword ptr ss:[ebp-18],ecx
                    "75 05" + //                 - jnz short Wow.0047FB09
                    "B8 ????????" + //           - mov eax,2
                    "8B 4D 08" + //              - mov ecx,dword ptr ss:[ebp+8]
                    "89 45 EC", //               - mov dword ptr ss:ebp-14,eax
            Offset = 2
        };

        public static OffsetPattern AoeState = new OffsetPattern
        {
            // Experimental AoeState offsets for WOTLK, seems to be working fine. needs more testing.
            Pattern =
                "55" + //                    - push ebp
                "8B EC" + //                 - mov ebp,esp
                "8B 45 08" + //              - mov eax, dword ptr ss:[ebp+8]
                "A3 ????????" + //           - mov dword ptr ds:[C25DE4],eax
                "5D" + //                    - pop ebp
                "C3" + //                    - retn
                "CC" + //                    - int3
                "CC" + //                    - int3
                "CC" + //                    - int3
                "55" + //                    - push ebp
                "8B EC" + //                 - mov ebp,esp
                "56" + //                    - push esi
                "57" + //                    - push edi
                "8B 7D 08", //               - mov edi,dword ptr ss:[ebp+8]
            Offset = 7
        };



        public static OffsetPattern WalkState = new OffsetPattern
        {
            // couldn't find sigscan patterns for WalkState, using pointers instead.
        }; 

        public string Pattern { get; set; }
        public int Offset { get; set; }

    }
}