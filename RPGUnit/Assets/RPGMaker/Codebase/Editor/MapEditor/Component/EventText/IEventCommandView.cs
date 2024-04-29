using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText
{
    /**
     * EventCommandView全体で共通のインターフェース.
     * （AbstractEventTextというのがあるが、Text以外でもこのインターフェースはimplementすること）
     */
    public interface IEventCommandView
    {
        public VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand);
    }
}