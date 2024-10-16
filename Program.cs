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

    class Program
    {
        static string LBEEGamePath = @"E:\SteamLibrary\steamapps\common\Little Busters! English Edition";
        static string LBEECharset = @".\Charset.txt";
        static string TMPPath = Path.GetFullPath(@".\.tmp");
        static string TextMappingPath = Path.GetFullPath(@".\TextMapping");
        static string ExtractedScriptPath = Path.Combine(TMPPath, "Scripts");
        static string ExtractedFontPath = Path.Combine(TMPPath, "Fonts");
        static string PendingReplacePath = Path.Combine(TMPPath, "PendingReplace");
        static List<string> IgnoredScriptList = new List<string> { "SEEN8500", "SEEN8501" };

        static void Main(string[] args)
        {
            Directory.CreateDirectory(TMPPath);
            Directory.CreateDirectory(TextMappingPath);
            Directory.CreateDirectory(ExtractedScriptPath);
            Directory.CreateDirectory(PendingReplacePath);

            string LBEEScriptPak = Path.Combine(LBEEGamePath, @"files\SCRIPT.PAK");
            string LBEEFontPak = Path.Combine(LBEEGamePath, @"files\FONT.PAK");
            string TemplateLBEEScriptPak = Path.Combine(LBEEGamePath, @"files\template\SCRIPT.PAK");
            string TemplateLBEEFontPak = Path.Combine(LBEEGamePath, @"files\template\FONT.PAK");
            if (!Directory.Exists(Path.Combine(LBEEGamePath, @"files\template")))
            {
                Console.WriteLine("Need template files");
                return;
            }

            // Assuming LuckSystem is a separate executable that needs to be run
            Process.Start("LuckSystem\\lucksystem.exe", $"pak extract -i \"{TemplateLBEEScriptPak}\" -o {Path.Combine(TMPPath, "ScriptFileList.txt")} --all {ExtractedScriptPath}").WaitForExit();

            var scriptFiles = Directory.GetFiles(ExtractedScriptPath);
            foreach (var scriptFile in scriptFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(scriptFile);
                if (IgnoredScriptList.Contains(fileName) || fileName.StartsWith("_"))
                {
                    continue;
                }

                var allCommands = new List<LucaCommand>();
                var commandBytes = File.ReadAllBytes(scriptFile);
                int index = 0;
                while (index < commandBytes.Length)
                {
                    var curCommand = new LucaCommand();
                    index += curCommand.ReadCommand(commandBytes, index);
                    allCommands.Add(curCommand);
                }

                var scriptTextMapping = Path.Combine(TextMappingPath, $"{fileName}.json");
                if (!File.Exists(scriptTextMapping))
                {
                    var translationJson = new Dictionary<string, List<JObject>>();
                    foreach (var command in allCommands)
                    {
                        var commandName = InstructionProcessor.InstructionNameMapping.GetValueOrDefault(command.GetInstruction());
                        if (commandName != null)
                        {
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
                List<byte> NewScriptBuffer = new List<byte>();
                var ScriptTextMappingObj = JObject.Parse(File.ReadAllText(scriptTextMapping));
                var CmdIndexMap = new Dictionary<byte, int>();
                foreach (var command in allCommands)
                {
                    var CurInstruction = command.GetInstruction();
                    var commandName = InstructionProcessor.InstructionNameMapping.GetValueOrDefault(CurInstruction);
                    if (commandName != null)
                    {
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
                                    command.SetTranslationObj(CommandTranslationObj);
                                }
                            }
                        }
                    }
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
                //12,14,72,36,16,18,
                //20,24,28,30,32
                //28
            };

            var FontName = new string[]
            {
                "モダン","明朝","ゴシック","丸ゴシック"
            };

            var OriginalCharset = File.ReadAllText(LBEECharset);
            OriginalCharset+= "　";
            foreach (var oldChar in OriginalCharset.ToCharArray())
            {
                InstructionProcessor.CharCollection.Remove(oldChar);
            }
            string AllNewChar = new string(InstructionProcessor.CharCollection.ToArray());
            string AllNewCharFile = Path.Combine(TMPPath, "AllNewChar.txt");
            File.WriteAllText(AllNewCharFile, AllNewChar);

            foreach (var fSize in FontSize)
            {
                foreach (var fName in FontName)
                {
                    // 针对每一个字体都进行重绘
                    Process.Start("LuckSystem\\lucksystem.exe", $"font edit -s \"{ExtractedFontPath}\\{fName}{fSize}\" -S \"{ExtractedFontPath}\\info{fSize}\" -f C:\\Windows\\Fonts\\simhei.ttf -c {AllNewCharFile} -a -o {Path.Combine(PendingReplacePath, $"{fName}{fSize}")} -O {Path.Combine(PendingReplacePath, $"info{fSize}")}").WaitForExit();
                }    
            }
            Process.Start("LuckSystem\\lucksystem.exe", $"pak replace -s \"{TemplateLBEEFontPak}\" -i \"{PendingReplacePath}\" -o \"{LBEEFontPak}\"").WaitForExit();
            Directory.Delete(PendingReplacePath, true);
            Directory.CreateDirectory(PendingReplacePath);
        }
    }

    public class LucaCommand
    {
        public byte[]? Command { get; set; }

        public int GetPendingLength()
        {
            if(Command == null)
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
            return commandLength + commandLength % 2;
        }

        public JObject? GetTranslationObj()
        {
            if(Command == null)
            {
                return null;
            }
            if (InstructionProcessor.InstructionGetMapping.ContainsKey(GetInstruction()))
            {
                return InstructionProcessor.InstructionGetMapping[GetInstruction()](Command);
            }
            return null;
        }

        public void SetTranslationObj(JObject inJsonObj)
        {
            if (Command == null)
            {
                return; 
            }
            if (InstructionProcessor.InstructionSetMapping.ContainsKey(GetInstruction()))
            {
                Command = InstructionProcessor.InstructionSetMapping[GetInstruction()](Command, inJsonObj);
                Command[1] = (byte)(Command.Length / 256);
                Command[0] = (byte)(Command.Length % 256);
            }
        }
    }

}
