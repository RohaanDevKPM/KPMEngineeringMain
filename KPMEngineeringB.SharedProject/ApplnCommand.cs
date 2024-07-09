using Autodesk.Revit.UI;
using KPMEngineeringB.R._5._FifthButton;
using KPMEngineeringB.R._6._SixthButton;
using KPMEngineeringB.R.FourthButton;
using KPMEngineeringB.R.ThirdButton;
using KPMEngineeringB.R.FirstButton;
using KPMEngineeringB.R.SecondButton;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KPMEngineeringB.R._7._SeventhButton;
using KPMEngineeringB.R._99._ContactUsButton;
using KPMEngineeringB.R._8._EighthButton;
using KPMEngineeringB.R._9._NinthButton;
using KPMEngineeringB.R._10._TenthButton;
using KPMEngineeringB.R._11._ElevenButton;
using KPMEngineeringB.R._12._TwelfthButton;
using KPMEngineeringB.R._13._ThirteenthButton;
using KPMEngineeringB.R._14._FourteenthButton;
using KPMEngineeringB.R._15._FifteenthButton;
using KPMEngineeringB.R._16._SixteenththButton;
using KPMEngineeringB.R._17._SeventeenthButton;
using KPMEngineeringB.R._18._EighteenthButton;
using KPMEngineeringB.R._19._NineteenthButton;
using KPMEngineeringB.R._20._TwentiethButton;
using KPMEngineeringB.R._21.TwentyFirstButton;
using KPMEngineeringB.R._21.TwentySecondButton;

namespace KPMEngineeringB.R
{
    public class ApplnCommand : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {

        // Rohaan chalo chai pine
        
            try
            {
                application.CreateRibbonTab("KPM-Engineering");
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
            try
            {
                application.CreateRibbonTab("KPM-Generic");
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }

            var ribbonPanel1 = application.GetRibbonPanels("KPM-Engineering").FirstOrDefault(x => x.Name == "CAD to Revit") ??
                              application.CreateRibbonPanel("KPM-Engineering", "CAD to Revit");

            FirstButtonCommand.CreateBtn1(ribbonPanel1);
            SecondButtonCommand.CreateBtn2(ribbonPanel1);
            ThirdButtonCommand.CreateBtn3(ribbonPanel1);
            FourthButtonCommand.CreateBtn4(ribbonPanel1);
            FifthButtonCommand.CreateBtn5(ribbonPanel1);
            TwentySecondBtnCommand.CreateBtn22(ribbonPanel1);
            

            var ribbonPanel2 = application.GetRibbonPanels("KPM-Engineering").FirstOrDefault(x => x.Name == "MEP") ??
                              application.CreateRibbonPanel("KPM-Engineering", "MEP");

            SixthButtonCommand.CreateBtn6(ribbonPanel2);
            ThirteenthBtnCommand.CreateBtn13(ribbonPanel2);
            FourteenthBtnCommand.CreateBtn14(ribbonPanel2);
            FifteenthBtnCommand.CreateBtn15(ribbonPanel2);
            

            var ribbonPanel3 = application.GetRibbonPanels("KPM-Engineering").FirstOrDefault(x => x.Name == "Structure") ??
                application.CreateRibbonPanel("KPM-Engineering", "Structure");

            SeventhButtonCommand.CreateBtn7(ribbonPanel3);
            EighteenthBtnCommand.CreateBtn18(ribbonPanel3);
            NineteenthBtnCommand.CreateBtn19(ribbonPanel3);
            TwentiethBtnCommand.CreateBtn20(ribbonPanel3);
            TwentyFirstBtnCommand.CreateBtn21(ribbonPanel3);

            var ribbonPanel4 = application.GetRibbonPanels("KPM-Generic").FirstOrDefault(x => x.Name == "Generic") ??
                application.CreateRibbonPanel("KPM-Generic", "Generic");

            EightBtnCommand.CreateBtn8(ribbonPanel4);
            NinthBtnCommand.CreateBtn9(ribbonPanel4);
            TenthBtnCommand.CreateBtn10(ribbonPanel4);
            EleventhBtnCommand.CreateBtn11(ribbonPanel4);
            TwelfthBtnCommand.CreateBtn12(ribbonPanel4);
            SixteenthBtnCommand.CreateBtn16(ribbonPanel4);
            SeventeenthBtnCommand.CreateBtn17(ribbonPanel4);

            var ribbonPanel98 = application.GetRibbonPanels("KPM-Generic").FirstOrDefault(x => x.Name == "KPM-Engineering") ??
                application.CreateRibbonPanel("KPM-Generic", "KPM-Engineering");
            
            ContactUsCommand.CreateBtn99(ribbonPanel98);

            var ribbonPanel99 = application.GetRibbonPanels("KPM-Engineering").FirstOrDefault(x => x.Name == "KPM-Engineering") ??
                application.CreateRibbonPanel("KPM-Engineering", "KPM-Engineering");
            
            ContactUsCommand.CreateBtn99(ribbonPanel99);

            return Result.Succeeded;
        }
    }
}
