namespace ApiCraftSystem.Helper.Utility
{
    public static class ExcelExporter
    {
        public static byte[] ExportToExcel<T>(IEnumerable<T> data)
        {
            using var package = new OfficeOpenXml.ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Data");
            worksheet.Cells.LoadFromCollection(data, true);
            return package.GetAsByteArray();
        }
    }

}
