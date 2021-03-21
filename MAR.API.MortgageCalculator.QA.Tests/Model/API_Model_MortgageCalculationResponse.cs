﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace MAR.API.MortgageCalculator.QA.Tests.Model
{
    public class MortgageCalculationResult
    {
        public MortgageCalculationRequest Request { get; }
        public List<string> Errors { get; }
        public List<string> ValidationErrors { get; }
        public bool IsSuccessful
        {
            get
            {
                return (Errors == null ? true : !Errors.Any())
                    && (ValidationErrors == null ? true : !ValidationErrors.Any());
            }
        }
        public MortgageCalculationResult(MortgageCalculationRequest request)
        {
            Request = request ?? throw new ArgumentNullException(nameof(request));
            ValidationErrors = new List<string>();
            Errors = new List<string>();
        }
        public MortgageCalculationResult(MortgageCalculationRequest request, List<string> validationErrors, List<string> errorMessages)
        {
            Request = request;
            ValidationErrors = validationErrors ?? new List<string>();
            Errors = errorMessages ?? new List<string>();
        }
        public decimal MortgagePaymentyMonthly { get; set; }
        public decimal PropertyTaxPaymentMonthly { get; set; }
        public decimal HomeownersInsurancePaymentMonthly { get; set; }
        public decimal TotalMonthlyPayment =>
            MortgagePaymentyMonthly
            + PropertyTaxPaymentMonthly
            + HomeownersInsurancePaymentMonthly
            + Request?.HOAMonthly ?? 0.00M;

        public decimal DownPayment { get; set; }
        public decimal TermInterestPaid { get; set; }
        public decimal TotalCostOfLoan => (Request?.PurchasePrice ?? 0.00M) - DownPayment + TermInterestPaid;
        public decimal HOAAnnual => (Request?.HOAMonthly ?? 0.00M) * 12;
    }
}
