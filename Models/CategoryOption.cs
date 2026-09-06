using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dLab.General.ElementsReport.Models
{
    public class CategoryOption
    {
        public CategoryOption(ReportCategory value, string displayName)
        {
            Value = value;
            DisplayName = displayName;
        }
        public ReportCategory Value { get; }
        public string DisplayName { get; }
    }
}
