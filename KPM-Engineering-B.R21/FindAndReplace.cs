using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Polly;
using Polly.Retry;
namespace KPMEngineeringB.R
{
    public class RetryPolicyProvider
    {
        public static RetryPolicy CreateRetryPolicy()
        {
            return Policy
                .Handle<Exception>()
                .WaitAndRetry(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }
    }
    public partial class FindAndReplace : System.Windows.Forms.Form
    {
        Autodesk.Revit.DB.Document Doc;
        private readonly RetryPolicy _retryPolicy;


        public List<string> CatList = new List<string>();
        public static List<Category> CatSelect = new List<Category>();
        public static List<Element> famSelect = new List<Element>();

        public static List<string> parameterNames = new List<string>();
        public static IList<Element> collectElement = new List<Element>();


        public List<string> ElTypeList = new List<string>();
        public List<string> EleFrom = new List<string>();

        public List<string> MethodList = new List<string>();

        private DataTable listValue = new DataTable();

        public static List<ElementId> cElemtoTable = new List<ElementId>();
        public FindAndReplace(Document doc)
        {
            InitializeComponent();
            Doc = doc;
            _retryPolicy = RetryPolicyProvider.CreateRetryPolicy();
        }

        private void FindAndReplace_Load(object sender, EventArgs e)
        {
            _retryPolicy.Execute(() =>
            {


                List<Category> TempCat = new List<Category>();
            var all_categories = Doc.Settings.Categories;
            foreach (var category in all_categories)
            {
                if (((Autodesk.Revit.DB.Category)category).CategoryType == CategoryType.Model)
                {
                    CatList.Add(((Autodesk.Revit.DB.Category)category).Name);
                    TempCat.Add((Autodesk.Revit.DB.Category)category); // for select
                }
            }
            
            List<int> sortedIndices = Enumerable.Range(0, CatList.Count).OrderBy(i => CatList[i]).ToList();
            var sortedListB = sortedIndices.Select(i => TempCat[i]).ToList();
            CatSelect = CatSelect.Concat(sortedListB).ToList();
            CatList.Sort();

            cb_SelectCategory.DataSource = CatList;
            cb_SelectCategory.SelectedIndex = 0;

            ElTypeList.Clear();
            ElTypeList.Add("Instance");
            ElTypeList.Add("Type");
            cb_ElementType.DataSource = ElTypeList;
            cb_ElementType.SelectedIndex = 0;

            MethodList.Clear();
            MethodList.Add("Equals");
            MethodList.Add("Contains");
            cb_FindMethod.DataSource = MethodList;

            EleFrom.Clear();
            EleFrom.Add("Active View");
            EleFrom.Add("Project");
            cb_Element_From.DataSource = EleFrom;

            listValue.Columns.Add("Sr.No",typeof(int));
            listValue.Columns["Sr.No"].ReadOnly = true;
            
            listValue.Columns.Add("Element Id");
            listValue.Columns["Element Id"].ReadOnly = true;
            listValue.Columns.Add("Parameter Old Value");
            listValue.Columns["Parameter Old Value"].ReadOnly = true;
            listValue.Columns.Add("Parameter New Value");
            listValue.Columns["Parameter New Value"].ReadOnly = true;

           
            dataGridView1.RowHeadersVisible = false;


            });
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            famSelect.Clear();
            var cat = cb_SelectCategory.SelectedIndex;
            var eleCat = CatSelect[cat].Id;
            var familyType = new FilteredElementCollector(Doc).OfCategoryId(eleCat).WhereElementIsElementType().ToElements();

            if (familyType.Count > 0)
            {
                List<string> familyTypeNames = familyType
               .Select(element => element.get_Parameter(BuiltInParameter.ALL_MODEL_FAMILY_NAME).AsString() + " - " + element.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString())
                .ToList();

                foreach (var famType in familyType)
                {
                    famSelect.Add(famType);
                }
                cb_FamilyType.DataSource = familyTypeNames; 
            }
            else
            {
                cb_FamilyType.DataSource = null;

                cb_SelectParam.DataSource = null;
            }
        }
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            var cat = cb_SelectCategory.SelectedIndex;
            var eleCat = CatSelect[cat].Id;
            if (cb_ElementType.SelectedValue == "Instance")
            {
                label4.Visible = true;
                cb_Element_From.Visible = true;

            }
            else if (cb_ElementType.SelectedValue == "Type")
            {
                label4.Visible = false;
                cb_Element_From.Visible = false;
            }
            var familyType = new FilteredElementCollector(Doc).OfCategoryId(eleCat).WhereElementIsElementType().OfType<Element>().ToList();
            if (familyType.Count > 0)
            {
                List<string> familyTypeNames = familyType
                    .Select(element => element.get_Parameter(BuiltInParameter.ALL_MODEL_FAMILY_NAME).AsString() + " - " + element.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString())
                .ToList();

                foreach (var famType in familyType)
                {
                    famSelect.Add(famType);
                }
            
                cb_FamilyType.DataSource = familyTypeNames; 
                cb_FamilyType.SelectedIndex = 0;
            }
            else
            {
                cb_FamilyType.DataSource = null;
            }
            if (famSelect.Count > 0)
            {
                IList<FilterRule> filterRules = new List<FilterRule>();
                var fam = cb_FamilyType.SelectedIndex;
                var famElement = Doc.GetElement(famSelect[fam].Id);
                var famName = famElement.get_Parameter(BuiltInParameter.ALL_MODEL_FAMILY_NAME).AsString();
                var typeName = famElement.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString();
                var famNameID = famElement.get_Parameter(BuiltInParameter.ALL_MODEL_FAMILY_NAME).Id;
                var typeNameID = famElement.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).Id;
                var filterRule1 = ParameterFilterRuleFactory.CreateEqualsRule(famNameID, famName, true);
                var filterRule2 = ParameterFilterRuleFactory.CreateEqualsRule(typeNameID, typeName, true);
                filterRules.Add(filterRule1);
                filterRules.Add(filterRule2);
                var elemParaFilter = new ElementParameterFilter(filterRules);

