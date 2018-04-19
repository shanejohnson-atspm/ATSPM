﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.DataVisualization.Charting;
using MOE.Common.Business.SplitFail;
using MOE.Common.Models;
using MOE.Common.Models.Repositories;

namespace MOE.Common.Business.WCFServiceLibrary
{
    [DataContract]
    public class SplitFailOptions : MetricOptions
    {
        public SplitFailOptions(string signalID, DateTime startDate, DateTime endDate,
            int metricTypeID, int firstSecondsOfRed, bool showFailLines, bool showAvgLines, bool showPercentFailLine)
        {
            SignalID = signalID;
            StartDate = startDate;
            EndDate = endDate;
            MetricTypeID = metricTypeID;
            FirstSecondsOfRed = firstSecondsOfRed;
            ShowFailLines = showFailLines;
            ShowAvgLines = showAvgLines;
            ShowPercentFailLines = showPercentFailLine;
        }

        public SplitFailOptions()
        {
            MetricTypeID = 12;
            SetDefaults();
        }

        [Required]
        [DataMember]
        [Display(Name = "First Seconds Of Red")]
        public int FirstSecondsOfRed { get; set; }

        [DataMember]
        [Display(Name = "Show Fail Lines")]
        public bool ShowFailLines { get; set; }

        [DataMember]
        [Display(Name = "Show Average Lines")]
        public bool ShowAvgLines { get; set; }

        [DataMember]
        [Display(Name = "Show Percent Fail Lines")]
        public bool ShowPercentFailLines { get; set; }

        public void SetDefaults()
        {
            FirstSecondsOfRed = 5;
            ShowFailLines = true;
            ShowAvgLines = true;
            ShowPercentFailLines = false;
        }

        public override List<string> CreateMetric()
        {
            base.CreateMetric();
            EndDate = EndDate.AddSeconds(59);
            var returnString = new List<string>();
            var sr = SignalsRepositoryFactory.Create();
            var signal = sr.GetVersionOfSignalByDate(SignalID, StartDate);
            var metricApproaches = signal.GetApproachesForSignalThatSupportMetric(MetricTypeID);
            if (metricApproaches.Count > 0)
                //Parallel.ForEach(metricApproaches, approach =>
                foreach (Approach approach in metricApproaches)
                    {
                        if (approach.ProtectedPhaseNumber > 0)
                        {
                            var splitFailPhase = new SplitFailPhase(approach, this, false);
                            var chartName = CreateFileName();
                            GetChart(splitFailPhase, chartName, returnString, false, approach);
                        }
                        if (approach.PermissivePhaseNumber != null && approach.PermissivePhaseNumber > 0)
                        {
                            
                            var splitFailPermissivePhase = new SplitFailPhase(approach, this, true);
                            var permChartName = CreateFileName();
                            GetChart(splitFailPermissivePhase, permChartName, returnString, true, approach);
                        }
                    }
                //);
            return returnString;
        }

        private void GetChart(SplitFailPhase splitFailPhase, string chartName, List<string> returnString,
            bool getPermissivePhase, Approach approach)
        {
            var sfChart = new SplitFailChart(this, splitFailPhase, getPermissivePhase);
            var detector = approach.GetDetectorsForMetricType(12).FirstOrDefault();
            if (detector != null)
            {
                var direction = detector.MovementType.Description;
                if (getPermissivePhase)
                {
                    sfChart.Chart.BackColor = Color.LightGray;
                }
            }
            Thread.Sleep(300);
            chartName = chartName.Replace(".", approach.DirectionType.Description + ".");
            try
            {
                sfChart.Chart.SaveImage(MetricFileLocation + chartName, ChartImageFormat.Jpeg);
            }
            catch (Exception ex)
            {
                try
                {
                    sfChart.Chart.SaveImage(MetricFileLocation + chartName, ChartImageFormat.Jpeg);
                }
                catch
                {
                    var appEventRepository =
                        ApplicationEventRepositoryFactory.Create();
                    var applicationEvent = new ApplicationEvent();
                    applicationEvent.ApplicationName = "SPM Website";
                    applicationEvent.Description = MetricType.ChartName + ex.Message + " Failed While Saving File";
                    applicationEvent.SeverityLevel = ApplicationEvent.SeverityLevels.Medium;
                    applicationEvent.Timestamp = DateTime.Now;
                    appEventRepository.Add(applicationEvent);
                }
            }
            returnString.Add(MetricWebPath + chartName);
        }
    }
}