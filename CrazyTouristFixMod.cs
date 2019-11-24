/*
MIT License

Copyright(c) 2019 chronofanz

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using ColossalFramework;
using Harmony;
using ICities;
using System.Reflection;
using UnityEngine;

namespace CrazyTouristFix
{
    public class CrazyTouristFixMod : IUserMod, ILoadingExtension
    {
        public const string version = "1.0.0";
        public string Name => "Crazy Tourist Fix 1.0.0";
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
