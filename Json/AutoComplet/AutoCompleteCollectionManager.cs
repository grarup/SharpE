using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SharpE.Definitions.Project;
using SharpE.Json.Data;
using SharpE.Json.Schemas;

namespace SharpE.Json.AutoComplet
{
  public class AutoCompleteCollectionManager : IAutoCompleteCollectionManager
  {
    private readonly SchemaManager m_schemaManager;
    private readonly Dictionary<string, List<List<string>>> m_collections = new Dictionary<string, List<List<string>>>();
    private readonly Dictionary<string, DateTime> m_fileTimeStamps = new Dictionary<string, DateTime>();
    private ITreeNode m_project;
    private readonly List<Task> m_tasks = new List<Task>();
    private bool m_isCompling;
    private CancellationTokenSource m_cancellationTokenSource;

    public AutoCompleteCollectionManager(SchemaManager schemaManager)
    {
      m_schemaManager = schemaManager;
    }

    public IEnumerable<string> GetAutoCompleteValues(string key)
    {
      if (m_collections.ContainsKey(key))
        return m_collections[key].SelectMany(n => n);
      return new List<string>();
    }

    private void Add(string key, string value, string prefix, IFileViewModel fileViewModel)
    {
      Dictionary<string, List<string>> dictionary = fileViewModel.GetTag("AutoComplete") as Dictionary<string, List<string>>;
      if (dictionary == null)
      {
        dictionary = new Dictionary<string, List<string>>();
        fileViewModel.SetTag("AutoComplete", dictionary);
      }
      List<string> list;
      dictionary.TryGetValue(key, out list);
      if (list == null)
      {
        list = new List<string>();
        lock (m_collections)
        {
          if (!m_collections.ContainsKey(key))
            m_collections.Add(key, new List<List<string>>());
          m_collections[key].Add(list);
          dictionary.Add(key, list);
        }
      }
      lock (list)
      {
        list.Add(prefix + value);
      }
    }

    private void AddRange(string key, IEnumerable<object> value, string prefix, IFileViewModel fileViewModel)
    {
      Dictionary<string, List<string>> dictionary = fileViewModel.GetTag("AutoComplete") as Dictionary<string, List<string>>;
      if (dictionary == null)
      {
        dictionary = new Dictionary<string, List<string>>();
        fileViewModel.SetTag("AutoComplete", dictionary);
      }
      List<string> list;
      dictionary.TryGetValue(key, out list);
      if (list == null)
      {
        list = new List<string>();
        lock (m_collections)
        {
          if (!m_collections.ContainsKey(key))
            m_collections.Add(key, new List<List<string>>());
          m_collections[key].Add(list);
          dictionary.Add(key, list);
        }
      } 
      lock (list)
      {
        list.AddRange(value.Select(n => prefix + n.ToString()));
      }
    }

    public void Save()
    {
      JsonNode root = new JsonNode();
      JsonNode collections = new JsonNode();
      foreach (KeyValuePair<string, List<List<string>>> keyValuePair in m_collections)
      {
        JsonArray jsonArray = new JsonArray();
        foreach (string item in keyValuePair.Value.SelectMany(n => n))
          jsonArray.Add(item);
        collections.Add(new JsonElement(keyValuePair.Key, jsonArray));
      }
      root.Add(new JsonElement("collections", collections));
      JsonNode filetimeStamps = new JsonNode();
      foreach (KeyValuePair<string, DateTime> keyValuePair in m_fileTimeStamps)
      {
        filetimeStamps.Add(new JsonElement(keyValuePair.Key.Replace("\\", "\\\\"), keyValuePair.Value));
      }
      root.Add(new JsonElement("fileTimeStamps", filetimeStamps));
      StreamWriter streamWriter = File.CreateText(m_project.Children[0].Path + "\\autocompletdata.json");
      streamWriter.Write(root.ToString());
      streamWriter.Close();
    }

    public void Load()
    {
      //try
      //{
      //  if (!File.Exists(m_project.Path + "\\autocompletdata.json"))
      //    return;
      //  StreamReader streamReader = File.OpenText(m_project.Path + "\\autocompletdata.json");
      //  JsonNode jsonNode = (JsonNode)JsonHelperFunctions.Parse(streamReader.ReadToEnd());
      //  streamReader.Close();
      //  foreach (JsonElement jsonElement in (IEnumerable<JsonElement>)jsonNode["collections"])
      //  {
      //    List<string> collection = new List<string>();
      //    foreach (string item in (IEnumerable)jsonElement.Value)
      //    {
      //      collection.Add(item);
      //    }
      //    m_collections.Add(jsonElement.Key, collection);
      //  }
      //  foreach (JsonElement jsonElement in (IEnumerable<JsonElement>)jsonNode["fileTimeStamps"])
      //    m_fileTimeStamps.Add(jsonElement.Key.Replace("\\\\", "\\"), DateTime.Parse(jsonElement.Value.ToString()));
      //}
      //catch (Exception)
      //{
      //}
    }

