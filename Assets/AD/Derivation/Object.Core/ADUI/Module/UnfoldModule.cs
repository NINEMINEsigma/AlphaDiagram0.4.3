using System.Collections.Generic;
using UnityEngine;

namespace AD.UI
{
    public class UnfoldModule : PropertyModule
    {
        public Animator animator;

        public bool IsFolded { get; private set; } = true;

        public List<UnfoldModule> SubUnfoldModules = new();

        public bool IsButtonOnHover = false;
        public ModernUIButton mButton;

        protected override void Start()
        {
            base.Start();
            for (int i = 0; i < SubUnfoldModules.Count; i++)
            {
                UnfoldModule single = SubUnfoldModules[i];
                Add(i, single.gameObject);
            }
            if (IsButtonOnHover) mButton.hoverEvent.AddListener(SwitchMode);
            else mButton.clickEvent.AddListener(SwitchMode);

            animator.Play("Fold");
        }

        protected override void LetChildAdd(GameObject child)
        {

        }

        protected override void LetChildDestroy(GameObject child)
        {

        }

        public void Fold()
        {
            foreach (var child in Childs)
            {
                if (child.Value.TryGetComponent(out UnfoldModule unfold))
                {
                    unfold.Fold();
                }
            }
            if (!IsFolded)
                animator.Play("Fold");
            IsFolded = true;
        }

        public void Unfold()
        {
            foreach (var child in Childs)
            {
                if (child.Value.TryGetComponent(out UnfoldModule unfold))
                {
                    unfold.Unfold();
                }
            }
            if (IsFolded)
                animator.Play("Unfold");
            IsFolded = false;
        }

        public void FoldAllChild()
        {
            foreach (var child in Childs)
            {
                if (child.Value.TryGetComponent(out UnfoldModule unfold))
                {
                    unfold.Fold();
                }
            }
        }

        public void UnfoldAllChild()
        {
            foreach (var child in Childs)
            {
                if (child.Value.TryGetComponent(out UnfoldModule unfold))
                {
                    unfold.Unfold();
                }
            }
        }

        public void Select(UnfoldModule single)
        {
            foreach (var child in Childs)
            {
                if (child.Value.TryGetComponent(out UnfoldModule unfold))
                {
                    if (single != unfold)
                        unfold.Fold();
                }
            }
            single.Unfold();
        }

        public void SwitchMode()
        {
            if (IsFolded)
            {
                Unfold();
            }
            else
            {
                Fold();
            }
        }
    }
}
