using ColossalFramework;
using Harmony;
using ICities;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CrazyTouristFix
{
    public class CrazyTouristFixMod : IUserMod, ILoadingExtension
    {
        public string Name => "Crazy Tourist Fix";
        public string Description => "Temporary fix for despawning tourists at outside connections";

        public void OnCreated(ILoading loading)
        {
            var harmony = HarmonyInstance.Create("com.pachang.crazytouristfix");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public void OnLevelLoaded(LoadMode mode)
        {
        }

        public void OnLevelUnloading()
        {
        }

        public void OnReleased()
        {
        }
    }

    [HarmonyPatch(typeof(ResidentAI))]
    [HarmonyPatch(nameof(ResidentAI.SetCurrentVehicle))]
    public class ResidentAIPatchSetCurrentVehicle
    {
        public static bool Prefix(ushort instanceID, ref CitizenInstance citizenData, ushort vehicleID, uint unitID, Vector3 position)
        {
            // Prevent the citizen from adjusting their target position by leaving a vehicle if they are at an outside connection!
            // Following code motivated by logic of HumanAI::SetCurrentVehicle.
            if (citizenData.m_path == 0 && citizenData.m_targetBuilding != 0 && Singleton<BuildingManager>.instance.m_buildings.m_buffer[citizenData.m_targetBuilding].Info.m_buildingAI is OutsideConnectionAI)
            {
                uint citizen = citizenData.m_citizen;
                Singleton<CitizenManager>.instance.m_citizens.m_buffer[citizen].SetVehicle(citizen, 0, 0);
            }

            // Execute base code.
            return true;
        }
    }

    [HarmonyPatch(typeof(TouristAI))]
    [HarmonyPatch(nameof(TouristAI.SetCurrentVehicle))]
    public class TouristAIPatchSetCurrentVehicle
    {
        public static bool Prefix(ushort instanceID, ref CitizenInstance citizenData, ushort vehicleID, uint unitID, Vector3 position)
        {
            // Prevent the citizen from adjusting their target position by leaving a vehicle if they are at an outside connection!
            // Following code motivated by logic of HumanAI::SetCurrentVehicle.
            if (citizenData.m_path == 0 && citizenData.m_targetBuilding != 0 && Singleton<BuildingManager>.instance.m_buildings.m_buffer[citizenData.m_targetBuilding].Info.m_buildingAI is OutsideConnectionAI)
            {
                uint citizen = citizenData.m_citizen;
                Singleton<CitizenManager>.instance.m_citizens.m_buffer[citizen].SetVehicle(citizen, 0, 0);
            }

            // Execute base code.
            return true;
        }
    }
}
