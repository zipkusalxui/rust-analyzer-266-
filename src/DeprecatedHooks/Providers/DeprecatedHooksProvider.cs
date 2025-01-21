using RustAnalyzer.src.Hooks.Attributes;
using RustAnalyzer.src.DeprecatedHooks.Interfaces;
using RustAnalyzer.Models;
using System;
using System.Reflection;

namespace RustAnalyzer.src.DeprecatedHooks.Providers
{
    [HooksVersion("LastVersion")]
    public class DeprecatedHooksProvider : BaseDeprecatedJsonHooksProvider 
    {
        protected override string JsonContent => "{\r\n  \"deprecated\": {\r\n    \"OnItemResearchEnd(ResearchTable,float)\": \"\",\r\n    \"OnDispenserBonus(ResourceDispenser,BaseEntity,Item)\": \"OnDispenserBonus(ResourceDispenser,BasePlayer,Item)\",\r\n    \"OnDispenserGather(ResourceDispenser,BaseEntity,Item)\": \"OnDispenserGather(ResourceDispenser,BasePlayer,Item)\",\r\n    \"OnServerShutdown(unknown)\": \"OnServerShutdown()\",\r\n    \"CanSetBedPublic(SleepingBag,BasePlayer)\": \"CanSetBedPublic(BasePlayer,SleepingBag)\",\r\n    \"CanPickupEntity(BaseCombatEntity,BasePlayer)\": \"CanPickupEntity(BasePlayer,BaseCombatEntity)\",\r\n    \"OnItemResearchStart(ResearchTable)\": \"\",\r\n    \"CanEquipItem(PlayerInventory,Item)\": \"CanEquipItem(PlayerInventory,Item,int)\",\r\n    \"CanLock(CodeLock,BasePlayer)\": \"CanLock(BasePlayer,ModularCar,ModularCarCodeLock)\",\r\n    \"OnCollectiblePickup(Item,BasePlayer,CollectibleEntity)\": \"OnCollectiblePickup(CollectibleEntity,BasePlayer,bool)\",\r\n    \"OnServerInitialized(unknown)\": \"OnServerInitialized(bool)\",\r\n    \"CanAdministerVending(VendingMachine,BasePlayer)\": \"CanAdministerVending(BasePlayer,VendingMachine)\",\r\n    \"OnStructureUpgrade(BuildingBlock,BasePlayer,BuildingGrade.Enum)\": \"OnStructureUpgrade(BuildingBlock,BasePlayer,BuildingGrade.Enum,ulong)\",\r\n    \"IOnPlayerAttack(BaseMelee,HitInfo)\": \"\",\r\n    \"OnReloadMagazine(BasePlayer,BaseProjectile)\": \"\",\r\n    \"OnPlayerDie(BasePlayer,HitInfo)\": \"\",\r\n    \"CanMoveItem(Item,PlayerInventory,uint,int,int)\": \"CanMoveItem(Item,PlayerInventory,ItemContainerId,int,int,ItemMoveModifier)\",\r\n    \"CanAcceptItem(ItemContainer,Item)\": \"CanAcceptItem(ItemContainer,Item,int)\",\r\n    \"InitLogging(unknown)\": \"InitLogging()\",\r\n    \"OnCropGather(PlantEntity,Item,BasePlayer)\": \"OnGrowableGathered(GrowableEntity,Item,BasePlayer)\",\r\n    \"OnItemAction(Item,string)\": \"OnItemAction(Item,string,BasePlayer)\",\r\n    \"IOnServerUsersRemove(ulong)\": \"\",\r\n    \"CanUseVending(VendingMachine,BasePlayer)\": \"CanUseVending(BasePlayer,VendingMachine)\",\r\n    \"OnPlayerKicked(BasePlayer,string)\": \"OnPlayerKicked(Network.Connection,string)\",\r\n    \"CanSeeStash(StashContainer,BasePlayer)\": \"CanSeeStash(BasePlayer,StashContainer)\",\r\n    \"OnPlayerSpawn(BasePlayer)\": \"OnPlayerSpawn(BasePlayer,Network.Connection)\",\r\n    \"IOnPlayerRevive(MedicalTool,BasePlayer)\": \"\",\r\n    \"OnPlayerInit(BasePlayer)\": \"OnPlayerConnected(BasePlayer)\",\r\n    \"OnSignUpdated(Signage,BasePlayer)\": \"OnSignUpdated(CarvablePumpkin,BasePlayer)\",\r\n    \"IOnServerUsersSet(ulong,ServerUsers.UserGroup,string,string)\": \"\",\r\n    \"OnConsumeFuel(BaseOven,Item,ItemModBurnable)\": \"\",\r\n    \"IOnRconInitialize(unknown)\": \"IOnRconInitialize()\",\r\n    \"OnQuarryEnabled(MiningQuarry)\": \"\",\r\n    \"OnTrapSnapped(BaseTrapTrigger,UnityEngine.GameObject)\": \"OnTrapSnapped(BaseTrapTrigger,UnityEngine.GameObject,UnityEngine.Collider)\",\r\n    \"OnPlayerRespawn(BasePlayer)\": \"OnPlayerRespawn(BasePlayer,SleepingBag)\",\r\n    \"OnNpcPlayerTarget(NPCPlayerApex,BaseEntity)\": \"\",\r\n    \"CanAffordUpgrade(BasePlayer,BuildingBlock,BuildingGrade.Enum)\": \"CanAffordUpgrade(BasePlayer,BuildingBlock,BuildingGrade.Enum,ulong)\",\r\n    \"CanDemolish(BasePlayer,BuildingBlock)\": \"CanDemolish(BasePlayer,StabilityEntity)\",\r\n    \"CanLock(KeyLock,BasePlayer)\": \"CanLock(BasePlayer,ModularCar,ModularCarCodeLock)\",\r\n    \"CanVendingAcceptItem(VendingMachine,Item)\": \"CanVendingAcceptItem(VendingMachine,Item,int)\",\r\n    \"IOnEnableServerConsole(ServerConsole)\": \"\",\r\n    \"OnAddVendingOffer(VendingMachine,BasePlayer,ProtoBuf.VendingMachine.SellOrder)\": \"OnAddVendingOffer(VendingMachine,ProtoBuf.VendingMachine.SellOrder)\",\r\n    \"CanAssignBed(SleepingBag,BasePlayer,ulong)\": \"CanAssignBed(BasePlayer,SleepingBag,ulong)\",\r\n    \"OnExplosiveThrown(BasePlayer,BaseEntity)\": \"OnExplosiveThrown(BasePlayer,BaseEntity,ThrownWeapon)\",\r\n    \"OnCreateWorldProjectile(HitInfo,Item)\": \"\",\r\n    \"CanChangeGrade(BasePlayer,BuildingBlock,BuildingGrade.Enum)\": \"CanChangeGrade(BasePlayer,BuildingBlock,BuildingGrade.Enum,ulong)\",\r\n    \"OnExplosiveDropped(BasePlayer,BaseEntity)\": \"OnExplosiveDropped(BasePlayer,BaseEntity,ThrownWeapon)\",\r\n    \"CanBuild(Planner,Construction,UnityEngine.Vector3)\": \"CanBuild(Planner,Construction,Construction.Target)\",\r\n    \"OnPlayerConnected(Network.Message)\": \"OnPlayerConnected(BasePlayer)\",\r\n    \"CanChangeCode(CodeLock,BasePlayer,string,bool)\": \"CanChangeCode(BasePlayer,CodeLock,string,bool)\",\r\n    \"CanUnlock(KeyLock,BasePlayer)\": \"CanUnlock(BasePlayer,ModularCarCodeLock,string)\",\r\n    \"IOnStructureDemolish(BuildingBlock,BasePlayer)\": \"\",\r\n    \"IOnLootPlayer(PlayerLoot,BasePlayer)\": \"\",\r\n    \"OnItemDeployed(Deployer,BaseEntity)\": \"OnItemDeployed(Deployer,BaseEntity,BaseEntity)\",\r\n    \"CanUnlock(CodeLock,BasePlayer)\": \"CanUnlock(BasePlayer,ModularCarCodeLock,string)\",\r\n    \"OnDeleteVendingOffer(VendingMachine,BasePlayer)\": \"OnDeleteVendingOffer(VendingMachine,int)\",\r\n    \"IOnLootItem(PlayerLoot,Item)\": \"\",\r\n    \"OnRecycleItem(Recycler,Item)\": \"\",\r\n    \"OnTick(unknown)\": \"OnTick()\",\r\n    \"OnTerrainInitialized(unknown)\": \"OnTerrainInitialized()\",\r\n    \"CanWearItem(PlayerInventory,Item)\": \"CanWearItem(PlayerInventory,Item,int)\",\r\n    \"OnRconConnection(System.Net.IPEndPoint)\": \"OnRconConnection(System.Net.IPAddress)\",\r\n    \"OnItemCraftCancelled(ItemCraftTask)\": \"OnItemCraftCancelled(ItemCraftTask,ItemCrafter)\",\r\n    \"OnPlayerChat(ConsoleSystem.Arg)\": \"OnPlayerChat(BasePlayer,string,ConVar.Chat.ChatChannel)\",\r\n    \"IOnLootEntity(PlayerLoot,BaseEntity)\": \"\",\r\n    \"IOnStructureImmediateDemolish(BuildingBlock,BasePlayer)\": \"\",\r\n    \"CanNpcAttack(BaseNpc,BaseEntity)\": \"\",\r\n    \"OnReloadWeapon(BasePlayer,BaseProjectile)\": \"\",\r\n    \"OnVendingTransaction(VendingMachine,BasePlayer,int,int)\": \"OnVendingTransaction(VendingMachine,BasePlayer,int,int,ItemContainer)\",\r\n    \"OnPlayerWound(BasePlayer)\": \"OnPlayerWound(BasePlayer,HitInfo)\",\r\n    \"OnItemCraftFinished(ItemCraftTask,Item)\": \"OnItemCraftFinished(ItemCraftTask,Item,ItemCrafter)\",\r\n    \"CanCraft(ItemCrafter,ItemBlueprint,int)\": \"CanCraft(PlayerBlueprints,ItemDefinition,int)\",\r\n    \"OnItemResearch(Item,BasePlayer)\": \"OnItemResearch(ResearchTable,Item,BasePlayer)\",\r\n    \"IOnDisableServerConsole(unknown)\": \"\",\r\n    \"CanHideStash(StashContainer,BasePlayer)\": \"CanHideStash(BasePlayer,StashContainer)\",\r\n    \"OnMapImageUpdated(unknown)\": \"OnMapImageUpdated()\",\r\n    \"OnOpenVendingShop(VendingMachine,BasePlayer)\": \"\",\r\n    \"IOnRunCommandLine(unknown)\": \"IOnRunCommandLine()\",\r\n    \"OnUserBanned(string,string,string,string)\": \"OnUserBanned(string,string,string,string,long)\",\r\n    \"OnPlayerBanned(string,ulong,string,string)\": \"OnPlayerBanned(Network.Connection,string)\",\r\n    \"IOnPlayerChat(ConsoleSystem.Arg)\": \"IOnPlayerChat(ulong,string,string,ConVar.Chat.ChatChannel,BasePlayer)\",\r\n    \"OnRconConnection(unknown)\": \"OnRconConnection(System.Net.IPAddress)\",\r\n    \"CanMountEntity(BaseMountable,BasePlayer)\": \"CanMountEntity(BasePlayer,BaseMountable)\",\r\n    \"OnItemScrap(ResearchTable,Item)\": \"\",\r\n    \"CanResearchItem(Item,BasePlayer)\": \"CanResearchItem(BasePlayer,Item)\",\r\n    \"OnTurretModeToggle(AutoTurret)\": \"OnTurretModeToggle(AutoTurret,BasePlayer)\",\r\n    \"OnAddVendingOffer(VendingMachine,BasePlayer,ItemDefinition)\": \"OnAddVendingOffer(VendingMachine,ProtoBuf.VendingMachine.SellOrder)\",\r\n    \"OnPlayerActiveItemChanged(BasePlayer,Item,Item)\": \"\",\r\n    \"OnSwitchAmmo(BasePlayer,BaseProjectile)\": \"\",\r\n    \"OnStructureDemolish(BuildingBlock,BasePlayer,bool)\": \"OnStructureDemolish(StabilityEntity,BasePlayer,bool)\",\r\n    \"CanLootEntity(ResourceContainer,BasePlayer)\": \"CanLootEntity(BasePlayer,BaseRidableAnimal)\",\r\n    \"CanDismountEntity(BaseMountable,BasePlayer)\": \"CanDismountEntity(BasePlayer,BaseMountable)\",\r\n    \"IOnNpcPlayerTarget(NPCPlayerApex,BaseEntity)\": \"\",\r\n    \"CanExperiment(BasePlayer,ItemDefinition)\": \"\",\r\n    \"OnServerInitialized()\": \"OnServerInitialized(bool)\",\r\n    \"OnHelicopterAttacked(CH47HelicopterAIController)\": \"\",\r\n    \"OnHelicopterKilled(CH47HelicopterAIController)\": \"\",\r\n    \"IOnDisableServerConsole()\": \"\",\r\n    \"OnRconCommand(?,string,string[])\": \"OnRconCommand(System.Net.IPEndPoint,string,string[])\",\r\n    \"OnPlayerCommand(ConsoleSystem.Arg)\": \"OnPlayerCommand(BasePlayer,string,string[])\",\r\n    \"OnNpcStopMoving(NPCPlayerApex)\": \"\",\r\n    \"OnFlameExplosion(FlameExplosive,BaseEntity)\": \"OnFlameExplosion(FlameExplosive,UnityEngine.Collider)\",\r\n    \"OnNpcPlayerResume(NPCPlayerApex)\": \"\",\r\n    \"IOnPlayerCommand(ConsoleSystem.Arg)\": \"\",\r\n    \"CanExperiment(BasePlayer,Workbench)\": \"\",\r\n    \"OnNpcDestinationSet(NPCPlayerApex,UnityEngine.Vector3)\": \"\",\r\n    \"IOnNpcPlayerSenseVision(NPCPlayerApex)\": \"\",\r\n    \"IOnNpcPlayerSenseClose(NPCPlayerApex)\": \"\",\r\n    \"OnNpcPlayerTarget(NPCPlayerApex,BasePlayer)\": \"\",\r\n    \"OnNpcPlayerTarget(Rust.Ai.HTN.BaseNpcMemory,BaseNpc)\": \"\",\r\n    \"OnNpcPlayerTarget(BaseEntity,BasePlayer)\": \"\",\r\n    \"OnRotateVendingMachine(VendingMachine,unknown)\": \"OnRotateVendingMachine(VendingMachine,BasePlayer)\",\r\n    \"OnExplosiveDropped(BasePlayer,UnityEngine.RaycastHit)\": \"OnExplosiveDropped(BasePlayer,BaseEntity,ThrownWeapon)\",\r\n    \"OnNpcResume(NPCPlayerApex)\": \"\",\r\n    \"IOnNpcTarget(NPCPlayerApex,BaseEntity)\": \"IOnNpcTarget(BaseNpc,BaseEntity)\",\r\n    \"OnInventoryNetworkUpdate(PlayerInventory,ItemContainer,ProtoBuf.UpdateItemContainer,PlayerInventory.Type,bool)\": \"OnInventoryNetworkUpdate(PlayerInventory,ItemContainer,ProtoBuf.UpdateItemContainer,PlayerInventory.Type,PlayerInventory.NetworkInventoryMode)\",\r\n    \"OnNpcTarget(BaseEntity,BasePlayer)\": \"OnNpcTarget(BaseEntity,BaseEntity)\",\r\n    \"CanDeployItem(BasePlayer,Deployer,uint)\": \"CanDeployItem(BasePlayer,Deployer,NetworkableId)\",\r\n    \"OnDieselEngineToggle(BasePlayer,DieselEngine)\": \"OnDieselEngineToggle(DieselEngine,BasePlayer)\",\r\n    \"OnHelicopterStrafeEnter(PatrolHelicopterAI,UnityEngine.Vector3)\": \"OnHelicopterStrafeEnter(PatrolHelicopterAI,UnityEngine.Vector3,BasePlayer)\",\r\n    \"CanNetworkTo(BaseEntity,BasePlayer)\": \"CanNetworkTo(BaseNetworkable,BasePlayer)\",\r\n    \"OnResearchCostDetermine(Item,ResearchTable)\": \"OnResearchCostDetermine(Item)\",\r\n    \"IOnNpcTarget(Rust.Ai.HTN.IHTNAgent,BasePlayer,int)\": \"IOnNpcTarget(BaseNpc,BaseEntity)\",\r\n    \"OnNpcTarget(Rust.Ai.HTN.BaseNpcMemory,BaseNpc)\": \"OnNpcTarget(BaseEntity,BaseEntity)\",\r\n    \"OnSleepingBagDestroyed(SleepingBag,BasePlayer)\": \"OnSleepingBagDestroyed(SleepingBag,ulong)\",\r\n    \"OnNpcTarget(HumanNPC,BasePlayer)\": \"OnNpcTarget(BaseEntity,BaseEntity)\",\r\n    \"OnNpcTarget(NPCPlayerApex,BaseEntity)\": \"OnNpcTarget(BaseEntity,BaseEntity)\",\r\n    \"OnSleepingBagDestroy(SleepingBag,BasePlayer)\": \"OnSleepingBagDestroy(SleepingBag,ulong)\",\r\n    \"OnPlayerChat(BasePlayer,string)\": \"OnPlayerChat(BasePlayer,string,ConVar.Chat.ChatChannel)\",\r\n    \"OnPlayerOfflineChat(ulong,string,string)\": \"OnPlayerOfflineChat(ulong,string,string,ConVar.Chat.ChatChannel)\",\r\n    \"OnNpcTarget(NPCPlayerApex,BasePlayer)\": \"OnNpcTarget(BaseEntity,BaseEntity)\",\r\n    \"OnPlayerWound(BasePlayer,BasePlayer)\": \"OnPlayerWound(BasePlayer,HitInfo)\",\r\n    \"OnEngineStart(MiniCopter,BasePlayer)\": \"OnEngineStart(MotorRowboat,BasePlayer)\",\r\n    \"OnGrowableGather(GrowableEntity,BasePlayer)\": \"OnGrowableGather(GrowableEntity,BasePlayer,bool)\",\r\n    \"OnEntityDistanceCheck(BaseEntity,BasePlayer,uint,string,float)\": \"OnEntityDistanceCheck(BaseEntity,BasePlayer,uint,string,float,bool)\",\r\n    \"IOnUpdateServerDescription()\": \"\",\r\n    \"OnCorpsePopulate(HumanNPC,NPCPlayerCorpse)\": \"OnCorpsePopulate(NPCPlayer,NPCPlayerCorpse)\",\r\n    \"OnEntityControl(PoweredRemoteControlEntity)\": \"OnEntityControl(RemoteControlEntity,ulong)\",\r\n    \"IOnPlayerCommand(BasePlayer,string)\": \"\",\r\n    \"OnFindSpawnPoint()\": \"OnFindSpawnPoint(BasePlayer)\",\r\n    \"OnRidableAnimalClaim(BaseRidableAnimal,BasePlayer)\": \"OnRidableAnimalClaim(BaseRidableAnimal,BasePlayer,Item)\",\r\n    \"OnMapMarkersCleared(BasePlayer,ProtoBuf.MapNote)\": \"OnMapMarkersCleared(BasePlayer)\",\r\n    \"OnCorpsePopulate(NPCMurderer,NPCPlayerCorpse)\": \"OnCorpsePopulate(NPCPlayer,NPCPlayerCorpse)\",\r\n    \"OnActiveItemChange(BasePlayer,Item,uint)\": \"OnActiveItemChange(BasePlayer,Item,ItemId)\",\r\n    \"OnMapMarkerRemove(BasePlayer,ProtoBuf.MapNote)\": \"OnMapMarkerRemove(BasePlayer,System.Collections.Generic.List\\u003CProtoBuf.MapNote\\u003E,int)\",\r\n    \"OnCorpsePopulate(HTNPlayer,NPCPlayerCorpse)\": \"OnCorpsePopulate(NPCPlayer,NPCPlayerCorpse)\",\r\n    \"OnWireConnect(BasePlayer,IOEntity,int,IOEntity,int)\": \"OnWireConnect(BasePlayer,IOEntity,int,IOEntity,int,System.Collections.Generic.List\\u003CUnityEngine.Vector3\\u003E,System.Collections.Generic.List\\u003Cfloat\\u003E)\",\r\n    \"OnReloadMagazine(BasePlayer,BaseProjectile,int)\": \"\",\r\n    \"IOnUpdateServerInformation()\": \"\",\r\n    \"IOnNpcSenseVision(NPCPlayerApex)\": \"\",\r\n    \"OnEngineStarted(MiniCopter,BasePlayer)\": \"OnEngineStarted(MotorRowboat,BasePlayer)\",\r\n    \"OnHelicopterAttacked(CH47HelicopterAIController,HitInfo)\": \"\",\r\n    \"OnMapMarkersClear(BasePlayer,ProtoBuf.MapNote)\": \"OnMapMarkersClear(BasePlayer,System.Collections.Generic.List\\u003CProtoBuf.MapNote\\u003E)\",\r\n    \"OnCorpsePopulate(Scientist,NPCPlayerCorpse)\": \"OnCorpsePopulate(NPCPlayer,NPCPlayerCorpse)\",\r\n    \"OnItemDeployed(Deployer,ItemModDeployable)\": \"OnItemDeployed(Deployer,BaseEntity,BaseEntity)\",\r\n    \"IOnServerUsersSet(ulong,ServerUsers.UserGroup,string,string,long)\": \"\",\r\n    \"OnEntityControl(RemoteControlEntity)\": \"OnEntityControl(RemoteControlEntity,ulong)\",\r\n    \"IOnNpcSenseClose(NPCPlayerApex)\": \"\",\r\n    \"OnSamSiteTarget(SamSite,BaseCombatEntity)\": \"OnSamSiteTarget(SamSite,SamSite.ISamSiteTarget)\",\r\n    \"OnPlayerRecover(BasePlayer,BasePlayer)\": \"OnPlayerRecover(BasePlayer)\",\r\n    \"OnEngineStart(ModularCar,BasePlayer)\": \"OnEngineStart(MotorRowboat,BasePlayer)\",\r\n    \"OnVehicleLockableCheck(ModularCarLock)\": \"OnVehicleLockableCheck(ModularCarCodeLock)\",\r\n    \"OnEngineStarted(ModularCar)\": \"OnEngineStarted(MotorRowboat,BasePlayer)\",\r\n    \"OnEntityControl(AutoTurret)\": \"OnEntityControl(RemoteControlEntity,ulong)\",\r\n    \"OnMagazineReload(BaseProjectile,int,BasePlayer)\": \"OnMagazineReload(BaseProjectile,IAmmoContainer,BasePlayer)\",\r\n    \"OnWireConnect(BasePlayer,BaseNetworkable,int,BaseNetworkable,int,System.Collections.Generic.List\\u003CUnityEngine.Vector3\\u003E)\": \"OnWireConnect(BasePlayer,IOEntity,int,IOEntity,int,System.Collections.Generic.List\\u003CUnityEngine.Vector3\\u003E,System.Collections.Generic.List\\u003Cfloat\\u003E)\",\r\n    \"OnCorpsePopulate(GingerbreadNPC,NPCPlayerCorpse)\": \"OnCorpsePopulate(NPCPlayer,NPCPlayerCorpse)\",\r\n    \"OnCorpsePopulate(ScarecrowNPC,string)\": \"OnCorpsePopulate(NPCPlayer,NPCPlayerCorpse)\",\r\n    \"OnEyePosValidate(AttackEntity,BasePlayer,UnityEngine.Vector3)\": \"\",\r\n    \"OnRespawnInformationGiven(BasePlayer,ProtoBuf.RespawnInformation)\": \"OnRespawnInformationGiven(BasePlayer,System.Collections.Generic.List\\u003CProtoBuf.RespawnInformation.SpawnOptions\\u003E)\",\r\n    \"OnPlayerWantsMount(BaseVehicle,BaseMountable)\": \"OnPlayerWantsMount(BasePlayer,BaseMountable)\",\r\n    \"OnItemStacked(IItemContainerEntity,Item,ItemContainer)\": \"OnItemStacked(Item,Item,ItemContainer)\",\r\n    \"OnCorpsePopulate(FrankensteinPet,NPCPlayerCorpse)\": \"OnCorpsePopulate(NPCPlayer,NPCPlayerCorpse)\",\r\n    \"OnHelicopterAttacked(BaseHelicopter,HitInfo)\": \"\",\r\n    \"CanMoveItem(Item,PlayerInventory,ItemContainerId,int,int)\": \"CanMoveItem(Item,PlayerInventory,ItemContainerId,int,int,ItemMoveModifier)\",\r\n    \"OnCorpsePopulate(ScarecrowNPC,NPCPlayerCorpse)\": \"OnCorpsePopulate(NPCPlayer,NPCPlayerCorpse)\",\r\n    \"OnCorpsePopulate(FrankensteinPet,BaseCorpse)\": \"OnCorpsePopulate(NPCPlayer,NPCPlayerCorpse)\"\r\n  } }";
    }
} 