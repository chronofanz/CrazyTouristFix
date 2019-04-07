using CrazyTouristFix.Detours;
using ICities;
using System;
using System.Collections.Generic;

namespace CrazyTouristFix
{
    public class CrazyTouristFixMod : IUserMod, ILoadingExtension
    {
        public string Name => "Crazy Tourist Fix";
        public string Description => "Temporary fix for despawning tourists at outside connections";

        public void OnCreated(ILoading loading)
        {
        }

        public void OnLevelLoaded(LoadMode mode)
        {
            var mapping = new Dictionary<Type, Type>
            {
                {typeof (ResidentAI), typeof (ResidentAIDetour)},
                {typeof (TouristAI), typeof (TouristAIDetour)},
            };

            ReplaceCustomAI(mapping);
        }

        public void OnLevelUnloading()
        {
            var mapping = new Dictionary<Type, Type>
            {
                {typeof (ResidentAIDetour), typeof (ResidentAI)},
                {typeof (TouristAIDetour), typeof (TouristAI)},
            };

            ReplaceCustomAI(mapping);
        }

        public void OnReleased()
        {
        }

        private static void ReplaceCustomAI(Dictionary<Type, Type> mapping)
        {
            {
                int num = PrefabCollection<CitizenInfo>.PrefabCount();
                for (uint i = 0; i < num; i++)
                {
                    var info = PrefabCollection<CitizenInfo>.GetPrefab(i);
                    AdjustAI(info, mapping);
                }
            }

            {
                int num = PrefabCollection<VehicleInfo>.PrefabCount();
                for (uint i = 0; i < num; i++)
                {
                    var vi = PrefabCollection<VehicleInfo>.GetPrefab(i);
                    AdjustAI(vi, mapping);
                }
            }
        }

        #region AdjustAI methods

        // chronofanz: WARNING, I think this only works if the type we are replacing is a 'leaf' type in the hierarchies
        //             of types!
        public static void AdjustAI(CitizenInfo info, Dictionary<Type, Type> componentRemap)
        {
            if (info == null)
            {
                return;
            }

            var newAI = ConstructNewAI<CitizenInfo, CitizenAI>(info, componentRemap);
            if (newAI != null)
            {
                UnityEngine.Debug.Log($"CrazyTouristFixMod::AdjustAI: CitizenInfo ${info.name} replaced CitizenAI type {newAI.GetType().Name}");
                newAI.m_info = info;
                info.m_citizenAI = newAI;
                newAI.InitializeAI();
            }
        }

        // chronofanz: WARNING, I think this only works if the type we are replacing is a 'leaf' type in the hierarchies
        //             of types!
        public static void AdjustAI(VehicleInfo info, Dictionary<Type, Type> componentRemap)
        {
            if (info == null)
            {
                return;
            }

            var newAI = ConstructNewAI<VehicleInfo, VehicleAI>(info, componentRemap);
            if (newAI != null)
            {
                UnityEngine.Debug.Log($"CrazyTouristFixMod::AdjustAI: VehicleInfo ${info.name} replaced VehicleAI type {newAI.GetType().Name}");
                newAI.m_info = info;
                info.m_vehicleAI = newAI;
                newAI.InitializeAI();
            }
        }

        private static AI ConstructNewAI<Info, AI>(Info info, Dictionary<Type, Type> componentRemap)
            where Info : PrefabInfo
            where AI : PrefabAI
        {
            var oldAI = info.GetComponent<AI>();
            if (oldAI == null)
            {
                UnityEngine.Debug.Log($"CrazyTouristFixMod::AdjustAI: PrefabInfo {info.name} uses PrefabAI of unexpected type!");
                return null;
            }

            var oldCompType = oldAI.GetType();
            if (!componentRemap.TryGetValue(oldCompType, out Type newCompType))
            {
                return null;
            }

            // pull fields from old AI and destroy it
            var fields = ExtractFields(oldAI);
            UnityEngine.Object.DestroyObject(oldAI);

            // create new AI and add it to the game
            var newAI = info.gameObject.AddComponent(newCompType) as AI;
            if (newAI == null)
            {
                UnityEngine.Debug.Log($"CrazyTouristFixMod::AdjustAI: Aborting, ${newCompType} is not derived form VehicleAI!");
                return null;
            }

            // import old AI settings to new AI instance and initialize it
            SetFields(newAI, fields);
            return newAI;
        }

        private static Dictionary<string, object> ExtractFields(object a)
        {
            var fields = a.GetType().GetFields();
            var dict = new Dictionary<string, object>(fields.Length);
            for (int i = 0; i < fields.Length; i++)
            {
                var af = fields[i];
                dict[af.Name] = af.GetValue(a);
            }
            return dict;
        }

        private static void SetFields(object b, Dictionary<string, object> fieldValues)
        {
            var bType = b.GetType();
            foreach (var kvp in fieldValues)
            {
                var bf = bType.GetField(kvp.Key);
                if (bf == null)
                {
                    continue;
                }

                bf.SetValue(b, kvp.Value);
            }
        }

        #endregion
    }
}
