using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace RecordsAttachment
{
    public partial class Frm_Main : Form
    {
        public Frm_Main()
        {
            InitializeComponent();
        }

        FirefoxOptions options;
        //ChromeOptions chromeOptions;
        IWebDriver driver;
        List<string> Lst_data;
        string Str_Url = "";
        string Str_Ptype = "Hag";

        private void Form1_Load(object sender, EventArgs e)
        {
            options = new FirefoxOptions();
            options.BrowserExecutableLocation = @"C:\Program Files\Mozilla Firefox\firefox.exe";
            //options.SetPreference("browser.download.folderList", 2);
            //options.SetPreference("browser.download.dir", "D:\\Temp\\");
            //options.SetPreference("browser.download.useDownloadDir", true);
            options.SetPreference("browser.helperApps.neverAsk.saveToDisk", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;application/vnd.ms-excel;text/html;text/plain;application/text;text/xml;application/xml");
            options.SetPreference("print.always_print_silent", true);

            if (driver == null)
                driver = new FirefoxDriver(options);
            driver.Manage().Window.Maximize();
            //IJavaScriptExecutor js41 = (IJavaScriptExecutor)driver;

            driver.Navigate().GoToUrl("http://ittms.tax.gov.ir/Pages/Remained/StrRem/");
            //string script = "document.getElementById('CPC_Remained_Str_Rem_btnTTMS').click();";
            //js41.ExecuteScript(script);
        }

      

        private void Btn_Start_Click(object sender, EventArgs e)
        {
            Btn_Start.Enabled = false;
            Prg_Attach.Value = 1;
            LbL_Message.Visible = false;
            bool SelectType = false;

            Lst_data_Exe = new List<string>();

            foreach (RadioButton Rbtn in groupBox1.Controls.OfType<RadioButton>())
            {
                if (Rbtn.Checked)
                    SelectType = true;
            }
            if (!SelectType)
            {
                LbL_Message.Text = "Select Type!";
                LbL_Message.Visible = true;
                Btn_Start.Enabled = true;
                return;
            }
            Str_Url = driver.Url.ToString();
            LbL_Message.Visible = false;
            if (Txt_FileAddrress.Text.Trim() == "")
            {
                LbL_Message.Text="Select a file please!";
                LbL_Message.Visible = true;
                Btn_Start.Enabled = true;
                return;
            }

            if (Rbtn_Hag.Checked)
                Str_Ptype = "Hag";
            else
                Str_Ptype = "Hog";

            Lst_data = new List<string>();
            string SourceFile = @Txt_FileAddrress.Text;
            Lst_data = File.ReadAllLines(SourceFile).ToList();

            if (Lst_data[0].Split('*').Length > 2)
                Prg_Attach.Maximum = Lst_data.Select(l => l.Split('*')[0] +"*"+ l.Split('*')[1]).Distinct().ToList().Count + 1;
            else
                Prg_Attach.Maximum = Lst_data.Count + 1;

            Prg_Attach.Minimum = 1;
            Prg_Attach.Step = 1;

            LoadItem();
        }

        List<string> Lst_data_Exe;
        List<string> Lst_Codes;
        string Str_CurrentId= "";
        string Str_CurrentTIN= "";
        string Str_HagType = "";
        //string Str_LastProcessTIN = "";
        private void LoadItem()
        {
            string Str_data_sub = "";
            if(Lst_data[0].Split('*').Length > 2)
            {
                Str_HagType = "MultipleP";
                Lst_Codes = new List<string>();
               
                foreach (var item in Lst_data)
                {
                    
                    if (Lst_Codes.Count == 0 && (Str_CurrentId != item.Split('*')[0] || Str_CurrentTIN!= item.Split('*')[1]))
                    {
                        Str_CurrentId = item.Split('*')[0];
                        Str_CurrentTIN = item.Split('*')[1];
                    }
                    if (item.Split('*')[0] == Str_CurrentId)
                    {
                        
                        if (item.Split('*')[1] == Str_CurrentTIN)
                        {
                            Lst_Codes.Add(item.Split('*')[2]);

                            if (item.Contains('>'))
                            {
                                Lst_Codes.Remove(item.Split('*')[2]);
                                if (!Lst_data_Exe.Contains(Str_CurrentId + "*" + Str_CurrentTIN))
                                {
                                    Lst_data_Exe.Add(Str_CurrentId + "*" + Str_CurrentTIN);
                                    DoStepProgress();
                                }
                            }
                            //if (Lst_Codes.Count != 0)
                            //{
                            //    Str_LastProcessTIN = Str_CurrentTIN;
                            //}
                            if (!Lst_data_Exe.Contains(item.Split('*')[0] + "*" + item.Split('*')[1]))
                            {
                                Str_data_sub = item.Split('*')[0] + "*" + item.Split('*')[1];
                            }
                        }
                        else
                        {
                            Str_CurrentTIN = item.Split('*')[1];
                            break;
                        }
                    }
                    else if (Lst_Codes.Count != 0 && item != "")
                    {
                        Str_CurrentId = item.Split('*')[0];
                        Str_CurrentTIN = item.Split('*')[1];
                        //DoStepProgress();
                        break;
                    }
                }

            }

            else
            {
                Str_HagType = "SingleP";
                foreach (var item in Lst_data)
                {
                    if (item.Contains('>'))
                    {
                        if (!Lst_data_Exe.Contains(item))
                        {
                            Lst_data_Exe.Add(item);
                            DoStepProgress();
                        }
                        continue;
                    }
                    if (!Lst_data_Exe.Contains(item))
                    {
                        Str_data_sub = item;
                        break;
                    }
                }
            }

           

            if (Str_data_sub != "")
            {
                if (Str_Ptype == "Hag")
                {
                    LblCurrent_Changing(Str_data_sub.Split('*')[0] + " is being done...");
                    Thread Thread1 = new Thread(() => Hag_DoJob(Str_data_sub));
                    Thread1.Name += Str_data_sub.Split('*')[0];
                    Thread1.Start();
                }
                else if (Str_Ptype == "Hog")
                {
                    LblCurrent_Changing(Str_data_sub + " is being done...");
                    Thread Thread1 = new Thread(() => Hog_DoJob(Str_data_sub));
                    Thread1.Name += Str_data_sub;
                    Thread1.Start();
                }
            }


            if (Lst_data_Exe.Count == Prg_Attach.Maximum - 1)
            {
                Btn_Start.BeginInvoke((Action)delegate
                {
                    Btn_Start.Enabled = true;
                    LblCurrent_Changing("Completed...");
                });
            }
        }

        delegate void SetTextForLabel(string text);
        private void LblCurrent_Changing(string text)
        {
            if (LbL_Current.InvokeRequired)
            {
                SetTextForLabel Stl = new SetTextForLabel(LblCurrent_Changing);
                this.Invoke(Stl, new object[] { text });
            }
            else
            {
                LbL_Current.Text = text;
            }
        }

        List<string> Lst_reports;
        private void SendReportToFile(string report)
        {
            //Lst_reports.Add(report);
            Lst_reports = new List<string>();
            string DestinationFile = @Txt_FileAddrress.Text;
            //string DestinationFile = Path.GetDirectoryName(SourceFile) + "\\" + Path.GetFileNameWithoutExtension(SourceFile) + "_tmp.txt";
            if (!File.Exists(DestinationFile))
            {
                File.Create(DestinationFile);
            }
            Lst_reports = Lst_data;
            if(Str_Ptype=="Hag")
            {
                if(Str_HagType=="SingleP")
                {
                    for (int i = 0; i < Lst_reports.Count; i++)
                    {
                        if (Lst_reports[i] == report.Split('>')[0])
                        {
                            Lst_reports[i] = report;
                            break;
                        }
                    }
                }
                else if (Str_HagType == "MultipleP")
                {
                    int rows = 0;
                    for (int i = 0; i < Lst_reports.Count; i++)
                    {
                        if(rows==Lst_Codes.Count)
                        {
                            break;
                        }
                        if (Lst_reports[i].Split('*')[0] + "*" + Lst_reports[i].Split('*')[1] == report.Split('>')[0] && !Lst_reports[i].Contains('>'))
                        {
                            rows += 1;
                            Lst_reports[i] = Lst_reports[i] + ">Done";
                        }
                    }
                }
                    
            }
            else if (Str_Ptype == "Hog")
            {
                for (int i = 0; i < Lst_reports.Count; i++)
                {
                    if (Lst_reports[i] == report.Split('>')[0])
                    {
                        Lst_reports[i] = Lst_reports[i] + ">Done";
                        break;
                    }
                }
            }
           
            try
            {
                File.WriteAllLines(DestinationFile, Lst_reports);

            }
            catch { }
            //StreamReader reader = new StreamReader(DestinationFile);
            //StreamWriter Writer = new StreamWriter(DestinationFile);
            //string input = "";
            //while (input == reader.ReadLine())
            //{
            //    if(input==report.Split('>')[0])
            //    {
            //        Writer.WriteLine(report);
            //        break;
            //    }
            //}
            //reader.Close();
            ////string last = File.ReadLines(DestinationFile).Last();
            //Writer.WriteLine(report);
            //Writer.Close();

            //using (var fs = File.Open(DestinationFile, FileMode.Open, FileAccess.ReadWrite))
            //{
            //    StreamReader Reader = new StreamReader(fs);
            //    StreamWriter Writer = new StreamWriter(fs);
            //    foreach (var item in Lst_reports)
            //    {
            //        Writer.WriteLine(item);
            //    }

            //}
        }
        public class SomeException : Exception
        {
            // Add your own constructors and properties here.
        }
        private void Hag_DoJob(string data)
        {

            //IJavaScriptExecutor js1 = (IJavaScriptExecutor)driver;
            //string script = "document.getElementById('CPC_Remained_Str_Rem_btnTTMS').click();";
            //js1.ExecuteScript(script);
            //Thread.Sleep(10000);

            try
            {
                //int step01 = 0;
                int step02 = 0;
                //int step1 = 0;
                int step2 = 0;
                int step3 = 0;
                int step4 = 0;
                int step5 = 0;
                int step6 = 0;
                int step7 = 0;
                int step8 = 0;
                int step9 = 0;
                int step10 = 0;
                int step11 = 0;
                int step12 = 0;
                int step13 = 0;

                //WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                int stepcounter = 0;
                int loopcounter = 0;

                while (stepcounter < 8)
                {
                    ++loopcounter;
                    if(loopcounter >3 )
                    {
                        throw new SomeException();
                    }
                    //if (step01 == 0 && FindElement("navbar-form dropdown", FindBy.ByClass))
                    //{
                    //    IJavaScriptExecutor js2 = (IJavaScriptExecutor)driver;
                    //    string script2 = "var element=document.getElementsByClassName('navbar-form dropdown')[0]; element.className = 'navbar - form dropdown open';";
                    //    js2.ExecuteScript(script2);
                    //    step01 = 1;
                    //    ++stepcounter;
                    //}

                    if (step02 == 0 && FindElement("CPC_urvRemainViewSolr103_ddlFilterAttachParam1", FindBy.ById)) 
                    {
                        IJavaScriptExecutor js3 = (IJavaScriptExecutor)driver;
                        string script3 = "var ddl = document.getElementById('CPC_urvRemainViewSolr103_ddlFilterAttachParam1');  ddl.selectedIndex = 1 ";
                        js3.ExecuteScript(script3);
                        step02 = 1;
                        ++stepcounter;
                    }


                    //wait.Until(w => w.FindElements(By.CssSelector("[class='navbar-form dropdown']")).ToList()[10]);
                    ////if (step1 == 0 && step02 == 1 && FindElement("navbar-form dropdown", FindBy.ByClass))
                    ////{
                    ////    IJavaScriptExecutor js2 = (IJavaScriptExecutor)driver;
                    ////    string script2 = "var element=document.getElementsByClassName('navbar-form dropdown')[10]; element.className = 'navbar - form dropdown open';";
                    ////    js2.ExecuteScript(script2);
                    ////    step1 = 1;
                    ////    ++stepcounter;
                    ////}

                    //Thread.Sleep(3000);

                    if (step2 == 0 && step02 == 1 && FindElement("CPC_urvRemainViewSolr103_KharidarNationalCode", FindBy.ById))
                    {
                        IJavaScriptExecutor js3 = (IJavaScriptExecutor)driver;
                        string script3 = "var txt = document.getElementById('CPC_urvRemainViewSolr103_KharidarNationalCode');  txt.value =" + data.Split('*')[0];
                        js3.ExecuteScript(script3);
                        step2 = 1;
                        ++stepcounter;
                    }


                    //IJavaScriptExecutor js42 = (IJavaScriptExecutor)driver;
                    //string script42 = " var ddl = document.getElementById('CPC_urvRemainViewSolr103_ListPage1_ddlItemPerPage').selectedIndex = 7 ";
                    //js42.ExecuteScript(script42);
                    //Thread.Sleep(3000);
                    //WebDriverWait wait2 = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

                    if (step3 == 0 && step2 == 1 && FindElement("CPC_urvRemainViewSolr103_ListPage1_ddlItemPerPage", FindBy.ById))
                    {
                        //wait.Until(w => w.FindElements(By.Id("CPC_urvRemainViewSolr103_ListPage1_ddlItemPerPage")));
                        IWebElement element = driver.FindElement(By.Id("CPC_urvRemainViewSolr103_ListPage1_ddlItemPerPage"));
                        SelectElement select = new SelectElement(element);
                        select.SelectByText("1000");
                        step3 = 1;
                        ++stepcounter;

                    }



                    //WebDriverWait wait3 = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

                    if (step4 == 0 && step3 == 1 && FindElement("navbar-form dropdown", FindBy.ByClass))
                    {
                        //wait.Until(w => w.FindElements(By.CssSelector("[class='navbar-form dropdown']")).ToList()[10]);
                        IJavaScriptExecutor js4 = (IJavaScriptExecutor)driver;
                        string script4 = " var div=document.getElementsByClassName('navbar-form dropdown')[10];  var btn = div.querySelectorAll('button')[1];   var btn = div.querySelectorAll('button')[1]; btn.click(); ";
                        js4.ExecuteScript(script4);
                        step4 = 1;
                        ++stepcounter;

                    }

                    //Thread.Sleep(8000);
                    int loopcounter2 = 0;
                    while (step5 == 0)
                    {
                        ++loopcounter2;
                        if (loopcounter2 > 5)
                        {
                            throw new SomeException();
                        }

                        IJavaScriptExecutor js5 = (IJavaScriptExecutor)driver;
                        string Str_records_count = (string)js5.ExecuteScript("return document.getElementsByClassName('fgButton')[0].rows[0].cells[3].innerText");
                        Regex re = new Regex(@"\d+");
                        Match records_count = re.Match(Str_records_count);
                        int Records = int.Parse(records_count.ToString());
                        if (Records > 200)
                        {
                            Thread.Sleep(2000);
                        }
                        else
                        {
                            break;
                        }

                    }
                    if(Str_HagType== "SingleP")
                    {
                        if (step5 == 0 && step4 == 1 && FindElement("fgButton", FindBy.ByClass))
                        {
                            //wait.Until(w => w.FindElements(By.CssSelector("[class='fgButton']")).ToList()[0]);
                            IJavaScriptExecutor js5 = (IJavaScriptExecutor)driver;
                            string script5 = " "
                                   + " var tbl = document.getElementsByClassName('fgButton')[0];"
                                   + " var spn_text = tbl.rows[0].cells[3].innerText;"
                                   + " var regex = /\\d +/g;"
                                   + " var rowcount = spn_text.match(regex);"
                                   + " if (rowcount < 200)"
                                   + " {"
                                   + "     var checkAll = document.getElementsByClassName('checkAllgrp')[0];"
                                   + "    checkAll.click();"
                                   + "}"
                                   + "else"
                                   + "{"
                                   + " window.location.reload();"
                                   + " };";

                            js5.ExecuteScript(script5);
                            step5 = 1;
                            ++stepcounter;

                        }

                        Thread.Sleep(1000);

                        if (step6 == 0 && step5 == 1 && FindElement("checkAllgrp", FindBy.ByClass))
                        {
                            //wait.Until(w => w.FindElements(By.CssSelector("[class='checkAllgrp']")).ToList()[0]);
                            if (driver.FindElement(By.CssSelector("[class='checkAllgrp']")).Selected == true)
                            {
                                IJavaScriptExecutor js6 = (IJavaScriptExecutor)driver;
                                string script6 = "var btn2 = document.getElementsByClassName('btn btn-md btn-info FlexGridHandler')[0]; btn2.click();";
                                js6.ExecuteScript(script6);
                                step6 = 1;
                                ++stepcounter;
                            }

                        }
                    }
                    else if(Str_HagType == "MultipleP")
                    {
                        if (step5 == 0 && step4 == 1 && FindElement("fgButton", FindBy.ByClass))
                        {
                            //wait.Until(w => w.FindElements(By.CssSelector("[class='fgButton']")).ToList()[0]);
                            IJavaScriptExecutor js5 = (IJavaScriptExecutor)driver;
                            foreach (var code in Lst_Codes)
                            {
                                string script5 = "var grid = document.getElementById('FlexGrid'); " +
                                    "for (var i = 1; i < grid.rows.length; i++)" +
                                    "{" +
                                    " var checkbox = grid.rows[i].cells[0].firstChild;" +
                                    "  if (grid.rows[i].cells[4].innerText == '" + code + "')" +
                                    " {" +
                                       " checkbox.checked = true;" +
                                    " }" +
                                    "}";

                                js5.ExecuteScript(script5);
                            }
                            step5 = 1;
                            ++stepcounter;

                        }

                        Thread.Sleep(1000);

                        if (step6 == 0 && step5 == 1)
                        {
                            //wait.Until(w => w.FindElements(By.CssSelector("[class='checkAllgrp']")).ToList()[0]);
                            
                            IJavaScriptExecutor js6 = (IJavaScriptExecutor)driver;
                            string script6 = "var btn2 = document.getElementsByClassName('btn btn-md btn-info FlexGridHandler')[0]; btn2.click();";
                            js6.ExecuteScript(script6);
                            step6 = 1;
                            ++stepcounter;
                            

                        }
                    }
                   

                    

                    Thread.Sleep(2000);

                    if (step7 == 0 && step6 == 1 && FindElement("modal-footer", FindBy.ByClass))
                    {
                        //wait.Until(w => w.FindElements(By.CssSelector("[class='modal-footer']")));

                        IJavaScriptExecutor js7 = (IJavaScriptExecutor)driver;
                        string script7 = "  var div1 = document.getElementsByClassName('modal-footer')[0]; var btn3 = div1.querySelectorAll('button')[0]; btn3.click(); ";
                        js7.ExecuteScript(script7);
                        step7 = 1;
                        ++stepcounter;

                    }

                    Thread.Sleep(4000);

                    if (step8 == 0 && step7 == 1 && FindElement("bootbox-body", FindBy.ByClass))
                    {
                        IJavaScriptExecutor js8 = (IJavaScriptExecutor)driver;
                        string script8 = "";
                        if (Str_Ptype == "Hag")
                        {
                            script8 = "  var div2 = document.getElementsByClassName('bootbox-body')[0];var a_= div2.querySelectorAll('a')[0]; a_.click();  ";
                        }
                        else if (Str_Ptype == "Hog")
                        {
                            script8 = "  var div2 = document.getElementsByClassName('bootbox-body')[0];var a_= div2.querySelectorAll('a')[1]; a_.click();  ";
                        }
                        js8.ExecuteScript(script8);
                        step8 = 1;
                        ++stepcounter;
                    }




                }

                if (stepcounter == 8)
                {
                    stepcounter = 0;
                    loopcounter = 0;
                }

                while(!FindElement("FlexGrid",FindBy.ById))
                {
                    Thread.Sleep(2000);
                }

                while (stepcounter < 5)
                {
                    ++loopcounter;
                    if (loopcounter > 3)
                    {
                        throw new SomeException();
                    }

                    string Str_el = "";
                    if (Str_Ptype == "Hag")
                    {
                        Str_el = "CPC_urvRTCMIS_NationalCode";
                    }
                    else if (Str_Ptype == "Hog")
                    {
                        Str_el = "CPC_urvRTCITMS_NationalCode";
                    }
                    if (step9 == 0 && FindElement(Str_el, FindBy.ById))
                    {

                        IJavaScriptExecutor js9 = (IJavaScriptExecutor)driver;
                        string script9 = " var div = document.getElementsByClassName('navbar-form dropdown')[3];div.className = 'navbar-form dropdown open';";
                        script9 += "var txt = document.getElementById('"+ Str_el + "');txt.value = '" + data.Split('*')[0] + "';";
                        script9 += "var btn = div.querySelectorAll('button')[1];";
                        script9 += "btn.click(); ";
                        js9.ExecuteScript(script9);
                        step9 = 1;
                        ++stepcounter;

                    }

                    int rows_count = 0;
                    int loopcounter2 = 0;
                    while (step10 == 0)
                    {
                        ++loopcounter2;
                        if(loopcounter2==10)
                        {
                            throw new SomeException();
                        }
                        IJavaScriptExecutor js10_ = (IJavaScriptExecutor)driver;
                        long count_record = (long)js10_.ExecuteScript("return document.getElementById('FlexGrid').rows.length");
                        if (count_record > 5)
                        {
                            Thread.Sleep(2000);
                        }
                        else
                        {
                            rows_count = (int)count_record;
                            break;
                        }
                    }
                    if (rows_count == 1)
                    {
                        stepcounter = 5;
                        step10 = 1;
                        step11 = 1;
                        step12 = 1;
                        step13 = 1;
                        SendReportToFile(data + ">Not");

                        IJavaScriptExecutor js11_ = (IJavaScriptExecutor)driver;
                        string script11_ = "var btn2 = document.getElementsByClassName('btn btn-md btn-warning FlexGridHandler')[0];btn2.click();";
                        js11_.ExecuteScript(script11_);

                        Thread.Sleep(3000);

                        if (FindElement("modal-footer", FindBy.ByClass))
                        {
                            IJavaScriptExecutor js12 = (IJavaScriptExecutor)driver;
                            string script12 = "var div1 = document.getElementsByClassName('modal-footer')[0];var btn3 = div1.getElementsByClassName('btn btn-success')[0];btn3.click();";
                            js12.ExecuteScript(script12);
                        }

                        Thread.Sleep(4000);

                        if (FindElement("modal-footer", FindBy.ByClass))
                        {
                            IJavaScriptExecutor js13 = (IJavaScriptExecutor)driver;
                            string script13 = "var div1 = document.getElementsByClassName('modal-footer')[0];var btn3 = div1.getElementsByClassName('btn btn-info')[0];btn3.click();";
                            js13.ExecuteScript(script13);
                        }

                    }

                    if(rows_count > 2)
                    {
                        IJavaScriptExecutor js10_ = (IJavaScriptExecutor)driver;
                        string script10_ = " var div = document.getElementsByClassName('navbar-form dropdown')[4];div.className = 'navbar-form dropdown open';";
                        script10_ += "var txt = document.getElementById('CPC_urvRTCMIS_Sanim_TIN');txt.value = '" + data.Split('*')[1] + "';";
                        script10_ += "var btn = div.querySelectorAll('button')[1];";
                        script10_ += "btn.click(); ";
                        js10_.ExecuteScript(script10_);

                        int loopcounter3 = 0;
                        while (true)
                        {
                            ++loopcounter3;
                            if (loopcounter3 == 10)
                            {
                                throw new SomeException();
                            }
                            IJavaScriptExecutor js0_ = (IJavaScriptExecutor)driver;
                            long count_record = (long)js0_.ExecuteScript("return document.getElementById('FlexGrid').rows.length");
                            if (count_record > 2)
                            {
                                Thread.Sleep(2000);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                  

                    if (step10 == 0 && FindElement("FlexGrid", FindBy.ById))
                    {
                        SendReportToFile(data + ">Done");

                        IJavaScriptExecutor js10 = (IJavaScriptExecutor)driver;
                        string script10 = " var tbl = document.getElementById('FlexGrid');"
                                        + "if (tbl.rows.length >= 2)"
                                        + "{"
                                        + "   var checkAll = document.getElementsByClassName('checkAllgrp')[0];"
                                        + "  checkAll.click();"
                                        + "}";
                        js10.ExecuteScript(script10);
                        step10 = 1;
                        ++stepcounter;
                    }

                    Thread.Sleep(2000);

                    if (step11 == 0 && FindElement("btn btn-md btn-info FlexGridHandler", FindBy.ByClass))
                    {
                        IJavaScriptExecutor js11 = (IJavaScriptExecutor)driver;
                        string script11 = "var btn2 = document.getElementsByClassName('btn btn-md btn-info FlexGridHandler')[0];btn2.click();";
                        js11.ExecuteScript(script11);
                        step11 = 1;
                        ++stepcounter;
                    }
                    Thread.Sleep(2000);

                    if (step12 == 0 && FindElement("modal-footer", FindBy.ByClass))
                    {
                        IJavaScriptExecutor js12 = (IJavaScriptExecutor)driver;
                        string script12 = "var div1 = document.getElementsByClassName('modal-footer')[0];var btn3 = div1.querySelectorAll('button')[0];btn3.click();";
                        js12.ExecuteScript(script12);
                        step12 = 1;
                        ++stepcounter;
                    }
                    Thread.Sleep(5000);

                    if (step13 == 0 && FindElement("modal-footer", FindBy.ByClass))
                    {
                        IJavaScriptExecutor js13 = (IJavaScriptExecutor)driver;
                        string script13 = "var div1 = document.getElementsByClassName('modal-footer')[0];var btn3 = div1.querySelectorAll('button')[0];btn3.click();";
                        js13.ExecuteScript(script13);
                        step13 = 1;
                        ++stepcounter;
                    }
                }
                if (stepcounter == 5)
                {
                    //DoStepProgress();
                    LoadItem();
                }
            }
            catch
            {
                IJavaScriptExecutor js13 = (IJavaScriptExecutor)driver;
                string script13 = " window.location.href='"+Str_Url+"'";
                js13.ExecuteScript(script13);
                Thread.Sleep(5000);
                //DoStepProgress();
                SendReportToFile(data + ">Done");
                LoadItem();
            }
        }


        private void Hog_DoJob(string data)
        {

            //IJavaScriptExecutor js1 = (IJavaScriptExecutor)driver;
            //string script = "document.getElementById('CPC_Remained_Str_Rem_btnTTMS').click();";
            //js1.ExecuteScript(script);
            //Thread.Sleep(10000);

            try
            {
                //int step01 = 0;
                int step02 = 0;
                //int step1 = 0;
                int step2 = 0;
                int step3 = 0;
                int step4 = 0;
                int step5 = 0;
                int step6 = 0;
                int step7 = 0;
                int step8 = 0;
                int step9 = 0;
                int step10 = 0;
                int step11 = 0;
                int step12 = 0;
                int step13 = 0;

                //WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                int stepcounter = 0;
                int loopcounter = 0;

                while (stepcounter < 8)
                {
                    ++loopcounter;
                    if (loopcounter > 3)
                    {
                        throw new SomeException();
                    }
                    //if (step01 == 0 && FindElement("navbar-form dropdown", FindBy.ByClass))
                    //{
                    //    IJavaScriptExecutor js2 = (IJavaScriptExecutor)driver;
                    //    string script2 = "var element=document.getElementsByClassName('navbar-form dropdown')[0]; element.className = 'navbar - form dropdown open';";
                    //    js2.ExecuteScript(script2);
                    //    step01 = 1;
                    //    ++stepcounter;
                    //}

                    if (step02 == 0 && FindElement("CPC_urvRemainViewSolr103_ddlFilterAttachParam1", FindBy.ById))
                    {
                        IJavaScriptExecutor js3 = (IJavaScriptExecutor)driver;
                        string script3 = "var ddl = document.getElementById('CPC_urvRemainViewSolr103_ddlFilterAttachParam1');  ddl.selectedIndex = 1 ";
                        js3.ExecuteScript(script3);
                        step02 = 1;
                        ++stepcounter;
                    }


                    //wait.Until(w => w.FindElements(By.CssSelector("[class='navbar-form dropdown']")).ToList()[10]);
                    ////if (step1 == 0 && step02 == 1 && FindElement("navbar-form dropdown", FindBy.ByClass))
                    ////{
                    ////    IJavaScriptExecutor js2 = (IJavaScriptExecutor)driver;
                    ////    string script2 = "var element=document.getElementsByClassName('navbar-form dropdown')[10]; element.className = 'navbar - form dropdown open';";
                    ////    js2.ExecuteScript(script2);
                    ////    step1 = 1;
                    ////    ++stepcounter;
                    ////}

                    //Thread.Sleep(3000);

                    if (step2 == 0 && step02 == 1 && FindElement("CPC_urvRemainViewSolr103_KharidarNationalCode", FindBy.ById))
                    {
                        IJavaScriptExecutor js3 = (IJavaScriptExecutor)driver;
                        string script3 = "var txt = document.getElementById('CPC_urvRemainViewSolr103_KharidarNationalCode');  txt.value =" + data.Split('*')[0];
                        js3.ExecuteScript(script3);
                        step2 = 1;
                        ++stepcounter;
                    }


                    //IJavaScriptExecutor js42 = (IJavaScriptExecutor)driver;
                    //string script42 = " var ddl = document.getElementById('CPC_urvRemainViewSolr103_ListPage1_ddlItemPerPage').selectedIndex = 7 ";
                    //js42.ExecuteScript(script42);
                    //Thread.Sleep(3000);
                    //WebDriverWait wait2 = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

                    if (step3 == 0 && step2 == 1 && FindElement("CPC_urvRemainViewSolr103_ListPage1_ddlItemPerPage", FindBy.ById))
                    {
                        //wait.Until(w => w.FindElements(By.Id("CPC_urvRemainViewSolr103_ListPage1_ddlItemPerPage")));
                        IWebElement element = driver.FindElement(By.Id("CPC_urvRemainViewSolr103_ListPage1_ddlItemPerPage"));
                        SelectElement select = new SelectElement(element);
                        select.SelectByText("1000");
                        step3 = 1;
                        ++stepcounter;

                    }



                    //WebDriverWait wait3 = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

                    if (step4 == 0 && step3 == 1 && FindElement("navbar-form dropdown", FindBy.ByClass))
                    {
                        //wait.Until(w => w.FindElements(By.CssSelector("[class='navbar-form dropdown']")).ToList()[10]);
                        IJavaScriptExecutor js4 = (IJavaScriptExecutor)driver;
                        string script4 = " var div=document.getElementsByClassName('navbar-form dropdown')[10];  var btn = div.querySelectorAll('button')[1];   var btn = div.querySelectorAll('button')[1]; btn.click(); ";
                        js4.ExecuteScript(script4);
                        step4 = 1;
                        ++stepcounter;

                    }

                    //Thread.Sleep(8000);
                    int loopcounter2 = 0;
                    while (step5 == 0)
                    {
                        ++loopcounter2;
                        if (loopcounter2 > 5)
                        {
                            throw new SomeException();
                        }

                        IJavaScriptExecutor js5 = (IJavaScriptExecutor)driver;
                        string Str_records_count = (string)js5.ExecuteScript("return document.getElementsByClassName('fgButton')[0].rows[0].cells[3].innerText");
                        Regex re = new Regex(@"\d+");
                        Match records_count = re.Match(Str_records_count);
                        int Records = int.Parse(records_count.ToString());
                        if (Records > 200)
                        {
                            Thread.Sleep(2000);
                        }
                        else
                        {
                            break;
                        }

                    }
                    if (step5 == 0 && step4 == 1 && FindElement("fgButton", FindBy.ByClass))
                    {
                        //wait.Until(w => w.FindElements(By.CssSelector("[class='fgButton']")).ToList()[0]);
                        IJavaScriptExecutor js5 = (IJavaScriptExecutor)driver;
                        string script5 = " "
                               + " var tbl = document.getElementsByClassName('fgButton')[0];"
                               + " var spn_text = tbl.rows[0].cells[3].innerText;"
                               + " var regex = /\\d +/g;"
                               + " var rowcount = spn_text.match(regex);"
                               + " if (rowcount < 200)"
                               + " {"
                               + "     var checkAll = document.getElementsByClassName('checkAllgrp')[0];"
                               + "    checkAll.click();"
                               + "}"
                               + "else"
                               + "{"
                               + " window.location.reload();"
                               + " };";

                        js5.ExecuteScript(script5);
                        step5 = 1;
                        ++stepcounter;

                    }

                    Thread.Sleep(1000);


                    if (step6 == 0 && step5 == 1 && FindElement("checkAllgrp", FindBy.ByClass))
                    {
                        //wait.Until(w => w.FindElements(By.CssSelector("[class='checkAllgrp']")).ToList()[0]);
                        if (driver.FindElement(By.CssSelector("[class='checkAllgrp']")).Selected == true)
                        {
                            IJavaScriptExecutor js6 = (IJavaScriptExecutor)driver;
                            string script6 = "var btn2 = document.getElementsByClassName('btn btn-md btn-info FlexGridHandler')[0]; btn2.click();";
                            js6.ExecuteScript(script6);
                            step6 = 1;
                            ++stepcounter;
                        }

                    }

                    Thread.Sleep(2000);

                    if (step7 == 0 && step6 == 1 && FindElement("modal-footer", FindBy.ByClass))
                    {
                        //wait.Until(w => w.FindElements(By.CssSelector("[class='modal-footer']")));

                        IJavaScriptExecutor js7 = (IJavaScriptExecutor)driver;
                        string script7 = "  var div1 = document.getElementsByClassName('modal-footer')[0]; var btn3 = div1.querySelectorAll('button')[0]; btn3.click(); ";
                        js7.ExecuteScript(script7);
                        step7 = 1;
                        ++stepcounter;

                    }

                    Thread.Sleep(4000);

                    if (step8 == 0 && step7 == 1 && FindElement("bootbox-body", FindBy.ByClass))
                    {
                        IJavaScriptExecutor js8 = (IJavaScriptExecutor)driver;
                        string script8 = "";
                        if (Str_Ptype == "Hag")
                        {
                            script8 = "  var div2 = document.getElementsByClassName('bootbox-body')[0];var a_= div2.querySelectorAll('a')[0]; a_.click();  ";
                        }
                        else if (Str_Ptype == "Hog")
                        {
                            script8 = "  var div2 = document.getElementsByClassName('bootbox-body')[0];var a_= div2.querySelectorAll('a')[1]; a_.click();  ";
                        }
                        js8.ExecuteScript(script8);
                        step8 = 1;
                        ++stepcounter;
                    }




                }

                if (stepcounter == 8)
                {
                    stepcounter = 0;
                    loopcounter = 0;
                }

                while (!FindElement("FlexGrid", FindBy.ById))
                {
                    Thread.Sleep(2000);
                }

                while (stepcounter < 5)
                {
                    ++loopcounter;
                    if (loopcounter > 3)
                    {
                        throw new SomeException();
                    }
                    if (step9 == 0 && FindElement("CPC_urvRTCITMS_NationalCode", FindBy.ById))
                    {

                        IJavaScriptExecutor js9 = (IJavaScriptExecutor)driver;
                        string script9 = " var div = document.getElementsByClassName('navbar-form dropdown')[3];div.className = 'navbar-form dropdown open';";
                        script9 += "var txt = document.getElementById('CPC_urvRTCITMS_NationalCode');txt.value = '" + data + "';";
                        script9 += "var btn = div.querySelectorAll('button')[1];";
                        script9 += "btn.click(); ";
                        js9.ExecuteScript(script9);
                        step9 = 1;
                        ++stepcounter;

                    }

                    int rows_count = 0;
                    int loopcounter2 = 0;
                    while (step10 == 0)
                    {
                        ++loopcounter2;
                        if (loopcounter2 == 10)
                        {
                            throw new SomeException();
                        }
                        IJavaScriptExecutor js10_ = (IJavaScriptExecutor)driver;
                        long count_record = (long)js10_.ExecuteScript("return document.getElementById('FlexGrid').rows.length");
                        if (count_record > 5)
                        {
                            Thread.Sleep(2000);
                        }
                        else
                        {
                            rows_count = (int)count_record;
                            break;
                        }
                    }

                    if (rows_count == 1)
                    {
                        stepcounter = 5;
                        step10 = 1;
                        step11 = 1;
                        step12 = 1;
                        step13 = 1;
                        SendReportToFile(data + ">Not");

                        IJavaScriptExecutor js11_ = (IJavaScriptExecutor)driver;
                        string script11_ = "var btn2 = document.getElementsByClassName('btn btn-md btn-warning FlexGridHandler')[0];btn2.click();";
                        js11_.ExecuteScript(script11_);

                        Thread.Sleep(3000);

                        if (FindElement("modal-footer", FindBy.ByClass))
                        {
                            IJavaScriptExecutor js12 = (IJavaScriptExecutor)driver;
                            string script12 = "var div1 = document.getElementsByClassName('modal-footer')[0];var btn3 = div1.getElementsByClassName('btn btn-success')[0];btn3.click();";
                            js12.ExecuteScript(script12);
                        }

                        Thread.Sleep(4000);

                        if (FindElement("modal-footer", FindBy.ByClass))
                        {
                            IJavaScriptExecutor js13 = (IJavaScriptExecutor)driver;
                            string script13 = "var div1 = document.getElementsByClassName('modal-footer')[0];var btn3 = div1.getElementsByClassName('btn btn-info')[0];btn3.click();";
                            js13.ExecuteScript(script13);
                        }

                    }

                    if (step10 == 0 && FindElement("FlexGrid", FindBy.ById))
                    {
                        SendReportToFile(data + ">Done");

                        IJavaScriptExecutor js10 = (IJavaScriptExecutor)driver;
                        string script10 = " var tbl = document.getElementById('FlexGrid');"
                                        + "if (tbl.rows.length >= 2)"
                                        + "{"
                                        + "   var checkAll = document.getElementsByClassName('checkAllgrp')[0];"
                                        + "  checkAll.click();"
                                        + "}";
                        js10.ExecuteScript(script10);
                        step10 = 1;
                        ++stepcounter;
                    }

                    Thread.Sleep(2000);

                    if (step11 == 0 && FindElement("btn btn-md btn-info FlexGridHandler", FindBy.ByClass))
                    {
                        IJavaScriptExecutor js11 = (IJavaScriptExecutor)driver;
                        string script11 = "var btn2 = document.getElementsByClassName('btn btn-md btn-info FlexGridHandler')[0];btn2.click();";
                        js11.ExecuteScript(script11);
                        step11 = 1;
                        ++stepcounter;
                    }
                    Thread.Sleep(2000);

                    if (step12 == 0 && FindElement("modal-footer", FindBy.ByClass))
                    {
                        IJavaScriptExecutor js12 = (IJavaScriptExecutor)driver;
                        string script12 = "var div1 = document.getElementsByClassName('modal-footer')[0];var btn3 = div1.querySelectorAll('button')[0];btn3.click();";
                        js12.ExecuteScript(script12);
                        step12 = 1;
                        ++stepcounter;
                    }
                    Thread.Sleep(5000);

                    if (step13 == 0 && FindElement("modal-footer", FindBy.ByClass))
                    {
                        IJavaScriptExecutor js13 = (IJavaScriptExecutor)driver;
                        string script13 = "var div1 = document.getElementsByClassName('modal-footer')[0];var btn3 = div1.querySelectorAll('button')[0];btn3.click();";
                        js13.ExecuteScript(script13);
                        step13 = 1;
                        ++stepcounter;
                    }
                }
                if (stepcounter == 5)
                {
                    //DoStepProgress();
                    LoadItem();
                }
            }
            catch
            {
                IJavaScriptExecutor js13 = (IJavaScriptExecutor)driver;
                string script13 = " window.location.href='" + Str_Url + "'";
                js13.ExecuteScript(script13);
                Thread.Sleep(5000);
                //DoStepProgress();
                SendReportToFile(data + ">Done");
                LoadItem();
            }
        }

        private bool FindElement(string element,FindBy findBy)
        {
            int Counter = 5;
            IWebElement webElement;
            bool Result = false;

            for (int i = 0; i <= Counter; i++)
            {
                if (findBy.Equals(FindBy.ById))
                    try
                    {
                        webElement = driver.FindElement(By.Id(element));
                        Result = true;
                        break;
                    }
                    catch
                    {
                        Thread.Sleep(2000);
                    }
                else if (findBy.Equals(FindBy.ByClass))
                    try
                    {
                        element = "[class='" + element + "']";
                        webElement = driver.FindElement(By.CssSelector(element));
                        Result = true;
                        break;
                    }
                    catch
                    {
                        Thread.Sleep(2000);
                    }
            }
            return Result;
        }

        enum FindBy
        {
            ById,
            ByClass
        }



        delegate void SetPerformStep();
        public void DoStepProgress()
        {
            if (Prg_Attach.InvokeRequired)
            {
                SetPerformStep Stc_ = new SetPerformStep(DoStepProgress);
                this.Invoke(Stc_, new object[] {  });
            }
            else
            {
                Prg_Attach.PerformStep();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //openFileDialog1.ShowDialog();
            DialogResult result = openFileDialog1.ShowDialog();
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            openFileDialog1.InitialDirectory = "/documents/visual studio 2013/Projects/DNA/DNA.txt";
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All Files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;
            if (result == DialogResult.OK) // Test result.
            {
                string file = openFileDialog1.FileName;
                try
                {
                   
                        Txt_FileAddrress.Text = System.IO.Path.GetFullPath(file);
                    
                    //string text = File.ReadAllText(file);
                    //textBox1.Text = fbd.SelectedPath; ;
                    //Size = text.Length;
                }
                catch (IOException)
                {
                }
            }
            //Brows.Enabled = false;

        }

        private void Btn_Puse_Click(object sender, EventArgs e)
        {
            
        }
    }
}
