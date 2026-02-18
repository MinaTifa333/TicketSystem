using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using TicketSystem.TSModel;

namespace TicketSystem.Services.Export;

public class ExportService
{
    public byte[] ExportOrdersWithDetailsToExcel(List<Order> orders)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage();
        var sheet = package.Workbook.Worksheets.Add("الطلبات");

        sheet.View.RightToLeft = true;

        // رؤوس أعمدة الطلب
        string[] headers = { "رقم الطلب", "أنشئ بواسطة", "تاريخ الإنشاء", "القسم", "آخر قسم", "عامل التكرار", "الحالة" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = sheet.Cells[1, i + 1];
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Font.Size = 12;
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(Color.LightSkyBlue);
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            cell.Style.Border.BorderAround(ExcelBorderStyle.Medium);
        }
        sheet.View.FreezePanes(2, 1);

        int row = 2;
        foreach (var order in orders)
        {
            // بيانات الطلب
            sheet.Cells[row, 1].Value = order.Id;
            sheet.Cells[row, 2].Value = order.CreatedBy;
            sheet.Cells[row, 3].Value = order.DateTime?.ToString("yyyy/MM/dd HH:mm");
            sheet.Cells[row, 4].Value = order.Section?.Name;
            sheet.Cells[row, 5].Value = order.LastDepartment;
            sheet.Cells[row, 6].Value = order.RepeateFactor;
            sheet.Cells[row, 7].Value = order.Closed == true ? "مغلق" : "مفتوح";

            for (int col = 1; col <= 7; col++)
            {
                var cell = sheet.Cells[row, col];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(Color.WhiteSmoke);
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            row++;

            if (order.OrderDetails?.Any() == true)
            {
                // دمج عنوان لتفاصيل الطلب
                sheet.Cells[row, 2, row, 8].Merge = true;
                sheet.Cells[row, 2].Value = "تفاصيل الطلب";
                sheet.Cells[row, 2].Style.Font.Bold = true;
                sheet.Cells[row, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[row, 2].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 242, 204));
                sheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[row, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                sheet.Cells[row, 2].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                row++;

                // رؤوس تفاصيل الطلب
                string[] detailHeaders = { "من قسم", "إلى قسم", "المبلغ", "ملاحظات", "الموافقة", "تاريخ الإنشاء", "أنشئ بواسطة" };
                for (int i = 0; i < detailHeaders.Length; i++)
                {
                    var cell = sheet.Cells[row, i + 2];
                    cell.Value = detailHeaders[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(Color.PeachPuff);
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
                row++;

                foreach (var detail in order.OrderDetails)
                {
                    sheet.Cells[row, 2].Value = detail.FromDep;
                    sheet.Cells[row, 3].Value = detail.ToDepartment;
                    sheet.Cells[row, 4].Value = detail.Amount;
                    sheet.Cells[row, 5].Value = detail.Notes;
                    sheet.Cells[row, 6].Value = detail.NeedApproval ? "نعم" : "لا";
                    sheet.Cells[row, 7].Value = detail.CreationDate.ToString("yyyy/MM/dd HH:mm");
                    sheet.Cells[row, 8].Value = detail.CreatedBy;

                    for (int col = 2; col <= 8; col++)
                    {
                        var cell = sheet.Cells[row, col];
                        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cell.Style.Border.BorderAround(ExcelBorderStyle.Dotted);
                    }

                    row++;
                }
            }

            // ترك سطر فاصل بين الطلبات
            row++;
        }

        // تنسيق تلقائي للأعمدة
        sheet.Cells[sheet.Dimension.Address].AutoFitColumns();

        return package.GetAsByteArray();
    }
}
