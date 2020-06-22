﻿using System.Collections.Generic;
using WPFDesktopUI.Models.SidePaneModels.Attributes.Interfaces;

namespace WPFDesktopUI.Models {
  public interface IQuickBooksSidePaneModel {
    Dictionary<string, IQbAttribute> Attr { get; set; }
    void AttrAdd(IQbAttribute a, string k, string s);
  }
}