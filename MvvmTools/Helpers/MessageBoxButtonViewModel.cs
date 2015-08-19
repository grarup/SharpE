﻿using System;
using System.Windows;
using SharpE.MvvmTools.Commands;

namespace SharpE.MvvmTools.Helpers
{
  public class MessageBoxButtonViewModel
  {
    private readonly GenericManualCommand<MessageBoxResult> m_command;
    private readonly string m_text;
    private readonly MessageBoxResult m_result;

    public MessageBoxButtonViewModel(string text, MessageBoxResult result, Action<MessageBoxResult> returnAction)
    {
      m_text = text;
      m_result = result;
      m_command = new GenericManualCommand<MessageBoxResult>(returnAction);
    }

    public MessageBoxResult Result
    {
      get { return m_result; }
    }

    public string Text
    {
      get { return m_text; }
    }

    public GenericManualCommand<MessageBoxResult> Command
    {
      get { return m_command; }
    }
  }
}
