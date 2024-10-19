using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows.Input;

namespace LBEE_TranslationPatch
{
    public static class InstructionProcessor
    {

        public static HashSet<char> CharCollection = new HashSet<char>();

        public static int GetCmdHeaderLength(byte[] command)
        {
            int SpecByte = command[3];
            return Math.Min(SpecByte, 2) * 2 + 4;
        }

        public static int GetStrLength(byte[] Command,int StartIndex)
        {
            int index = StartIndex;
            while (Command[index] != 0 || Command[index + 1] != 0)
            {
                index += 2;
            }
            return index - StartIndex;
        }

        public static Dictionary<byte, Func<byte[], JObject?>> InstructionGetMapping = new ()
        {
            { 0x1F, MESSAGE_GET },
            { 0x19, VARSTR_SET_GET },
            { 0x5A, TASK_GET }
        };

        public static Dictionary<byte, Func<byte[], JObject, byte[]>> InstructionSetMapping = new ()
        {
            { 0x1F, MESSAGE_SET },
            { 0x19, VARSTR_SET_SET },
            { 0x5A, TASK_SET }
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
            int index = GetCmdHeaderLength(command)+2;
            int strALength = GetStrLength(command,index);
            int strBLength = GetStrLength(command, index + strALength + 2);

            var outObj = new JObject
            {
                ["JP"] = Encoding.Unicode.GetString(command, index,strALength),
                ["EN"] = Encoding.Unicode.GetString(command, index+ strALength + 2, strBLength),
            };

            outObj["Translation"] = outObj["EN"];

            return outObj;
        }

