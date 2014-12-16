using FastColoredTextBoxNS;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace luaformatter
{
    public class luatextbox : FastColoredTextBoxNS.FastColoredTextBox
    {
        static SolidBrush b = new SolidBrush(Color.LightBlue);
        static SolidBrush bb = new SolidBrush(Color.LightGreen);

        static Style GreenStyle = new TextStyle(Brushes.Green, null, FontStyle.Italic);
        static Regex LuaStringRegex = new Regex("\"\"|''|\".*?[^\\\\]\"|'.*?[^\\\\]'", SyntaxHighlighter.RegexCompiledOption);
        static Regex LuaCommentRegex1 = new Regex("--.*$", RegexOptions.Multiline | SyntaxHighlighter.RegexCompiledOption);
        static Regex LuaCommentRegex2 = new Regex("(--\\[\\[.*?\\]\\])|(--\\[\\[.*)", RegexOptions.Singleline | SyntaxHighlighter.RegexCompiledOption);
        static Regex LuaCommentRegex3 = new Regex("(--\\[\\[.*?\\]\\])|(.*\\]\\])", RegexOptions.Singleline | RegexOptions.RightToLeft | SyntaxHighlighter.RegexCompiledOption);
        static Regex LuaNumberRegex = new Regex("\\b\\d+[\\.]?\\d*([eE]\\-?\\d+)?[lLdDfF]?\\b|\\b0x[a-fA-F\\d]+\\b", SyntaxHighlighter.RegexCompiledOption);
        static Regex LuaKeywordRegex = new Regex("\\b(and|break|do|else|elseif|end|false|for|function|if|in|local|nil|not|or|repeat|return|then|true|until|while)\\b", SyntaxHighlighter.RegexCompiledOption);
        static Regex LuaFunctionsRegex = new Regex("\\b(assert|collectgarbage|dofile|error|getfenv|getmetatable|ipairs|load|loadfile|loadstring|module|next|pairs|pcall|print|rawequal|rawget|rawset|require|select|setfenv|setmetatable|tonumber|tostring|type|unpack|xpcall)\\b", SyntaxHighlighter.RegexCompiledOption);

        private Regex high;

        public Regex hightlight { get { return high; } set { high = value; find(); } }

        public Dictionary<int, int> indexs = new Dictionary<int, int>();

        public luatextbox()
        {
            SyntaxHighlighter.FunctionsStyle = GreenStyle;

            this.Dock = System.Windows.Forms.DockStyle.Fill;
        }

        public override void OnTextChangedDelayed(Range changedRange)
        {
            base.OnTextChangedDelayed(changedRange);
            changedRange.SetStyle(GreenStyle, LuaKeywordRegex);
        }

        protected override void OnPaintLine(PaintLineEventArgs e)
        {
            base.OnPaintLine(e);
            if (indexs.ContainsKey(e.LineIndex))
                e.Graphics.FillRectangle(b, e.LineRect);
        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            base.OnPaint(e);

            Rectangle rect = new Rectangle(Width - System.Windows.Forms.SystemInformation.VerticalScrollBarWidth*2, 0, 20, 3);

            float vliencount = (float)this.LinesCount;

            foreach (var c in indexs)
            {
                float v1 = 100 * (float)c.Key / vliencount;
                rect.Y = (int)((float)(this.Height * (v1/100f)));
                e.Graphics.FillRectangle(bb, rect);
            }
        }

        void find()
        {
           indexs.Clear();

           if (high == null)
               return;

            for(int i =0;  i < Lines.Count; i++)
            {
                if (high.Match(Lines[i]).Success)
                    indexs.Add(i, 0);
            }
        }
    }
}
