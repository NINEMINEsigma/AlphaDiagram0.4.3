using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AD.UI
{
    [Serializable]
    public class CharacterBox
    {
        public string key;
        public GameObject Item;
    }

    public class PlayerInfoBox : PropertyModule
    {
        [SerializeField] private List<CharacterBox> characters = new();

        public Text Title, SubTitle;

        public int CurrentIndex = 0;

        public void SelectCharacter(int index)
        {
            if (index >= 0 && index < characters.Count)
            {
                CurrentIndex = index;
                for (int i = 0; i < characters.Count; i++)
                {
                    CharacterBox character = characters[i];
                    character.Item.gameObject.SetActive(i == index);
                }
            }
            else Debug.LogError("Out of range");
        }

        private void OnValidate()
        {
            SelectCharacter(CurrentIndex);
        }

    }
}
