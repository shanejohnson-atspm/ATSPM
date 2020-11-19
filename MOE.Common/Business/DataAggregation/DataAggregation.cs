﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Caching;
using System.Web.UI.DataVisualization.Charting;
using System.Web.UI.WebControls;
using Microsoft.EntityFrameworkCore.Internal;
using MOE.Common.Business.PEDDelay;
using MOE.Common.Business.Speed;
using MOE.Common.Business.SplitFail;
using MOE.Common.Business.WCFServiceLibrary;
using MOE.Common.Models;
using MOE.Common.Models.Repositories;

namespace MOE.Common.Business.DataAggregation
{
    public class DataAggregation
    {
        private ConcurrentQueue<ApproachCycleAggregation> _approachCycleAggregationConcurrentQueue =
            new ConcurrentQueue<ApproachCycleAggregation>();

        private ConcurrentQueue<ApproachPcdAggregation> _approachPcdAggregationConcurrentQueue =
            new ConcurrentQueue<ApproachPcdAggregation>();

        private ConcurrentQueue<ApproachSpeedAggregation> _approachSpeedAggregationConcurrentQueue =
            new ConcurrentQueue<ApproachSpeedAggregation>();

        private ConcurrentQueue<ApproachSplitFailAggregation> _approachSplitFailAggregationConcurrentQueue =
            new ConcurrentQueue<ApproachSplitFailAggregation>();

        private ConcurrentQueue<ApproachYellowRedActivationAggregation>
            _approachYellowRedActivationAggregationConcurrentQueue =
                new ConcurrentQueue<ApproachYellowRedActivationAggregation>();

        private ConcurrentQueue<DetectorAggregation> _detectorAggregationConcurrentQueue =
            new ConcurrentQueue<DetectorAggregation>();

        private ConcurrentQueue<PreemptionAggregation> _preemptAggregationConcurrentQueue =
            new ConcurrentQueue<PreemptionAggregation>();

        private ConcurrentQueue<PriorityAggregation> _priorityAggregationConcurrentQueue =
            new ConcurrentQueue<PriorityAggregation>();

        private ConcurrentQueue<PhaseTerminationAggregation> _phaseTerminationAggregationQueue =
            new ConcurrentQueue<PhaseTerminationAggregation>();

        private ConcurrentQueue<PhasePedAggregation> _phasePedAggregations =
            new ConcurrentQueue<PhasePedAggregation>();

        private DateTime _endDate;
        private DateTime _startDate;
        private DateTime _endBackwardTime;
        private int _processDration;

        private bool _commandLineArgs;

        //private bool _moreProcessing;
        private int _numberOfRows;
        private DateTime _newTime;

        private ConcurrentQueue<SignalEventCountAggregation> _signalEventAggregationConcurrentQueue =
            new ConcurrentQueue<SignalEventCountAggregation>();

        private ConcurrentQueue<ApproachEventCountAggregation> _approachEventAggregationConcurrentQueue =
            new ConcurrentQueue<ApproachEventCountAggregation>();

        //private bool _allStop;
        private long _maxMemoryLimit;
        private DateTime _testDate;
        public int _binSize;

        public void StartAggregation(string[] args)
        {
            NameValueCollection appSettings = ConfigurationManager.AppSettings;
            _binSize = Convert.ToInt32(appSettings["BinSize"]);
            _processDration = Convert.ToInt32(appSettings["HoursForProcessing"]);
            _processDration = 60 * _processDration + _binSize;
            _endBackwardTime = Convert.ToDateTime(appSettings["EndBackwardTime"]);
            _maxMemoryLimit = Convert.ToInt64(appSettings["MaxMemoryLimit"]);
            _commandLineArgs = false;
            _newTime = DateTime.Now;
            var year = _newTime.Year;
            var month = _newTime.Month;
            var day = _newTime.Day;
            var hour = _newTime.Hour;
            _newTime = new DateTime(year, month, day, hour, 0, 0);
            //_newTime = new DateTime(2020, 3, 27, 0, 0, 0);

            var db = new SPM();
            SetStartEndDate(args);
            if (!_commandLineArgs)
            {
                _endDate = _startDate.AddMinutes(_processDration);
            }

            Console.WriteLine("Begining of Data Aggregation  " + _startDate.ToString("yyyy-MM-dd HH:mm"));
            ParallelOptions options =
                new ParallelOptions {MaxDegreeOfParallelism = Convert.ToInt32(appSettings["MaxThreads"])};
            List<Signal> signals = GetSignalVersionByDate(_startDate);
            List<Signal> nextSignals = new List<Signal>();
            while (_startDate > _endBackwardTime)
            {
                for (var i = 0; i < 8; i++)
                {
                    Console.WriteLine("Starting Aggregation:for {0} to {1} ",
                        _startDate.ToString("yyyy-MM-dd HH:mm"), _endDate.ToString("yyyy-MM-dd HH:mm"));
                    for (DateTime dt = _startDate; dt < _endDate; dt = dt.AddMinutes(_binSize))
                    {
                        if (nextSignals.Any())
                        {
                            signals = nextSignals;
                            nextSignals = new List<Signal>();
                        }

                        //List<Signal> signals = GetSignalVersionByDate(db, dt);
                        //Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +
                        //    " Lets Start!  Begin Aggregating Signals with clear collections when memory is high.");
                        Parallel.Invoke(
                            () =>
                            {
                                _testDate = dt.AddMinutes(3);
                                _numberOfRows = db.ApproachPcdAggregations.Count
                                    (t => t.BinStartTime >= dt && t.BinStartTime < _testDate);
                                if (_numberOfRows == 0 && InsertBinStartTimeInMoeStatus(dt))
                                {
                                    try
                                    {
                                        //Parallel.ForEach(signals, signal =>
                                        Parallel.ForEach(signals, options, signal =>
                                            //foreach (var signal in signals)
                                        {
                                            ProcessSignal(signal, dt, dt.AddMinutes(_binSize));
                                        });
                                        signals = new List<Signal>();
                                        GC.Collect();
                                        GC.WaitForPendingFinalizers();
                                    }
                                    catch (Exception e)
                                    {
                                        //ClearCollections(DateTime.Now);
                                        Console.WriteLine("Inside ProcessSignal Catch: " +
                                                          "was Processing Signals, an execption has occurred.");
                                        Console.WriteLine("e.TargetSite: " + e.TargetSite + " Message is: " +
                                                          e.Message);
                                    }
                                }
                            },
                            () => { nextSignals = GetSignalVersionByDate(dt.AddMinutes(_binSize)); });
                        _testDate = dt.AddMinutes(3);
                        _numberOfRows = db.ApproachPcdAggregations.Count
                            (t => t.BinStartTime >= dt && t.BinStartTime < _testDate);
                        if (_numberOfRows != 0)
                        {   ClearCollections(dt);
                            Console.WriteLine(" The data for {0}, is allready in the ApproachPcdAggregations table.  ", dt);
                            Console.WriteLine("CLearCollections.  Skipping bulk writing the data for all aggregations tables.");
                            UpdateNumberRowsInMoeStatus(dt, -999);
                            break;
                        }
                        Console.WriteLine(
                            "At {0}, the data for {1}, is being written to the database.",
                            DateTime.Now.ToString("HH:mm"), dt.ToString("MM-dd HH:mm"));
                        UpdateNumberRowsInMoeStatus(dt, _approachPcdAggregationConcurrentQueue.Count);
                        BulkSaveAllAggregateDataInParallel();
                    } // end of for dt loop to process data
                    if (_commandLineArgs)
                    {
                        ClearCollections(DateTime.Now);
                        break;
                    }
                    _startDate =  GetNextTime(_endDate);
                    _endDate = _startDate.AddMinutes(_processDration);
                    ClearCollections(DateTime.Now);
                } // end counter loop Repeating for this time and then check if anthing newer is available

                if (_commandLineArgs)
                {
                    ClearCollections(DateTime.Now);
                    break;
                }
                ClearCollections(DateTime.Now);
                _newTime = DateTime.Now;
                year = _newTime.Year;
                month = _newTime.Month;
                day = _newTime.Day;
                hour = _newTime.Hour;
                _newTime = new DateTime(year, month, day, hour, 0, 0);
                _startDate = GetNextTime(_newTime);
                _endDate = _startDate.AddMinutes(_processDration);
            } // end while loop
        }

