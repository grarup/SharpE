using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SharpE.Json.Data;

namespace SharpE.Json.Schemas
{
  public class SchemaObject
  {
    #region declerations
    private readonly Schema m_schema;
    private readonly JsonObject m_jsonObject;
    private bool m_isLoaded;
    private readonly string m_name;
    private SchemaDataType m_type;
    private Dictionary<string, SchemaObject> m_properties;
    private SchemaObject m_ref;
    private Dictionary<string, SchemaObject> m_definitions;
    private List<SchemaObject> m_items;
    private List<object> m_enums;
    private SchemaObject m_additionalProperties;
    private List<SchemaObject> m_allOf;
    private List<SchemaObject> m_anyOf;
    private List<SchemaObject> m_oneOf;
    private JsonArray m_anyOfArray;
    private JsonArray m_oneOfArray;
    private double m_min;
    private double m_max;
    private bool m_exclusiveMin;
    private bool m_exclusiveMax;
    private List<string> m_requiredProperties;
    private string m_description;
    private SchemaAutoCompletType m_schemaAutoCompletType;
    private string m_prefix = "";
    private string m_suffix = "";
    private Regex m_autoCompleteFilter;
    private string m_autoCompleteTargetKey;
    private string m_autoCompleteSourceKey;
    private char m_autoCompletePathSeperator;
    private string m_id;
    private bool m_uniqueItems;
    private double? m_multipleOf;
    private Regex m_pattern;
    private Dictionary<Regex, SchemaObject> m_patternProperties;
    #endregion

    #region constructor
    public SchemaObject(JsonElement jsonElement, Schema schema)
      : this((JsonObject)jsonElement.Value, schema)
    {
      m_name = jsonElement.Key;
    }

    public SchemaObject(JsonObject jsonObject, Schema schema)
    {
      m_schema = schema;
      m_jsonObject = jsonObject;
    }
    #endregion