    public ITreeNode Project
    {
      get { return m_project; }
      set
      {
        if (m_project == value) return;
        m_project = value;
        m_collections.Clear();
        m_fileTimeStamps.Clear();
        if (m_project != null)
        {
          //Load();
          if (m_cancellationTokenSource != null)
          {
            m_cancellationTokenSource.Cancel();
            while (m_cancellationTokenSource != null)
              Thread.Sleep(10);
          }
          m_cancellationTokenSource = new CancellationTokenSource();
          m_isCompling = true;
          BuildCollection(m_project);
          Task.Factory.StartNew(() =>
          {
            Task.WaitAll(m_tasks.ToArray());
            m_tasks.Clear();
            m_cancellationTokenSource = null;
            m_isCompling = false;
            Save();
          });
        }
      }
    }

    public bool IsCompling
    {
      get { return m_isCompling; }
    }

    private void BuildCollection(ITreeNode root)
    {
      if (root == null) return;
      IFileViewModel fileViewModel = root as IFileViewModel;
      if (fileViewModel != null)
      {
        if (fileViewModel.Exstension != ".json")
          return;
        DateTime fileTimeStamp = File.GetLastWriteTime(fileViewModel.Path);
        if (m_fileTimeStamps.ContainsKey(fileViewModel.Path) && (fileTimeStamp - m_fileTimeStamps[fileViewModel.Path]).TotalSeconds < 1)
          return;
        if (m_fileTimeStamps.ContainsKey(fileViewModel.Path))
          m_fileTimeStamps[fileViewModel.Path] = fileTimeStamp;
        else
          m_fileTimeStamps.Add(fileViewModel.Path, fileTimeStamp);
        Regex scheamRegex = new Regex(@"""\$schema""\s*\:\s*""(.*)""", RegexOptions.IgnoreCase);
        Match match = scheamRegex.Match(fileViewModel.GetContent<string>());
        Schema schema = match.Success ? m_schemaManager.GetSchema(match.Groups[1].ToString()) : null;
        if (schema != null)
        {
          JsonException jsonException;
          JsonNode jsonNode = (JsonNode)JsonHelperFunctions.Parse(fileViewModel.GetContent<string>(), out jsonException);
          if (jsonException == null)
          {
            m_tasks.Add(Task.Factory.StartNew(() =>
              {
                BuildCollection(fileViewModel, jsonNode, schema, new List<string>(), m_cancellationTokenSource.Token);
                fileViewModel.FileChangedOnDisk += FileViewModelOnFileChangedOnDisk;
              }));
          }
        }
        if (!fileViewModel.HasUnsavedChanges)
          fileViewModel.Reset();
      }
      else
      {
        foreach (ITreeNode treeNode in root.Children)
          BuildCollection(treeNode);
      }
    }

    private void FileViewModelOnFileChangedOnDisk(IFileViewModel fileViewModel)
    {
      Dictionary<string, List<string>> dictionary = fileViewModel.GetTag("AutoComplete") as Dictionary<string, List<string>>;
      if (dictionary != null)
      {
        foreach (KeyValuePair<string, List<string>> keyValuePair in dictionary)
        {
          m_collections[keyValuePair.Key].Remove(keyValuePair.Value);
        }
      }
      dictionary.Clear();
      Regex scheamRegex = new Regex(@"""\$schema""\s*\:\s*""(.*)""", RegexOptions.IgnoreCase);
      Match match = scheamRegex.Match(fileViewModel.GetContent<string>());
      Schema schema = match.Success ? m_schemaManager.GetSchema(match.Groups[1].ToString()) : null;
      if (schema != null)
      {
        JsonException jsonException;
        JsonNode jsonNode = (JsonNode)JsonHelperFunctions.Parse(fileViewModel.GetContent<string>(), out jsonException);
        if (jsonException == null)
          m_tasks.Add(Task.Factory.StartNew(() => BuildCollection(fileViewModel, jsonNode, schema, new List<string>(), null)));
      }
    }