        private bool IsThereBinStartTimeInMoeStatus(DateTime testTime)
        {
            bool foundData;
            var aggConnectionString =
                ConfigurationManager.ConnectionStrings["AggStatus"].ConnectionString;
            var aggStatus = new SqlConnection(aggConnectionString);
            string seeIfTimePresent = testTime.ToString("G");
            aggStatus.Open();
            {
                string sql = "Select BinStartTime From TimesAggregated Where BinStartTime = '" + seeIfTimePresent + "'";
                var command = new SqlCommand(sql, aggStatus);
                SqlDataReader dataReader = command.ExecuteReader();
                foundData = dataReader.HasRows;
            }
            aggStatus.Close();
            return foundData;
        }

        private bool InsertBinStartTimeInMoeStatus(DateTime binStartTime)
        {
            bool rowPresent = false;
            var aggConnectionString =
                ConfigurationManager.ConnectionStrings["AggStatus"].ConnectionString;
            var aggStatus = new SqlConnection(aggConnectionString);
            aggStatus.Open();
            string seeIfTimePresent = binStartTime.ToString("G");
            string sql = "Select BinStartTime From TimesAggregated Where BinStartTime = '"
                         + seeIfTimePresent + "'";
            var command = new SqlCommand(sql, aggStatus);
            SqlDataReader dataReader = command.ExecuteReader();
            rowPresent = dataReader.HasRows;
            if (!rowPresent)
            {
                sql = "Insert into TimesAggregated(BinStartTime, StartTime) values('"
                      + binStartTime.ToString("G") + "','" + DateTime.Now.ToString("G") + "')";
                command = new SqlCommand(sql, aggStatus);
                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.InsertCommand = new SqlCommand(sql, aggStatus);
                adapter.InsertCommand.ExecuteNonQuery();
            }

            command.Dispose();
            aggStatus.Close();
            return !rowPresent;
        }

        private void UpdateNumberRowsInMoeStatus(DateTime updateTime, int rowCount)
        {
            var aggConnectionString =
                ConfigurationManager.ConnectionStrings["AggStatus"].ConnectionString;
            var aggStatus = new SqlConnection(aggConnectionString);
            aggStatus.Open();
            string sql = "Update TimesAggregated set NumberRows = " + rowCount + " Where BinStartTime = '"
                         + updateTime.ToString("G") + "'";
            var command = new SqlCommand(sql, aggStatus);
            SqlDataAdapter adapter = new SqlDataAdapter();
            adapter.UpdateCommand = new SqlCommand(sql, aggStatus);
            adapter.UpdateCommand.ExecuteNonQuery();
            command.Dispose();
            aggStatus.Close();
            return;
        }

        private List<Signal> GetSignalVersionByDate(DateTime dt)
        {
            var db = new SPM();
            db.Configuration.LazyLoadingEnabled = false;
            //Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +
            //                  " > Getting the latest version of signals for time period: " + dt.ToString() + " end date " + _endDate);
            var versionIds = db.Signals.Where(
                    r => r.VersionActionId != 3 && r.Start < dt //&& (r.SignalID == "6723")
                ).GroupBy(r => r.SignalID).Select(g => g.OrderByDescending(r => r.Start).FirstOrDefault())
                .Select(s => s.VersionID).ToList();
            var signals = db.Signals.Where(signal => versionIds.Contains(signal.VersionID))
                .Include(signal => signal.Approaches.Select(a => a.Detectors.Select(d => d.DetectionTypes)))
                .Include(signal => signal.Approaches.Select(a =>
                    a.Detectors.Select(d => d.DetectionTypes.Select(det => det.MetricTypes))))
                .Include(signal => signal.Approaches.Select(a => a.Detectors.Select(d => d.DetectionHardware)))
                .Include(signal => signal.Approaches.Select(a => a.DirectionType))
                .OrderBy(signal => signal.SignalID).ToList();
            return signals;
        }

        private void ClearCollections(DateTime dt)
        {
            var memoryStartClearCollections = GC.GetTotalMemory(false);

            _approachCycleAggregationConcurrentQueue = new ConcurrentQueue<ApproachCycleAggregation>();
            _approachPcdAggregationConcurrentQueue = new ConcurrentQueue<ApproachPcdAggregation>();
            _approachSplitFailAggregationConcurrentQueue = new ConcurrentQueue<ApproachSplitFailAggregation>();
            _approachYellowRedActivationAggregationConcurrentQueue =
                new ConcurrentQueue<ApproachYellowRedActivationAggregation>();
            _approachSpeedAggregationConcurrentQueue = new ConcurrentQueue<ApproachSpeedAggregation>();
            _detectorAggregationConcurrentQueue = new ConcurrentQueue<DetectorAggregation>();
            _priorityAggregationConcurrentQueue = new ConcurrentQueue<PriorityAggregation>();
            _preemptAggregationConcurrentQueue = new ConcurrentQueue<PreemptionAggregation>();
            _signalEventAggregationConcurrentQueue = new ConcurrentQueue<SignalEventCountAggregation>();
            _phaseTerminationAggregationQueue = new ConcurrentQueue<PhaseTerminationAggregation>();
            _approachEventAggregationConcurrentQueue = new ConcurrentQueue<ApproachEventCountAggregation>();
            _phasePedAggregations = new ConcurrentQueue<PhasePedAggregation>();
            GC.Collect();
            var memoryEndClearCollections = GC.GetTotalMemory(false);
            var memoryInformation = dt + "Memory Limit is " + _maxMemoryLimit.ToString("0,0") +
                                    ". Memory at the start of clear collection " +
                                    memoryStartClearCollections.ToString("0,0") +
                                    ". Memory at the end of Clear collection " +
                                    memoryEndClearCollections.ToString("0,0");
            ////Console.WriteLine(memoryInformation);
            var errorLog = ApplicationEventRepositoryFactory.Create();
            errorLog.QuickAdd(System.Reflection.Assembly.GetExecutingAssembly().GetName().ToString(),
                this.GetType().DisplayName(), "clearCollections", ApplicationEvent.SeverityLevels.Medium,
                memoryInformation);
        }

        private void BulkSaveAllAggregateDataInParallel()
        {
            //Parallel.Invoke(
            //    () =>
            //    {
            if (_approachCycleAggregationConcurrentQueue.Count > 0)
            {
                Console.WriteLine(" 1.  Saving Approach Cycle Data to Database...");
                BulkSaveApproachCycleData();
            }

            //    },
            //    () =>
            //    {
            if (_approachPcdAggregationConcurrentQueue.Count > 0)
            {
                Console.WriteLine(" 2.  Saving Approach PCD Data to Database...");

                BulkSaveApproachPcdData();
            }

            //    },
            //    () =>
            //    {
            if (_approachSplitFailAggregationConcurrentQueue.Count > 0)
            {
                Console.WriteLine(" 3.  Saving Approach Split Fail Data to Database...");
                BulkSaveApproachSplitFailData();
            }

            //    },
            //    () =>
            //    {
            if (_approachYellowRedActivationAggregationConcurrentQueue.Count > 0)
            {
                Console.WriteLine(" 4.  Saving Approach Yellow Red Activations Data to Database...");
                BulkSaveApproachYellowRedActivationsData();
            }

            //},
            //    () =>
            //    {
            if (_approachSpeedAggregationConcurrentQueue.Count > 0)
            {
                Console.WriteLine(" 5.  Saving Approach Speed Data to Database...");
                BulkSaveApproachSpeedData();
            }

            //},
            //    () =>
            //    {
            if (_detectorAggregationConcurrentQueue.Count > 0)
            {
                Console.WriteLine(" 6.  Saving Detector Data to Database...");
                BulkSaveDetectorData();
            }

            //},
            //    () =>
            //    {
            if (_priorityAggregationConcurrentQueue.Count > 0)
            {
                Console.WriteLine(" 7.  Saving Priority Data to Database...");
                BulkSavePriorityData();
            }

            //    },
            //    () =>
            //    {
            if (_preemptAggregationConcurrentQueue.Count > 0)
            {
                Console.WriteLine(" 8.  Saving Preempt Data to Database...");
                BulkSavePreemptData();
            }

            //    },
            //    () =>
            //    {
            if (_signalEventAggregationConcurrentQueue.Count > 0)
            {
                Console.WriteLine(" 9.  Saving Signal Event Data to Database...");
                BulkSaveSignalEventData();
            }

            //    },
            //    () =>
            //    {
            if (_phaseTerminationAggregationQueue.Count > 0)
            {
                Console.WriteLine("10.  Saving Phase Termination Data to Database...");
                BulkSavePhaseTerminationData();
            }

            //    },
            //    () =>
            //    {
            if (_approachEventAggregationConcurrentQueue.Count > 0)
            {
                Console.WriteLine("11.  Saving Phase Event Data to Database...");
                BulkSavePhaseEventData();
            }

            //    },
            //    () =>
            //    {
            if (_phasePedAggregations.Count > -1)
            {
                Console.WriteLine("12.  Saving Ped Data to Database...");
                BulkSavePedData();
            }

            //    }
            //);
            ClearCollections(DateTime.Now);
        }

