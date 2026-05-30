using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Forms;

namespace OpStream.BlazorComponents.Inputs
{
    public class InputFile : Microsoft.AspNetCore.Components.Forms.InputFile, IDisposable
    {
        [Parameter] public string? Class { get; set; }
        [Parameter] public string? FieldName { get; set; }
        [CascadingParameter] public ICollabFormContext? Context { get; set; }

        private FieldEditor? _editor;
        private bool _locked => _editor is not null;
        private ICollabFormContext? _subscribed;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            if (!ReferenceEquals(_subscribed, Context))
            {
                if (_subscribed is not null) _subscribed.StateChanged -= OnContextChanged;
                _subscribed = Context;
                if (_subscribed is not null) _subscribed.StateChanged += OnContextChanged;
            }

            if (!string.IsNullOrEmpty(FieldName))
            {
                _editor = Context?.EditorOf(FieldName);
            }
        }

        private void OnContextChanged()
        {
            if (!string.IsNullOrEmpty(FieldName))
            {
                _editor = Context?.EditorOf(FieldName);
            }
            InvokeAsync(StateHasChanged);
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", $"collab-field {Class} {(_locked ? "locked" : "")}");
            if (_locked) builder.AddAttribute(2, "style", $"--lock-color:{_editor!.Color};");

            if (!string.IsNullOrEmpty(FieldName))
            {
                builder.AddAttribute(3, "onfocusin", EventCallback.Factory.Create<FocusEventArgs>(this,
                    () => Context?.NotifyFocusAsync(FieldName) ?? Task.CompletedTask));
                builder.AddAttribute(4, "onfocusout", EventCallback.Factory.Create<FocusEventArgs>(this,
                    () => Context?.NotifyBlurAsync(FieldName) ?? Task.CompletedTask));
            }

            builder.OpenElement(5, "div");
            builder.AddAttribute(6, "class", "collab-field-head");
            if (_locked)
            {
                builder.OpenElement(7, "span");
                builder.AddAttribute(8, "class", "lock-badge");
                builder.AddAttribute(9, "style", $"background:{_editor!.Color}");
                builder.AddContent(10, $"{_editor!.Name} is editing…");
                builder.CloseElement();
            }
            builder.CloseElement();

            builder.OpenElement(11, "fieldset");
            builder.AddAttribute(12, "class", "collab-field-inner");
            builder.AddAttribute(13, "disabled", _locked);

            base.BuildRenderTree(builder);

            builder.CloseElement(); // </fieldset>
            builder.CloseElement(); // </div>
        }

        public void Dispose()
        {
            if (_subscribed is not null) _subscribed.StateChanged -= OnContextChanged;
        }
    }
}
