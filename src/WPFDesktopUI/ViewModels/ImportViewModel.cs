﻿using System.ComponentModel;
using Caliburn.Micro;
using MCBusinessLogic.Controllers;
using System.Data;
using System.Threading.Tasks;
using WPFDesktopUI.ViewModels.Interfaces;

namespace WPFDesktopUI.ViewModels {
  public class ImportViewModel : Screen, IImportViewModel {
   

    public string CsvFilePath { get; set; }
    public DataView CsvDataView { get; set; }
    public static DataTable CsvData { get; set; }
    public string TabHeader { get; set; } = "Import";



    public event PropertyChangedEventHandler PropertyChanged;

    public void OnSelected() {
    }

    public async Task BtnOpenCsvFile() {
      var fileName = FileSystemHelper.GetFilePath("CSV (Comma delimited) |*.csv");
      CsvFilePath = fileName;
      var sep = Properties.Settings.Default["StnCsvSeparation"].ToString();

      await Task.Run(() => {
        CsvData = CsvParser.ParseFromFile(fileName, sep);

        // Sanitize column headers
        foreach (DataColumn col in CsvData.Columns) {
          col.ColumnName = col.ColumnName.Replace("[", "").Replace("]", "");
        }

        // Match data structure to the UI view (this lets the user see the data)
        CsvDataView = CsvData.DefaultView;
      });
    }
  }
}