                if (cb_ElementType.SelectedValue == "Type")
                {
                    collectElement.Clear();
                    parameterNames.Clear();
                    cb_SelectParam.DataSource = null;
                    ParameterMap parameterMap = famElement.ParametersMap;
                    foreach (var p in parameterMap)
                    {
                        Parameter parameter = p as Parameter;
                        if (parameter.IsReadOnly == false)
                        {
                            if (parameter.StorageType == StorageType.String)
                            {
                                parameterNames.Add(parameter.Definition.Name);
                            }
                        }
                    }
                    cb_SelectParam.DataSource = parameterNames;
                }
                else if (cb_ElementType.SelectedValue == "Instance")
                {
                    if (cb_Element_From.SelectedValue == "Active View")
                    {
                        collectElement.Clear();
                        parameterNames.Clear();
                        cb_SelectParam.DataSource = null;
                        var ActiveView = Doc.ActiveView;
                        var collectEle = new FilteredElementCollector(Doc, ActiveView.Id).WherePasses(elemParaFilter).WhereElementIsNotElementType().ToElements();
                        collectElement = collectElement.Concat(collectEle).ToList();
                        if (collectEle.Count > 0)
                        {
                            var firstEle = collectEle.FirstOrDefault();
                            ParameterMap parameterMap = firstEle.ParametersMap;
                            foreach ( var p in parameterMap)
                            {
                                Parameter parameter = p as Parameter;
                                if (parameter.IsReadOnly == false)
                                {
                                    if (parameter.StorageType == StorageType.String)
                                    {
                                        parameterNames.Add(parameter.Definition.Name);
                                    }
                                }
                            }
                            cb_SelectParam.DataSource = parameterNames;
                        }
                    }
                    else if (cb_Element_From.SelectedValue == "Project")
                    {
                        collectElement.Clear();
                        parameterNames.Clear();
                        cb_SelectParam.DataSource = null;
                        var collectEle = new FilteredElementCollector(Doc).WherePasses(elemParaFilter).WhereElementIsNotElementType().ToElements();
                        collectElement = collectElement.Concat(collectEle).ToList();
                        if (collectEle.Count > 0)
                        {
                            var firstEle = collectEle.FirstOrDefault();
                            ParameterMap parameterMap = firstEle.ParametersMap;
                            foreach (var p in parameterMap)
                            {
                                Parameter parameter = p as Parameter;
                                if (parameter.IsReadOnly == false)
                                {
                                    if (parameter.StorageType == StorageType.String)
                                    {
                                        parameterNames.Add(parameter.Definition.Name);
                                    }
                                }
                            }
                            cb_SelectParam.DataSource = parameterNames;
                        }
                    }
                }
            }
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (famSelect.Count > 0)
            {
                IList<FilterRule> filterRules = new List<FilterRule>();
                var fam = cb_FamilyType.SelectedIndex;
                var famElement = Doc.GetElement(famSelect[fam].Id);
                var famName = famElement.get_Parameter(BuiltInParameter.ALL_MODEL_FAMILY_NAME).AsString();
                var typeName = famElement.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString();
                var famNameID = famElement.get_Parameter(BuiltInParameter.ALL_MODEL_FAMILY_NAME).Id;
                var typeNameID = famElement.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).Id;
                var filterRule1 = ParameterFilterRuleFactory.CreateEqualsRule(famNameID, famName, true);
                var filterRule2 = ParameterFilterRuleFactory.CreateEqualsRule(typeNameID, typeName, true);
                filterRules.Add(filterRule1);
                filterRules.Add(filterRule2);
                var elemParaFilter = new ElementParameterFilter(filterRules);
                
