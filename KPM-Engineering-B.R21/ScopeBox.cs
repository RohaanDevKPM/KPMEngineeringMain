using System;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Drawing;
using System.IO;
using View = Autodesk.Revit.DB.View;
using Color = System.Drawing.Color;
using System.Collections.Generic;

namespace KPMEngineeringA.Revit
{
    public partial class ScopeBox : System.Windows.Forms.Form
    {
        private UIApplication uiapp;
        private Document doc;
        private string dataFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RevitScopeBoxData.txt");
        private int serialNoCounter = 1;

        public ScopeBox(UIApplication uiapp)
        {
            InitializeComponent();
            this.uiapp = uiapp;
            this.doc = uiapp.ActiveUIDocument.Document;

            button1.Click += button1_Click;
            button2.Click += button2_Click;
            button3.Click += button3_Click;
            button4.Click += button4_Click;
            button5.Click += button5_Click;
            btn_Finish.Click += btn_Finish_Click;
            btn_Apply.Text = "Apply";
            btn_Apply.Click += btn_Apply_Click;

            this.StartPosition = FormStartPosition.CenterScreen;
            this.Controls.Add(btn_Apply);

            PopulateFloorPlans();
            PopulateScopeBoxes();

            InitializeListView();
            LoadPersistedData();
        }

        private void InitializeListView()
        {
            if (listView1 != null)
            {
                listView1.View = System.Windows.Forms.View.Details;
                listView1.Columns.Add("Serial No", -2, HorizontalAlignment.Left);
                listView1.Columns.Add("View Name", -2, HorizontalAlignment.Left);
                listView1.Columns.Add("Scope Box", -2, HorizontalAlignment.Left);
                listView1.Columns.Add("Date & Time", -2, HorizontalAlignment.Left);
            }
        }

        private void PopulateScopeBoxes()
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfCategory(BuiltInCategory.OST_VolumeOfInterest);
            collector.WhereElementIsNotElementType();

            foreach (Element element in collector)
            {
                if (element.Category != null && element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_VolumeOfInterest)
                {
                    checkedListBox1.Items.Add(element.Name);
                }
            }
        }

        private void PopulateFloorPlans()
        {
            var collectorViews = new FilteredElementCollector(doc);
            var collectedViews = collectorViews.OfClass(typeof(View)).ToElements();

            foreach (View view in collectedViews)
            {
                if (view.ViewType == ViewType.FloorPlan)
                {
                    checkedListBox2.Items.Add(view.Name);
                }
            }
        }

        private void btn_Apply_Click(object sender, EventArgs e)
        {
            ApplyScopeBoxesToSelectedViews();
        }

        private void ApplyScopeBoxesToSelectedViews()
        {
            // Retrieve the selected scope box
            var selectedScopeBoxName = checkedListBox1.CheckedItems.Cast<string>().FirstOrDefault();
            var selectedScopeBox = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_VolumeOfInterest)
                .FirstOrDefault(e => e.Name == selectedScopeBoxName);

            if (selectedScopeBox == null)
            {
                MessageBox.Show("Please select a scope box.");
                return;
            }

            // Retrieve the selected views
            var selectedViewNames = checkedListBox2.CheckedItems.Cast<string>().ToList();
            var selectedViews = new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(v => selectedViewNames.Contains(v.Name) && v.ViewType == ViewType.FloorPlan)
                .ToList();

            if (!selectedViews.Any())
            {
                MessageBox.Show("Please select at least one view.");
                return;
            }

            using (Transaction trans = new Transaction(doc, "Apply Scope Box"))
            {
                trans.Start();

                HashSet<string> existingEntries = new HashSet<string>(
                    listView1.Items.Cast<ListViewItem>().Select(i => $"{i.SubItems[1].Text}-{i.SubItems[2].Text}")
                );

                foreach (var view in selectedViews)
                {
                    Parameter volumeOfInterestParam = view.get_Parameter(BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP);
                    if (volumeOfInterestParam != null && !volumeOfInterestParam.IsReadOnly)
                    {
                        volumeOfInterestParam.Set(selectedScopeBox.Id);

                        string entryKey = $"{view.Name}-{selectedScopeBox.Name}";
                        if (!existingEntries.Contains(entryKey))
                        {
                            string currentTime = DateTime.Now.ToString("g");
                            string dataLine = $"{serialNoCounter}\t{view.Name}\t{selectedScopeBox.Name}\t{currentTime}\n";
                            File.AppendAllText(dataFilePath, dataLine);

                            ListViewItem item = new ListViewItem(serialNoCounter.ToString());
                            item.SubItems.Add(view.Name);
                            item.SubItems.Add(selectedScopeBox.Name);
                            item.SubItems.Add(currentTime);
                            item.ForeColor = System.Drawing.Color.Red;
                            listView1.Items.Add(item);

                            serialNoCounter++;
                        }
                    }
                }

                trans.Commit();
            }
        }

        private void LoadPersistedData()
        {
            if (File.Exists(dataFilePath))
            {
                string[] lines = File.ReadAllLines(dataFilePath);
                foreach (string line in lines)
                {
                    string[] parts = line.Split('\t');
                    if (parts.Length == 4)
                    {
                        ListViewItem item = new ListViewItem(parts[0]);
                        item.SubItems.Add(parts[1]);
                        item.SubItems.Add(parts[2]);
                        item.SubItems.Add(parts[3]);
                        item.ForeColor = Color.Maroon;
                        listView1.Items.Add(item);

                        int currentSerial = int.Parse(parts[0]);
                        if (currentSerial >= serialNoCounter)
                        {
                            serialNoCounter = currentSerial + 1;
                        }
                    }
                }
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void btn_Finish_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button5_Click(object sender, EventArgs e)
        {

        }

        private void ClearDataFile()
        {
            if (File.Exists(dataFilePath))
            {
                File.WriteAllText(dataFilePath, string.Empty);
            }
        }
    }
}
