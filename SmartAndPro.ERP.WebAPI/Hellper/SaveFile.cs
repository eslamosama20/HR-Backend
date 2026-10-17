namespace SmartAndPro.ERP.WebAPI.Hellper
{
    public static class SaveFile
    {
        public static async Task<string> AddFile(string base64String)
        {
            // Replace "uploads" with your desired folder name within the wwwroot directory
            string uploadsFolderPath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "uploads");

            // Validate base64 string format to avoid potential errors
            if (string.IsNullOrEmpty(base64String)) return string.Empty;
            try
            {
                string contentType = "image/jpeg";
                var contentBytes = Convert.FromBase64String(base64String.Trim());

                // Ensure directory exists
                if (!Directory.Exists(uploadsFolderPath))
                {
                    Directory.CreateDirectory(uploadsFolderPath);
                }

                // Generate unique filename based on current timestamp and extension
                string fileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}.{GetFileExtension(contentType)}";
                string filePath = Path.Combine(uploadsFolderPath, fileName);

                // Save the file asynchronously
                await File.WriteAllBytesAsync(filePath, contentBytes);

                return filePath;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }
        private static string GetFileExtension(string contentType)
        {
            return contentType switch
            {
                "image/jpeg" => "jpg",
                "image/png" => "png",
                "application/pdf" => "pdf",
                _ => "bin" // Handle unknown content types safely
            };
        }
    }
}
