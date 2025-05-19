using Syncfusion.XlsIO;
using Shared;

namespace backend.Services;
//Denne service bruges til at oprette excelfiler baseret på dahsboard filtrering - ud fra en NuGet pakke
public class ExcelEksportService
{
    public byte[] GenererExcelMedNavne(List<Bruger> elever)
    {
        using var excelEngine = new ExcelEngine();
        var app = excelEngine.Excel;
        app.DefaultVersion = ExcelVersion.Excel2016;

        var workbook = app.Workbooks.Create(1);
        var sheet = workbook.Worksheets[0];

        // Sæt overskrift på kolonnen 
        sheet.Range["A1"].Text = "Navn";

        // Udfyld værdier for celler i kolonnen - her med elevnavn
        for (int i = 0; i < elever.Count; i++)
        {
            sheet.Range[$"A{i + 2}"].Text = elever[i].Navn;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
