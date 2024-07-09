using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows;
using MySql.Data.MySqlClient;
using System.Net.NetworkInformation;
using System.Net;
using System.Linq;
using Autodesk.Revit.UI.Selection;
using System.Transactions;
using MySqlX.XDevAPI;
using System.Windows.Controls;

namespace KPMEngineeringB.R._7._SeventhButton
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    internal class SeventhButtonCommand : IExternalCommand
    {
        public static IList<Element> collectedElementS = new List<Element>();

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            collectedElementS.Clear();
            var uidoc = commandData.Application.ActiveUIDocument;
            var doc = uidoc.Document;
            SupportDatA.btnName = "Column to Wall";
            if (SupportDatA.CheckAuthorize(commandData))
            {
                try
                {
                    collectedElementS = GetManyRefByRectangle(uidoc, doc);
                }
                catch { }
                if (collectedElementS.Count > 0)
                {
                    using (System.Windows.Forms.Form formS = new Form7(doc, commandData, collectedElementS))
                    {
                        if (formS.ShowDialog() == DialogResult.OK)  
                        {
                            SupportDatA.checkData(commandData);
                            return Result.Succeeded;
                        }
                        else 
                        {
                            return Result.Cancelled;
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
        public static void CreateBtn7(RibbonPanel panel)
        {
            var assembly = Assembly.GetExecutingAssembly();
            RibbonItem pushBtn = panel.AddItem(new PushButtonData(
                MethodBase.GetCurrentMethod().DeclaringType?.Name, "Column to" + Environment.NewLine + "Wall",
                assembly.Location,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName)
            {
                ToolTip = "Convert Selected Structural Columns into Walls.",
                LargeImage = Imaging.CreateBitmapSourceFromHBitmap(Resource1.Column_to_Wall_32x32.GetHbitmap(), IntPtr.Zero,
                             Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()),
                Image = Imaging.CreateBitmapSourceFromHBitmap(Resource1.Column_to_Wall_16x16.GetHbitmap(), IntPtr.Zero,
                        Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()),

            });
            ContextualHelp contexHelp = new ContextualHelp(ContextualHelpType.Url, "https://kpmdesignservices-my.sharepoint.com/:v:/g/personal/nimesh_j_kpm-engineering_com/ER4T-jqcEIpPvQG9Sokxd1UBQgZ_zz3jbnOUSkjH5kqfwg?nav=eyJyZWZlcnJhbEluZm8iOnsicmVmZXJyYWxBcHAiOiJPbmVEcml2ZUZvckJ1c2luZXNzIiwicmVmZXJyYWxBcHBQbGF0Zm9ybSI6IldlYiIsInJlZmVycmFsTW9kZSI6InZpZXciLCJyZWZlcnJhbFZpZXciOiJNeUZpbGVzTGlua0NvcHkifX0&e=rdmQNQ");
            pushBtn.SetContextualHelp(contexHelp);
        }

        public static IList<Element> GetManyRefByRectangle(UIDocument uidoc, Document doc)
        {
            ReferenceArray ra = new ReferenceArray();
            ISelectionFilter selFilter = new MySelectionFilter();
            var eleReferences = uidoc.Selection.PickObjects(ObjectType.Element, selFilter, "Select Structural Columns");
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
                    if (element.Category.Name == "Structural Columns")
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

            public bool AllowReference(Reference refer, XYZ point)
            {
                return false;
            }
        }

    }
}
