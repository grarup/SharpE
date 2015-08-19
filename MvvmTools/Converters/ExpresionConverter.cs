/*
 * ExpresionConverter and accompanying samples are copyright (c) 2011 by Ivan Krivyakov
 * ivan [at] ikriv.com
 * They are distributed under the Apache License http://www.apache.org/licenses/LICENSE-2.0.html
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace SharpE.MvvmTools.Converters
{
  /// <summary>
  /// Value converter that performs arithmetic calculations over its argument(s)
  /// </summary>
  /// <remarks>
  /// ExpresionConverter can act as a value converter, or as a multivalue converter (WPF only).
  /// It is also a markup extension (WPF only) which allows to avoid declaring resources,
  /// ConverterParameter must contain an arithmetic expression over converter arguments. Operations supported are +, -, * and /
  /// Single argument of a value converter may referred as x, a, or {0}
  /// Arguments of multi value converter may be referred as x,y,z,t (first-fourth argument), or a,b,c,d, or {0}, {1}, {2}, {3}, {4}, ...
  /// The converter supports arithmetic expressions of arbitrary complexity, including nested subexpressions
  /// </remarks>
  public class ExpresionConverter : MarkupExtension, IMultiValueConverter, IValueConverter
  {
    readonly Dictionary<string, IExpression> m_storedExpressions = new Dictionary<string, IExpression>();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return Convert(new[] { value }, targetType, parameter, culture);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      try
      {
        decimal result = Parse(parameter.ToString()).Eval(values);
        //Debug.WriteLine("Exp: " + parameter + " | " + result + " | " + string.Join(", ", values));
        if (targetType == typeof(decimal)) return result;
        if (targetType == typeof(string)) return result.ToString(CultureInfo.InvariantCulture);
        if (targetType == typeof(int)) return (int)result;
        if (targetType == typeof(double)) return (double)result;
        if (targetType == typeof(long)) return (long)result;
        if (targetType == typeof (bool)) return result != 0;
        if (targetType == typeof (Visibility)) return result != 0 ? Visibility.Visible : Visibility.Collapsed;
        if (targetType == typeof (object)) return result;
        throw new ArgumentException(String.Format("Unsupported target type {0}", targetType.FullName));
      }
      catch (Exception ex)
      {
        ProcessException(ex);
      }

      return DependencyProperty.UnsetValue;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
      return this;
    }

    protected virtual void ProcessException(Exception ex)
    {
      Console.WriteLine(ex.Message);
    }

    private IExpression Parse(string s)
    {
      IExpression result;
      if (!m_storedExpressions.TryGetValue(s, out result))
      {
        result = new Parser().Parse(s);
        m_storedExpressions[s] = result;
      }

      return result;
    }

    interface IExpression
    {
      decimal Eval(object[] args);
    }

    class Constant : IExpression
    {
      private readonly decimal m_value;

      public Constant(string text)
      {
        if (!decimal.TryParse(text, out m_value))
        {
          throw new ArgumentException(String.Format("'{0}' is not a valid number", text));
        }
      }

      public decimal Eval(object[] args)
      {
        return m_value;
      }
    }

    class Variable : IExpression
    {
      private readonly int m_index;

      public Variable(string text)
      {
        if (!int.TryParse(text, out m_index) || m_index < 0)
        {
          throw new ArgumentException(String.Format("'{0}' is not a valid parameter index", text));
        }
      }

      public Variable(int n)
      {
        m_index = n;
      }

      public decimal Eval(object[] args)
      {
        if (m_index >= args.Length)
        {
          throw new ArgumentException(String.Format("ExpresionConverter: parameter index {0} is out of range. {1} parameter(s) supplied", m_index, args.Length));
        }
        return System.Convert.ToDecimal(args[m_index]);
      }
    }

    class BinaryOperation : IExpression
    {
      private readonly Func<decimal, decimal, decimal> m_operation;
      private readonly IExpression m_left;
      private readonly IExpression m_right;

      public BinaryOperation(char operation, IExpression left, IExpression right)
      {
        m_left = left;
        m_right = right;
        switch (operation)
        {
          case '+': m_operation = (a, b) => (a + b); break;
          case '-': m_operation = (a, b) => (a - b); break;
          case '*': m_operation = (a, b) => (a * b); break;
          case '/': m_operation = (a, b) => (a / b); break;
          default: throw new ArgumentException("Invalid operation " + operation);
        }
      }

      public decimal Eval(object[] args)
      {
        return m_operation(m_left.Eval(args), m_right.Eval(args));
      }
    }

    class BoolOperation : IExpression
    {
      private readonly Func<decimal, decimal, decimal> m_operation;
      private readonly IExpression m_left;
      private readonly IExpression m_right;

      public BoolOperation(char operation, IExpression left, IExpression right)
      {
        m_left = left;
        m_right = right;

        switch (operation)
        {
          case '<':
            m_operation = (a, b) => (a < b ? 1 : 0);
            break;
          case '>':
            m_operation = (a, b) => (a > b ? 1 : 0);
            break;
          case '=':
            m_operation = (a, b) => (a == b ? 1 : 0);
            break;
          case '&':
            m_operation = (a, b) => (a != 0 && b != 0 ? 1 : 0);
            break;
          case '|':
            m_operation = (a, b) => (a != 0 || b != 0 ? 1 : 0);
            break;
          default: throw new ArgumentException("Invalid operation " + operation);
        }
      }

      public decimal Eval(object[] args)
      {
        return m_operation(m_left.Eval(args), m_right.Eval(args));
      }
    }

    class Negate : IExpression
    {
      private readonly IExpression m_param;

      public Negate(IExpression param)
      {
        m_param = param;
      }

      public decimal Eval(object[] args)
      {
        return -m_param.Eval(args);
      }
    }

    class Parser
    {
      private string m_text;
      private int m_pos;

      public IExpression Parse(string text)
      {
        try
        {
          m_pos = 0;
          m_text = text;
          var result = ParseBoolExpresion();
          RequireEndOfText();
          return result;
        }
        catch (Exception ex)
        {
          string msg =
              String.Format("ExpresionConverter: error parsing expression '{0}'. {1} at position {2}", text, ex.Message, m_pos);

          throw new ArgumentException(msg, ex);
        }
      }

      private IExpression ParseBoolExpresion()
      {
        IExpression left = ParseBoolTerm();

        while (true)
        {
          if (m_pos >= m_text.Length) return left;

          var c = m_text[m_pos];

          
         if (c == '&' || c == '|')
          {
            ++m_pos;
            IExpression right = ParseBoolTerm();
            left = new BoolOperation(c, left, right);
          }
          else
          {
            return left;
          }
        }
      }

      private IExpression ParseBoolTerm()
      {
        IExpression left = ParseExpression();

        while (true)
        {
          if (m_pos >= m_text.Length) return left;

          var c = m_text[m_pos];

          if (c == '>' || c == '<' ||  c == '=')
          {
            ++m_pos;
            IExpression right = ParseExpression();
            left = new BoolOperation(c, left, right);
          }
          else
          {
            return left;
          }
        }
      }

      private IExpression ParseExpression()
      {
        IExpression left = ParseTerm();

        while (true)
        {
          if (m_pos >= m_text.Length) return left;

          var c = m_text[m_pos];

          if (c == '+' || c == '-')
          {
            ++m_pos;
            IExpression right = ParseTerm();
            left = new BinaryOperation(c, left, right);
          }
          else
          {
            return left;
          }
        }
      }

      private IExpression ParseTerm()
      {
        IExpression left = ParseFactor();

        while (true)
        {
          if (m_pos >= m_text.Length) return left;

          var c = m_text[m_pos];

          if (c == '*' || c == '/')
          {
            ++m_pos;
            IExpression right = ParseFactor();
            left = new BinaryOperation(c, left, right);
          }
          else
          {
            return left;
          }
        }
      }

      private IExpression ParseFactor()
      {
        SkipWhiteSpace();
        if (m_pos >= m_text.Length) throw new ArgumentException("Unexpected end of text");

        var c = m_text[m_pos];

        if (c == '+')
        {
          ++m_pos;
          return ParseFactor();
        }

        if (c == '-')
        {
          ++m_pos;
          return new Negate(ParseFactor());
        }

        if (c == 'x' || c == 'a') return CreateVariable(0);
        if (c == 'y' || c == 'b') return CreateVariable(1);
        if (c == 'z' || c == 'c') return CreateVariable(2);
        if (c == 'r' || c == 'd') return CreateVariable(3);
        if (c == 's' || c == 'e') return CreateVariable(4);
        if (c == 't' || c == 'f') return CreateVariable(5);

        if (c == '(')
        {
          ++m_pos;
          var expression = ParseBoolExpresion();
          SkipWhiteSpace();
          Require(')');
          SkipWhiteSpace();
          return expression;
        }

        if (c == '{')
        {
          ++m_pos;
          var end = m_text.IndexOf('}', m_pos);
          if (end < 0) { --m_pos; throw new ArgumentException("Unmatched '{'"); }
          if (end == m_pos) { throw new ArgumentException("Missing parameter index after '{'"); }
          var result = new Variable(m_text.Substring(m_pos, end - m_pos).Trim());
          m_pos = end + 1;
          SkipWhiteSpace();
          return result;
        }

        const string decimalRegEx = @"(\d+\.?\d*|\d*\.?\d+)";
        var match = Regex.Match(m_text.Substring(m_pos), decimalRegEx);
        if (match.Success)
        {
          m_pos += match.Length;
          SkipWhiteSpace();
          return new Constant(match.Value);
        }

        throw new ArgumentException(String.Format("Unexpeted character '{0}'", c));
      }

      private IExpression CreateVariable(int n)
      {
        ++m_pos;
        SkipWhiteSpace();
        return new Variable(n);
      }

      private void SkipWhiteSpace()
      {
        while (m_pos < m_text.Length && Char.IsWhiteSpace((m_text[m_pos]))) ++m_pos;
      }

      private void Require(char c)
      {
        if (m_pos >= m_text.Length || m_text[m_pos] != c)
        {
          throw new ArgumentException("Expected '" + c + "'");
        }

        ++m_pos;
      }

      private void RequireEndOfText()
      {
        if (m_pos != m_text.Length)
        {
          throw new ArgumentException("Unexpected character '" + m_text[m_pos] + "'");
        }
      }
    }
  }
}