using Autodesk.Revit.UI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net;
using System.Text;
using Autodesk.Revit.DB;
using System.Linq;
using Mysqlx.Crud;

namespace KPMEngineeringB.R
{
    public static class SupportDatA

    {
        public static string btnName { get; set; } = "Not Defined";
        public static void checkData(ExternalCommandData commandData)
        {
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;

            string newStr = Resource1.newString;

            string insertValue = "INSERT INTO KPMData_Rvt(" +
                                 "ClientName,UserName,HostName,ButtonName,Rvt_Version,FirstMacAddress,IP_Address,TimeStamp)" +
                                 "VALUES(" +
                                 "@Vcname,@Vuser,@VHost,@VbtnName,@Vrvt,@VMacAd,@VIp,@VTime)";
            string ClientName = "KPM";
            string UserName = doc.Application.Username;
            string hostName = Dns.GetHostName();
            string VersionNumber = doc.Application.VersionNumber;
            var networkInterface = NetworkInterface.GetAllNetworkInterfaces();
            string macAddress = "Not Found";
            string ethernetName = "Ethernet";
            foreach (var nic in networkInterface)
            {
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    if (nic.Name.Equals(ethernetName))
                    {
                        macAddress = nic.GetPhysicalAddress().ToString();
                        break;
                    }
                    else if (nic.Name.Contains(macAddress))
                    {
                        macAddress = nic.GetPhysicalAddress().ToString();
                        break;
                    }
                }
                else if (nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 && nic.Name.Contains("Wi-Fi"))
                {
                    macAddress = nic.GetPhysicalAddress().ToString();
                }
            }
            var myIPList = Dns.GetHostEntry(hostName).AddressList.ToList();
            string myIP ="Not Found";
            foreach(var ip in myIPList)
            {
                if (ip.ToString().Contains("."))
                {
                    myIP = ip.ToString();
                    break;
                }
                else
                {
                    myIP = ip.ToString();
                }
            }

            using (MySqlConnection checkMe = new MySqlConnection(newStr))
            {
                try
                {
                    if (checkMe != null)
                    {
                        checkMe.Open();
                        MySqlCommand command = new MySqlCommand(insertValue, checkMe);
                        command.Parameters.AddWithValue("@Vcname", ClientName);
                        command.Parameters.AddWithValue("@Vuser", UserName);
                        command.Parameters.AddWithValue("@VHost", hostName);
                        command.Parameters.AddWithValue("@VbtnName", btnName);
                        command.Parameters.AddWithValue("@Vrvt", VersionNumber);
                        command.Parameters.AddWithValue("@VMacAd", macAddress);
                        command.Parameters.AddWithValue("@VIp", myIP);
                        command.Parameters.AddWithValue("@VTime", DateTime.Now);
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    ex.ToString();
                    System.Windows.Forms.MessageBox.Show("Facing issue while connecting with server.");
                }
                finally
                {
                    checkMe.Close();
                }
            }
        }

        public static bool CheckInternet()
        {
            string newStr = Resource1.newString;

            using (MySqlConnection mySqlConnection = new MySqlConnection(newStr))
            {
                try
                {
                    if (mySqlConnection != null)
                    {
                        mySqlConnection.Open();
                    }
                    return true;

                }
                catch (Exception ex)
                {
                    ex.ToString();
                    System.Windows.Forms.MessageBox.Show("No Internet Connected, Please ensure your Internet Connectivity");
                    return false;
                }
                finally
                {
                    mySqlConnection.Close();
                }
            }
        }

