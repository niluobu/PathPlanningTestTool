using Framework.MVP;
using UniRx;
using UnityEngine;
using Zenject;

namespace Project.Main
{
    public class EditorPresenter : IPresenter, IInitializable
    {
        private readonly IParentView<EditorWidgets> _view;

        public EditorPresenter(IParentView<EditorWidgets> view)
        {
            _view = view;

            _view.ViewObservable.Load.Subscribe(_ => OnLoad());
        }

        public void Initialize()
        {
            _view.Load(true);
        }

        private void OnLoad()
        {
            _view.Widgets.SaveButton.Clicked.Subscribe(_ =>
            {
                Debug.Log("Save");
            });

            _view.Widgets.ReturnButton.Clicked.Subscribe(_ =>
            {
                Debug.Log("Return");
            });

            _view.Widgets.RevocationButton.Clicked.Subscribe(_ =>
            {
                Debug.Log("Revocation");
            });
        }
    }
}