    private void BuildCollection(IFileViewModel fileViewModel, JsonNode jsonNode, Schema schema, List<string> path, CancellationToken? token)
    {
      if (token.HasValue && token.Value.IsCancellationRequested)
        return;
      foreach (JsonElement jsonElement in jsonNode)
      {
        path.Add(jsonElement.Key);
        SchemaObject schemaObject = schema.GetSchemaObject(path);
        if (schemaObject != null)
        {
          if (schemaObject.AutoCompleteTargetKey != null)
          {
            string prefix = schemaObject.Prefix;
            AddParameters(ref prefix, fileViewModel);
            if (jsonElement.Value is JsonNode)
              AddRange(schemaObject.AutoCompleteTargetKey, ((JsonNode)jsonElement.Value).Keys, prefix, fileViewModel);
            else
              Add(schemaObject.AutoCompleteTargetKey, jsonElement.Value.ToString(), prefix,fileViewModel);
          }
          JsonNode node = jsonElement.Value as JsonNode;
          if (node != null)
            BuildCollection(fileViewModel, node, schema, path, token);
          else
          {
            JsonArray array = jsonElement.Value as JsonArray;
            if (array != null)
              BuildCollection(fileViewModel, array, schema, path, token);
          }
        }
        path.RemoveAt(path.Count - 1);
      }
    }

    private void AddParameters(ref string orignal, ITreeNode treeNode)
    {
      string[] parts = orignal.Split('+');
      orignal = "";
      foreach (string part in parts)
      {
        Regex scheamRegex = new Regex(@"{(.*)}", RegexOptions.IgnoreCase);
        Match match = scheamRegex.Match(part.Trim());
        if (match.Success)
        {
          string parameter = treeNode.GetParameter(match.Groups[1].ToString());
          if (parameter != null)
          {
            orignal += parameter;
          }
          else
          {
            orignal += part;
          }
        }
        else
          orignal += part;
      }
    }

    private void BuildCollection(IFileViewModel fileViewModel, JsonArray jsonArray, Schema schema, List<string> path, CancellationToken? token)
    {
      foreach (object obj in jsonArray)
      {
        path.Add("[0]");
        SchemaObject schemaObject = schema.GetSchemaObject(path);
        if (schemaObject == null) continue;
        if (schemaObject.AutoCompleteTargetKey != null)
        {
          string prefix = schemaObject.Prefix;
          AddParameters(ref prefix, fileViewModel);
          Add(schemaObject.AutoCompleteTargetKey, obj.ToString(), prefix, fileViewModel);
        }
        JsonNode node = obj as JsonNode;
        if (node != null)
          BuildCollection(fileViewModel, node, schema, path, token);
        JsonArray array = obj as JsonArray;
        if (array != null)
          BuildCollection(fileViewModel, array, schema, path, token);
        path.RemoveAt(path.Count - 1);
      }
    }

