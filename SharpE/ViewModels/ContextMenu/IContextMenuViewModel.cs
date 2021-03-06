﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpE.ViewModels.ContextMenu
{
  public interface IContextMenuViewModel
  {
    List<MenuItemViewModel> MenuItems { get; }
    void Refresh();
  }
}
