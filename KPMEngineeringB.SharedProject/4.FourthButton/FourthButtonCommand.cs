﻿using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using KPMEngineeringB.R;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Linq;

namespace KPMEngineeringB.R.FourthButton
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    internal class FourthButtonCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;

            IList<Element> cadFiles = new FilteredElementCollector(doc, doc.ActiveView.Id)
                .OfClass(typeof(ImportInstance)).WhereElementIsNotElementType()
                .ToElements();
            SupportDatA.btnName = "CAD to Conduit";
                if (SupportDatA.CheckAuthorize(commandData))
                {
                    if (doc.ActiveView.ViewType == ViewType.FloorPlan)
                    {
                        if (cadFiles.Count > 0)
                        {
                            using (System.Windows.Forms.Form formS = new Form4(doc))
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
                            TaskDialog.Show("Error", "No CAD file found in Active View.");
                            return Result.Cancelled;
                        }
                    }
                    else
                    {
                        TaskDialog.Show("Error", "Active View is not a Floor Plan.\nPlease try again on 2D floor plan.");
                        return Result.Cancelled;
                    }
                }
                else
                {
                    return Result.Cancelled;
                }
        }
        public static void CreateBtn4(RibbonPanel panel)
        {
            var assembly = Assembly.GetExecutingAssembly();
            RibbonItem pushBtn = panel.AddItem(new PushButtonData(
                MethodBase.GetCurrentMethod().DeclaringType?.Name, "CAD to" + Environment.NewLine + "Conduit",
                assembly.Location,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName)
            {
                ToolTip = "Convert Link or Import CAD Lines & Polylines into Revit Conduits.",
                LargeImage = Imaging.CreateBitmapSourceFromHBitmap(Resource1.Conduit_32x32.GetHbitmap(), IntPtr.Zero,
                             Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()),
                Image = Imaging.CreateBitmapSourceFromHBitmap(Resource1.Conduit_16x16.GetHbitmap(), IntPtr.Zero,
                        Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()),
                LongDescription = "This will convert all CAD lines & polylines into Conduits.",

            });
            ContextualHelp contexHelp = new ContextualHelp(ContextualHelpType.Url, "https://kpmdesignservices-my.sharepoint.com/:v:/g/personal/nimesh_j_kpm-engineering_com/ETwtmo_Q5GtAi9aY1TC6dpoBupblKWewR5mSNfYWTKSabQ?nav=eyJyZWZlcnJhbEluZm8iOnsicmVmZXJyYWxBcHAiOiJPbmVEcml2ZUZvckJ1c2luZXNzIiwicmVmZXJyYWxBcHBQbGF0Zm9ybSI6IldlYiIsInJlZmVycmFsTW9kZSI6InZpZXciLCJyZWZlcnJhbFZpZXciOiJNeUZpbGVzTGlua0NvcHkifX0&e=4dnZ0i");
            pushBtn.SetContextualHelp(contexHelp);
        }
    }
}