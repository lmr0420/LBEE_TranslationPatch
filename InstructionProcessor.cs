using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LBEE_TranslationPatch
{
    public static class InstructionProcessor
    {

        public static HashSet<char> CharCollection = new HashSet<char>();

        public static Dictionary<byte, Func<byte[], JObject?>> InstructionGetMapping = new ()
        {
            { 0x1F, MESSAGE_GET }
        };

        public static Dictionary<byte, Func<byte[], JObject, byte[]>> InstructionSetMapping = new ()
        {
            { 0x1F, MESSAGE_SET }
        };

        public static Dictionary<byte, Func<List<LucaCommand>, int, LucaCommand[]?>> AssignCmdMapping = new ()
        {
            { 14, TAIL4Ptr_ASSIGN_CMD },    // GOTO
            { 16, TAIL4Ptr_ASSIGN_CMD },    // GOSUB
            { 17, TAIL4Ptr_ASSIGN_CMD },    // IFY
            { 18, TAIL4Ptr_ASSIGN_CMD }     // IFN
        };

        public static Dictionary<byte, Action<LucaCommand,LucaCommand[]>> FixPtrMapping = new()
        {
            { 14, TAIL4Ptr_FIX_PTR },
            { 16, TAIL4Ptr_FIX_PTR },
            { 17, TAIL4Ptr_FIX_PTR },
            { 18, TAIL4Ptr_FIX_PTR } 
        };

        public static JObject? MESSAGE_GET(byte[] command)
        {
            if(command.Length < 16)
            {
                return new JObject
                {
                    ["ERR"] = "#Invalid Instruct",
                }; ;
            }
            // 前12个字节代表各种控制符或语音参数，不重要，跳过
            int index = 12;
            while (command[index] != 0 || command[index + 1] != 0)
            {
                index += 2;
            }
            int strAEnd = index;
            index += 2;
            while (command[index] != 0 || command[index + 1] != 0)
            {
                index += 2;
            }
            int strBEnd = index;

            var outObj = new JObject
            {
                ["JP"] = Encoding.Unicode.GetString(command, 12, strAEnd - 12),
                ["EN"] = Encoding.Unicode.GetString(command, strAEnd + 2, strBEnd - strAEnd - 2),
            };

            outObj["Translation"] = outObj["EN"];

            return outObj;
        }

        public static byte[] MESSAGE_SET(byte[] command, JObject inJsonObj)
        {
            if (command.Length < 16)
            {
                return command;
            }
            // 前12个字节代表各种控制符或语音参数，不重要，跳过
            int index = 12;
            while (command[index] != 0 || command[index + 1] != 0)
            {
                index += 2;
            }
            index += 2;
            int strStart = index;
            while (command[index] != 0 || command[index + 1] != 0)
            {
                index += 2;
            }
            int strEnd = index;

            List<byte> newCommand = new List<byte>(command[..strStart]);
            string Translation = inJsonObj["Translation"]?.Value<string>()??"";
            string EN = inJsonObj["EN"]?.Value<string>()??"";
            if(Translation!=EN)
            {
                foreach(var newChar in Translation.ToCharArray())
                {
                    CharCollection.Add(newChar);
                }
            }
            newCommand.AddRange(Encoding.Unicode.GetBytes(Translation));
            newCommand.AddRange(command.Skip(strEnd));

            // 不需要修正指令长度，交由上层修复
            return newCommand.ToArray();
        }

        public static int LittleEndian2Int(byte[] InBytes)
        {
            int result = 0;
            for (int i = 0; i < 4; i++)
            {
                result |= InBytes[i] << (8 * i);
            }
            return result;
        }

        public static void Int2LittleEndian(byte[] InBytes, int Offset, int Value)
        {
            for (int i = 0; i < 4; i++)
            {
                InBytes[Offset + i] = (byte)((Value >> (8 * i)) & 0xFF);
            }
        }

        public static LucaCommand[]? TAIL4Ptr_ASSIGN_CMD(List<LucaCommand> InAllCommands,int CmdIndex)
        {
            var CurCmd = InAllCommands[CmdIndex];
            if(CurCmd.Command==null)
            {
                return null;
            }
            int TargetCmdPtr = LittleEndian2Int(CurCmd.Command[^4..]);
            for (int i = 0; i < InAllCommands.Count; i++)
            {
                if (InAllCommands[i].CmdPtr == TargetCmdPtr)
                {
                    return new LucaCommand[] { InAllCommands[i] };
                }
            }
            return null;
        }

        public static void TAIL4Ptr_FIX_PTR(LucaCommand CurCmd, LucaCommand[] InCommands)
        {
            if (CurCmd.Command != null && InCommands.Length>0)
            {
                Int2LittleEndian(CurCmd.Command, CurCmd.Command.Length - 4, InCommands[0].CmdPtr);
            }
        }
    }

}
