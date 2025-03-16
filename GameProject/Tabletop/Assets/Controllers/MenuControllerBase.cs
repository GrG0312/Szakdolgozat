using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Controllers.DataObjects;
using Controllers.Data;

namespace Controllers
{
    public abstract class MenuControllerBase<T> : BaseController<T> where T : MenuControllerBase<T>
    {
        [SerializeField] protected GameObject menuParent;

        protected readonly Stack<ScreenType> screenStack = new Stack<ScreenType>();

        protected void ChangeScreen(ScreenType type)
        {
            if (screenStack.TryPeek(out ScreenType prev) && prev == type)
            {
                return;
            }
            screenStack.Push(type);

            foreach (Transform child in menuParent.transform)
            {
                if (child.gameObject.TryGetComponent<MenuElement>(out MenuElement element))
                {
                    child.gameObject.SetActive(element.ScreenType == type);
                }
            }
            //StackDebug("Change");
        }
        public void Back()
        {
            try
            {
                screenStack.Pop();
                ChangeScreen(screenStack.Pop());
            }
            catch (InvalidOperationException)
            {
                Debug.LogWarning("Quitting application because stack was empty...");
                Application.Quit();
            }
            //StackDebug("Back");
        }
        private void StackDebug(string message = "")
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(message + ": ");
            sb.Append("[");
            foreach (ScreenType type in screenStack)
            {
                sb.Append(" " + type);
            }
            sb.Append(" ]");
            Debug.Log(sb.ToString());
        }
    }
}
