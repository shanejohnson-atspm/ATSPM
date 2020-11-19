﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SPM.MetricGeneratorService {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="MetricGeneratorService.IMetricGenerator")]
    public interface IMetricGenerator {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMetricGenerator/CreateMetric", ReplyAction="http://tempuri.org/IMetricGenerator/CreateMetricResponse")]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.PCDOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.TMCOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.AoROptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.ApproachDelayOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.PhaseTerminationOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.PreemptDetailOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.PreemptServiceMetricOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.PreemptServiceRequestOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.YellowAndRedOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.ApproachSpeedOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.SplitFailOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.SplitMonitorOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.PedDelayOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.ApproachVolumeOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.SignalAggregationMetricOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.ApproachAggregationMetricOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.ApproachSplitFailAggregationOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.ApproachPcdAggregationOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.ApproachYellowRedActivationsAggregationOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.DetectorAggregationMetricOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.DetectorVolumeAggregationOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.ApproachSpeedAggregationOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.ApproachCycleAggregationOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.ApproachEventCountAggregationOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.SignalPreemptionAggregationOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.SignalPriorityAggregationOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.SignalEventCountAggregationOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.PhaseAggregationMetricOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.PhaseTerminationAggregationOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.PhasePedAggregationOptions))]
        string[] CreateMetric(MOE.Common.Business.WCFServiceLibrary.MetricOptions options);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMetricGenerator/CreateMetric", ReplyAction="http://tempuri.org/IMetricGenerator/CreateMetricResponse")]
        System.Threading.Tasks.Task<string[]> CreateMetricAsync(MOE.Common.Business.WCFServiceLibrary.MetricOptions options);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMetricGenerator/GetChartAndXmlFileLocations", ReplyAction="http://tempuri.org/IMetricGenerator/GetChartAndXmlFileLocationsResponse")]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.PCDOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.TMCOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.AoROptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.ApproachDelayOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.PhaseTerminationOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.PreemptDetailOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.PreemptServiceMetricOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.PreemptServiceRequestOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.YellowAndRedOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.ApproachSpeedOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.SplitFailOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.SplitMonitorOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.PedDelayOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.ApproachVolumeOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.SignalAggregationMetricOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.ApproachAggregationMetricOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.ApproachSplitFailAggregationOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.ApproachPcdAggregationOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.ApproachYellowRedActivationsAggregationOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.DetectorAggregationMetricOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.DetectorVolumeAggregationOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.ApproachSpeedAggregationOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.ApproachCycleAggregationOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.ApproachEventCountAggregationOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.SignalPreemptionAggregationOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.SignalPriorityAggregationOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.SignalEventCountAggregationOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.PhaseAggregationMetricOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.PhaseTerminationAggregationOptions))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MOE.Common.Business.WCFServiceLibrary.PhasePedAggregationOptions))]
        System.Tuple<string, string>[] GetChartAndXmlFileLocations(MOE.Common.Business.WCFServiceLibrary.MetricOptions options);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMetricGenerator/GetChartAndXmlFileLocations", ReplyAction="http://tempuri.org/IMetricGenerator/GetChartAndXmlFileLocationsResponse")]
        System.Threading.Tasks.Task<System.Tuple<string, string>[]> GetChartAndXmlFileLocationsAsync(MOE.Common.Business.WCFServiceLibrary.MetricOptions options);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMetricGenerator/CreateMetricWithDataTable", ReplyAction="http://tempuri.org/IMetricGenerator/CreateMetricWithDataTableResponse")]
        MOE.Common.Business.ApproachVolume.MetricInfo[] CreateMetricWithDataTable(MOE.Common.Business.WCFServiceLibrary.ApproachVolumeOptions options);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMetricGenerator/CreateMetricWithDataTable", ReplyAction="http://tempuri.org/IMetricGenerator/CreateMetricWithDataTableResponse")]
        System.Threading.Tasks.Task<MOE.Common.Business.ApproachVolume.MetricInfo[]> CreateMetricWithDataTableAsync(MOE.Common.Business.WCFServiceLibrary.ApproachVolumeOptions options);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMetricGenerator/CreateTMCChart", ReplyAction="http://tempuri.org/IMetricGenerator/CreateTMCChartResponse")]
        MOE.Common.Business.TMC.TMCInfo CreateTMCChart(MOE.Common.Business.WCFServiceLibrary.TMCOptions options);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMetricGenerator/CreateTMCChart", ReplyAction="http://tempuri.org/IMetricGenerator/CreateTMCChartResponse")]
        System.Threading.Tasks.Task<MOE.Common.Business.TMC.TMCInfo> CreateTMCChartAsync(MOE.Common.Business.WCFServiceLibrary.TMCOptions options);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IMetricGeneratorChannel : SPM.MetricGeneratorService.IMetricGenerator, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class MetricGeneratorClient : System.ServiceModel.ClientBase<SPM.MetricGeneratorService.IMetricGenerator>, SPM.MetricGeneratorService.IMetricGenerator {
        
        public MetricGeneratorClient() {
        }
        
        public MetricGeneratorClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public MetricGeneratorClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public MetricGeneratorClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public MetricGeneratorClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public string[] CreateMetric(MOE.Common.Business.WCFServiceLibrary.MetricOptions options) {
            return base.Channel.CreateMetric(options);
        }
        
        public System.Threading.Tasks.Task<string[]> CreateMetricAsync(MOE.Common.Business.WCFServiceLibrary.MetricOptions options) {
            return base.Channel.CreateMetricAsync(options);
        }
        
        public System.Tuple<string, string>[] GetChartAndXmlFileLocations(MOE.Common.Business.WCFServiceLibrary.MetricOptions options) {
            return base.Channel.GetChartAndXmlFileLocations(options);
        }
        
        public System.Threading.Tasks.Task<System.Tuple<string, string>[]> GetChartAndXmlFileLocationsAsync(MOE.Common.Business.WCFServiceLibrary.MetricOptions options) {
            return base.Channel.GetChartAndXmlFileLocationsAsync(options);
        }
        
        public MOE.Common.Business.ApproachVolume.MetricInfo[] CreateMetricWithDataTable(MOE.Common.Business.WCFServiceLibrary.ApproachVolumeOptions options) {
            return base.Channel.CreateMetricWithDataTable(options);
        }
        
        public System.Threading.Tasks.Task<MOE.Common.Business.ApproachVolume.MetricInfo[]> CreateMetricWithDataTableAsync(MOE.Common.Business.WCFServiceLibrary.ApproachVolumeOptions options) {
            return base.Channel.CreateMetricWithDataTableAsync(options);
        }
        
        public MOE.Common.Business.TMC.TMCInfo CreateTMCChart(MOE.Common.Business.WCFServiceLibrary.TMCOptions options) {
            return base.Channel.CreateTMCChart(options);
        }
        
        public System.Threading.Tasks.Task<MOE.Common.Business.TMC.TMCInfo> CreateTMCChartAsync(MOE.Common.Business.WCFServiceLibrary.TMCOptions options) {
            return base.Channel.CreateTMCChartAsync(options);
        }
    }
}