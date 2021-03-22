using System;

namespace MAR.API.MortgageCalculator.QA.Tests
{
    public class AppSettings
    {
        public string ApiResponseApiVersion { get; set; }
        public string ApiResponseApplicationName { get; set; }
        /// <summary>
        /// Base url for Mortgage API
        /// </summary>
        /// <example>http://myapi.domain.com/</example>
        public string BaseUrl { get; set; }
        /// <summary>
        /// Public paid access user id for testing
        /// </summary>
        public Guid PublicPaidAccessUserId { get; set; }
        /// <summary>
        /// Public paid access user password for testing
        /// </summary>
        public string PublicPaidAccessUserPassword { get; set; }
    }
}
