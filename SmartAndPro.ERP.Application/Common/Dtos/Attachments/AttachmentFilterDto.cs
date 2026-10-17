namespace SmartAndPro.ERP.Application.Common.Dtos.Attachments
{
    public class AttachmentFiltersDto
    {
        /// <summary>
        /// Generic search keyword. When provided, searches across all entity properties marked with [Searchable].
        /// </summary>
        public string? SearchKeyword { get; set; }
    }
}
