﻿using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using KPMEngineeringB.R;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Runtime.CompilerServices;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

namespace CADtoRvtPipe.R._5._FifthButton
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class FifthButtonCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;

            IList<Element> cadFiles = new FilteredElementCollector(doc, doc.ActiveView.Id)
                .OfClass(typeof(ImportInstance)).WhereElementIsNotElementType()
                .ToElements();
            var currentTime = DateTime.Now;
            DateTime.TryParse(setDate, out DateTime setTime);
            int compareResult = DateTime.Compare(setTime, currentTime);
            if (compareResult != -1)
            {
                if (doc.ActiveView.ViewType != ViewType.ThreeD)
                {
                    if (cadFiles.Count > 0)
                    {
                        using (System.Windows.Forms.Form formS = new Form5(doc))
                        {
                            if (formS.ShowDialog() == DialogResult.OK)
                            {
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
                        TaskDialog.Show("Error", "No CAD file found in Active View.");
                        return Result.Cancelled;
                    }
                }
                else
                {
                    TaskDialog.Show("Error", "Active View is 3D, Please try again on 2D plan.");
                    return Result.Cancelled;
                }
            }
            else
            {
                TaskDialog.Show("Trial Period", "Your trial period is expired, please contact with KPM-Engineering Team.");
                return Result.Cancelled;
            }
        }
        public static void CreateBtn5(RibbonPanel panel)
        {
            var assembly = Assembly.GetExecutingAssembly();
            RibbonItem pushBtn = panel.AddItem(new PushButtonData(
                MethodBase.GetCurrentMethod().DeclaringType?.Name, "CAD to" + Environment.NewLine + "Lines",
                assembly.Location,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName)
            {
                ToolTip = "Convert Link or Import CAD Lines & Polylines into Revit Conduits.",
                LargeImage = Imaging.CreateBitmapSourceFromHBitmap(Resource1.Line_32x32.GetHbitmap(), IntPtr.Zero,
                             Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()),
                Image = Imaging.CreateBitmapSourceFromHBitmap(Resource1.Line_16x16.GetHbitmap(), IntPtr.Zero,
                        Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()),
                LongDescription = "This will convert all CAD lines & polylines into Conduits.",

            });
            ContextualHelp contexHelp = new ContextualHelp(ContextualHelpType.Url, "https://drive.google.com/file/d/1GdDRn1AdVstruocKZwriuZKAurO-Mgxx/view?usp=sharing");
            pushBtn.SetContextualHelp(contexHelp);
        }
        public string setDate = "2023-12-31";
    }
}