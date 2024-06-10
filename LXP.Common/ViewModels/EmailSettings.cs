using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LXP.Common.ViewModels
{
    public class EmailSettings
    {
        public string SenderEmail { get; set; }
        public string SenderPassword { get; set; }
        public string GmailSmtpServer { get; set; }
        public int GmailSmtpPort { get; set; }
        public string OutlookSmtpServer { get; set; }
        public int OutlookSmtpPort { get; set; }
    }
    //public class EmailSettings
    //{
    //    public string SenderEmail { get; set; }
    //    public string SenderPassword { get; set; }
    //}
}
