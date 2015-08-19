using System.Collections.Generic;
using SharpE.Definitions.Project;
using SharpE.Json.Schemas;

namespace SharpE.Json.AutoComplet
{
  public interface IAutoCompleteCollectionManager
  {
    IEnumerable<string> GetAutoCompleteValues(string key);
    ITreeNode Project { get; set; }
    bool IsCompling { get; }
    List<AutoCompleteValue> GenerateAutoComplete(bool isInkey, SchemaObject schemaObject, string text, int offset, string relativeStartPath);
  }
}