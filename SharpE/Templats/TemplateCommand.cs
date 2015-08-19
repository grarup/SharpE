using System;
using System.Collections.Generic;
using System.IO;
using SharpE.Json.Data;

namespace SharpE.Templats
{
  public enum Condition
  {
    None,
    DoesNotExsist,
  }

  public class TemplateCommand
  {
    private readonly JsonNode m_jsonNode;
    private readonly Template m_template;
    private readonly TemplateCommandType m_type;
    private string m_source;
    private string m_target;
    private string m_findstring;
    private string m_valuestring;
    private bool m_hasErrors;
    private readonly List<TemplateParameter> m_usedParameters = new List<TemplateParameter>();
    private readonly List<string> m_errors = new List<string>();
    private string m_string;
    private Condition m_condition;
    private readonly char? m_pathSeperator;

    public TemplateCommand(JsonNode jsonNode, Template template)
    {
      m_jsonNode = jsonNode;
      m_template = template;
      m_type = jsonNode.GetObjectOrDefault("type", TemplateCommandType.Undefined);
      m_pathSeperator = jsonNode.GetObjectOrDefault<char?>("pathSeparator", null);
    }

    public List<TemplateParameter> UsedParameters
    {
      get { return m_usedParameters; }
    }

    public List<string> Errors
    {
      get { return m_errors; }
    }

    public bool HasErrors
    {
      get { return m_hasErrors; }
    }

