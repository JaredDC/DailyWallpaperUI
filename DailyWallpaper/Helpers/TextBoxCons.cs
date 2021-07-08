﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DailyWallpaper
{
    class TextBoxCons : TextWriter
    {
        private IEnumerable<TextWriter> writers;
        public TextBoxCons(IEnumerable<TextWriter> writers)
        {
            this.writers = writers.ToList();
        }
        public TextBoxCons(params TextWriter[] writers)
        {
            this.writers = writers;
        }

        public override void Write(char value)
        {
            foreach (var writer in writers)
                writer.Write(value);
        }

        public override void Write(string value)
        {
            foreach (var writer in writers)
                writer.Write(value);
        }

        public override void Flush()
        {
            foreach (var writer in writers)
                writer.Flush();
        }

        public override void Close()
        {
            foreach (var writer in writers)
                writer.Close();
        }

        public override Encoding Encoding
        {
            get { return Encoding.Default; }
        }
    }
    public class ConsWriter : TextWriter
    {
        private TextBox textbox;
        public ConsWriter(TextBox textbox)
        {
            this.textbox = textbox;
        }

        public /*override*/ void WriteC(char value)
        {
            string s = "";
            s += value;
            textbox.AppendText(s);
        }

        delegate void SetWriteCBack(char value);
        public override void Write(char value)
        {
            if (textbox.InvokeRequired)
            {
                var stcb = new SetWriteCBack(Write);
                textbox.Invoke(stcb, new object[] { value });
            }
            else
            {
                string s = "";
                s += value;
                textbox.AppendText(s);
            }
        }

        public /*override*/ void WriteT(string value)
        {
            textbox.AppendText(value);
        }

        public override void Write(string text)
        {
            if (textbox.InvokeRequired)
            {
                SetWriteLineBack stcb = new SetWriteLineBack(Write);
                textbox.Invoke(stcb, new object[] { text });
            }
            else
            {
                textbox.AppendText(text);
            }
        }


        public /*override*/ void WriteLineOLD(string value)
        {
            // two long cause problem, maybe.
/*            if (textbox.Text.Length > 10000)
            {
                var tmp = textbox.Text;
                textbox.Text = tmp.Substring(8000, tmp.Length);
            }*/
            textbox.AppendText(value + NewLine);

            textbox.Update();
        }

        delegate void SetWriteLineBack(string text);
        public override void WriteLine(string text)
        {
            if (textbox.InvokeRequired)
            {
                SetWriteLineBack stcb = new SetWriteLineBack(WriteLine);
                textbox.Invoke(stcb, new object[] { text });
            }
            else
            {
                textbox.AppendText(text + NewLine);
                textbox.Update();
            }
        }
        public override Encoding Encoding
        {
            get { return Encoding.Default; }
        }
    }
}
