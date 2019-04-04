// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public static class ContrastSecurityRuleIds
    {
        public const string AntiCachingControlsMissing = "cache-controls-missing";
        public const string ArbitraryServerSideForwards = "unvalidated forward";
        public const string AuthorizationRulesMisordered = "authorization-rules-misordered";
        public const string AuthorizationRulesMissingDenyRule = "authorization-missing-deny";
        public const string CacheControlHeaderDisabled = "cache-control-disabled";
        public const string CrossSiteScripting = "reflected-xss";
        public const string DetailedErrorMessagesDisplayed = "custom-errors-off";
        public const string EventValidationDisabled = "event-validation-disabled";
        public const string ExpiredSessionIDsNotRegenerated = "session-regenerate";
        public const string FormsAuthenticationCrossAppRedirect = "forms-auth-redirect";
        public const string FormsAuthenticationProtectionMode = "forms-auth-protection";
        public const string FormsAuthenticationSSL = "forms-auth-ssl";
        public const string FormsWithoutAutocompletePrevention = "autocomplete-missing";
        public const string HeaderCheckingDisabled = "header-checking-disabled";
        public const string HeaderInjection = "header-injection";
        public const string HttpOnlyCookieFlagDisabled = "http-only-disabled";
        public const string InsecureAuthenticationProtocol = "insecure-auth-protocol";
        public const string InsecureEncryptionAlgorithms = "crypto-bad-cyphers";
        public const string InsecureHashAlgorithms = "crypto-bad-mac";
        public const string LargeMaxRequestLength = "max-request-length";
        public const string LdapInjection = "ldap-injection";
        public const string LogInjection = "log-injection";
        public const string OSCommandInjection = "cmd-injection";
        public const string OverlyLongSessionTimeout = "session-timeout";
        public const string PagesWithoutAntiClickjackingControls = "clickjacking-control-missing";
        public const string ParameterPollution = "parameter-pollution";
        public const string PathTraversal = "path-traversal";
        public const string RequestValidationDisabled = "request-validation-disabled";
        public const string RequestValidationModeDisabled = "request-validation-control-disabled";
        public const string ResponseWithInsecurelyConfiguredContentSecurityPolicyHeader = "csp-header-insecure";
        public const string ResponseWithXXssProtectionDisabled = "xssprotection-header-disabled";
        public const string ResponseWithoutContentSecurityPolicyHeader = "csp-header-missing";
        public const string ResponseWithoutXContentTypeOptionsHeader = "xcontenttype-header-missing";
        public const string RoleManagerProtectionMode = "role-manager-protection";
        public const string RoleManagerSSL = "role-manager-ssl";
        public const string SessionCookieHasNoHttpOnlyFlag = "httponly";
        public const string SessionCookieHasNoSecureFlag = "secure-flag-missing";
        public const string SessionRewriting = "session-rewriting";
        public const string SqlInjection = "sql-injection";
        public const string StoredCrossSiteScripting = "stored-xss";
        public const string TracingEnabled = "trace-enabled";
        public const string TracingEnabledforASPX = "trace-enabled-aspx";
        public const string TrustBoundaryViolation = "trust-boundary-violation";
        public const string UnprotectedConnectionStrings = "plaintext-conn-strings";
        public const string UnvalidatedRedirect = "unvalidated-redirect";
        public const string VersionHeaderEnabled = "version-header-enabled";
        public const string ViewstateEncryptionDisabled = "viewstate-encryption-disabled";
        public const string ViewstateMACValidationDisabled = "viewstate-mac-disabled";
        public const string WcfExceptionDetails = "wcf-exception-details";
        public const string WcfReplayDetectionNotEnabled = "wcf-detect-replays";
        public const string WcfServiceMetadataEnabled = "wcf-metdata-enabled";
        public const string WeakMembershipConfiguration = "weak-membership-config";
        public const string WeakRandomNumberGeneration = "crypto-weak-randomness";
        public const string WebApplicationDeployedinDebugMode = "compilation-debug";
        public const string XmlExternalEntityInjection = "xxe";
        public const string XPathInjection = "xpath-injection";
    }
}
