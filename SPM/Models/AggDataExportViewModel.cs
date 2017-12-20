﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Eventing.Reader;
using MOE.Common.Models.Repositories;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using MOE.Common.Models;
using MOE.Common.Models.ViewModel.Chart;

namespace SPM.Models
{
    public class AggDataExportViewModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public virtual List<int> MetricTypeIDs { get; set; }
        public virtual ICollection<MetricType> AllMetricTypes { get; set; }
        public virtual List<int> ApproachTypeIDs { get; set; }
        public virtual ICollection<DirectionType> AllApproachTypes { get; set; }
        public virtual List<int> MovementTypeIDs { get; set; }
        public virtual ICollection<MovementType> AllMovementTypes { get; set; }
        public virtual List<int> LaneTypeIDs { get; set; }
        public virtual ICollection<LaneType> AllLaneTypes { get; set; }
        //public List<SelectListItem> AggregateMetricsList { get; set; }

        public bool Weekdays { get; set; }
        public bool Weekends { get; set; }
        public bool IsSum { get; set; }
        public List<string> WeekdayWeekendIDs { get; set; }
        public ICollection<string> SelectedWeekdayWeekend { get; set; }
        public string DataAggregationType { get; set; }
        public List<Route> Routes { get; set; }
        public int SelectedRouteId { get; set; }

        public SignalSearchViewModel SignalSearchViewModel { get; set; }

        //[Required]
        //[Display(Name = "Signal ID")]
        //public string SignalId { get; set; }
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Start Date")]
        public DateTime StartDateDay { get; set; }
        [Required]
        [Display(Name = "Start Time")]
        public string StartTime { get; set; }
        [Required]
        [Display(Name = "Start AM/PM")]
        public string SelectedStartAMPM { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "End Date")]
        public DateTime EndDateDay { get; set; }
        [Required]
        [Display(Name = "End Time")]
        public string EndTime { get; set; }
        [Required]
        [Display(Name = "End AM/PM")]
        public string SelectedEndAMPM { get; set; }

        public List<SelectListItem> StartAMPMList { get; set; }
        public List<SelectListItem> EndAMPMList { get; set; }

        [Required]
        [DataMember]
        [Display(Name = "Volume Bin Size")]
        public string SelectedBinSize { get; set; }
        [DataMember]
        public List<string> BinSizeList { get; set; }

        public int? Count { get; set; }

        private IMetricTypeRepository _metricRepository;
        public AggDataExportViewModel()
        {
            _metricRepository = MetricTypeRepositoryFactory.Create();
            var regionRepositry = MOE.Common.Models.Repositories.RegionsRepositoryFactory.Create();
            GetMetrics(_metricRepository);
            SignalSearchViewModel = new MOE.Common.Models.ViewModel.Chart.SignalSearchViewModel(regionRepositry, _metricRepository);
            SetDefaultDates();
            SetBinSizeList();
        }

        protected void SetBinSizeList()
        {
            BinSizeList = new List<string>();
            BinSizeList.Add("15 minutes");
            BinSizeList.Add("1 hour");
            BinSizeList.Add("1 day");
            BinSizeList.Add("1 week");
            BinSizeList.Add("1 month");
            BinSizeList.Add("1 year");
        }

        protected void SetDefaultDates()
        {
            StartDateDay = DateTime.Today;
            EndDateDay = DateTime.Today;
            StartTime = "12:00";
            EndTime = "11:59";
            StartAMPMList = new List<SelectListItem>();
            StartAMPMList.Add(new SelectListItem { Value = "AM", Text = "AM", Selected = true });
            StartAMPMList.Add(new SelectListItem { Value = "PM", Text = "PM" });
            EndAMPMList = new List<SelectListItem>();
            EndAMPMList.Add(new SelectListItem { Value = "AM", Text = "AM" });
            EndAMPMList.Add(new SelectListItem { Value = "PM", Text = "PM", Selected = true });
        }
        public void GetMetrics(IMetricTypeRepository metricRepository)
        {
            List<MOE.Common.Models.MetricType> metricTypes = metricRepository.GetAllToDisplayMetrics();
            //TODO: change metricTypes to aggregateMetricTypes after Shane's check-in
            //AggregateMetricsList = new List<SelectListItem>();
            //foreach (MOE.Common.Models.MetricType m in metricTypes)
            //{
            //    AggregateMetricsList.Add(new SelectListItem { Value = m.MetricID.ToString(), Text = m.ChartName });
            //}
        }

    }
}