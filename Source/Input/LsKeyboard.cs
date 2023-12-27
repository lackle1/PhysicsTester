using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework.Input;

namespace Grondslag
{
    public class LsKeyboard
    {
        private KeyboardState _newKeyboard, _oldKeyboard;

        private List<LsKey> _pressedKeys = new List<LsKey>(), _previousPressedKeys = new List<LsKey>();

        public LsKeyboard()
        {

        }

        public virtual void Update()
        {
            _newKeyboard = Keyboard.GetState();
            GetPressedKeys();
        }

        public void UpdateOld()
        {
            _oldKeyboard = _newKeyboard;

            //previousPressedKeys = pressedKeys;
            //this creates a reference to the same List object

            _previousPressedKeys = new List<LsKey>();

            for (int i = 0; i < _pressedKeys.Count; i++)
            {
                _previousPressedKeys.Add(_pressedKeys[i]);
            }

            //this creates an entirely new list and copies over the values

        }

        public bool GetPress(string key)
        {
            for (int i = 0; i < _pressedKeys.Count; i++)
            {
                if (_pressedKeys[i].key == key)
                {
                    return true;
                }
            }

            return false;
        }

        public bool GetSinglePress(string key)
        {
            if (GetPress(key) && !_previousPressedKeys.Select(LsKey => LsKey.key).Contains(key))
            {
                return true;
            }

            return false;
        }

        public bool GetRelease(string key)
        {
            if (!GetPress(key) && _previousPressedKeys.Select(LsKey => LsKey.key).Contains(key))
            {
                return true;
            }

            return false;
        }

        public virtual void GetPressedKeys()
        {
            _pressedKeys.Clear();
            for (int i = 0; i < _newKeyboard.GetPressedKeys().Length; i++)
            {
                _pressedKeys.Add(new LsKey(_newKeyboard.GetPressedKeys()[i].ToString(), 1));
            }
        }
    }
}
