using System;
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

        private void Awake()
        {
            var text = GetComponentInChildren<TMPro.TextMeshPro>();
#if BOUNCE_DEBUG
            SetText(ID.ToString());
#else
            if (text != null)
            {
                text.gameObject.SetActive(false);
            }
#endif
        }
     
#if BOUNCE_DEBUG
        public void SetText(string s)
        {
            var text = GetComponentInChildren<TMPro.TextMeshPro>();
            if (text != null)
            {
                text.text = s;
            }
        }
#endif
        
        public override string ToString()
        {
            return PathDataManager.GetIdentifier(Type, ID);
        }
    }
}