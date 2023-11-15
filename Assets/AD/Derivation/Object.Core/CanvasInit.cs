using System;
using System.Collections.Generic;
using UnityEngine;

namespace AD.UI
{
	[Serializable]
	public class CanvasInitializerObject
    {
		[SerializeField] private Canvas EADCanvas = null;
		[SerializeField] private Camera EADCamera = null;
		[SerializeField] private CanvasExtension.CanvasAdaptive EADAdaptiveType = CanvasExtension.CanvasAdaptive.HorizontalAndVertical;

		public void Initialize()
		{
			if (EADCanvas == null || EADCamera == null) throw new System.Exception("AD.UI.CanvasExtension.CanvasInitializerObject.member null error");
            EADCanvas.Adaptive(EADCamera, EADAdaptiveType);
        }
    }

	[Serializable]
	public class CanvasInitializer
	{
		[SerializeField] private List<CanvasInitializerObject> EADObjects = new List<CanvasInitializerObject>();

		public void Initialize()
		{
			foreach (var cat in EADObjects) cat.Initialize();
        }
	}
}