                if (cb_ElementType.SelectedValue == "Type")
                {
                    collectElement.Clear();
                    parameterNames.Clear();
                    cb_SelectParam.DataSource = null;
                    ParameterMap parameterMap = famElement.ParametersMap;
                    foreach (var p in parameterMap)
                    {
                        Parameter parameter = p as Parameter;
                        if (parameter.IsReadOnly == false)
                        {
                            if (parameter.StorageType == StorageType.String)
                            {
                                parameterNames.Add(parameter.Definition.Name); 
                            }
                        }
                    }
                    cb_SelectParam.DataSource = parameterNames;
                }
                else if (cb_ElementType.SelectedValue == "Instance")
                {
                    if (cb_Element_From.SelectedValue == "Active View")
                    {
                        collectElement.Clear();
                        parameterNames.Clear();
                        cb_SelectParam.DataSource = null;
                        var ActiveView = Doc.ActiveView;
                        var collectEle = new FilteredElementCollector(Doc, ActiveView.Id).WherePasses(elemParaFilter).WhereElementIsNotElementType().ToElements();
                        collectElement = collectElement.Concat(collectEle).ToList();
                        if (collectEle.Count > 0)
                        {
                            var firstEle = collectEle.FirstOrDefault();
                            ParameterMap parameterMap = firstEle.ParametersMap;
                            foreach (var p in parameterMap)
                            {
                               
                                Parameter parameter = p as Parameter;
                                if (parameter.IsReadOnly == false)
                                {
                                    if (parameter.StorageType == StorageType.String)
                                    {
                                        parameterNames.Add(parameter.Definition.Name);
                                    }
                                }
                            }
                           
                            cb_SelectParam.DataSource = parameterNames;
                        }
                    }
                    else if (cb_Element_From.SelectedValue == "Project")
                    {
                        collectElement.Clear();
                        parameterNames.Clear();
                        cb_SelectParam.DataSource = null;
                        var collectEle = new FilteredElementCollector(Doc).WherePasses(elemParaFilter).WhereElementIsNotElementType().ToElements();
                        collectElement = collectElement.Concat(collectEle).ToList();
                        if (collectEle.Count > 0)
                        {
                            var firstEle = collectEle.FirstOrDefault();
                         
                            ParameterMap parameterMap = firstEle.ParametersMap;
                            foreach (var p in parameterMap)
                            {
                                Parameter parameter = p as Parameter;
                                if (parameter.IsReadOnly == false)
                                {
                                    if (parameter.StorageType == StorageType.String)
                                    {
                                        parameterNames.Add(parameter.Definition.Name); 
                                    }
                                }
                            }
                            cb_SelectParam.DataSource = parameterNames;
                        }
                    }
                }
            }

        }

        private void cb_Element_From_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (famSelect.Count > 0)
            {
                IList<FilterRule> filterRules = new List<FilterRule>();
                var fam = cb_FamilyType.SelectedIndex;
                var famElement = Doc.GetElement(famSelect[fam].Id);
                var famName = famElement.get_Parameter(BuiltInParameter.ALL_MODEL_FAMILY_NAME).AsString();
                var typeName = famElement.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString();
                var famNameID = famElement.get_Parameter(BuiltInParameter.ALL_MODEL_FAMILY_NAME).Id;
                var typeNameID = famElement.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).Id;
                var filterRule1 = ParameterFilterRuleFactory.CreateEqualsRule(famNameID, famName, true);
                var filterRule2 = ParameterFilterRuleFactory.CreateEqualsRule(typeNameID, typeName, true);
                filterRules.Add(filterRule1);
                filterRules.Add(filterRule2);
                var elemParaFilter = new ElementParameterFilter(filterRules);
            
                if (cb_ElementType.SelectedValue == "Type")
                {
                    parameterNames.Clear();
                    cb_SelectParam.DataSource = null;
                    ParameterMap parameterMap = famElement.ParametersMap;
                    foreach (var p in parameterMap)
                    {
                        Parameter parameter = p as Parameter;
                        if (parameter.IsReadOnly == false)
                        {
                            if (parameter.StorageType == StorageType.String)
                            {
                                parameterNames.Add(parameter.Definition.Name); 
                            }
                        }
                    }
                    cb_SelectParam.DataSource = parameterNames;
                }
                else if (cb_ElementType.SelectedValue == "Instance")
                {
                    if (cb_Element_From.SelectedValue == "Active View")
                    {
                        collectElement.Clear();
                        parameterNames.Clear();
                        cb_SelectParam.DataSource = null;
                        var ActiveView = Doc.ActiveView;
                        var collectEle = new FilteredElementCollector(Doc, ActiveView.Id).WherePasses(elemParaFilter).WhereElementIsNotElementType().ToElements();
                        collectElement = collectElement.Concat(collectEle).ToList();
                        
                        if (collectEle.Count > 0)
                        {
                            var firstEle = collectEle.FirstOrDefault();
                            ParameterMap parameterMap = firstEle.ParametersMap;
                            foreach (var p in parameterMap)
                            {
                                Parameter parameter = p as Parameter;
                                if (parameter.IsReadOnly == false)
                                {
                                    if (parameter.StorageType == StorageType.String)
                                    {
                                        parameterNames.Add(parameter.Definition.Name); 
                                    }
                                }
                            }
                                cb_SelectParam.DataSource = parameterNames;
                        }
                    }
                    else if (cb_Element_From.SelectedValue == "Project")
                    {
                        collectElement.Clear();
                        parameterNames.Clear();
                        cb_SelectParam.DataSource = null;
                        var collectEle = new FilteredElementCollector(Doc).WherePasses(elemParaFilter).WhereElementIsNotElementType().ToElements();
                        collectElement = collectElement.Concat(collectEle).ToList();
                     
                        if (collectEle.Count > 0)
                        {
                            var firstEle = collectEle.FirstOrDefault();
                          
                            ParameterMap parameterMap = firstEle.ParametersMap;
                            foreach (var p in parameterMap)
                            {
                                Parameter parameter = p as Parameter;
                                if (parameter.IsReadOnly == false)
                                {
                                    if (parameter.StorageType == StorageType.String)
                                    {
                                        parameterNames.Add(parameter.Definition.Name);
                                    }
                                }
                            }
                            cb_SelectParam.DataSource = parameterNames;
                        }
                    }
                }
            }
        }
        // Find/Select
        private void Find_Select_Click(object sender, EventArgs e)
        {
            bool headerPresent = false;
           
            foreach (DataGridViewColumn col in dataGridView1.Columns)
            {
                var checkBoxCell = col.HeaderCell; // ["Select"]
                if (checkBoxCell != null && checkBoxCell.Value != null && checkBoxCell.Value == "Select")
                {
                    headerPresent = true;
                    break;
                }

            }
            if (!headerPresent)
            {
                DataGridViewCheckBoxColumn checkColumn = new DataGridViewCheckBoxColumn();
                checkColumn.Name = "Select";
                checkColumn.Width = 50;
                checkColumn.ReadOnly = false;

                dataGridView1.Columns.Add(checkColumn);
            }
            var fam = cb_FamilyType.SelectedIndex;
            var famElement = Doc.GetElement(famSelect[fam].Id);
            int rowNumber = 1;
            if (cb_ElementType.SelectedValue == "Type")
            {
                listValue.Clear();
                var p = cb_SelectParam.SelectedValue.ToString();
                var value = famElement.LookupParameter(p).AsString();

                if (value != null)
                {
                    if (cb_FindMethod.SelectedValue == "Equals" && textBox1.Text == value)
                    {
                        
                        listValue.Rows.Add(rowNumber,famElement.Id, value,textBox2.Text);
                       cElemtoTable.Add(famElement.Id);
                        rowNumber++;
                    }
                    if (cb_FindMethod.SelectedValue == "Contains" && value.Contains(textBox1.Text))
                    {
                        var rep = value.Replace(textBox1.Text, textBox2.Text);
                        listValue.Rows.Add(rowNumber,famElement.Id, value, rep);
                        cElemtoTable.Add(famElement.Id);
                        rowNumber++;
                    }
                   
                    dataGridView1.DataSource = listValue;
                    dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView1.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView1.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView1.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    
                }
            }

            else if (cb_ElementType.SelectedValue == "Instance")
            {
                if (cb_Element_From.SelectedValue == "Active View")
                {
                    listValue.Clear();
                    foreach (var element in collectElement)
                    {
                        var p = cb_SelectParam.SelectedValue.ToString();
                        var value = element.LookupParameter(p).AsString();

                        if (value != null)
                        {
                            if (cb_FindMethod.SelectedValue == "Equals" && textBox1.Text == value)
                            {
                                listValue.Rows.Add(rowNumber, element.Id, value,textBox2.Text);
                                cElemtoTable.Add(element.Id);
                                rowNumber++;
                            }
                            if (cb_FindMethod.SelectedValue == "Contains" && value.Contains(textBox1.Text))
                            {
                              
                                var rep = value.Replace(textBox1.Text, textBox2.Text);
                                listValue.Rows.Add(rowNumber,element.Id, value, rep);
                                cElemtoTable.Add(element.Id);
                                rowNumber++;
                            }
                            dataGridView1.DataSource = listValue;
                            dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                            dataGridView1.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                            dataGridView1.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                            dataGridView1.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                          
                        }
                    }
                }
                else if (cb_Element_From.SelectedValue == "Project")
                {
                    listValue.Clear();
              
                    foreach (var element in collectElement)
                    {
                        var p = cb_SelectParam.SelectedValue.ToString();
                        var value = element.LookupParameter(p).AsString();

                        if (cb_FindMethod.SelectedValue == "Equals" && textBox1.Text == value)
                        {
                            
                            listValue.Rows.Add(rowNumber, element.Id, value,textBox2.Text);
                            cElemtoTable.Add(element.Id);
                            rowNumber++;

                        }
                        if (value != null)
                        {
                            if (cb_FindMethod.SelectedValue == "Contains" && value.Contains(textBox1.Text))
                            {
                                
                                var rep = value.Replace(textBox1.Text, textBox2.Text);
                                listValue.Rows.Add(rowNumber, element.Id, value,rep);
                                cElemtoTable.Add(element.Id);
                                rowNumber++;
                            }
                        }
                        dataGridView1.DataSource = listValue;
                        dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                        dataGridView1.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                        dataGridView1.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                        dataGridView1.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    }
                }
            }
        }

        // Replace
        private void Replace_Click(object sender, EventArgs e)
        {
            var p = cb_SelectParam.SelectedValue.ToString();
            List<int> countList = new List<int>();

            using (Transaction transaction = new Transaction(Doc))
            {
                transaction.Start("Update Elements");
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    bool isChecked = false;
                  
                    DataGridViewCheckBoxCell checkBoxCell = row.Cells["Select"] as DataGridViewCheckBoxCell;
                    if (checkBoxCell != null && checkBoxCell.Value != null)
                    {
                        isChecked = (bool)checkBoxCell.Value;
                    }
                   
                    int elementID = Convert.ToInt32(row.Cells["Element Id"].Value);

                    if(isChecked == true)
                    {
                        if(elementID != null && elementID != 0)
                        {
                            var elemenT = Doc.GetElement(new ElementId(elementID));
                           
                            var value = elemenT.LookupParameter(p).AsString();
                            Parameter pV = elemenT.LookupParameter(p);
                            var rep = value.Replace(textBox1.Text, textBox2.Text);
                            var cSet = pV.Set(rep);
                            countList.Add(Convert.ToInt32(cSet));
                        }  
                    }
                }
                if (countList.Count != 0)
                {
                    TaskDialog.Show("Results", "Number of Replace Done : " + countList.Count);
                }
                else
                {
                    TaskDialog.Show("Results", "Replace  Not Done. ");
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
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

