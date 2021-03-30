using System.Collections.Generic;

namespace MAR.API.MortgageCalculator.QA.Tests.Model
{
    /// <summary>
    /// Comes from MAR.API.MortgageCalculator.Model project
    /// </summary>
    public class BulkMortgageCalcuationRequest
    {
        public List<MortgageCalculationRequest> Requests { get; set; }
    }
}