    #region private methods
    private void Load()
    {
      lock (this)
      {
        if (m_isLoaded)
          return;
        JsonNode jsonNode = m_jsonObject as JsonNode;
        if (jsonNode != null)
        {
          string refPath = jsonNode.GetObjectOrDefault<string>("$ref", null);
          if (refPath != null)
          {
            m_ref = m_schema.GetSchemaObjectByRef(refPath);
            return;
          }
          m_id = jsonNode.GetObjectOrDefault("id", "undefined");
          m_type = jsonNode.GetObjectOrDefault("type", SchemaDataType.Undefined);
          JsonNode definitions = jsonNode.GetObjectOrDefault<JsonNode>("definitions", null);
          InitDictionary(out m_definitions, definitions);
          JsonNode properties = jsonNode.GetObjectOrDefault<JsonNode>("properties", null);
          InitDictionary(out m_properties, properties);
          if (m_properties != null && m_type == SchemaDataType.Undefined)
            m_type = SchemaDataType.Object;

          JsonObject items = jsonNode.GetObjectOrDefault<JsonObject>("items", null);
          InitArray(out m_items, items);
          JsonArray enums = jsonNode.GetObjectOrDefault<JsonArray>("enum", null);
          if (enums != null)
          {
            m_enums = new List<object>();
            foreach (object obj in enums)
              m_enums.Add(obj);
          }
          JsonNode additionalProperties = jsonNode.GetObjectOrDefault<JsonNode>("additionalProperties", null);
          if (additionalProperties != null)
          {
            m_additionalProperties = new SchemaObject(additionalProperties, m_schema);
          }
          JsonArray allOf = jsonNode.GetObjectOrDefault<JsonObject>("allOf", null) as JsonArray;
          InitArray(out m_allOf, allOf);
          m_anyOfArray = jsonNode.GetObjectOrDefault<JsonObject>("anyOf", null) as JsonArray;
          InitArray(out m_anyOf, m_anyOfArray);
          m_oneOfArray = jsonNode.GetObjectOrDefault<JsonObject>("oneOf", null) as JsonArray;
          InitArray(out m_oneOf, m_oneOfArray);
          m_description = jsonNode.GetObjectOrDefault("description", default(string));
          JsonNode autoCompleteNode = jsonNode.GetObjectOrDefault<JsonNode>("AutoComplet", null);
          if (autoCompleteNode != null)
          {
            m_schemaAutoCompletType = autoCompleteNode.GetObjectOrDefault("type", SchemaAutoCompletType.Undefined);
            m_prefix = autoCompleteNode.GetObjectOrDefault("prefix", "");
            m_suffix = autoCompleteNode.GetObjectOrDefault("suffix", "");
            string filter = autoCompleteNode.GetObjectOrDefault<string>("filter", null);
            if (filter != null)
              m_autoCompleteFilter = new Regex(filter);
            m_autoCompleteTargetKey = autoCompleteNode.GetObjectOrDefault<string>("targetkey", null);
            m_autoCompleteSourceKey = autoCompleteNode.GetObjectOrDefault<string>("sourcekey", null);
            m_autoCompletePathSeperator = autoCompleteNode.GetObjectOrDefault("pathseperator", "\\")[0];
          }
          object mulipleOfObject = jsonNode.GetObjectOrDefault<object>("multipleOf", null);
          if (mulipleOfObject != null)
          {
            if (mulipleOfObject is int)
              m_multipleOf = (int)mulipleOfObject;
            else if (mulipleOfObject is double)
              m_multipleOf = (double)mulipleOfObject;
          }
          JsonNode patternPropertiesNode = jsonNode.GetObjectOrDefault<object>("patternProperties", null) as JsonNode;
          Dictionary<string, SchemaObject> patternProperties;
          InitDictionary(out patternProperties, patternPropertiesNode);
          if (patternProperties != null)
          {
            m_patternProperties = new Dictionary<Regex, SchemaObject>();
            foreach (KeyValuePair<string, SchemaObject> patternProperty in patternProperties)
              m_patternProperties.Add(new Regex(patternProperty.Key), patternProperty.Value);
          }
          if (m_type == SchemaDataType.Undefined)
          {
            if (jsonNode.ContainsKey("minLength") || jsonNode.ContainsKey("maxLength") || jsonNode.ContainsKey("pattern"))
              m_type = SchemaDataType.String;
            if (jsonNode.ContainsKey("minimum") || jsonNode.ContainsKey("maximun") || jsonNode.ContainsKey("exclusiveMinimum") || jsonNode.ContainsKey("exclusiveMaximum") || jsonNode.ContainsKey("multipleOf"))
              m_type = SchemaDataType.Number;
            if (jsonNode.ContainsKey("minItems") || jsonNode.ContainsKey("maxItems") || jsonNode.ContainsKey("uniqueItems"))
              m_type = SchemaDataType.Array;
            if (jsonNode.ContainsKey("minProperties") || jsonNode.ContainsKey("maxProperties") || jsonNode.ContainsKey("required"))
              m_type = SchemaDataType.Object;

          }

          switch (m_type)
          {
            case SchemaDataType.String:
              m_min = jsonNode.GetObjectOrDefault("minLength", 0);
              m_max = jsonNode.GetObjectOrDefault("maxLength", int.MaxValue);
              string pattern = jsonNode.GetObjectOrDefault<string>("pattern", null);
              if (pattern != null)
                m_pattern = new Regex(pattern);
              break;
            case SchemaDataType.Integer:
              m_min = jsonNode.GetObjectOrDefault("minimum", int.MinValue);
              m_max = jsonNode.GetObjectOrDefault("maximum", int.MaxValue);
              m_exclusiveMin = jsonNode.GetObjectOrDefault("exclusiveMinimum", false);
              m_exclusiveMax = jsonNode.GetObjectOrDefault("exclusiveMaximum", false);
              break;
            case SchemaDataType.Number:
              object minJsonValue = jsonNode.GetObjectOrDefault<object>("minimum", null);
              if (minJsonValue != null)
              {
                if (minJsonValue is int)
                  m_min = (int)minJsonValue;
                else if (minJsonValue is double)
                  m_min = (double)minJsonValue;
                else
                  m_min = double.MinValue;
              }
              else
              {
                m_min = double.MinValue;
              }
              object maxJsonValue = jsonNode.GetObjectOrDefault<object>("maximum", null);
              if (maxJsonValue != null)
              {
                if (maxJsonValue is int)
                  m_max = (int)maxJsonValue;
                else if (maxJsonValue is double)
                  m_max = (double)maxJsonValue;
                else
                  m_max = double.MaxValue;
              }
              else
              {
                m_max = double.MaxValue;
              }
              m_exclusiveMin = jsonNode.GetObjectOrDefault("exclusiveMinimum", false);
              m_exclusiveMax = jsonNode.GetObjectOrDefault("exclusiveMaximum", false);
              break;
            case SchemaDataType.Array:
              m_min = jsonNode.GetObjectOrDefault("minItems", 0);
              m_max = jsonNode.GetObjectOrDefault("maxItems", int.MaxValue);
              m_uniqueItems = jsonNode.GetObjectOrDefault("uniqueItems", false);
              break;
            case SchemaDataType.Object:
              m_min = jsonNode.GetObjectOrDefault("minProperties", 0);
              m_max = jsonNode.GetObjectOrDefault("maxProperties", int.MaxValue);
              JsonArray jsonArray = jsonNode.GetObjectOrDefault<JsonArray>("required", null);
              if (jsonArray != null)
              {
                m_requiredProperties = new List<string>();
                foreach (JsonValue jsonValue in jsonArray)
                  m_requiredProperties.Add((string)jsonValue.Value);
              }
              break;
            case SchemaDataType.Undefined:
              break;
          }
        }
        m_isLoaded = true;
      }
    }

