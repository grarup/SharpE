using SharpE.Definitions.Editor;

namespace SharpE.QubicleViewer
{
  class EditorCreator :IEditorCreator
  {
    public IEditor CreateEditor()
    {
      return new QubicleEditor();
    }
  }
}
