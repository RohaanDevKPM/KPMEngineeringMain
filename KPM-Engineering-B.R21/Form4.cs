using Autodesk.Revit.Creation;
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
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Plumbing;
using System.Net;
using Autodesk.Revit.DB.Electrical;
using System.Xml.Linq;

namespace KPMEngineeringB.R
{
    public partial class Form4 : System.Windows.Forms.Form
    {
        private IList<Line> lineList = new List<Line>();

        private IList<PolyLine> polyList = new List<PolyLine>();
        private static double middleEle { get; set; }

        private static double conduitSize {  get; set; }
        private static ConduitType eleConduitType {  get; set;}
        private static XYZ CADOrigin { get; set; }

        Autodesk.Revit.DB.Document Doc;
        public Form4(Autodesk.Revit.DB.Document doc)
        {
            InitializeComponent();
            Doc = doc;
        }

        private void Form4_Load(object sender, EventArgs e)
        {
            // CAD Files
            IList<Element> cadFiles = new FilteredElementCollector(Doc, Doc.ActiveView.Id)
                .OfClass(typeof(ImportInstance)).WhereElementIsNotElementType()
                .ToElements();
            try
            {
                IList<string> cadFileList = new List<string>();
                foreach (ImportInstance cadFile in cadFiles)
                {
                    cadFileList.Add(cadFile.get_Parameter(BuiltInParameter.IMPORT_SYMBOL_NAME).AsString());
                }
                cb_CAD_Name.DataSource = cadFileList;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.ToString());
            }
            cb_CAD_Name.SelectedIndex = 0;

            //Conduit Type
            IList<Element> conduitType = new FilteredElementCollector(Doc)
                .OfCategory(BuiltInCategory.OST_Conduit).WhereElementIsElementType()
                .ToElements();
            try
            {
                IList<string> conduitTypeName = new List<string>();
                foreach(ConduitType pt in conduitType)
                {
                    conduitTypeName.Add((pt.get_Parameter(BuiltInParameter.SYMBOL_FAMILY_NAME_PARAM).AsString())
                        + " - " + (pt.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString()));
                }
                cb_Conduit_Type.DataSource = conduitTypeName;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.ToString());
            }
            cb_Conduit_Type.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btn_OK_Click(object sender, EventArgs e)
        {
            double defalutEle;
            double.TryParse(tb_MiddleEle.Text, out defalutEle);
            middleEle = defalutEle/304.8;

            Level activeLevel = Doc.ActiveView.GenLevel;
            IList<ElementId> finalList = new List<ElementId>();
            IList<ElementId> secondList = new List<ElementId>();
            string selectedLayer = cb_CAD_Layer.SelectedItem.ToString();
            string selectedConduitType = cb_Conduit_Type.SelectedItem.ToString();
         
            IList<Element> conduitType = new FilteredElementCollector(Doc)
                .OfCategory(BuiltInCategory.OST_Conduit).WhereElementIsElementType()
                .ToElements();
            foreach (ConduitType cnt in conduitType)
            {
                if ((cnt.get_Parameter(BuiltInParameter.SYMBOL_FAMILY_NAME_PARAM).AsString())
                    + " - " + (cnt.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString()) == selectedConduitType)
                {
                    eleConduitType = cnt;
                    break;
                }
            }

            Transaction t = new Transaction(Doc, "CADtoRvtConduit");
            t.Start();
            foreach (Line selectedline in lineList)
            {
                ElementId graphicId = selectedline.GraphicsStyleId;
                GraphicsStyle graphicInstance = (Doc.GetElement(graphicId)) as GraphicsStyle;
                Category categorY = graphicInstance.GraphicsStyleCategory;
                string categoryName = categorY.Name;
                if (selectedLayer == categoryName)
                {
                    var bothPts = selectedline.Tessellate();
                    var startPoint = bothPts[0] + CADOrigin;
                    var endPoint = bothPts[1] + CADOrigin;

                    try
                    {
                        var createConduits = Autodesk.Revit.DB.Electrical.Conduit.Create(Doc, eleConduitType.Id, startPoint, endPoint, activeLevel.Id);
                        var pSize = createConduits.get_Parameter(BuiltInParameter.RBS_CONDUIT_DIAMETER_PARAM).Set(conduitSize);
                        var mEle = createConduits.get_Parameter(BuiltInParameter.RBS_OFFSET_PARAM).Set(middleEle);
                        finalList.Add(createConduits.Id);
                    }
                    catch (Exception ex)
                    {
                        TaskDialog.Show("Error", ex.Message.ToString());
                    }
                    
                }
            }
            foreach (PolyLine selectedPoly in polyList)
            {
                ElementId graphicId = selectedPoly.GraphicsStyleId;
                GraphicsStyle graphicInstance = (Doc.GetElement(graphicId)) as GraphicsStyle;
                Category categorY = graphicInstance.GraphicsStyleCategory;
                string categoryName = categorY.Name;
                if (selectedLayer == categoryName)
                {
                    var Pts = selectedPoly.GetCoordinates();
                    for (int i = 0; i < Pts.Count - 1; i++)
                    {
                        try
                        {
                            var createConduits = Autodesk.Revit.DB.Electrical.Conduit.Create(Doc,
                                eleConduitType.Id, Pts[i] + CADOrigin, Pts[i + 1] + CADOrigin, activeLevel.Id);
                            var pSize = createConduits.get_Parameter(BuiltInParameter.RBS_CONDUIT_DIAMETER_PARAM).Set(conduitSize);
                            var mEle = createConduits.get_Parameter(BuiltInParameter.RBS_OFFSET_PARAM).Set(middleEle);
                            finalList.Add(createConduits.Id);
                        }
                        catch (Exception ex)
                        {
                            TaskDialog.Show("Error", ex.Message.ToString());
                        }
                    }
                }
            }
            t.Commit();
            TaskDialog.Show("Output", "Sucess!\n" + finalList.Count.ToString()
                + " number of Conduits Created.");
            
            
        }

