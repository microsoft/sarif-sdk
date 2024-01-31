namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ADOProvideRequiredReportingDescriptorProperties
        : BaseProvideRequiredResultProperties
    {
        /// <summary>
        /// ADO2012
        /// </summary>
        public override string Id => RuleId.ADOProvideRequiredReportingDescriptorProperties;

        protected override string ServiceName => RuleResources.ServiceName_ADO;

        public ADOProvideRequiredReportingDescriptorProperties()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
        }

        protected override void Analyze(ReportingDescriptor reportingDescriptor, string reportingDescriptorPointer)
        {
            base.Analyze(reportingDescriptor, reportingDescriptorPointer);

            if (reportingDescriptor != null && string.IsNullOrWhiteSpace(reportingDescriptor.name))
            {
                // {0}: This 'reportingDescriptor' object does not provide a 'name' value. This property is required by the {1} service.
                LogResult(
                    reportingDescriptorPointer,
                    nameof(RuleResources.ADO2012_ProvideRequiredResultProperties_Error_MissingName),
                    this.ServiceName);
            }
        }
    }
}
