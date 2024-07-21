using THLL.BaseSystem;
using UnityEngine;

namespace THLL.LocationSystem
{
    public class GameLocationMgr : GameBehaviour
    {
        protected override void Start()
        {
            base.Start();
            ClassC classC = new();
            ClassD classD = new(classC);
            classD.Show();
        }
    }

    public abstract class BaseA
    {
        public string Name => "Doremy";
    }

    public abstract class BaseB<T> where T : BaseA
    {
        private readonly T _data;
        public string Name => _data.Name;

        public BaseB(T data)
        {
            _data = data;
        }
    }

    public class ClassC : BaseA
    {
        public new string Name => "Sweet";
    }

    public class ClassD : BaseB<ClassC>
    {
        public void Show()
        {
            Debug.Log(Name);
        }

        public ClassD(ClassC data) : base(data) { }
    }
}