        public static bool CheckAuthorize(ExternalCommandData commandData)
        {
            if (CheckInternet())
            {
                string newStr = Resource1.newString;
                UIApplication uiApp = commandData.Application;
                Document doc = uiApp.ActiveUIDocument.Document;

                string insertValue = "INSERT INTO KPMUser_Rvt(" +
                                     "ClientName,UserName,HostName,FirstMacAddress,TimeStamp,Authorize,ExpireDate,Count)" +
                                     "VALUES(" +
                                     "@Vcname,@Vuser,@VHost,@VMacAd,@VTime,@VAuth,@VExpDate, @counT)";
                string updateVal = "UPDATE kpmdb01.KPMUser_Rvt SET Authorize = 'Disapprove' Where(UserName = @Vuser AND FirstMacAddress != @VMacAd)";
                string ClientName = "KPM";
                string UserName = doc.Application.Username;
                string hostName = Dns.GetHostName();
                var networkInterface = NetworkInterface.GetAllNetworkInterfaces();
                string macAddress = "Not Found";
                string ethernetName = "Ethernet";
                foreach (var nic in networkInterface)
                {
                    if (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                    {
                        if (nic.Name.Equals(ethernetName))
                        {
                            macAddress = nic.GetPhysicalAddress().ToString();
                            break;
                        }
                        else if (nic.Name.Contains(ethernetName))
                        {
                            macAddress = nic.GetPhysicalAddress().ToString();
                        }
                    }

                }
                if (macAddress == "Not Found")
                {
                    foreach (var nic in networkInterface)
                    {
                        if (nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 && nic.Name.Contains("Wi-Fi"))
                        {
                            macAddress = nic.GetPhysicalAddress().ToString();
                        }
                    }
                }
                string Authorize = "Trial";

                DateTime expiryDate = DateTime.Now.AddDays(15);
                List<string> clientName = new List<string>();
                List<string> userName = new List<string>();
                List<string> authList = new List<string>();
                List<string> firstMacAdd = new List<string>();
                List<DateTime> exDateList = new List<DateTime>();
                List<int> counTList = new List<int>();
                using (MySqlConnection mySqlConnection = new MySqlConnection(newStr))
                {
                    if (mySqlConnection != null)
                    {
                        mySqlConnection.Open();
                        string query = "SELECT * FROM KPMUser_Rvt";
                        MySqlCommand com = new MySqlCommand(query, mySqlConnection);
                        using (MySqlDataReader reader = com.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string cname = reader.GetString(0);
                                clientName.Add(cname);
                                string uName = reader.GetString(1);
                                userName.Add(uName);
                                string macAdd = reader.GetString(3);
                                firstMacAdd.Add(macAdd);
                                string auth = reader.GetString(5);
                                authList.Add(auth);
                                var exDate = reader.GetDateTime(6);
                                exDateList.Add(exDate);
                                var counT = reader.GetInt16(7);
                                counTList.Add(counT);
                            }
                            reader.Close();
                        }
                    }
                    else
                    {
                        mySqlConnection.Close();
                        return false;
                    }
                    int counTNo = 0;
                    foreach (var a in Enumerable.Range(0, userName.Count))
                    {
                        if (clientName[a] == ClientName)
                        {
                            counTNo = counTList[a];
                            break;
                        }
                    }
                    int registeredUser = 0;
                    foreach (var b in Enumerable.Range(0, userName.Count))
                    {
                        if (clientName[b] == ClientName && (authList[b] == "Approve" || authList[b] == "Trial"))
                        {
                            if (userName[b] != UserName)
                            {
                                registeredUser++;
                            }
                        }
                    }
                    if (registeredUser < counTNo)
                    {
                        if ((userName.Contains(UserName) && firstMacAdd.Contains(macAddress)) != true)
                        {
                            using (MySqlConnection newcon = new MySqlConnection(newStr))
                            {
                                newcon.Open();
                                MySqlCommand newcom = new MySqlCommand(insertValue, newcon);
                                newcom.Parameters.AddWithValue("@Vcname", ClientName);
                                newcom.Parameters.AddWithValue("@Vuser", UserName);
                                newcom.Parameters.AddWithValue("@VHost", hostName);
                                newcom.Parameters.AddWithValue("@VMacAd", macAddress);
                                newcom.Parameters.AddWithValue("@VTime", DateTime.Now);
                                newcom.Parameters.AddWithValue("@VAuth", Authorize);
                                newcom.Parameters.AddWithValue("@VExpDate", expiryDate);
                                newcom.Parameters.AddWithValue("@counT", 10);
                                newcom.ExecuteNonQuery();
                                MySqlCommand newUp = new MySqlCommand(updateVal, newcon);
                                newUp.Parameters.AddWithValue("@Vuser", UserName);
                                newUp.Parameters.AddWithValue("@VMacAd", macAddress);
                                newUp.ExecuteNonQuery();
                                newcon.Close();
                                return true;
                            }
                        }
                        else if (userName.Contains(UserName))
                        {
                            bool auth = false;
                            bool access = false;

                            using (MySqlConnection updateCon = new MySqlConnection(newStr))
                            {
                                updateCon.Open();
                                MySqlCommand newUp = new MySqlCommand(updateVal, updateCon);
                                newUp.Parameters.AddWithValue("@Vuser", UserName);
                                newUp.Parameters.AddWithValue("@VMacAd", macAddress);
                                newUp.ExecuteNonQuery();
                                updateCon.Close();
                            }

                            foreach (var i in Enumerable.Range(0, userName.Count))
                            {
                                if (userName[i] == UserName && firstMacAdd[i] == macAddress && (authList[i] == "Approve" || authList[i] == "Trial"))
                                {
                                    var currentTime = DateTime.Now;
                                    var difference = exDateList[i] - currentTime;
                                    if (authList[i] == "Approve")
                                    {
                                        access = true;
                                        int compareResult = DateTime.Compare(exDateList[i], currentTime);
                                        if (compareResult != -1)
                                        {
                                            auth = true;
                                            if (difference.TotalDays <= 3 && difference.TotalDays >= 0)
                                            {
                                                System.Windows.Forms.MessageBox.Show($"Your License is about to Expire, {((difference.Days) + 1).ToString()} Days Left.\nPlease contact with KPM-Engineering Team to Renew Your License.");
                                            }
                                            break;
                                        }
                                        else
                                        {
                                            auth = false;
                                            System.Windows.Forms.MessageBox.Show("Your License is Expired, Please contact with KPM-Engineering Team to Renew Your License.");
                                            break;
                                        }
                                    }
                                    else if (authList[i] == "Trial")
                                    {
                                        access = true;
                                        int compareResult = DateTime.Compare(exDateList[i], currentTime);
                                        if (compareResult != -1)
                                        {
                                            auth = true;
                                            if (difference.TotalDays <= 3 && difference.TotalDays >= 0)
                                            {
                                                System.Windows.Forms.MessageBox.Show($"Your Trail Period is about to Expire, {((difference.Days) + 1).ToString()} Days Left.\nPlease contact with KPM-Engineering Team to Purchase a License.");
                                            }
                                            break;
                                        }
                                        else
                                        {
                                            auth = false;
                                            System.Windows.Forms.MessageBox.Show("Your Trial Period is Expired, Please contact with KPM-Engineering Team to Purchase a License.");
                                            break;
                                        }
                                    }
                                }

                            }
                            if (!auth && !access)
                            {
                                System.Windows.Forms.MessageBox.Show("Access Denied.\nPlease Contact KPM-Engineering Team.");
                            }
                            return auth;
                        }
                        return false;
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Oops! You've Reached the Limit of Registered Users." + Environment.NewLine +
                            "Please contact KPM-Engineering Team.");
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
        }
    }    
}
