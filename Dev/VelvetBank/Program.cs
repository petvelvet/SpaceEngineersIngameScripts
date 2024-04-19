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

        float transactionFee = 0.01f; //The fee paid to the bank during each time you buy credits (from 0 to 1)
                                      //Keys & Tags
        string BCKey = "MyObjectBuilder_Component/ZoneChip";
        string BankCargoGroupKey = "[Bank]";
        string PersonalCargoKey = "[Input]";
        string[] StockPairNames = new string[16];
        string InfoDisplayKey = "[InfoDisplay]";

        string BCSymbol = "ZC";
        //Lists
        Dictionary<string, string> ItemNames = new Dictionary<string, string>();
        Dictionary<string, int> ItemCounts = new Dictionary<string, int>();
        Dictionary<string, float> ItemValues = new Dictionary<string, float>();
        Dictionary<string, MyItemType> ItemTypes = new Dictionary<string, MyItemType>();

        //Blocks
        List<IMyTerminalBlock> BankContainers = new List<IMyTerminalBlock>();
        List<IMyInventory> Inventories = new List<IMyInventory>();
        IMyInventory InputInventory;
        List<MyInventoryItem> InputInventoryItems = new List<MyInventoryItem>();
        Dictionary<IMyTextPanel, IMyButtonPanel> StockDisplays = new Dictionary<IMyTextPanel, IMyButtonPanel>();
        List<IMyTextPanel> TextPanels = new List<IMyTextPanel>();
        List<IMyTextSurface> InfoDisplays = new List<IMyTextSurface>();
        string[] InfoTexts = new string[3];
        float MinimumReserveRatio = 2f;
        int BankCreditCount = 0;
        int PersonalCreditCount = 0;
        int MinimumStock = 1;

        MyItemType MainCurrency = new MyItemType();



        public Program()
        {
            ItemNames = new Dictionary<string, string>
{
    {"MyObjectBuilder_Ingot/Credits", "Bank Credits (BC)" },
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
};
            Echo("Searching for bank blocks...");
            IMyBlockGroup bankBlockGroup = GridTerminalSystem.GetBlockGroupWithName(BankCargoGroupKey);
            bankBlockGroup.GetBlocksOfType(BankContainers);
            BankContainers.ForEach(container => Inventories.Add(container.GetInventory()));
            Echo("Success!");
            Echo("Searching for display blocks...");
            for (int i = 0; i < StockPairNames.Length; i++)
            {
                StockPairNames[i] = "[Stock" + (i + 1) + "]";
            }
            foreach (string stockIndex in StockPairNames)
            {
                IMyButtonPanel buttonPanel = GridTerminalSystem.GetBlockWithName(stockIndex) as IMyButtonPanel;
                IMyTextPanel displayPanel = GridTerminalSystem.GetBlockWithName(stockIndex) as IMyTextPanel;
                StockDisplays.Add(displayPanel, buttonPanel);
            }
            Echo("Success!");
            Echo("Searching for Input block...");
            IMyTerminalBlock inputBlock = GridTerminalSystem.GetBlockWithName(PersonalCargoKey);
            InputInventory = inputBlock.GetInventory();
            Echo("Success!");
            Echo("Searching for Info Displays...");
            IMyTextSurfaceProvider infoDisplay = GridTerminalSystem.GetBlockWithName(InfoDisplayKey) as IMyTextSurfaceProvider;
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
            ResetCounts();
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
            Echo("Main/EvaluateInputStocks()");
            EvaluateInputStocks();
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
            }

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
                    stockValues[i] = $"Page {i + 1}/{stockValues.Length}:\n";
                    stockValues[i] += $"1 {BCSymbol} Equals:\n";
                    for (int j = 0; j < stocksPerPage; j++)
                    {
                        int index = i * stocksPerPage + j;
                        if (index >= stockRows.Count) break;
                        stockValues[i] += stockRows[index];
                    }
                }
                for (int i = 0; i < TextPanels.Count; i++)
                {
                    IMyTextPanel displayPanel = TextPanels[i];
                    if (displayPanel != null)
                    {
                        displayPanel.WriteText(stockValues[i]);
                    }
                }
            }
            Echo("Stock Displays Updated.");
            UpdateMainDisplay();
        }
        void UpdateMainDisplay()
        {
            InfoTexts[0] = "";
            Echo("Updating Main Display...");
            InfoTexts[0] = "Input Container Contents:\n";
            MyFixedPoint goodsWorth = 0;
            foreach (MyInventoryItem inputItem in InputInventoryItems)
            {
                string key = inputItem.Type.ToString();
                goodsWorth += inputItem.Amount * ItemValues[key];
                InfoTexts[0] += ItemNames[key] + ": " + inputItem.Amount + "\n";
                InfoTexts[0] += "Total Value: " + goodsWorth + " " + BCSymbol + "\n";
            }
            InfoDisplays[0].WriteText(InfoTexts[0]);
            Echo("Main Display updated.");
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
            Echo("Credit Count Set: " + BankCreditCount  + " " + BCSymbol);
        }
        void SetItemValues()
        {
            Echo("Setting Item values...");
            float cargoWorth = BankCreditCount * MinimumReserveRatio;
            Dictionary<string, int> inputItems = new Dictionary<string, int>();
            List<MyInventoryItem> inputInventoryItems = new List<MyInventoryItem>();
            InputInventory.GetItems(inputInventoryItems);
            foreach (MyInventoryItem item in inputInventoryItems)
            {
                string key = item.Type.ToString();
                int value = item.Amount.ToIntSafe();
                if (inputItems.ContainsKey(key))
                {
                    inputItems[key] += value;
                }
                else
                {
                    inputItems.Add(key, value);
                }
            }
            foreach (var item in ItemCounts)
            {
                string key = item.Key;
                if (ItemValues.ContainsKey(key))
                {
                    float itemWorth = 1;
                    float bonusAmount = 0;
                    if (inputItems.ContainsKey(item.Key))
                    {
                        bonusAmount += inputItems[item.Key];
                    }
                    float itemCount = (item.Value + bonusAmount);
                    if (item.Key != BCKey)
                    {
                        itemWorth = 1 / itemCount / (ItemNames.Count()) * cargoWorth;
                    }
                    ItemValues[key] = itemWorth;
                }
            }
            Echo("Item values set.");
        }
        void Sell()
        {
            Echo("Selling items...");
            Dictionary<string, int> inputItemCounts = ScanInventory(InputInventory);
            Dictionary<string, int> inputItemSellStacks = new Dictionary<string, int>();
            int inputWorth = 0;
            foreach(var inputItem in inputItemCounts)
            {
                string inputItemKey = inputItem.Key;
                if (inputItemKey != BCKey)
                {
                    int sellCount = (int)((float)inputItem.Value - ((float)inputItem.Value % (ItemValues[inputItemKey] / (1 + transactionFee))));
                    int BCPayBack = (int)(sellCount * ItemValues[inputItemKey]);
                    inputWorth += BCPayBack;
                    inputItemSellStacks.Add(inputItemKey, sellCount);
                }
            }
            foreach (var item in InputInventoryItems)
            {
                string key = item.Type.ToString();
                if (inputItemSellStacks.ContainsKey(key))
                {
                    int sellTic = 0;
                    float itemWorth = ItemValues[key];
                    if (item.Amount >= inputItemSellStacks[key])
                    {
                        sellTic = inputItemSellStacks[key];
                    }
                    else
                    {
                        sellTic = (int)item.Amount.ToIntSafe();
                    }
                    MoveToBank(item, sellTic);
                    inputItemSellStacks[key] -= sellTic;
                }
            }
            MoveFromBank(MainCurrency, inputWorth);
            Echo("Selling items complete. Received " + $"{inputWorth} {BCSymbol}");
        }
        void MoveToBank(MyInventoryItem item, int amount)
        {
            Echo($"Moving item {amount} units of {ItemNames[item.Type.ToString()]} to bank");
            foreach(IMyInventory inventory in Inventories)
            {
                if (inventory.CanItemsBeAdded(amount, item.Type) && InputInventory.CanTransferItemTo(inventory,item.Type))
                {
                    MoveItem(inventory, InputInventory, item, amount);
                }
                else
                {
                    HandleError($"Cannot move item");
                }
            }
            Echo($"Moving item {amount} units of {ItemNames[item.Type.ToString()]} to bank");
        }
        int MoveFromBank(MyItemType itemType, int amount)
        {
            Echo($"Moving item {amount} units of {ItemNames[itemType.ToString()]} from bank");
            int transferAmount = amount;
            while (transferAmount > 0)
            {
                MyInventoryItem stock = new MyInventoryItem();
                foreach (IMyInventory inventory in Inventories)
                {
                    stock = (MyInventoryItem)inventory.FindItem(itemType);
                    if (stock == null) continue;
                    int stockAmount = stock.Amount.ToIntSafe();
                    if (stockAmount <= transferAmount)
                    {
                        transferAmount -= stockAmount;
                    }
                    if (InputInventory.CanItemsBeAdded(amount, itemType) && InputInventory.CanTransferItemTo(inventory, itemType))
                    {
                        MoveItem(InputInventory, inventory, stock, transferAmount);
                    }
                }
                if (stock == null) break;
            }
            return transferAmount;
        }
        void MoveItem(IMyInventory targetInventory, IMyInventory sourceInventory, MyInventoryItem item, int amount)
        {
            sourceInventory.TransferItemTo(targetInventory, item, amount);
        }
        void EvaluateInputStocks()
        {
            InputInventory.GetItems(InputInventoryItems);
        }
        void Buy(string itemKey)
        {
            Echo($"Buying item {ItemNames[itemKey]}");
            MyInventoryItem myCash = (MyInventoryItem)InputInventory.FindItem(MainCurrency);
            if (PersonalCreditCount != myCash.Amount.ToIntSafe()) PersonalCreditCount = myCash.Amount.ToIntSafe();
            if (myCash != null && PersonalCreditCount > 0)
            {
                int itemsToBuy = (int)(PersonalCreditCount / ItemValues[itemKey]);

                if (itemsToBuy <= ItemCounts[itemKey])
                {
                    BuyStock(itemKey, itemsToBuy);
                }
            }
            Echo($"Purchase Complete.");
        }
        void BuyStock(string targetItem, int itemsToBuy)
        {
            Echo($"Finding a stock of {ItemNames[targetItem]}");
            List<MyInventoryItem> inventoryItems = new List<MyInventoryItem>();
            MyInventoryItem credits = (MyInventoryItem)InputInventory.FindItem(MainCurrency);
            MoveToBank(credits, PersonalCreditCount);
            int AmountToAfford = (int)Math.Round((float)itemsToBuy / (1 + transactionFee));
            float leftoverCash = 0;
            if (ItemCounts[targetItem] >= MinimumStock)
            {
                foreach(IMyInventory inventory in Inventories)
                {
                    inventory.GetItems(inventoryItems);
                    foreach(MyInventoryItem inventoryItem in inventoryItems)
                    {
                        if (inventoryItem.Type.ToString() == targetItem)
                        {
                            leftoverCash = MoveFromBank(inventoryItem.Type, AmountToAfford) * ItemValues[inventoryItem.Type.ToString()];
                        }
                    }
                }
            }
            else
            {
                Echo("Item " + ItemNames[targetItem] + " out of stock!");
            }
            while (leftoverCash > 1)
            {
                MoveFromBank(MainCurrency, 1);
                leftoverCash--;
            }
            if (leftoverCash > 0)
            {
                string secondaryCurrency = "MyObjectBuilder_PhysicalObject/SpaceCredit";
                MoveFromBank(ItemTypes[secondaryCurrency], (int)(leftoverCash / ItemValues[secondaryCurrency]));
            }
            PersonalCreditCount = 0;
        }
        void ResetCounts()
        {
            Echo("Resetting Item Counts...");
            ItemValues.Clear();
            ItemCounts.Clear();
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
                PersonalCreditCount = 0;
                Echo("Searching for block with name " + target + "...");
                IMyTerminalBlock inputBlock = GridTerminalSystem.GetBlockWithName(target);
                if (inputBlock == null && inputBlock.InventoryCount > 0)
                {
                    Echo("Block " + target + " Found.");
                    InputInventory = inputBlock.GetInventory(0);
                }
                Echo("Populating List InputInventoryItems...");
                InputInventory.GetItems(InputInventoryItems);
                Echo("Populating List Complete. Number of items: " + InputInventoryItems.Count);
                foreach (MyInventoryItem item in InputInventoryItems)
                {
                    Echo("Item: " + ItemNames[item.Type.ToString()] + ", Count: " + item.Amount.ToIntSafe());
                    if (item.Type.ToString() == BCKey)
                    {
                        if (MainCurrency != item.Type)
                        {
                            MainCurrency = item.Type;
                        }
                        PersonalCreditCount += item.Amount.ToIntSafe();
                    }
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
