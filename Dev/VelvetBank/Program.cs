using Sandbox.Game;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.GameSystems;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        #region PASTE THIS INTO YOUR SCRIPT BLOCK (WITHOUT REGION MARKINGS)        
        // This file contains your actual script.
        //
        // You can either keep all your code here, or you can create separate
        // code files to make your program easier to navigate while coding.
        //
        // In order to add a new utility class, right-click on your project, 
        // select 'New' then 'Add Item...'. Now find the 'Space Engineers'
        // category under 'Visual C# Items' on the left hand side, and select
        // 'Utility Class' in the main area. Name it in the box below, and
        // press OK. This utility class will be merged in with your code when
        // deploying your final script.
        //
        // You can also simply create a new utility class manually, you don't
        // have to use the template if you don't want to. Just do so the first
        // time to see what a utility class looks like.
        // 
        // Go to:
        // https://github.com/malware-dev/MDK-SE/wiki/Quick-Introduction-to-Space-Engineers-Ingame-Scripts
        //
        // to learn more about ingame scripts.

        float transactionFee = 0.1f; //The fee paid to the bank during each time you buy credits (from 0 to 1)
        //Keys & Tags
        string BCKey = "MyObjectBuilder_Ingot/Gold";
        string BankCargoGroupKey = "[Bank]";
        string PersonalCargoKey = "[Input]";
        string[] StockPairNames = new string[16];
        string InfoDisplayKey = "[InfoDisplay]";

        string BCSymbol = "G";
        //Lists
        Dictionary<string, string> ItemNames = new Dictionary<string, string>();
        Dictionary<string, int> ItemCounts = new Dictionary<string, int>();
        Dictionary<string, float> ItemValues = new Dictionary<string, float>();
        Dictionary<string, MyItemType> ItemTypes = new Dictionary<string, MyItemType>();
        string SelectedItemType;
        MyItemType SelectedStock = new MyItemType();

        //Blocks
        List<IMyTerminalBlock> BankContainers = new List<IMyTerminalBlock>();
        List<IMyInventory> Inventories = new List<IMyInventory>();
        IMyInventory InputInventory;
        List<MyInventoryItem> InputInventoryItems = new List<MyInventoryItem>();
        Dictionary<IMyTextPanel, IMyButtonPanel> StockDisplays = new Dictionary<IMyTextPanel, IMyButtonPanel>();
        List<IMyTextPanel> TextPanels = new List<IMyTextPanel>();
        List<IMyTextSurface> InfoDisplays = new List<IMyTextSurface>();
        string[] InfoTexts = new string[3];
        float MinimumReserveRatio = .5f;
        int BankCreditCount = 0;
        int MinimumStock = 2;
        float DigitalBalance = 0;

        MyItemType MainCurrency = new MyItemType();



        public Program()
        {
            ItemNames = new Dictionary<string, string>
{
    {"MyObjectBuilder_Component/BulletproofGlass", "Bulletproof Glass" },
    {"MyObjectBuilder_Component/Canvas", "Canvas" },
    {"MyObjectBuilder_Component/Computer", "Computer" },
    {"MyObjectBuilder_Component/Construction", "Construction Components" },
    {"MyObjectBuilder_Component/Detector", "Detector Components" },
    {"MyObjectBuilder_Component/Display", "Display" },
    {"MyObjectBuilder_Component/EngineerPlushie", "Engineer Plushie" },
    {"MyObjectBuilder_Component/Explosives", "Explosives" },
    {"MyObjectBuilder_Component/Girder", "Girder" },
    {"MyObjectBuilder_Component/GravityGenerator", "Gravity Components" },
    {"MyObjectBuilder_Component/InteriorPlate", "Interior Plate" },
    {"MyObjectBuilder_Component/LargeTube", "Large Steel Tube" },
    {"MyObjectBuilder_Component/Medical", "Medical Components" },
    {"MyObjectBuilder_Component/MetalGrid", "Metal Grid" },
    {"MyObjectBuilder_Component/Motor", "Motor" },
    {"MyObjectBuilder_Component/PowerCell", "Power Cell" },
    {"MyObjectBuilder_Component/RadioCommunication", "Radio Comm. Components" },
    {"MyObjectBuilder_Component/Reactor", "Reactor Components" },
    {"MyObjectBuilder_Component/SabiroidPlushie", "Saberoid Plushie" },
    {"MyObjectBuilder_Component/SmallTube", "Small Steel Tube" },
    {"MyObjectBuilder_Component/SolarCell", "Solar Cell" },
    {"MyObjectBuilder_Component/SteelPlate", "Steel Plate" },
    {"MyObjectBuilder_Component/Superconductor", "Superconductor" },
    {"MyObjectBuilder_Component/Thrust", "Thruster Components" },
    {"MyObjectBuilder_Component/ZoneChip", "Zone Chip (ZC)" },
    {"MyObjectBuilder_Ingot/Cobalt", "Cobalt Ingot" },
    {"MyObjectBuilder_Ingot/Gold", "Gold Ingot" },
    {"MyObjectBuilder_Ingot/Stone", "Gravel" },
    {"MyObjectBuilder_Ingot/Iron", "Iron Ingot" },
    {"MyObjectBuilder_Ingot/Magnesium", "Magnesium Powder" },
    {"MyObjectBuilder_Ingot/Nickel", "Nickel Ingot" },
    {"MyObjectBuilder_Ingot/Platinum", "Platinum Ingot" },
    {"MyObjectBuilder_Ingot/Silicon", "Silicon Wafer" },
    {"MyObjectBuilder_Ingot/Silver", "Silver Ingot" },
    {"MyObjectBuilder_Ingot/Uranium", "Uranium Ingot" },
    {"MyObjectBuilder_Ore/Cobalt", "Cobalt Ore" },
    {"MyObjectBuilder_Ore/Gold", "Gold Ore" },
    {"MyObjectBuilder_Ore/Ice", "Ice" },
    {"MyObjectBuilder_Ore/Iron", "Iron Ore" },
    {"MyObjectBuilder_Ore/Magnesium", "Magnesium Ore" },
    {"MyObjectBuilder_Ore/Nickel", "Nickel Ore" },
    {"MyObjectBuilder_Ore/Platinum", "Platinum Ore" },
    {"MyObjectBuilder_Ore/Scrap", "Scrap" },
    {"MyObjectBuilder_Ore/Silicon", "Silicon Ore" },
    {"MyObjectBuilder_Ore/Silver", "Silver Ore" },
    {"MyObjectBuilder_Ore/Stone", "Stone" },
    {"MyObjectBuilder_Ore/Uranium", "Uranium Ore" },
    {"MyObjectBuilder_ConsumableItem/ClangCola", "Clang Cola" },
    {"MyObjectBuilder_ConsumableItem/CosmicCoffee", "Cosmic Coffee" },
    {"MyObjectBuilder_ConsumableItem/Medkit", "Medkit" },
    {"MyObjectBuilder_ConsumableItem/Powerkit", "Powerkit" },
    {"MyObjectBuilder_PhysicalObject/SpaceCredit", "Space Credits (SC)" },
    {"MyObjectBuilder_Ingot/Credits", "Moon Credits (MC)" },
    {"MyObjectBuilder_PhysicalGunObject/AngleGrinder4Item", "Elite Grinder"},
    {"MyObjectBuilder_PhysicalGunObject/HandDrill4Item", "Elite Hand Drill"},
    {"MyObjectBuilder_PhysicalGunObject/Welder4Item", "Elite Welder"},
    {"MyObjectBuilder_PhysicalGunObject/AngleGrinder3Item", "Proficient Grinder"},
    {"MyObjectBuilder_PhysicalGunObject/HandDrill3Item", "Proficient Hand Drill"},
    {"MyObjectBuilder_PhysicalGunObject/Welder3Item", "Proficient Welder"},
    {"MyObjectBuilder_PhysicalGunObject/AngleGrinder2Item", "Enhanced Grinder"},
    {"MyObjectBuilder_PhysicalGunObject/HandDrill2Item", "Enhanced Hand Drill"},
    {"MyObjectBuilder_PhysicalGunObject/Welder2Item", "Enhanced Welder"},
    {"MyObjectBuilder_AmmoMagazine/NATO_25x184mm", "Gatling Ammo Box"},
};
            Echo("Searching for bank blocks...");
            IMyBlockGroup bankBlockGroup = GridTerminalSystem.GetBlockGroupWithName(BankCargoGroupKey);
            bankBlockGroup.GetBlocksOfType(BankContainers);
            BankContainers.ForEach(container => Inventories.Add(container.GetInventory()));
            Echo("Success!");
            Echo("Generating stock pair names...");
            string tag = "[Stocks";
            for (int i = 0; i < StockPairNames.Length; i++)
            {
                string stockIndex = tag + (i + 1) + "]";
                Echo(stockIndex);
                StockPairNames[i] = stockIndex;
            }
            Echo("Searching for display blocks...");
            List<IMyButtonPanel> buttonPanels = new List<IMyButtonPanel>();
            List<IMyTextPanel> textPanels = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType<IMyButtonPanel>(buttonPanels, x => x.CubeGrid == Me.CubeGrid && x.CustomName.Contains(tag));
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(textPanels, x => x.CubeGrid == Me.CubeGrid && x.CustomName.Contains(tag));
            foreach (string stockName in StockPairNames)
            {
                Echo($"Searching for {stockName}...");
                IMyButtonPanel buttonPanel = buttonPanels.Find(x => x.CustomName.Contains($"{stockName}"));
                IMyTextPanel textPanel = textPanels.Find(x => x.CustomName.Contains($"{stockName}"));
                if (buttonPanel != null && textPanel != null)
                {
                    Echo($"Display and Button Panel Found");
                    StockDisplays.Add(textPanel, buttonPanel);
                }
            }
            Echo("Success!");
            Echo("Searching for Input block...");
            IMyTerminalBlock inputBlock = GridTerminalSystem.GetBlockWithName(PersonalCargoKey);
            List<IMyTerminalBlock> inputBlocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(inputBlocks, iB => iB.CubeGrid == Me.CubeGrid && iB.CustomName.Contains(PersonalCargoKey));
            InputInventory = GetBlockWithTag(PersonalCargoKey).GetInventory();
            Echo("Success!");
            Echo("Searching for Info Displays...");
            IMyTextSurfaceProvider infoDisplay = GetBlockWithTag(InfoDisplayKey) as IMyTextSurfaceProvider;
            if (infoDisplay != null)
            {
                Echo("Info Display Block Found. Fetching text surfaces...");
                if (infoDisplay.SurfaceCount > 0)
                {
                    Echo("Number of text surfaces: " + infoDisplay.SurfaceCount);
                    for (int i = 0; i < infoDisplay.SurfaceCount; i++)
                    {
                        IMyTextSurface display = infoDisplay.GetSurface(i);
                        InfoDisplays.Add(display);
                        Echo("Info Display " + (i + 1) + " found.");
                    }
                    Echo("Success!");
                }
            }
            SelectedItemType = BCKey;
        }
        public IMyTerminalBlock GetBlockWithTag(string tag)
        {
            Echo($"Getting block with tag '{tag}'");
            List<IMyTerminalBlock> myTerminalBlocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(myTerminalBlocks, x => x.CubeGrid == Me.CubeGrid && x.CustomName.Contains(tag));
            Echo($"Block with tag '{tag}' found.");
            if (myTerminalBlocks.Count > 0) return myTerminalBlocks.First();
            else return null;
        }
        public void Main(string argument)
        {
            Echo("VelvetBank Launched: argument: " + argument);
            // The main entry point of the script, invoked every time
            // one of the programmable block's Run actions are invoked,
            // or the script updates itself. The updateSource argument
            // describes where the update came from. Be aware that the
            // updateSource is a  bitfield  and might contain more than 
            // one update type.
            // 
            // The method itself is required, but the arguments above
            // can be removed if not needed.
            Echo("Main/ResetCounts()");
            Reset();
            Echo("Main/GetBlockContents(BankCargoGroupKey)");
            GetBlockContents(BankCargoGroupKey);
            Echo("Main/GetBlockContents(PersonalCargoKey)");
            GetBlockContents(PersonalCargoKey);
            Echo("Main/if (InputInventory == null)");
            MinimumReserveRatio = ItemTypes.Count / 2;
            if (InputInventory == null)
            {
                HandleError("NO INPUT INVENTORY");
                return;
            }
            Echo("Main/if (BankCreditCount <= 0)");
            if (BankCreditCount <= 0)
            {
                MarketCrash();
                return;
            }
            Echo("Main/SetItemValues()");
            SetItemValues();
            Echo("Main/if (argument != null)");
            if (argument != null)
            {
                if (argument.StartsWith("sell"))
                {
                    Sell();
                }
                else if (argument.StartsWith("buy_")) //ARGUMENT MUST BE IN FORMAT "buy_Component/SteelPlate", "buy_Ingot/Cobalt", "buy_Ore/Platinum"
                {
                    string productKey = argument.Replace("buy_", "");
                    string productName = "";
                    string productTitle = "";
                    foreach (var item in ItemNames)
                    {
                        string key = item.Key;
                        if (item.Key.EndsWith(productKey))
                        {
                            productName = key;
                            SelectedItemType = key;
                            productTitle = item.Value;
                            if (ItemCounts[productName] < MinimumStock)
                            {
                                HandleError("Item " + productTitle + " out of stock!");
                            }
                            else
                            {
                                Buy(productName);
                            }
                        }
                    }
                }
                CashBack();
            }
            Echo("Main/ResetCounts()");
            Reset();
            Echo("Main/GetBlockContents(BankCargoGroupKey)");
            GetBlockContents(BankCargoGroupKey);
            Echo("Main/GetBlockContents(PersonalCargoKey)");
            GetBlockContents(PersonalCargoKey);
            Echo("Main/if (InputInventory == null)");
            if (InputInventory == null)
            {
                HandleError("NO INPUT INVENTORY");
                return;
            }
            Echo("Main/if (BankCreditCount <= 0)");
            if (BankCreditCount <= 0)
            {
                MarketCrash();
                return;
            }
            Echo("Main/SetItemValues()");
            SetItemValues();
            Echo("Main/UpdateStockDisplays()");
            InitializeStockMenus();/*
    Highest(5);
    Lowest(5);*/
        }
        void InitializeStockMenus()
        {
            Echo("Updating Stock Displays...");
            List<string> stockRows = new List<string>();
            foreach (var item in ItemNames)
            {
                string row = $"{Math.Floor(1 / ItemValues[item.Key])} {ItemNames[item.Key]}\n";
                stockRows.Add(row);
            }
            string[] stockValues = new string[StockDisplays.Count];

            if (stockValues.Length > 0)
            {
                int stocksPerPage = 4;
                for (int i = 0; i < stockValues.Length; i++)
                {
                    stockValues[i] = $"1 {ItemNames[BCKey]} =\n";
                    for (int j = 0; j < stocksPerPage; j++)
                    {
                        int index = i * stocksPerPage + j;
                        if (index >= stockRows.Count) break;
                        stockValues[i] += stockRows[index];
                    }
                }
                int stockMenuId = 0;
                foreach (var stockMenu in StockDisplays)
                {
                    IMyTextPanel stockDisplay = stockMenu.Key;
                    IMyButtonPanel stockButtonPanel = stockMenu.Value;
                    stockDisplay.ClearImagesFromSelection();
                    stockDisplay.WriteText(stockValues[stockMenuId]);
                    stockButtonPanel.AnyoneCanUse = true;


                    stockMenuId++;
                }
            }
            Echo("Stock Displays Updated.");
            UpdateMainDisplay();
        }
        void UpdateMainDisplay()
        {
            for (int i = 0; i < InfoDisplays.Count; i++)
            {
                Echo($"Resetting MenuDisplay {i + 1}");
                InfoTexts[i] = "";

                Echo($"MenuDisplay{i + 1} reset.");


            }
            for (int i = 0; i < InfoDisplays.Count; i++)
            {                
                Echo("Rendering Buy Menu Screen");
                Dictionary<string, int> wallet = new Dictionary<string, int>(InventoryItemCounts(InputInventoryItems));
                string[] rows = new string[wallet.Count];
                int index = 0;
                foreach (var item in wallet)
                {
                    string key = item.Key;
                    rows[index] = $"{item.Value}x {ItemNames[key]} = {(ItemValues[key]) * item.Value}\n";
                    index++;
                }
                InfoTexts[i] += "[1] = sell; [2] = refresh;\n";
                foreach (string row in rows)
                {
                    InfoTexts[i] += row;
                }
                InfoDisplays[i].WriteText(InfoTexts[i]);
            }


        }
        void MarketCrash()
        {
            Echo("BANK DEFAULTING...");
        }
        void HandleError(string message)
        {
            Echo("ERROR: " + message);
        }
        void SetCreditCount()
        {
            Echo("Setting Credit Count...");
            if (BCKey == null)
            {
                BankCreditCount = 0;
                return;
            }
            int count = ItemCounts[BCKey];
            BankCreditCount = count;
            Echo("Credit Count Set: " + BankCreditCount + " " + BCSymbol);
        }
        void SetItemValues()
        {
            Echo("Setting Item values...");
            Dictionary<string, int> inputItemCounts = new Dictionary<string, int>(InventoryItemCounts(InputInventoryItems));
            float cargoWorth = BankCreditCount * MinimumReserveRatio;
            
            foreach(var item in inputItemCounts)
            {
                string key = item.Key;
                ItemCounts[key] += (int)(item.Value * (1+transactionFee));

            }
            foreach (var item in ItemCounts)
            {
                string key = item.Key;
                if (ItemValues.ContainsKey(key))
                {
                    float itemWorth = 1;
                    float itemCount = item.Value;
                    if (item.Key != BCKey)
                    {
                        if (itemCount == 0) itemCount = 1;
                        itemWorth = 1 / itemCount / (ItemNames.Count()) * cargoWorth;
                    }
                    ItemValues[key] = itemWorth;
                }
            }
            Echo("Item values set.");
        }
        Dictionary<string, int> InventoryItemCounts(List<MyInventoryItem> inventoryItems)
        {
            Echo($"Getting item counts in inventory...");
            Dictionary<string, int> inventoryItemCounts = new Dictionary<string, int>();
            int i = 0;
            foreach (MyInventoryItem item in inventoryItems)
            {
                string key = item.Type.ToString();
                Echo($"item {i + 1}: {key}");
                if (ItemNames.ContainsKey(key))
                {
                    if (inventoryItemCounts.ContainsKey(key))
                    {
                        inventoryItemCounts[key] += item.Amount.ToIntSafe();
                    }
                    else
                    {
                        inventoryItemCounts[key] = item.Amount.ToIntSafe();
                    }
                }
                i++;
            }
            return inventoryItemCounts;
        }
        void Sell()
        {
            Echo("Selling items...");
            Dictionary<string, int> inputItemCounts = new Dictionary<string, int>(InventoryItemCounts(InputInventoryItems));
            foreach (var item in InputInventoryItems)
            {
                string key = item.Type.ToString();
                if (key != BCKey)
                {
                    MyFixedPoint amount = item.Amount;
                    MoveToBank(item, amount);
                }
            }
            Echo("Selling items complete. Balance: " + $"{DigitalBalance} {BCSymbol}");
        }
        void MoveToBank(MyInventoryItem item, MyFixedPoint amount)
        {
            Echo($"Moving item {amount} units of {ItemNames[item.Type.ToString()]} to bank");
            foreach (IMyInventory inventory in Inventories)
            {
                if (inventory.CanItemsBeAdded(amount, item.Type) && InputInventory.CanTransferItemTo(inventory, item.Type))
                {
                    MoveItem(inventory, InputInventory, item, amount);
                    DigitalBalance += (float)(ItemValues[item.Type.ToString()] * amount) * (1 - transactionFee);
                }
                else
                {
                    HandleError($"Cannot move item");
                }
            }
        }
        void MoveFromBank(MyItemType itemType, int amount)
        {
            Echo($"Moving {amount} units of {ItemNames[itemType.ToString()]} from bank");
            MyFixedPoint totalTransferAmount = (MyFixedPoint)(amount / (1-transactionFee));
            MyInventoryItem stock = new MyInventoryItem();
            foreach (IMyInventory inventory in Inventories)
            {
                if (totalTransferAmount <= 0 || DigitalBalance <= 0)
                {
                    Echo("Item transfer ended prematurely:");
                    if (totalTransferAmount <= 0) Echo("Cause: Found all items");
                    else Echo("Cause: Insufficient balance");
                }
                stock = (MyInventoryItem)inventory.FindItem(itemType);
                MyFixedPoint stockAmount = stock.Amount;
                MyFixedPoint transferAmount = stockAmount;
                if (stockAmount > totalTransferAmount)
                {
                    transferAmount = totalTransferAmount;
                }
                if (InputInventory.CanItemsBeAdded(amount, itemType) && InputInventory.CanTransferItemTo(inventory, itemType))
                {
                    MoveItem(InputInventory, inventory, stock, transferAmount);
                    DigitalBalance -= (float)(transferAmount * ItemValues[itemType.ToString()]);
                }
                totalTransferAmount -= transferAmount;
            }
        }
        void CashBack()
        {
            MyFixedPoint transferAmount = (MyFixedPoint)DigitalBalance;
            foreach (IMyInventory inventory in Inventories)
            {
                MyInventoryItem money = (MyInventoryItem)inventory.FindItem(MainCurrency);
                if (money == null) continue;
                MyFixedPoint stockAmount = money.Amount;
                if (stockAmount < transferAmount)
                {
                    transferAmount = stockAmount;
                }
                else
                {
                    transferAmount = (MyFixedPoint)DigitalBalance;
                }
                if (InputInventory.CanItemsBeAdded(transferAmount - MinimumStock, MainCurrency) && InputInventory.CanTransferItemTo(inventory, MainCurrency))
                {
                    transferAmount = transferAmount < BankCreditCount - MinimumStock ? transferAmount : BankCreditCount - MinimumStock;
                    MoveItem(InputInventory, inventory, money, transferAmount);
                    DigitalBalance -= (float)transferAmount;
                }
            }
        }
        void MoveItem(IMyInventory targetInventory, IMyInventory sourceInventory, MyInventoryItem item, MyFixedPoint amount)
        {
            if ((!item.Type.ToString().Contains("Ore") && !item.Type.ToString().Contains("Ingot")) || item.Type.ToString().Contains("/Credits"))
            {
                amount = (int)amount;
            }
            Echo($"Moving Item {ItemNames[item.Type.ToString()]}, {amount} units");
            sourceInventory.TransferItemTo(targetInventory, item, amount);
        }
        void Buy(string itemKey)
        {
            Echo($"Buying item {ItemNames[itemKey]}");
            MyInventoryItem BC = (MyInventoryItem)InputInventory.FindItem(MainCurrency);
            MoveToBank(BC, BC.Amount);
            if (DigitalBalance > 0)
            {
                int itemsToBuy = (int)(DigitalBalance / ItemValues[itemKey]);
                Echo($"Amount to buy: {itemsToBuy}");

                if (itemsToBuy < ItemCounts[itemKey])
                {
                    MoveFromBank(ItemTypes[itemKey], itemsToBuy);
                }
                else
                {
                    itemsToBuy = ItemCounts[itemKey] - MinimumStock;
                    MoveFromBank(ItemTypes[itemKey], itemsToBuy);
                }
            }
            Echo($"Purchase Complete.");
        }
        void Reset()
        {
            DigitalBalance = 0;
            SelectedItemType = BCKey;
            Echo("Resetting Item Counts...");
            ItemValues.Clear();
            ItemCounts.Clear();
            InputInventoryItems.Clear();
            foreach (var item in ItemNames)
            {
                ItemCounts.Add(item.Key, 1);
                ItemValues.Add(item.Key, 1);
            }
            Echo("Reset Complete");
        }
        void GetBlockContents(string t)
        {
            Echo($"Targeting Entity {t}");
            string target = t;
            if (target == BankCargoGroupKey)
            {
                Echo("Processing Bank Block Group...");
                foreach (var inventory in Inventories)
                {
                    Dictionary<string, int> scannedItems = ScanInventory(inventory);
                    foreach (var item in scannedItems)
                    {
                        if (ItemCounts.ContainsKey(item.Key)) ItemCounts[item.Key] += item.Value;
                    }
                }
                SetCreditCount();
            }
            else if (target == PersonalCargoKey)
            {
                Echo("Processing Input Block...");
                if (InputInventoryItems.Count == 0)
                {
                    InputInventory.GetItems(InputInventoryItems);
                }
            }
            Echo("Processing Complete.");
        }
        public Dictionary<string, int> ScanInventory(IMyInventory inventory)
        {
            Echo($"Scanning inventory {inventory}");
            Dictionary<string, int> scannedItems = new Dictionary<string, int>();
            List<MyInventoryItem> inventoryItems = new List<MyInventoryItem>();
            inventory.GetItems(inventoryItems);
            foreach (MyInventoryItem item in inventoryItems)
            {
                string key = item.Type.ToString();
                if (key == SelectedItemType && SelectedStock != item.Type)
                {
                    SelectedStock = item.Type;
                }
                int value = item.Amount.ToIntSafe();
                if (key == BCKey)
                {
                    MainCurrency = item.Type;
                }
                if (ItemNames.ContainsKey(key))
                {
                    if (!ItemTypes.ContainsKey(key))
                    {
                        ItemTypes.Add(key, item.Type);
                    }
                    if (scannedItems.ContainsKey(key))
                    {
                        scannedItems[key] += value;
                    }
                    else
                    {
                        scannedItems.Add(key, value);
                    }
                }
            }
            Echo($"Scan complete. Number of unique items found: {scannedItems.Count}");
            return scannedItems;
        }
        #endregion
    }
}
