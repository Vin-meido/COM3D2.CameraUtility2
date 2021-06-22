using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEngine.VR;
using BepInEx;
using BepInEx.Configuration;

namespace COM3D2.CameraUtility2.Plugin
{
    class Man
    {
        private Maid man;
        private GameObject head;

        public int ManNumber { get; private set; }

        public Transform HeadTransform
        {
            get
            {
                return head.transform;
            }
        }

        public Quaternion LookRotation
        {
            get
            {
                return Quaternion.LookRotation(-this.HeadTransform.up, this.HeadTransform.forward);
            }
        }

        public bool Visible
        {
            get
            {
                return man.isActiveAndEnabled && man.Visible;
            }
        }

        public bool HeadVisible
        {
            set
            {
                SetRendererEnabled(head, value);
            }
        }

        public override string ToString()
        {
            return man.ToString();
        }

        private Man() { }

        private static Man GetMan(int manNumber=0, bool onlyIfActiveAndVisible=false)
        {
            var man = GameMain.Instance.CharacterMgr.GetMan(manNumber);
            if (onlyIfActiveAndVisible && !(man.isActiveAndEnabled && man.Visible)) return null;
            if (!man || !man.body0 || !man.body0.trsHead) return null;
            var head = man.body0.trsHead.gameObject;
            if (!head) return null;
            var mhead = FindByNameInChildren(head, "mhead");
            if (!mhead) return null;
            var actual_head = FindByNameInChildren(mhead, "ManHead");

            return new Man
            {
                man = man,
                head = actual_head,
                ManNumber = manNumber
            };
        }

        public static Man GetVisibleMan(int start=-1)
        {
            var manCount = GameMain.Instance.CharacterMgr.GetManCount();
            start += 1;
            if (start < manCount && start < 0)
            {
                start = 0;
            }

            for (int number = start; number < manCount; number++)
            {
                var man = GetMan(number, true);
                if (man != null && man.Visible)
                {
                    return man;
                }
            }

            return null;
        }

        public static void VisibleAllManHead()
        {
            var manCount = GameMain.Instance.CharacterMgr.GetManCount();
            for (int number = 0; number < manCount; number++)
            {
                var man = GetMan(number);
                if (man != null)
                {
                    man.HeadVisible = true;
                }
            }
        }

        private static GameObject FindByNameInChildren(GameObject parent, string name)
        {
            foreach (Transform transform in parent.transform)
            {
                if (transform.name.IndexOf(name) > -1)
                {
                    return transform.gameObject;
                }
            }
            return null;
        }

        private static void SetRendererEnabled(GameObject obj, bool enabled)
        {
            var renderer = obj.GetComponent<Renderer>();
            if (renderer)
            {
                renderer.enabled = enabled;
            }
        }

    }
}