using Document = Autodesk.Revit.DB.Document;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using View = Autodesk.Revit.DB.View;
using MySqlX.XDevAPI.Relational;
using Autodesk.Revit.UI;
using System.Xml.Linq;
using Microsoft.Office.Interop.Excel;
using DataTable = System.Data.DataTable;
using Parameter = Autodesk.Revit.DB.Parameter;
using Autodesk.Revit.DB.Mechanical;

namespace KPMEngineeringB.R
{
    // Task:- Change Revision of Revision Cloud Plugin
    public partial class ChangeRevisionCloud : System.Windows.Forms.Form
    {
        Autodesk.Revit.DB.Document Doc;
        public string Sr_No { get; set; }
        public string Views { get; set; }


        private DataTable dataRvt = new DataTable();

        private List<List<Element>> ViewL = new List<List<Element>>();
        private List<View> finalViews = new List<View>();
        private List<string> RevNl = new List<string>();
        public ChangeRevisionCloud(Document doc) 
        {
            InitializeComponent();
            Doc = doc;
        }

        private void ChangeRevisionCloud_Load(object sender, EventArgs e)
        {
            dataRvt.Columns.Add("Sr No", typeof(int));
            dataRvt.Columns["Sr No"].ReadOnly = true;
            dataRvt.Columns.Add("Views");
            dataRvt.Columns["Views"].ReadOnly = true;

            dataGridView1.RowHeadersVisible = false;


            var Rev = new FilteredElementCollector(Doc).OfCategory(BuiltInCategory.OST_Revisions).WhereElementIsNotElementType().ToElements();
            
            foreach (var r in Rev)
            {
                string RevName = r.Name;
                RevNl.Add(RevName);
            }

            int rowNumber = 1;
           
            var maxLength = 0;
            var VCol = new FilteredElementCollector(Doc).OfCategory(BuiltInCategory.OST_Views).WhereElementIsNotElementType().OfType<View>().ToList();// .ToList()
            var SheetCol = new FilteredElementCollector(Doc).OfCategory(BuiltInCategory.OST_Sheets).WhereElementIsNotElementType().OfType<View>().ToList();

            var ViewCol = VCol.Concat(SheetCol).ToList();
            foreach (var vi in ViewCol)
            {
                if (vi.ViewType == ViewType.FloorPlan ||
                vi.ViewType == ViewType.CeilingPlan ||
                vi.ViewType == ViewType.Elevation ||
                vi.ViewType == ViewType.DrawingSheet ||
                vi.ViewType == ViewType.DraftingView ||
                vi.ViewType == ViewType.EngineeringPlan ||
                vi.ViewType == ViewType.Section )
                {
                    finalViews.Add(vi);
                }
            }
        
            foreach (View V in finalViews)
            {
                string viewName = V.Name;
                dataRvt.Rows.Add(rowNumber, V.ViewType.ToString() + " - " + viewName);
                rowNumber++;
            }

            dataGridView1.DataSource = dataRvt;

            dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            for (int i = 0; i < finalViews.Count; i++)
            {
                var eleCategory = new ElementCategoryFilter(BuiltInCategory.OST_RevisionClouds);
                var collectRevCloud = finalViews[i].GetDependentElements(eleCategory);

                // Create a new list to store elements
                var elements = new List<Element>();

                // Convert ElementIds to Elements
                foreach (var elementId in collectRevCloud)
                {
                    var element = Doc.GetElement(elementId);
                    if (element != null)
                    {
                        elements.Add(element);
                    }
                }
                // Add the list of elements to ViewL
                ViewL.Add(elements);

                // Find the maximum length among the inner lists in ViewL
                foreach (var innerList in ViewL)
                {
                    // subList is elements
                    if (elements.Count > maxLength)
                    {
                        maxLength = innerList.Count;
                    }
                }
            }
            for (int i = 1; i <= maxLength; i++)
            {
                DataGridViewComboBoxColumn comboBoxC = new DataGridViewComboBoxColumn();
                comboBoxC.HeaderText = "Revision Cloud - " + (i); 
                dataGridView1.Columns.Add(comboBoxC);

           /*     dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView1.Seleceted*/
               // dataGridView1.MultiSelect = false;
            }
           
            for (int i = 0; i < finalViews.Count; i++)
            {
                var eleCategory = new ElementCategoryFilter(BuiltInCategory.OST_RevisionClouds);
                var collectRevCloud = finalViews[i].GetDependentElements(eleCategory);

                for (int j = 0; j < collectRevCloud.Count; j++)
                {
                    var el = Doc.GetElement(collectRevCloud[j]);

                    string selectRev = el.get_Parameter(BuiltInParameter.REVISION_CLOUD_REVISION).AsValueString();
               

                    int columnIndex= -1; // Initialize to -1 to indicate that the column wasn't found

                 
                    // Iterate through the Columns collection to find the column index
                    foreach (DataGridViewColumn column in dataGridView1.Columns)
                    {
                        if (column.HeaderText == "Revision Cloud - "+ (j + 1).ToString())
                        {
                            columnIndex = column.Index; // Store the index of the "Revision Cloud" column
                            break; 
                        }
                    }
                  
                    int rowIndex = i; // Specify the row index where you want to set the ComboBox value
                        
                    // Check if the specified row index is valid
                    if (rowIndex >= 0 && rowIndex < dataGridView1.Rows.Count)
                    {
                        // Get the ComboBox cell in the specified row and column
                        DataGridViewComboBoxCell comboBoxCell = dataGridView1.Rows[rowIndex].Cells[columnIndex] as DataGridViewComboBoxCell;
                            
                        // Check if the cell is a ComboBox cell
                        if (comboBoxCell != null)
                        {
                            // Set the items for the ComboBox cell
                            comboBoxCell.DataSource = RevNl; 
                            comboBoxCell.Value = RevNl[0];
                                                                
                            for(int k=0;k<RevNl.Count;k++) 
                            { 
                                if (selectRev == RevNl[k])
                                {
                                    comboBoxCell.Value = RevNl[k];
                                    dataGridView1.Columns[columnIndex].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                                }
                            }       
                        }
                        else
                        {
                            MessageBox.Show("The cell is not a ComboBox cell.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Invalid row index.");
                    }
                }
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        public ElementId GetElementId(string revisionName, Document Doc)
        {
            ElementId eleId = null;
            FilteredElementCollector coll = new FilteredElementCollector(Doc);
            coll.OfClass(typeof(Revision));
            var eleType = from Revision rc in coll
                          where rc.Name.Equals(revisionName)
                          select rc;
            eleId = eleType.First().Id;
            return eleId;
        }

        // Apply Button
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                using (TransactionGroup transactiongrp = new TransactionGroup(Doc, "Update Revision Cloud"))
                {
                    transactiongrp.Start();
                    // Iterate through each final view
                    for (int viewIndex = 0; viewIndex < finalViews.Count; viewIndex++)
                    {
                        // Get all the revision clouds associated with the current view
                        var eleCategory = new ElementCategoryFilter(BuiltInCategory.OST_RevisionClouds);
                        var collectRevCloud = finalViews[viewIndex].GetDependentElements(eleCategory);

                        // Iterate through each revision cloud in the current view
                        for (int cloudIndex = 0; cloudIndex < collectRevCloud.Count; cloudIndex++)
                        {
                            // Get the Revit element for the current revision cloud
                            var el = Doc.GetElement(collectRevCloud[cloudIndex]);

                            // Get the current selected revision associated with the revision cloud
                            string currentRevision = el.get_Parameter(BuiltInParameter.REVISION_CLOUD_REVISION).AsValueString();

                            // Find the corresponding DataGridView column index for the current revision cloud
                            int columnIndex = -1;
                            foreach (DataGridViewColumn column in dataGridView1.Columns)
                            {
                                if (column.HeaderText == "Revision Cloud - " + (cloudIndex + 1).ToString())
                                {
                                    columnIndex = column.Index;
                                    break;
                                }
                            }

                            // Check if the column index is valid
                            if (columnIndex != -1)
                            {
                                // Get the selected revision from the ComboBox cell in the DataGridView
                                DataGridViewComboBoxCell comboBoxCell = dataGridView1.Rows[viewIndex].Cells[columnIndex] as DataGridViewComboBoxCell;
                                if (comboBoxCell != null)
                                {
                                    string selectedRevision = comboBoxCell.Value?.ToString();

                                    // Check if the selected revision is different from the current revision
                                    if (selectedRevision != currentRevision)
                                    {
                                        // Update the revision cloud's revision parameter with the selected revision
                                        Parameter revisionParameter = el.get_Parameter(BuiltInParameter.REVISION_CLOUD_REVISION);
                                        using (Transaction transaction = new Transaction(Doc, "Update RC"))
                                        {
                                            transaction.Start();
                                            if (revisionParameter != null)
                                            {
                                                ElementId rCloud = GetElementId(selectedRevision, Doc);
                                                revisionParameter.Set(rCloud);
                                            }
                                            transaction.Commit();
                                        }
                                    }
                                }
                            }
                        }
                    }
                    transactiongrp.Assimilate();
                }

                MessageBox.Show("Revision updates applied successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while updating revisions: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //Finish Button
        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            Close();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel1.LinkVisited = true;
            System.Diagnostics.Process.Start("https://kpm-engineering.com/");
        }
    }
}
