using UnityEngine;

namespace THLL.BaseSystem
{
    public abstract class BaseGameAssets<TData, TValue> : ScriptableObject where TValue : BaseGameEntity<TData> where TData : BaseGameData
    {
        //目标的ID
        public string targetID;

        //应用于目标
        public abstract void Apply(TValue target);
    }
}
