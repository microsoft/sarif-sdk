// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    /// <summary>
    /// Contains the information needed to resolve a reportingDescriptorReference to the
    /// reportingDescriptor to which it refers, at the current point in the object model visit.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Several objects in the SARIF object model contain one or more properties whose values
    /// are reportingDescriptorReference objects. A reportingDescriptorReference object
    /// designates a particular reportingDescriptor object within a particular toolComponent
    /// object. Depending on which reportingDescriptorReference-valued property is being
    /// visited, the designated toolComponent might be theTool.driver, an element of
    /// theTool.extensions[], or an element of theRun.taxonomies[]. (There is no
    /// reportingDescriptorReference-valued property in the SARIF specification that can
    /// validly specify an element of theRun.translations[] or theRun.policies[], even though
    /// those arrays do contain toolComponent objects.) Within the designated toolComponent,
    /// again depending on which property is being visited, the reportingDescriptorReference
    /// object might designate a reportingDescriptorReference that is a member of
    /// toolComponent.rules[], toolComponent.notifications[], or toolComponent.taxa[].
    /// </para>
    /// <para>
    /// The validator (specifically, rule SARIF1017) uses the information in the
    /// ReportingDescriptorReference object to validate the index property of a
    /// reportingDescriptorReference object against the appropriate array within the
    /// appropriate toolComponent object.
    /// </para>
    internal class ReportingDescriptorReferenceContext : IDisposable
    {
        private readonly ReportingDescriptorReferenceKinds _previousReportingDescriptorReferenceKinds;
        private readonly SarifValidationContext _context;

        internal ReportingDescriptorReferenceContext(SarifValidationContext context, ReportingDescriptorReferenceKinds currentReportingDescriptorReferenceKinds)
        {
            _previousReportingDescriptorReferenceKinds = context.CurrentReportingDescriptorReferenceKinds;
            _context = context;
            _context.CurrentReportingDescriptorReferenceKinds = currentReportingDescriptorReferenceKinds;
        }

        public void Dispose()
        {
            _context.CurrentReportingDescriptorReferenceKinds = _previousReportingDescriptorReferenceKinds;
        }
    }
}