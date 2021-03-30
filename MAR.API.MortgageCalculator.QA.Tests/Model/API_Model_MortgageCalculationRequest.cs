﻿using System;

namespace MAR.API.MortgageCalculator.QA.Tests.Model
{
    /// <summary>
    /// Comes from MAR.API.MortgageCalculator.Model project
    /// </summary>
    public class MortgageCalculationRequest
    {
        public MortgageCalculationRequest()
        {
            RequestId = Guid.NewGuid();
        }
        /// <summary>
        /// Unique id for reference purposes
        /// </summary>
        public Guid RequestId { get; set; }
        /// <summary>
        /// Purchase / sales price of home
        /// </summary>
        /// <example>100000.00</example>
        public decimal PurchasePrice { get; set; }
        /// <summary>
        /// Annual Percentage Rate in whole percentage points (e.g. 1.12% => 1.12)
        /// </summary>
        /// <example>1.15</example>
        public decimal APR { get; set; }
        /// <summary>
        /// Loan term in whole years
        /// </summary>
        /// <example>30</example>
        public int LoanTermYears { get; set; }
        /// <summary>
        /// Down payment applied toward <see cref="PurchasePrice"/> in whole percentage points (e.g. 20% => 20.00)
        /// </summary>
        /// <example>20</example>
        public decimal DownPaymentPercent { get; set; }
        /// <summary>
        /// Property tax rate in whole percentage points (e.g. 1.05% => 1.05)
        /// </summary>
        /// <example>1.15</example>
        public decimal PropertyTaxRate { get; set; }
        /// <summary>
        /// Homeowner's insurance rate in whole percentage points (e.g. 0.25% => 0.25)
        /// </summary>
        /// <example>0.22</example>
        public decimal HomeownerInsuranceRate { get; set; }
        /// <summary>
        /// Homeowner's association monthly fee
        /// </summary>
        /// <example>10.00</example>
        public decimal HOAMonthly { get; set; }
    }
}
