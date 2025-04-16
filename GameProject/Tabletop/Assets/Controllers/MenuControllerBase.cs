using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Controllers.Objects;
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
        }
        public void BackOrQuit()
        {
            try
            {
                screenStack.Pop();
                ChangeScreen(screenStack.Pop());
            }
            catch (InvalidOperationException)
            {
                Application.Quit();
            }
        }
    }
}
