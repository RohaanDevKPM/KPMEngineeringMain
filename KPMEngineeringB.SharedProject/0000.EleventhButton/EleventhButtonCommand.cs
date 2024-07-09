using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using System;
using System.Reflection;
using System.Windows.Forms;

using KPMEngineeringB.R;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Interop;
using KPMEngineeringA.Revit;

namespace KPM.Revit.EleventhButton
{
    [Transaction(TransactionMode.Manual)]
    internal class EleventhButtonCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            SupportDatA.btnName = "Scope Box";

            if (SupportDatA.CheckInternet())
            {
                if (SupportDatA.CheckAuthorize(commandData))
                {
                    try
                    {
                        var uiapp = commandData.Application;
                        var doc = uiapp.ActiveUIDocument.Document;

                        using (var scopeBoxForm = new ScopeBox(uiapp))
                        {
                            if (scopeBoxForm.ShowDialog() == DialogResult.OK)
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
                    catch (Exception ex)
                    {
                        message = ex.Message;
                        return Result.Failed;
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

        public static void CreateButton(RibbonPanel panel)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var buttonData = new PushButtonData(
                MethodBase.GetCurrentMethod().DeclaringType?.Name,
                "Scope Box",
                assembly.Location,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName)
            {
                ToolTip = "Apply scope boxes by selecting the floor plans.",
                LongDescription = "Press F1 to open the video tutorial.",
                // LargeImage = ImageUtils.LoadImage(assembly, "32x32.Box.png")  LargeImage = Imaging.CreateBitmapSourceFromHBitmap(Resource1.Line_32x32.GetHbitmap(), IntPtr.Zero,
                LargeImage = Imaging.CreateBitmapSourceFromHBitmap(Resource1.Line_32x32.GetHbitmap(), IntPtr.Zero,
                             Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
            };

            var pushButton = panel.AddItem(buttonData) as PushButton;
            var videoTutorialUri = new Uri("https://www.youtube.com/watch?v=cIcFEKkh1uA");
            var contextHelp = new ContextualHelp(ContextualHelpType.Url, videoTutorialUri.ToString());

            pushButton.SetContextualHelp(contextHelp);
        }
    }
}
