using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SharpE.Json.Data
{
  public static class JsonHelperFunctions
  {
    public static string DeterminPath(string text, int index, out List<string> path)
    {
      if (index >= text.Length)
        index = text.Length - 1;
      if (CountQoatsBefore(text, index) % 2 == 1)
      {
        int indexQuotStart = Math.Min(index, text.Length - 1);
        bool isEcaped = true;
        while (isEcaped)
        {
          indexQuotStart = text.LastIndexOf('"', indexQuotStart - 1);
          isEcaped = indexQuotStart != 0 && text[indexQuotStart - 1] == '\\';
        }
        index = indexQuotStart - 1;
      }
      path = new List<string>();
      DeterminPath(text, index - 1, path);
      return string.Join("\\", path);
    }

    private static void DeterminPath(string text, int index, List<string> path)
    {
      if (index < 0) return;
      bool isArray;
      int indexBracket = IndexOfNextLevel(text, index, out isArray);
      if (indexBracket == -1)
        return;
      char c;
      int indexColon = TryToFindBefore(text, new List<char> { ':', ',', '{', '[' }, out c, index);
      if (indexColon == -1)
        return;
      if (c == ':')
      {
        int indexQuotEnd = text.LastIndexOf('"', indexColon, indexColon + 1) - 1;
        int indexQuotStart = text.LastIndexOf('"', indexQuotEnd, indexQuotEnd + 1);
        path.Insert(0, text.Substring(indexQuotStart + 1, indexQuotEnd - indexQuotStart));
      }
      else
      {
        if (isArray)
        {
          path.Insert(0, "[" + DeterminArrayIndex(text, index) + "]");
        }
      }
      DeterminPath(text, Math.Min(indexBracket - 1, indexColon - 1), path);
    }

    public static string GetValue(string text, int startOffset, int arrayIndex = -1)
    {
      if (startOffset >= text.Length)
        return "-";
      char c;
      if (arrayIndex != -1)
      {
        int arrayStartIndex = text.LastIndexOf('[', startOffset);
        if (arrayStartIndex == -1)
          return "-";
        int startIndex = arrayStartIndex + 1;
        for (int i = 0; i < arrayIndex; i++)
        {
          startIndex = TryToFindAfter(text, new List<char> { ']', ',' }, out c, startOffset - 1) + 1;
          if (startIndex == -1)
            return "-";
        }
        int endIndex = TryToFindAfter(text, new List<char> { ',', ']' }, out c, startIndex);
        return endIndex == -1 || startIndex > endIndex ? "-" : text.Substring(startIndex, endIndex - startIndex);
      }
      else
      {
        int startIndex = TryToFindBefore(text, new List<char> { ':', ',' }, out c, startOffset - 1) + 1;
        if (startIndex == -1)
          return "-";
        int endIndex = TryToFindAfter(text, new List<char> { '}', ',', ']' }, out c, startIndex);
        return endIndex == -1 ? "-" : text.Substring(startIndex, endIndex - startIndex);
      }
    }

    public static int DeterminArrayIndex(string text, int index)
    {
      bool inQutes = false;
      int countCurl = 1;
      int countSquare = 1;
      int count = 0;
      while (index >= 0)
      {
        if (inQutes && text[index] != '"')
        {
          index--;
          continue;
        }

        switch (text[index])
        {
          case '"':
            inQutes = !inQutes;
            break;
          case '[':
            countSquare--;
            break;
          case ']':
            countSquare++;
            break;
          case '{':
            countCurl--;
            break;
          case '}':
            countCurl++;
            break;
          case ',':
            if (countSquare == 1 && countCurl == 1)
              count++;
            break;
        }
        if (countSquare == 0)
        {
          return count;
        }
        index--;
      }
      return -1;
    }

    public static int IndexOfNextLevel(string text, int index, out bool isArray)
    {
      if (index >= text.Length)
        index = text.Length - 1;
      bool inQutes = false;
      isArray = false;
      int countCurl = 1;
      int countSquare = 1;
      while (index >= 0)
      {
        if (inQutes && text[index] != '"')
        {
          index--;
          continue;
        }

        switch (text[index])
        {
          case '"':
            inQutes = !inQutes;
            break;
          case '{':
            countCurl--;
            break;
          case '}':
            countCurl++;
            break;
          case '[':
            countSquare--;
            break;
          case ']':
            countSquare++;
            break;
        }
        if (countCurl == 0)
        {
          return index;
        }
        if (countSquare == 0)
        {
          isArray = true;
          return index;
        }
        index--;
      }
      return -1;
    }

    private static int TryToFindBefore(string text, List<char> chars, out char c, int index)
    {
      if (index >= text.Length)
      {
        index = text.Length - 1;
      }
      bool inQutes = false;
      int curlyCount = 0;
      int squareCount = 0;
      int seperatorsToIgnore = 0;
      while (index >= 0)
      {
        if (inQutes && text[index] != '"')
        {
          index--;
          continue;
        }

        switch (text[index])
        {
          case '"':
            inQutes = !inQutes;
            break;
          case '{':
            curlyCount--;
            if (curlyCount == 0)
              seperatorsToIgnore++;
            if (curlyCount < 0)
            {
              c = '{';
              return index;
            }
            break;
          case '}':
            curlyCount++;
            break;
          case '[':
            if (squareCount == 0 && seperatorsToIgnore > 0)
            {
              seperatorsToIgnore--;
              break;
            }
            squareCount--;
            if (squareCount < 0)
            {
              c = '[';
              return index;
            }
            break;
          case ']':
            squareCount++;
            break;
          default:
            if (squareCount > 0 || curlyCount > 0)
              break;
            if (seperatorsToIgnore > 0 && (text[index] == ':' || text[index] == ','))
            {
              seperatorsToIgnore--;
              break;
            }
            foreach (char c1 in chars)
            {
              if (c1 == text[index])
              {
                c = c1;
                return index;
              }
            }
            break;
        }
        index--;
      }
      c = default(char);
      return -1;
    }

    private static int TryToFindAfter(string text, List<char> chars, out char c, int index)
    {
      if (index >= text.Length)
      {
        index = text.Length - 1;
      }
      bool inQutes = false;
      int curlyCount = 0;
      int squareCount = 0;
      int seperatorsToIgnore = 0;
      while (index < text.Length)
      {
        if (inQutes && text[index] != '"')
        {
          index++;
          continue;
        }

        switch (text[index])
        {
          case '"':
            inQutes = !inQutes;
            break;
          case '{':
            curlyCount++;
            break;
          case '}':
            curlyCount--;
            if (curlyCount == 0)
              seperatorsToIgnore++;
            if (curlyCount < 0)
            {
              c = '}';
              return index;
            }
            break;
          case '[':
            if (squareCount == 0 && seperatorsToIgnore > 0)
            {
              seperatorsToIgnore--;
              break;
            }
            squareCount++;
            break;
          case ']':
            squareCount--;
            if (squareCount < 0)
            {
              c = ']';
              return index;
            }
            break;
          default:
            if (squareCount > 0 || curlyCount > 0)
              break;
            if (seperatorsToIgnore > 0 && (text[index] == ':' || text[index] == ','))
            {
              seperatorsToIgnore--;
              break;
            }
            foreach (char c1 in chars)
            {
              if (c1 == text[index])
              {
                c = c1;
                return index;
              }
            }
            break;
        }
        index++;
      }
      c = default(char);
      return -1;
    }

    public static bool DetectIsInKey(string text, int index)
    {
      if (index == 0)
        return false;
      index = Math.Min(index, text.Length - 1);
      bool isEcaped = true;
      while (isEcaped)
      {
        index = text.LastIndexOf('"', index - 1);
        isEcaped = index != 0 && text[index - 1] == '\\';
      }
      index--;
      bool commameet = false;
      bool inQutes = false;
      while (index >= 0)
      {
        if (inQutes && text[index] != '"')
        {
          index--;
          continue;
        }

        switch (text[index])
        {
          case '"':
            inQutes = !inQutes;
            break;
          case ',':
            if (commameet)
              return false;
            commameet = true;
            break;
          case ']':
          case '}':
            bool isArray;
            index = IndexOfNextLevel(text, index - 1, out isArray) - 1;
            break;
          case '{':
            return true;
          case ':':
            if (commameet)
              return true;
            return false;
          case '[':
            return false;
        }
        index--;
      }
      return false;
    }

    public static int CountQoatsBefore(string text, int index)
    {
      int retVal = 0;
      bool isEscaped = false;
      bool isInQuat = false;
      for (int i = 0; i < index; i++)
      {
        if (text[i] == '"' && !isEscaped)
        {
          isInQuat = !isInQuat;
          retVal++;
        }
        else
        {
          if (text[i] == '\\' && isInQuat)
            isEscaped = true;
          else
            isEscaped = false;
        }
      }
      return retVal;
    }

    public static JsonObject Parse(string text)
    {
      JsonException jsonException;
      return Parse(text, out jsonException);
    }

    public static JsonObject Parse(string text, out JsonException exception)
    {
      exception = null;
      if (!text.TrimStart().StartsWith("{"))
      {
        exception = new JsonException("JsonNode should start with {", 0, 0);
        return null;
      }
      Stack<string> path = new Stack<string>();
      Stack<JsonObject> nodeStack = new Stack<JsonObject>();
      JsonObject currentNode = null;
      bool inQuats = false;
      bool isEscaped = false;
      string key = null;
      StringBuilder stringBuilder = new StringBuilder();
      int textLineIndex = 0;
      int textCharIndex = 0;
      int keyLineIndex = 0;
      int keyCharIndex = 0;
      int seperatorLineIndex = 0;
      int seperatorCharIndex = 0;
      int charIndex = 0;
      int lineIndex = 0;
      bool needSeperator = false;
      StringBuilder lineBuilder = new StringBuilder();
      for (int index = 0; index < text.Length; index++)
      {
        char c = text[index];
        lineBuilder.Append(c);
        charIndex++;
        if (lineBuilder.Length > 1 & (c == '\n' || c == '\r'))
        {
          lineIndex++;
          lineBuilder.Remove(0, lineBuilder.Length);
          charIndex = 0;
        }
        if (!inQuats && Char.IsWhiteSpace(c))
        {
          continue;
        }
        if (inQuats && (c != '"' || isEscaped))
        {
          if (stringBuilder.Length == 0)
          {
            textLineIndex = lineIndex;
            textCharIndex = charIndex;
          }
          if (c == '\\' && !isEscaped)
          {
            isEscaped = true;
            continue;
          }
          if (isEscaped)
          {
            switch (c)
            {
              case 't':
                stringBuilder.Append('\t');
                break;
              case '"':
                stringBuilder.Append('"');
                break;
              case '\'':
                stringBuilder.Append('\'');
                break;
              case 'r':
                stringBuilder.Append('\r');
                break;
              case 'n':
                stringBuilder.Append('\n');
                break;
              case '\\':
                stringBuilder.Append('\\');
                break;
              default:
                exception = new JsonException("unknown escape charctar \\" + c, lineIndex, charIndex);
                return null;
            }
            isEscaped = false;
            continue;
          }
          stringBuilder.Append(c);
          continue;
        }
        switch (c)
        {
          case '{':
            {
              if (needSeperator)
              {
                SetError(seperatorLineIndex, seperatorCharIndex, "expected comma.", out exception);
                while (nodeStack.Count > 0)
                  currentNode = nodeStack.Pop();
                return currentNode;
              }
              needSeperator = false;
              if (key == null && currentNode == null)
              {
                currentNode = new JsonNode(lineIndex, charIndex);
                nodeStack.Push(currentNode);
                break;
              }
              JsonNode newJsonNode = new JsonNode(lineIndex, charIndex);
              JsonNode parentJsonNode = currentNode as JsonNode;
              if (parentJsonNode != null)
              {
                path.Push(key);
                parentJsonNode.Add(key, newJsonNode, lineIndex, charIndex);
                key = null;
              }
              else
              {
                if (!(currentNode is JsonArray))
                  throw new JsonException("Exspected JsonArray", lineIndex, charIndex);
                path.Push("[" + DeterminArrayIndex(text, index) + "]");
                ((JsonArray)currentNode).Add(newJsonNode);
              }
              nodeStack.Push(currentNode);
              currentNode = newJsonNode;
            }
            break;
          case '}':
            {
              needSeperator = true;
              seperatorCharIndex = charIndex + 1;
              seperatorLineIndex = lineIndex;
              path.Push(key);
              CheckForNumboerAndBool(stringBuilder, currentNode, ref key, textLineIndex, textCharIndex, keyLineIndex, keyCharIndex);
              path.Pop();
              if (currentNode is JsonArray)
              {
                SetError(lineIndex, charIndex, "] expected first.", out exception);
                while (nodeStack.Count > 0)
                  currentNode = nodeStack.Pop();
                return currentNode;
              }
              if (nodeStack.Count == 0)
              {
                SetError(lineIndex, charIndex, "To many }", out exception);
                while (nodeStack.Count > 0)
                  currentNode = nodeStack.Pop();
                return currentNode;
              }
              if (path.Count > 0)
                path.Pop();
              currentNode = nodeStack.Pop();
            }
            break;
          case '[':
            {
              if (needSeperator)
              {
                SetError(seperatorLineIndex, seperatorCharIndex, "expected comma.", out exception);
                while (nodeStack.Count > 0)
                  currentNode = nodeStack.Pop();
                return currentNode;
              }
              needSeperator = false;
              if (key == null && currentNode == null)
              {
                currentNode = new JsonArray(lineIndex, charIndex);
                nodeStack.Push(currentNode);
                break;
              }
              JsonArray newJsonArray = new JsonArray(lineIndex, charIndex);
              JsonNode parentJsonNode = currentNode as JsonNode;
              if (parentJsonNode != null)
              {
                path.Push(key);

                parentJsonNode.Add(key, newJsonArray, lineIndex, charIndex);
                key = null;
              }
              else
              {
                if (!(currentNode is JsonArray))
                  throw new JsonException("Exspected JsonArray", lineIndex, charIndex);
                path.Push("[" + DeterminArrayIndex(text, index) + "]");
                ((JsonArray)currentNode).Add(newJsonArray);
              }
              nodeStack.Push(currentNode);
              currentNode = newJsonArray;
            }
            break;
          case ']':
            {
              needSeperator = true;
              seperatorCharIndex = charIndex + 1;
              seperatorLineIndex = lineIndex;
              path.Push(key);
              CheckForNumboerAndBool(stringBuilder, currentNode, ref key, lineIndex, charIndex, keyLineIndex, keyCharIndex);
              path.Pop();
              if (currentNode is JsonNode)
              {
                SetError(lineIndex, charIndex, "} expected first.", out exception);
                return null;
              }
              if (nodeStack.Count == 0)
              {
                SetError(lineIndex, charIndex, "To many ]", out exception);
                return null;
              }
              path.Pop();
              currentNode = nodeStack.Pop();
            }
            break;
          case ':':
            if (key == null)
            {
              SetError(keyLineIndex, keyCharIndex, "value missing", out exception);
              while (nodeStack.Count > 0)
                currentNode = nodeStack.Pop();
              return currentNode;
            }
            break;
          case '"':
            if (isEscaped)
            {
              stringBuilder.Append("\"");
              isEscaped = false;
              break;
            }
            if (needSeperator)
            {
              SetError(seperatorLineIndex, seperatorCharIndex, "expected comma.", out exception);
              while (nodeStack.Count > 0)
                currentNode = nodeStack.Pop();
              return currentNode;
            }
            if (inQuats)
            {
              inQuats = false;
              JsonNode node = currentNode as JsonNode;
              if (node != null)
              {
                if (key == null)
                {
                  key = stringBuilder.ToString();
                  keyLineIndex = lineIndex;
                  keyCharIndex = charIndex;
                  path.Push(key);

                  path.Pop();
                }
                else
                {
                  path.Push(key);
                  node.Add(key, new JsonValue(stringBuilder.ToString(), textLineIndex, textCharIndex), lineIndex, charIndex);
                  path.Pop();
                  key = null;
                  needSeperator = true;
                  seperatorCharIndex = charIndex + 1;
                  seperatorLineIndex = lineIndex;
                }
              }
              else
              {
                if (!(currentNode is JsonArray))
                  throw new JsonException("Exspected JsonArray", lineIndex, charIndex);
                JsonArray jsonArray = currentNode as JsonArray;
                jsonArray.Add(new JsonValue(stringBuilder.ToString(), textLineIndex, textCharIndex));
              }
              stringBuilder.Remove(0, stringBuilder.Length);
            }
            else
            {
              if (stringBuilder.Length > 0)
              {
                SetError(textLineIndex, textCharIndex, "text in unexpected place", out exception);
                while (nodeStack.Count > 0)
                  currentNode = nodeStack.Pop();
                return currentNode;
              }
              inQuats = true;
            }
            break;
          case '\\':
            if (!inQuats)
            {
              SetError(lineIndex, charIndex, "\\ found in wrong place", out exception);
              while (nodeStack.Count > 0)
                currentNode = nodeStack.Pop();
              return currentNode;
            }
            if (isEscaped)
            {
              stringBuilder.Append("\\");
              isEscaped = false;
            }
            else
              isEscaped = true;
            break;
          case ',':
            {
              if (key == null && !needSeperator && currentNode is JsonNode)
              {
                SetError(lineIndex, charIndex, "extra comma found", out exception);
                while (nodeStack.Count > 0)
                  currentNode = nodeStack.Pop();
                return currentNode;
              }
              needSeperator = false;
              path.Push(key);
              CheckForNumboerAndBool(stringBuilder, currentNode, ref key, lineIndex, charIndex, keyLineIndex, keyCharIndex);
              path.Pop();
            }
            break;
          default:
            isEscaped = false;
            if (needSeperator)
            {
              SetError(seperatorLineIndex, seperatorCharIndex, "expected comma.", out exception);
              while (nodeStack.Count > 0)
                currentNode = nodeStack.Pop();
              return currentNode;
            }
            if (stringBuilder.Length == 0)
            {
              textLineIndex = lineIndex;
              textCharIndex = charIndex;
            }
            stringBuilder.Append(c);
            break;
        }
      }
      if (stringBuilder.Length > 0)
        SetError(textLineIndex, textCharIndex, "text in unexpected place", out exception);
      if (key != null)
        SetError(keyLineIndex, keyCharIndex, "value missing", out exception);
      if (nodeStack.Count > 0)
      {
        SetError(lineBuilder.Length > 0 ? lineIndex : lineIndex - 1, charIndex, currentNode is JsonArray ? "missing ]" : "missing }", out exception);
      }
      while (nodeStack.Count > 0)
        currentNode = nodeStack.Pop();
      return currentNode;
    }

    private static void SetError(int lineIndex, int charIndex, string message, out JsonException exception)
    {
      exception = new JsonException(message, lineIndex, charIndex);
    }

    private static void CheckForNumboerAndBool(StringBuilder stringBuilder, object currentNode, ref string key, int lineIndex, int charIndex, int keyLineIndex, int keyCharIndex)
    {
      if (stringBuilder.Length > 0)
      {
        object value = null;
        int intValue;
        double floatValue;
        bool boolValue;
        if (int.TryParse(stringBuilder.ToString(), NumberStyles.Integer, null, out intValue))
        {
          stringBuilder.Remove(0, stringBuilder.Length);
          value = intValue;
        }
        else if (double.TryParse(stringBuilder.ToString(), NumberStyles.Float, null, out floatValue))
        {
          stringBuilder.Remove(0, stringBuilder.Length);
          value = floatValue;
        }
        else if (bool.TryParse(stringBuilder.ToString(), out boolValue))
        {
          stringBuilder.Remove(0, stringBuilder.Length);
          value = boolValue;
        }
        else if (stringBuilder.ToString() == "null")
        {
          stringBuilder.Remove(0, stringBuilder.Length);
        }
        JsonNode jsonNode = currentNode as JsonNode;
        if (jsonNode != null)
        {
          jsonNode.Add(key, new JsonValue(value, lineIndex, charIndex), keyLineIndex, keyCharIndex);
          key = null;
        }
        else
        {
          ((JsonArray)currentNode).Add(new JsonValue(value, lineIndex, charIndex));
        }
      }
    }
  }
}
