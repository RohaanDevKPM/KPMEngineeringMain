using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace KPMEngineeringB.R
{
    public partial class Form3 : System.Windows.Forms.Form
    {
        private IList<Line> lineList = new List<Line>();

        private IList<PolyLine> polyList = new List<PolyLine>();
        private static double middleEle { get; set; }
        private static double ctWidth {  get; set; }
        private static double ctHeight { get; set; }
        private static CableTrayType eleCTType {  get; set;}

        private static XYZ CADOrigin { get; set; }

        Autodesk.Revit.DB.Document Doc;
        public Form3(Autodesk.Revit.DB.Document doc)
        {
            InitializeComponent();
            Doc = doc;
        }

        private void Form3_Load(object sender, EventArgs e)
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

            //Cable Tray Type
            IList<Element> CTType = new FilteredElementCollector(Doc)
                .OfCategory(BuiltInCategory.OST_CableTray).WhereElementIsElementType()
                .ToElements();
            try
            {
                IList<string> CTTypeName = new List<string>();
                foreach(CableTrayType ct in CTType)
                {
                    CTTypeName.Add((ct.get_Parameter(BuiltInParameter.SYMBOL_FAMILY_NAME_PARAM).AsString())
                        + " - " + (ct.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString()));
                }
                cb_CableTray_Type.DataSource = CTTypeName;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.ToString());
            }
            cb_CableTray_Type.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btn_OK_Click(object sender, EventArgs e)
        {
            double defaultEle;
            double.TryParse(tb_MiddleEle.Text, out defaultEle);
            middleEle = defaultEle/304.8;

            double defaultWidth;
            double.TryParse(tb_Width.Text, out defaultWidth);
            double tempWidth = defaultWidth;
            if (tempWidth == 0)
            {
                ctWidth = 200/304.8;
            }
            else
            {
                ctWidth = defaultWidth / 304.8;
            }
            double defaultHeight;
            double.TryParse(tb_Height.Text, out defaultHeight);
            double tempHeight = defaultHeight;
            if (tempHeight == 0)
            {
                ctHeight = 200 / 304.8;
            }
            else
            {
                ctHeight = defaultHeight / 304.8;
            }

            Level activeLevel = Doc.ActiveView.GenLevel;
            IList<ElementId> finalList = new List<ElementId>();
            string selectedLayer = cb_CAD_Layer.SelectedItem.ToString();
            string selectedCTType = cb_CableTray_Type.SelectedItem.ToString();

            IList<Element> CTType = new FilteredElementCollector(Doc)
                .OfCategory(BuiltInCategory.OST_CableTray).WhereElementIsElementType()
                .ToElements();
            foreach (CableTrayType ct in CTType)
            {
                if ((ct.get_Parameter(BuiltInParameter.SYMBOL_FAMILY_NAME_PARAM).AsString())
                    + " - " + (ct.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString()) == selectedCTType)
                {
                    eleCTType = ct;
                    break;
                }
            }


            Transaction t = new Transaction(Doc, "CADtoRvtCableTray");
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
                        var createCableTray = Autodesk.Revit.DB.Electrical.CableTray.Create(Doc,
                            eleCTType.Id, startPoint, endPoint, activeLevel.Id);
                        var CTwidth = createCableTray.get_Parameter(BuiltInParameter.RBS_CABLETRAY_WIDTH_PARAM).Set(ctWidth);
                        var CTheight = createCableTray.get_Parameter(BuiltInParameter.RBS_CABLETRAY_HEIGHT_PARAM).Set(ctHeight);
                        var mEle = createCableTray.get_Parameter(BuiltInParameter.RBS_OFFSET_PARAM).Set(middleEle);
                        finalList.Add(createCableTray.Id);
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
                            var createCableTray = Autodesk.Revit.DB.Electrical.CableTray.Create(Doc,eleCTType.Id,
                                Pts[i] + CADOrigin, Pts[i + 1] + CADOrigin, activeLevel.Id);
                            var CTwidth = createCableTray.get_Parameter(BuiltInParameter.RBS_CABLETRAY_WIDTH_PARAM).Set(ctWidth);
                            var CTheight = createCableTray.get_Parameter(BuiltInParameter.RBS_CABLETRAY_HEIGHT_PARAM).Set(ctHeight);
                            var mEle = createCableTray.get_Parameter(BuiltInParameter.RBS_OFFSET_PARAM).Set(middleEle);
                            finalList.Add(createCableTray.Id);
                        }
                        catch (Exception ex)
                        {
                            TaskDialog.Show("Error", ex.Message.ToString());
                        }
                    }
                }
            }

            t.Commit();
            TaskDialog.Show("Output", "Success!\n" + finalList.Count.ToString()
                + " number of Cable Trays Created."); 
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

        private void cb_CableTray_Type_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedCTType = cb_CableTray_Type.SelectedItem.ToString();
            System.Collections.Generic.HashSet<double> sizes = new HashSet<double>();
            FilteredElementCollector collectorCTType = new FilteredElementCollector(Doc)
                .OfClass(typeof(CableTrayType));
            IEnumerable<CableTrayType> CTTypes = collectorCTType.ToElements().Cast<CableTrayType>();
            CableTrayType CTTypeInstance = null;
            foreach (CableTrayType cT in CTTypes)
            {
                if ((cT.get_Parameter(BuiltInParameter.SYMBOL_FAMILY_NAME_PARAM).AsString())
                    + " - " + (cT.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString()) == selectedCTType)
                {
                    CTTypeInstance = cT;
                    break;
                }
            }
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