    private void InitArray(out List<SchemaObject> list, JsonObject values)
    {
      list = null;
      if (values == null)
        return;
      JsonArray array = values as JsonArray;
      if (array != null)
        list = array.OfType<JsonNode>().Select(node1 => new SchemaObject(node1, m_schema)).ToList();
      JsonNode node = values as JsonNode;
      if (node != null)
      {
        list = new List<SchemaObject> { new SchemaObject(node, m_schema) };
      }
    }

    private void InitDictionary(out Dictionary<string, SchemaObject> list, IEnumerable<JsonElement> node)
    {
      if (node == null)
      {
        list = null;
        return;
      }
      list = new Dictionary<string, SchemaObject>();
      foreach (JsonElement jsonElement in node)
      {
        SchemaObject schemaObject = new SchemaObject(jsonElement, m_schema);
        list.Add(schemaObject.Name, schemaObject);
      }
    }

    private void AddError(Dictionary<int, List<ValidationError>> errors, ValidationError error)
    {
      if (errors == null) return;
      if (errors.ContainsKey(error.LineIndex))
        errors[error.LineIndex].Add(error);
      else
        errors.Add(error.LineIndex, new List<ValidationError> { error });
    }

    private void ValidateElement(Dictionary<int, List<ValidationError>> errors, JsonElement jsonElement, string localPath)
    {
      if (jsonElement.Key == null)
      {
        //AddError(errors, new ValidationError(ValidationErrorState.NotCorrectJson, jsonElement, "Key exspected"));
        return;
      }
      if (jsonElement.Key == "default")
        return;
      if (m_properties != null && m_properties.ContainsKey(jsonElement.Key))
      {
        m_properties[jsonElement.Key].Validate(jsonElement.Value, errors, localPath);
      }
      else
      {
        if (m_patternProperties != null)
        {
          foreach (KeyValuePair<Regex, SchemaObject> patternedProperty in m_patternProperties)
          {
            if (patternedProperty.Key.IsMatch(jsonElement.Key))
            {
              patternedProperty.Value.Validate(jsonElement.Value, errors, localPath);
              return;
            }
          }
        }
        if (m_additionalProperties != null)
        {
          m_additionalProperties.Validate(jsonElement.Value, errors, localPath);
          //else
          //{
          //  bool any = m_additionalProperties.Any(n => n.Validate(jsonElement.Value, null));
          //  if (!any)
          //    AddError(errors, new ValidationError(ValidationErrorState.WrongData, jsonElement, jsonElement.Key + "needs to validate to at least on of these: \r\n" + string.Join(",", m_additionalProperties.Select(n => n.m_jsonObject.ToString()))));
          //}
        }
        else
        {
          AddError(errors, new ValidationError(ValidationErrorState.NotInSchema, jsonElement, jsonElement.Key + " not part of schema"));
          return;
        }
      }
      if (m_allOf != null)
      {
        foreach (SchemaObject schema2Object in m_allOf)
          schema2Object.Validate(jsonElement, errors, localPath);
      }
      if (m_anyOf != null)
      {
        bool anyfound = m_anyOf.Any(schema2Object => schema2Object.Validate(jsonElement, null, localPath));
        if (!anyfound)
          AddError(errors, new ValidationError(ValidationErrorState.WrongData, jsonElement, jsonElement.Key + "needs to validate to at least on of these: \r\n" + m_anyOfArray));
      }
      if (m_oneOf != null)
      {
        int validCount = m_oneOf.Count(schema2Object => schema2Object.Validate(jsonElement, null, localPath));
        if (validCount != 1)
          AddError(errors, new ValidationError(ValidationErrorState.WrongData, jsonElement, jsonElement.Key + "needs to validate exactly on of these: \r\n" + m_oneOfArray));
      }
    }

