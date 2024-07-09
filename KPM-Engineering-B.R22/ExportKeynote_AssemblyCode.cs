using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Application = Microsoft.Office.Interop.Excel.Application;

namespace KPMEngineeringB.R
{
    public partial class ExportKeynote_AssemblyCode : System.Windows.Forms.Form
    {
        Autodesk.Revit.DB.Document Doc;
        // Define a DataTable to hold the Excel data
        System.Data.DataTable dt = new System.Data.DataTable();

        private ExternalCommandData storedCommandData;
        public ExportKeynote_AssemblyCode(Document doc, ExternalCommandData commandData)
        {
            InitializeComponent();
            Doc = doc;
            storedCommandData = commandData;
        }
        //Create Button
        private void button1_Click(object sender, EventArgs e)
        {

            folderBrowserDialog1.ShowDialog();
            var folD = folderBrowserDialog1.SelectedPath as string;
            using (Transaction t = new Transaction(Doc, "Create Sheet For IFC Family"))
            {
                t.Start();

                List<string> selectedStrings = ChooseCategory.getLists;

                List<ElementId> elementIds = new List<ElementId>();
                var all_categories = Doc.Settings.Categories;

                foreach (string selectedString in selectedStrings)
                {
                    foreach (var category in all_categories)
                    {
                        if (((Autodesk.Revit.DB.Category)category).CategoryType == CategoryType.Model)
                        {
                            if (((Autodesk.Revit.DB.Category)category).Name == selectedString)
                            {
                                elementIds.Add(((Autodesk.Revit.DB.Category)category).Id);
                                break;
                            }
                        }
                    }
                }


                ICollection<ElementId> collection = new List<ElementId>(elementIds);

                ElementParameterFilter parameterFilter = new ElementParameterFilter(new FilterCategoryRule(collection));

                FilteredElementCollector collector = new FilteredElementCollector(Doc);
                List<Element> elements = new List<Element>(collector.WherePasses(parameterFilter).WhereElementIsElementType().ToElements());

                List<string> familyName = new List<string> { "Family Name" };
                List<string> familyType = new List<string> { "Type Name" };
                List<string> familyId = new List<string> { "Element Id" };
              
                List<string> AssemblyCode = new List<string> { "Assembly Code" };
                List<string> KeyNote = new List<string> { "Key Note" };

                List<string> eleCategory = new List<string> { "Element Category" };

                foreach (Element element in elements)
                {
                    familyId.Add(element.Id.ToString());
                    eleCategory.Add(element.Category.Name);
                    familyName.Add(element.LookupParameter("Family Name").AsString());
                    familyType.Add(element.LookupParameter("Type Name").AsString());

                    try
                    {
                        if (element.get_Parameter(BuiltInParameter.UNIFORMAT_CODE) != null)
                        {  
                          AssemblyCode.Add(element.get_Parameter(BuiltInParameter.UNIFORMAT_CODE).AsString());
                            
                        }
                        else
                        {
                            AssemblyCode.Add("");
                        }
                        if (element.get_Parameter(BuiltInParameter.KEYNOTE_PARAM) != null)
                        {
                            KeyNote.Add(element.get_Parameter(BuiltInParameter.KEYNOTE_PARAM).AsString()); 
                        }
                        else
                        {
                            KeyNote.Add("");
                               
                        }
                    }
                    catch
                    {
                        AssemblyCode.Add("");
                        KeyNote.Add("");
                    }

                }

                List<List<string>> GL = new List<List<string>> { familyId, eleCategory, familyName, familyType, AssemblyCode, KeyNote}; // ifcExportAs 

                string fullPath = Path.GetFullPath(folD);
                string savePath = Path.Combine(fullPath, "Export Keynote & Assembly Code");
                WriteToExcel(GL, savePath);

                t.Commit();
            }
        }
        // Import Button
        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel Files (*.xls)|*.xls|All Files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.ShowDialog();
            var selectedFilePath = openFileDialog.FileName;

            Application excel = new Application();
            excel.Visible = false; // Set to true to make Excel visible, false to keep it hidden


            Workbook wb = excel.Workbooks.Open(selectedFilePath);

            // Get the first worksheet
            Worksheet ws = (Worksheet)wb.Worksheets[1];
            // Get the range of used cells in the worksheet
            Range range = ws.UsedRange;


            // Loop through each row and column in the range and add data to the DataTable
            for (int row = 1; row <= range.Rows.Count; row++)
            {
                DataRow dr = dt.NewRow();
                for (int col = 1; col <= range.Columns.Count; col++)
                {
                    // Add column headers to the DataTable if it's the first row
                    if (row == 1)
                    {
                        dt.Columns.Add((range.Cells[row, col] as Range).Value2.ToString());
                    }
                    else
                    {
                        dr[col - 1] = (range.Cells[row, col] as Range).Value2;
                    }
                }
                if (row != 1) // Exclude the header row
                {
                    dt.Rows.Add(dr);
                }
            }

