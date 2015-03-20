using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace Adam_Schema_to_wiki_converter
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }

        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string filename = "";
                filename = openFileDialog1.FileName;
                ParseFile(filename);
            }
        }

        private void ParseFile(string filename)
        {
            if (File.Exists(filename))
            {
                List<AttributeOrClass> contents = new List<AttributeOrClass>();
                AttributeOrClass current = new AttributeOrClass();
                string currentLine = "";
                StreamReader adamFile = new StreamReader(filename);
                while ((currentLine = adamFile.ReadLine()) != null)
                {
                    if (currentLine == "" || currentLine == "-")
                    {
                        continue;
                    }
                    if (currentLine.StartsWith("# Attribute:"))
                    {
                        if (current.type != AttributeOrClass.objtype.notset)
                        {contents.Add(current);}
                        current = new AttributeOrClass();
                        current.type = AttributeOrClass.objtype.Attribute;
                        Debug.WriteLine("Found " + currentLine);
                        continue;
                    }
                    else if (currentLine.StartsWith("# Class:"))
                    {
                        if (current.type != AttributeOrClass.objtype.notset)
                        {contents.Add(current);}
                        current = new AttributeOrClass();
                        current.type = AttributeOrClass.objtype.Class;
                        Debug.WriteLine("Found " + currentLine);
                        continue;
                    }
                    if (currentLine.StartsWith("# objectclasses: ") || currentLine.StartsWith("# attributetypes: "))
                    {
                        current.Description = System.Text.RegularExpressions.Regex.Match(currentLine, @"DESC '(.*?)' ").Value.Replace("' ",string.Empty);
                        //current.Description = currentLine.Substring(currentLine.IndexOf("DESC '"),currentLine.IndexOf("'",currentLine.IndexOf("DESC '")));
                    }
                    if (currentLine.StartsWith("dn: "))
                    {
                        current.DN = currentLine.Substring(4);
                        continue;
                    }
                    if (currentLine.StartsWith("cn: "))
                    {
                        current.CN = currentLine.Substring(4);
                        continue;
                    }
                    if (currentLine.StartsWith("objectClass: "))
                    {
                        current.ObjectClass.Add(currentLine.Substring(13));
                        continue;
                    }
                    if (currentLine.StartsWith("showInAdvancedViewOnly: "))
                    {
                        current.AdvancedOnly = Convert.ToBoolean(currentLine.Substring(24));
                        continue;
                    }
                    if (currentLine.StartsWith("adminDisplayName: "))
                    {
                        current.AdminName = currentLine.Substring(18);
                        continue;
                    }
                    if (currentLine.StartsWith("lDAPDisplayName: "))
                    {
                        current.DisplayName = currentLine.Substring(17);
                        continue;
                    }
                    if (currentLine.StartsWith("mayContain: "))
                    {
                        if (!current.MayContain.Contains("[[#" + currentLine.Substring(12) + "|" + currentLine.Substring(12) + "]]"))
                        {
                            current.MayContain.Add("[[#" + currentLine.Substring(12) + "|" + currentLine.Substring(12) + "]]");
                        }
                        continue;
                    }
                    if (currentLine.StartsWith("mustContain: "))
                    {
                        if (!current.MustContain.Contains("[[#" + currentLine.Substring(13) + "|" + currentLine.Substring(13) + "]]"))
                        {
                            current.MustContain.Add("[[#" + currentLine.Substring(13) + "|" + currentLine.Substring(13) + "]]");
                        }
                        continue;
                    }
                    if (currentLine.StartsWith("possSuperiors: "))
                    {
                        if (!current.PossibleSuperiors.Contains(currentLine.Substring(15)))
                        {
                            current.PossibleSuperiors.Add(currentLine.Substring(15));
                        }
                        continue;
                    }
                    
                }
                PrintCode(contents);
            }

        }

        private void PrintCode(List<AttributeOrClass> contents)
        {
            string AttrHeader = "=Attributes=" + Environment.NewLine;
            string ClassHeader = "=Classes=" + Environment.NewLine;
            string AttrData = "";
            string ClassData = "";
            foreach(AttributeOrClass current in contents)
            {
                if (current.type == AttributeOrClass.objtype.Attribute)
                {
                    //AttrHeader += string.Format("[[{0}]]<br>{1}", current.DisplayName,Environment.NewLine);
                    AttrData += string.Format("=={0}=={7}CN: {1}<br>DN: {2}<br>Description: {3}<br><br>{7}", current.DisplayName, current.CN, current.DN, current.Description.Replace("DESC '", string.Empty), String.Join(", ", current.PossibleSuperiors.ToArray()), string.Join(", ", current.MustContain.ToArray()), string.Join(", ", current.MayContain.ToArray()), Environment.NewLine);
                }
                if (current.type == AttributeOrClass.objtype.Class)
                {
                    //ClassHeader += string.Format("[[{0}]]{1}", current.DisplayName,Environment.NewLine);
                    ClassData += string.Format("=={0}=={7}CN: {1}<br>DN: {2}<br>Description: {3}<br>Possible Superiors: {4}<br>Must Contain: {5}<br>May Contain: {6}<br><br>{7}",current.DisplayName,current.CN,current.DN,current.Description.Replace("DESC '",string.Empty),String.Join(", ", current.PossibleSuperiors.ToArray()),string.Join(", ", current.MustContain.ToArray()),string.Join(", ", current.MayContain.ToArray()),Environment.NewLine);
                }
            }

            textBox1.Text = AttrHeader + Environment.NewLine + AttrData + Environment.NewLine + ClassHeader + Environment.NewLine + ClassData;
            textBox1.SelectAll();
        }
    }

    class AttributeOrClass
    {
        public objtype type = objtype.notset;
        public string Description = "";
        public string DN = "";
        public List<string> ObjectClass = new List<string>();
        public string CN = "";
        public bool AdvancedOnly = false;
        public string AdminName = "";
        public string DisplayName = "";
        public List<string> PossibleSuperiors = new List<string>();
        public List<string> MayContain = new List<string>();
        public List<string> MustContain = new List<string>();

        public enum objtype { Attribute, Class, notset };
    }
}