    private bool ValidateEnum(object value)
    {
      if (Enums == null)
        return true;
      return Enums.Any(n => ((JsonValue)n).Value.Equals(value));
    }

    private static void Indent(StringBuilder stringBuilder, int depth)
    {
      for (int i = 0; i < depth; i++)
      {
        stringBuilder.Append("\t");
      }
    }
    #endregion

    #region public properties
    public string Name
    {
      get
      {
        return m_name;
      }
    }

    public SchemaDataType Type
    {
      get
      {
        Load();
        if (m_ref != null)
          return m_ref.Type;

        return m_type;
      }
    }

    public Dictionary<string, SchemaObject> Properties
    {
      get
      {
        Load();
        if (m_ref != null)
          return m_ref.Properties;

        return m_properties;
      }
    }

    public Dictionary<string, SchemaObject> Definitions
    {
      get
      {
        Load();
        if (m_ref != null)
          return m_ref.Definitions;

        return m_definitions;
      }
    }

    public List<SchemaObject> Items
    {
      get { return m_items; }
    }

    public List<object> Enums
    {
      get
      {
        Load();
        if (m_ref != null)
          return m_ref.Enums;
        return m_enums;
      }
    }

    public string Description
    {
      get
      {
        Load();
        if (m_ref != null)
          return m_ref.Description;
        return m_description;
      }
    }

    public SchemaAutoCompletType SchemaAutoCompletType
    {
      get
      {
        Load();
        return m_ref == null ? m_schemaAutoCompletType : m_ref.SchemaAutoCompletType;
      }
    }

    public string Id
    {
      get
      {
        Load();
        return m_ref != null ? m_ref.Id : m_id;
      }
    }

    public string Prefix
    {
      get
      {
        Load();
        return m_ref != null ? m_ref.Prefix : m_prefix;
      }
    }

    public string Suffix
    {
      get
      {
        Load();
        return m_ref != null ? m_ref.Suffix : m_suffix;
      }
    }

    public Regex AutoCompleteFilter
    {
      get
      {
        Load();
        return m_ref != null ? m_ref.AutoCompleteFilter : m_autoCompleteFilter;
      }
    }

    public string AutoCompleteTargetKey
    {
      get
      {
        Load();
        return m_ref != null ? m_ref.AutoCompleteSourceKey : m_autoCompleteTargetKey;
      }
    }

    public string AutoCompleteSourceKey
    {
      get
      {
        Load();
        return m_ref != null ? m_ref.AutoCompleteSourceKey : m_autoCompleteSourceKey;
      }
    }

    public char AutoCompletePathSeperator
    {
      get
      {
        Load();
        return m_ref != null ? m_ref.AutoCompletePathSeperator : m_autoCompletePathSeperator;
      }
    }

    #endregion

