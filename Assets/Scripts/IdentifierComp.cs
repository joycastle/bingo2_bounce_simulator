using UnityEngine;

namespace DefaultNamespace
{
    public enum EFrameType
    {
        None,
        Nail,
        Bumper,
        Wall,
        CollectionZone,
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