        private void BulkSavePhaseEventData()
            //  Name has bee changed!  sqlBulkCopy.DestinationTableName = "ApproachEventCountAggregations"
        {
            var eventAggregationTable = new DataTable();
            eventAggregationTable.Columns.Add(new DataColumn("BinStartTime", typeof(DateTime)));
            eventAggregationTable.Columns.Add(new DataColumn("ApproachId", typeof(int)));
            eventAggregationTable.Columns.Add(new DataColumn("EventCount", typeof(int)));
            eventAggregationTable.Columns.Add(new DataColumn("IsProtectedPhase", typeof(bool)));
            //eventAggregationTable.Columns.Add(new DataColumn("Id", typeof(int)));

            while (_approachEventAggregationConcurrentQueue.TryDequeue(out var preemptionAggregation))
            {
                var dataRow = eventAggregationTable.NewRow();
                dataRow["BinStartTime"] = preemptionAggregation.BinStartTime;
                dataRow["ApproachId"] = preemptionAggregation.ApproachId;
                dataRow["EventCount"] = preemptionAggregation.EventCount;
                dataRow["IsProtectedPhase"] = preemptionAggregation.IsProtectedPhase;
                eventAggregationTable.Rows.Add(dataRow);
            }

            var connectionString =
                ConfigurationManager.ConnectionStrings["SPM"].ConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                var sqlBulkCopy = new SqlBulkCopy(connectionString, SqlBulkCopyOptions.UseInternalTransaction);
                sqlBulkCopy.DestinationTableName = "ApproachEventCountAggregations";
                sqlBulkCopy.BulkCopyTimeout = 180;
                sqlBulkCopy.BatchSize = 50000;
                try
                {
                    connection.Open();
                    sqlBulkCopy.WriteToServer(eventAggregationTable);
                    connection.Close();
                }
                catch (Exception e)
                {
                    var errorLog = ApplicationEventRepositoryFactory.Create();
                    errorLog.QuickAdd(System.Reflection.Assembly.GetExecutingAssembly().GetName().ToString(),
                        this.GetType().DisplayName(), e.TargetSite.ToString(), ApplicationEvent.SeverityLevels.High,
                        e.Message);
                    throw new Exception("Unable to import Approach (Phase) Event Count Data");
                }
            }

            eventAggregationTable.Dispose();
        }

        private void BulkSaveSignalEventData()
        {
            var eventAggregationTable = new DataTable();
            eventAggregationTable.Columns.Add(new DataColumn("BinStartTime", typeof(DateTime)));
            eventAggregationTable.Columns.Add(new DataColumn("SignalId", typeof(string)));
            eventAggregationTable.Columns.Add(new DataColumn("EventCount", typeof(int)));
            eventAggregationTable.Columns.Add(new DataColumn("Id", typeof(int)));

            while (_signalEventAggregationConcurrentQueue.TryDequeue(out var preemptionAggregation))
            {
                var dataRow = eventAggregationTable.NewRow();
                dataRow["BinStartTime"] = preemptionAggregation.BinStartTime;
                dataRow["SignalId"] = preemptionAggregation.SignalId;
                dataRow["EventCount"] = preemptionAggregation.EventCount;
                eventAggregationTable.Rows.Add(dataRow);
            }

            var connectionString =
                ConfigurationManager.ConnectionStrings["SPM"].ConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                var sqlBulkCopy = new SqlBulkCopy(connectionString, SqlBulkCopyOptions.UseInternalTransaction);
                sqlBulkCopy.DestinationTableName = "SignalEventCountAggregations";
                sqlBulkCopy.BulkCopyTimeout = 180;
                sqlBulkCopy.BatchSize = 50000;
                try
                {
                    connection.Open();
                    sqlBulkCopy.WriteToServer(eventAggregationTable);
                    connection.Close();
                }
                catch (Exception e)
                {
                    var errorLog = ApplicationEventRepositoryFactory.Create();
                    errorLog.QuickAdd(System.Reflection.Assembly.GetExecutingAssembly().GetName().ToString(),
                        this.GetType().DisplayName(), e.TargetSite.ToString(), ApplicationEvent.SeverityLevels.High,
                        e.Message);
                    throw new Exception("Unable to import Signal Event Count Data");
                }
            }

