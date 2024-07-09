using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
///using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Form = System.Windows.Forms.Form;
using View = System.Windows.Forms.View;

namespace KPMEngineeringB.R
{
    public partial class ChooseCategory : System.Windows.Forms.Form
    {
        Autodesk.Revit.DB.Document Doc;
        
        private ExternalCommandData storedCommandData;
        public static List<string> getLists = new List<string>();

        public ChooseCategory(Document doc,ExternalCommandData commandData)
        {
            InitializeComponent();
            Doc = doc;
            storedCommandData = commandData;
            getLists.Clear();          
        }

        private void ChooseCategory_Load(object sender, EventArgs e)
        {
            listView1.Scrollable = true;
            listView1.View = View.Details;

            listView1.HeaderStyle = ColumnHeaderStyle.None;
            ColumnHeader header = new ColumnHeader();
          
            
            listView1.Columns.Add(header);
           
            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
         

            var all_categories = Doc.Settings.Categories;
         
            foreach (var category in all_categories)
            {
                if (((Autodesk.Revit.DB.Category)category).CategoryType == CategoryType.Model)
                {
                    listView1.Items.Add(((Autodesk.Revit.DB.Category)category).Name);
                }
            }
        }
        private void listView1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (ModifierKeys == Keys.Control || ModifierKeys == Keys.Shift)
            {
                e.NewValue = e.CurrentValue;
            }
        }

        // Select All
        private void button1_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                listView1.Items[i].Checked = true;
            }  
        }

        // create
        private void button3_Click(object sender, EventArgs e)
        {
            var getItem="";
            foreach (ListViewItem item in listView1.Items)
            {
                if (item.Checked)
                {   
                    try
                    {
                        getItem = item.Text;
                        getLists.Add(getItem);
                    }
                    catch
                    {
                        //Khush
                    }
                }
            }
            Close();
        }
        // select None
        private void button2_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                listView1.Items[i].Checked = false;

            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel1.LinkVisited = true;
            System.Diagnostics.Process.Start("https://kpm-engineering.com/");
        }
    }
}
