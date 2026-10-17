using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.Domains.ValueObjects;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace SmartAndPro.ERP.Domains.Entities.Reservation
{
    public class ReservationPrintTableTemplate : Entity
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }
        

        public List<PrintTableColumn> Columns { get; set; } = new List<PrintTableColumn>();

        public PageOrientation PageOrientation { get; set; } = PageOrientation.Portrait;

        public int DisplayOrder { get; set; } = 0;

        public ReservationPrintTableTemplate()
        {
        }

        public ReservationPrintTableTemplate(string name, string tableType, string? description = null)
        {
            Name = name;
            Description = description;
            Columns = new List<PrintTableColumn>();
        }

        public void AddColumn(PrintTableColumn column)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            Columns.Add(column);
        }

        public void RemoveColumn(string columnName)
        {
            var column = Columns.FirstOrDefault(c => c.ColumnName == columnName);
            if (column != null)
            {
                Columns.Remove(column);
            }
        }

        public void UpdateColumnOrder(string columnName, int newOrderIndex)
        {
            var column = Columns.FirstOrDefault(c => c.ColumnName == columnName);
            if (column != null)
            {
                column.OrderIndex = newOrderIndex;
            }
        }

        public List<PrintTableColumn> GetVisibleColumns()
        {
            return Columns.Where(c => c.IsVisible).OrderBy(c => c.OrderIndex).ToList();
        }
    }
}
