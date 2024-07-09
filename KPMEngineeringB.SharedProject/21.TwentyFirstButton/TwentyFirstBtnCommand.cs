using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows;

namespace KPMEngineeringB.R._21.TwentyFirstButton
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    internal class TwentyFirstBtnCommand : IExternalCommand
    {
        public static IList<Element> collectedFloorEle = new List<Element>();
        public static IList<Element> collectedElements = new List<Element>();
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var unjoinedList = new List<Element>();
            collectedFloorEle.Clear();
            var uidoc = commandData.Application.ActiveUIDocument;
            var doc = uidoc.Document;
            SupportDatA.btnName = "Unjoin Geometry";
            if (SupportDatA.CheckAuthorize(commandData))
            {
                
                try
                {
                    collectedFloorEle = GetFloorEleByRectangle(uidoc, doc);
                    collectedElements = GetElesByRectangle(uidoc, doc);
                }
                catch
                {

                }
                if (collectedFloorEle.Count > 0)
                {
                    using (Transaction transaction = new Transaction(doc))
                    {
                        transaction.Start("Unjoin Geometry Elements");

                        foreach (var flooR in collectedFloorEle)
                        {

                            foreach (var elemenT in collectedElements)
                            {
                                if ((Autodesk.Revit.DB.JoinGeometryUtils.AreElementsJoined(doc, flooR, elemenT)))
                                {
                                    try
                                    {
                                        Autodesk.Revit.DB.JoinGeometryUtils.UnjoinGeometry(doc, flooR, elemenT);
                                        unjoinedList.Add(elemenT);
                                    }
                                    catch
                                    {

                                    }
                                }

                            }

                        }
                        System.Windows.MessageBox.Show(unjoinedList.Count.ToString() + " Elements Unjoin Successfully.");
                        transaction.Commit();
                        SupportDatA.checkData(commandData);
                        return Result.Succeeded;
                    }
                }
                else
                {
                    return Result.Cancelled;
                }
            }
            else
            {
                return Result.Cancelled;
            }
        }
        public static void CreateBtn21(RibbonPanel panel)
        {
            var assembly = Assembly.GetExecutingAssembly();
            RibbonItem pushBtn = panel.AddItem(new PushButtonData(
                MethodBase.GetCurrentMethod().DeclaringType?.Name,
                "Unjoin" + Environment.NewLine + "Geometry",
                assembly.Location,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName
                )
            {
                ToolTip = "Easily Unjoin Geometry to selected Floors.",
                LargeImage = Imaging.CreateBitmapSourceFromHBitmap(Resource1.icons8_separate_document_32.GetHbitmap(), IntPtr.Zero,
                        Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()),
                Image = Imaging.CreateBitmapSourceFromHBitmap(Resource1.icons8_separate_document_16.GetHbitmap(), IntPtr.Zero,
                        Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()),
            });
            ContextualHelp contexHelp = new ContextualHelp(ContextualHelpType.Url, "https://kpmdesignservices-my.sharepoint.com/:v:/g/personal/nimesh_j_kpm-engineering_com/EQW48M84huZDq6TCQrDvtnwBsJBET-ey29QhaRF-H72PIg?nav=eyJyZWZlcnJhbEluZm8iOnsicmVmZXJyYWxBcHAiOiJPbmVEcml2ZUZvckJ1c2luZXNzIiwicmVmZXJyYWxBcHBQbGF0Zm9ybSI6IldlYiIsInJlZmVycmFsTW9kZSI6InZpZXciLCJyZWZlcnJhbFZpZXciOiJNeUZpbGVzTGlua0NvcHkifX0&e=cN3Cuc");
            pushBtn.SetContextualHelp(contexHelp);
        }
        public static IList<Element> GetFloorEleByRectangle(UIDocument uidoc, Document doc)
        {
            ReferenceArray ra = new ReferenceArray();
            ISelectionFilter selFilter = new MySelectionFilter();
            var eleReferences = uidoc.Selection.PickObjects(ObjectType.Element, selFilter, "Select Floors");
            ///IList <Element> eList = doc.Selection.PickElementsByRectangle(selFilter,
            ///   "Select Columns") as IList<Element>;
            IList<Element> eList = new List<Element>();
            foreach (var reF in eleReferences)
            {
                var element = doc.GetElement(reF.ElementId);
                eList.Add(element);
            }
            return eList;
        }
        public class MySelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element element)
            {
                if (element != null && element.Category != null)
                {
                    if (element.Category.Name == "Floors")
                    {
                        return true;
                    }
                    return false;
                }
                else
                {
                    return false;
                }
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                throw new NotImplementedException();
            }
        }

        public static IList<Element> GetElesByRectangle(UIDocument uidoc, Document doc)
        {
            ReferenceArray ra = new ReferenceArray();
            ISelectionFilter selFilter = new EleSelectionFilter();
            var eleReferences = uidoc.Selection.PickObjects(ObjectType.Element, selFilter, "Select Elements to Unjoin");
            ///IList <Element> eList = doc.Selection.PickElementsByRectangle(selFilter,
            ///   "Select Columns") as IList<Element>;
            IList<Element> eList = new List<Element>();
            foreach (var reF in eleReferences)
            {
                var element = doc.GetElement(reF.ElementId);
                eList.Add(element);
            }
            return eList;
        }
        public class EleSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element element)
            {
                if (element != null && element.Category != null)
                {
                    if (element.Category.Name == "Structural Columns" || element.Category.Name == "Columns"
                        || element.Category.Name == "Walls" || element.Category.Name == "Structural Framing")
                    {
                        return true;
                    }
                    return false;
                }
                else
                {
                    return false;
                }
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                throw new NotImplementedException();
            }
        }
    }
}

