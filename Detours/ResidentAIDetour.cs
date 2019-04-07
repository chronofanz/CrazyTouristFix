using ColossalFramework;
using CrazyTouristFix.Attributes;
using UnityEngine;

namespace CrazyTouristFix.Detours
{
    public class ResidentAIDetour : ResidentAI
    {
        public override bool SetCurrentVehicle(ushort instanceID, ref CitizenInstance citizenData, ushort vehicleID, uint unitID, Vector3 position)
        {
            #region Mod

            // Prevent the citizen from adjusting their target position by leaving a vehicle if they are at an outside connection!
            // Following code motivated by logic of HumanAI::SetCurrentVehicle.
            if (citizenData.m_path == 0 && citizenData.m_targetBuilding != 0 && Singleton<BuildingManager>.instance.m_buildings.m_buffer[citizenData.m_targetBuilding].Info.m_buildingAI is OutsideConnectionAI)
            {
                uint citizen = citizenData.m_citizen;
                Singleton<CitizenManager>.instance.m_citizens.m_buffer[citizen].SetVehicle(citizen, 0, 0);
            }

            #endregion

            return base.SetCurrentVehicle(instanceID, ref citizenData, vehicleID, unitID, position);
        }
    }
}
