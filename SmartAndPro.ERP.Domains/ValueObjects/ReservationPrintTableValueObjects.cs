using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace SmartAndPro.ERP.Domains.ValueObjects
{
    [Owned]
    public class PrintTableColumn
    {
        [Required]
        [StringLength(100)]
        public string ColumnName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string DisplayName { get; set; } = string.Empty;

        [StringLength(50)]
        public string DataType { get; set; } = "string";

        public int OrderIndex { get; set; }

        public bool IsVisible { get; set; } = true;

        public int? Width { get; set; }

        [StringLength(50)]
        public string? Alignment { get; set; }


        public PrintTableColumn()
        {
        }

        public PrintTableColumn(string columnName, string displayName, string dataType = "string",
            int orderIndex = 0, bool isVisible = true, int? width = null,
            string? alignment = null, string? cssClass = null, string? format = null)
        {
            ColumnName = columnName;
            DisplayName = displayName;
            DataType = dataType;
            OrderIndex = orderIndex;
            IsVisible = isVisible;
            Width = width;
            Alignment = alignment;
        }
    }
}