            eventAggregationTable.Dispose();
        }

        private void BulkSavePreemptData()
        {
            var preemptAggregationTable = new DataTable();
            preemptAggregationTable.Columns.Add(new DataColumn("BinStartTime", typeof(DateTime)));
            preemptAggregationTable.Columns.Add(new DataColumn("SignalID", typeof(string)));
            preemptAggregationTable.Columns.Add(new DataColumn("PreemptNumber", typeof(int)));
            preemptAggregationTable.Columns.Add(new DataColumn("PreemptRequests", typeof(int)));
            preemptAggregationTable.Columns.Add(new DataColumn("PreemptServices", typeof(int)));
            preemptAggregationTable.Columns.Add(new DataColumn("VersionId", typeof(int)));
            preemptAggregationTable.Columns.Add(new DataColumn("Id", typeof(int)));

            while (_preemptAggregationConcurrentQueue.TryDequeue(out var preemptionAggregation))
            {
                var dataRow = preemptAggregationTable.NewRow();
                dataRow["BinStartTime"] = preemptionAggregation.BinStartTime;
                dataRow["SignalID"] = preemptionAggregation.SignalId;
                dataRow["PreemptNumber"] = preemptionAggregation.PreemptNumber;
                dataRow["PreemptRequests"] = preemptionAggregation.PreemptRequests;
                dataRow["PreemptServices"] = preemptionAggregation.PreemptServices;
                dataRow["VersionId"] = preemptionAggregation.VersionId;
                preemptAggregationTable.Rows.Add(dataRow);
            }

            var connectionString =
                ConfigurationManager.ConnectionStrings["SPM"].ConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                var sqlBulkCopy = new SqlBulkCopy(connectionString, SqlBulkCopyOptions.UseInternalTransaction);
                sqlBulkCopy.DestinationTableName = "PreemptionAggregations";
                sqlBulkCopy.BulkCopyTimeout = 180;
                sqlBulkCopy.BatchSize = 50000;
                try
                {
                    connection.Open();
                    sqlBulkCopy.WriteToServer(preemptAggregationTable);
                    connection.Close();
                }
                catch (Exception e)
                {
                    var errorLog = ApplicationEventRepositoryFactory.Create();
                    errorLog.QuickAdd(System.Reflection.Assembly.GetExecutingAssembly().GetName().ToString(),
                        this.GetType().DisplayName(), e.TargetSite.ToString(), ApplicationEvent.SeverityLevels.High,
                        e.Message);
                    throw new Exception("Unable to import Preempt data");
                }
            }

            preemptAggregationTable.Dispose();
        }

        private void BulkSavePriorityData()
        {
            var priorityAggregationTable = new DataTable();
            priorityAggregationTable.Columns.Add(new DataColumn("BinStartTime", typeof(DateTime)));
            priorityAggregationTable.Columns.Add(new DataColumn("SignalID", typeof(string)));
            priorityAggregationTable.Columns.Add(new DataColumn("PriorityNumber", typeof(int)));
            priorityAggregationTable.Columns.Add(new DataColumn("TotalCycles", typeof(int)));
            priorityAggregationTable.Columns.Add(new DataColumn("PriorityRequests", typeof(int)));
            priorityAggregationTable.Columns.Add(new DataColumn("PriorityServiceEarlyGreen", typeof(int)));
            priorityAggregationTable.Columns.Add(new DataColumn("PriorityServiceExtendedGreen", typeof(int)));
            priorityAggregationTable.Columns.Add(new DataColumn("VersionId", typeof(int)));
            priorityAggregationTable.Columns.Add(new DataColumn("Id", typeof(int)));

            while (_priorityAggregationConcurrentQueue.TryDequeue(out var priorityAggregationData))
            {
                var dataRow = priorityAggregationTable.NewRow();
                dataRow["BinStartTime"] = priorityAggregationData.BinStartTime;
                dataRow["SignalID"] = priorityAggregationData.SignalID;
                dataRow["PriorityNumber"] = priorityAggregationData.PriorityNumber;
                dataRow["TotalCycles"] = priorityAggregationData.TotalCycles;
                dataRow["PriorityRequests"] = priorityAggregationData.PriorityRequests;
                dataRow["PriorityServiceEarlyGreen"] = priorityAggregationData.PriorityServiceEarlyGreen;
                dataRow["PriorityServiceExtendedGreen"] = priorityAggregationData.PriorityServiceExtendedGreen;
                dataRow["VersionId"] = priorityAggregationData.VersionId;
                priorityAggregationTable.Rows.Add(dataRow);
            }

            var connectionString =
                ConfigurationManager.ConnectionStrings["SPM"].ConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                var sqlBulkCopy = new SqlBulkCopy(connectionString, SqlBulkCopyOptions.UseInternalTransaction);
                sqlBulkCopy.DestinationTableName = "PriorityAggregations";
                sqlBulkCopy.BulkCopyTimeout = 180;
                sqlBulkCopy.BatchSize = 50000;
                try
                {
                    connection.Open();
                    sqlBulkCopy.WriteToServer(priorityAggregationTable);
                    connection.Close();
                }
                catch (Exception e)
                {
                    var errorLog = ApplicationEventRepositoryFactory.Create();
                    errorLog.QuickAdd(System.Reflection.Assembly.GetExecutingAssembly().GetName().ToString(),
                        this.GetType().DisplayName(), e.TargetSite.ToString(), ApplicationEvent.SeverityLevels.High,
                        e.Message);
                    throw new Exception("Unable to import Priority Data");
                }
            }

            priorityAggregationTable.Dispose();
        }

        private void BulkSaveDetectorData()
        {
            var detectorAggregationTable = new DataTable();
            detectorAggregationTable.Columns.Add(new DataColumn("BinStartTime", typeof(DateTime)));
            detectorAggregationTable.Columns.Add(new DataColumn("Volume", typeof(double)));
            detectorAggregationTable.Columns.Add(new DataColumn("DetectorPrimaryId", typeof(string)));
            detectorAggregationTable.Columns.Add(new DataColumn("Id", typeof(long)));

            while (_detectorAggregationConcurrentQueue.TryDequeue(out var detectorAggregationData))
            {
                var dataRow = detectorAggregationTable.NewRow();
                dataRow["BinStartTime"] = detectorAggregationData.BinStartTime;
                dataRow["Volume"] = detectorAggregationData.Volume;
                dataRow["DetectorPrimaryId"] = detectorAggregationData.DetectorPrimaryId;
                detectorAggregationTable.Rows.Add(dataRow);
            }

            var connectionString =
                ConfigurationManager.ConnectionStrings["SPM"].ConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                var sqlBulkCopy = new SqlBulkCopy(connectionString, SqlBulkCopyOptions.UseInternalTransaction);
                sqlBulkCopy.DestinationTableName = "DetectorAggregations";
                sqlBulkCopy.BulkCopyTimeout = 180;
                sqlBulkCopy.BatchSize = 50000;
                try
                {
                    connection.Open();
                    sqlBulkCopy.WriteToServer(detectorAggregationTable);
                    connection.Close();
                }
                catch (Exception e)
                {
                    var errorLog = ApplicationEventRepositoryFactory.Create();
                    errorLog.QuickAdd(System.Reflection.Assembly.GetExecutingAssembly().GetName().ToString(),
                        this.GetType().DisplayName(), e.TargetSite.ToString(), ApplicationEvent.SeverityLevels.High,
                        e.Message);
                    throw new Exception("Unable to import Detector Data");
                }
            }

            detectorAggregationTable.Dispose();
        }

        private void BulkSaveApproachSpeedData()
        {
            var approachSpeedAggregationTable = new DataTable();
            approachSpeedAggregationTable.Columns.Add(new DataColumn("BinStartTime", typeof(DateTime)));
            approachSpeedAggregationTable.Columns.Add(new DataColumn("ApproachID", typeof(int)));
            approachSpeedAggregationTable.Columns.Add(new DataColumn("SummedSpeed", typeof(double)));
            approachSpeedAggregationTable.Columns.Add(new DataColumn("SpeedVolume", typeof(double)));
            approachSpeedAggregationTable.Columns.Add(new DataColumn("Speed85th", typeof(double)));
            approachSpeedAggregationTable.Columns.Add(new DataColumn("Speed15th", typeof(double)));
            approachSpeedAggregationTable.Columns.Add(new DataColumn("IsProtectedPhase", typeof(bool)));
            approachSpeedAggregationTable.Columns.Add(new DataColumn("Id", typeof(int)));

            while (_approachSpeedAggregationConcurrentQueue.TryDequeue(out var approachAggregationData))
            {
                var dataRow = approachSpeedAggregationTable.NewRow();
                dataRow["BinStartTime"] = approachAggregationData.BinStartTime;
                dataRow["ApproachID"] = approachAggregationData.ApproachId;
                dataRow["SummedSpeed"] = approachAggregationData.SummedSpeed;
                dataRow["SpeedVolume"] = approachAggregationData.SpeedVolume;
                dataRow["Speed85th"] = approachAggregationData.Speed85Th;
                dataRow["Speed15th"] = approachAggregationData.Speed15Th;
                dataRow["IsProtectedPhase"] = approachAggregationData.IsProtectedPhase;
                approachSpeedAggregationTable.Rows.Add(dataRow);
            }

            var connectionString =
                ConfigurationManager.ConnectionStrings["SPM"].ConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                var sqlBulkCopy = new SqlBulkCopy(connectionString, SqlBulkCopyOptions.UseInternalTransaction);
                sqlBulkCopy.DestinationTableName = "ApproachSpeedAggregations";
                sqlBulkCopy.BulkCopyTimeout = 180;
                sqlBulkCopy.BatchSize = 50000;
                try
                {
                    connection.Open();
                    sqlBulkCopy.WriteToServer(approachSpeedAggregationTable);
                    connection.Close();
                }
                catch (Exception e)
                {
                    var errorLog = ApplicationEventRepositoryFactory.Create();
                    errorLog.QuickAdd(System.Reflection.Assembly.GetExecutingAssembly().GetName().ToString(),
                        this.GetType().DisplayName(), e.TargetSite.ToString(), ApplicationEvent.SeverityLevels.High,
                        e.Message);
                    throw new Exception("Unable to import Approach Speed Data");
                }
            }

            approachSpeedAggregationTable.Dispose();
        }

        private void BulkSaveApproachCycleData()
        {
            var approachAggregationTable = new DataTable();
            approachAggregationTable.Columns.Add(new DataColumn("Id", typeof(int)));
            approachAggregationTable.Columns.Add(new DataColumn("BinStartTime", typeof(DateTime)));
            approachAggregationTable.Columns.Add(new DataColumn("ApproachID", typeof(int)));
            approachAggregationTable.Columns.Add(new DataColumn("RedTime", typeof(double)));
            approachAggregationTable.Columns.Add(new DataColumn("YellowTime", typeof(double)));
            approachAggregationTable.Columns.Add(new DataColumn("GreenTime", typeof(double)));
            approachAggregationTable.Columns.Add(new DataColumn("TotalCycles", typeof(int)));
            approachAggregationTable.Columns.Add(new DataColumn("PedActuations", typeof(int)));
            approachAggregationTable.Columns.Add(new DataColumn("IsProtectedPhase", typeof(bool)));
            while (_approachCycleAggregationConcurrentQueue.TryDequeue(out var approachAggregationData))
            {
                var dataRow = approachAggregationTable.NewRow();
                dataRow["BinStartTime"] = approachAggregationData.BinStartTime;
                dataRow["ApproachID"] = approachAggregationData.ApproachId;
                dataRow["RedTime"] = approachAggregationData.RedTime;
                dataRow["YellowTime"] = approachAggregationData.YellowTime;
                dataRow["GreenTime"] = approachAggregationData.GreenTime;
                dataRow["TotalCycles"] = approachAggregationData.TotalCycles;
                dataRow["PedActuations"] = approachAggregationData.PedActuations;
                dataRow["IsProtectedPhase"] = approachAggregationData.IsProtectedPhase;
                approachAggregationTable.Rows.Add(dataRow);
            }

            var connectionString =
                ConfigurationManager.ConnectionStrings["SPMImport"].ConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                var sqlBulkCopy = new SqlBulkCopy(connectionString, SqlBulkCopyOptions.UseInternalTransaction);
                sqlBulkCopy.DestinationTableName = "ApproachCycleAggregations";
                sqlBulkCopy.BulkCopyTimeout = 180;
                sqlBulkCopy.BatchSize = 50000;
                try
                {
                    connection.Open();
                    sqlBulkCopy.WriteToServer(approachAggregationTable);
                    connection.Close();
                }
                catch (Exception e)
                {
                    var errorLog = ApplicationEventRepositoryFactory.Create();
                    errorLog.QuickAdd(System.Reflection.Assembly.GetExecutingAssembly().GetName().ToString(),
                        this.GetType().DisplayName(), e.TargetSite.ToString(), ApplicationEvent.SeverityLevels.High,
                        e.Message);
                    throw new Exception("Unable to import Approach Cycle Data");
                }

            }

            approachAggregationTable.Dispose();
        }

        private void BulkSaveApproachPcdData()
        {
            var approachAggregationTable = new DataTable();
            //approachAggregationTable.Columns.Add(new DataColumn("Id", typeof(int)));
            approachAggregationTable.Columns.Add(new DataColumn("BinStartTime", typeof(DateTime)));
            approachAggregationTable.Columns.Add(new DataColumn("ApproachID", typeof(int)));
            approachAggregationTable.Columns.Add(new DataColumn("ArrivalsOnGreen", typeof(int)));
            approachAggregationTable.Columns.Add(new DataColumn("ArrivalsOnRed", typeof(int)));
            approachAggregationTable.Columns.Add(new DataColumn("ArrivalsOnYellow", typeof(int)));
            approachAggregationTable.Columns.Add(new DataColumn("IsProtectedPhase", typeof(bool)));
            approachAggregationTable.Columns.Add(new DataColumn("Volume", typeof(int)));
            while (_approachPcdAggregationConcurrentQueue.TryDequeue(out var approachAggregationData))
            {
                var dataRow = approachAggregationTable.NewRow();
                dataRow["BinStartTime"] = approachAggregationData.BinStartTime;
                dataRow["ApproachID"] = approachAggregationData.ApproachId;
                dataRow["ArrivalsOnGreen"] = approachAggregationData.ArrivalsOnGreen;
                dataRow["ArrivalsOnRed"] = approachAggregationData.ArrivalsOnRed;
                dataRow["ArrivalsOnYellow"] = approachAggregationData.ArrivalsOnYellow;
                dataRow["IsProtectedPhase"] = approachAggregationData.IsProtectedPhase;
                dataRow["Volume"] = approachAggregationData.Volume;
                approachAggregationTable.Rows.Add(dataRow);
            }

            var connectionString =
                ConfigurationManager.ConnectionStrings["SPM"].ConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                var sqlBulkCopy = new SqlBulkCopy(connectionString, SqlBulkCopyOptions.UseInternalTransaction);
                sqlBulkCopy.DestinationTableName = "ApproachPcdAggregations";
                sqlBulkCopy.BulkCopyTimeout = 180;
                sqlBulkCopy.BatchSize = 50000;
                try
                {
                    connection.Open();
                    sqlBulkCopy.WriteToServer(approachAggregationTable);
                    connection.Close();
                }
                catch (Exception e)
                {
                    var errorLog = ApplicationEventRepositoryFactory.Create();
                    errorLog.QuickAdd(System.Reflection.Assembly.GetExecutingAssembly().GetName().ToString(),
                        this.GetType().DisplayName(), e.TargetSite.ToString(), ApplicationEvent.SeverityLevels.High,
                        e.Message);
                    throw new Exception("Unable to import Approach PCD Data");
                }

            }

            approachAggregationTable.Dispose();
        }

        private void BulkSaveApproachSplitFailData()
        {
            var approachAggregationTable = new DataTable();
            approachAggregationTable.Columns.Add(new DataColumn("BinStartTime", typeof(DateTime)));
            approachAggregationTable.Columns.Add(new DataColumn("ApproachID", typeof(int)));
            approachAggregationTable.Columns.Add(new DataColumn("SplitFailures", typeof(int)));
            approachAggregationTable.Columns.Add(new DataColumn("IsProtectedPhase", typeof(bool)));
            approachAggregationTable.Columns.Add(new DataColumn("Id", typeof(int)));
            while (_approachSplitFailAggregationConcurrentQueue.TryDequeue(out var approachAggregationData))
            {
                var dataRow = approachAggregationTable.NewRow();
                dataRow["BinStartTime"] = approachAggregationData.BinStartTime;
                dataRow["ApproachID"] = approachAggregationData.ApproachId;
                dataRow["SplitFailures"] = approachAggregationData.SplitFailures;
                dataRow["IsProtectedPhase"] = approachAggregationData.IsProtectedPhase;
                approachAggregationTable.Rows.Add(dataRow);
            }

            var connectionString =
                ConfigurationManager.ConnectionStrings["SPM"].ConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                var sqlBulkCopy = new SqlBulkCopy(connectionString, SqlBulkCopyOptions.UseInternalTransaction);
                sqlBulkCopy.DestinationTableName = "ApproachSplitFailAggregations";
                sqlBulkCopy.BulkCopyTimeout = 180;
                sqlBulkCopy.BatchSize = 50000;
                try
                {
                    connection.Open();
                    sqlBulkCopy.WriteToServer(approachAggregationTable);
                    connection.Close();
                }
                catch (Exception e)
                {
                    var errorLog = ApplicationEventRepositoryFactory.Create();
                    errorLog.QuickAdd(System.Reflection.Assembly.GetExecutingAssembly().GetName().ToString(),
                        this.GetType().DisplayName(), e.TargetSite.ToString(), ApplicationEvent.SeverityLevels.High,
                        e.Message);
                    throw new Exception("Unable to import Approach Split Fail Data");
                }
            }

            approachAggregationTable.Dispose();
        }

        private void BulkSavePhaseTerminationData()
        {
            var phaseTerminationAggregationTable = new DataTable();
            phaseTerminationAggregationTable.Columns.Add(new DataColumn("BinStartTime", typeof(DateTime)));
            phaseTerminationAggregationTable.Columns.Add(new DataColumn("SignalId", typeof(string)));
            phaseTerminationAggregationTable.Columns.Add(new DataColumn("PhaseNumber", typeof(int)));
            phaseTerminationAggregationTable.Columns.Add(new DataColumn("GapOuts", typeof(int)));
            phaseTerminationAggregationTable.Columns.Add(new DataColumn("ForceOffs", typeof(int)));
            phaseTerminationAggregationTable.Columns.Add(new DataColumn("MaxOuts", typeof(int)));
            phaseTerminationAggregationTable.Columns.Add(new DataColumn("UnknownTerminationTypes", typeof(int)));
            phaseTerminationAggregationTable.Columns.Add(new DataColumn("Id", typeof(int)));
            while (_phaseTerminationAggregationQueue.TryDequeue(out var phaseTerminationAggregation))
            {
                var dataRow = phaseTerminationAggregationTable.NewRow();
                dataRow["BinStartTime"] = phaseTerminationAggregation.BinStartTime;
                dataRow["SignalId"] = phaseTerminationAggregation.SignalId;
                dataRow["PhaseNumber"] = phaseTerminationAggregation.PhaseNumber;
                dataRow["GapOuts"] = phaseTerminationAggregation.GapOuts;
                dataRow["ForceOffs"] = phaseTerminationAggregation.ForceOffs;
                dataRow["MaxOuts"] = phaseTerminationAggregation.MaxOuts;
                dataRow["UnknownTerminationTypes"] = phaseTerminationAggregation.UnknownTerminationTypes;
                phaseTerminationAggregationTable.Rows.Add(dataRow);
            }

            var connectionString =
                ConfigurationManager.ConnectionStrings["SPM"].ConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                var sqlBulkCopy = new SqlBulkCopy(connectionString, SqlBulkCopyOptions.UseInternalTransaction);
                sqlBulkCopy.DestinationTableName = "PhaseTerminationAggregations";
                sqlBulkCopy.BulkCopyTimeout = 180;
                sqlBulkCopy.BatchSize = 50000;
                try
                {
                    connection.Open();
                    sqlBulkCopy.WriteToServer(phaseTerminationAggregationTable);
                    connection.Close();
                }
                catch (Exception e)
                {
                    var errorLog = ApplicationEventRepositoryFactory.Create();
                    errorLog.QuickAdd(System.Reflection.Assembly.GetExecutingAssembly().GetName().ToString(),
                        this.GetType().DisplayName(), e.TargetSite.ToString(), ApplicationEvent.SeverityLevels.High,
                        e.Message);
                    throw new Exception("Unable to import Phase Termination Data");
                }
            }

            phaseTerminationAggregationTable.Dispose();
        }


        private void BulkSavePedData()
        {
            var phasePedAggregationTable = new DataTable();
            phasePedAggregationTable.Columns.Add(new DataColumn("BinStartTime", typeof(DateTime)));
            phasePedAggregationTable.Columns.Add(new DataColumn("SignalId", typeof(int)));
            phasePedAggregationTable.Columns.Add(new DataColumn("PhaseNumber", typeof(int)));
            phasePedAggregationTable.Columns.Add(new DataColumn("PedCount", typeof(int)));
            phasePedAggregationTable.Columns.Add(new DataColumn("PedDelay", typeof(int)));
            phasePedAggregationTable.Columns.Add(new DataColumn("Id", typeof(int)));
            while (_phasePedAggregations.TryDequeue(out var phaseTerminationAggregation))
            {
                var dataRow = phasePedAggregationTable.NewRow();
                dataRow["BinStartTime"] = phaseTerminationAggregation.BinStartTime;
                dataRow["SignalId"] = phaseTerminationAggregation.SignalId;
                dataRow["PhaseNumber"] = phaseTerminationAggregation.PhaseNumber;
                dataRow["PedDelay"] = phaseTerminationAggregation.PedDelay;
                dataRow["PedCount"] = phaseTerminationAggregation.PedCount;
                phasePedAggregationTable.Rows.Add(dataRow);
            }

            var connectionString =
                ConfigurationManager.ConnectionStrings["SPM"].ConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                var sqlBulkCopy = new SqlBulkCopy(connectionString, SqlBulkCopyOptions.UseInternalTransaction);
                sqlBulkCopy.DestinationTableName = "PhasePedAggregations";
                sqlBulkCopy.BulkCopyTimeout = 180;
                sqlBulkCopy.BatchSize = 50000;
                try
                {
                    connection.Open();
                    sqlBulkCopy.WriteToServer(phasePedAggregationTable);
                    connection.Close();
                }
                catch (Exception e)
                {
                    var errorLog = ApplicationEventRepositoryFactory.Create();
                    errorLog.QuickAdd(System.Reflection.Assembly.GetExecutingAssembly().GetName().ToString(),
                        this.GetType().DisplayName(), e.TargetSite.ToString(), ApplicationEvent.SeverityLevels.High,
                        e.Message);
                    throw new Exception("Unable to import Phase Ped Data");
                }
            }

            phasePedAggregationTable.Dispose();
        }

        private void BulkSaveApproachYellowRedActivationsData()
        {
            var approachAggregationTable = new DataTable();
            //approachAggregationTable.Columns.Add(new DataColumn("Id", typeof(int)));
            approachAggregationTable.Columns.Add(new DataColumn("BinStartTime", typeof(DateTime)));
            approachAggregationTable.Columns.Add(new DataColumn("ApproachID", typeof(int)));
            approachAggregationTable.Columns.Add(new DataColumn("SevereRedLightViolations", typeof(int)));
            approachAggregationTable.Columns.Add(new DataColumn("TotalRedLightViolations", typeof(int)));
            approachAggregationTable.Columns.Add(new DataColumn("IsProtectedPhase", typeof(bool)));
            while (_approachYellowRedActivationAggregationConcurrentQueue.TryDequeue(out var approachAggregationData))
            {
                var dataRow = approachAggregationTable.NewRow();
                dataRow["BinStartTime"] = approachAggregationData.BinStartTime;
                dataRow["ApproachID"] = approachAggregationData.ApproachId;
                dataRow["SevereRedLightViolations"] = approachAggregationData.SevereRedLightViolations;
                dataRow["TotalRedLightViolations"] = approachAggregationData.TotalRedLightViolations;
                dataRow["IsProtectedPhase"] = approachAggregationData.IsProtectedPhase;
                approachAggregationTable.Rows.Add(dataRow);
            }

            var connectionString =
                ConfigurationManager.ConnectionStrings["SPM"].ConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                var sqlBulkCopy = new SqlBulkCopy(connectionString, SqlBulkCopyOptions.UseInternalTransaction);
                sqlBulkCopy.DestinationTableName = "ApproachYellowRedActivationAggregations";
                sqlBulkCopy.BulkCopyTimeout = 180;
                sqlBulkCopy.BatchSize = 50000;
                try
                {
                    connection.Open();
                    sqlBulkCopy.WriteToServer(approachAggregationTable);
                    connection.Close();
                }
                catch (Exception e)
                {
                    var errorLog = ApplicationEventRepositoryFactory.Create();
                    errorLog.QuickAdd(System.Reflection.Assembly.GetExecutingAssembly().GetName().ToString(),
                        this.GetType().DisplayName(), e.TargetSite.ToString(), ApplicationEvent.SeverityLevels.High,
                        e.Message);
                    throw new Exception("Unable to import Approach Yellow and Red Acvtivation Data");
                }
            }

            approachAggregationTable.Dispose();
        }


        public void SetStartEndDate(string[] args)
        {
            _startDate = DateTime.Today;
            if (args.Length == 1)
            {
                _startDate = Convert.ToDateTime(args[0]);
                _endDate = _startDate.AddDays(1);
                _commandLineArgs = true;
            }
            else if (args.Length == 2)
            {
                _startDate = Convert.ToDateTime(args[0]);
                _endDate = Convert.ToDateTime(args[1]);
                _commandLineArgs = true;
            }
            else if (args.Length == 3)
            {
                var db = new SPM();
                _startDate = db.ApproachPcdAggregations.Select(s => s.BinStartTime).Max();
                _startDate = _startDate.AddMinutes(_binSize);
                int hoursBeforeCurrent = Convert.ToInt16(args[2]);
                _endDate = _newTime.AddHours(-hoursBeforeCurrent);
                _commandLineArgs = true;
            }
            else
            {
                var db = new SPM();
                _newTime = db.ApproachPcdAggregations.Select(s => s.BinStartTime).Max();
                _commandLineArgs = false;
                _startDate = GetNextTime(_newTime);
                _endDate = _startDate.AddMinutes(-_processDration);
            }
        }

        private DateTime GetNextTime(DateTime seekTime)
        {
            var db = new SPM();
            var notFoundNextTime = false;
            seekTime = seekTime.AddMinutes(-_processDration);
            do
            {
                seekTime = seekTime.AddMinutes(-_processDration );
                _testDate = seekTime.AddMinutes(3);
                _numberOfRows = db.ApproachPcdAggregations.Count
                    (t => t.BinStartTime >= seekTime && t.BinStartTime < _testDate);
                var dateInStatus = IsThereBinStartTimeInMoeStatus(seekTime);
                notFoundNextTime = !(_numberOfRows == 0 && !dateInStatus);
            } while (notFoundNextTime);
            return seekTime;
        }
    

    private void ProcessSignal(Models.Signal signal, DateTime startTime, DateTime endTime)
        {
            //var controllerEventLogRepository = ControllerEventLogRepositoryFactory.Create();
            //var records = controllerEventLogRepository.GetAllAggregationCodes(signal.SignalID, startTime, endTime);
            Console.Write(signal.SignalID + "    \r");
            //var preemptCodes = new List<int> { 102, 105 };
            //var priorityCodes = new List<int> { 112, 113, 114 };
            //Parallel.Invoke(
            //    () =>
            //    {
            //        int eventCount = controllerEventLogRepository.GetSignalEventsCountBetweenDates(signal.SignalID, startTime, endTime);
            //        _signalEventAggregationConcurrentQueue.Enqueue(new SignalEventCountAggregation
            //        {
            //            BinStartTime = startTime,
            //            EventCount = eventCount,
            //            SignalId = signal.SignalID
            //        });
            //    },
                //() =>
                //{
                //    AggregatePhaseTerminations(startTime, endTime, signal);
                //},
                //() =>
                //{
                //    AggregatePedDelay(startTime, endTime, signal);
                //},
                //() =>
                //{
                //    if (records.Count(r => preemptCodes.Contains(r.EventCode)) > 0)
                //        AggregatePreemptCodes(startTime, records, signal, preemptCodes);
                //},
                //() =>
                //{
                //    if (records.Count(r => priorityCodes.Contains(r.EventCode)) > 0)
                //        AggregatePriorityCodes(startTime, records, signal, priorityCodes);
                //},
                //() =>
                //{
                    if (signal.Approaches != null)
                        ProcessApproach(signal, startTime, endTime);
                //}
            //);
        }

        private void AggregatePedDelay(DateTime startTime, DateTime endTime, Models.Signal signal)
        {
            PedDelaySignal pedDelaySignal = new PedDelaySignal(signal, startTime, endTime);
            foreach (var pedPhase in pedDelaySignal.PedPhases)
            {
                PhasePedAggregation pedAggregation = new PhasePedAggregation
                {
                    SignalId = signal.SignalID,
                    PhaseNumber = pedPhase.PhaseNumber,
                    BinStartTime = startTime,
                    PedCount = pedPhase.Cycles.Count,
                    PedDelay = pedPhase.TotalDelay,
                };
                _phasePedAggregations.Enqueue(pedAggregation);
            }
        }

        private void AggregatePhaseTerminations(DateTime startTime, DateTime endTime, Models.Signal signal)
        {
            AnalysisPhaseCollection analysisPhaseCollection = new AnalysisPhaseCollection(signal.SignalID, startTime, endTime, 1);
            foreach (var analysisPhase in analysisPhaseCollection.Items)
            {
                PhaseTerminationAggregation phaseTerminationAggregation = new PhaseTerminationAggregation
                {
                    BinStartTime = startTime,
                    SignalId = signal.SignalID,
                    ForceOffs = analysisPhase.ConsecutiveForceOff.Count,
                    MaxOuts = analysisPhase.ConsecutiveMaxOut.Count,
                    GapOuts = analysisPhase.ConsecutiveGapOuts.Count,
                    PhaseNumber = analysisPhase.PhaseNumber,
                    UnknownTerminationTypes = analysisPhase.UnknownTermination.Count
                };
                _phaseTerminationAggregationQueue.Enqueue(phaseTerminationAggregation);
            }
        }

        private void ProcessApproach(Models.Signal signal, DateTime startTime, DateTime endTime)
        {

            if (signal.Approaches != null)
                //foreach (var signalApproach in signal.Approaches)
                //{
                //    if (signalApproach.Detectors != null && signalApproach.Detectors.Count > 0)
                //    {
                //        SetApproachSpeedAggregationData(startTime, endTime, signalApproach);
                //        SetApproachAggregationData(startTime, endTime, records, signalApproach);
                //        SetDetectorAggregationData(startTime, endTime, signalApproach);
                //    }
                //}
                Parallel.ForEach(signal.Approaches, signalApproach =>
                //   foreach (var signalApproach in signal.Approaches)
                {
                    if (signalApproach.Detectors != null && signalApproach.Detectors.Count > 0)
                         SetApproachAggregationData(startTime, endTime, signalApproach);
                        //Parallel.Invoke(
                        //    //() => { SetApproachSpeedAggregationData(startTime, endTime, signalApproach); },
                        //    () => { SetApproachAggregationData(startTime, endTime, records, signalApproach); }//,
                        //    //() => { SetDetectorAggregationData(startTime, endTime, signalApproach);
                        });

        }

        private void SetDetectorAggregationData(DateTime startTime, DateTime endTime, Approach signalApproach)
        {
            //Console.Write("\n-Aggregate Detector data ");
            //DateTime dt = DateTime.Now;
            //Parallel.ForEach(signalApproach.Detectors, detector =>
            foreach (var detector in signalApproach.Detectors)
            {
                var controllerEventLogRepository = ControllerEventLogRepositoryFactory.Create();
                var count = controllerEventLogRepository.GetDetectorActivationCount(signalApproach.SignalID, startTime,
                    endTime, detector.DetChannel);
                var detectorAggregation = new DetectorAggregation
                {
                    DetectorPrimaryId = detector.ID,
                    BinStartTime = startTime,
                    Volume = count
                };
                _detectorAggregationConcurrentQueue.Enqueue(detectorAggregation);
            }//);
            //Console.Write((DateTime.Now - dt).Milliseconds.ToString());
        }


        private void SetApproachAggregationData(DateTime startTime, DateTime endTime, Approach approach)
        {
            var signalPhase = new SignalPhase(startTime, endTime, approach, true, 15, 6, false);
            //var controllerEventLogRepository = ControllerEventLogRepositoryFactory.Create();
            //int eventCount =
            //    controllerEventLogRepository.GetApproachEventsCountBetweenDates(approach.ApproachID,
            //        startTime, endTime, approach.ProtectedPhaseNumber);
            //_approachEventAggregationConcurrentQueue.Enqueue(new ApproachEventCountAggregation
            //{
            //    BinStartTime = startTime,
            //    EventCount = eventCount,
            //    ApproachId = approach.ApproachID,
            //    IsProtectedPhase = true
            //});
            SetApproachCycleData(signalPhase, startTime, approach, false);
            SetApproachPcdData(signalPhase, startTime, approach, false);
            //SetSplitFailData(startTime, endTime, approach, false); 
            //SetYellowRedActivationData(startTime, endTime, approach, false); 
            //Parallel.Invoke(() => { SetApproachCycleData(signalPhase, startTime, approach, records, false); },
            //    () => { SetApproachPcdData(signalPhase, startTime, approach, false); }//,
            //() => { SetSplitFailData(startTime, endTime, approach, false); },
            //() => { SetYellowRedActivationData(startTime, endTime, approach, false); }
            // );
            if (approach.PermissivePhaseNumber != null && approach.PermissivePhaseNumber > 0)
            {
                //var permissiveEventCount =
                //    controllerEventLogRepository.GetApproachEventsCountBetweenDates(approach.ApproachID,
                //        startTime, endTime, (int)approach.PermissivePhaseNumber);
                var permissiveSignalPhase = new SignalPhase(startTime, endTime, approach, false, 15, 6, true);
                SetApproachCycleData(permissiveSignalPhase, startTime, approach, true);
                SetApproachPcdData(permissiveSignalPhase, startTime, approach, true);
                //Parallel.Invoke(
                //    () => { SetApproachCycleData(permissiveSignalPhase, startTime, approach, records, true); },
                //    () => { SetApproachPcdData(permissiveSignalPhase, startTime, approach, true); }//,
                //    //() => { SetSplitFailData(startTime, endTime, approach, true); },
                //    //() => { SetYellowRedActivationData(startTime, endTime, approach, true); }
                //    );
                //_approachEventAggregationConcurrentQueue.Enqueue(new ApproachEventCountAggregation
                //{
                //    BinStartTime = startTime,
                //    EventCount = permissiveEventCount,
                //    ApproachId = approach.ApproachID,
                //    IsProtectedPhase = false
                //});
            }

        }

        private void SetYellowRedActivationData(DateTime startTime, DateTime endTime, Approach approach,
            bool isPermissivePhase)
        {
            if (approach.GetDetectorsForMetricType(11).Any())
            {
                //Console.Write("\n-Aggregate Yellow Red data ");
                //DateTime dt = DateTime.Now;
                var options = new YellowAndRedOptions();
                options.SetDefaults();
                options.StartDate = startTime;
                options.EndDate = endTime;
                var yellowRedAcuationsPhase = new RLMSignalPhase(startTime, endTime, 15, options.SevereLevelSeconds,
                    approach, isPermissivePhase);
                _approachYellowRedActivationAggregationConcurrentQueue.Enqueue(
                    new ApproachYellowRedActivationAggregation
                    {
                        ApproachId = approach.ApproachID,
                        BinStartTime = startTime,
                        SevereRedLightViolations = Convert.ToInt32(yellowRedAcuationsPhase.SevereRedLightViolations),
                        TotalRedLightViolations = Convert.ToInt32(yellowRedAcuationsPhase.Violations),
                        IsProtectedPhase = !isPermissivePhase
                    });
                //Console.Write((DateTime.Now - dt).Milliseconds.ToString());
            }
        }

        private void SetApproachPcdData(SignalPhase signalPhase, DateTime startTime, Approach approach, bool isPermissivePhase)
        {
            if (approach.GetDetectorsForMetricType(6).Any())
                _approachPcdAggregationConcurrentQueue.Enqueue(new ApproachPcdAggregation
                {
                    ApproachId = approach.ApproachID,
                    ArrivalsOnGreen = Convert.ToInt32(signalPhase.TotalArrivalOnGreen),
                    ArrivalsOnRed = Convert.ToInt32(signalPhase.TotalArrivalOnRed),
                    ArrivalsOnYellow = Convert.ToInt32(signalPhase.TotalArrivalOnYellow),
                    Volume = Convert.ToInt32(signalPhase.TotalVolume),
                    BinStartTime = startTime,
                    IsProtectedPhase = !isPermissivePhase
                });
        }

        private void SetApproachCycleData(SignalPhase signalPhase, DateTime startTime, Approach approach, bool isPermissivePhase)
        {
            //Console.Write("\n-Aggregate Cycle data ");
            //DateTime dt = DateTime.Now;
            //var pedActuations = 0;
            //var totalCycles = 0;

            //if (isPermissivePhase)
            //{
            //    totalCycles = records.Count(r => r.EventCode == 1 && r.EventParam == approach.PermissivePhaseNumber);
            //}
            //else
            //{
            //    totalCycles = records.Count(r => r.EventCode == 1 && r.EventParam == approach.ProtectedPhaseNumber);
            //}
            var approachAggregation = new ApproachCycleAggregation
            {
                BinStartTime = startTime,
                ApproachId = approach.ApproachID,
                GreenTime = signalPhase.TotalGreenTime,
                RedTime = signalPhase.TotalRedTime,
                YellowTime = signalPhase.TotalYellowTime,
                PedActuations =  0,
                TotalCycles = signalPhase.Cycles.Count,// totalCycles,
                IsProtectedPhase = !isPermissivePhase
            };
            _approachCycleAggregationConcurrentQueue.Enqueue(approachAggregation);

            //Console.Write((DateTime.Now - dt).Milliseconds.ToString());
        }

        private void SetSplitFailData(DateTime startTime, DateTime endTime, Approach approach, bool getPermissivePhase)
        {
            if (!approach.GetDetectorsForMetricType(12).Any()) return;
            //Console.Write("\n-Aggregate Split Fail data ");
            //DateTime dt = DateTime.Now;
            var splitFailOptions = new SplitFailOptions
            {
                FirstSecondsOfRed = 5,
                StartDate = startTime,
                EndDate = endTime,
                MetricTypeID = 12
            };
            var splitFailPhase = new SplitFailPhase(approach, splitFailOptions, getPermissivePhase);
            _approachSplitFailAggregationConcurrentQueue.Enqueue(new ApproachSplitFailAggregation
            {
                ApproachId = approach.ApproachID,
                BinStartTime = startTime,
                SplitFailures = splitFailPhase.TotalFails,
                IsProtectedPhase = !getPermissivePhase
            });

            //Console.Write((DateTime.Now - dt).Milliseconds.ToString());
        }

        private void SetApproachSpeedAggregationData(DateTime startTime, DateTime endTime, Approach signalApproach)
        {
            var speedDetectors = signalApproach.GetDetectorsForMetricType(10);
            if (speedDetectors.Count > 0)
                foreach (var detector in speedDetectors)
                {
                    var detectorSpeed = new DetectorSpeed(detector, startTime, endTime, 15, false);
                    if (detectorSpeed.AvgSpeedBucketCollection.AvgSpeedBuckets.Any())
                    {
                        var speedBucket = detectorSpeed.AvgSpeedBucketCollection.AvgSpeedBuckets.FirstOrDefault();
                        var approachSpeedAggregation =
                            new ApproachSpeedAggregation
                            {
                                ApproachId = signalApproach.ApproachID,
                                BinStartTime = startTime,
                                Speed85Th = speedBucket.EightyFifth,
                                Speed15Th = speedBucket.FifteenthPercentile,
                                SpeedVolume = speedBucket.SpeedVolume,
                                SummedSpeed = speedBucket.SummedSpeed,
                                IsProtectedPhase = true
                            };
                        _approachSpeedAggregationConcurrentQueue.Enqueue(approachSpeedAggregation);
                    }
                }
        }

        private void AggregatePriorityCodes(DateTime startTime, List<Controller_Event_Log> records,
            Models.Signal signal, List<int> eventCodes)
        {
            for (var i = 0; i <= 10; i++)
                if (records.Count(r => r.EventParam == i && eventCodes.Contains(r.EventCode)) > 0)
                {
                    var priorityAggregation = new PriorityAggregation
                    {
                        SignalID = signal.SignalID,
                        VersionId = signal.VersionID,
                        BinStartTime = startTime,
                        PriorityNumber = i,
                        PriorityRequests = records.Count(r => r.EventCode == 112),
                        PriorityServiceEarlyGreen = records.Count(r => r.EventCode == 113),
                        PriorityServiceExtendedGreen = records.Count(r => r.EventCode == 114)
                    };
                    _priorityAggregationConcurrentQueue.Enqueue(priorityAggregation);
                }
        }

        private void AggregatePreemptCodes(DateTime startTime, List<Controller_Event_Log> records, Models.Signal signal,
            List<int> eventCodes)
        {
            for (var i = 0; i <= 10; i++)
                if (records.Count(r => r.EventParam == i && eventCodes.Contains(r.EventCode)) > 0)
                {
                    var preemptionAggregationData = new PreemptionAggregation
                    {
                        SignalId = signal.SignalID,
                        VersionId = signal.VersionID,
                        BinStartTime = startTime,
                        PreemptNumber = i,
                        PreemptRequests = records.Count(r => r.EventCode == 102),
                        PreemptServices = records.Count(r => r.EventCode == 105)
                    };
                    _preemptAggregationConcurrentQueue.Enqueue(preemptionAggregationData);
                }
        }
    }
}