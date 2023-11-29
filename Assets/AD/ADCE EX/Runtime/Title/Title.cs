using System.Collections;
using System.Collections.Generic;
using AD.BASE;
using UnityEngine;

namespace AD.Experimental.GameEditor
{
    public class Title : ADController
    {
        public AD.UI.Text Source;

        private void Start()
        {
            GameEditorApp.instance.RegisterController(this);
        }

        public override void Init()
        {
            Source.SetText("Custom Editor");
        }

        public void SetTitle(string title)
        {
            Source.SetText(title);
        }

    }
}
