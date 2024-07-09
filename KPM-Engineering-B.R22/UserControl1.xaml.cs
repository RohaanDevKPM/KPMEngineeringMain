using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Google.Protobuf.WellKnownTypes;
using MySqlX.XDevAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Category = Autodesk.Revit.DB.Category;
using CheckBox = System.Windows.Controls.CheckBox;
using Document = Autodesk.Revit.DB.Document;

namespace KPMEngineeringB.R
{
    public partial class UserControl1 : Window
    {
        Autodesk.Revit.DB.Document Doc;
        UIDocument UiDoc;
        public static List<string> CatList = new List<string>();
        public static HashSet<string> CatSelect = new HashSet<string>();
        public static List<string> selectParam = new List<string>();
        public List<Category> TempCat = new List<Category>();
        private ExternalCommandData storedCommandData;
        public static HashSet<string> selectedCategoryParameters = new HashSet<string>();

        public UserControl1(Document doc, ExternalCommandData commandData)
        {
            InitializeComponent();
            Doc = doc;
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            UiDoc = uiDoc;
            storedCommandData = commandData;
        }

        public class CategorySelectionFilter : ISelectionFilter
        {
            private HashSet<string> CatSelect;

            // Constructor that takes the selectedCategories list as a parameter
            public CategorySelectionFilter(HashSet<string> CatSelect)
            {
                this.CatSelect = CatSelect;
            }

            public bool AllowElement(Element elem)
            {
                return elem.Category != null && CatSelect.Contains(elem.Category.Name);
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                return true;
            }
        }

        private void AutoNumbering_Load(object sender, RoutedEventArgs e)
        {
            listView1.Items.Clear(); // Clear the items in the ListView
            
            var all_categories = Doc.Settings.Categories;
            foreach (var category in all_categories)
            {
                if (((Autodesk.Revit.DB.Category)category).CategoryType == CategoryType.Model && !((Autodesk.Revit.DB.Category)category).Name.Contains("dwg"))
                {
                    CatList.Add(((Autodesk.Revit.DB.Category)category).Name);
                    TempCat.Add((Autodesk.Revit.DB.Category)category); // for select
                }
            }
            var sortedCategories = CatList.OrderBy(c => c).Distinct().ToList();

            listView1.Items.Clear();

            foreach (var category in sortedCategories)
            {
                listView1.Items.Add(category);
            }
        }


        // Event handler for when an item is checked
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UpdateParameters(sender, isChecked: true);
        }
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateParameters(sender, isChecked: false);
        }

        private void UpdateParameters(object sender, bool isChecked)
        {
            selectedCategoryParameters.Clear();
            var checkBox = (CheckBox)sender;
            var categoryName = checkBox.Content.ToString();

            if (isChecked)
            {
                CatSelect.Add(categoryName);
            }
            else
            {
                CatSelect.Remove(categoryName);
            }

            // Iterate through each selected category
            foreach (var selectedCategory in CatSelect)
            {
                var categoryParameters = GetCategoryParameters(selectedCategory);

                if (selectedCategoryParameters.Count == 0)
                {
                    // If this is the first selected category, add all its parameters
                    selectedCategoryParameters.UnionWith(categoryParameters);
                }
                else
                {
                    // Find common parameters among all selected categories
                    selectedCategoryParameters.IntersectWith(categoryParameters);
                }
            }
           ExCB.ItemsSource = selectedCategoryParameters.ToList();
          
        }

        private HashSet<string> GetCategoryParameters(string categoryName)
        {
            var categoryParameters = new HashSet<string>();
            var category = TempCat.FirstOrDefault(cat => cat.Name == categoryName);
            if (category != null)
            {
                var categoryId = category.Id.IntegerValue;
                FilteredElementCollector collector = new FilteredElementCollector(Doc);
                ICollection<Element> elements = collector.OfCategoryId(new ElementId(categoryId)).WhereElementIsNotElementType().ToElements();

                foreach (Element element in elements)
                {
                    foreach (Autodesk.Revit.DB.Parameter param in element.Parameters)
                    {
                        if (param.IsReadOnly == false && param.StorageType == StorageType.String)
                        {
                            categoryParameters.Add(param.Definition.Name);
                        }
                    }
                }
            }
            return categoryParameters;
        }
       
        // Closing event handler
        private void UserControl1_Closing(object sender, CancelEventArgs e)
        {
            CatSelect.Clear();
        }

        // Start Button
        private void Start_Selection(object sender, RoutedEventArgs e)
        {
            var WindoW = new Autodesk.Revit.UI.ColorSelectionDialog();
            WindoW.Show();
            var UColoR = WindoW.SelectedColor;

            this.Hide();

            var OverRideSet = new Autodesk.Revit.DB.OverrideGraphicSettings();
            OverRideSet.SetProjectionLineColor(UColoR);
            var ActiveV = Doc.ActiveView;
            Reference reference = null;

            // Start a new transaction group
            using (TransactionGroup group = new TransactionGroup(Doc, "AutoNumbering"))
            {
                group.Start();

                try
                {
                    Autodesk.Revit.DB.OverrideGraphicSettings ResetGraphics = new Autodesk.Revit.DB.OverrideGraphicSettings();
                    List<ElementId> selectedElementIds = new List<ElementId>();

                    try
                    {
                        while (true)
                        {
                            try
                            {
                                reference = UiDoc.Selection.PickObject(ObjectType.Element, new CategorySelectionFilter(UserControl1.CatSelect));
                                // Start a new transaction for the selection process
                                using (Transaction selectionTransaction = new Transaction(Doc, "Element Selection"))
                                {
                                    selectionTransaction.Start();
                                    if (reference == null)
                                        break;

                                    else if (selectedElementIds.Contains(reference.ElementId))
                                    {
                                        selectedElementIds.Remove(reference.ElementId);
                                        ActiveV.SetElementOverrides(reference.ElementId, ResetGraphics);
                                    }
                                    else
                                    {
                                        selectedElementIds.Add(reference.ElementId);
                                        ActiveV.SetElementOverrides(reference.ElementId, OverRideSet);
                                    }
                                    Doc.Regenerate();
                                    selectionTransaction.Commit();
                                }
                            }
                            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                            {
                                // User pressed Escape (Esc) key to cancel selection
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        TaskDialog.Show("Error", ex.Message);
                        // selectionTransaction.RollBack();
                    }
                 

                    // Start a new transaction within the transaction group for parameter setting
                    using (Transaction transaction = new Transaction(Doc, "Set Value"))
                    {
                        transaction.Start();

                        try
                        {
                            for(int i = 0; i < selectedElementIds.Count; i++)
                            {  
                                var ele = Doc.GetElement(selectedElementIds[i]);

                                var selectedValue = ExCB.SelectedValue;

                                var p = ele.LookupParameter((string)selectedValue);

                                if (p != null)
                                {
                                    var prefixText = Prefix.Text;
                                    var startNum = i + Convert.ToInt64(StartNumber.Text);
                                    var suffixText = Suffix.Text;
                                  
                                    var value = prefixText + startNum.ToString() + suffixText;

                                    p.Set(value);
                                }
                                else
                                {
                                    TaskDialog.Show("Error", "Selected parameter does not exist for element: " + ele.Name);
                                }
                            }

                            // Commit the transaction for parameter setting
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            TaskDialog.Show("Error", ex.Message);
                            transaction.RollBack();
                        }
                    }

                    // Commit the transaction group
                    group.Assimilate();
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Error", ex.Message);
                    group.RollBack();
                }
            }
        }
    }
}