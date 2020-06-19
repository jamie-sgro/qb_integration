﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using MCBusinessLogic.Controllers;
using MCBusinessLogic.Controllers.Interfaces;
using WPFDesktopUI.Models;
using WPFDesktopUI.ViewModels.Interfaces;
using stn = WPFDesktopUI.Controllers.SettingsController;
using ErrHandler = WPFDesktopUI.Controllers.QbImportExceptionHandler;

namespace WPFDesktopUI.ViewModels.QuickBooks {
  public class QuickBooksSidePaneViewModel : Screen, IMainTab, IQuickBooksSidePaneViewModel {
    public QuickBooksSidePaneViewModel() {
      QbspModel = Factory.CreateQuickBooksSidePaneModel();

      QbspModel.AttrAdd(Factory.CreateQbStringAttribute(), "CustomerRefFullName", "CUSTOMER:JOB");
      QbspModel.Attr["CustomerRefFullName"].IsMandatory = true;

      QbspModel.AttrAdd(Factory.CreateQbStringAttribute(), "ClassRefFullName", "CLASS");

      QbspModel.AttrAdd(Factory.CreateQbNullAttribute(), "TemplateRefFullName", "TEMPLATE");
      QbspModel.Attr["TemplateRefFullName"].IsMandatory = true;
      QbspModel.Attr["TemplateRefFullName"].ComboBox.RequiresCsv = false;
      QbspModel.Attr["TemplateRefFullName"].ToolTip = "Please select 'Query QuickBooks' before"+
                                                      " custom lists can be generated";

      QbspModel.AttrAdd(Factory.CreateQbDateTimeAttribute(), "TxnDate", "DATE");

      QbspModel.AttrAdd(Factory.CreateQbStringAttribute(), "BillAddress", "BILL ADDRESS");

      QbspModel.AttrAdd(Factory.CreateQbStringAttribute(), "ShipAddress", "SHIP ADDRESS");

      QbspModel.AttrAdd(Factory.CreateQbStringAttribute(),
        "Other", stn.QbInvHasHeaderOther() ? stn.QbInvHeaderOtherName() : "OTHER");

      QbspModel.AttrAdd(Factory.CreateQbStringAttribute(), "ItemRef", "ITEM");
      QbspModel.Attr["ItemRef"].IsMandatory = true;

      QbspModel.AttrAdd(Factory.CreateQbDoubleAttribute(), "ORRatePriceLevelRate", "RATE");

      QbspModel.AttrAdd(Factory.CreateQbDoubleAttribute(), "Quantity", "QUANTITY");

      QbspModel.AttrAdd(Factory.CreateQbDateTimeAttribute(), "ServiceDate", "SERVICE DATE");

      QbspModel.AttrAdd(Factory.CreateQbStringAttribute(), "Other1", "TIME IN OUT");

      QbspModel.AttrAdd(Factory.CreateQbStringAttribute(), "Other2", "STAFF NAME");
    }

    public IQuickBooksSidePaneModel QbspModel { get; set; }
    public bool CanQbExport { get; set; } = true;
    public bool QbProgressBarIsVisible { get; set; } = false;
    public string ConsoleMessage { get; set; } = "Please select 'Query QuickBooks' before custom lists can be generated";


    public async void OnSelected() {
      await Task.Run(() => {
        QbspModel.Attr["Other"].Name = stn.QbInvHasHeaderOther() ? stn.QbInvHeaderOtherName() : "OTHER";


        var csvData = ImportViewModel.CsvData;
        if (csvData == null) return;
        var csvHeaders = GetCsvHeaders(csvData);

        // Loop through all QbAttribute ComboBoxes in QbspModel
        foreach (var attribute in QbspModel.Attr) {
          if (!QbspModel.Attr[attribute.Key].ComboBox.RequiresCsv) continue;
          QbspModel.Attr[attribute.Key].ComboBox.ItemsSource = csvHeaders;
          QbspModel.Attr[attribute.Key].ComboBox.IsEnabled = true;
        }
      });
    }

    public async void QbExport() {
      SessionStart();
      try {
        var templateList = await InitTemplateRefFullName();
        QbspModel.Attr["TemplateRefFullName"].ComboBox.ItemsSource = templateList;
        SessionEnd();
      } catch (Exception e) {
        ConsoleMessage = ErrHandler.DelegateHandle(e);
      } finally {
        CanQbExport = true;
        QbProgressBarIsVisible = false;
      }
    }

    private void SessionStart() {
      CanQbExport = false;
      QbProgressBarIsVisible = true;
    }

    private void SessionEnd() {
      QbspModel.Attr["TemplateRefFullName"].ComboBox.IsEnabled = true;
      ConsoleMessage = "Query successfully completed";
    }

    /// <summary>
    /// Returns a list of templates used in QuickBooks based on their name
    /// </summary>
    /// <param name="qbFilePath">The full path for the QuickBooks file</param>
    /// <returns></returns>
    private static async Task<List<string>> InitTemplateRefFullName() {
      IQbExportController qbExportController = Factory.CreateQbExportController();
      var templates = await Task.Run(() => {
        return qbExportController.GetTemplateNamesList();
      });

      // Add blank to start
      templates.Insert(0, "");
      return templates;
    }

    private static List<string> GetCsvHeaders(DataTable dt) {
      string[] columnHeaders = dt?.Columns.Cast<DataColumn>()
        .Select(x => x.ColumnName)
        .ToArray();

      // Convert string[] to List<string>
      List<string> finalList = columnHeaders?.ToList();

      if (finalList == null) return new List<string>();

      // Add blank to start
      finalList.Insert(0, "");
      return finalList;
    }
  }
}