    public bool CheckCommand()
    {
      m_hasErrors = false;
      m_errors.Clear();
      switch (m_type)
      {
        case TemplateCommandType.Undefined:
          m_errors.Add("Missing type");
          m_string = "Undefined";
          break;
        case TemplateCommandType.Copy:
          m_source = GetString("source", true);
          m_target = GetString("target");
          m_condition = m_jsonNode.GetObjectOrDefault("condition", Condition.None);
          m_string = "copy \"" + m_source + "\" to\r\n\"" + m_target + "\"";
          break;
        case TemplateCommandType.CreateFolder:
          m_target = GetString("target");
          m_string = "create folder \"" + m_target + "\"";
          break;
        case TemplateCommandType.Replace:
          m_source = GetString("source", true);
          m_findstring = GetString("findstring");
          m_valuestring = GetString("valuestring", false, true);
          m_string = "Replace \"" + m_findstring + "\" with \"" + m_valuestring + " in \"" + m_source + "\"";
          break;
        case TemplateCommandType.Insert:
          {
            m_source = GetString("source", true);
            m_findstring = GetString("findstring");
            m_valuestring = GetString("valuestring", false, true);
            if (!HasErrors)
            {
              string text = File.ReadAllText(m_source);
              int index = text.IndexOf(m_findstring, StringComparison.Ordinal);
              if (index == -1)
              {
                m_errors.Add("Insert command could not find " + m_findstring);
                m_hasErrors = true;
              }
            }
            m_string = "Insert \"" + (m_valuestring == null ? "" : m_valuestring.Replace("\r", "").Replace("\n", "")) + "\"\r\n in \"" + m_source + "\" after \"" + m_findstring + "\"";
          }
          break;
        case TemplateCommandType.InjectJson:
          {
            m_source = GetString("source", true);
            m_findstring = GetString("findstring");
            m_valuestring = GetString("valuestring", false, true);
            m_condition = m_jsonNode.GetObjectOrDefault("condition", Condition.None);
            if (m_valuestring != null)
            {
              JsonException jsonException;
              JsonObject jsonObject = JsonHelperFunctions.Parse(m_valuestring, out jsonException);
              if (jsonException != null)
              {
                m_errors.Add(m_valuestring.Replace("\r", "").Replace("\n", "") + " is not a valid json");
                m_hasErrors = true;
              }
            }
            if (!HasErrors)
            {
              JsonException jsonException;
              JsonNode jsonNode = JsonHelperFunctions.Parse(File.ReadAllText(m_source), out jsonException) as JsonNode;
              if (jsonException != null || jsonNode == null)
              {
                m_errors.Add(m_source + " is not a valid json file");
                m_hasErrors = true;
              }
              else
              {
                JsonNode injectionJsonNode = jsonNode.GetJsonObject(m_findstring.Split('.')) as JsonNode;
                if (injectionJsonNode == null)
                {
                  m_errors.Add(m_source + " does not contain " + m_findstring);
                  m_hasErrors = true;
                }
              }
            }
            m_string = "Inject \"" + (m_valuestring == null ? "" : m_valuestring.Replace("\r", "").Replace("\n", "")) + "\"\r\n in \"" + m_source + "\" at \"" + m_findstring + "\"";
          }
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
      return !HasErrors;
    }

    public bool Execute()
    {
      CheckCommand();
      if (HasErrors)
        return false;
      switch (m_type)
      {
        case TemplateCommandType.Undefined:
          return false;
        case TemplateCommandType.Copy:
          {
            try
            {
              if (m_condition == Condition.None || (m_condition == Condition.DoesNotExsist && !File.Exists(m_target)))
                File.Copy(m_source, m_target, m_template.OverrideExsistingFiles);
            }
            catch (Exception)
            {
              m_errors.Add("Copy from \"" + m_source + "\" to\r\n\"" + m_target + "\" failed");
              return false;
            }
          }
          break;
        case TemplateCommandType.CreateFolder:
          {
            try
            {
              Directory.CreateDirectory(m_target);
            }
            catch (Exception)
            {
              m_errors.Add("Create folder \"" + m_target + "\" failed");
              return false;
            }
          }
          break;
        case TemplateCommandType.Replace:
          {
            string text = File.ReadAllText(m_source).Replace(m_findstring, m_valuestring);
            try
            {
              File.WriteAllText(m_source, text);
            }
            catch (Exception)
            {
              m_errors.Add("Could not save to file: \"" + m_source + "\"");
              return false;
            }
          }
          break;
        case TemplateCommandType.Insert:
          {
            string text = File.ReadAllText(m_source);
            int index = text.IndexOf(m_findstring, StringComparison.Ordinal);
            if (index == -1)
            {
              m_errors.Add("Insert command could not find \"" + m_findstring + "\"");
              return false;
            }
            index += m_findstring.Length;
            text = text.Substring(0, index) + m_valuestring + text.Substring(index, text.Length - index);
            try
            {
              File.WriteAllText(m_source, text);
            }
            catch (Exception)
            {
              m_errors.Add("Insert could not save to file: \"" + m_source + "\"");
              return false;
            }
          }
          break;
        case TemplateCommandType.InjectJson:
          {
            JsonException jsonException;
            JsonObject jsonObject = JsonHelperFunctions.Parse(m_valuestring, out jsonException);
            if (jsonException != null)
            {
              m_errors.Add(m_valuestring.Replace("\r", "").Replace("\n", "") + " is not a valid json");
              m_hasErrors = true;
            }
            else
            {

              JsonNode jsonNode = JsonHelperFunctions.Parse(File.ReadAllText(m_source), out jsonException) as JsonNode;
              if (jsonException != null || jsonNode == null)
              {
                m_errors.Add(m_source + " is not a valid json file");
                m_hasErrors = true;
              }
              else
              {
                JsonNode injectionJsonNode = jsonNode.GetJsonObject(m_findstring.Split('.')) as JsonNode;
                if (injectionJsonNode == null)
                {
                  m_errors.Add(m_source + " does not contain " + m_findstring);
                  m_hasErrors = true;
                }
                else
                {
                  JsonNode injectionNodeValue = jsonObject as JsonNode;
                  if (injectionNodeValue != null)
                  {
                    foreach (JsonElement jsonElement in injectionNodeValue)
                    {
                      if (m_condition == Condition.None || (m_condition == Condition.DoesNotExsist && !injectionJsonNode.ContainsKey(jsonElement.Key)))
                        injectionJsonNode.Add(jsonElement);
                    }
                    File.WriteAllText(m_source, jsonNode.ToString());
                  }
                }
              }
            }
          }
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }

      return true;
    }

    private string GetString(string name, bool isFile = false, bool isForJson = false)
    {
      string retVal = m_jsonNode.GetObjectOrDefault<string>(name, null);
      if (retVal == null)
      {
        m_errors.Add("Missing " + name);
        m_hasErrors = true;
      }
      else
      {
        m_hasErrors = HasErrors | !m_template.AddParameters(ref retVal, m_errors, m_usedParameters, isForJson, m_pathSeperator);
        if (isFile && !File.Exists(retVal))
        {
          m_errors.Add("File does not exsist: " + retVal);
          m_hasErrors = true;
        }
      }
      return retVal;
    }


    private PathRelativity GetPathRelativity(ref string parameter)
    {
      switch (parameter[0])
      {
        case '\\':
          parameter = parameter.Substring(1);
          return PathRelativity.Folder;
        case '%':
          parameter = parameter.Substring(1);
          return PathRelativity.Project;
        case '>':
          parameter = parameter.Substring(1);
          return PathRelativity.Target;
      }
      return PathRelativity.Absolute;
    }

    public override string ToString()
    {
      return m_string;
    }
  }
}
