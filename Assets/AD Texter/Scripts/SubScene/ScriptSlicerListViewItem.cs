using System.Collections;
using System.Collections.Generic;
using AD.BASE;
using AD.UI;
using AD.Utility;
using UnityEngine;
using UnityEngine.UIElements;

namespace AD.Sample.Texter.Scene
{
    public class ScriptSlicerListViewItem : ListViewItem
    {
        public InputField CharacterName, AudioAssets;
        public ModernUIInputField WordInputText;
        public ModernUIButton PreviewButton,DeleteButton;

        public InputField CommandInputField;
        public ModernUIDropdown SelectCharaterDrop;
        public ModernUIButton RefreshSelectDrop;
        public InputField ImgPathField, PositionX;

        public InputField AddNewCharacterThisName;
        public ModernUIButton AddCharacterThis, RemoveCharacterThis;

        public ScriptItemEntry mEntry;

        public override ListViewItem Init()
        {
            return this;
        }

        public int currentCharacter;

        public void SetupScriptSlicerTarget(ScriptItemEntry entry)
        {
            mEntry = entry;
            CharacterName.SetText(entry.Name);
            CharacterName.AddListener(T => entry.Name = T);
            AudioAssets.SetText(entry.SoundAssets);
            AudioAssets.AddListener(T => entry.SoundAssets = T);
            WordInputText.SetText(entry.Words);
            WordInputText.AddListener(T => entry.Words = T);
            PreviewButton.AddListener(()=>ScriptSlicerManager.PreviewOneEntry(entry));
            DeleteButton.AddListener(() => ScriptSlicerManager.Remove(this));
            CommandInputField.SetText(entry.Command);
            CommandInputField.AddListener(T => entry.Command = T);

            AddCharacterThis.AddListener(() =>
            {
                if (string.IsNullOrEmpty(AddNewCharacterThisName.text)) return;
                if (mEntry.CharacterNameList.FindIndex(T => T == AddNewCharacterThisName.text) != -1) return;
                mEntry.CharacterNameList.Add(AddNewCharacterThisName.text);
                mEntry.CharacterXPositionList.Add(0);
                mEntry.CharacterImageKeyList.Add("");
            });
            RemoveCharacterThis.AddListener(() =>
            {
                if (mEntry.CharacterNameList.Count > 0)
                {
                    mEntry.CharacterImageKeyList.RemoveAt(currentCharacter);
                    mEntry.CharacterXPositionList.RemoveAt(currentCharacter);
                    mEntry.CharacterNameList.RemoveAt(currentCharacter);
                    currentCharacter = 0;
                    RefreshCharactersStatusField();
                }
                currentCharacter = 0;
                SelectCharaterDrop.ClearOptions();
                if (entry.CharacterNameList.Count > 0)
                    SelectCharaterDrop.AddOption(entry.CharacterNameList.ToArray());
            });

            SelectCharaterDrop.ClearOptions();
            SelectCharaterDrop.AddOption(entry.CharacterNameList.ToArray());
            SelectCharaterDrop.AddListener(T =>
            {
                currentCharacter = entry.CharacterNameList.FindIndex(P => P == T).Share(out int tryResult) == -1 ? currentCharacter : tryResult;
                RefreshCharactersStatusField();
            });

            ImgPathField.AddListener(SetImgPath);
            PositionX.AddListener(SetPosX);

            RefreshSelectDrop.AddListener(() =>
            {
                SelectCharaterDrop.ClearOptions();
                SelectCharaterDrop.AddOption(entry.CharacterNameList.ToArray());
            });
        }

        public void RefreshCharactersStatusField()
        {
            if (mEntry.CharacterNameList.Count > 0)
            {
                SelectCharaterDrop.SetTitle(mEntry.CharacterNameList[currentCharacter]);
                ImgPathField.SetTextWithoutNotify(mEntry.CharacterImageKeyList[currentCharacter]);
                PositionX.SetTextWithoutNotify(mEntry.CharacterXPositionList[currentCharacter].ToString());
            }
            else
            {
                SelectCharaterDrop.SetTitle("目前没有角色会在该语句播放时修改");
                ImgPathField.SetTextWithoutNotify("");
                PositionX.SetTextWithoutNotify("");
            }
        }

        private void SetImgPath(string T)
        {
            if (mEntry.CharacterNameList.Count == 0)
            {
                ImgPathField.SetTextWithoutNotify("");
                PositionX.SetTextWithoutNotify("");
                return;
            }
            mEntry.CharacterImageKeyList[currentCharacter] = T;
        }
        private void SetPosX(string T)
        {
            if (mEntry.CharacterNameList.Count == 0)
            {
                ImgPathField.SetTextWithoutNotify("");
                PositionX.SetTextWithoutNotify("");
                return;
            }
            if (ArithmeticExtension.TryParse(T, out var result))
                mEntry.CharacterXPositionList[currentCharacter] = result.ReadValue();
            else
                PositionX.SetTextWithoutNotify(T);
        }
    }
}