        private void cb_CAD_Name_SelectedIndexChanged(object sender, EventArgs e)
        {
            IList<Element> cadFiles = new FilteredElementCollector(Doc, Doc.ActiveView.Id)
                .OfClass(typeof(ImportInstance)).WhereElementIsNotElementType()
                .ToElements();
            string selectedCadFile = cb_CAD_Name.SelectedItem.ToString();
            ImportInstance selectedCadInstance = null;
            foreach (ImportInstance cadF in cadFiles)
            {
                if (selectedCadFile == cadF.get_Parameter(BuiltInParameter.IMPORT_SYMBOL_NAME).AsString())
                {
                    selectedCadInstance = cadF;
                    break;
                }
            }


            IList<GeometryElement> geoList = new List<GeometryElement>();

            IList<string> layerfromCAD = new List<string>();

            Options op = Doc.Application.Create.NewGeometryOptions();
            op.ComputeReferences = true;
            op.IncludeNonVisibleObjects = true;

            GeometryElement geometrY = selectedCadInstance.get_Geometry(op);
            if (geometrY != null)
            {
                foreach (GeometryObject geoObj in geometrY)
                {
                    GeometryInstance geoInstance = geoObj as GeometryInstance;
                    if (geoInstance != null)
                    {
                        CADOrigin = geoInstance.Transform.Origin;
                        GeometryElement geoEle = geoInstance.GetSymbolGeometry() as GeometryElement;
                        if (geoEle != null)
                        {
                            foreach (GeometryObject geoObj1 in geoEle)
                            {
                                if (geoObj1 is Line)
                                {
                                    lineList.Add(geoObj1 as Line);
                                    Line lineData = geoObj1 as Line;
                                    ElementId graphicId = lineData.GraphicsStyleId;
                                    GraphicsStyle graphicInstance = (Doc.GetElement(graphicId)) as GraphicsStyle;
                                    Category categorY = graphicInstance.GraphicsStyleCategory;
                                    string categoryName = categorY.Name;
                                    layerfromCAD.Add(categoryName);

                                }
                                else if (geoObj1 is PolyLine)
                                {
                                    polyList.Add(geoObj1 as PolyLine);
                                    PolyLine polyData = geoObj1 as PolyLine;
                                    ElementId graphicId = polyData.GraphicsStyleId;
                                    GraphicsStyle graphicInstance = (Doc.GetElement(graphicId)) as GraphicsStyle;
                                    Category categorY = graphicInstance.GraphicsStyleCategory;
                                    string categoryName = categorY.Name;
                                    layerfromCAD.Add(categoryName);
                                }
                            }
                        }
                    }
                }
            }


            IList<string> uniq = new HashSet<string>(layerfromCAD).ToList();
            cb_CAD_Layer.DataSource = uniq;
            cb_CAD_Layer.SelectedIndex = 0;

        }

        private void cb_Conduit_Type_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedConduitType = cb_Conduit_Type.SelectedItem.ToString();
            System.Collections.Generic.HashSet<double> sizes = new HashSet<double>();
            FilteredElementCollector collectorConduitType = new FilteredElementCollector(Doc)
                .OfClass(typeof(ConduitType));
            IEnumerable<ConduitType> conduitTypes = collectorConduitType.ToElements().Cast<ConduitType>();
            ConduitType ConduitTypeInstance = null;
            foreach (ConduitType cnt in conduitTypes)
            {
                if ((cnt.get_Parameter(BuiltInParameter.SYMBOL_FAMILY_NAME_PARAM).AsString())
                    + " - " + (cnt.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString()) == selectedConduitType)
                {
                    ConduitTypeInstance = cnt;
                    break;
                }
            }

            RoutingPreferenceManager rpm = ConduitTypeInstance.RoutingPreferenceManager;

            int segmentCount = rpm.GetNumberOfRules(RoutingPreferenceRuleGroupType.Segments);
            for (int index = 0; index != segmentCount; ++index)
            {
                RoutingPreferenceRule segmentRule = rpm.GetRule(RoutingPreferenceRuleGroupType.Segments, index);
                Segment segment = Doc.GetElement(segmentRule.MEPPartId) as Segment;
                foreach (MEPSize size in segment.GetSizes())
                {
                    sizes.Add(Math.Round((size.NominalDiameter) * 304.8));
                }
            }
            List<double> sizesSorted = sizes.ToList();
            sizesSorted.Sort();
            cb_ConduitSize.DataSource = sizesSorted;
            cb_ConduitSize.SelectedIndex = 0;
        }

        private void tb_MiddleEle_TextChanged(object sender, EventArgs e)
        {

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel1.LinkVisited = true;
            System.Diagnostics.Process.Start("https://kpm-engineering.com/");
        }

        private void bt_Finish_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            Close();
        }
    }
}
