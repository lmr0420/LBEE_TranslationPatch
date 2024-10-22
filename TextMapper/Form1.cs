using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TextMapper
{
    public partial class Form1 : Form
    {
        JObject EditingTranslationObj { get; set; }

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern short GetKeyState(int nVirtKey);
        public const int VK_LCONTROL = 0xA2;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var OFD = new OpenFileDialog();
            OFD.Filter = "Json Files|*.json";
            OFD.Title = "Select the Json file to load";
            OFD.Multiselect = false;
            if (OFD.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            this.Text = Path.GetFileName(OFD.FileName);
            var JsonStr = File.ReadAllText(OFD.FileName);
            this.EditingTranslationObj = JObject.Parse(JsonStr);
            this.TextList.Items.Clear();
            RefreshTextList();
        }

        void RefreshTextList()
        {
            var Messages = EditingTranslationObj.GetValue("MESSAGE")?.ToList();
            if (Messages != null)
            {
                while(Messages.Count > this.TextList.Items.Count)
                {
                    this.TextList.Items.Add("");
                }
                for (int i = 0; i < Messages.Count; i++)
                {
                    this.TextList.Items[i] = Messages[i]["EN"]+"/"+ Messages[i]["Translation"];
                }
            }
        }

        private void TextList_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (TextList.SelectedIndex < 0)
                {
                    return;
                }
                var Messages = EditingTranslationObj.GetValue("MESSAGE")?.ToList();
                if (Messages == null)
                {
                    return;
                }
                var TrasnlateTextObj = Messages[TextList.SelectedIndex].Value<JObject>();
                if (TrasnlateTextObj != null)
                {
                    textJP.Text = TrasnlateTextObj["JP"]?.ToString();
                    textEN.Text = TrasnlateTextObj["EN"]?.ToString();
                    textTranslation.Text = TrasnlateTextObj["Translation"]?.ToString();
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                this.contextMenuStrip1.Show(this.TextList, e.Location);
            }
        }

        private void InsertClipboard_Click(object sender, EventArgs e)
        {
            if (TextList.SelectedIndex < 0)
            {
                return;
            }
            string TextPendingInsert = Clipboard.GetText().Replace("\r", "");
            string[] TextLines = TextPendingInsert.Split('\n');
            var Messages = EditingTranslationObj.GetValue("MESSAGE")?.ToList();
            if(Messages == null)
            {
                return;
            }
            for (int i=0;i<TextLines.Length;i++)
            {
                if(i+TextList.SelectedIndex>= Messages.Count)
                {
                    break;
                }
                var TrasnlateTextObj = Messages[i + TextList.SelectedIndex].Value<JObject>();
                if(TrasnlateTextObj != null)
                {
                    string PendingReplace = TextLines[i];
                    if ((GetKeyState(VK_LCONTROL) & 0x8000) == 0)
                    {
                        if (TextLines[i].Contains("「") && TextLines[i].IndexOf('「') != 0 && !TextLines[i].StartsWith("`"))
                        {
                            int DialogueStart = TextLines[i].IndexOf('「');
                            if (DialogueStart < 10)
                            {
                                PendingReplace = "`" + PendingReplace.Insert(DialogueStart, "@");
                            }
                        }
                        else if (TextLines[i].Contains("『") && TextLines[i].IndexOf('『') != 0 && !TextLines[i].StartsWith("`"))
                        {
                            int DialogueStart = TextLines[i].IndexOf('『');
                            if (DialogueStart < 10)
                            {
                                PendingReplace = "`" + PendingReplace.Insert(DialogueStart, "@");
                            }
                        }
                    }
                    TrasnlateTextObj["Translation"] = PendingReplace;
                }
            }
            RefreshTextList();
        }

        private void InsertText_Click(object sender, EventArgs e)
        {
            if (TextList.SelectedIndex < 0)
            {
                return;
            }
            var Messages = EditingTranslationObj.GetValue("MESSAGE")?.ToList();
            if (Messages == null)
            {
                return;
            }
            for (int i = Messages.Count - 1; i > TextList.SelectedIndex; i--)
            {
                var TrasnlateTextBObj = Messages[i].Value<JObject>();
                var TrasnlateTextAObj = Messages[i-1].Value<JObject>();
                if (TrasnlateTextBObj != null && TrasnlateTextAObj != null)
                {
                    TrasnlateTextBObj["Translation"] = TrasnlateTextAObj["Translation"];
                }
            }
            RefreshTextList();
        }

        private void RemoveText_Click(object sender, EventArgs e)
        {
            if (TextList.SelectedIndex < 0)
            {
                return;
            }
            var Messages = EditingTranslationObj.GetValue("MESSAGE")?.ToList();
            if (Messages == null)
            {
                return;
            }
            for (int i = TextList.SelectedIndex;i<Messages.Count-1; i++)
            {
                var TrasnlateTextAObj = Messages[i].Value<JObject>();
                var TrasnlateTextBObj = Messages[i + 1].Value<JObject>();
                if (TrasnlateTextBObj != null && TrasnlateTextAObj != null)
                {
                    TrasnlateTextAObj["Translation"] = TrasnlateTextBObj["Translation"];
                }
            }
            RefreshTextList();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var OFD = new SaveFileDialog();
            OFD.Filter = "Json Files|*.json";
            OFD.Title = "Select the Json file to load";
            if (OFD.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            File.WriteAllText(OFD.FileName, EditingTranslationObj.ToString(Newtonsoft.Json.Formatting.Indented));
        }
    }
}