        public static byte[] MESSAGE_SET(byte[] command, JObject inJsonObj)
        {
            int index = GetCmdHeaderLength(command)+2;
            int strStart = GetStrLength(command, index) + 2;
            int strEnd = GetStrLength(command, index + strStart);
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

        public static JObject? VARSTR_SET_GET(byte[] command)
        {
            JObject TrasnlationObj = new JObject();
            int index = GetCmdHeaderLength(command) + 2; // Header+ID
            TrasnlationObj["Text"] = Encoding.Unicode.GetString(command[index..(index+GetStrLength(command, index))]);
            TrasnlationObj["Translation"] = TrasnlationObj["Text"];
            return TrasnlationObj;
        }

        public static byte[] VARSTR_SET_SET(byte[] command, JObject inJsonObj)
        {
            int index = GetCmdHeaderLength(command) + 2; // Header+ID
            string Translation = inJsonObj["Translation"]?.Value<string>() ?? "";
            List<byte> newCommand = new List<byte>();
            newCommand.AddRange(command[..index]);
            newCommand.AddRange(Encoding.Unicode.GetBytes(Translation));
            newCommand.Add(0);
            newCommand.Add(0);
            return newCommand.ToArray();
        }

        public static JObject? TASK_GET(byte[] command)
        {
            JObject TrasnlationObj = new JObject();
            int index = GetCmdHeaderLength(command); // Header
            int TaskID = command[index] + command[index + 1] * 256;
            index += 2;
            string? msgStr_jp1 = null;
            string? msgStr_en1 = null;
            string? msgStr_jp2 = null;
            string? msgStr_en2 = null;
            if (command.Length <= index)
            {
                return null;
            }
            if (TaskID == 4)
            {
                int TaskVar1 = command[index]+command[index+1]*256;
                index += 2;
                if (command.Length <= index)
                {
                    return null;
                }
                if (TaskVar1 == 0 || TaskVar1 == 4 || TaskVar1 == 5 || TaskVar1 == 6)
                {
                    index += 2; // TaskVar2
                    if (TaskVar1 == 6)
                    {
                        index += 2; //TaskVar3
                    }
                    int strLength = GetStrLength(command, index);
                    msgStr_jp1 = Encoding.Unicode.GetString(command[index..(index + strLength)]);
                    index += strLength+2; // Include \0;
                    msgStr_en1 = Encoding.Unicode.GetString(command[index..(index + GetStrLength(command, index))]);
                }
                else if (TaskVar1 == 1)
                {
                    index += 2 * 3; // TaskVar2,3,4
                    int strLength = GetStrLength(command, index);
                    msgStr_jp1 = Encoding.Unicode.GetString(command[index..(index + strLength)]);
                    index += strLength+2;
                    strLength = GetStrLength(command, index);
                    msgStr_en1 = Encoding.Unicode.GetString(command[index..(index + strLength)]);
                    index += strLength + 2;
                    strLength = GetStrLength(command, index);
                    msgStr_jp2 = Encoding.Unicode.GetString(command[index..(index + strLength)]);
                    index += strLength+2;
                    msgStr_en2 = Encoding.Unicode.GetString(command[index..(index + GetStrLength(command, index))]);
                }
            }
            else if (TaskID == 54)
            {
                // 只有英文？有点怪
                int strLength = GetStrLength(command, index);
                msgStr_en1 = Encoding.Unicode.GetString(command[index..(index + strLength)]);
            }
            else if (TaskID == 69)
            {
                index += 2;
                int strLength = GetStrLength(command, index);
                msgStr_jp1 = Encoding.Unicode.GetString(command[index..(index + strLength)]);
                index += strLength + 2;
                strLength = GetStrLength(command, index);
                msgStr_en1 = Encoding.Unicode.GetString(command[index..(index + strLength)]);
                index += strLength + 2;
                strLength = GetStrLength(command, index);
                msgStr_jp2 = Encoding.Unicode.GetString(command[index..(index + strLength)]);
                index += strLength + 2;
                msgStr_en2 = Encoding.Unicode.GetString(command[index..(index + GetStrLength(command, index))]);
            }

            if (msgStr_en1 == null && msgStr_jp1 == null &&
                msgStr_en2 == null && msgStr_jp2 == null)
            {
                return null;
            }

            if (msgStr_jp1 != null)
            {
                TrasnlationObj["JP1"] = msgStr_jp1;
            }
            if (msgStr_jp2 != null)
            {
                TrasnlationObj["JP2"] = msgStr_jp2;
            }
            if (msgStr_en1 != null)
            {
                TrasnlationObj["EN1"] = msgStr_en1;
                TrasnlationObj["Translation1"] = msgStr_en1;
            }
            if (msgStr_en2 != null)
            {
                TrasnlationObj["EN2"] = msgStr_en2;
                TrasnlationObj["Translation2"] = msgStr_en2;
            }
            return TrasnlationObj;
        }

        public static byte[] TASK_SET(byte[] command, JObject inJsonObj)
        {
            int index = GetCmdHeaderLength(command); // Header
            int TaskID = command[index] + command[index + 1] * 256;
            index += 2;
            if (command.Length <= index)
            {
                return command;
            }
            if (TaskID == 4)
            {
                int TaskVar1 = command[index] + command[index + 1] * 256;
                index += 2;
                if (command.Length <= index)
                {
                    return command;
                }
                if (TaskVar1 == 0 || TaskVar1 == 4 || TaskVar1 == 5 || TaskVar1 == 6)
                {
                    var newCommand = new List<byte>();
                    index += 2; // TaskVar2
                    if (TaskVar1 == 6)
                    {
                        index += 2; //TaskVar3
                    }
                    index += GetStrLength(command, index) + 2;
                    newCommand.AddRange(command[..index]);
                    int strLength = GetStrLength(command, index);
                    string Translation = inJsonObj["Translation1"]?.Value<string>() ?? "";
                    newCommand.AddRange(Encoding.Unicode.GetBytes(Translation));
                    newCommand.AddRange(command.Skip(index+strLength));
                    return newCommand.ToArray();
                }
                else if (TaskVar1 == 1)
                {
                    var newCommand = new List<byte>();
                    index += 2 * 3; // TaskVar2,3,4
                    index += GetStrLength(command, index) + 2;
                    newCommand.AddRange(command[..index]); //str1

                    int strLength = GetStrLength(command, index);
                    string Translation = inJsonObj["Translation1"]?.Value<string>() ?? "";
                    newCommand.AddRange(Encoding.Unicode.GetBytes(Translation));
                    index += strLength + 2;  // str2

                    strLength = GetStrLength(command, index);
                    newCommand.AddRange(command[index..(index + strLength + 2)]);
                    index += strLength + 2; //str3

                    strLength = GetStrLength(command, index);
                    string Translation2 = inJsonObj["Translation2"]?.Value<string>() ?? "";
                    newCommand.AddRange(Encoding.Unicode.GetBytes(Translation2));
                    newCommand.AddRange(command.Skip(index + strLength)); //str4

                    return newCommand.ToArray();
                }
            }
            else if (TaskID == 54)
            {
                // 只有英文？有点怪
                int strLength = GetStrLength(command, index);
                string Translation = inJsonObj["Translation1"]?.Value<string>() ?? "";
                List<byte> newCommand = new List<byte>(command[..index]);
                newCommand.AddRange(Encoding.Unicode.GetBytes(Translation));
                newCommand.AddRange(command.Skip(index + strLength));
                return newCommand.ToArray();
            }
            else if (TaskID == 69)
            {
                var newCommand = new List<byte>();
                index += 2;
                index += GetStrLength(command, index) + 2;
                newCommand.AddRange(command[..index]); //str1

                int strLength = GetStrLength(command, index);
                string Translation = inJsonObj["Translation1"]?.Value<string>() ?? "";
                newCommand.AddRange(Encoding.Unicode.GetBytes(Translation));
                index += strLength + 2;  // str2

                strLength = GetStrLength(command, index);
                newCommand.AddRange(command[index..(index + strLength + 2)]);
                index += strLength + 2; //str3

                strLength = GetStrLength(command, index);
                string Translation2 = inJsonObj["Translation2"]?.Value<string>() ?? "";
                newCommand.AddRange(Encoding.Unicode.GetBytes(Translation2));
                newCommand.AddRange(command.Skip(index + strLength)); //str4

                return newCommand.ToArray();
            }
            return command;
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