            // Close the workbook and Excel application
            wb.Close(false);
            excel.Quit();


            dataGridView1.DataSource = dt;

            dataGridView1.RowHeadersVisible = false;
            // Set your desired AutoSize Mode:
            dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView1.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView1.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView1.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            // Now that DataGridView has calculated it's Widths; we can now store each column Width values.
            for (int i = 0; i <= dataGridView1.Columns.Count - 1; i++)
            {
                // Store Auto Sized Widths:
                int colw = dataGridView1.Columns[i].Width;

                // Remove AutoSizing:
                dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;

                // Set Width to calculated AutoSize value:
                dataGridView1.Columns[i].Width = colw;
            }

            HashSet<string> uniqueValues = new HashSet<string>();

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (!row.IsNewRow && row.Cells["Element Category"].Value != null)
                {
                    string value = row.Cells["Element Category"].Value.ToString().Trim();

                    uniqueValues.Add(value);
                }
            }

            comboBox1.Items.AddRange(uniqueValues.ToArray());

        }

        private void ExportToFamilyForIFC_Load(object sender, EventArgs e)
        {


        }
        private void WriteToExcel(List<List<string>> data, string outputPath)
        {
            // Create Excel application
            Microsoft.Office.Interop.Excel.Application excelApp = new Microsoft.Office.Interop.Excel.Application();
            excelApp.Visible = false; // Make Excel visible

            // Create a new workbook and worksheet
            Microsoft.Office.Interop.Excel.Workbook workbook = excelApp.Workbooks.Add();
            _Worksheet worksheet = (_Worksheet)workbook.Sheets[1];

            //  Perfect loop for data display in excel
            for (int i = 0; i < data.Count; i++)
            {
                List<string> innerList = data[i];

                for (int j = 0; j < innerList.Count; j++)
                {
                    worksheet.Cells[j + 1, i + 1] = innerList[j];
                    // Get the header cell
                    var headerCell = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[1, i + 1];

                    // Set the font of the header cell to bold
                    headerCell.Font.Bold = true;
                }
            }

            // Save workbook
            workbook.SaveAs(outputPath, XlFileFormat.xlWorkbookNormal);

            // Close workbook and release resources
            workbook.Close();
            excelApp.Quit();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);
            MessageBox.Show("Excel File Exported");
        }
        private void ElementCategory_selectedIndex(object sender, EventArgs e)
        {
            // Filter the DataTable using DataView
            DataView dv = new DataView(dt);
            dv.RowFilter = $"[Element Category] = '{comboBox1.SelectedItem}'";

            dataGridView1.DataSource = dv.ToTable();
        }

        private void flyName_TextChanged(object sender, EventArgs e)
        {
            // Filter the DataTable using DataView
            DataView dv = new DataView(dt);
            dv.RowFilter = $"[Family Name] LIKE '%{textBox1.Text}%'";

            dataGridView1.DataSource = dv.ToTable();
        }

        // Update
        private void button3_Click(object sender, EventArgs e)
        {
           List<string> updatedElements = new List<string>();
            using (Transaction transaction = new Transaction(Doc))
            {
                transaction.Start("Update Elements");
          
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.IsNewRow) continue;

                    string assemblyCodeValue = Convert.ToString(row.Cells["Assembly Code"].Value);
                    string keyNoteValue = Convert.ToString(row.Cells["Key Note"].Value);
                    int elementID = Convert.ToInt32(row.Cells["Element Id"].Value);

                    var elemenT = Doc.GetElement(new ElementId(elementID));
                   

                    try
                    {
                        if (elemenT.get_Parameter(BuiltInParameter.UNIFORMAT_CODE) != null)
                        {
                            var update = elemenT.get_Parameter(BuiltInParameter.UNIFORMAT_CODE).Set(assemblyCodeValue);
                        }
                        
                        if (elemenT.get_Parameter(BuiltInParameter.KEYNOTE_PARAM) != null)
                        {
                            var update = elemenT.get_Parameter(BuiltInParameter.KEYNOTE_PARAM).Set(keyNoteValue);
                        }
                        
                    }
                    catch { }
                }

                transaction.Commit();
                TaskDialog.Show("Updated Family Types", " Key Note & Assembly Code Values of Family Types Updated.");

            }
            this.DialogResult = DialogResult.OK;
            Close();
        }

        // choose category
        private void button4_Click(object sender, EventArgs e)
        {
            ChooseCategory chooseBro = new ChooseCategory(Doc, storedCommandData);
            chooseBro.Show();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel1.LinkVisited = true;
            System.Diagnostics.Process.Start("https://kpm-engineering.com/");
        }
    }
}
