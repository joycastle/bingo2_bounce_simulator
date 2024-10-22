using UnityEngine;

namespace DefaultNamespace
{
    public enum EFrameType
    {
        None,
        Nail,
        Bumper,
        Wall,
        Outlet,
    }
    public class IdentifierComp : MonoBehaviour
    {
        public EFrameType Type;
        public int ID;

        public override string ToString()
        {
            return PathDataManager.GetIdentifier(Type, ID);
        }
    }
}