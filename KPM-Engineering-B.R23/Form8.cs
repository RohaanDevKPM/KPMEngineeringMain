using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Form = System.Windows.Forms.Form;

namespace KPMEngineeringB.R
{
    public partial class Form8 : System.Windows.Forms.Form
    {
        Autodesk.Revit.DB.Document Doc;
        private List<string> vsList = new List<string>();
    
        private List<ViewSheet> viewSheets = new List<ViewSheet>();
        private List<ViewSheet> SelectedViewSheets = new List<ViewSheet>();
    
        public Form8(Document doc)
        {
            InitializeComponent();
            Doc = doc;
        }
       
        private void CopyLegenedFromOnesheet_to_MultipleSheet_Load(object sender, EventArgs e)
        {
            var viewS = new FilteredElementCollector(Doc).OfCategory(BuiltInCategory.OST_Sheets).WhereElementIsNotElementType().OfType<ViewSheet>().ToList();
            foreach (var element in viewS)
            {
                var pV = element.GetAllPlacedViews();
                foreach (var v in pV)
                {
                    var ele = Doc.GetElement(v);
                    if ((ele as Autodesk.Revit.DB.View).ViewType == ViewType.Legend)
                    {
                        string sheetName = element.Name;
                        Autodesk.Revit.DB.Parameter sNum = element.get_Parameter(BuiltInParameter.SHEET_NUMBER);

                        vsList.Add(sNum.AsString() + " - " + sheetName); // combinedInfo
                        viewSheets.Add(element); // add element in list  to filter viewsheet
                        break; 
                    }
                }
            }
            comboBox1.DataSource = vsList;
        }

       
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedIndex = comboBox1.SelectedIndex;
            var selectedElement = viewSheets[selectedIndex];
            var legends = selectedElement.GetAllPlacedViews();
            var filteredLegends = new List<string>(); // Create a new list for filtered legends
            foreach (var legend in legends)
            {
                var ele = (Doc.GetElement(legend));
                if ((ele as Autodesk.Revit.DB.View).ViewType == ViewType.Legend)
                {
                    filteredLegends.Add(ele.Name);
                }
            }

            comboBox2.DataSource = filteredLegends;
            comboBox2.SelectedIndex = 0;

        }
       
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedIndex = comboBox1.SelectedIndex;
            var selectedElement = viewSheets[selectedIndex];
            var selectedLeg = comboBox2.SelectedValue.ToString();
            var vS = new FilteredElementCollector(Doc).OfCategory(BuiltInCategory.OST_Sheets).WhereElementIsNotElementType().OfType<ViewSheet>().ToList();
            var fL = new List<string>();

            foreach (var el in vS)
            {
                if (el.GetAllPlacedViews().Count != 0 && el.Id != selectedElement.Id)
                {
                    var pV = el.GetAllPlacedViews();
                    bool pass = false;
                    foreach (var v in pV)
                    {
                        var ele = Doc.GetElement(v);
                        if ((ele as Autodesk.Revit.DB.View).ViewType == ViewType.Legend && ele.Name == selectedLeg)
                        {
                            pass = false;
                            break;
                        }
                        else
                        {
                            pass = true;
                        }
                    }
                    if (pass)
                    {
                        string sheetName = el.Name;
                        Autodesk.Revit.DB.Parameter sNum = el.get_Parameter(BuiltInParameter.SHEET_NUMBER);
                        
                        fL.Add(sNum.AsString() + " - " + sheetName);
                        SelectedViewSheets.Add(el);
                        
                    }
                }
                else if (el.GetAllPlacedViews().Count == 0 && el.Id != selectedElement.Id)
                {
                    string sheetName = el.Name;
                    Autodesk.Revit.DB.Parameter sNum = el.get_Parameter(BuiltInParameter.SHEET_NUMBER);
                   
                    fL.Add(sNum.AsString() + " - " + sheetName);
                    SelectedViewSheets.Add(el);
                    
                }
            }
           
            listView1.View = System.Windows.Forms.View.Details;

            listView1.HeaderStyle = ColumnHeaderStyle.None;
            ColumnHeader header = new ColumnHeader();


            listView1.Columns.Add(header);
            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

            // Need to Clear so no duplicate 
            listView1.Items.Clear();

            foreach (var l in fL)
            {
              listView1.Items.Add(l);   
            } 
        }

        // Select None
        private void button1_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                listView1.Items[i].Checked = false;
            }
        }
        // Select All 
        private void button2_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                listView1.Items[i].Checked = true;
            }
        }
        private void listView1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (ModifierKeys == Keys.Control || ModifierKeys == Keys.Shift)
            {
                e.NewValue = e.CurrentValue;
            }
           
        }
        // Finish 
        private void button3_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            Close();
        }
        // Add
        private void button4_Click(object sender, EventArgs e)
        {
            var selectedName = comboBox2.SelectedValue.ToString();
           
            var legCollect = new FilteredElementCollector(Doc).OfCategory(BuiltInCategory.OST_Views).WhereElementIsNotElementType().OfType<Autodesk.Revit.DB.View>().Where(x => x.ViewType == ViewType.Legend).Where(z => z.Name == selectedName).FirstOrDefault();
            
            var selectedIndex = comboBox1.SelectedIndex;
            var selectedElement = viewSheets[selectedIndex];
            var getViewPorts = selectedElement.GetAllViewports();
            XYZ refPoint = new XYZ();
            Viewport selectedVP;

            foreach (var viewPort in getViewPorts)
            {
                var eleViewPort = (Doc.GetElement(viewPort)) as Autodesk.Revit.DB.Viewport;
                if ((Doc.GetElement(eleViewPort.ViewId)).Name == selectedName)
                {
                    selectedVP = eleViewPort;
                    refPoint = selectedVP.GetBoxCenter();
                    break;
                }
            }
            using (Transaction transaction = new Transaction(Doc, "Place Legends"))
            {
                transaction.Start();
           
               foreach (int item in listView1.CheckedIndices)
                {
                    var collectSheet = listView1.Items[item].Text;
                  
                    var selectedSheeT = new FilteredElementCollector(Doc).OfCategory(BuiltInCategory.OST_Sheets).WhereElementIsNotElementType().OfType<ViewSheet>().Where(z => (z.SheetNumber + " - " +z.Name ) == collectSheet).FirstOrDefault();  // collectSheet
                    if (selectedSheeT != null)
                    {
                        if (item >= 0 && item < SelectedViewSheets.Count)
                        {
                            var createVP = Viewport.Create(Doc, selectedSheeT.Id, legCollect.Id, refPoint);  
                        }
                    }
                }
        
                var count = listView1.CheckedIndices.Count;
                if (count != 0)
                    TaskDialog.Show("Results", "Number of Sheet Updated : " + count.ToString());
                else
                    TaskDialog.Show("Results", "No Sheet Updated.");

                transaction.Commit();
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel1.LinkVisited = true;
            System.Diagnostics.Process.Start("https://kpm-engineering.com/");
        }
    }
}
