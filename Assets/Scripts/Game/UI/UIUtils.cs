using UnityEngine;

namespace Gfen.Game.UI
{
    public static class UIUtils
    {
        public static T InstantiateUICell<T>(Transform rootTransform, T templateCell) where T : UICell
        {
            var instantiateCell = Object.Instantiate<T>(templateCell);
            instantiateCell.transform.SetParent(rootTransform, false);

            return instantiateCell;
        }
    }
}
