﻿using CodeWalker.World;
using SharpDX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace CodeWalker.GameFiles
{
    public class VehiclesFile : GameFile, PackedFile
    {


        public string ResidentTxd { get; set; }
        public List<VehicleInitData> InitDatas { get; set; }
        public Dictionary<string, string> TxdRelationships { get; set; }




        public VehiclesFile() : base(null, GameFileType.Vehicles)
        {
        }
        public VehiclesFile(RpfFileEntry entry) : base(entry, GameFileType.Vehicles)
        {
        }



        public void LoadOld(byte[] data, RpfFileEntry entry)
        {
            RpfFileEntry = entry;
            Name = entry.Name;
            FilePath = Name;


            if (entry.IsExtension(".meta"))
            {
                string xml = TextUtil.GetUTF8Text(data);

                XmlDocument xmldoc = new XmlDocument();
                xmldoc.LoadXml(xml);


                ResidentTxd = Xml.GetChildInnerText(xmldoc.SelectSingleNode("CVehicleModelInfo__InitDataList"), "residentTxd");

                LoadInitDatas(xmldoc);

                LoadTxdRelationships(xmldoc);

                Loaded = true;
            }
        }

        public void Load(byte[] data, RpfFileEntry entry)
        {
            RpfFileEntry = entry;
            Name = entry.Name;
            FilePath = Name;


            if (entry.IsExtension(".meta"))
            {
                using var textReader = new StreamReader(new MemoryStream(data), Encoding.UTF8);

                using var xmlReader = XmlReader.Create(textReader);

                while (xmlReader.Read())
                {
                    xmlReader.MoveToContent();

                    //var _ = xmlReader.Name switch
                    //{
                    //    "residentTxd" => ResidentTxd = Xml.GetChildInnerText(xmlReader, "residentTxd"),
                    //    "InitDatas" => LoadInitDatas(xmlReader),
                    //    "txdRelationships" => LoadTxdRelationships(xmlReader),
                    //    _ => throw new Exception()
                    //};

                    switch (xmlReader.Name)
                    {
                        case string Name when Name.Equals("residentTxd", StringComparison.OrdinalIgnoreCase):
                            ResidentTxd = Xml.GetChildInnerText(xmlReader, "residentTxd");
                            break;
                        case string Name when Name.Equals("InitDatas", StringComparison.OrdinalIgnoreCase):
                            LoadInitDatas(xmlReader);
                            break;
                        case string Name when Name.Equals("txdRelationships", StringComparison.OrdinalIgnoreCase):
                            LoadTxdRelationships(xmlReader);
                            break;
                        default:
                            break;
                    }
                }


                //ResidentTxd = Xml.GetChildInnerText(xmldoc.SelectSingleNode("CVehicleModelInfo__InitDataList"), "residentTxd");

                //LoadInitDatas(xmldoc);

                //LoadTxdRelationships(xmldoc);

                Loaded = true;
            }
        }


        private void LoadInitDatas(XmlDocument xmldoc)
        {
            XmlNodeList items = xmldoc.SelectNodes("CVehicleModelInfo__InitDataList/InitDatas/Item | CVehicleModelInfo__InitDataList/InitDatas/item");

            InitDatas = new List<VehicleInitData>(items.Count);
            for (int i = 0; i < items.Count; i++)
            {
                var node = items[i];
                VehicleInitData d = new VehicleInitData();
                d.Load(node);
                InitDatas.Add(d);
            }
        }

        private void LoadInitDatas(XmlReader reader)
        {
            if (!reader.IsStartElement() || reader.Name != "InitDatas")
            {
                throw new InvalidOperationException("XmlReader is not at start element of \"InitDatas\"");
            }

            InitDatas = new List<VehicleInitData>();

            reader.ReadStartElement("InitDatas");

            while (reader.MoveToContent() == XmlNodeType.Element && reader.Name == "Item")
            {
                if (reader.IsStartElement())
                {
                    VehicleInitData d = new VehicleInitData();
                    d.Load(reader);
                    InitDatas.Add(d);
                }
            }
        }

        private void LoadTxdRelationships(XmlReader reader)
        {
            if (reader.IsEmptyElement)
            {
                TxdRelationships = new Dictionary<string, string>();
                reader.ReadStartElement();
                return;
            }
            TxdRelationships = new Dictionary<string, string>();

            foreach(var item in Xml.IterateItems(reader, "txdRelationships"))
            {
                var childstr = item.Element("child")?.Value;
                var parentstr = item.Element("parent")?.Value;

                if ((!string.IsNullOrEmpty(parentstr)) && (!string.IsNullOrEmpty(childstr)))
                {
                    if (!TxdRelationships.ContainsKey(childstr))
                    {
                        TxdRelationships.Add(childstr, parentstr);
                    }
                }
            }
        }

        private void LoadTxdRelationships(XmlDocument xmldoc)
        {
            XmlNodeList items = xmldoc.SelectNodes("CVehicleModelInfo__InitDataList/txdRelationships/Item | CVehicleModelInfo__InitDataList/txdRelationships/item");

            TxdRelationships = new Dictionary<string, string>();
            for (int i = 0; i < items.Count; i++)
            {
                string parentstr = Xml.GetChildInnerText(items[i], "parent");
                string childstr = Xml.GetChildInnerText(items[i], "child");

                if ((!string.IsNullOrEmpty(parentstr)) && (!string.IsNullOrEmpty(childstr)))
                {
                    if (!TxdRelationships.ContainsKey(childstr))
                    {
                        TxdRelationships.Add(childstr, parentstr);
                    }
                    else
                    { }
                }
            }
        }
    }


    public class VehicleInitData
    {
        
        public string modelName { get; set; }                   //<modelName>impaler3</modelName>
        public string txdName { get; set; }                     //<txdName>impaler3</txdName>
        public string handlingId { get; set; }                  //<handlingId>IMPALER3</handlingId>
        public string gameName { get; set; }                    //<gameName>IMPALER3</gameName>
        public string vehicleMakeName { get; set; }             //<vehicleMakeName>DECLASSE</vehicleMakeName>
        public string expressionDictName { get; set; }          //<expressionDictName>null</expressionDictName>
        public string expressionName { get; set; }              //<expressionName>null</expressionName>
        public string animConvRoofDictName { get; set; }        //<animConvRoofDictName>null</animConvRoofDictName>
        public string animConvRoofName { get; set; }            //<animConvRoofName>null</animConvRoofName>
        public string[] animConvRoofWindowsAffected { get; set; } //<animConvRoofWindowsAffected />
        public string ptfxAssetName { get; set; }               //<ptfxAssetName>weap_xs_vehicle_weapons</ptfxAssetName>
        public string audioNameHash { get; set; }               //<audioNameHash />
        public string layout { get; set; }                      //<layout>LAYOUT_STD_ARENA_1HONLY</layout>
        public string coverBoundOffsets { get; set; }           //<coverBoundOffsets>IMPALER_COVER_OFFSET_INFO</coverBoundOffsets>
        public string explosionInfo { get; set; }               //<explosionInfo>EXPLOSION_INFO_DEFAULT</explosionInfo>
        public string scenarioLayout { get; set; }              //<scenarioLayout />
        public string cameraName { get; set; }                  //<cameraName>FOLLOW_CHEETAH_CAMERA</cameraName>
        public string aimCameraName { get; set; }               //<aimCameraName>DEFAULT_THIRD_PERSON_VEHICLE_AIM_CAMERA</aimCameraName>
        public string bonnetCameraName { get; set; }            //<bonnetCameraName>VEHICLE_BONNET_CAMERA_STANDARD_LONG_DEVIANT</bonnetCameraName>
        public string povCameraName { get; set; }               //<povCameraName>REDUCED_NEAR_CLIP_POV_CAMERA</povCameraName>
        public Vector3 FirstPersonDriveByIKOffset { get; set; }                     //<FirstPersonDriveByIKOffset x="0.020000" y="-0.065000" z="-0.050000" />
        public Vector3 FirstPersonDriveByUnarmedIKOffset { get; set; }              //<FirstPersonDriveByUnarmedIKOffset x="0.000000" y="-0.100000" z="0.000000" />
        public Vector3 FirstPersonProjectileDriveByIKOffset { get; set; }           //<FirstPersonProjectileDriveByIKOffset x="0.000000" y="-0.130000" z="-0.050000" />
        public Vector3 FirstPersonProjectileDriveByPassengerIKOffset { get; set; }  //<FirstPersonProjectileDriveByPassengerIKOffset x="0.000000" y="-0.100000" z="0.000000" />
        public Vector3 FirstPersonDriveByRightPassengerIKOffset { get; set; }       //<FirstPersonDriveByRightPassengerIKOffset x="-0.020000" y="-0.065000" z="-0.050000" />
        public Vector3 FirstPersonDriveByRightPassengerUnarmedIKOffset { get; set; }//<FirstPersonDriveByRightPassengerUnarmedIKOffset x="0.000000" y="-0.100000" z="0.000000" />
        public Vector3 FirstPersonMobilePhoneOffset { get; set; }                   //<FirstPersonMobilePhoneOffset x="0.146000" y="0.220000" z="0.510000" />
        public Vector3 FirstPersonPassengerMobilePhoneOffset { get; set; }          //<FirstPersonPassengerMobilePhoneOffset x="0.234000" y="0.169000" z="0.395000" />
        public Vector3 PovCameraOffset { get; set; }                                //<PovCameraOffset x="0.000000" y="-0.195000" z="0.640000" />
        public Vector3 PovCameraVerticalAdjustmentForRollCage { get; set; }         //<PovCameraVerticalAdjustmentForRollCage value="0.000000" />
        public Vector3 PovPassengerCameraOffset { get; set; }                       //<PovPassengerCameraOffset x="0.000000" y="0.000000" z="0.000000" />
        public Vector3 PovRearPassengerCameraOffset { get; set; }                   //<PovRearPassengerCameraOffset x="0.000000" y="0.000000" z="0.000000" />
        public string vfxInfoName { get; set; }                         //<vfxInfoName>VFXVEHICLEINFO_CAR_GENERIC</vfxInfoName>
        public bool shouldUseCinematicViewMode { get; set; }            //<shouldUseCinematicViewMode value="true" />
        public bool shouldCameraTransitionOnClimbUpDown { get; set; }   //<shouldCameraTransitionOnClimbUpDown value="false" />
        public bool shouldCameraIgnoreExiting { get; set; }             //<shouldCameraIgnoreExiting value="false" />
        public bool AllowPretendOccupants { get; set; }                 //<AllowPretendOccupants value="true" />
        public bool AllowJoyriding { get; set; }                        //<AllowJoyriding value="true" />
        public bool AllowSundayDriving { get; set; }                    //<AllowSundayDriving value="true" />
        public bool AllowBodyColorMapping { get; set; }                 //<AllowBodyColorMapping value="true" />
        public float wheelScale { get; set; }                           //<wheelScale value="0.202300" />
        public float wheelScaleRear { get; set; }                       //<wheelScaleRear value="0.0.201800" />
        public float dirtLevelMin { get; set; }                         //<dirtLevelMin value="0.000000" />
        public float dirtLevelMax { get; set; }                         //<dirtLevelMax value="0.450000" />
        public float envEffScaleMin { get; set; }                       //<envEffScaleMin value="0.000000" />
        public float envEffScaleMax { get; set; }                       //<envEffScaleMax value="1.000000" />
        public float envEffScaleMin2 { get; set; }                      //<envEffScaleMin2 value="0.000000" />
        public float envEffScaleMax2 { get; set; }                      //<envEffScaleMax2 value="1.000000" />
        public float damageMapScale { get; set; }                       //<damageMapScale value="0.000000" />
        public float damageOffsetScale { get; set; }                    //<damageOffsetScale value="0.100000" />
        public Color4 diffuseTint { get; set; }                         //<diffuseTint value="0x00FFFFFF" />
        public float steerWheelMult { get; set; }                       //<steerWheelMult value="0.700000" />
        public float HDTextureDist { get; set; }                        //<HDTextureDist value="5.000000" />
        public float[] lodDistances { get; set; }                       //<lodDistances content="float_array">//  10.000000//  25.000000//  60.000000//  120.000000//  500.000000//  500.000000//</lodDistances>
        public float minSeatHeight { get; set; }                        //<minSeatHeight value="0.844" />
        public float identicalModelSpawnDistance { get; set; }          //<identicalModelSpawnDistance value="20" />
        public int maxNumOfSameColor { get; set; }                      //<maxNumOfSameColor value="1" />
        public float defaultBodyHealth { get; set; }                    //<defaultBodyHealth value="1000.000000" />
        public float pretendOccupantsScale { get; set; }                //<pretendOccupantsScale value="1.000000" />
        public float visibleSpawnDistScale { get; set; }                //<visibleSpawnDistScale value="1.000000" />
        public float trackerPathWidth { get; set; }                     //<trackerPathWidth value="2.000000" />
        public float weaponForceMult { get; set; }                      //<weaponForceMult value="1.000000" />
        public float frequency { get; set; }                            //<frequency value="30" />
        public string swankness { get; set; }                           //<swankness>SWANKNESS_4</swankness>
        public int maxNum { get; set; }                                 //<maxNum value="10" />
        public string[] flags { get; set; }                             //<flags>FLAG_RECESSED_HEADLIGHT_CORONAS FLAG_EXTRAS_STRONG FLAG_AVERAGE_CAR FLAG_HAS_INTERIOR_EXTRAS FLAG_CAN_HAVE_NEONS FLAG_HAS_JUMP_MOD FLAG_HAS_NITROUS_MOD FLAG_HAS_RAMMING_SCOOP_MOD FLAG_USE_AIRCRAFT_STYLE_WEAPON_TARGETING FLAG_HAS_SIDE_SHUNT FLAG_HAS_WEAPON_SPIKE_MODS FLAG_HAS_SUPERCHARGER FLAG_INCREASE_CAMBER_WITH_SUSPENSION_MOD FLAG_DISABLE_DEFORMATION</flags>
        public string type { get; set; }                                //<type>VEHICLE_TYPE_CAR</type>
        public string plateType { get; set; }                           //<plateType>VPT_FRONT_AND_BACK_PLATES</plateType>
        public string dashboardType { get; set; }                       //<dashboardType>VDT_DUKES</dashboardType>
        public string vehicleClass { get; set; }                        //<vehicleClass>VC_MUSCLE</vehicleClass>
        public string wheelType { get; set; }                           //<wheelType>VWT_MUSCLE</wheelType>
        public string[] trailers { get; set; }                          //<trailers />
        public string[] additionalTrailers { get; set; }                //<additionalTrailers />
        public VehicleDriver[] drivers { get; set; }                    //<drivers />
        public string[] extraIncludes { get; set; }                     //<extraIncludes />
        public string[] doorsWithCollisionWhenClosed { get; set; }      //<doorsWithCollisionWhenClosed />
        public string[] driveableDoors { get; set; }                    //<driveableDoors />
        public bool bumpersNeedToCollideWithMap { get; set; }           //<bumpersNeedToCollideWithMap value="false" />
        public bool needsRopeTexture { get; set; }                      //<needsRopeTexture value="false" />
        public string[] requiredExtras { get; set; }                    //<requiredExtras>EXTRA_1 EXTRA_2 EXTRA_3</requiredExtras>
        public string[] rewards { get; set; }                           //<rewards />
        public string[] cinematicPartCamera { get; set; }               //<cinematicPartCamera>//  <Item>WHEEL_FRONT_RIGHT_CAMERA</Item>//  <Item>WHEEL_FRONT_LEFT_CAMERA</Item>//  <Item>WHEEL_REAR_RIGHT_CAMERA</Item>//  <Item>WHEEL_REAR_LEFT_CAMERA</Item>//</cinematicPartCamera>
        public string NmBraceOverrideSet { get; set; }                  //<NmBraceOverrideSet />
        public Vector3 buoyancySphereOffset { get; set; }               //<buoyancySphereOffset x="0.000000" y="0.000000" z="0.000000" />
        public float buoyancySphereSizeScale { get; set; }              //<buoyancySphereSizeScale value="1.000000" />
        public VehicleOverrideRagdollThreshold pOverrideRagdollThreshold { get; set; }  //<pOverrideRagdollThreshold type="NULL" />
        public string[] firstPersonDrivebyData { get; set; }            //<firstPersonDrivebyData>//  <Item>STD_IMPALER2_FRONT_LEFT</Item>//  <Item>STD_IMPALER2_FRONT_RIGHT</Item>//</firstPersonDrivebyData>

        public void Load(XmlReader reader)
        {
            reader.ReadStartElement("Item");
            while (reader.Name != "Item" && reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "Item")
                {
                    reader.ReadEndElement();
                    return;
                }
                if (reader.IsStartElement())
                {
                    while (reader.IsStartElement())
                    {
                        switch (reader.Name)
                        {
                            case "modelName":
                                modelName = Xml.GetChildInnerText(reader, "modelName");
                                break;
                            case "txdName":
                                txdName = Xml.GetChildInnerText(reader, "txdName");
                                break;
                            case "handlingId":
                                handlingId = Xml.GetChildInnerText(reader, "handlingId");
                                break;
                            case "gameName":
                                gameName = Xml.GetChildInnerText(reader, "gameName");
                                break;
                            case "vehicleMakeName":
                                vehicleMakeName = Xml.GetChildInnerText(reader, "vehicleMakeName");
                                break;
                            case "expressionDictName":
                                expressionDictName = Xml.GetChildInnerText(reader, "expressionDictName");
                                break;
                            case "expressionName":
                                expressionName = Xml.GetChildInnerText(reader, "expressionName");
                                break;
                            case "animConvRoofDictName":
                                animConvRoofDictName = Xml.GetChildInnerText(reader, "animConvRoofDictName");
                                break;
                            case "animConvRoofName":
                                animConvRoofName = Xml.GetChildInnerText(reader, "animConvRoofName");
                                break;
                            case "animConvRoofWindowsAffected":
                                animConvRoofWindowsAffected = GetStringItemArray(reader, "animConvRoofWindowsAffected");
                                break;
                            case "ptfxAssetName":
                                ptfxAssetName = Xml.GetChildInnerText(reader, "ptfxAssetName");
                                break;
                            case "audioNameHash":
                                audioNameHash = Xml.GetChildInnerText(reader, "audioNameHash");
                                break;
                            case "layout":
                                layout = Xml.GetChildInnerText(reader, "layout");
                                break;
                            case "coverBoundOffsets":
                                coverBoundOffsets = Xml.GetChildInnerText(reader, "coverBoundOffsets");
                                break;
                            case "explosionInfo":
                                explosionInfo = Xml.GetChildInnerText(reader, "explosionInfo");
                                break;
                            case "scenarioLayout":
                                scenarioLayout = Xml.GetChildInnerText(reader, "scenarioLayout");
                                break;
                            case "cameraName":
                                cameraName = Xml.GetChildInnerText(reader, "cameraName");
                                break;
                            case "aimCameraName":
                                aimCameraName = Xml.GetChildInnerText(reader, "aimCameraName");
                                break;
                            case "bonnetCameraName":
                                bonnetCameraName = Xml.GetChildInnerText(reader, "bonnetCameraName");
                                break;
                            case "povCameraName":
                                povCameraName = Xml.GetChildInnerText(reader, "povCameraName");
                                break;
                            case "FirstPersonDriveByIKOffset":
                                FirstPersonDriveByIKOffset = Xml.GetChildVector3Attributes(reader, "FirstPersonDriveByIKOffset");
                                break;
                            case "FirstPersonDriveByUnarmedIKOffset":
                                FirstPersonDriveByUnarmedIKOffset = Xml.GetChildVector3Attributes(reader, "FirstPersonDriveByUnarmedIKOffset");
                                break;
                            case "FirstPersonProjectileDriveByIKOffset":
                                FirstPersonProjectileDriveByIKOffset = Xml.GetChildVector3Attributes(reader, "FirstPersonProjectileDriveByIKOffset");
                                break;
                            case "FirstPersonProjectileDriveByPassengerIKOffset":
                                FirstPersonProjectileDriveByPassengerIKOffset = Xml.GetChildVector3Attributes(reader, "FirstPersonProjectileDriveByPassengerIKOffset");
                                break;
                            case "FirstPersonDriveByRightPassengerIKOffset":
                                FirstPersonDriveByRightPassengerIKOffset = Xml.GetChildVector3Attributes(reader, "FirstPersonDriveByRightPassengerIKOffset");
                                break;
                            case "FirstPersonDriveByRightPassengerUnarmedIKOffset":
                                FirstPersonDriveByRightPassengerUnarmedIKOffset = Xml.GetChildVector3Attributes(reader, "FirstPersonDriveByRightPassengerUnarmedIKOffset");
                                break;
                            case "FirstPersonMobilePhoneOffset":
                                FirstPersonMobilePhoneOffset = Xml.GetChildVector3Attributes(reader, "FirstPersonMobilePhoneOffset");
                                break;
                            case "FirstPersonPassengerMobilePhoneOffset":
                                FirstPersonPassengerMobilePhoneOffset = Xml.GetChildVector3Attributes(reader, "FirstPersonPassengerMobilePhoneOffset");
                                break;
                            case "PovCameraOffset":
                                PovCameraOffset = Xml.GetChildVector3Attributes(reader, "PovCameraOffset");
                                break;
                            case "PovCameraVerticalAdjustmentForRollCage":
                                PovCameraVerticalAdjustmentForRollCage = Xml.GetChildVector3Attributes(reader, "PovCameraVerticalAdjustmentForRollCage");
                                break;
                            case "PovPassengerCameraOffset":
                                PovPassengerCameraOffset = Xml.GetChildVector3Attributes(reader, "PovPassengerCameraOffset");
                                break;
                            case "PovRearPassengerCameraOffset":
                                PovRearPassengerCameraOffset = Xml.GetChildVector3Attributes(reader, "PovRearPassengerCameraOffset");
                                break;
                            case "vfxInfoName":
                                vfxInfoName = Xml.GetChildInnerText(reader, "vfxInfoName");
                                break;
                            case "shouldUseCinematicViewMode":
                                shouldUseCinematicViewMode = Xml.GetChildBoolAttribute(reader, "shouldUseCinematicViewMode");
                                break;
                            case "shouldCameraTransitionOnClimbUpDown":
                                shouldCameraTransitionOnClimbUpDown = Xml.GetChildBoolAttribute(reader, "shouldCameraTransitionOnClimbUpDown");
                                break;
                            case "shouldCameraIgnoreExiting":
                                shouldCameraIgnoreExiting = Xml.GetChildBoolAttribute(reader, "shouldCameraIgnoreExiting");
                                break;
                            case "AllowPretendOccupants":
                                AllowPretendOccupants = Xml.GetChildBoolAttribute(reader, "AllowPretendOccupants");
                                break;
                            case "AllowJoyriding":
                                AllowJoyriding = Xml.GetChildBoolAttribute(reader, "AllowJoyriding");
                                break;
                            case "AllowSundayDriving":
                                AllowSundayDriving = Xml.GetChildBoolAttribute(reader, "AllowSundayDriving");
                                break;
                            case "AllowBodyColorMapping":
                                AllowBodyColorMapping = Xml.GetChildBoolAttribute(reader, "AllowBodyColorMapping");
                                break;
                            case "wheelScale":
                                wheelScale = Xml.GetChildFloatAttribute(reader, "wheelScale");
                                break;
                            case "wheelScaleRear":
                                wheelScaleRear = Xml.GetChildFloatAttribute(reader, "wheelScaleRear");
                                break;
                            case "dirtLevelMin":
                                dirtLevelMin = Xml.GetChildFloatAttribute(reader, "dirtLevelMin");
                                break;
                            case "dirtLevelMax":
                                dirtLevelMax = Xml.GetChildFloatAttribute(reader, "dirtLevelMax");
                                break;
                            case "envEffScaleMin":
                                envEffScaleMin = Xml.GetChildFloatAttribute(reader, "envEffScaleMin");
                                break;
                            case "envEffScaleMax":
                                envEffScaleMax = Xml.GetChildFloatAttribute(reader, "envEffScaleMax");
                                break;
                            case "envEffScaleMin2":
                                envEffScaleMin2 = Xml.GetChildFloatAttribute(reader, "envEffScaleMin2");
                                break;
                            case "envEffScaleMax2":
                                envEffScaleMax2 = Xml.GetChildFloatAttribute(reader, "envEffScaleMax2");
                                break;
                            case "damageMapScale":
                                damageMapScale = Xml.GetChildFloatAttribute(reader, "damageMapScale");
                                break;
                            case "damageOffsetScale":
                                damageOffsetScale = Xml.GetChildFloatAttribute(reader, "damageOffsetScale");
                                break;
                            case "diffuseTint":
                                diffuseTint = new Color4(Convert.ToUInt32(Xml.GetChildStringAttribute(reader, "diffuseTint", "value").Replace("0x", ""), 16)); ;
                                break;
                            case "steerWheelMult":
                                steerWheelMult = Xml.GetChildFloatAttribute(reader, "steerWheelMult");
                                break;
                            case "HDTextureDist":
                                HDTextureDist = Xml.GetChildFloatAttribute(reader, "HDTextureDist");
                                break;
                            case "lodDistances":
                                lodDistances = GetFloatArray(reader, "lodDistances", '\n');
                                break;
                            case "minSeatHeight":
                                minSeatHeight = Xml.GetChildFloatAttribute(reader, "minSeatHeight");
                                break;
                            case "identicalModelSpawnDistance":
                                identicalModelSpawnDistance = Xml.GetChildFloatAttribute(reader, "identicalModelSpawnDistance");
                                break;
                            case "maxNumOfSameColor":
                                maxNumOfSameColor = Xml.GetChildIntAttribute(reader, "maxNumOfSameColor");
                                break;
                            case "defaultBodyHealth":
                                defaultBodyHealth = Xml.GetChildFloatAttribute(reader, "defaultBodyHealth");
                                break;
                            case "pretendOccupantsScale":
                                pretendOccupantsScale = Xml.GetChildFloatAttribute(reader, "pretendOccupantsScale");
                                break;
                            case "visibleSpawnDistScale":
                                visibleSpawnDistScale = Xml.GetChildFloatAttribute(reader, "visibleSpawnDistScale");
                                break;
                            case "trackerPathWidth":
                                trackerPathWidth = Xml.GetChildFloatAttribute(reader, "trackerPathWidth");
                                break;
                            case "weaponForceMult":
                                weaponForceMult = Xml.GetChildFloatAttribute(reader, "weaponForceMult");
                                break;
                            case "frequency":
                                frequency = Xml.GetChildFloatAttribute(reader, "frequency");
                                break;
                            case "swankness":
                                swankness = Xml.GetChildInnerText(reader, "swankness");
                                break;
                            case "maxNum":
                                maxNum = Xml.GetChildIntAttribute(reader, "maxNum", "value");
                                break;
                            case "flags":
                                flags = GetStringArray(reader, "flags", ' ');
                                break;
                            case "type":
                                type = Xml.GetChildInnerText(reader, "type");
                                break;
                            case "plateType":
                                plateType = Xml.GetChildInnerText(reader, "plateType");
                                break;
                            case "dashboardType":
                                dashboardType = Xml.GetChildInnerText(reader, "dashboardType");
                                break;
                            case "vehicleClass":
                                vehicleClass = Xml.GetChildInnerText(reader, "vehicleClass");
                                break;
                            case "wheelType":
                                wheelType = Xml.GetChildInnerText(reader, "wheelType");
                                break;
                            case "trailers":
                                trailers = GetStringItemArray(reader, "trailers");
                                break;
                            case "additionalTrailers":
                                additionalTrailers = GetStringItemArray(reader, "additionalTrailers");
                                break;
                            case "drivers":
                                if (reader.IsEmptyElement)
                                {
                                    reader.ReadStartElement();
                                    break;
                                }

                                var _drivers = new List<VehicleDriver>();

                                foreach (var item in Xml.IterateItems(reader, "drivers"))
                                {
                                    var driver = new VehicleDriver();
                                    driver.driverName = item.Element("driverName")?.Value ?? string.Empty;
                                    driver.npcName = item.Element("npcName")?.Value ?? string.Empty;

                                    if (!string.IsNullOrEmpty(driver.npcName) || !string.IsNullOrEmpty(driver.driverName))
                                    {
                                        _drivers.Add(driver);
                                    }
                                }
                                drivers = _drivers.ToArray();
                                break;
                            case "doorsWithCollisionWhenClosed":
                                doorsWithCollisionWhenClosed = GetStringItemArray(reader, "doorsWithCollisionWhenClosed");
                                break;
                            case "driveableDoors":
                                driveableDoors = GetStringItemArray(reader, "driveableDoors");
                                break;
                            case "bumpersNeedToCollideWithMap":
                                bumpersNeedToCollideWithMap = Xml.GetChildBoolAttribute(reader, "bumpersNeedToCollideWithMap", "value");
                                break;
                            case "needsRopeTexture":
                                needsRopeTexture = Xml.GetChildBoolAttribute(reader, "needsRopeTexture", "value");
                                break;
                            case "requiredExtras":
                                requiredExtras = GetStringArray(reader, "requiredExtras", ' ');
                                break;
                            case "rewards":
                                rewards = GetStringItemArray(reader, "rewards");
                                break;
                            case "cinematicPartCamera":
                                cinematicPartCamera = GetStringItemArray(reader, "cinematicPartCamera");
                                break;
                            case "NmBraceOverrideSet":
                                NmBraceOverrideSet = Xml.GetChildInnerText(reader, "NmBraceOverrideSet");
                                break;
                            case "buoyancySphereOffset":
                                buoyancySphereOffset = Xml.GetChildVector3Attributes(reader, "buoyancySphereOffset");
                                break;
                            case "buoyancySphereSizeScale":
                                buoyancySphereSizeScale = Xml.GetChildFloatAttribute(reader, "buoyancySphereSizeScale", "value");
                                break;
                            case "pOverrideRagdollThreshold":
                                if (reader.IsEmptyElement)
                                {
                                    reader.ReadStartElement();
                                    break;
                                }

                                switch (reader.GetAttribute("type"))
                                {
                                    case "NULL":
                                        break;
                                    case "CVehicleModelInfo__CVehicleOverrideRagdollThreshold":
                                        reader.ReadStartElement();
                                        pOverrideRagdollThreshold = new VehicleOverrideRagdollThreshold();
                                        while (reader.MoveToContent() == XmlNodeType.Element)
                                        {
                                            if (reader.Name == "MinComponent")
                                            {
                                                pOverrideRagdollThreshold.MinComponent = Xml.GetChildIntAttribute(reader, "MinComponent", "value");
                                            }
                                            else if (reader.Name == "MaxComponent")
                                            {
                                                pOverrideRagdollThreshold.MaxComponent = Xml.GetChildIntAttribute(reader, "MaxComponent", "value");
                                            }
                                            else if (reader.Name == "ThresholdMult")
                                            {
                                                pOverrideRagdollThreshold.ThresholdMult = Xml.GetChildFloatAttribute(reader, "ThresholdMult", "value");
                                            } else
                                            {
                                                break;
                                            }
                                        }

                                        reader.ReadEndElement();
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            case "firstPersonDrivebyData":
                                firstPersonDrivebyData = GetStringItemArray(reader, "firstPersonDrivebyData");
                                break;
                            case "extraIncludes":
                                extraIncludes = GetStringItemArray(reader, "extraIncludes");
                                break;
                            case "FirstPersonDriveByLeftPassengerIKOffset":
                            case "FirstPersonDriveByLeftPassengerUnarmedIKOffset":
                            default:
                                reader.Skip();
                                break;
                        }
                    }
                }
                else
                {
                    reader.Read();
                }
            }

            reader.ReadEndElement();
        }

        public void Load(XmlNode node)
        {
            modelName = Xml.GetChildInnerText(node, "modelName");
            txdName = Xml.GetChildInnerText(node, "txdName");
            handlingId = Xml.GetChildInnerText(node, "handlingId");
            gameName = Xml.GetChildInnerText(node, "gameName");
            vehicleMakeName = Xml.GetChildInnerText(node, "vehicleMakeName");
            expressionDictName = Xml.GetChildInnerText(node, "expressionDictName");
            expressionName = Xml.GetChildInnerText(node, "expressionName");
            animConvRoofDictName = Xml.GetChildInnerText(node, "animConvRoofDictName");
            animConvRoofName = Xml.GetChildInnerText(node, "animConvRoofName");
            animConvRoofWindowsAffected = GetStringItemArray(node, "animConvRoofWindowsAffected");//?
            ptfxAssetName = Xml.GetChildInnerText(node, "ptfxAssetName");
            audioNameHash = Xml.GetChildInnerText(node, "audioNameHash");
            layout = Xml.GetChildInnerText(node, "layout");
            coverBoundOffsets = Xml.GetChildInnerText(node, "coverBoundOffsets");
            explosionInfo = Xml.GetChildInnerText(node, "explosionInfo");
            scenarioLayout = Xml.GetChildInnerText(node, "scenarioLayout");
            cameraName = Xml.GetChildInnerText(node, "cameraName");
            aimCameraName = Xml.GetChildInnerText(node, "aimCameraName");
            bonnetCameraName = Xml.GetChildInnerText(node, "bonnetCameraName");
            povCameraName = Xml.GetChildInnerText(node, "povCameraName");
            FirstPersonDriveByIKOffset = Xml.GetChildVector3Attributes(node, "FirstPersonDriveByIKOffset");
            FirstPersonDriveByUnarmedIKOffset = Xml.GetChildVector3Attributes(node, "FirstPersonDriveByUnarmedIKOffset");
            FirstPersonProjectileDriveByIKOffset = Xml.GetChildVector3Attributes(node, "FirstPersonProjectileDriveByIKOffset");
            FirstPersonProjectileDriveByPassengerIKOffset = Xml.GetChildVector3Attributes(node, "FirstPersonProjectileDriveByPassengerIKOffset");
            FirstPersonDriveByRightPassengerIKOffset = Xml.GetChildVector3Attributes(node, "FirstPersonDriveByRightPassengerIKOffset");
            FirstPersonDriveByRightPassengerUnarmedIKOffset = Xml.GetChildVector3Attributes(node, "FirstPersonDriveByRightPassengerUnarmedIKOffset");
            FirstPersonMobilePhoneOffset = Xml.GetChildVector3Attributes(node, "FirstPersonMobilePhoneOffset");
            FirstPersonPassengerMobilePhoneOffset = Xml.GetChildVector3Attributes(node, "FirstPersonPassengerMobilePhoneOffset");
            PovCameraOffset = Xml.GetChildVector3Attributes(node, "PovCameraOffset");
            PovCameraVerticalAdjustmentForRollCage = Xml.GetChildVector3Attributes(node, "PovCameraVerticalAdjustmentForRollCage");
            PovPassengerCameraOffset = Xml.GetChildVector3Attributes(node, "PovPassengerCameraOffset");
            PovRearPassengerCameraOffset = Xml.GetChildVector3Attributes(node, "PovRearPassengerCameraOffset");
            vfxInfoName = Xml.GetChildInnerText(node, "vfxInfoName");
            shouldUseCinematicViewMode = Xml.GetChildBoolAttribute(node, "shouldUseCinematicViewMode", "value");
            shouldCameraTransitionOnClimbUpDown = Xml.GetChildBoolAttribute(node, "shouldCameraTransitionOnClimbUpDown", "value");
            shouldCameraIgnoreExiting = Xml.GetChildBoolAttribute(node, "shouldCameraIgnoreExiting", "value");
            AllowPretendOccupants = Xml.GetChildBoolAttribute(node, "AllowPretendOccupants", "value");
            AllowJoyriding = Xml.GetChildBoolAttribute(node, "AllowJoyriding", "value");
            AllowSundayDriving = Xml.GetChildBoolAttribute(node, "AllowSundayDriving", "value");
            AllowBodyColorMapping = Xml.GetChildBoolAttribute(node, "AllowBodyColorMapping", "value");
            wheelScale = Xml.GetChildFloatAttribute(node, "wheelScale", "value");
            wheelScaleRear = Xml.GetChildFloatAttribute(node, "wheelScaleRear", "value");
            dirtLevelMin = Xml.GetChildFloatAttribute(node, "dirtLevelMin", "value");
            dirtLevelMax = Xml.GetChildFloatAttribute(node, "dirtLevelMax", "value");
            envEffScaleMin = Xml.GetChildFloatAttribute(node, "envEffScaleMin", "value");
            envEffScaleMax = Xml.GetChildFloatAttribute(node, "envEffScaleMax", "value");
            envEffScaleMin2 = Xml.GetChildFloatAttribute(node, "envEffScaleMin2", "value");
            envEffScaleMax2 = Xml.GetChildFloatAttribute(node, "envEffScaleMax2", "value");
            damageMapScale = Xml.GetChildFloatAttribute(node, "damageMapScale", "value");
            damageOffsetScale = Xml.GetChildFloatAttribute(node, "damageOffsetScale", "value");
            diffuseTint = new Color4(Convert.ToUInt32(Xml.GetChildStringAttribute(node, "diffuseTint", "value").Replace("0x", ""), 16));
            steerWheelMult = Xml.GetChildFloatAttribute(node, "steerWheelMult", "value");
            HDTextureDist = Xml.GetChildFloatAttribute(node, "HDTextureDist", "value");
            lodDistances = GetFloatArray(node, "lodDistances", '\n');
            minSeatHeight = Xml.GetChildFloatAttribute(node, "minSeatHeight", "value");
            identicalModelSpawnDistance = Xml.GetChildFloatAttribute(node, "identicalModelSpawnDistance", "value");
            maxNumOfSameColor = Xml.GetChildIntAttribute(node, "maxNumOfSameColor", "value");
            defaultBodyHealth = Xml.GetChildFloatAttribute(node, "defaultBodyHealth", "value");
            pretendOccupantsScale = Xml.GetChildFloatAttribute(node, "pretendOccupantsScale", "value");
            visibleSpawnDistScale = Xml.GetChildFloatAttribute(node, "visibleSpawnDistScale", "value");
            trackerPathWidth = Xml.GetChildFloatAttribute(node, "trackerPathWidth", "value");
            weaponForceMult = Xml.GetChildFloatAttribute(node, "weaponForceMult", "value");
            frequency = Xml.GetChildFloatAttribute(node, "frequency", "value");
            swankness = Xml.GetChildInnerText(node, "swankness");
            maxNum = Xml.GetChildIntAttribute(node, "maxNum", "value");
            flags = GetStringArray(node, "flags", ' ');
            type = Xml.GetChildInnerText(node, "type");
            plateType = Xml.GetChildInnerText(node, "plateType");
            dashboardType = Xml.GetChildInnerText(node, "dashboardType");
            vehicleClass = Xml.GetChildInnerText(node, "vehicleClass");
            wheelType = Xml.GetChildInnerText(node, "wheelType");
            trailers = GetStringItemArray(node, "trailers");
            additionalTrailers = GetStringItemArray(node, "additionalTrailers");
            var dnode = node.SelectSingleNode("drivers");
            if (dnode != null)
            {
                var items = dnode.SelectNodes("Item");
                if (items.Count > 0)
                {
                    drivers = new VehicleDriver[items.Count];
                    for (int i = 0; i < items.Count; i++)
                    {
                        var item = items[i];
                        var driver = new VehicleDriver();
                        driver.driverName = Xml.GetChildInnerText(item, "driverName");
                        driver.npcName = Xml.GetChildInnerText(item, "npcName");
                        drivers[i] = driver;
                    }
                }
            }
            extraIncludes = GetStringItemArray(node, "extraIncludes");
            doorsWithCollisionWhenClosed = GetStringItemArray(node, "doorsWithCollisionWhenClosed");
            driveableDoors = GetStringItemArray(node, "driveableDoors");
            bumpersNeedToCollideWithMap = Xml.GetChildBoolAttribute(node, "bumpersNeedToCollideWithMap", "value");
            needsRopeTexture = Xml.GetChildBoolAttribute(node, "needsRopeTexture", "value");
            requiredExtras = GetStringArray(node, "requiredExtras", ' ');
            rewards = GetStringItemArray(node, "rewards");
            cinematicPartCamera = GetStringItemArray(node, "cinematicPartCamera");
            NmBraceOverrideSet = Xml.GetChildInnerText(node, "NmBraceOverrideSet");
            buoyancySphereOffset = Xml.GetChildVector3Attributes(node, "buoyancySphereOffset");
            buoyancySphereSizeScale = Xml.GetChildFloatAttribute(node, "buoyancySphereSizeScale", "value");
            var tnode = node.SelectSingleNode("pOverrideRagdollThreshold");
            if (tnode != null)
            {
                var ttype = tnode.Attributes["type"]?.Value;
                switch (ttype)
                {
                    case "NULL": break;
                    case "CVehicleModelInfo__CVehicleOverrideRagdollThreshold":
                        pOverrideRagdollThreshold = new VehicleOverrideRagdollThreshold();
                        pOverrideRagdollThreshold.MinComponent = Xml.GetChildIntAttribute(tnode, "MinComponent", "value");
                        pOverrideRagdollThreshold.MaxComponent = Xml.GetChildIntAttribute(tnode, "MaxComponent", "value");
                        pOverrideRagdollThreshold.ThresholdMult = Xml.GetChildFloatAttribute(tnode, "ThresholdMult", "value");
                        break;
                    default:
                        break;
                }
            }
            firstPersonDrivebyData = GetStringItemArray(node, "firstPersonDrivebyData");
        }

        private string[] GetStringItemArray(XmlNode node, string childName)
        {
            var cnode = node.SelectSingleNode(childName);
            if (cnode == null) return null;
            var items = cnode.SelectNodes("Item");
            if (items == null) return null;
            getStringArrayList.Clear();
            foreach (XmlNode inode in items)
            {
                var istr = inode.InnerText;
                if (!string.IsNullOrEmpty(istr))
                {
                    getStringArrayList.Add(istr);
                }
            }
            if (getStringArrayList.Count == 0) return null;
            return getStringArrayList.ToArray();
        }

        private string[] GetStringItemArray(XmlReader node, string childName)
        {
            if (node.IsEmptyElement)
            {
                node.ReadStartElement();
                return null;
            }
                
            lock(getStringArrayList)
            {
                getStringArrayList.Clear();
                node.ReadStartElement();
                while (node.MoveToContent() == XmlNodeType.Element && node.Name == "Item")
                {
                    var istr = node.ReadElementContentAsString();
                    if (!string.IsNullOrEmpty(istr))
                    {
                        getStringArrayList.Add(istr);
                    }
                }
                node.ReadEndElement();

                if (getStringArrayList.Count == 0)
                    return null;
                return getStringArrayList.ToArray();
            }
        }

        private string[] GetStringArray(string ldastr, char delimiter)
        {
            var ldarr = ldastr?.Split(delimiter);
            if (ldarr == null) return null;
            lock(getStringArrayList)
            {
                getStringArrayList.Clear();
                foreach (var ldstr in ldarr)
                {
                    var ldt = ldstr?.Trim();
                    if (!string.IsNullOrEmpty(ldt))
                    {
                        getStringArrayList.Add(ldt);
                    }
                }
                if (getStringArrayList.Count == 0) return null;
                return getStringArrayList.ToArray();
            }
        }

        private string[] GetStringArray(XmlNode node, string childName, char delimiter)
        {
            var ldastr = Xml.GetChildInnerText(node, childName);
            return GetStringArray(ldastr, delimiter);
        }

        private string[] GetStringArray(XmlReader reader, string childName, char delimiter)
        {
            var ldastr = Xml.GetChildInnerText(reader, childName);
            return GetStringArray(ldastr, delimiter);
        }

        private unsafe float[] GetFloatArray(string ldastr, char delimiter)
        {
            var ldarr = ldastr?.Split(delimiter);
            if (ldarr == null) return null;
            var floats = stackalloc float[ldarr.Length];
            var i = 0;
            foreach (var ldstr in ldarr)
            {
                var ldt = ldstr?.Trim();
                if (!string.IsNullOrEmpty(ldt))
                {
                    float f;
                    if (FloatUtil.TryParse(ldt, out f))
                    {
                        floats[i] = f;
                        i++;
                    }
                }
            }
            if (i == 0) return null;

            var result = new float[i];

            Marshal.Copy((IntPtr)floats, result, 0, i);

            return result;
        }

        private float[] GetFloatArray(XmlNode node, string childName, char delimiter)
        {
            var ldastr = Xml.GetChildInnerText(node, childName);
            return GetFloatArray(ldastr, delimiter);
        }

        private float[] GetFloatArray(XmlReader reader, string childName, char delimiter)
        {
            var ldastr = Xml.GetChildInnerText(reader, childName);
            return GetFloatArray(ldastr, delimiter);
        }

        private static List<string> getStringArrayList = new List<string>(); //kinda hacky..
        private static List<float> getFloatArrayList = new List<float>(); //kinda hacky..


        public override string ToString()
        {
            return modelName;
        }

        public override bool Equals(object obj)
        {
            return obj is VehicleInitData data &&
                   modelName == data.modelName &&
                   txdName == data.txdName &&
                   handlingId == data.handlingId &&
                   gameName == data.gameName &&
                   vehicleMakeName == data.vehicleMakeName &&
                   expressionDictName == data.expressionDictName &&
                   expressionName == data.expressionName &&
                   animConvRoofDictName == data.animConvRoofDictName &&
                   animConvRoofName == data.animConvRoofName &&
                   animConvRoofWindowsAffected == data.animConvRoofWindowsAffected &&
                   ptfxAssetName == data.ptfxAssetName &&
                   audioNameHash == data.audioNameHash &&
                   layout == data.layout &&
                   coverBoundOffsets == data.coverBoundOffsets &&
                   explosionInfo == data.explosionInfo &&
                   scenarioLayout == data.scenarioLayout &&
                   cameraName == data.cameraName &&
                   aimCameraName == data.aimCameraName &&
                   bonnetCameraName == data.bonnetCameraName &&
                   povCameraName == data.povCameraName &&
                   FirstPersonDriveByIKOffset.Equals(data.FirstPersonDriveByIKOffset) &&
                   FirstPersonDriveByUnarmedIKOffset.Equals(data.FirstPersonDriveByUnarmedIKOffset) &&
                   FirstPersonProjectileDriveByIKOffset.Equals(data.FirstPersonProjectileDriveByIKOffset) &&
                   FirstPersonProjectileDriveByPassengerIKOffset.Equals(data.FirstPersonProjectileDriveByPassengerIKOffset) &&
                   FirstPersonDriveByRightPassengerIKOffset.Equals(data.FirstPersonDriveByRightPassengerIKOffset) &&
                   FirstPersonDriveByRightPassengerUnarmedIKOffset.Equals(data.FirstPersonDriveByRightPassengerUnarmedIKOffset) &&
                   FirstPersonMobilePhoneOffset.Equals(data.FirstPersonMobilePhoneOffset) &&
                   FirstPersonPassengerMobilePhoneOffset.Equals(data.FirstPersonPassengerMobilePhoneOffset) &&
                   PovCameraOffset.Equals(data.PovCameraOffset) &&
                   PovCameraVerticalAdjustmentForRollCage.Equals(data.PovCameraVerticalAdjustmentForRollCage) &&
                   PovPassengerCameraOffset.Equals(data.PovPassengerCameraOffset) &&
                   PovRearPassengerCameraOffset.Equals(data.PovRearPassengerCameraOffset) &&
                   vfxInfoName == data.vfxInfoName &&
                   shouldUseCinematicViewMode == data.shouldUseCinematicViewMode &&
                   shouldCameraTransitionOnClimbUpDown == data.shouldCameraTransitionOnClimbUpDown &&
                   shouldCameraIgnoreExiting == data.shouldCameraIgnoreExiting &&
                   AllowPretendOccupants == data.AllowPretendOccupants &&
                   AllowJoyriding == data.AllowJoyriding &&
                   AllowSundayDriving == data.AllowSundayDriving &&
                   AllowBodyColorMapping == data.AllowBodyColorMapping &&
                   wheelScale == data.wheelScale &&
                   wheelScaleRear == data.wheelScaleRear &&
                   dirtLevelMin == data.dirtLevelMin &&
                   dirtLevelMax == data.dirtLevelMax &&
                   envEffScaleMin == data.envEffScaleMin &&
                   envEffScaleMax == data.envEffScaleMax &&
                   envEffScaleMin2 == data.envEffScaleMin2 &&
                   envEffScaleMax2 == data.envEffScaleMax2 &&
                   damageMapScale == data.damageMapScale &&
                   damageOffsetScale == data.damageOffsetScale &&
                   diffuseTint.Equals(data.diffuseTint) &&
                   steerWheelMult == data.steerWheelMult &&
                   HDTextureDist == data.HDTextureDist &&
                   StructuralComparisons.StructuralEqualityComparer.Equals(lodDistances, data.lodDistances) &&
                   minSeatHeight == data.minSeatHeight &&
                   identicalModelSpawnDistance == data.identicalModelSpawnDistance &&
                   maxNumOfSameColor == data.maxNumOfSameColor &&
                   defaultBodyHealth == data.defaultBodyHealth &&
                   pretendOccupantsScale == data.pretendOccupantsScale &&
                   visibleSpawnDistScale == data.visibleSpawnDistScale &&
                   trackerPathWidth == data.trackerPathWidth &&
                   weaponForceMult == data.weaponForceMult &&
                   frequency == data.frequency &&
                   swankness == data.swankness &&
                   maxNum == data.maxNum &&
                   StructuralComparisons.StructuralEqualityComparer.Equals(flags, data.flags) &&
                   type == data.type &&
                   plateType == data.plateType &&
                   dashboardType == data.dashboardType &&
                   vehicleClass == data.vehicleClass &&
                   wheelType == data.wheelType &&
                   StructuralComparisons.StructuralEqualityComparer.Equals(trailers, data.trailers) &&
                   StructuralComparisons.StructuralEqualityComparer.Equals(additionalTrailers, data.additionalTrailers) &&
                   StructuralComparisons.StructuralEqualityComparer.Equals(drivers, data.drivers) &&
                   StructuralComparisons.StructuralEqualityComparer.Equals(extraIncludes, data.extraIncludes) &&
                   StructuralComparisons.StructuralEqualityComparer.Equals(doorsWithCollisionWhenClosed, data.doorsWithCollisionWhenClosed) &&
                   StructuralComparisons.StructuralEqualityComparer.Equals(driveableDoors, data.driveableDoors) &&
                   bumpersNeedToCollideWithMap == data.bumpersNeedToCollideWithMap &&
                   needsRopeTexture == data.needsRopeTexture &&
                   StructuralComparisons.StructuralEqualityComparer.Equals(requiredExtras, data.requiredExtras) &&
                   StructuralComparisons.StructuralEqualityComparer.Equals(rewards, data.rewards) &&
                   StructuralComparisons.StructuralEqualityComparer.Equals(cinematicPartCamera, data.cinematicPartCamera) &&
                   NmBraceOverrideSet == data.NmBraceOverrideSet &&
                   buoyancySphereOffset.Equals(data.buoyancySphereOffset) &&
                   buoyancySphereSizeScale == data.buoyancySphereSizeScale &&
                   EqualityComparer<VehicleOverrideRagdollThreshold>.Default.Equals(pOverrideRagdollThreshold, data.pOverrideRagdollThreshold) &&
                   StructuralComparisons.StructuralEqualityComparer.Equals(firstPersonDrivebyData, data.firstPersonDrivebyData);
        }

        public override int GetHashCode()
        {
            int hashCode = 1102137281;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(modelName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(txdName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(handlingId);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(gameName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(vehicleMakeName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(expressionDictName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(expressionName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(animConvRoofDictName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(animConvRoofName);
            hashCode = hashCode * -1521134295 + StructuralComparisons.StructuralEqualityComparer.GetHashCode(animConvRoofWindowsAffected);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ptfxAssetName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(audioNameHash);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(layout);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(coverBoundOffsets);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(explosionInfo);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(scenarioLayout);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(cameraName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(aimCameraName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(bonnetCameraName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(povCameraName);
            hashCode = hashCode * -1521134295 + FirstPersonDriveByIKOffset.GetHashCode();
            hashCode = hashCode * -1521134295 + FirstPersonDriveByUnarmedIKOffset.GetHashCode();
            hashCode = hashCode * -1521134295 + FirstPersonProjectileDriveByIKOffset.GetHashCode();
            hashCode = hashCode * -1521134295 + FirstPersonProjectileDriveByPassengerIKOffset.GetHashCode();
            hashCode = hashCode * -1521134295 + FirstPersonDriveByRightPassengerIKOffset.GetHashCode();
            hashCode = hashCode * -1521134295 + FirstPersonDriveByRightPassengerUnarmedIKOffset.GetHashCode();
            hashCode = hashCode * -1521134295 + FirstPersonMobilePhoneOffset.GetHashCode();
            hashCode = hashCode * -1521134295 + FirstPersonPassengerMobilePhoneOffset.GetHashCode();
            hashCode = hashCode * -1521134295 + PovCameraOffset.GetHashCode();
            hashCode = hashCode * -1521134295 + PovCameraVerticalAdjustmentForRollCage.GetHashCode();
            hashCode = hashCode * -1521134295 + PovPassengerCameraOffset.GetHashCode();
            hashCode = hashCode * -1521134295 + PovRearPassengerCameraOffset.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(vfxInfoName);
            hashCode = hashCode * -1521134295 + shouldUseCinematicViewMode.GetHashCode();
            hashCode = hashCode * -1521134295 + shouldCameraTransitionOnClimbUpDown.GetHashCode();
            hashCode = hashCode * -1521134295 + shouldCameraIgnoreExiting.GetHashCode();
            hashCode = hashCode * -1521134295 + AllowPretendOccupants.GetHashCode();
            hashCode = hashCode * -1521134295 + AllowJoyriding.GetHashCode();
            hashCode = hashCode * -1521134295 + AllowSundayDriving.GetHashCode();
            hashCode = hashCode * -1521134295 + AllowBodyColorMapping.GetHashCode();
            hashCode = hashCode * -1521134295 + wheelScale.GetHashCode();
            hashCode = hashCode * -1521134295 + wheelScaleRear.GetHashCode();
            hashCode = hashCode * -1521134295 + dirtLevelMin.GetHashCode();
            hashCode = hashCode * -1521134295 + dirtLevelMax.GetHashCode();
            hashCode = hashCode * -1521134295 + envEffScaleMin.GetHashCode();
            hashCode = hashCode * -1521134295 + envEffScaleMax.GetHashCode();
            hashCode = hashCode * -1521134295 + envEffScaleMin2.GetHashCode();
            hashCode = hashCode * -1521134295 + envEffScaleMax2.GetHashCode();
            hashCode = hashCode * -1521134295 + damageMapScale.GetHashCode();
            hashCode = hashCode * -1521134295 + damageOffsetScale.GetHashCode();
            hashCode = hashCode * -1521134295 + diffuseTint.GetHashCode();
            hashCode = hashCode * -1521134295 + steerWheelMult.GetHashCode();
            hashCode = hashCode * -1521134295 + HDTextureDist.GetHashCode();
            hashCode = hashCode * -1521134295 + StructuralComparisons.StructuralEqualityComparer.GetHashCode(lodDistances);
            hashCode = hashCode * -1521134295 + minSeatHeight.GetHashCode();
            hashCode = hashCode * -1521134295 + identicalModelSpawnDistance.GetHashCode();
            hashCode = hashCode * -1521134295 + maxNumOfSameColor.GetHashCode();
            hashCode = hashCode * -1521134295 + defaultBodyHealth.GetHashCode();
            hashCode = hashCode * -1521134295 + pretendOccupantsScale.GetHashCode();
            hashCode = hashCode * -1521134295 + visibleSpawnDistScale.GetHashCode();
            hashCode = hashCode * -1521134295 + trackerPathWidth.GetHashCode();
            hashCode = hashCode * -1521134295 + weaponForceMult.GetHashCode();
            hashCode = hashCode * -1521134295 + frequency.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(swankness);
            hashCode = hashCode * -1521134295 + maxNum.GetHashCode();
            hashCode = hashCode * -1521134295 + StructuralComparisons.StructuralEqualityComparer.GetHashCode(flags);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(type);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(plateType);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(dashboardType);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(vehicleClass);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(wheelType);
            hashCode = hashCode * -1521134295 + StructuralComparisons.StructuralEqualityComparer.GetHashCode(trailers);
            hashCode = hashCode * -1521134295 + StructuralComparisons.StructuralEqualityComparer.GetHashCode(additionalTrailers);
            hashCode = hashCode * -1521134295 + StructuralComparisons.StructuralEqualityComparer.GetHashCode(drivers);
            hashCode = hashCode * -1521134295 + StructuralComparisons.StructuralEqualityComparer.GetHashCode(extraIncludes);
            hashCode = hashCode * -1521134295 + StructuralComparisons.StructuralEqualityComparer.GetHashCode(doorsWithCollisionWhenClosed);
            hashCode = hashCode * -1521134295 + StructuralComparisons.StructuralEqualityComparer.GetHashCode(driveableDoors);
            hashCode = hashCode * -1521134295 + bumpersNeedToCollideWithMap.GetHashCode();
            hashCode = hashCode * -1521134295 + needsRopeTexture.GetHashCode();
            hashCode = hashCode * -1521134295 + StructuralComparisons.StructuralEqualityComparer.GetHashCode(requiredExtras);
            hashCode = hashCode * -1521134295 + StructuralComparisons.StructuralEqualityComparer.GetHashCode(rewards);
            hashCode = hashCode * -1521134295 + StructuralComparisons.StructuralEqualityComparer.GetHashCode(cinematicPartCamera);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(NmBraceOverrideSet);
            hashCode = hashCode * -1521134295 + buoyancySphereOffset.GetHashCode();
            hashCode = hashCode * -1521134295 + buoyancySphereSizeScale.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<VehicleOverrideRagdollThreshold>.Default.GetHashCode(pOverrideRagdollThreshold);
            hashCode = hashCode * -1521134295 + StructuralComparisons.StructuralEqualityComparer.GetHashCode(firstPersonDrivebyData);
            return hashCode;
        }

        public static bool operator ==(VehicleInitData left, VehicleInitData right)
        {
            return EqualityComparer<VehicleInitData>.Default.Equals(left, right);
        }

        public static bool operator !=(VehicleInitData left, VehicleInitData right)
        {
            return !(left == right);
        }
    }

    public class VehicleOverrideRagdollThreshold : IEquatable<VehicleOverrideRagdollThreshold>
    {
        public int MinComponent { get; set; }
        public int MaxComponent { get; set; }
        public float ThresholdMult { get; set; }

        public override bool Equals(object obj)
        {
            return obj is VehicleOverrideRagdollThreshold threshold && Equals(threshold);
        }

        public bool Equals(VehicleOverrideRagdollThreshold other)
        {
            return MinComponent == other.MinComponent &&
                   MaxComponent == other.MaxComponent &&
                   ThresholdMult == other.ThresholdMult;
        }

        public override int GetHashCode()
        {
            int hashCode = 1172526364;
            hashCode = hashCode * -1521134295 + MinComponent.GetHashCode();
            hashCode = hashCode * -1521134295 + MaxComponent.GetHashCode();
            hashCode = hashCode * -1521134295 + ThresholdMult.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return MinComponent.ToString() + ", " + MaxComponent.ToString() + ", " + ThresholdMult.ToString();
        }

        public static bool operator ==(VehicleOverrideRagdollThreshold left, VehicleOverrideRagdollThreshold right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(VehicleOverrideRagdollThreshold left, VehicleOverrideRagdollThreshold right)
        {
            return !(left == right);
        }
    }
    public class VehicleDriver : IEquatable<VehicleDriver>
    {
        public string driverName { get; set; }
        public string npcName { get; set; }

        public override bool Equals(object obj)
        {
            return obj is VehicleDriver driver && Equals(driver);
        }

        public bool Equals(VehicleDriver other)
        {
            return driverName == other.driverName &&
                   npcName == other.npcName;
        }

        public override int GetHashCode()
        {
            int hashCode = -1906737521;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(driverName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(npcName);
            return hashCode;
        }

        public override string ToString()
        {
            return driverName + ", " + npcName;
        }

        public static bool operator ==(VehicleDriver left, VehicleDriver right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(VehicleDriver left, VehicleDriver right)
        {
            return !(left == right);
        }
    }

}
