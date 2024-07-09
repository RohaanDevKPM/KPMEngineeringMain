using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows;
using Microsoft.Office.Interop.Outlook;

namespace KPMEngineeringB.R._99._ContactUsButton
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    internal class ContactUsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            SupportDatA.btnName = "Contact Us";
            if (SupportDatA.CheckAuthorize(commandData))
            {
                string emailAddress = "bim_automation@kpm-engineering.com"; 
                string subject = "KPM Plug-in Feedback or Request";
                string body = "Dear Automation Team, We have some feedback or requirement for a plug-in...";
                string mailtoLink = $"mailto:{emailAddress}?subject={Uri.EscapeDataString(subject)}&body={Uri.EscapeDataString(body)}";
                    
                try
                {
                    Microsoft.Office.Interop.Outlook.Application outlookApp = new Microsoft.Office.Interop.Outlook.Application();
                    Microsoft.Office.Interop.Outlook.MailItem mailItem = (Microsoft.Office.Interop.Outlook.MailItem)outlookApp.CreateItem(Microsoft.Office.Interop.Outlook.OlItemType.olMailItem);

                    mailItem.To = emailAddress;
                    mailItem.Subject = subject;
                    mailItem.Body = body;
                    mailItem.Display();
                }
                catch (System.Exception ex)
                {
                    Process.Start(new ProcessStartInfo(mailtoLink));
                    ex.ToString();
                }

                SupportDatA.checkData(commandData);
                return Result.Succeeded;
            }
            else
            {
                return Result.Cancelled;
            }
        }
        public static void CreateBtn99(RibbonPanel panel)
        {
            var assembly = Assembly.GetExecutingAssembly();
            RibbonItem pushBtn = panel.AddItem(new PushButtonData(
                MethodBase.GetCurrentMethod().DeclaringType?.Name, "Contact" + Environment.NewLine + "Us",
                assembly.Location,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName)
            {
                ToolTip = "Share feedback or request for new plug-in.",
                LargeImage = Imaging.CreateBitmapSourceFromHBitmap(Resource1.Contact_us_32x32.GetHbitmap(), IntPtr.Zero,
                             Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()),
                Image = Imaging.CreateBitmapSourceFromHBitmap(Resource1.Contact_us_16x16.GetHbitmap(), IntPtr.Zero,
                        Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()),

            });
            ContextualHelp contexHelp = new ContextualHelp(ContextualHelpType.Url, "https://kpm-digitalengineering.com/contact/");
            pushBtn.SetContextualHelp(contexHelp);
        }
    }
}