    #region public methods
    public bool Validate(object obj, Dictionary<int, List<ValidationError>> errors, string localPath)
    {
      Load();
      if (m_ref != null)
        return m_ref.Validate(obj, errors, localPath);
      JsonValue jsonValue = obj as JsonValue;
      bool valid = true;

      switch (m_type)
      {
        case SchemaDataType.Undefined:
          if (jsonValue != null && Enums != null)
          {
            if (Enums.Any(jsonValue.Equals))
              break;
            AddError(errors, new ValidationError(ValidationErrorState.WrongData, jsonValue, "not in enum"));
          }
          if (m_allOf != null)
          {
            foreach (SchemaObject schema2Object in m_allOf)
              schema2Object.Validate(obj, errors, localPath);
          }
          if (m_anyOf != null)
          {
            bool anyfound = m_anyOf.Any(schema2Object => schema2Object.Validate(obj, null, localPath));
            if (!anyfound)
              AddError(errors, new ValidationError(ValidationErrorState.WrongData, obj as IDocPosition, "needs to validate to at least on of these: \r\n" + m_anyOfArray));
          }
          if (m_oneOf != null)
          {
            int validCount = m_oneOf.Count(schema2Object => schema2Object.Validate(obj, null, localPath));
            if (validCount != 1)
              AddError(errors, new ValidationError(ValidationErrorState.WrongData, obj as IDocPosition, "needs to validate exactly on of these: \r\n" + m_oneOfArray));
          }
          break;
        case SchemaDataType.String:
          if (jsonValue == null || !(jsonValue.Value is string))
          {
            AddError(errors, new ValidationError(ValidationErrorState.WrongData, obj as IDocPosition, "Expection a string"));
            return false;
          }
          if (!ValidateEnum(jsonValue.Value))
          {
            AddError(errors, new ValidationError(ValidationErrorState.WrongData, jsonValue, jsonValue.Value + " not in enum"));
            valid = false;
          }
          int stringlength = ((string)jsonValue.Value).Length;
          if (stringlength < m_min || stringlength > m_max)
          {
            AddError(errors, new ValidationError(ValidationErrorState.WrongData, jsonValue, "length need to be between " + m_min + " and " + m_max));
            valid = false;
          }
          if (m_pattern != null)
          {
            Match match = m_pattern.Match((string)jsonValue.Value);
            if (!match.Success)
            {
              AddError(errors, new ValidationError(ValidationErrorState.WrongData, jsonValue, "string must follow pattern: " + m_pattern));
              valid = false;
            }
          }
          switch (m_schemaAutoCompletType)
          {
            case SchemaAutoCompletType.Undefined:
              break;
            case SchemaAutoCompletType.FileRelative:
              {
                string path = localPath + "\\" + RemovePrefixAndSuffix(((string)jsonValue.Value).Replace(m_autoCompletePathSeperator, '\\'));
                if (path.IndexOfAny(Path.GetInvalidPathChars()) < 0)
                {
                  if (Path.GetExtension(path) == string.Empty)
                  {
                    if (!Directory.Exists(path))
                    {
                      AddError(errors, new ValidationError(ValidationErrorState.WrongData, jsonValue, "directory does not exsist"));
                      valid = false;
                    }
                  }
                  else
                  {
                    if (!File.Exists(path))
                    {
                      AddError(errors, new ValidationError(ValidationErrorState.WrongData, jsonValue, "file does not exsist"));
                      valid = false;
                    }
                  }
                }
              }
              break;
            case SchemaAutoCompletType.FileAbsolute:
              {
                string path = ((string)jsonValue.Value).Replace(m_autoCompletePathSeperator, '\\');
                if (path.IndexOfAny(Path.GetInvalidPathChars()) < 0)
                {
                  if (Path.GetExtension(path) == string.Empty)
                  {
                    if (!Directory.Exists(path))
                    {
                      AddError(errors,
                               new ValidationError(ValidationErrorState.WrongData, jsonValue,
                                                   "directory does not exsist"));
                      valid = false;
                    }
                  }
                  else
                  {
                    if (File.Exists(path))
                    {
                      AddError(errors,
                               new ValidationError(ValidationErrorState.WrongData, jsonValue, "file does not exsist"));
                      valid = false;
                    }
                  }
                }
              }
              break;
            case SchemaAutoCompletType.Key:
              //TODO check if value exist
              break;
            default:
              throw new ArgumentOutOfRangeException();
          }
          break;
        case SchemaDataType.Boolean:
          if (jsonValue == null || !(jsonValue.Value is bool))
          {
            AddError(errors, new ValidationError(ValidationErrorState.WrongData, obj as IDocPosition, "Expecting a boolean"));
            return false;
          }
          break;
        case SchemaDataType.Integer:
          if (jsonValue == null || !(jsonValue.Value is int))
          {
            AddError(errors, new ValidationError(ValidationErrorState.WrongData, obj as IDocPosition, "Expecting a integer"));
            return false;
          }
          if (!ValidateEnum(jsonValue.Value))
          {
            AddError(errors, new ValidationError(ValidationErrorState.WrongData, jsonValue, jsonValue.Value + " not in enum"));
            valid = false;
          }
          int intValue = (int)jsonValue.Value;
          if (((m_exclusiveMin && intValue == m_min) || intValue < m_min) || ((m_exclusiveMax && intValue == m_max) || intValue > m_max))
          {
            AddError(errors, new ValidationError(ValidationErrorState.WrongData, jsonValue, "value need to be in this range: " + m_min + "  " + (m_exclusiveMin ? ">" : ">=") + " x " + (m_exclusiveMax ? "<" : "<=") + m_max));
            valid = false;
          }
          if (m_multipleOf != null && intValue % m_multipleOf != 0)
          {
            AddError(errors, new ValidationError(ValidationErrorState.WrongData, jsonValue, "value must be a multipleOf " + m_multipleOf));
            valid = false;
          }
          break;
        case SchemaDataType.Number:
          if (jsonValue == null || !(jsonValue.Value is int || jsonValue.Value is double))
          {
            AddError(errors, new ValidationError(ValidationErrorState.WrongData, obj as IDocPosition, "Expecting a number type"));
            return false;
          }
          if (!ValidateEnum(jsonValue.Value))
          {
            AddError(errors, new ValidationError(ValidationErrorState.WrongData, jsonValue, jsonValue.Value + " not in enum"));
            valid = false;
          }
          double numberValue;
          if (jsonValue.Value is int)
            numberValue = (int)jsonValue.Value;
          else
            numberValue = (double)jsonValue.Value;
          if (((m_exclusiveMin && numberValue == m_min) || numberValue < m_min) || ((m_exclusiveMax && numberValue == m_max) || numberValue > m_max))
          {
            AddError(errors, new ValidationError(ValidationErrorState.WrongData, jsonValue, "value need to be in this range: " + m_min + "  " + (m_exclusiveMin ? ">" : ">=") + " x " + (m_exclusiveMax ? "<" : "<=") + m_max));
            valid = false;
          }
          if (m_multipleOf != null && numberValue % m_multipleOf != 0)
          {
            AddError(errors, new ValidationError(ValidationErrorState.WrongData, jsonValue, "value must be a multipleOf " + m_multipleOf));
            valid = false;
          }
          break;
        case SchemaDataType.Array:
          JsonArray jsonArray = obj as JsonArray;
          if (jsonArray != null)
          {
            if (m_items != null)
            {
              {
                foreach (IDocPosition jsonElement in jsonArray)
                {
                  if (m_items.Count == 1)
                  {
                    m_items[0].Validate(jsonElement, errors, localPath);
                  }
                  else
                  {
                    bool validItem =
                      m_items //.Where(schema2Object => schema2Object != null)
                        .Any(schema2Object => schema2Object.Validate(jsonElement, null, localPath));
                    if (!validItem)
                    {
                      AddError(errors,
                        new ValidationError(ValidationErrorState.WrongData, jsonElement,
                          "needs to validate to at least on of these: \r\n" + string.Join(",", m_items.Select(n => n.m_jsonObject.ToString()))));
                      valid = false;
                    }
                  }
                }
              }
            }
            if (jsonArray.Count < m_min || jsonArray.Count > m_max)
            {
              AddError(errors, new ValidationError(ValidationErrorState.WrongData, jsonArray, "the array length need to be between " + m_min + " and " + m_max));
            }
            if (m_uniqueItems && jsonArray.GroupBy(n => n.ToString()).Any(x => x.Skip(1).Any()))
            {
              AddError(errors, new ValidationError(ValidationErrorState.WrongData, jsonArray, "all items must be unique."));
            }
          }
          else
          {
            AddError(errors, new ValidationError(ValidationErrorState.WrongData, obj as IDocPosition, "expecting an array"));
            return false;
          }
          break;
        case SchemaDataType.Object:
          JsonNode jsonNode = obj as JsonNode;
          if (jsonNode != null)
          {
            foreach (JsonElement jsonElement in jsonNode)
            {
              ValidateElement(errors, jsonElement, localPath);
            }
            if (jsonNode.Count < m_min || jsonNode.Count > m_max)
            {
              AddError(errors, new ValidationError(jsonNode.Count < m_min ? ValidationErrorState.MissingChild : ValidationErrorState.ToMany, jsonNode, "the number of properties need to be between " + m_min + " and " + m_max));
            }
            if (m_requiredProperties != null)
            {
              if (!m_requiredProperties.All(jsonNode.ContainsKey))
              {
                AddError(errors, new ValidationError(ValidationErrorState.MissingChild, jsonNode, "missing these properties: " + string.Join(",", m_requiredProperties.Where(n => !jsonNode.ContainsKey(n)))));
              }
            }
          }
          else
          {
            AddError(errors, new ValidationError(ValidationErrorState.NotInSchema, obj as IDocPosition, "expecting an object"));
            return false;
          }
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
      return valid;
    }

    public string RemovePrefixAndSuffix(string value)
    {
      Load();
      if (m_ref != null)
        return m_ref.RemovePrefixAndSuffix(value);

      string retval = value;
      if (value.StartsWith(m_prefix))
        retval = retval.Substring(m_prefix.Length);
      if (retval.EndsWith(m_suffix))
        retval = retval.Substring(0, retval.Length - m_suffix.Length);
      return retval;
    }

    public override string ToString()
    {
      Load();
      return Name ?? (m_ref != null ? m_ref.m_id : m_id);
    }

    public void GenerateRequired(StringBuilder stringBuilder, int depth)
    {
      Load();
      if (m_ref != null)
      {
        m_ref.GenerateRequired(stringBuilder, depth);
        return;
      }
      switch (m_type)
      {
        case SchemaDataType.Undefined:
          break;
        case SchemaDataType.String:
          stringBuilder.Append("\"\"");
          break;
        case SchemaDataType.Boolean:
          stringBuilder.Append("false");
          break;
        case SchemaDataType.Integer:
          stringBuilder.Append(m_min > 0 ? m_min.ToString(CultureInfo.InvariantCulture) : "0");
          break;
        case SchemaDataType.Number:
          stringBuilder.Append(m_min > 0 ? m_min.ToString(CultureInfo.InvariantCulture) : "0.0");
          break;
        case SchemaDataType.Array:
          stringBuilder.Append("[\r\n");
          if (m_min > 0)
          {
            foreach (SchemaObject schemaObject in Items)
            {
              Indent(stringBuilder, depth + 1);
              schemaObject.GenerateRequired(stringBuilder, depth + 1);
            }
          }
          Indent(stringBuilder, depth);
          stringBuilder.Append("]\r\n");
          break;
        case SchemaDataType.Object:
          if (m_type != SchemaDataType.Object || m_requiredProperties == null)
          {
            stringBuilder.Append("{}");
            return;
          }
          stringBuilder.Append("{\r\n");
          foreach (string requiredProperty in m_requiredProperties)
          {
            Indent(stringBuilder, depth + 1);
            stringBuilder.Append("\"" + requiredProperty + "\" : ");
            if (m_properties != null && m_properties.ContainsKey(requiredProperty))
              m_properties[requiredProperty].GenerateRequired(stringBuilder, depth);
            else
            {
              if (m_additionalProperties != null)
                m_additionalProperties.GenerateRequired(stringBuilder, depth);
            }
            stringBuilder.Append(",\r\n");
          }
          Indent(stringBuilder, depth);
          stringBuilder.Append("}\r\n");
          break;
        case SchemaDataType.Nullable:
          break;
        case SchemaDataType.All:
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    public void GenerateAll(StringBuilder stringBuilder, int depth)
    {
      Load();
      if (m_ref != null)
      {
        m_ref.GenerateAll(stringBuilder, depth);
        return;
      }
      switch (m_type)
      {
        case SchemaDataType.Undefined:
          break;
        case SchemaDataType.String:
          stringBuilder.Append("\"\"");
          break;
        case SchemaDataType.Boolean:
          stringBuilder.Append("false");
          break;
        case SchemaDataType.Integer:
          stringBuilder.Append(m_min > 0 ? m_min.ToString(CultureInfo.InvariantCulture) : "0");
          break;
        case SchemaDataType.Number:
          stringBuilder.Append(m_min > 0 ? m_min.ToString(CultureInfo.InvariantCulture) : "0.0");
          break;
        case SchemaDataType.Array:
          stringBuilder.Append("[");
          foreach (SchemaObject schemaObject in Items)
          {
            stringBuilder.Append("\r\n");
            Indent(stringBuilder, depth + 1);
            schemaObject.GenerateAll(stringBuilder, depth + 1);
          }
          Indent(stringBuilder, depth);
          stringBuilder.Append("\r\n");
          Indent(stringBuilder, depth);
          stringBuilder.Append("]");
          break;
        case SchemaDataType.Object:
          stringBuilder.Append("{");
          foreach (SchemaObject schemaObject in m_properties.Values)
          {
            stringBuilder.Append("\r\n");
            Indent(stringBuilder, depth + 1);
            stringBuilder.Append("\"" + schemaObject.Name + "\" : ");
            schemaObject.GenerateAll(stringBuilder, depth + 1);
            stringBuilder.Append(",");
          }
          if (m_additionalProperties != null)
          {
            stringBuilder.Append("\r\n");
            Indent(stringBuilder, depth + 1);
            stringBuilder.Append("\"AdditionalPropperty\" : ");
            m_additionalProperties.GenerateAll(stringBuilder, depth + 1);
            stringBuilder.Append(",");
          }
          stringBuilder.Append("\r\n");
          Indent(stringBuilder, depth);
          stringBuilder.Append("}");
          break;
        case SchemaDataType.Nullable:
          break;
        case SchemaDataType.All:
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    public int GenerateMin(StringBuilder stringBuilder, string path)
    {
      Load();
      if (m_ref != null)
      {
        return m_ref.GenerateMin(stringBuilder, path);
      }

      switch (m_type)
      {
        case SchemaDataType.Undefined:
          break;
        case SchemaDataType.String:
          stringBuilder.Append("\"" + Prefix + (m_schemaAutoCompletType == SchemaAutoCompletType.FileAbsolute ? Path.GetDirectoryName(path) + "\\" : "") + Suffix + "\"");
          return Suffix.Length + 1;
        case SchemaDataType.Boolean:
          break;
        case SchemaDataType.Integer:
          break;
        case SchemaDataType.Number:
          break;
        case SchemaDataType.Array:
          stringBuilder.Append("[]");
          return 1;
        case SchemaDataType.Object:
          stringBuilder.Append("{}");
          return 1;
        case SchemaDataType.Nullable:
          break;
        case SchemaDataType.All:
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
      return 0;
    }

    public SchemaObject GetChild(string key)
    {
      Load();
      if (m_ref != null)
        return m_ref.GetChild(key);
      if (m_properties != null && m_properties.ContainsKey(key))
        return m_properties[key];
      if (m_additionalProperties != null)
        return m_additionalProperties;
      return null;
    }

    public List<SchemaObject> GetPosibilties()
    {
      List<SchemaObject> retVal = new List<SchemaObject>();
      if (m_anyOf != null)
        retVal.AddRange(m_anyOf);
      if (m_oneOf != null)
        retVal.AddRange(m_oneOf);
      return retVal;
    }
    #endregion
  }
}
