﻿using Autodesk.Revit.Attributes;
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
using System.Windows.Forms;
using System.Windows.Controls;

namespace KPMEngineeringB.R._18._EighteenthButton
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class EighteenthBtnCommand : IExternalCommand
    {
        public static IList<Element> collectedElementS = new List<Element>();
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            collectedElementS.Clear();
            var uidoc = commandData.Application.ActiveUIDocument;
            var doc = uidoc.Document;
            SupportDatA.btnName = "Disallow Join";
            if (SupportDatA.CheckAuthorize(commandData))
            {
                try
                {
                    collectedElementS = GetManyRefByRectangle(uidoc, doc);

                }
                catch { }
                if (collectedElementS.Count > 0)
                {
                    using (Transaction transaction = new Transaction(doc))
                    {
                        transaction.Start("Disallow Join");
                        try
                        {
                            foreach (var element in collectedElementS)
                            {
                                Autodesk.Revit.DB.Structure.StructuralFramingUtils.DisallowJoinAtEnd(element as FamilyInstance, 0);
                                Autodesk.Revit.DB.Structure.StructuralFramingUtils.DisallowJoinAtEnd(element as FamilyInstance, 1);
                            }
                            System.Windows.MessageBox.Show("Total number of " + collectedElementS.Count.ToString() +
                                " updated.");
                        }
                        catch (Exception ex)
                        {
                            System.Windows.MessageBox.Show(ex.Message);
                        }
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
        public static void CreateBtn18(RibbonPanel panel)
        {
            var assembly = Assembly.GetExecutingAssembly();
            RibbonItem pushBtn = panel.AddItem(new PushButtonData(
                MethodBase.GetCurrentMethod().DeclaringType?.Name,
                "Disallow" + Environment.NewLine + "Join",
                assembly.Location,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName
                )
            {
                ToolTip = "Easily Disallow Join at ends by Selecting Structural Framing Elements.",
                LargeImage = Imaging.CreateBitmapSourceFromHBitmap(Resource1.icons8_broken_chain_32.GetHbitmap(), IntPtr.Zero,
                        Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()),
                Image = Imaging.CreateBitmapSourceFromHBitmap(Resource1.icons8_broken_chain_16.GetHbitmap(), IntPtr.Zero,
                        Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()),
            });
            ContextualHelp contexHelp = new ContextualHelp(ContextualHelpType.Url, "https://kpmdesignservices-my.sharepoint.com/:v:/g/personal/nimesh_j_kpm-engineering_com/ERB82xNtde5LopmL0B9tgIIB1z6S6Dc1i0IBkvKYCSF1SQ?nav=eyJyZWZlcnJhbEluZm8iOnsicmVmZXJyYWxBcHAiOiJPbmVEcml2ZUZvckJ1c2luZXNzIiwicmVmZXJyYWxBcHBQbGF0Zm9ybSI6IldlYiIsInJlZmVycmFsTW9kZSI6InZpZXciLCJyZWZlcnJhbFZpZXciOiJNeUZpbGVzTGlua0NvcHkifX0&e=1gk7GK");
            pushBtn.SetContextualHelp(contexHelp);
        }
        public static IList<Element> GetManyRefByRectangle(UIDocument uidoc, Document doc)
        {
            ReferenceArray ra = new ReferenceArray();
            ISelectionFilter selFilter = new MySelectionFilter();
            var eleReferences = uidoc.Selection.PickObjects(ObjectType.Element, selFilter, "Select Structural Framing Elements");
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
                    if (element.Category.Name == "Structural Framing")
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