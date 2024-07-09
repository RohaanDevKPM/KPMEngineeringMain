using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows;
using Autodesk.Revit.UI.Selection;
using System.Linq;

namespace KPMEngineeringB.R._20._TwentiethButton
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class TwentiethBtnCommand : IExternalCommand
    {
        public static IList<Element> collectedFloorEle = new List<Element>();
        public static IList<Element> collectedElements = new List<Element>();
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var joinedList = new List<Element>();
            var unjoinedList = new List<Element>();
            collectedFloorEle.Clear();
            var uidoc = commandData.Application.ActiveUIDocument;
            var doc = uidoc.Document;
            SupportDatA.btnName = "Join Geometry";
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
                    using (TransactionGroup transactionGroup = new TransactionGroup(doc))
                    {
                        using (Transaction transaction = new Transaction(doc))
                        {
                            transactionGroup.Start("Join Geometry");
                            transaction.Start("Join Geometry Elements");

                            foreach (var flooR in collectedFloorEle)
                            {

                                foreach (var elemenT in collectedElements)
                                {
                                    if (!(Autodesk.Revit.DB.JoinGeometryUtils.AreElementsJoined(doc, flooR, elemenT)))
                                    {
                                        try
                                        {
                                            Autodesk.Revit.DB.JoinGeometryUtils.JoinGeometry(doc, flooR, elemenT);
                                            setFailureDialogSuppressor(transaction);
                                            joinedList.Add(elemenT);
                                        }
                                        catch
                                        {

                                        }
                                    }

                                }

                            }
                            transaction.Commit();
                            transaction.Start("Solve Warning");
                            var collectWarning = doc.GetWarnings();
                            foreach (var warning in collectWarning)
                            {
                                var descriptioN = warning.GetDescriptionText();
                                if ("Highlighted elements are joined but do not intersect." == descriptioN)
                                {
                                    var getFailingEle = warning.GetFailingElements().ToList();
                                    Autodesk.Revit.DB.JoinGeometryUtils.UnjoinGeometry(doc, doc.GetElement(getFailingEle[0]), doc.GetElement(getFailingEle[1]));
                                    unjoinedList.Add(doc.GetElement(getFailingEle[0]));
                                }
                            }
                            transaction.Commit();
                            transactionGroup.Assimilate();
                            System.Windows.MessageBox.Show((joinedList.Count - unjoinedList.Count).ToString() + " Elements Joined Successfully.");
                            SupportDatA.checkData(commandData);
                            return Result.Succeeded;
                        }
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
        public static void CreateBtn20(RibbonPanel panel)
        {
            var assembly = Assembly.GetExecutingAssembly();
            RibbonItem pushBtn = panel.AddItem(new PushButtonData(
                MethodBase.GetCurrentMethod().DeclaringType?.Name,
                "Join" + Environment.NewLine + "Geometry",
                assembly.Location,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName
                )
            {
                ToolTip = "Easily Join Geometry to selected Floors.",
                LargeImage = Imaging.CreateBitmapSourceFromHBitmap(Resource1.icons8_merge_documents_32.GetHbitmap(), IntPtr.Zero,
                        Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()),
                Image = Imaging.CreateBitmapSourceFromHBitmap(Resource1.icons8_merge_documents_16.GetHbitmap(), IntPtr.Zero,
                        Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()),
            });
            ContextualHelp contexHelp = new ContextualHelp(ContextualHelpType.Url, "https://kpmdesignservices-my.sharepoint.com/:v:/g/personal/nimesh_j_kpm-engineering_com/ESVMiL2WVoFCpubGM8vEW_QBpctb-dToKSFszZBtCvvQXQ?nav=eyJyZWZlcnJhbEluZm8iOnsicmVmZXJyYWxBcHAiOiJPbmVEcml2ZUZvckJ1c2luZXNzIiwicmVmZXJyYWxBcHBQbGF0Zm9ybSI6IldlYiIsInJlZmVycmFsTW9kZSI6InZpZXciLCJyZWZlcnJhbFZpZXciOiJNeUZpbGVzTGlua0NvcHkifX0&e=UfiPtz");
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
            var eleReferences = uidoc.Selection.PickObjects(ObjectType.Element, selFilter, "Select Elements to Join");
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
        public class WarningSolver : IFailuresPreprocessor 
        {
            public FailureProcessingResult PreprocessFailures(FailuresAccessor a)
            {
                IList<FailureMessageAccessor> failures = a.GetFailureMessages();

                foreach (FailureMessageAccessor f in failures)
                {
                    FailureDefinitionId id = f.GetFailureDefinitionId();

                    FailureSeverity failureSeverity = a.GetSeverity();

                    if (failureSeverity == FailureSeverity.Warning)   //simply catch all warnings, so you don't have to find out what warning is causing the message to pop up
                    {
                        a.DeleteWarning(f);
                    }
                    else
                    {
                        return FailureProcessingResult.ProceedWithRollBack;
                    }
                }
                return FailureProcessingResult.Continue;
            }
        }
        public static void setFailureDialogSuppressor(Transaction transaction)
        {
            FailureHandlingOptions failOpt = transaction.GetFailureHandlingOptions();
            failOpt.SetFailuresPreprocessor(new WarningSolver());
            transaction.SetFailureHandlingOptions(failOpt);

        }
    }
}
