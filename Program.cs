namespace LBEE_TranslationPatch
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Newtonsoft.Json;
    using System.Collections;
    using System.Text;
    using System.Xml;
    using Newtonsoft.Json.Linq;
    using System.Security.Cryptography;
    using System.Diagnostics;
    using System.Xml.Linq;
    using System.Reflection.Metadata;

    class Program
    {
        static string LBEEGamePath = @"E:\SteamLibrary\steamapps\common\Little Busters! English Edition";
        static string LBEECharset = @".\Charset.txt";
        static string TMPPath = Path.GetFullPath(@".\.tmp");
        static string TextMappingPath = Path.GetFullPath(@".\TextMapping");
        static string ExtractedScriptPath = Path.Combine(TMPPath, "Scripts");
        static string ExtractedFontPath = Path.Combine(TMPPath, "Fonts");
        static string PendingReplacePath = Path.Combine(TMPPath, "PendingReplace");
        static List<string> IgnoredScriptList = new List<string> 
        { 
            "SEEN8500", "SEEN8501",

            // 这两个脚本里有两个未知的复杂指令ONGOTO，不清楚是做什么的
            // 但脚本本身没有包含MESSAGE指令，并且都是一些控制变量的指令，大概都是计算的逻辑，无需翻译，可以先跳过
            "SEEN8590", "SEEN8610",

            // 这些脚本应该是一些数值变量或者控制逻辑，看起来没有翻译的必要
            "_ARFLAG","_BUILD_COUNT","_CGMODE","_QUAKE",
            "_SCR_LABEL","_TASK","_VARNUM","_VOICE_PARAM"
        };

        static void Main(string[] args)
        {
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Environment.ProcessPath!)!);
            if(args.Length>0)
            {
                string LBEE_Exe = args[0];
                // 获取LBEE_Exe所在文件夹
                LBEEGamePath = Path.GetDirectoryName(LBEE_Exe)??"";
            }
            if(!Path.Exists(LBEEGamePath))
            {
                Console.WriteLine("找不到LBEE的游戏目录，请将LBEE的主程序拖动到汉化程序上");
                Console.ReadKey();
                return;
            }

            Directory.CreateDirectory(TMPPath);
            Directory.CreateDirectory(TextMappingPath);
            Directory.CreateDirectory(ExtractedScriptPath);
            if (Directory.Exists(PendingReplacePath))
            {
                Directory.Delete(PendingReplacePath, true);
            }
            Directory.CreateDirectory(PendingReplacePath);

            string LBEEScriptPak = Path.Combine(LBEEGamePath, @"files\SCRIPT.PAK");
            string LBEEFontPak = Path.Combine(LBEEGamePath, @"files\FONT.PAK");
            string TemplateLBEEScriptPak = Path.Combine(LBEEGamePath, @"files\template\SCRIPT.PAK");
            string TemplateLBEEFontPak = Path.Combine(LBEEGamePath, @"files\template\FONT.PAK");
            if (!Directory.Exists(Path.Combine(LBEEGamePath, @"files\template")))
            {
                if (File.Exists(LBEEScriptPak) && File.Exists(LBEEFontPak))
                {
                    Directory.CreateDirectory(Path.Combine(LBEEGamePath, @"files\template"));
                    File.Copy(LBEEScriptPak, TemplateLBEEScriptPak);
                    File.Copy(LBEEFontPak, TemplateLBEEFontPak);
                }
                else
                {
                    Console.WriteLine("Need template files");
                    return;
                }
            }

            // Assuming LuckSystem is a separate executable that needs to be run
            Process.Start("LuckSystem\\lucksystem.exe", $"pak extract -i \"{TemplateLBEEScriptPak}\" -o {Path.Combine(TMPPath, "ScriptFileList.txt")} --all {ExtractedScriptPath}").WaitForExit();

            string[] Operators = File.ReadAllText("LuckSystem\\data\\LB_EN\\OPCODE.txt").ReplaceLineEndings().Split(Environment.NewLine);
            var scriptFiles = Directory.GetFiles(ExtractedScriptPath);
            foreach (var scriptFile in scriptFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(scriptFile);
                if (IgnoredScriptList.Contains(fileName))
                {
                    continue;
                }

                // 首先读出所有指令
                var allCommands = new List<LucaCommand>();
                var commandBytes = File.ReadAllBytes(scriptFile);
                int index = 0;
                while (index < commandBytes.Length)
                {
                    var curCommand = new LucaCommand();
                    index += curCommand.ReadCommand(commandBytes, index);
                    allCommands.Add(curCommand);
                }

                // 对所有指令执行AssignCommand，绑定到对应的指令上
                // 由于有跳转指令的存在，以及脚本翻译后各指令的指针会发生变化
                // 此处需要解析部分跳转指令，绑定到要跳转的目标指令上，然后在翻译结束后执行FixPtr修正跳转指针
                for (int i = 0;i < allCommands.Count; i++)
                {
                    allCommands[i].AssignCommand(allCommands, i);
                }

                // 没有对应翻译的话，新建一个翻译文件
                var scriptTextMapping = Path.Combine(TextMappingPath, $"{fileName}.json");
                if (!File.Exists(scriptTextMapping))
                {
                    var translationJson = new Dictionary<string, List<JObject>>();
                    foreach (var command in allCommands)
                    {
                        if (InstructionProcessor.InstructionGetMapping.ContainsKey(command.GetInstruction()))
                        {
                            var commandName = Operators[command.GetInstruction()];
                            if (!translationJson.ContainsKey(commandName))
                            {
                                translationJson[commandName] = new List<JObject>();
                            }
                            var TranslationObj = command.GetTranslationObj();
                            if (TranslationObj != null)
                            {
                                translationJson[commandName].Add(TranslationObj);
                            }
                        }
                    }
                    File.WriteAllText(scriptTextMapping, JsonConvert.SerializeObject(translationJson, Newtonsoft.Json.Formatting.Indented));
                    continue;
                }

                // 如果有翻译文件的话，执行翻译
                List<byte> NewScriptBuffer = new List<byte>();
                var ScriptTextMappingObj = JObject.Parse(File.ReadAllText(scriptTextMapping));
                var CmdIndexMap = new Dictionary<byte, int>();
                //Console.WriteLine(scriptFile);
                int CommandPtr = 0;
                foreach (var command in allCommands)
                {
                    var CurInstruction = command.GetInstruction();
                    /*if(Operators[CurInstruction]== "BATTLE" && command.GetCmdLength()>8)
                    {
                        Console.WriteLine($"{Operators[CurInstruction]} {command.CmdPtr}");
                    }*/
                    if (InstructionProcessor.InstructionSetMapping.ContainsKey(CurInstruction))
                    {
                        var commandName = Operators[CurInstruction];
                        if (ScriptTextMappingObj.ContainsKey(commandName))
                        {
                            int CurCmdIndex;
                            if (CmdIndexMap.ContainsKey(CurInstruction))
                            {
                                CmdIndexMap[CurInstruction]++;
                                CurCmdIndex = CmdIndexMap[CurInstruction];
                            }
                            else
                            {
                                CmdIndexMap.Add(CurInstruction, 0);
                                CurCmdIndex = 0;
                            }
                            var CommandTranslationCollection = ScriptTextMappingObj.GetValue(commandName)!.ToArray();
                            if (CommandTranslationCollection != null && CommandTranslationCollection.Length > CurCmdIndex)
                            {
                                var CommandTranslationObj = CommandTranslationCollection[CurCmdIndex].Value<JObject>();
                                if (CommandTranslationObj != null)
                                {
                                    if(!command.SetTranslationObj(CommandTranslationObj))
                                    {
                                        // 如果指令未被接受，则往前退一步
                                        CmdIndexMap[CurInstruction]--;
                                    }
                                }
                            }
                        }
                    }
                    // 为Command设置翻译后的指针
                    command.SetCmdPtr(CommandPtr);
                    CommandPtr += command.GetCmdLength()+ command.GetPendingLength();
                }
                foreach (var command in allCommands)
                {
                    // 修正跳转指令的指针
                    command.FixCommandPtr();
                    NewScriptBuffer.AddRange(command.Command!);
                    if (command.GetPendingLength() > 0)
                    {
                        NewScriptBuffer.Add(0);
                    }
                }
                File.WriteAllBytes(scriptFile, NewScriptBuffer.ToArray());
            }

            Process.Start("LuckSystem\\lucksystem.exe", $"pak replace -s \"{TemplateLBEEScriptPak}\" -i \"{ExtractedScriptPath}\" -o \"{LBEEScriptPak}\"").WaitForExit();

            // 解开字体
            Process.Start("LuckSystem\\lucksystem.exe", $"pak extract -i \"{TemplateLBEEFontPak}\" -o {Path.Combine(TMPPath, "FontFileList.txt")} --all {ExtractedFontPath}").WaitForExit();

            // 重绘字体
            var FontSize = new int[]
            {
                // 这些字体貌似有点问题,重绘后会导致游戏崩溃，先放着不动
                //12,14,72,36,16,
                18,20,24,28,30,32
                //28
            };

            var FontName = new string[]
            {
                "モダン","明朝","ゴシック","丸ゴシック"
            };

            var FontTemplate = "明朝";

            var Charset = File.ReadAllText(LBEECharset);
            HashSet<char> OriginalCharCollection =new (InstructionProcessor.CharCollection);
            InstructionProcessor.CharCollection.Remove('　');
            InstructionProcessor.CharCollection.Remove('\n');
            foreach (var oldChar in Charset.ToCharArray())
            {
                InstructionProcessor.CharCollection.Remove(oldChar);
            }
            var PendingOverrideChars = Charset[^(InstructionProcessor.CharCollection.Count)..];
            foreach(char PendingOverrideChar in PendingOverrideChars)
            {
                if(OriginalCharCollection.Contains(PendingOverrideChar))
                {
                    InstructionProcessor.CharCollection.Add(PendingOverrideChar);
                }
            }
            int FontReplaceIndex = Charset.Length - PendingOverrideChars.Length;
            string AllNewChar = new string(InstructionProcessor.CharCollection.ToArray().Order().ToArray());
            string AllNewCharFile = Path.Combine(TMPPath, "AllNewChar.txt");
            File.WriteAllText(AllNewCharFile, AllNewChar);
            foreach (var fSize in FontSize)
            {
                // 针对Template进行重绘，然后复制到各个字体
                // 如果每个字体都进行重绘，那么重绘后的游戏会崩溃，但只用一份的话就正常，很奇怪，不清楚原因
                // 看起来很像是字体过大了，这里指定一下ReplaceIndex，把一部分原有字体替换掉
                Process.Start("LuckSystem\\lucksystem.exe", $"font edit -s \"{ExtractedFontPath}\\{FontTemplate}{fSize}\" -i {FontReplaceIndex} -S \"{ExtractedFontPath}\\info{fSize}\" -f C:\\Windows\\Fonts\\simhei.ttf -c {AllNewCharFile} -o {Path.Combine(PendingReplacePath, $"{FontTemplate}{fSize}.png")} -O {Path.Combine(PendingReplacePath, $"info{fSize}")}").WaitForExit();
                Process.Start("czutil.exe", $"replace \"{ExtractedFontPath}\\{FontTemplate}{fSize}\" {Path.Combine(PendingReplacePath, $"{FontTemplate}{fSize}.png")} {Path.Combine(PendingReplacePath, $"{FontTemplate}{fSize}")}").WaitForExit();
                File.Delete(Path.Combine(PendingReplacePath, $"{FontTemplate}{fSize}.png"));
                foreach (var fName in FontName)
                {
                    if (fName != FontTemplate)
                    {
                        File.Copy(Path.Combine(PendingReplacePath, $"{FontTemplate}{fSize}"), Path.Combine(PendingReplacePath, $"{fName}{fSize}"));
                    }
                }
            }
            Process.Start("LuckSystem\\lucksystem.exe", $"pak replace -s \"{TemplateLBEEFontPak}\" -i \"{PendingReplacePath}\" -o \"{LBEEFontPak}\"").WaitForExit();
        }
    }

    public class LucaCommand
    {
        public byte[]? Command { get; set; }

        public int CmdPtr = 0;

        public LucaCommand[]? AssignedCommand = null;

        public int GetCmdLength()
        {
            return Command != null ? Command[0] + Command[1] * 256 : 0;
        }

        public int GetPendingLength()
        {
            if (Command == null)
            {
                return 0;
            }
            return Command.Length % 2;
        }

        public byte GetInstruction()
        {
            if (Command == null)
            {
                return 0;
            }
            return Command[2];
        }

        public int ReadCommand(byte[] scriptBytes, int index)
        {
            int commandLength = scriptBytes[index] + scriptBytes[index + 1] * 256;
            Command = scriptBytes.Skip(index).Take(commandLength).ToArray();
            CmdPtr = index;
            return commandLength + commandLength % 2;
        }

        public void SetCmdPtr(int Ptr)
        {
            this.CmdPtr = Ptr;
        }

        public JObject? GetTranslationObj()
        {
            if (Command == null)
            {
                return null;
            }
            if (InstructionProcessor.InstructionGetMapping.ContainsKey(GetInstruction()))
            {
                return InstructionProcessor.InstructionGetMapping[GetInstruction()](Command);
            }
            return null;
        }

        public bool SetTranslationObj(JObject inJsonObj)
        {
            if (Command == null)
            {
                return false;
            }
            if (InstructionProcessor.InstructionSetMapping.ContainsKey(GetInstruction()))
            {
                byte[]? NewCommand = InstructionProcessor.InstructionSetMapping[GetInstruction()](Command, inJsonObj);
                if (NewCommand == null)
                {
                    return false;
                }
                Command = NewCommand;
                Command[1] = (byte)(Command.Length / 256);
                Command[0] = (byte)(Command.Length % 256);
                return true;
            }
            return false;
        }

        public void AssignCommand(List<LucaCommand> InAllCommands, int CmdIndex)
        {
            if (InstructionProcessor.AssignCmdMapping.TryGetValue(GetInstruction(), out var AssignCmdFunc))
            {
                AssignedCommand = AssignCmdFunc(InAllCommands, CmdIndex);
            }
        }

        public void FixCommandPtr()
        {
            if (AssignedCommand != null)
            {
                if (InstructionProcessor.FixPtrMapping.TryGetValue(GetInstruction(), out var FixPtrFunc))
                {
                    FixPtrFunc(this, AssignedCommand);
                }
            }
        }
    }
}
