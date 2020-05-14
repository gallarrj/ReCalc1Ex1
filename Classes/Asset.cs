using System;
using System.Collections.Generic;
using System.Text;

namespace ReCalc1Ex1.Classes
{
    class Asset
    {
        public int HistoryRunId { get; set; }
        public int Id { get; set; }
        public int RollYear { get; set; }
        public string Folio { get; set; }
        public int AcquisitionYear { get; set; }
        public decimal CostValue { get; set; }
        public string Index { get; set; }
        public int Life { get; set; }
        public decimal Depreciation { get; set; }
        public decimal AdjustmentFactor { get; set; }
        public decimal CalculatedValue { get; set; }
        public bool IsResidual { get; set; }
        public string Schedule { get; set; }
        public bool ResidualFlag { get; set; }
        public int EffectiveAge  { get; set; }
        public decimal Depreciation2 { get; set; }
        public decimal Factor { get; set; }
        public decimal ReplacementCostNew { get; set; }
        public decimal CalculatedValueNew { get; set; }

    }
}