    public List<AutoCompleteValue> GenerateAutoComplete(bool isInkey, SchemaObject schemaObject, string text, int offset, string relativeStartPath)
    {
      List<AutoCompleteValue> autocompletList = new List<AutoCompleteValue>();
      if (schemaObject == null)
        return autocompletList;

      if (isInkey)
      {
        if (schemaObject.Properties != null)
          foreach (KeyValuePair<string, SchemaObject> keyValuePair in schemaObject.Properties)
            autocompletList.Add(new AutoCompleteValue(AutocompleteType.String, keyValuePair.Value, schemaObject));
      }
      else
      {

        if (schemaObject.SchemaAutoCompletType != SchemaAutoCompletType.Undefined)
        {
          switch (schemaObject.SchemaAutoCompletType)
          {
            case SchemaAutoCompletType.FileRelative:
              {
                string value;
                int endQuatPosition = text.Length - schemaObject.Suffix.Length;
                if (endQuatPosition != offset)
                {
                  List<char> startChars = new List<char> { schemaObject.AutoCompletePathSeperator };
                  if (schemaObject.Prefix.Length > 0)
                    startChars.Add(schemaObject.Prefix.Last());
                  int indexPathStart = offset == 0 ? 0 : text.LastIndexOfAny(startChars.ToArray(), Math.Min(offset - 1, text.Length - 1)) + 1;
                  if (indexPathStart < 0)
                    indexPathStart = 0;
                  int indexQuatStrat = schemaObject.Prefix.Length;
                  if (indexQuatStrat < indexPathStart)
                    value = schemaObject.Prefix + text.Substring(indexQuatStrat, indexPathStart - indexQuatStrat) +
                            schemaObject.Suffix;
                  else
                    value = schemaObject.Prefix + schemaObject.Suffix;
                }
                else
                  value = text;
                value = schemaObject.RemovePrefixAndSuffix(value);
                string filePath = Path.GetDirectoryName(relativeStartPath) + "\\" + value.Replace(schemaObject.AutoCompletePathSeperator, '\\');
                if (File.Exists(filePath) || !Directory.Exists(filePath))
                  return autocompletList;
                autocompletList.Add(new AutoCompleteValue(AutocompleteType.File, ".." + schemaObject.AutoCompletePathSeperator, schemaObject));
                foreach (string directory in Directory.GetDirectories(filePath))
                  autocompletList.Add(new AutoCompleteValue(AutocompleteType.File,
                                                              Path.GetFileName(directory) + schemaObject.AutoCompletePathSeperator, schemaObject));
                foreach (string file in Directory.GetFiles(filePath))
                {
                  if (schemaObject.AutoCompleteFilter == null || schemaObject.AutoCompleteFilter.IsMatch(file))
                    autocompletList.Add(new AutoCompleteValue(AutocompleteType.File, Path.GetFileName(file),
                                                                schemaObject));
                }
              }
              break;
            case SchemaAutoCompletType.FileAbsolute:
              {
                string value;
                int endQuatPosition = text.Length - schemaObject.Suffix.Length;
                if (endQuatPosition != offset)
                {
                  List<char> startChars = new List<char> { schemaObject.AutoCompletePathSeperator, };
                  if (schemaObject.Prefix.Length > 0)
                    startChars.Add(schemaObject.Prefix.Last());
                  int indexPathStart = offset == 0 ? 0 : (text.LastIndexOfAny(startChars.ToArray(), Math.Min(offset - 1, text.Length - 1)) + 1);
                  int indexQuatStrat = schemaObject.Prefix.Length;
                  if (indexQuatStrat < indexPathStart)
                    value = schemaObject.Prefix + text.Substring(indexQuatStrat, indexPathStart - indexQuatStrat) +
                            schemaObject.Suffix;
                  else
                    value = schemaObject.Prefix + schemaObject.Suffix;
                }
                else
                  value = text;
                if (value.Length >= schemaObject.Prefix.Length)
                  value = value.Substring(schemaObject.Prefix.Length,
                                          value.Length - schemaObject.Prefix.Length - schemaObject.Suffix.Length);
                string filePath = value.Replace(schemaObject.AutoCompletePathSeperator, '\\');
                if (File.Exists(filePath) || !Directory.Exists(filePath))
                  return autocompletList;
                autocompletList.Add(new AutoCompleteValue(AutocompleteType.File, ".." + schemaObject.AutoCompletePathSeperator, schemaObject));
                foreach (string directory in Directory.GetDirectories(filePath))
                  autocompletList.Add(new AutoCompleteValue(AutocompleteType.File,
                                                              Path.GetFileName(directory) + schemaObject.AutoCompletePathSeperator, schemaObject));
                foreach (string file in Directory.GetFiles(filePath))
                {
                  if (schemaObject.AutoCompleteFilter == null || schemaObject.AutoCompleteFilter.IsMatch(file))
                    autocompletList.Add(new AutoCompleteValue(AutocompleteType.File, Path.GetFileName(file),
                                                                schemaObject));
                }
              }
              break;
            case SchemaAutoCompletType.Key:
              {
                if (schemaObject.AutoCompleteSourceKey == null)
                  break;
                foreach (string autoCompleteValue in GetAutoCompleteValues(schemaObject.AutoCompleteSourceKey))
                  autocompletList.Add(new AutoCompleteValue(AutocompleteType.String, autoCompleteValue, schemaObject));
              }
              break;
          }
        }
        else
        {
          switch (schemaObject.Type)
          {
            case SchemaDataType.Undefined:
              List<SchemaObject> posibilities = schemaObject.GetPosibilties();
              foreach (SchemaObject posibility in posibilities)
              {
                autocompletList.Add(new AutoCompleteValue(AutocompleteType.Selector, posibility, schemaObject));
              }
              break;
            case SchemaDataType.String:
              if (schemaObject.Enums != null)
              {
                foreach (object obj in schemaObject.Enums)
                  autocompletList.Add(new AutoCompleteValue(AutocompleteType.String, obj, schemaObject));
              }
              break;
            case SchemaDataType.Boolean:
              autocompletList.Add(new AutoCompleteValue(AutocompleteType.String, "true", schemaObject));
              autocompletList.Add(new AutoCompleteValue(AutocompleteType.String, "false", schemaObject));
              break;
          }
        }
      }
      return autocompletList;
    }
  }
}
