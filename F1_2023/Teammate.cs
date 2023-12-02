using System.Collections.Generic;
using SimHub.Plugins;

namespace PluginDeMo_v2.F1_2023
{
    public class Teammate
    {
        public Player PlayerOfTeammate { get; set; } // this class is the player's teammate, but this attribute is the player
        public Participant Participant => PlayerOfTeammate.TeammateParticipant;
        public List<Property<object>> Properties { get; set; } = new List<Property<object>>();

        public Teammate(PluginManager pluginManager, Player player)
        {
            PlayerOfTeammate = player;
            AddProperties(pluginManager);
        }

        public void UpdateProperties()
        {
            foreach (Property<object> property in Properties)
            {
                property.Update();
            }
        }

        public void AddProperties(PluginManager pluginManager)
        {
            string namePrefix = Utility.ParticipantPrefix(isTeammate: true);

            //// car setup ////
            string setupPrefix = $"{namePrefix}Setup_";
            Properties.Add(
                new Property<object>( // FrontWing
                    pluginManager: pluginManager,
                    prefix: setupPrefix,
                    suffix: "FrontWing",
                    pluginType: typeof(byte),
                    valueFunc: () => Participant.CarSetupData.m_frontWing,
                    updateRate: 1000
                )
            );
            Properties.Add(
                new Property<object>( // RearWing
                    pluginManager: pluginManager,
                    prefix: setupPrefix,
                    suffix: "RearWing",
                    pluginType: typeof(byte),
                    valueFunc: () => Participant.CarSetupData.m_rearWing,
                    updateRate: 1000
                )
            );
            Properties.Add(
                new Property<object>( // OnThrottle
                    pluginManager: pluginManager,
                    prefix: setupPrefix,
                    suffix: "OnThrottle",
                    pluginType: typeof(byte),
                    valueFunc: () => Participant.CarSetupData.m_onThrottle,
                    updateRate: 1000
                )
            );
            Properties.Add(
                new Property<object>( // OffThrottle
                    pluginManager: pluginManager,
                    prefix: setupPrefix,
                    suffix: "OffThrottle",
                    pluginType: typeof(byte),
                    valueFunc: () => Participant.CarSetupData.m_offThrottle,
                    updateRate: 1000
                )
            );
            Properties.Add(
                new Property<object>( // FrontCamber
                    pluginManager: pluginManager,
                    prefix: setupPrefix,
                    suffix: "FrontCamber",
                    pluginType: typeof(float),
                    valueFunc: () => Participant.CarSetupData.m_frontCamber,
                    updateRate: 1000
                )
            );
            Properties.Add(
                new Property<object>( // RearCamber
                    pluginManager: pluginManager,
                    prefix: setupPrefix,
                    suffix: "RearCamber",
                    pluginType: typeof(float),
                    valueFunc: () => Participant.CarSetupData.m_rearCamber,
                    updateRate: 1000
                )
            );
            Properties.Add(
                new Property<object>( // FrontToe
                    pluginManager: pluginManager,
                    prefix: setupPrefix,
                    suffix: "FrontToe",
                    pluginType: typeof(float),
                    valueFunc: () => Participant.CarSetupData.m_frontToe,
                    updateRate: 1000
                )
            );
            Properties.Add(
                new Property<object>( // RearToe
                    pluginManager: pluginManager,
                    prefix: setupPrefix,
                    suffix: "RearToe",
                    pluginType: typeof(float),
                    valueFunc: () => Participant.CarSetupData.m_rearToe,
                    updateRate: 1000
                )
            );
            Properties.Add(
                new Property<object>( // FrontSuspension
                    pluginManager: pluginManager,
                    prefix: setupPrefix,
                    suffix: "FrontSuspension",
                    pluginType: typeof(byte),
                    valueFunc: () => Participant.CarSetupData.m_frontSuspension,
                    updateRate: 1000
                )
            );
            Properties.Add(
                new Property<object>( // RearSuspension
                    pluginManager: pluginManager,
                    prefix: setupPrefix,
                    suffix: "RearSuspension",
                    pluginType: typeof(byte),
                    valueFunc: () => Participant.CarSetupData.m_rearSuspension,
                    updateRate: 1000
                )
            );
            Properties.Add(
                new Property<object>( // FrontAntiRollBar
                    pluginManager: pluginManager,
                    prefix: setupPrefix,
                    suffix: "FrontAntiRollBar",
                    pluginType: typeof(byte),
                    valueFunc: () => Participant.CarSetupData.m_frontAntiRollBar,
                    updateRate: 1000
                )
            );
            Properties.Add(
                new Property<object>( // RearAntiRollBar
                    pluginManager: pluginManager,
                    prefix: setupPrefix,
                    suffix: "RearAntiRollBar",
                    pluginType: typeof(byte),
                    valueFunc: () => Participant.CarSetupData.m_rearAntiRollBar,
                    updateRate: 1000
                )
            );
            Properties.Add(
                new Property<object>( // FrontSuspensionHeight
                    pluginManager: pluginManager,
                    prefix: setupPrefix,
                    suffix: "FrontSuspensionHeight",
                    pluginType: typeof(byte),
                    valueFunc: () => Participant.CarSetupData.m_frontSuspensionHeight,
                    updateRate: 1000
                )
            );
            Properties.Add(
                new Property<object>( // RearSuspensionHeight
                    pluginManager: pluginManager,
                    prefix: setupPrefix,
                    suffix: "RearSuspensionHeight",
                    pluginType: typeof(byte),
                    valueFunc: () => Participant.CarSetupData.m_rearSuspensionHeight,
                    updateRate: 1000
                )
            );
            Properties.Add(
                new Property<object>( // BrakePressure
                    pluginManager: pluginManager,
                    prefix: setupPrefix,
                    suffix: "BrakePressure",
                    pluginType: typeof(byte),
                    valueFunc: () => Participant.CarSetupData.m_brakePressure,
                    updateRate: 1000
                )
            );
            Properties.Add(
                new Property<object>( // BrakeBias
                    pluginManager: pluginManager,
                    prefix: setupPrefix,
                    suffix: "BrakeBias",
                    pluginType: typeof(byte),
                    valueFunc: () => Participant.CarSetupData.m_brakeBias,
                    updateRate: 1000
                )
            );
            Properties.Add(
                new Property<object>( // RearLeftTyrePressure
                    pluginManager: pluginManager,
                    prefix: setupPrefix,
                    suffix: "RearLeftTyrePressure",
                    pluginType: typeof(float),
                    valueFunc: () => Participant.CarSetupData.m_rearLeftTyrePressure,
                    updateRate: 1000
                )
            );
            Properties.Add(
                new Property<object>( // RearRightTyrePressure
                    pluginManager: pluginManager,
                    prefix: setupPrefix,
                    suffix: "RearRightTyrePressure",
                    pluginType: typeof(float),
                    valueFunc: () => Participant.CarSetupData.m_rearRightTyrePressure,
                    updateRate: 1000
                )
            );
            Properties.Add(
                new Property<object>( // FrontLeftTyrePressure
                    pluginManager: pluginManager,
                    prefix: setupPrefix,
                    suffix: "FrontLeftTyrePressure",
                    pluginType: typeof(float),
                    valueFunc: () => Participant.CarSetupData.m_frontLeftTyrePressure,
                    updateRate: 1000
                )
            );
            Properties.Add(
                new Property<object>( // FrontRightTyrePressure
                    pluginManager: pluginManager,
                    prefix: setupPrefix,
                    suffix: "FrontRightTyrePressure",
                    pluginType: typeof(float),
                    valueFunc: () => Participant.CarSetupData.m_frontRightTyrePressure,
                    updateRate: 1000
                )
            );
            Properties.Add(
                new Property<object>( // Ballast
                    pluginManager: pluginManager,
                    prefix: setupPrefix,
                    suffix: "Ballast",
                    pluginType: typeof(byte),
                    valueFunc: () => Participant.CarSetupData.m_ballast,
                    updateRate: 1000
                )
            );
            Properties.Add(
                new Property<object>( // FuelLoad
                    pluginManager: pluginManager,
                    prefix: setupPrefix,
                    suffix: "FuelLoad",
                    pluginType: typeof(float),
                    valueFunc: () => Participant.CarSetupData.m_fuelLoad,
                    updateRate: 1000
                )
            );
        }
    }
}
