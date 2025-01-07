﻿namespace ServicePlusAPIs.ReportsViewModel
{
    public class ConsolidatePendencyReport
    {
        public string ServiceNane
        {
            get;
            set;
        }
        public int ApplicationRecieved
        {
            get;
            set;
        }
        public int Deliverd
        {
            get;
            set;
        }
        public int Rejected
        {
            get;
            set;
        }
        public int InProcess
        {
            get;
            set;
        }
        public int Day1to5
        {
            get;
            set;
        }
        public int Day6to30
        {
            get;
            set;
        }
        public int Day31to60
        {
            get;
            set;
        }
        public int Day61to90
        {
            get;
            set;
        }
        public int Day91toAbove
        {
            get;
            set;
        }
        public int TotalPendingDays
        {
            get;
            set;
        }
        public int SendBack
        {
            get;
            set;
        }
        public double PendencyPercentage
        {
            get;
            set;
        }
    }
}
