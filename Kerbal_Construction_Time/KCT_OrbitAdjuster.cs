﻿// This file is copied in part from the HyperEdit plugin for Kerbal Space Program, Copyright Erickson Swift, 2013.
// HyperEdit is licensed under the GPL.
// Currently supported by Team HyperEdit, and Ezriilc.
// Original HyperEdit concept and code by khyperia (no longer involved).
//
// Some modifications were made by Michael Marvin for use with the Kerbal Construction Time plugin for Kerbal Space Program
// 2014


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Kerbal_Construction_Time
{
    class KCT_OrbitAdjuster
    {
        public static void PutInOrbitAround(CelestialBody body, double altitude)
        {
            PutInOrbitAround(body, altitude, 0);
        }
        public static void PutInOrbitAround(CelestialBody body, double altitude, double inclination)
        {
            OrbitDriver _orbit = FlightGlobals.ActiveVessel.orbitDriver;
            Set(_orbit.orbit, CreateOrbit(inclination, 0, altitude + body.Radius, 0, 0, 0, 0, body));
        }


        public static void Set(Orbit orbit, Orbit newOrbit)
        {
            var vessel = FlightGlobals.fetch == null ? null : FlightGlobals.Vessels.FirstOrDefault(v => v.orbitDriver != null && v.orbit == orbit);
            var body = FlightGlobals.fetch == null ? null : FlightGlobals.Bodies.FirstOrDefault(v => v.orbitDriver != null && v.orbit == orbit);
            if (vessel != null)
                WarpShip(vessel, newOrbit);
            else if (body != null)
                WarpPlanet(body, newOrbit);
            else
                HardsetOrbit(orbit, newOrbit);
        }

        private static void WarpShip(Vessel vessel, Orbit newOrbit)
        {
            if (newOrbit.getRelativePositionAtUT(Planetarium.GetUniversalTime()).magnitude > newOrbit.referenceBody.sphereOfInfluence)
            {
                return;
            }

            vessel.Landed = false;
            vessel.Splashed = false;
            vessel.landedAt = string.Empty;
            var parts = vessel.parts;
            if (parts != null)
            {
                var clamps = parts.Where(p => p.Modules != null && p.Modules.OfType<LaunchClamp>().Any()).ToList();
                foreach (var clamp in clamps)
                    clamp.Die();
            }

            try
            {
                OrbitPhysicsManager.HoldVesselUnpack(60);
            }
            catch (NullReferenceException)
            {
            }

            foreach (var v in (FlightGlobals.fetch == null ? (IEnumerable<Vessel>)new[] { vessel } : FlightGlobals.Vessels).Where(v => v.packed == false))
                v.GoOnRails();

            HardsetOrbit(vessel.orbit, newOrbit);

            vessel.orbitDriver.pos = vessel.orbit.pos.xzy;
            vessel.orbitDriver.vel = vessel.orbit.vel;
        }

        private static void WarpPlanet(CelestialBody body, Orbit newOrbit)
        {
            var oldBody = body.referenceBody;
            HardsetOrbit(body.orbit, newOrbit);
            if (oldBody != newOrbit.referenceBody)
            {
                oldBody.orbitingBodies.Remove(body);
                newOrbit.referenceBody.orbitingBodies.Add(body);
            }
            body.CBUpdate();
        }

        private static void HardsetOrbit(Orbit orbit, Orbit newOrbit)
        {
            orbit.inclination = newOrbit.inclination;
            orbit.eccentricity = newOrbit.eccentricity;
            orbit.semiMajorAxis = newOrbit.semiMajorAxis;
            orbit.LAN = newOrbit.LAN;
            orbit.argumentOfPeriapsis = newOrbit.argumentOfPeriapsis;
            orbit.meanAnomalyAtEpoch = newOrbit.meanAnomalyAtEpoch;
            orbit.epoch = newOrbit.epoch;
            orbit.referenceBody = newOrbit.referenceBody;
            orbit.Init();
            orbit.UpdateFromUT(Planetarium.GetUniversalTime());
        }

        private static Orbit CreateOrbit(double inc, double e, double sma, double lan, double w, double mEp, double epoch, CelestialBody body)
        {
            if (double.IsNaN(inc))
                inc = 0;
            if (double.IsNaN(e))
                e = 0;
            if (double.IsNaN(sma))
                sma = body.Radius + body.maxAtmosphereAltitude + 10000;
            if (double.IsNaN(lan))
                lan = 0;
            if (double.IsNaN(w))
                w = 0;
            if (double.IsNaN(mEp))
                mEp = 0;
            if (double.IsNaN(epoch))
                mEp = Planetarium.GetUniversalTime();

            if (Math.Sign(e - 1) == Math.Sign(sma))
                sma = -sma;

            if (Math.Sign(sma) >= 0)
            {
                while (mEp < 0)
                    mEp += Math.PI * 2;
                while (mEp > Math.PI * 2)
                    mEp -= Math.PI * 2;
            }

            return new Orbit(inc, e, sma, lan, w, mEp, epoch, body);
        }


        //Lander stuff, based on HyperEdit's lander code

        public static void LandAtTarget(double latitude, double longitude, double altitude)
        {
            KCTDebug.Log("Landing a vessel at " + latitude + ", " + longitude + ", alt " + altitude);
            if (FlightGlobals.fetch == null || FlightGlobals.ActiveVessel == null)
                return;
            var lander = FlightGlobals.ActiveVessel.GetComponent<LanderAttachment>();
            if (lander == null)
            {
                lander = FlightGlobals.ActiveVessel.gameObject.AddComponent<LanderAttachment>();
                lander.Latitude = latitude;
                lander.Longitude = longitude;
                lander.Altitude = altitude;
            }
            else
                UnityEngine.Object.Destroy(lander);
        }
    }

    //This code all is nearly identical to that of HyperEdit.
    public class LanderAttachment : MonoBehaviour
    {
        private bool _alreadyTeleported;
        public double Latitude;
        public double Longitude;
        public double Altitude;

        public void FixedUpdate()
        {
            var vessel = GetComponent<Vessel>();
            if (vessel != FlightGlobals.ActiveVessel)
            {
                Destroy(this);
                return;
            }
            if (_alreadyTeleported)
            {
              /*  if (Math.Max(0, vessel.terrainAltitude) > Altitude)
                {
                    var diff = vessel.mainBody.GetWorldSurfacePosition(Latitude, Longitude, Math.Max(0, vessel.terrainAltitude) + Altitude) - vessel.GetWorldPos3D();
                    ((Krakensbane)FindObjectOfType(typeof(Krakensbane))).setOffset(diff);
                }*/
                if (vessel.LandedOrSplashed)
                {
                    Destroy(this);
                }
                else
                {
                    var accel = (vessel.srf_velocity + vessel.upAxis) * -0.5;
                    vessel.ChangeWorldVelocity(accel);
                }
            }
            else
            {
                var alt = vessel.mainBody.pqsController.GetSurfaceHeight(
                    QuaternionD.AngleAxis(Longitude, Vector3d.down) *
                    QuaternionD.AngleAxis(Latitude, Vector3d.forward) * Vector3d.right) -
                          vessel.mainBody.pqsController.radius;
                alt = Math.Max(alt, 0); // Underwater!
                var diff = vessel.mainBody.GetWorldSurfacePosition(Latitude, Longitude, alt + Altitude) - vessel.GetWorldPos3D();
                if (vessel.Landed)
                    vessel.Landed = false;
                else if (vessel.Splashed)
                    vessel.Splashed = false;
                foreach (var part in vessel.parts.Where(part => part.Modules.OfType<LaunchClamp>().Any()).ToList())
                    part.Die();
                
                ((Krakensbane)FindObjectOfType(typeof(Krakensbane))).setOffset(diff);
                vessel.ChangeWorldVelocity(-vessel.obt_velocity);
                _alreadyTeleported = true;
            }
        }
    }
}