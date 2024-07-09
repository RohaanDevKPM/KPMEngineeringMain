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

namespace CADtoRvt.R
{
    public partial class Form1 : System.Windows.Forms.Form
    {
        private IList<Line> lineList = new List<Line>();

        private IList<PolyLine> polyList = new List<PolyLine>();
        private static double middleEle { get; set; }

        private static double pipeSize {  get; set; }

        private static Element elePipeSys { get; set;}
        private static PipeType elePipeType {  get; set;}

        private static XYZ CADOrigin { get; set; }

        Autodesk.Revit.DB.Document Doc;
        public Form1(Autodesk.Revit.DB.Document doc)
        {
            InitializeComponent();
            Doc = doc;
        }

        private void Form1_Load(object sender, EventArgs e)
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

            //Pipe Type
            IList<Element> pipeType = new FilteredElementCollector(Doc)
                .OfCategory(BuiltInCategory.OST_PipeCurves).WhereElementIsElementType()
                .ToElements();
            try
            {
                IList<string> pipeTypeName = new List<string>();
                foreach(PipeType pt in pipeType)
                {
                    pipeTypeName.Add((pt.get_Parameter(BuiltInParameter.SYMBOL_FAMILY_NAME_PARAM).AsString())
                        + " - " + (pt.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString()));
                }
                cb_Pipe_Type.DataSource = pipeTypeName;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.ToString());
            }
            cb_Pipe_Type.SelectedIndex = 0;

            //Pipe System Type
            IList<Element> pipeSysType = new FilteredElementCollector(Doc)
                .OfCategory(BuiltInCategory.OST_PipingSystem).WhereElementIsElementType()
                .ToElements();
            try
            {
                IList<string> pipeSysTypeName = new List<string>();
                foreach(Element pst in pipeSysType)
                {
                    pipeSysTypeName.Add(pst.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString());
                }
                cb_Pipe_System.DataSource = pipeSysTypeName;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.ToString());
            }
            cb_Pipe_System.SelectedIndex = 0;

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

            double defaluteSize;
            double.TryParse(cb_PipeSize.Text, out defaluteSize);
            double tempSize = defaluteSize;
            if (tempSize == 0)
            {
                pipeSize = 50/304.8;
            }
            else
            {
                pipeSize = defaluteSize / 304.8;
            }
            Level activeLevel = Doc.ActiveView.GenLevel;
            IList<ElementId> finalList = new List<ElementId>();
            IList<ElementId> secondList = new List<ElementId>();
            string selectedLayer = cb_CAD_Layer.SelectedItem.ToString();
            string selectedPipeType = cb_Pipe_Type.SelectedItem.ToString();
            string selectedPipeSys = cb_Pipe_System.SelectedItem.ToString();

            IList<Element> pipeType = new FilteredElementCollector(Doc)
                .OfCategory(BuiltInCategory.OST_PipeCurves).WhereElementIsElementType()
                .ToElements();
            foreach (PipeType pt in pipeType)
            {
                if ((pt.get_Parameter(BuiltInParameter.SYMBOL_FAMILY_NAME_PARAM).AsString())
                    + " - " + (pt.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString()) == selectedPipeType)
                {
                    elePipeType = pt;
                    break;
                }
            }

            IList<Element> pipeSysType = new FilteredElementCollector(Doc)
                .OfCategory(BuiltInCategory.OST_PipingSystem).WhereElementIsElementType()
                .ToElements();
            foreach (Element pst in pipeSysType)
            {
                if ((pst.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString()) == selectedPipeSys)
                {
                    elePipeSys = pst;
                    break;
                }
            }

            Transaction t = new Transaction(Doc, "CADtoRvtPipe");
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
                        var createPlaceholder = Autodesk.Revit.DB.Plumbing.Pipe.CreatePlaceholder(Doc, elePipeSys.Id,
                            elePipeType.Id, activeLevel.Id, startPoint, endPoint);
                        var pSize = createPlaceholder.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).Set(pipeSize);
                        var mEle = createPlaceholder.get_Parameter(BuiltInParameter.RBS_OFFSET_PARAM).Set(middleEle);
                        finalList.Add(createPlaceholder.Id);
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
                            var createPlaceholder = Autodesk.Revit.DB.Plumbing.Pipe.CreatePlaceholder(Doc, elePipeSys.Id,
                                elePipeType.Id, activeLevel.Id, Pts[i] + CADOrigin, Pts[i + 1] + CADOrigin);
                            var pSize = createPlaceholder.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).Set(pipeSize);
                            var mEle = createPlaceholder.get_Parameter(BuiltInParameter.RBS_OFFSET_PARAM).Set(middleEle);
                            finalList.Add(createPlaceholder.Id);
                        }
                        catch (Exception ex)
                        {
                            TaskDialog.Show("Error", ex.Message.ToString());
                        }
                    }
                }
            }
            
            try
            {
                var ConvertPH = Autodesk.Revit.DB.Plumbing.PlumbingUtils.ConvertPipePlaceholders(Doc, finalList);
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.Message.ToString());
            }
            Doc.Regenerate();
            t.Commit();
            
            Transaction t1 = new Transaction(Doc, "CADtoRvtPipe");
            t.Start();
            foreach(ElementId ph in finalList)
            {
                if (Doc.GetElement(ph) != null)
                {
                    secondList.Add(ph);
                }
            }
            if (secondList.Count > 0)
                try
                {
                    var ConvertPH = Autodesk.Revit.DB.Plumbing.PlumbingUtils.ConvertPipePlaceholders(Doc, secondList);
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Error", ex.Message.ToString());
                }
            t.Commit() ;
            
            TaskDialog.Show("Output", "Sucess!\n" + finalList.Count.ToString()
                + " number of Pipes Created.");
            
            
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

        private void cb_Pipe_Type_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedPipeType = cb_Pipe_Type.SelectedItem.ToString();
            System.Collections.Generic.HashSet<double> sizes = new HashSet<double>();
            FilteredElementCollector collectorPipeType = new FilteredElementCollector(Doc)
                .OfClass(typeof(PipeType));
            IEnumerable<PipeType> pipeTypes = collectorPipeType.ToElements().Cast<PipeType>();
            PipeType PipeTypeInstance = null;
            foreach (PipeType pT in pipeTypes)
            {
                if ((pT.get_Parameter(BuiltInParameter.SYMBOL_FAMILY_NAME_PARAM).AsString())
                    + " - " + (pT.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString()) == selectedPipeType)
                {
                    PipeTypeInstance = pT;
                    break;
                }
            }

            RoutingPreferenceManager rpm = PipeTypeInstance.RoutingPreferenceManager;

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
            cb_PipeSize.DataSource = sizesSorted;
            cb_PipeSize.SelectedIndex = 0;
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
