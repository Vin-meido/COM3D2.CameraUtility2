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
    public static class GameObjectExtensions
    {
        public static GameObject FindByNameInChildren(this GameObject parent, string name)
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
    }

    public class Man
    {
        private Maid man;
        private GameObject head;
        private List<Renderer> renderers;

        public int ManNumber { get; private set; }

        public Transform HeadTransform
        {
            get
            {
                return head.transform;
            }
        }

        Vector3 Forward => man.IsCrcBody ? HeadTransform.right : HeadTransform.up;
        Vector3 Up => man.IsCrcBody ? HeadTransform.up : HeadTransform.forward;

        public Quaternion LookRotation
        {
            get
            {
                return Quaternion.LookRotation(-Forward, Up);
            }
        }

        public Vector3 LookOffset(float forwardOffset, float upOffset)
        {
            return (Forward * forwardOffset) + (Up * upOffset);
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
                if(man.IsCrcBody)
                {
                    foreach(var renderer in this.renderers)
                    {
                        renderer.enabled = value;
                    }
                }
                else
                {
                    SetRendererEnabled(head, value);
                }
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

            if (!man.IsCrcBody) {
                var actual_head = head.FindByNameInChildren("mhead")?.FindByNameInChildren("ManHead");
                if (actual_head == null) return null;

                return new Man
                {
                    man = man,
                    head = actual_head,
                    ManNumber = manNumber
                };
            }
            else
            {
                var faceBone = head.FindByNameInChildren("SM_Face")?.FindByNameInChildren("Bone_Face");
                if (faceBone == null) return null;
                return new Man
                {
                    man = man,
                    head = faceBone,
                    ManNumber = manNumber,
                    renderers = FindRenderers(head),
                };
            }
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

        public static List<Renderer> FindRenderers(GameObject go)
        {
            var current = new List<Renderer>();
            FindRenderers(go, current);
            return current;
        }

        static void FindRenderers(GameObject go, List<Renderer> current)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                current.Add(renderer);
            }

            for(var i=0; i<go.transform.childCount; i++)
            {
                var child = go.transform.GetChild(i).gameObject;
                FindRenderers(child, current);
            }
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