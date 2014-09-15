﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kerbal_Construction_Time
{
    public static class KCT_GameStates
    {
        public static double UT;
        public static bool canWarp = false, warpInitiated = false;
        public static int lastWarpRate = 0;
        public static string lastSOIVessel = "";
        public static Dictionary<string, string> vesselDict = new Dictionary<string, string>();
        public static List<VesselType> VesselTypesForSOI = new List<VesselType>() { VesselType.Base, VesselType.Lander, VesselType.Probe, VesselType.Ship, VesselType.Station };
        public static List<Orbit.PatchTransitionType> SOITransitions = new List<Orbit.PatchTransitionType> { Orbit.PatchTransitionType.ENCOUNTER, Orbit.PatchTransitionType.ESCAPE };
        public static bool delayStart = false, delayMove = false;
        public static Dictionary<String, int> PartTracker = new Dictionary<string, int>();
        public static Dictionary<String, int> PartInventory = new Dictionary<string, int>();
        public static bool flightSimulated = false;
        public static String simulationReason;
        public static KCT_Settings settings = new KCT_Settings();
        public static KCT_TimeSettings timeSettings = new KCT_TimeSettings();
        public static List<KCT_BuildListVessel> VABList = new List<KCT_BuildListVessel>();
        public static List<KCT_BuildListVessel> VABWarehouse = new List<KCT_BuildListVessel>();
        public static List<KCT_BuildListVessel> SPHList = new List<KCT_BuildListVessel>();
        public static List<KCT_BuildListVessel> SPHWarehouse = new List<KCT_BuildListVessel>();
        public static List<KCT_TechItem> TechList = new List<KCT_TechItem>();
        public static List<int> VABUpgrades = new List<int>() {0};
        public static List<int> SPHUpgrades = new List<int>() {0};
        public static List<int> RDUpgrades = new List<int>() {0, 0};
        public static List<int> PurchasedUpgrades = new List<int>() { 0, 0 };
        public static int TotalUpgradePoints = 0;
        public static KCT_BuildListVessel launchedVessel, editedVessel;
        //public static Dictionary<uint, List<ProtoCrewMember>> launchedCrew = new Dictionary<uint, List<ProtoCrewMember>>();
        public static List<CrewedPart> launchedCrew = new List<CrewedPart>();
        public static IButton kctToolbarButton;
        public static bool EditorShipEditingMode = false, buildSimulatedVessel = false;
        public static bool firstStart = true;
        public static IKCTBuildItem targetedItem = null;
        public static double EditorBuildTime = 0;
        public static Dictionary<string, int> EditedVesselParts = new Dictionary<string, int>();
        public static bool LaunchFromTS = false;
        public static KCT_Reconditioning LaunchPadReconditioning;

        public static List<bool> showWindows = new List<bool> { false, true }; //build list, editor
        
        
        //Things pertaining to simulations
        public static CelestialBody simulationBody;
        public static bool simulateInOrbit = false;
        public static double simulationUT = 0;
        public static double simulationEndTime = 0, simulationTimeLimit = 0, simulationDefaultTimeLimit = 0;
        public static double simOrbitAltitude = 0, simInclination = 0;
        public static double simLatitude = 0, simLongitude = 0, simLandAlt = 50;
        public static List<String> BodiesVisited = new List<string> {"Kerbin"};
        public static float SimulationCost = 0, FundsToChargeAtSimEnd = 0, FundsGivenForVessel = 0;

        public static void reset()
        {
            //firstStart = true;
            PartTracker = new Dictionary<string, int>();
            PartInventory = new Dictionary<string, int>();
            flightSimulated = false;
            vesselDict = new Dictionary<string, string>();
            simulationBody = KCT_Utilities.GetBodyByName("Kerbin");
            simulateInOrbit = false;
            BodiesVisited = new List<string> {"Kerbin"};
            TotalUpgradePoints = 0;
            VABUpgrades = new List<int>() {0};
            SPHUpgrades = new List<int>() {0};
            RDUpgrades = new List<int>() {0, 0};
            PurchasedUpgrades = new List<int>() { 0, 0 };
            LaunchPadReconditioning = null;
            targetedItem = null;

            VABList = new List<KCT_BuildListVessel>();
            VABWarehouse = new List<KCT_BuildListVessel>();
            SPHList = new List<KCT_BuildListVessel>();
            SPHWarehouse = new List<KCT_BuildListVessel>();
            TechList = new List<KCT_TechItem>();
        }

    }

    public class CrewedPart
    {
        public List<ProtoCrewMember> crewList;
        public uint partID;

        public CrewedPart(uint ID, List<ProtoCrewMember> crew)
        {
            partID = ID;
            crewList = crew;
        }

        public CrewedPart FromPart(Part part, List<ProtoCrewMember> crew)
        {
            partID = part.uid;
            crewList = crew;
            return this;
        }
    }

}
/*
Copyright (C) 2014  Michael Marvin, Zachary Eck

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/